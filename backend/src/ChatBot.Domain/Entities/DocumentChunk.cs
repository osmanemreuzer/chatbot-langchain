using Pgvector;

namespace ChatBot.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public required string Content { get; set; }
    public Vector? Embedding { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
