using ChatBot.Application.DTOs;

namespace ChatBot.Application.Interfaces;

public interface IConversationService
{
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
    Task<ConversationDetailDto?> GetConversationAsync(Guid userId, Guid conversationId);
    Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationRequest request);
    Task<List<MessageDto>?> AddMessageAsync(Guid userId, Guid conversationId, AddMessageRequest request);
    Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId);
}
