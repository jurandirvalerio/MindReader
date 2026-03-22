using Microsoft.AspNetCore.Mvc;
using MindReader.Application.DTOs;
using MindReader.Application.UseCases;
using MindReader.Domain.Exceptions;

namespace MindReader.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly StartGameUseCase _startGameUseCase;
    private readonly AnswerQuestionUseCase _answerQuestionUseCase;
    private readonly RecordMissUseCase _recordMissUseCase;

    public GameController(
        StartGameUseCase startGameUseCase,
        AnswerQuestionUseCase answerQuestionUseCase,
        RecordMissUseCase recordMissUseCase)
    {
        _startGameUseCase = startGameUseCase;
        _answerQuestionUseCase = answerQuestionUseCase;
        _recordMissUseCase = recordMissUseCase;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartGameResponseDto>> StartGame([FromBody] StartGameRequestDto request)
    {
        try
        {
            var result = await _startGameUseCase.ExecuteAsync(request);
            return Ok(result);
        }
        catch (RateLimitException ex)
        {
            return StatusCode(429, ex.Message);
        }
    }

    [HttpPost("miss")]
    public async Task<IActionResult> RecordMiss([FromBody] RecordMissRequestDto request)
    {
        if (request.SessionId == Guid.Empty || string.IsNullOrWhiteSpace(request.CorrectAnswer))
            return BadRequest("SessionId and CorrectAnswer are required.");

        await _recordMissUseCase.ExecuteAsync(request);
        return NoContent();
    }

    [HttpPost("answer")]
    public async Task<ActionResult<AnswerQuestionResponseDto>> AnswerQuestion([FromBody] AnswerQuestionRequestDto request)
    {
        if (request.SessionId == Guid.Empty || string.IsNullOrWhiteSpace(request.Answer))
            return BadRequest("SessionId and Answer are required.");

        try
        {
            var result = await _answerQuestionUseCase.ExecuteAsync(request);

            if (result is null)
                return NotFound($"Game session {request.SessionId} not found.");

            return Ok(result);
        }
        catch (RateLimitException ex)
        {
            return StatusCode(429, ex.Message);
        }
    }
}
