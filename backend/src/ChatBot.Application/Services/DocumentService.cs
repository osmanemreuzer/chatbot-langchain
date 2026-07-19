using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;
using ChatBot.Domain.Entities;
using Pgvector;

namespace ChatBot.Application.Services;

public class DocumentService : IDocumentService
{
    private static readonly string[] AllowedExtensions = [".pdf", ".txt", ".docx", ".md"];
    private const long MaxSizeBytes = 20 * 1024 * 1024;

    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentProcessingService _documentProcessingService;

    public DocumentService(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IDocumentProcessingService documentProcessingService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _documentProcessingService = documentProcessingService;
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
            throw new InvalidOperationException("Dosya boyutu 20 MB'ı aşamaz.");
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
            var processedChunks = await _documentProcessingService.ProcessAsync(fileName, contentType, savedFileStream);

            var chunks = processedChunks
                .Select((chunk, index) => new DocumentChunk
                {
                    DocumentId = document.Id,
                    ChunkIndex = index,
                    Content = chunk.Content,
                    Embedding = new Vector(chunk.Embedding)
                })
                .ToList();

            if (chunks.Count > 0)
            {
                await _documentRepository.AddChunksAsync(chunks);
            }
        }
        catch (HttpRequestException)
        {
            // AI servisine ulaşılamazsa doküman yine de yüklenmiş sayılır, chunk'lama daha sonra tekrar denenebilir.
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

        await _fileStorageService.DeleteAsync(document.StoragePath);
        await _documentRepository.DeleteAsync(document);
        return true;
    }
}
