using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MindReader.Domain.Interfaces;

namespace MindReader.Infrastructure.Services;

public class ClaudeAIService : IClaudeAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string Model = "claude-sonnet-4-20250514";
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public ClaudeAIService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<string> AskAsync(string systemPrompt, IEnumerable<ConversationMessage> conversationHistory)
    {
        var messages = conversationHistory.Select(m => new { role = m.Role, content = m.Content }).ToList();

        var requestBody = new
        {
            model = Model,
            max_tokens = 256,
            system = systemPrompt,
            messages
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var textContent = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        // Extract JSON from response (Claude might wrap it)
        var trimmed = textContent.Trim();
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
            return trimmed[start..(end + 1)];

        return trimmed;
    }
}
