import type { AnswerQuestionResponse, StartGameResponse } from '../types/game.types';

const API_BASE = '/api/game';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorText = await response.text().catch(() => 'Unknown error');
    throw new Error(`API error ${response.status}: ${errorText}`);
  }
  return response.json() as Promise<T>;
}

export async function startGame(): Promise<StartGameResponse> {
  const response = await fetch(`${API_BASE}/start`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
  });
  return handleResponse<StartGameResponse>(response);
}

export async function answerQuestion(
  sessionId: string,
  answer: string,
  currentQuestion: string,
): Promise<AnswerQuestionResponse> {
  const response = await fetch(`${API_BASE}/answer`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ sessionId, answer, currentQuestion }),
  });
  return handleResponse<AnswerQuestionResponse>(response);
}
