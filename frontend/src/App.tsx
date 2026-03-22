import { LanguageProvider, useTranslation } from './i18n/LanguageContext';
import { LanguageToggle } from './components/LanguageToggle';
import { GamePage } from './pages/GamePage';
import { HomePage } from './pages/HomePage';
import { useGame } from './hooks/useGame';

function GameRoot() {
  const { language } = useTranslation();
  const {
    sessionId,
    currentQuestion,
    questionNumber,
    isGuess,
    guessSubject,
    isGameOver,
    isLoading,
    error,
    gameState,
    playerWon,
    startGame,
    submitAnswer,
    confirmGuess,
    restart,
  } = useGame(language);

  if (gameState === 'idle') {
    return <HomePage onStart={startGame} isLoading={isLoading} error={error} />;
  }

  return (
    <GamePage
      sessionId={sessionId ?? ''}
      currentQuestion={currentQuestion}
      questionNumber={questionNumber}
      isGuess={isGuess}
      guessSubject={guessSubject}
      isGameOver={isGameOver}
      isLoading={isLoading}
      error={error}
      playerWon={playerWon}
      gameState={gameState as 'playing' | 'guessing' | 'gameover'}
      onAnswer={submitAnswer}
      onConfirmGuess={confirmGuess}
      onRestart={restart}
    />
  );
}

export default function App() {
  return (
    <LanguageProvider>
      <LanguageToggle />
      <GameRoot />
    </LanguageProvider>
  );
}
