namespace MindReader.Application.DTOs;

public record StartGameResponseDto(
    Guid SessionId,
    string Question,
    int QuestionNumber
);
