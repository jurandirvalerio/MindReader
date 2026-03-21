using Microsoft.EntityFrameworkCore;
using MindReader.Domain.Entities;
using MindReader.Domain.Enums;
using MindReader.Domain.Interfaces;
using MindReader.Infrastructure.Persistence.Entities;

namespace MindReader.Infrastructure.Persistence.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly MindReaderDbContext _context;

    public GameSessionRepository(MindReaderDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(GameSession session)
    {
        var entity = MapToEntity(session);
        await _context.GameSessions.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<GameSession?> GetByIdAsync(Guid sessionId)
    {
        var entity = await _context.GameSessions
            .Include(s => s.Questions.OrderBy(q => q.Order))
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task UpdateAsync(GameSession session)
    {
        var entity = await _context.GameSessions
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Id == session.SessionId);

        if (entity is null) return;

        entity.Status = session.Status.ToString();

        var existingIds = entity.Questions.Select(q => q.Id).ToHashSet();
        foreach (var qa in session.Questions)
        {
            if (!existingIds.Contains(qa.Id))
            {
                entity.Questions.Add(new QuestionAnswerEntity
                {
                    Id = qa.Id,
                    SessionId = qa.SessionId,
                    Question = qa.Question,
                    Answer = qa.Answer.ToString(),
                    Order = qa.Order
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private static GameSessionEntity MapToEntity(GameSession session) =>
        new()
        {
            Id = session.SessionId,
            Status = session.Status.ToString(),
            CreatedAt = session.CreatedAt,
            Questions = session.Questions.Select(q => new QuestionAnswerEntity
            {
                Id = q.Id,
                SessionId = q.SessionId,
                Question = q.Question,
                Answer = q.Answer.ToString(),
                Order = q.Order
            }).ToList()
        };

    private static GameSession MapToDomain(GameSessionEntity entity)
    {
        var session = GameSessionFactory.Create(
            entity.Id,
            Enum.Parse<GameStatus>(entity.Status),
            entity.CreatedAt,
            entity.Questions.OrderBy(q => q.Order).Select(q => new QuestionAnswer(
                q.SessionId,
                q.Question,
                Enum.Parse<AnswerType>(q.Answer),
                q.Order
            )).ToList()
        );
        return session;
    }
}
