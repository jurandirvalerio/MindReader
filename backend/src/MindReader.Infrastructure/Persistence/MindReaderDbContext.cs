using Microsoft.EntityFrameworkCore;
using MindReader.Infrastructure.Persistence.Entities;

namespace MindReader.Infrastructure.Persistence;

public class MindReaderDbContext : DbContext
{
    public MindReaderDbContext(DbContextOptions<MindReaderDbContext> options) : base(options) { }

    public DbSet<GameSessionEntity> GameSessions => Set<GameSessionEntity>();
    public DbSet<QuestionAnswerEntity> QuestionAnswers => Set<QuestionAnswerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameSessionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasMany(e => e.Questions)
                  .WithOne(q => q.Session)
                  .HasForeignKey(q => q.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionAnswerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Question).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Answer).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Order).IsRequired();
        });
    }
}
