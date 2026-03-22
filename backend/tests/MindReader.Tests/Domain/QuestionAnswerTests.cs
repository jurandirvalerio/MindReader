using FluentAssertions;
using MindReader.Domain.Entities;
using MindReader.Domain.Enums;

namespace MindReader.Tests.Domain;

public class QuestionAnswerTests
{
    [Fact]
    public void Constructor_ShouldGenerateNewId()
    {
        var sessionId = Guid.NewGuid();

        var qa = new QuestionAnswer(sessionId, "Is it a person?", AnswerType.Yes, 1);

        qa.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_TwoInstances_ShouldHaveDifferentIds()
    {
        var sessionId = Guid.NewGuid();

        var qa1 = new QuestionAnswer(sessionId, "Question 1", AnswerType.Yes, 1);
        var qa2 = new QuestionAnswer(sessionId, "Question 2", AnswerType.No, 2);

        qa1.Id.Should().NotBe(qa2.Id);
    }

    [Fact]
    public void ReconstructionConstructor_ShouldPreserveExistingId()
    {
        var existingId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var qa = new QuestionAnswer(existingId, sessionId, "Is it alive?", AnswerType.Probably, 3);

        qa.Id.Should().Be(existingId);
        qa.SessionId.Should().Be(sessionId);
        qa.Question.Should().Be("Is it alive?");
        qa.Answer.Should().Be(AnswerType.Probably);
        qa.Order.Should().Be(3);
    }

    [Theory]
    [InlineData(AnswerType.Yes)]
    [InlineData(AnswerType.No)]
    [InlineData(AnswerType.IDontKnow)]
    [InlineData(AnswerType.Probably)]
    [InlineData(AnswerType.ProbablyNot)]
    public void Constructor_ShouldAcceptAllAnswerTypes(AnswerType answerType)
    {
        var qa = new QuestionAnswer(Guid.NewGuid(), "Test question?", answerType, 1);

        qa.Answer.Should().Be(answerType);
    }
}
