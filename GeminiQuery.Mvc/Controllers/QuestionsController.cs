using System.Net.Mime;
using System.Text.Json;
using Converter;
using GeminiQuery.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeminiQuery.Mvc.Controllers
{
    public class QuestionsController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content")] Question question)
        {
            JsonToXlsx jsonToXlsx = JsonSerializer.Serialize(new[] { new { name = "teste", id = 1, question.Content } });

            using var ms = jsonToXlsx.MemoryStream;
            return File(ms.ToArray(), MediaTypeNames.Application.Octet, $"gemini-quuery-{DateTime.Now:HHmmss}.xlsx", true);
        }

        /*
         var poicly = Policy
             .Handle<Exception>()
             .WaitAndRetryAsync(3, (x) => TimeSpan.FromSeconds(2));

 using var connection = new NpgsqlConnection(Envs.ConnectionString);
     connection.Open();

 var schema = await GetSchema(connection);

     var prompt = "apresente os 5 primeiros códigos de usuário e o produto comprado";

     var question = @$"como um especialista em banco de dados
 e baseado no schema em json {schema} do banco de dados em postgresql
 gere uma query que apresente o seguinte resultado: {prompt}
 apresente:
 apenas a query
 sem explicação
 sem rodapé
 sem cabeçalho
 texto plano
 apenas a string
 não utilize fortato markdown
 não utilize quebra de linha ou tabulação
 especifique as tabelas com o schema
 utilize exatamente os nomes informados no schema
 utilize alias com duas letras
 ";

     var resultData = await poicly.ExecuteAsync(() => Handler(connection, question));

     JsonToXlsx jsonToXlsx = resultData;

     jsonToXlsx.Workbook.SaveAs($"./{prompt}-{DateTime.Now:yyyyMMddHHmmss}.xlsx");

 static string MakeQueryJson(string query) => @$"
 select jsonb_agg(t)
 from
 (
     {query}
 ) t
 ";

     static async Task<string> RequestGemini(string question)
     {
         var request = new Request(question);

         var result = await Envs.Url
             .AppendQueryParam("key", Envs.Key)
             .PostJsonAsync(request)
             .ReceiveJson<Response>();
         return result.ToString();
     }

     static async Task<string> Handler(NpgsqlConnection connection, string question)
     {
         var resultQuery = await RequestGemini(question);

         Console.WriteLine($"Query: {resultQuery}");
         var finalQuery = MakeQueryJson(resultQuery);

         finalQuery = finalQuery
             .Replace(";", "")
             .Replace("```sql", "")
             .Replace("```", "");

         var resultData = await connection.QueryFirstAsync<string>(finalQuery);
         return resultData;
     }

     static async Task<string> GetSchema(NpgsqlConnection connection)
     {
         var selectSchema = MakeQueryJson(@"
     SELECT
     column_name,
     data_type,
     character_maximum_length,
     is_nullable,
     column_default,
     table_name,
     table_schema
     FROM
     information_schema.columns
     where table_schema ='aula' ");

         var schema = await connection.QueryFirstAsync<string>(selectSchema);
         return schema;
     }
        */
    }
}