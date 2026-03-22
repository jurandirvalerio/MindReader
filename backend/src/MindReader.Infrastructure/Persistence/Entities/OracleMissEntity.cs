namespace MindReader.Infrastructure.Persistence.Entities;

public class OracleMissEntity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string OracleGuess { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
