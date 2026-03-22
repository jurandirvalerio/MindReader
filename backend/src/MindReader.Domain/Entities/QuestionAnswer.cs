using MindReader.Domain.Enums;

namespace MindReader.Domain.Entities;

public class QuestionAnswer
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public string Question { get; private set; }
    public AnswerType Answer { get; private set; }
    public int Order { get; private set; }

    private QuestionAnswer()
    {
        Question = string.Empty;
    }

    public QuestionAnswer(Guid sessionId, string question, AnswerType answer, int order)
    {
        Id = Guid.NewGuid();
        SessionId = sessionId;
        Question = question;
        Answer = answer;
        Order = order;
    }

    // Used only by the repository when reconstructing from the database.
    public QuestionAnswer(Guid id, Guid sessionId, string question, AnswerType answer, int order)
    {
        Id = id;
        SessionId = sessionId;
        Question = question;
        Answer = answer;
        Order = order;
    }
}
