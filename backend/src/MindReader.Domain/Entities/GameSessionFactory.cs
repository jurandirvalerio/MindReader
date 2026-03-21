using MindReader.Domain.Enums;

namespace MindReader.Domain.Entities;

public static class GameSessionFactory
{
    public static GameSession Create(Guid sessionId, GameStatus status, DateTime createdAt, List<QuestionAnswer> questions)
    {
        return new GameSession(sessionId, status, createdAt, questions);
    }
}
