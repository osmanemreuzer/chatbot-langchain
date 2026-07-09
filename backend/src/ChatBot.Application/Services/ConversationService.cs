using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;

namespace ChatBot.Application.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAIService _aiService;

    public ConversationService(IConversationRepository conversationRepository, IAIService aiService)
    {
        _conversationRepository = conversationRepository;
        _aiService = aiService;
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

    private const string WelcomeMessage = "Merhaba, ben AP Asistanı. Size nasıl yardımcı olabilirim?";

    public async Task<ConversationDto> CreateConversationAsync(Guid userId, CreateConversationRequest request)
    {
        var conversation = new Conversation
        {
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "Yeni Sohbet" : request.Title.Trim()
        };

        await _conversationRepository.AddAsync(conversation);

        await _conversationRepository.AddMessageAsync(new Message
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = WelcomeMessage
        });

        return new ConversationDto(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt);
    }

    public async Task<List<MessageDto>?> AddMessageAsync(Guid userId, Guid conversationId, AddMessageRequest request)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation is null || conversation.UserId != userId)
        {
            return null;
        }

        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = request.Role,
            Content = request.Content
        };

        conversation.UpdatedAt = DateTime.UtcNow;
        await _conversationRepository.AddMessageAsync(userMessage);

        var createdMessages = new List<MessageDto>
        {
            new(userMessage.Id, userMessage.Role, userMessage.Content, userMessage.CreatedAt)
        };

        if (request.Role == "user")
        {
            string replyContent;
            try
            {
                replyContent = await _aiService.GetReplyAsync(request.Content);
            }
            catch (HttpRequestException)
            {
                replyContent = "AI servisine şu anda ulaşılamıyor, lütfen daha sonra tekrar deneyin.";
            }

            var assistantMessage = new Message
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = replyContent
            };

            conversation.UpdatedAt = DateTime.UtcNow;
            await _conversationRepository.AddMessageAsync(assistantMessage);

            createdMessages.Add(new MessageDto(
                assistantMessage.Id, assistantMessage.Role, assistantMessage.Content, assistantMessage.CreatedAt));
        }

        return createdMessages;
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
