using ChatBot.Domain.Entities;

namespace ChatBot.Application.Interfaces;

public interface IConversationRepository
{
    Task<List<Conversation>> GetByUserIdAsync(Guid userId);
    Task<Conversation?> GetByIdAsync(Guid id);
    Task<Conversation?> GetByIdWithMessagesAsync(Guid id);
    Task AddAsync(Conversation conversation);
    Task AddMessageAsync(Message message);
    Task DeleteAsync(Conversation conversation);
}
