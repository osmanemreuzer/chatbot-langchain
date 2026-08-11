using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChatBot.Application.Services;

public class DocumentService : IDocumentService
{
    private static readonly string[] AllowedExtensions = [".pdf", ".txt", ".docx", ".md"];
    private const long MaxSizeBytes = 100 * 1024 * 1024;

    private readonly IDocumentRepository _documentRepository; //Dokümanları DB’ye kaydetmek ve sorgulamak için  
    private readonly IFileStorageService _fileStorageService; //Dosyayı sunucuda/bulutta tutmak için (BLOB/Storage Account)
    private readonly IDocumentProcessingService _documentProcessingService; //PDF’i parçalamak (chunking) ve embedding
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IDocumentProcessingService documentProcessingService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _documentProcessingService = documentProcessingService;
        _logger = logger;
    }

    public async Task<List<DocumentDto>> GetDocumentsAsync(Guid userId)
    {
        var documents = await _documentRepository.GetByUserIdAsync(userId);

        return documents
            .Select(d => new DocumentDto(d.Id, d.FileName, d.ContentType, d.SizeBytes, d.CreatedAt))
            .ToList();
    }

    public async Task<DocumentDto> UploadAsync(
        Guid userId, string fileName, string contentType, long sizeBytes, Stream content)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Desteklenmeyen dosya türü. İzin verilenler: {string.Join(", ", AllowedExtensions)}");
        }

        if (sizeBytes > MaxSizeBytes)
        {
            throw new InvalidOperationException("Dosya boyutu 100 MB'ı aşamaz.");
        }

        var document = new Document
        {
            UserId = userId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            StoragePath = string.Empty
        };

        document.StoragePath = await _fileStorageService.SaveAsync(document.Id, fileName, content);

        await _documentRepository.AddAsync(document);

        try
        {
            await using var savedFileStream = File.OpenRead(document.StoragePath);
            await _documentProcessingService.ProcessAsync(
                document.Id, userId, fileName, contentType, savedFileStream);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // AI servisine ulaşılamazsa ya da işlem zaman aşımına uğrarsa doküman yine de
            // yüklenmiş sayılır, chunk'lama daha sonra tekrar denenebilir. Sessizce yutmak yerine
            // logluyoruz ki 0 chunk'lı dokümanların sebebi (ör. Voyage rate limit) görünür olsun.
            _logger.LogError(
                ex,
                "Doküman {DocumentId} ({FileName}) yüklendi ama chunk'lama/embedding başarısız oldu; doküman 0 chunk ile kaydedildi.",
                document.Id,
                fileName);
        }

        return new DocumentDto(document.Id, document.FileName, document.ContentType, document.SizeBytes, document.CreatedAt);
    }

    public async Task<bool> DeleteDocumentAsync(Guid userId, Guid documentId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document is null || document.UserId != userId)
        {
            return false;
        }

        try
        {
            await _documentProcessingService.DeleteAsync(document.Id);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // AI servisine ulaşılamazsa chunk'lar Postgres'te öksüz kalabilir, ama dokümanın
            // kendisi silinebilmeli - upload'taki tolerans deseniyle aynı.
            _logger.LogError(
                ex,
                "Doküman {DocumentId} siliniyor ama AI servisindeki chunk'lar temizlenemedi.",
                document.Id);
        }

        await _fileStorageService.DeleteAsync(document.StoragePath);
        await _documentRepository.DeleteAsync(document);
        return true;
    }
}
