using System.Text.Json;
using MindReader.Application.DTOs;
using MindReader.Domain.Entities;
using MindReader.Domain.Interfaces;

namespace MindReader.Application.UseCases;

public class StartGameUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IClaudeAIService _claudeService;

    public StartGameUseCase(IGameSessionRepository repository, IClaudeAIService claudeService)
    {
        _repository = repository;
        _claudeService = claudeService;
    }

    public async Task<StartGameResponseDto> ExecuteAsync(StartGameRequestDto request)
    {
        var sessionId = Guid.NewGuid();
        var session = new GameSession(sessionId);

        var history = new[]
        {
            new ConversationMessage("user", GamePromptBuilder.StartMessage(request.Language))
        };

        var response = await _claudeService.AskAsync(GamePromptBuilder.BuildSystemPrompt(request.Language), history);
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
