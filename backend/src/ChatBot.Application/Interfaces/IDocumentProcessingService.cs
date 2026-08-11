namespace ChatBot.Application.Interfaces;

public interface IDocumentProcessingService
{
    Task<int> ProcessAsync(Guid documentId, Guid userId, string fileName, string contentType, Stream content);
    Task DeleteAsync(Guid documentId);
}
