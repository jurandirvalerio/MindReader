using FluentAssertions;
using MindReader.Domain.Entities;
using MindReader.Domain.Enums;

namespace MindReader.Tests.Domain;

public class GameSessionTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithInProgressStatus()
    {
        var session = new GameSession(Guid.NewGuid());

        session.Status.Should().Be(GameStatus.InProgress);
        session.Questions.Should().BeEmpty();
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void AddQuestion_ShouldAddWithCorrectOrder()
    {
        var session = new GameSession(Guid.NewGuid());

        session.AddQuestion("Is it a person?", AnswerType.Yes);
        session.AddQuestion("Is it alive?", AnswerType.No);

        session.Questions.Should().HaveCount(2);
        session.Questions[0].Order.Should().Be(1);
        session.Questions[1].Order.Should().Be(2);
    }

    [Fact]
    public void AddQuestion_ShouldPreserveQuestionAndAnswer()
    {
        var session = new GameSession(Guid.NewGuid());

        session.AddQuestion("Is it an animal?", AnswerType.Probably);

        var qa = session.Questions.Single();
        qa.Question.Should().Be("Is it an animal?");
        qa.Answer.Should().Be(AnswerType.Probably);
        qa.SessionId.Should().Be(session.SessionId);
    }

    [Fact]
    public void AddMultipleQuestions_ShouldIncrementOrderSequentially()
    {
        var session = new GameSession(Guid.NewGuid());

        for (var i = 1; i <= 5; i++)
            session.AddQuestion($"Question {i}", AnswerType.Yes);

        session.Questions.Select(q => q.Order).Should().BeEquivalentTo([1, 2, 3, 4, 5]);
    }

    [Fact]
    public void MarkAsGuessed_ShouldSetStatusToGuessed()
    {
        var session = new GameSession(Guid.NewGuid());

        session.MarkAsGuessed();

        session.Status.Should().Be(GameStatus.Guessed);
    }

    [Fact]
    public void MarkAsGaveUp_ShouldSetStatusToGaveUp()
    {
        var session = new GameSession(Guid.NewGuid());

        session.MarkAsGaveUp();

        session.Status.Should().Be(GameStatus.GaveUp);
    }

    [Fact]
    public void NewSession_ShouldHaveUniqueSessionId()
    {
        var session1 = new GameSession(Guid.NewGuid());
        var session2 = new GameSession(Guid.NewGuid());

        session1.SessionId.Should().NotBe(session2.SessionId);
    }
}
