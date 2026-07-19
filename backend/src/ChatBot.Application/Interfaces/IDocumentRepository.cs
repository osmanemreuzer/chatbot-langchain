using ChatBot.Domain.Entities;

namespace ChatBot.Application.Interfaces;

public interface IDocumentRepository
{
    Task<List<Document>> GetByUserIdAsync(Guid userId);
    Task<Document?> GetByIdAsync(Guid id);
    Task AddAsync(Document document);
    Task AddChunksAsync(List<DocumentChunk> chunks);
    Task<List<DocumentChunk>> GetNearestChunksAsync(Guid userId, float[] queryEmbedding, int limit);
    Task DeleteAsync(Document document);
}
