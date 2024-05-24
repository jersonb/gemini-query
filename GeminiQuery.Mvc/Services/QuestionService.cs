using System.Text.Json;
using Dapper;
using Flurl;
using Flurl.Http;
using GeminiQuery.Mvc.Models;
using Npgsql;
using Polly;
using Polly.Retry;

namespace GeminiQuery.Mvc.Services;

public class QuestionService(
    IConfiguration configuration,
    ILogger<QuestionService> logger)
{
    public async Task<string> ExecuteQuestion(Question question)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        using var connection = new NpgsqlConnection(connectionString);

        connection.Open();

        var schema = await GetSchema(connection);

        var prompt = GetQuestion(schema, question.Content);

        logger.LogInformation("Query: {@Query}", prompt);

        (string Query, string ResultData) = await Policy.ExecuteAsync(() => Handler(connection, prompt));
        question.Query = Query;

        return ResultData;
    }

    private static async Task<string> GetSchema(NpgsqlConnection connection)
    {
        var schemaLines = await connection.QueryAsync<Schema>(QuerySchema);

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

    private async Task<(string Query, string ResultData)> Handler(NpgsqlConnection connection, string question)
    {
        var resultQuery = await RequestGemini(question);

        logger.LogInformation("Query {query}", resultQuery);

        var finalQuery = MakeQueryJson(resultQuery);

        finalQuery = finalQuery
            .Replace(";", "")
            .Replace("```sql", "")
            .Replace("```", "");

        var resultData = await connection.QueryFirstAsync<string>(finalQuery);
        return (finalQuery, resultData);
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

    private readonly AsyncRetryPolicy Policy = Polly.Policy
              .Handle<Exception>()
              .WaitAndRetryAsync(10, (x) => TimeSpan.FromSeconds(1), (ex, y, attempt, w) =>
              {
                  logger.LogError(ex, "Tentativa:{tentativa}", attempt);
              });

    private static string MakeQueryJson(string query)
      => @$" select jsonb_agg(t) from ( {query} ) t ";

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

    private static string QuerySchema => @"select
            concat(table_schema,'.',table_name) table_name,
            column_name,
            data_type,
            is_nullable
            FROM
            information_schema.columns
            where table_schema ='store'
            and not table_name='__EFMigrationsHistory'
            order by table_name";
}