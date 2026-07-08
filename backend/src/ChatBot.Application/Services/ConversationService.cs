using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;

namespace ChatBot.Application.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;

    public ConversationService(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(userId);

        return conversations
            .Select(c => new ConversationDto(c.Id, c.Title, c.CreatedAt, c.UpdatedAt))
            .ToList();
    }

    public async Task<ConversationDetailDto?> GetConversationAsync(Guid userId, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId);
        if (conversation is null || conversation.UserId != userId)
        {
            return null;
        }

        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(m.Id, m.Role, m.Content, m.CreatedAt))
            .ToList();

        return new ConversationDetailDto(
            conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt, messages);
    }

    public async Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationRequest request)
    {
        var conversation = new Conversation
        {
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "Yeni Sohbet" : request.Title.Trim()
        };

        await _conversationRepository.AddAsync(conversation);

        return new ConversationDto(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt);
    }

    public async Task<MessageDto?> AddMessageAsync(Guid userId, Guid conversationId, AddMessageRequest request)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation is null || conversation.UserId != userId)
        {
            return null;
        }

        var message = new Message
        {
            ConversationId = conversation.Id,
            Role = request.Role,
            Content = request.Content
        };

        conversation.UpdatedAt = DateTime.UtcNow;
        await _conversationRepository.AddMessageAsync(message);

        return new MessageDto(message.Id, message.Role, message.Content, message.CreatedAt);
    }

    public async Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation is null || conversation.UserId != userId)
        {
            return false;
        }

        await _conversationRepository.DeleteAsync(conversation);
        return true;
    }
}
