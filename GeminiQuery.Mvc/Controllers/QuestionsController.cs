using System.Net.Mime;
using Converter;
using GeminiQuery.Mvc.Models;
using GeminiQuery.Mvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeminiQuery.Mvc.Controllers
{
    public class QuestionsController(
      QuestionService service)
        : Controller
    {
        public IActionResult Create()
            => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content")] Question question)
        {
            var resultData = await service.ExecuteQuestion(question);

            JsonToXlsx jsonToXlsx = resultData;

            using var ms = jsonToXlsx.MemoryStream;
            return File(ms.ToArray(), MediaTypeNames.Application.Octet, $"gemini-query-{DateTime.Now:HHmmss}.xlsx", true);
        }
    }
}