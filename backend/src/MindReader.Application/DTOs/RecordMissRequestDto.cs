namespace MindReader.Application.DTOs;

public record RecordMissRequestDto(Guid SessionId, string OracleGuess, string CorrectAnswer);
