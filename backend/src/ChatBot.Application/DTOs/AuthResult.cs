namespace ChatBot.Application.DTOs;

public record AuthResult(
    bool Success,
    string? Error = null,
    string? Token = null,
    Guid? UserId = null,
    string? Email = null);
