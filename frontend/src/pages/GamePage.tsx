import { AnswerButtons } from '../components/AnswerButtons';
import { GameCard } from '../components/GameCard';
import { GuessReveal } from '../components/GuessReveal';
import { ProgressBar } from '../components/ProgressBar';
import { QuestionDisplay } from '../components/QuestionDisplay';
import { useTranslation } from '../i18n/LanguageContext';
import type { AnswerOption } from '../types/game.types';

interface GamePageProps {
  sessionId: string;
  currentQuestion: string;
  questionNumber: number;
  isGuess: boolean;
  guessSubject: string | null;
  isGameOver: boolean;
  isLoading: boolean;
  error: string | null;
  playerWon: boolean | null;
  gameState: 'playing' | 'guessing' | 'gameover';
  onAnswer: (answer: AnswerOption) => void;
  onConfirmGuess: (correct: boolean) => void;
  onRestart: () => void;
}

export function GamePage({
  sessionId,
  currentQuestion,
  questionNumber,
  isGuess,
  guessSubject,
  isGameOver,
  isLoading,
  error,
  playerWon,
  gameState,
  onAnswer,
  onConfirmGuess,
  onRestart,
}: GamePageProps) {
  const { t } = useTranslation();

  return (
    <div className="min-h-screen flex flex-col items-center justify-center px-4 py-8">
      {/* Header */}
      <div className="mb-6 text-center">
        <h1 className="font-cinzel text-2xl md:text-3xl font-black text-amber-400 text-shadow-gold">
          MindReader
        </h1>
        <p className="font-lato text-xs text-slate-500 uppercase tracking-widest mt-1">
          {t.oracleKnowsAll}
        </p>
      </div>

      <GameCard>
        {gameState === 'guessing' && guessSubject ? (
          <GuessReveal
            sessionId={sessionId}
            guess={guessSubject}
            onCorrect={() => onConfirmGuess(true)}
            onWrong={() => onConfirmGuess(false)}
            isLoading={isLoading}
          />
        ) : isGameOver ? (
          <div className="flex flex-col items-center text-center py-4">
            {playerWon === true ? (
              <>
                <div className="text-6xl mb-4">🔮</div>
                <h2 className="font-cinzel text-2xl font-bold text-amber-400 mb-2 text-shadow-gold">
                  {t.oraclePrevails}
                </h2>
                <p className="font-lato text-slate-300 mb-8">{t.mindRead}</p>
              </>
            ) : (
              <>
                <div className="text-6xl mb-4">🌑</div>
                <h2 className="font-cinzel text-2xl font-bold text-slate-300 mb-2">
                  {t.oracleStumped}
                </h2>
                <p className="font-lato text-slate-400 mb-8">
                  {isGuess ? t.tooMysterious : t.admitsDefeat}
                </p>
              </>
            )}
            <button
              onClick={onRestart}
              className="
                font-cinzel text-base font-bold tracking-wider uppercase
                px-8 py-3.5 rounded-full
                bg-gradient-to-r from-amber-500 to-amber-600
                text-navy-950 border-2 border-amber-400
                hover:from-amber-400 hover:to-amber-500 hover:scale-105
                transition-all duration-300
                focus:outline-none focus:ring-2 focus:ring-amber-400
              "
            >
              {t.playAgain}
            </button>
          </div>
        ) : (
          <>
            <div className="mb-6">
              <ProgressBar current={questionNumber} max={20} />
            </div>

            <QuestionDisplay
              question={currentQuestion}
              questionNumber={questionNumber}
            />

            {error && (
              <div className="mb-4 px-3 py-2 rounded-lg bg-red-900/40 border border-red-700/50 text-red-300 font-lato text-sm text-center">
                {error}
              </div>
            )}

            <AnswerButtons onAnswer={onAnswer} isLoading={isLoading} />
          </>
        )}
      </GameCard>

      {gameState === 'playing' && (
        <p className="mt-6 font-lato text-xs text-slate-600 text-center max-w-xs">
          {t.hint}
        </p>
      )}
    </div>
  );
}
