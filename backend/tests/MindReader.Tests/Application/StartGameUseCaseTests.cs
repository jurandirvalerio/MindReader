using FluentAssertions;
using Moq;
using MindReader.Application.DTOs;
using MindReader.Application.UseCases;
using MindReader.Domain.Entities;
using MindReader.Domain.Interfaces;

namespace MindReader.Tests.Application;

public class StartGameUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repositoryMock = new();
    private readonly Mock<IClaudeAIService> _claudeMock = new();
    private readonly StartGameUseCase _sut;

    public StartGameUseCaseTests()
    {
        _sut = new StartGameUseCase(_repositoryMock.Object, _claudeMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFirstQuestion()
    {
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .ReturnsAsync("{\"isGuess\": false, \"question\": \"Is it a person?\"}");

        var result = await _sut.ExecuteAsync(new StartGameRequestDto("en"));

        result.Question.Should().Be("Is it a person?");
        result.QuestionNumber.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNewSessionId()
    {
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .ReturnsAsync("{\"isGuess\": false, \"question\": \"Is it alive?\"}");

        var result = await _sut.ExecuteAsync(new StartGameRequestDto("en"));

        result.SessionId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveSessionToRepository()
    {
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .ReturnsAsync("{\"isGuess\": false, \"question\": \"Is it a person?\"}");

        await _sut.ExecuteAsync(new StartGameRequestDto("en"));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<GameSession>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallClaudeWithCorrectLanguage()
    {
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .ReturnsAsync("{\"isGuess\": false, \"question\": \"É uma pessoa?\"}");

        await _sut.ExecuteAsync(new StartGameRequestDto("pt"));

        _claudeMock.Verify(c => c.AskAsync(
            It.Is<string>(p => p.Contains("Portuguese")),
            It.IsAny<IEnumerable<ConversationMessage>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenClaudeResponseHasExtraWhitespace_ShouldParseProperly()
    {
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .ReturnsAsync("  {\"isGuess\": false, \"question\": \"Is it real?\"} ");

        var result = await _sut.ExecuteAsync(new StartGameRequestDto("en"));

        result.Question.Should().Be("Is it real?");
    }

    [Fact]
    public async Task ExecuteAsync_TwoCalls_ShouldReturnDifferentSessionIds()
    {
        _claudeMock
            .Setup(c => c.AskAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ConversationMessage>>()))
            .ReturnsAsync("{\"isGuess\": false, \"question\": \"Is it a person?\"}");

        var result1 = await _sut.ExecuteAsync(new StartGameRequestDto("en"));
        var result2 = await _sut.ExecuteAsync(new StartGameRequestDto("en"));

        result1.SessionId.Should().NotBe(result2.SessionId);
    }
}
