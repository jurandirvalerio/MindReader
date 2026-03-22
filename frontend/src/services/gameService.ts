import type { AnswerQuestionResponse, StartGameResponse } from '../types/game.types';

const API_BASE = '/api/game';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorText = await response.text().catch(() => '');
    if (response.status === 429) {
      throw new Error(errorText || 'The Oracle is being consulted too frequently. Please wait a moment and try again.');
    }
    throw new Error(errorText || `Unexpected error (${response.status}). Please try again.`);
  }
  return response.json() as Promise<T>;
}

export async function startGame(language: string): Promise<StartGameResponse> {
  const response = await fetch(`${API_BASE}/start`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ language }),
  });
  return handleResponse<StartGameResponse>(response);
}

export async function answerQuestion(
  sessionId: string,
  answer: string,
  currentQuestion: string,
  language: string,
): Promise<AnswerQuestionResponse> {
  const response = await fetch(`${API_BASE}/answer`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ sessionId, answer, currentQuestion, language }),
  });
  return handleResponse<AnswerQuestionResponse>(response);
}
