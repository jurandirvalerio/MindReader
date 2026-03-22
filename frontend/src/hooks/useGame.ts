import { useCallback, useState } from 'react';
import { answerQuestion, startGame } from '../services/gameService';
import type { AnswerOption, GameData, GameState } from '../types/game.types';

const initialState: GameData = {
  sessionId: null,
  currentQuestion: '',
  questionNumber: 0,
  isGuess: false,
  guessSubject: null,
  isGameOver: false,
  isLoading: false,
  error: null,
  gameState: 'idle',
  playerWon: null,
};

const ANSWER_MAP: Record<AnswerOption, string> = {
  Yes: 'Yes',
  No: 'No',
  IDontKnow: 'IDontKnow',
  Probably: 'Probably',
  ProbablyNot: 'ProbablyNot',
};

export function useGame(language: string) {
  const [state, setState] = useState<GameData>(initialState);

  const setLoading = (isLoading: boolean) =>
    setState((prev) => ({ ...prev, isLoading, error: null }));

  const setError = (error: string) =>
    setState((prev) => ({ ...prev, isLoading: false, error }));

  const handleStartGame = useCallback(async () => {
    setLoading(true);
    try {
      const response = await startGame(language);
      setState({
        ...initialState,
        sessionId: response.sessionId,
        currentQuestion: response.question,
        questionNumber: response.questionNumber,
        gameState: 'playing',
        isLoading: false,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to start game');
    }
  }, [language]);

  const handleSubmitAnswer = useCallback(
    async (answer: AnswerOption) => {
      if (!state.sessionId || state.isLoading) return;

      setLoading(true);
      try {
        const response = await answerQuestion(
          state.sessionId,
          ANSWER_MAP[answer],
          state.currentQuestion,
          language,
        );

        let nextGameState: GameState = 'playing';
        if (response.isGuess) {
          nextGameState = 'guessing';
        } else if (response.isGameOver) {
          nextGameState = 'gameover';
        }

        setState((prev) => ({
          ...prev,
          currentQuestion: response.question,
          questionNumber: response.questionNumber,
          isGuess: response.isGuess,
          guessSubject: response.guessSubject,
          isGameOver: response.isGameOver,
          gameState: nextGameState,
          isLoading: false,
          playerWon: null,
        }));
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to submit answer');
      }
    },
    [state.sessionId, state.isLoading, state.currentQuestion, language],
  );

  const handleGuessResult = useCallback((correct: boolean) => {
    setState((prev) => ({
      ...prev,
      playerWon: correct,
      gameState: 'gameover',
    }));
  }, []);

  const handleRestart = useCallback(() => {
    setState(initialState);
  }, []);

  return {
    ...state,
    startGame: handleStartGame,
    submitAnswer: handleSubmitAnswer,
    confirmGuess: handleGuessResult,
    restart: handleRestart,
  };
}
