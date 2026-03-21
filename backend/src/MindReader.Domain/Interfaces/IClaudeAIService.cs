namespace MindReader.Domain.Interfaces;

public record ConversationMessage(string Role, string Content);

public interface IClaudeAIService
{
    Task<string> AskAsync(string systemPrompt, IEnumerable<ConversationMessage> conversationHistory);
}
