namespace ChatBot.Application.DTOs;

public record DocumentDto(Guid Id, string FileName, string ContentType, long SizeBytes, DateTime CreatedAt);
// .net--frontend(dışa doğru)