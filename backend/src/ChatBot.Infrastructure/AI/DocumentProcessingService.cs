using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;

namespace ChatBot.Infrastructure.AI;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly HttpClient _httpClient;

    public DocumentProcessingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProcessedChunk>> ProcessAsync(string fileName, string contentType, Stream content)
    {
        using var formData = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        formData.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync("/documents/process", formData);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProcessDocumentResponse>();
        return result?.Chunks.Select(c => new ProcessedChunk(c.Content, c.Embedding)).ToList() ?? [];
    }

    private record ChunkDto(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("content")] string Content,
        [property: JsonPropertyName("embedding")] float[] Embedding);

    private record ProcessDocumentResponse(
        [property: JsonPropertyName("chunkCount")] int ChunkCount,
        [property: JsonPropertyName("chunks")] List<ChunkDto> Chunks);
}
