using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatBot.Application.DTOs;
using ChatBot.Application.Interfaces;

namespace ChatBot.Infrastructure.AI;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;

    public AIService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AiReply> GetReplyAsync(string message, IReadOnlyList<ChatHistoryMessage> history, string? context = null)
    {
        var request = new ChatRequest(
            message,
            context,
            history.Select(h => new ChatHistoryItem(h.Role, h.Content)).ToList());

        var response = await _httpClient.PostAsJsonAsync("/chat", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>();

        SupportTicketToolCall? toolCall = null;
        if (result?.ToolCall is { Name: "create_support_ticket" } tc)
        {
            toolCall = new SupportTicketToolCall(
                tc.Input.GetProperty("title").GetString() ?? string.Empty,
                tc.Input.GetProperty("description").GetString() ?? string.Empty);
        }

        return new AiReply(result?.Reply ?? string.Empty, toolCall);
    }

    public async Task<float[]> EmbedQueryAsync(string text)
    {
        var response = await _httpClient.PostAsJsonAsync("/embeddings/query", new EmbedQueryRequest(text));
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbedQueryResponse>();
        return result?.Embedding ?? [];
    }

    private record ChatRequest(
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("context")] string? Context,
        [property: JsonPropertyName("history")] List<ChatHistoryItem> History);

    private record ChatHistoryItem(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record ChatResponse(
        [property: JsonPropertyName("reply")] string Reply,
        [property: JsonPropertyName("toolCall")] ToolCallDto? ToolCall);

    private record ToolCallDto(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("input")] JsonElement Input);

    private record EmbedQueryRequest([property: JsonPropertyName("text")] string Text);

    private record EmbedQueryResponse([property: JsonPropertyName("embedding")] float[] Embedding);
}
