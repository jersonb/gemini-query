using System.Net.Mime;
using System.Text.Json;
using Converter;
using Dapper;
using Flurl;
using Flurl.Http;
using GeminiQuery.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Polly;

namespace GeminiQuery.Mvc.Controllers
{
    public class QuestionsController(IConfiguration configuration, ILogger<QuestionsController> logger) : Controller
    {
        private readonly IConfiguration configuration = configuration;
        private readonly ILogger<QuestionsController> logger = logger;

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content")] Question question)
        {
            var connectionString = configuration.GetConnectionString("Postgres");
            using var connection = new NpgsqlConnection(connectionString);

            connection.Open();

            var schema = await GetSchema(connection);

            var prompt = GetQuestion(schema, question.Content);
            var poicly = Policy
                       .Handle<Exception>()
                       .WaitAndRetryAsync(10, (x) => TimeSpan.FromSeconds(1), (ex, y, attempt, w) =>
                       {
                           logger.LogError(ex, "Tentativa:{tentativa}", attempt);
                       });

            var resultData = await poicly.ExecuteAsync(() => Handler(connection, prompt));

            logger.LogInformation("Query: {query}", prompt);

            JsonToXlsx jsonToXlsx = resultData;

            using var ms = jsonToXlsx.MemoryStream;
            return File(ms.ToArray(), MediaTypeNames.Application.Octet, $"gemini-quuery-{DateTime.Now:HHmmss}.xlsx", true);
        }

        private static string MakeQueryJson(string query)
            => @$" select jsonb_agg(t) from ( {query} ) t ";

        private static async Task<string> GetSchema(NpgsqlConnection connection)
        {
            var selectSchema = @"select
            concat(table_schema,'.',table_name) table_name,
            column_name,
            data_type,
            is_nullable
            FROM
            information_schema.columns
            where table_schema ='store'
            and not table_name='__EFMigrationsHistory'
            order by table_name";

            var schemaLines = await connection.QueryAsync<Schema>(selectSchema);

            var schema = schemaLines
                .GroupBy(x => x.table_name)
                .Select(x => new
                {
                    table_name = x.Key,
                    columns = x.Select(y => new
                    {
                        y.column_name,
                        y.data_type,
                        y.is_nullable,
                    })
                });
            return JsonSerializer.Serialize(schema);
        }

        private static string GetQuestion(string schema, string prompt)
            => @$"como um especialista em banco de dados postgresql,
            baseado no schema do banco de dados de uma loja no formato json:

            {schema}

            gere uma query que apresente o seguinte resultado: {prompt}

            não explique
            utilize alias com duas letras
            utilize lower case
            especifique o schema
            utilize o formato schema.table
            ";

        private async Task<string> Handler(NpgsqlConnection connection, string question)
        {
            var resultQuery = await RequestGemini(question);

            logger.LogInformation("Query {query}", resultQuery);

            var finalQuery = MakeQueryJson(resultQuery);

            finalQuery = finalQuery
                .Replace(";", "")
                .Replace("```sql", "")
                .Replace("```", "");
            var resultData = await connection.QueryFirstAsync<string>(finalQuery);
            return resultData;
        }

        private async Task<string> RequestGemini(string question)
        {
            var request = new Request(question);

            var result = await configuration["Gemini:Url"]
                .AppendQueryParam("key", configuration["Gemini:Key"])
                .PostJsonAsync(request)
                .ReceiveJson<Response>();
            return result.ToString();
        }
    }
}