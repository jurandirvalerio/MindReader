export interface StartGameResponse {
  sessionId: string;
  question: string;
  questionNumber: number;
}

export interface AnswerQuestionRequest {
  sessionId: string;
  answer: string;
}

export interface AnswerQuestionResponse {
  sessionId: string;
  question: string;
  isGuess: boolean;
  guessSubject: string | null;
  questionNumber: number;
  isGameOver: boolean;
}

export type AnswerOption = 'Yes' | 'No' | 'IDontKnow' | 'Probably' | 'ProbablyNot';

export type GameState = 'idle' | 'playing' | 'guessing' | 'gameover';

export interface GameData {
  sessionId: string | null;
  currentQuestion: string;
  questionNumber: number;
  isGuess: boolean;
  guessSubject: string | null;
  isGameOver: boolean;
  isLoading: boolean;
  error: string | null;
  gameState: GameState;
  playerWon: boolean | null;
}
