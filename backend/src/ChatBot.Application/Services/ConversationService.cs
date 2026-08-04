using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;

namespace ChatBot.Application.Services;

public class ConversationService : IConversationService
{
    private const int RagChunkLimit = 3;

    // Claude'a gönderilen geçmiş, token maliyetini sınırlamak için son N mesajla
    // kesiliyor. Çift olmak zorunda: mesajlar user/assistant sırasıyla dönüşümlü
    // dizildiğinden tek sayı kesim dizinin assistant ile
    // başlamasına yol açar ve Claude API bunu reddeder.
    private const int MaxHistoryMessages = 10;

    private readonly IConversationRepository _conversationRepository;
    private readonly IAIService _aiService;
    private readonly IDocumentRepository _documentRepository;
    private readonly ISupportTicketService _supportTicketService;

    public ConversationService(
        IConversationRepository conversationRepository,
        IAIService aiService,
        IDocumentRepository documentRepository,
        ISupportTicketService supportTicketService)
    {
        _conversationRepository = conversationRepository;
        _aiService = aiService;
        _documentRepository = documentRepository;
        _supportTicketService = supportTicketService;
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

    private const string WelcomeMessage = "Merhaba, ben EE Asistanı. Size nasıl yardımcı olabilirim?";

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
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId);
        if (conversation is null || conversation.UserId != userId)
        {
            return null;
        }

        // Claude'un messages listesi "user" ile başlamalı; ilk mesaj olan
        // karşılama (assistant) mesajını ve öncesini atlıyoruz.
        var history = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .SkipWhile(m => m.Role != "user")
            .TakeLast(MaxHistoryMessages)
            .Select(m => new ChatHistoryMessage(m.Role, m.Content))
            .ToList();

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
            AiReply aiReply;
            try
            {
                var context = await BuildRagContextAsync(userId, request.Content);
                aiReply = await _aiService.GetReplyAsync(request.Content, history, context);
            }
            catch (HttpRequestException)
            {
                aiReply = new AiReply("AI servisine şu anda ulaşılamıyor, lütfen daha sonra tekrar deneyin.", null);
            }

            var replyContent = aiReply.Reply;
            if (aiReply.SupportTicketToolCall is { } ticketCall)
            {
                await _supportTicketService.CreateAsync( //gerçek tool burda çalışıyor.
                    userId, conversation.Id, ticketCall.Title, ticketCall.Description);

                replyContent =
                    $"Destek talebiniz oluşturuldu: \"{ticketCall.Title}\". Ekibimiz en kısa sürede sizinle iletişime geçecek.";
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

    // Soruyu embed edip kullanıcının dokümanlarındaki en yakın chunk'ları context olarak birleştirir.
    // Doküman yoksa null döner (düz sohbet); embedding hatası düz sohbete düşürülür, akışı kırmaz.
    private async Task<string?> BuildRagContextAsync(Guid userId, string question)
    {
        List<DocumentChunk> chunks;
        try
        {
            var queryEmbedding = await _aiService.EmbedQueryAsync(question);//Kullanıcının sorusunu bir embedding vektörüne çeviriyor
            chunks = await _documentRepository.GetNearestChunksAsync(userId, queryEmbedding, RagChunkLimit);//O vektöre anlamca en yakın 3 doküman parçasını veritabanından (muhtemelen vektör benzerliği ile) çekiyor
        }
        catch (HttpRequestException)
        {
            return null;
        }

        return chunks.Count == 0
            ? null
            : string.Join("\n\n---\n\n", chunks.Select(c => c.Content));
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
