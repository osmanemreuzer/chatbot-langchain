using ChatBot.Application.DTOs;

namespace ChatBot.Application.Interfaces;

public interface IAIService
{
    Task<AiReply> GetReplyAsync(Guid userId, string message, IReadOnlyList<ChatHistoryMessage> history);
}
