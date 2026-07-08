namespace ChatBot.Application.DTOs;

public record ConversationDetailDto(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<MessageDto> Messages);
