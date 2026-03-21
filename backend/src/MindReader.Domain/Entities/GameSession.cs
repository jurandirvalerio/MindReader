using MindReader.Domain.Enums;

namespace MindReader.Domain.Entities;

public class GameSession
{
    public Guid SessionId { get; private set; }
    public List<QuestionAnswer> Questions { get; private set; }
    public GameStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public GameSession(Guid sessionId)
    {
        SessionId = sessionId;
        Questions = new List<QuestionAnswer>();
        Status = GameStatus.InProgress;
        CreatedAt = DateTime.UtcNow;
    }

    internal GameSession(Guid sessionId, GameStatus status, DateTime createdAt, List<QuestionAnswer> questions)
    {
        SessionId = sessionId;
        Status = status;
        CreatedAt = createdAt;
        Questions = questions;
    }

    public void AddQuestion(string question, AnswerType answer)
    {
        var order = Questions.Count + 1;
        Questions.Add(new QuestionAnswer(SessionId, question, answer, order));
    }

    public void MarkAsGuessed() => Status = GameStatus.Guessed;
    public void MarkAsGaveUp() => Status = GameStatus.GaveUp;
}
