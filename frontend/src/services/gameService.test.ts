import { describe, it, expect, vi, beforeEach } from 'vitest';
import { startGame, answerQuestion, recordMiss } from './gameService';

describe('gameService', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  describe('startGame', () => {
    it('sends POST to /api/game/start with language', async () => {
      const mockResponse = { sessionId: 'abc', question: 'Is it a person?', questionNumber: 1 };
      vi.spyOn(global, 'fetch').mockResolvedValueOnce(
        new Response(JSON.stringify(mockResponse), { status: 200 }),
      );

      const result = await startGame('en');

      expect(fetch).toHaveBeenCalledWith('/api/game/start', expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ language: 'en' }),
      }));
      expect(result).toEqual(mockResponse);
    });

    it('throws on HTTP error', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValueOnce(
        new Response('Internal error', { status: 500 }),
      );

      await expect(startGame('en')).rejects.toThrow();
    });

    it('throws friendly message on 429', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValueOnce(
        new Response('Rate limit exceeded', { status: 429 }),
      );

      await expect(startGame('en')).rejects.toThrow('Rate limit exceeded');
    });
  });

  describe('answerQuestion', () => {
    it('sends POST to /api/game/answer with all required fields', async () => {
      const mockResponse = {
        sessionId: 'abc',
        question: 'Is it alive?',
        isGuess: false,
        guessSubject: null,
        questionNumber: 2,
        isGameOver: false,
      };
      vi.spyOn(global, 'fetch').mockResolvedValueOnce(
        new Response(JSON.stringify(mockResponse), { status: 200 }),
      );

      const result = await answerQuestion('session-1', 'Yes', 'Is it a person?', 'en');

      expect(fetch).toHaveBeenCalledWith('/api/game/answer', expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({
          sessionId: 'session-1',
          answer: 'Yes',
          currentQuestion: 'Is it a person?',
          language: 'en',
        }),
      }));
      expect(result).toEqual(mockResponse);
    });

    it('throws on HTTP error', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValueOnce(
        new Response('Not Found', { status: 404 }),
      );

      await expect(answerQuestion('bad-id', 'Yes', 'Q?', 'en')).rejects.toThrow();
    });
  });

  describe('recordMiss', () => {
    it('sends POST to /api/game/miss with correct body', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValueOnce(
        new Response(null, { status: 204 }),
      );

      await recordMiss('session-1', 'Dog', 'My dog');

      expect(fetch).toHaveBeenCalledWith('/api/game/miss', expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ sessionId: 'session-1', oracleGuess: 'Dog', correctAnswer: 'My dog' }),
      }));
    });

    it('resolves even on error (fire-and-forget)', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValueOnce(
        new Response('Error', { status: 500 }),
      );

      await expect(recordMiss('session-1', 'Dog', 'Cat')).resolves.toBeUndefined();
    });
  });
});
