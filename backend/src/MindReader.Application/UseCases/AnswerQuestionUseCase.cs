using System.Text.Json;
using MindReader.Application.DTOs;
using MindReader.Domain.Enums;
using MindReader.Domain.Interfaces;

namespace MindReader.Application.UseCases;

public class AnswerQuestionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IClaudeAIService _claudeService;

    private const int MaxQuestions = 20;

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

    public AnswerQuestionUseCase(IGameSessionRepository repository, IClaudeAIService claudeService)
    {
        _repository = repository;
        _claudeService = claudeService;
    }

    public async Task<AnswerQuestionResponseDto?> ExecuteAsync(AnswerQuestionRequestDto request)
    {
        var session = await _repository.GetByIdAsync(request.SessionId);
        if (session is null) return null;

        if (session.Status != GameStatus.InProgress)
        {
            return new AnswerQuestionResponseDto(
                session.SessionId, string.Empty, false, null,
                session.Questions.Count, true);
        }

        var answerType = ParseAnswer(request.Answer);
        var history = BuildHistory(session, request.CurrentQuestion);
        history.Add(new ConversationMessage("user", FormatAnswer(answerType)));

        var claudeResponse = await _claudeService.AskAsync(BuildSystemPrompt(request.Language), history);
        var parsed = ParseClaudeResponse(claudeResponse);

        var questionNumber = session.Questions.Count + 1;

        // Add the Q&A pair to the session
        session.AddQuestion(parsed.Question, answerType);

        bool isGameOver = false;

        if (parsed.IsGuess)
        {
            session.MarkAsGuessed();
            isGameOver = true;
        }
        else if (questionNumber >= MaxQuestions)
        {
            session.MarkAsGaveUp();
            isGameOver = true;
        }

        await _repository.UpdateAsync(session);

        return new AnswerQuestionResponseDto(
            session.SessionId,
            parsed.Question,
            parsed.IsGuess,
            parsed.GuessSubject,
            questionNumber,
            isGameOver);
    }

    private static List<ConversationMessage> BuildHistory(Domain.Entities.GameSession session, string currentQuestion)
    {
        var history = new List<ConversationMessage>
        {
            new("user", "Let's play! I'm thinking of something. Start asking questions.")
        };

        foreach (var qa in session.Questions.OrderBy(q => q.Order))
        {
            history.Add(new ConversationMessage("assistant", $"{{\"isGuess\": false, \"question\": \"{EscapeJson(qa.Question)}\"}}"));
            history.Add(new ConversationMessage("user", FormatAnswer(qa.Answer)));
        }

        // The question currently on screen that the user is answering
        history.Add(new ConversationMessage("assistant", $"{{\"isGuess\": false, \"question\": \"{EscapeJson(currentQuestion)}\"}}"));

        return history;
    }

    private static string EscapeJson(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string FormatAnswer(AnswerType answer) => answer switch
    {
        AnswerType.Yes => "Yes",
        AnswerType.No => "No",
        AnswerType.IDontKnow => "I don't know",
        AnswerType.Probably => "Probably",
        AnswerType.ProbablyNot => "Probably not",
        _ => "I don't know"
    };

    private static AnswerType ParseAnswer(string answer) => answer.ToLowerInvariant() switch
    {
        "yes" => AnswerType.Yes,
        "no" => AnswerType.No,
        "idontknow" or "i don't know" or "idk" => AnswerType.IDontKnow,
        "probably" => AnswerType.Probably,
        "probablynot" or "probably not" => AnswerType.ProbablyNot,
        _ => AnswerType.IDontKnow
    };

    private static (string Question, bool IsGuess, string? GuessSubject) ParseClaudeResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var isGuess = root.TryGetProperty("isGuess", out var isGuessEl) && isGuessEl.GetBoolean();
        var question = root.GetProperty("question").GetString() ?? "Is this what you're thinking of?";
        string? guessSubject = null;
        if (isGuess && root.TryGetProperty("guess", out var guessEl))
            guessSubject = guessEl.GetString();
        return (question, isGuess, guessSubject);
    }
}
