namespace GeminiQuery.Mvc.Models
{
    public record Content(List<Part> parts)
    {
        public string role { get; set; } = "user";
    }

    public record Part(string text);

    public class Request(string question)
    {
        public List<Content> contents { get; } = [new([new(question)])];
    };

    public record Candidate(
        Content content,
        string finishReason,
        int index,
        List<SafetyRating> safetyRatings);

    public record PromptFeedback(List<SafetyRating> safetyRatings);

    public record Response(List<Candidate> candidates, PromptFeedback promptFeedback)
    {
        public override string ToString()
        => candidates[0].content.parts[0].text;
    };

    public record SafetyRating(string category, string probability);

    public static class Envs
    {
        public const string Url = "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent";
        public const string Key = "";
        public const string ConnectionString = "";
    }
}
