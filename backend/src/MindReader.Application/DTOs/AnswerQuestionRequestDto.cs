namespace MindReader.Application.DTOs;

public record AnswerQuestionRequestDto(
    Guid SessionId,
    string Answer,
    string CurrentQuestion,
    string Language
);
