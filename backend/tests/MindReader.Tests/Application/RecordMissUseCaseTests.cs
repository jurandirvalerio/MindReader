using FluentAssertions;
using Moq;
using MindReader.Application.DTOs;
using MindReader.Application.UseCases;
using MindReader.Domain.Interfaces;

namespace MindReader.Tests.Application;

public class RecordMissUseCaseTests
{
    private readonly Mock<IOracleMissRepository> _repositoryMock = new();
    private readonly RecordMissUseCase _sut;

    public RecordMissUseCaseTests()
    {
        _sut = new RecordMissUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidCorrection_ShouldSaveToRepository()
    {
        var request = new RecordMissRequestDto(Guid.NewGuid(), "Dog", "My dog");

        await _sut.ExecuteAsync(request);

        _repositoryMock.Verify(r => r.AddAsync(
            request.SessionId,
            "Dog",
            "My dog"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithWhitespace_ShouldTrimBeforeSaving()
    {
        var request = new RecordMissRequestDto(Guid.NewGuid(), "  Dog  ", "  My dog  ");

        await _sut.ExecuteAsync(request);

        _repositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            "Dog",
            "My dog"),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WhenCorrectAnswerIsEmpty_ShouldNotSave(string emptyAnswer)
    {
        var request = new RecordMissRequestDto(Guid.NewGuid(), "Dog", emptyAnswer);

        await _sut.ExecuteAsync(request);

        _repositoryMock.Verify(r => r.AddAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()),
            Times.Never);
    }
}
