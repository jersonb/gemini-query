namespace GeminiQuery.Mvc.Models
{
    public class Question
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty!;

        public string Query { get; set; } = string.Empty!;
    }
}