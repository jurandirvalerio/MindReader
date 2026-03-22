using System.Text;
using System.Text.Json;
using MindReader.Domain.Exceptions;
using MindReader.Domain.Interfaces;

namespace MindReader.Infrastructure.Services;

public class ClaudeAIService : IClaudeAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string Model = "claude-haiku-4-5-20251001";
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    private static readonly int[] RetryDelaysMs = [1000, 3000, 8000]; // 3 attempts

    private static readonly object[] Tools =
    [
        new { type = "web_search_20250305", name = "web_search" }
    ];

    public ClaudeAIService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<string> AskAsync(string systemPrompt, IEnumerable<ConversationMessage> conversationHistory)
    {
        var messages = conversationHistory
            .Select(m => (object)new { role = m.Role, content = m.Content })
            .ToList();

        while (true)
        {
            var requestBody = new
            {
                model = Model,
                max_tokens = 300,
                system = systemPrompt,
                tools = Tools,
                messages
            };

            var responseJson = await CallApiAsync(requestBody);
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var stopReason = root.TryGetProperty("stop_reason", out var sr) ? sr.GetString() : "end_turn";
            var contentArray = root.GetProperty("content");

            if (stopReason == "tool_use")
            {
                var assistantContent = BuildAssistantContent(contentArray);
                messages.Add(new { role = "assistant", content = (object)assistantContent });

                var toolResults = BuildToolResults(contentArray);
                messages.Add(new { role = "user", content = (object)toolResults });
                continue;
            }

            return ExtractTextContent(contentArray);
        }
    }

    private static List<object> BuildAssistantContent(JsonElement contentArray)
    {
        var blocks = new List<object>();
        foreach (var block in contentArray.EnumerateArray())
        {
            var type = block.GetProperty("type").GetString();
            if (type == "text")
                blocks.Add(new { type = "text", text = block.GetProperty("text").GetString() ?? "" });
            else if (type == "tool_use")
                blocks.Add(new
                {
                    type = "tool_use",
                    id = block.GetProperty("id").GetString() ?? "",
                    name = block.GetProperty("name").GetString() ?? "",
                    input = JsonSerializer.Deserialize<object>(block.GetProperty("input").GetRawText())!
                });
        }
        return blocks;
    }

    private static List<object> BuildToolResults(JsonElement contentArray)
    {
        var results = new List<object>();
        foreach (var block in contentArray.EnumerateArray())
        {
            if (block.GetProperty("type").GetString() == "tool_use")
                results.Add(new
                {
                    type = "tool_result",
                    tool_use_id = block.GetProperty("id").GetString() ?? "",
                    content = ""
                });
        }
        return results;
    }

    private async Task<string> CallApiAsync(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody);

        for (var attempt = 0; attempt <= RetryDelaysMs.Length; attempt++)
        {
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                if (attempt == RetryDelaysMs.Length)
                    throw new RateLimitException("The Claude API rate limit was reached. Please wait a moment and try again.");

                var delay = RetryDelaysMs[attempt];

                // Honour Retry-After header if provided
                if (response.Headers.TryGetValues("retry-after", out var values) &&
                    int.TryParse(values.FirstOrDefault(), out var retryAfterSeconds))
                {
                    delay = retryAfterSeconds * 1000;
                }

                await Task.Delay(delay);
                continue;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        throw new RateLimitException("The Claude API rate limit was reached. Please wait a moment and try again.");
    }

    private static string ExtractTextContent(JsonElement contentArray)
    {
        string text = "{}";
        foreach (var block in contentArray.EnumerateArray())
        {
            if (block.TryGetProperty("type", out var t) && t.GetString() == "text")
                text = block.GetProperty("text").GetString() ?? "{}";
        }

        var trimmed = text.Trim();
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
            return trimmed[start..(end + 1)];

        return trimmed;
    }
}
