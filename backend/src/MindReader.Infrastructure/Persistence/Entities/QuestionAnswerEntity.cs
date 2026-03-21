namespace MindReader.Infrastructure.Persistence.Entities;

public class QuestionAnswerEntity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int Order { get; set; }
    public GameSessionEntity Session { get; set; } = null!;
}
