using MindReader.Domain.Entities;

namespace MindReader.Domain.Interfaces;

public interface IGameSessionRepository
{
    Task AddAsync(GameSession session);
    Task<GameSession?> GetByIdAsync(Guid sessionId);
    Task UpdateAsync(GameSession session);
}
