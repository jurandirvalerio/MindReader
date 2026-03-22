using FluentAssertions;
using Moq;
using MindReader.Application.DTOs;
using MindReader.Application.UseCases;
using MindReader.Domain.Entities;
using MindReader.Domain.Enums;
using MindReader.Domain.Interfaces;

namespace MindReader.Tests.Application;

public class AnswerQuestionUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repositoryMock = new();
    private readonly Mock<IClaudeAIService> _claudeMock = new();
    private readonly AnswerQuestionUseCase _sut;

    public AnswerQuestionUseCaseTests()
    {
        _sut = new AnswerQuestionUseCase(_repositoryMock.Object, _claudeMock.Object);
    }

    private GameSession CreateSession(int existingQuestions = 0)
    {
        var session = new GameSession(Guid.NewGuid());
        for (var i = 1; i <= existingQuestions; i++)
            session.AddQuestion($"Question {i}?", AnswerType.Yes);
        return session;
    }

    private void SetupRepository(GameSession session)
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(session.SessionId))
            .ReturnsAsync(session);
    }

    private void SetupClaude(string response)
    {
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSessionNotFound_ShouldReturnNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((GameSession?)null);

        var result = await _sut.ExecuteAsync(new AnswerQuestionRequestDto(Guid.NewGuid(), "Yes", "Is it a person?", "en"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSessionFinished_ShouldReturnGameOverWithoutCallingClaude()
    {
        var session = CreateSession();
        session.MarkAsGuessed();
        SetupRepository(session);

        var result = await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, "Yes", "Is it a dog?", "en"));

        result!.IsGameOver.Should().BeTrue();
        _claudeMock.Verify(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_NormalTurn_ShouldReturnNextQuestion()
    {
        var session = CreateSession();
        SetupRepository(session);
        SetupClaude("{\"isGuess\": false, \"question\": \"Is it an animal?\"}");

        var result = await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, "Yes", "Is it a person?", "en"));

        result!.Question.Should().Be("Is it an animal?");
        result.IsGuess.Should().BeFalse();
        result.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenClaudeGuesses_ShouldMarkSessionAsGuessed()
    {
        var session = CreateSession(5);
        SetupRepository(session);
        SetupClaude("{\"isGuess\": true, \"guess\": \"Dog\", \"question\": \"Is it a dog?\"}");

        var result = await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, "Yes", "Is it furry?", "en"));

        result!.IsGuess.Should().BeTrue();
        result.IsGameOver.Should().BeTrue();
        result.GuessSubject.Should().Be("Dog");
        session.Status.Should().Be(GameStatus.Guessed);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveCurrentQuestionToHistoryBeforeCallingClaude()
    {
        var session = CreateSession();
        SetupRepository(session);

        IEnumerable<ConversationMessage>? capturedHistory = null;
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .Callback<string, IEnumerable<ConversationMessage>>((_, history) => capturedHistory = history)
            .ReturnsAsync("{\"isGuess\": false, \"question\": \"Is it alive?\"}");

        await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, "Yes", "Is it a person?", "en"));

        var historyList = capturedHistory!.ToList();
        historyList.Should().Contain(m => m.Content.Contains("Is it a person?"));
        historyList.Should().Contain(m => m.Content == "Yes");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncrementQuestionNumber()
    {
        var session = CreateSession(3);
        SetupRepository(session);
        SetupClaude("{\"isGuess\": false, \"question\": \"Next question?\"}");

        var result = await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, "No", "Question 3?", "en"));

        result!.QuestionNumber.Should().Be(5); // 3 existing + 1 saved now + 1 next
    }

    [Fact]
    public async Task ExecuteAsync_WhenReaching20Questions_ShouldForceBestGuess()
    {
        var session = CreateSession(19);
        SetupRepository(session);

        _claudeMock
            .SetupSequence(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .ReturnsAsync("{\"isGuess\": false, \"question\": \"Still guessing?\"}")    // normal call → no guess
            .ReturnsAsync("{\"isGuess\": true, \"guess\": \"Cat\", \"question\": \"Is it a cat?\"}"); // forced call

        var result = await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, "No", "Question 19?", "en"));

        result!.IsGuess.Should().BeTrue();
        result.GuessSubject.Should().Be("Cat");
        result.IsGameOver.Should().BeTrue();
        _claudeMock.Verify(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_At20Questions_ForcedGuessUsesSpecialPrompt()
    {
        var session = CreateSession(19);
        SetupRepository(session);

        var capturedPrompts = new List<string>();
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .Callback<string, IEnumerable<ConversationMessage>>((prompt, _) => capturedPrompts.Add(prompt))
            .ReturnsAsync("{\"isGuess\": true, \"guess\": \"Cat\", \"question\": \"Is it a cat?\"}");

        await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, "No", "Question 19?", "en"));

        // Second call (forced) should use a different, stronger prompt
        capturedPrompts.Should().HaveCountGreaterThanOrEqualTo(1);
        capturedPrompts.Last().Should().Contain("MUST");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateRepositoryAfterProcessing()
    {
        var session = CreateSession();
        SetupRepository(session);
        SetupClaude("{\"isGuess\": false, \"question\": \"Is it alive?\"}");

        await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, "Yes", "Is it a person?", "en"));

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<GameSession>()), Times.Once);
    }

    [Theory]
    [InlineData("yes", AnswerType.Yes)]
    [InlineData("no", AnswerType.No)]
    [InlineData("probably", AnswerType.Probably)]
    [InlineData("probablynot", AnswerType.ProbablyNot)]
    [InlineData("idontknow", AnswerType.IDontKnow)]
    [InlineData("idk", AnswerType.IDontKnow)]
    [InlineData("unknown_value", AnswerType.IDontKnow)]
    public async Task ExecuteAsync_ShouldParseAllAnswerTypes(string rawAnswer, AnswerType expectedType)
    {
        var session = CreateSession();
        SetupRepository(session);
        SetupClaude("{\"isGuess\": false, \"question\": \"Next?\"}");

        await _sut.ExecuteAsync(new AnswerQuestionRequestDto(session.SessionId, rawAnswer, "Question?", "en"));

        session.Questions.Last().Answer.Should().Be(expectedType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuestionContainsQuotes_ShouldEscapeJsonProperly()
    {
        var session = CreateSession();
        SetupRepository(session);

        IEnumerable<ConversationMessage>? capturedHistory = null;
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .Callback<string, IEnumerable<ConversationMessage>>((_, h) => capturedHistory = h)
            .ReturnsAsync("{\"isGuess\": false, \"question\": \"Next?\"}");

        await _sut.ExecuteAsync(new AnswerQuestionRequestDto(
            session.SessionId, "Yes", "Is it \"special\"?", "en"));

        var json = capturedHistory!.First(m => m.Role == "assistant").Content;
        var act = () => System.Text.Json.JsonDocument.Parse(json);
        act.Should().NotThrow();
    }
}
