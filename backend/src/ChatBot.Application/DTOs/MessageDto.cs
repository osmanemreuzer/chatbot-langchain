namespace ChatBot.Application.DTOs;

public record MessageDto(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt);
