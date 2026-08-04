namespace ChatBot.Application.DTOs;

public record SupportTicketDto(Guid Id, Guid ConversationId, string Title, string Description, string Status, DateTime CreatedAt);
