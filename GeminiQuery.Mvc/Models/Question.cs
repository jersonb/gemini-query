using System.ComponentModel.DataAnnotations;

namespace GeminiQuery.Mvc.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty!;
    }
}