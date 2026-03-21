using System.Text.Json;
using MindReader.Application.DTOs;
using MindReader.Domain.Entities;
using MindReader.Domain.Interfaces;

namespace MindReader.Application.UseCases;

public class StartGameUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IClaudeAIService _claudeService;

    private const string SystemPrompt =
        "You are playing a 20 questions game. The user is thinking of something (a person, animal, fictional character, object, place, food, etc.). " +
        "Ask one yes/no question at a time to figure it out. After each answer, ask the next most strategic question. " +
        "When you are confident (after at least 5 questions), make your guess by responding ONLY with a JSON object like: " +
        "{\"isGuess\": true, \"guess\": \"<what you think it is>\", \"question\": \"Is it <guess>?\"}. " +
        "For all other questions, respond ONLY with a JSON object like: {\"isGuess\": false, \"question\": \"<your question here>\"}. " +
        "Never include any text outside the JSON.";

    public StartGameUseCase(IGameSessionRepository repository, IClaudeAIService claudeService)
    {
        _repository = repository;
        _claudeService = claudeService;
    }

    public async Task<StartGameResponseDto> ExecuteAsync()
    {
        var sessionId = Guid.NewGuid();
        var session = new GameSession(sessionId);

        var history = new[]
        {
            new ConversationMessage("user", "Let's play! I'm thinking of something. Start asking questions.")
        };

        var response = await _claudeService.AskAsync(SystemPrompt, history);
        var parsed = ParseClaudeResponse(response);

        await _repository.AddAsync(session);

        return new StartGameResponseDto(sessionId, parsed.Question, 1);
    }

    private static (string Question, bool IsGuess) ParseClaudeResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var isGuess = root.TryGetProperty("isGuess", out var isGuessEl) && isGuessEl.GetBoolean();
        var question = root.GetProperty("question").GetString() ?? "Are you thinking of something?";
        return (question, isGuess);
    }
}
