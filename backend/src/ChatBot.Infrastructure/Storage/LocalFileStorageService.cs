using ChatBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ChatBot.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["Storage:UploadPath"] ?? "App_Data/documents";
        _rootPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
    }

    public async Task<string> SaveAsync(Guid documentId, string fileName, Stream content)
    {
        Directory.CreateDirectory(_rootPath);

        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{documentId}{extension}";
        var fullPath = Path.Combine(_rootPath, storedFileName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream);

        return fullPath;
    }

    public Task DeleteAsync(string storagePath)
    {
        if (File.Exists(storagePath))
        {
            File.Delete(storagePath);
        }

        return Task.CompletedTask;
    }
}
