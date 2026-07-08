namespace ChatBot.Application.DTOs;

public record ConversationDto(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt);
