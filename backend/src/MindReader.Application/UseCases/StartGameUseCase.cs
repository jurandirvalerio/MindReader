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

        var systemPrompt = BuildSystemPrompt(request.Language);
        var history = new[]
        {
            new ConversationMessage("user", StartMessage(request.Language))
        };

        var response = await _claudeService.AskAsync(systemPrompt, history);
        var parsed = ParseClaudeResponse(response);

        await _repository.AddAsync(session);

        return new StartGameResponseDto(sessionId, parsed.Question, 1);
    }

    private static string BuildSystemPrompt(string language)
    {
        var langInstruction = language == "pt"
            ? "You MUST ask all questions and make all guesses in Brazilian Portuguese (pt-BR)."
            : "You MUST ask all questions and make all guesses in English.";

        return
            $"{langInstruction} " +
            "You are playing a 20 questions game. The user is thinking of something (a person, animal, fictional character, object, place, food, etc.). " +
            "Ask one yes/no question at a time to figure it out. After each answer, ask the next most strategic question. " +
            "When you are confident (after at least 5 questions), make your guess by responding ONLY with a JSON object like: " +
            "{\"isGuess\": true, \"guess\": \"<what you think it is>\", \"question\": \"<your guess question in the correct language>\"}. " +
            "For all other questions, respond ONLY with a JSON object like: {\"isGuess\": false, \"question\": \"<your question here>\"}. " +
            "Never include any text outside the JSON.";
    }

    private static string StartMessage(string language) =>
        language == "pt"
            ? "Vamos jogar! Estou pensando em algo. Comece a fazer perguntas."
            : "Let's play! I'm thinking of something. Start asking questions.";

    private static (string Question, bool IsGuess) ParseClaudeResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var isGuess = root.TryGetProperty("isGuess", out var isGuessEl) && isGuessEl.GetBoolean();
        var question = root.GetProperty("question").GetString() ?? "Are you thinking of something?";
        return (question, isGuess);
    }
}
