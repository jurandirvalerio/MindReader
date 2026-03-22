import { useState } from 'react';
import { useTranslation } from '../i18n/LanguageContext';
import type { AnswerOption } from '../types/game.types';

interface AnswerButtonsProps {
  onAnswer: (answer: AnswerOption) => void;
  isLoading: boolean;
}

const ANSWER_COLORS: Record<AnswerOption, string> = {
  Yes: 'from-emerald-700 to-emerald-600 border-emerald-500 hover:from-emerald-600 hover:to-emerald-500',
  Probably: 'from-teal-800 to-teal-700 border-teal-600 hover:from-teal-700 hover:to-teal-600',
  IDontKnow: 'from-slate-700 to-slate-600 border-slate-500 hover:from-slate-600 hover:to-slate-500',
  ProbablyNot: 'from-orange-900 to-orange-800 border-orange-700 hover:from-orange-800 hover:to-orange-700',
  No: 'from-red-900 to-red-800 border-red-700 hover:from-red-800 hover:to-red-700',
};

const ANSWER_ORDER: AnswerOption[] = ['Yes', 'Probably', 'IDontKnow', 'ProbablyNot', 'No'];

export function AnswerButtons({ onAnswer, isLoading }: AnswerButtonsProps) {
  const { t } = useTranslation();
  const [clicked, setClicked] = useState<AnswerOption | null>(null);

  const labels: Record<AnswerOption, string> = {
    Yes: t.answerYes,
    Probably: t.answerProbably,
    IDontKnow: t.answerIDontKnow,
    ProbablyNot: t.answerProbablyNot,
    No: t.answerNo,
  };

  const handleClick = (value: AnswerOption) => {
    setClicked(value);
    onAnswer(value);
  };

  // Reset clicked state when loading finishes (new question arrived)
  if (!isLoading && clicked !== null) {
    setClicked(null);
  }

  return (
    <div className="grid grid-cols-1 gap-3 w-full max-w-sm mx-auto">
      {ANSWER_ORDER.map((value) => (
        <button
          key={value}
          onClick={() => handleClick(value)}
          disabled={isLoading}
          className={`
            font-lato text-base font-bold tracking-wide uppercase
            px-6 py-3.5 rounded-xl
            bg-gradient-to-r ${ANSWER_COLORS[value]}
            text-white border
            transition-all duration-200
            hover:scale-105 hover:shadow-lg
            disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:scale-100
            focus:outline-none focus:ring-2 focus:ring-amber-400 focus:ring-offset-2 focus:ring-offset-navy-950
          `}
        >
          {isLoading && clicked === value ? (
            <span className="flex items-center justify-center gap-2">
              <span className="inline-block w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin" />
              {t.readingMinds}
            </span>
          ) : (
            labels[value]
          )}
        </button>
      ))}
    </div>
  );
}
