using ChatBot.Application.DTOs;

namespace ChatBot.Application.Interfaces;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetDocumentsAsync(Guid userId);
    Task<DocumentDto> UploadAsync(Guid userId, string fileName, string contentType, long sizeBytes, Stream content);
    Task<bool> DeleteDocumentAsync(Guid userId, Guid documentId);
}
