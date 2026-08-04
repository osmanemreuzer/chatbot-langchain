namespace ChatBot.Domain.Entities;

public class SupportTicket
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ConversationId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
