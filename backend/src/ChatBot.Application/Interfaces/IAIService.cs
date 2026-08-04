using ChatBot.Application.DTOs;

namespace ChatBot.Application.Interfaces;

public interface IAIService
{
    Task<AiReply> GetReplyAsync(string message, IReadOnlyList<ChatHistoryMessage> history, string? context = null);
    Task<float[]> EmbedQueryAsync(string text);
}
