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

    public async Task<AiReply> GetReplyAsync(Guid userId, string message, IReadOnlyList<ChatHistoryMessage> history)
    {
        var request = new ChatRequest(
            message,
            userId.ToString(),
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

    private record ChatRequest(
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("userId")] string UserId,
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
}
