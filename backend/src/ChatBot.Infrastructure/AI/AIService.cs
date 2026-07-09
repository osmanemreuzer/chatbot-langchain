using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ChatBot.Application.Interfaces;

namespace ChatBot.Infrastructure.AI;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;

    public AIService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetReplyAsync(string message)
    {
        var response = await _httpClient.PostAsJsonAsync("/chat", new ChatRequest(message));
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>();
        return result?.Reply ?? string.Empty;
    }

    private record ChatRequest([property: JsonPropertyName("message")] string Message);

    private record ChatResponse([property: JsonPropertyName("reply")] string Reply);
}
