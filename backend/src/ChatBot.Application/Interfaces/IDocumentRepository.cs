using ChatBot.Domain.Entities;

namespace ChatBot.Application.Interfaces;

public interface IDocumentRepository
{
    Task<List<Document>> GetByUserIdAsync(Guid userId);
    Task<Document?> GetByIdAsync(Guid id);
    Task AddAsync(Document document);
    Task DeleteAsync(Document document);
}
