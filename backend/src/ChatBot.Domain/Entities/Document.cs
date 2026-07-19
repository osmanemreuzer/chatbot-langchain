namespace ChatBot.Domain.Entities;

public class Document
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public required string StoragePath { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
