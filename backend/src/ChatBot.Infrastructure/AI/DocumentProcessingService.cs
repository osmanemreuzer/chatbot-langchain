using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ChatBot.Application.Interfaces;

namespace ChatBot.Infrastructure.AI;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly HttpClient _httpClient;

    public DocumentProcessingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> ProcessAsync(
        Guid documentId, Guid userId, string fileName, string contentType, Stream content)
    {
        using var formData = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        // AI servisi dosya türünü sadece uzantıdan çıkarıyor, orijinal dosya adına ihtiyacı yok
        // (o zaten Documents tablosunda ayrıca saklanıyor). HttpClient'ın MultipartFormDataContent'i
        // Unicode dosya adlarını Content-Disposition header'ında düzgün encode edemediği için
        // (örn. Türkçe karakterler), burada ASCII-güvenli sabit bir ad kullanıyoruz.
        var safeFileName = "file" + Path.GetExtension(fileName);
        formData.Add(streamContent, "file", safeFileName);
        formData.Add(new StringContent(documentId.ToString()), "document_id");
        formData.Add(new StringContent(userId.ToString()), "user_id");

        var response = await _httpClient.PostAsync("/documents/process", formData);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProcessDocumentResponse>();
        return result?.ChunkCount ?? 0;
    }

    public async Task DeleteAsync(Guid documentId)
    {
        var response = await _httpClient.DeleteAsync($"/documents/{documentId}");
        response.EnsureSuccessStatusCode();
    }

    private record ProcessDocumentResponse(
        [property: JsonPropertyName("chunkCount")] int ChunkCount);
}
