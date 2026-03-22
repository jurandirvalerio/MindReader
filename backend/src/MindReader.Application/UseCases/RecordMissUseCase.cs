using MindReader.Application.DTOs;
using MindReader.Domain.Interfaces;

namespace MindReader.Application.UseCases;

public class RecordMissUseCase
{
    private readonly IOracleMissRepository _repository;

    public RecordMissUseCase(IOracleMissRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(RecordMissRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CorrectAnswer)) return;

        await _repository.AddAsync(
            request.SessionId,
            request.OracleGuess.Trim(),
            request.CorrectAnswer.Trim());
    }
}
