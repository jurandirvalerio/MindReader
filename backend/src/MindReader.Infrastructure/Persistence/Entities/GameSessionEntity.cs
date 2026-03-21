namespace MindReader.Infrastructure.Persistence.Entities;

public class GameSessionEntity
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<QuestionAnswerEntity> Questions { get; set; } = new List<QuestionAnswerEntity>();
}
