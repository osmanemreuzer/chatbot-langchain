namespace ChatBot.Domain.Entities;

public class Message
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public required string Role { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
