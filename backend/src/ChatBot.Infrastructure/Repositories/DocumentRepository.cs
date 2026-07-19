using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ChatBot.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _dbContext;

    public DocumentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Document>> GetByUserIdAsync(Guid userId)
        => _dbContext.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

    public Task<Document?> GetByIdAsync(Guid id)
        => _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id);

    public async Task AddAsync(Document document)
    {
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();
    }

    public Task<List<DocumentChunk>> GetNearestChunksAsync(Guid userId, float[] queryEmbedding, int limit)
    {
        var query = new Vector(queryEmbedding);

        return _dbContext.DocumentChunks
            .Where(c => c.Embedding != null)
            .Join(
                _dbContext.Documents.Where(d => d.UserId == userId),
                chunk => chunk.DocumentId,
                document => document.Id,
                (chunk, _) => chunk)
            .OrderBy(c => c.Embedding!.CosineDistance(query))
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddChunksAsync(List<DocumentChunk> chunks)
    {
        _dbContext.DocumentChunks.AddRange(chunks);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Document document)
    {
        _dbContext.Documents.Remove(document);
        await _dbContext.SaveChangesAsync();
    }
}
