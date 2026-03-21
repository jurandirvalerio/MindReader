namespace MindReader.Application.DTOs;

public record AnswerQuestionResponseDto(
    Guid SessionId,
    string Question,
    bool IsGuess,
    string? GuessSubject,
    int QuestionNumber,
    bool IsGameOver
);
