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

        // Save (currentQuestion, userAnswer) BEFORE calling Claude.
        // This keeps each pair correctly associated and makes history reconstruction trivial.
        session.AddQuestion(request.CurrentQuestion, answerType);

        // Always call Claude — even on the 20th answer — so it can process a "Yes"
        // confirmation to a guess question and declare the correct answer.
        var history = BuildHistory(session, request.Language);
        var claudeResponse = await _claudeService.AskAsync(GamePromptBuilder.BuildSystemPrompt(request.Language), history);
        var parsed = ParseClaudeResponse(claudeResponse);

        var nextQuestionNumber = session.Questions.Count + 1;

        // If the 20th answer was processed and Claude still hasn't guessed, force a best guess.
        if (!parsed.IsGuess && session.Questions.Count >= MaxQuestions)
        {
            var forcedHistory = BuildHistory(session, request.Language);
            var forcedResponse = await _claudeService.AskAsync(
                GamePromptBuilder.BuildForcedGuessPrompt(request.Language), forcedHistory);
            parsed = ParseClaudeResponse(forcedResponse);
        }

        bool isGameOver = false;
        if (parsed.IsGuess)
        {
            session.MarkAsGuessed();
            isGameOver = true;
        }
        else if (session.Questions.Count >= MaxQuestions)
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
            nextQuestionNumber,
            isGameOver);
    }

    // History is built entirely from saved Q&A pairs — no need to pass currentQuestion separately.
    private static List<ConversationMessage> BuildHistory(Domain.Entities.GameSession session, string language)
    {
        var history = new List<ConversationMessage> { new("user", GamePromptBuilder.StartMessage(language)) };

        foreach (var qa in session.Questions.OrderBy(q => q.Order))
        {
            history.Add(new ConversationMessage("assistant", $"{{\"isGuess\": false, \"question\": \"{EscapeJson(qa.Question)}\"}}"));
            history.Add(new ConversationMessage("user", FormatAnswer(qa.Answer)));
        }

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
