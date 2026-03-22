using MindReader.Domain.Interfaces;
using MindReader.Infrastructure.Persistence.Entities;

namespace MindReader.Infrastructure.Persistence.Repositories;

public class OracleMissRepository : IOracleMissRepository
{
    private readonly MindReaderDbContext _context;

    public OracleMissRepository(MindReaderDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Guid sessionId, string oracleGuess, string correctAnswer)
    {
        var entity = new OracleMissEntity
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            OracleGuess = oracleGuess,
            CorrectAnswer = correctAnswer,
            CreatedAt = DateTime.UtcNow,
        };

        await _context.OracleMisses.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}
