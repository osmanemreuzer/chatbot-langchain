namespace ChatBot.Application.Interfaces;

public interface IAIService
{
    Task<string> GetReplyAsync(string message, string? context = null);
    Task<float[]> EmbedQueryAsync(string text);
}
