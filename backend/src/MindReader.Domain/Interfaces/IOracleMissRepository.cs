namespace MindReader.Domain.Interfaces;

public interface IOracleMissRepository
{
    Task AddAsync(Guid sessionId, string oracleGuess, string correctAnswer);
}
