namespace ChatBot.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Guid documentId, string fileName, Stream content);
    Task DeleteAsync(string storagePath);
}
