import { useEffect, useState } from 'react';
import { useTranslation } from '../i18n/LanguageContext';

interface QuestionDisplayProps {
  question: string;
  questionNumber: number;
}

export function QuestionDisplay({ question, questionNumber }: QuestionDisplayProps) {
  const { t } = useTranslation();
  const [visible, setVisible] = useState(false);
  const [displayedQuestion, setDisplayedQuestion] = useState(question);

  useEffect(() => {
    setVisible(false);
    const timeout = setTimeout(() => {
      setDisplayedQuestion(question);
      setVisible(true);
    }, 200);
    return () => clearTimeout(timeout);
  }, [question]);

  return (
    <div className="text-center mb-8">
      <div className="mb-3">
        <span className="font-cinzel text-xs font-semibold text-amber-600 uppercase tracking-widest">
          {t.questionOf(questionNumber, 20)}
        </span>
      </div>
      <div
        className={`
          transition-all duration-500 ease-out
          ${visible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}
        `}
      >
        <p className="font-cinzel text-xl md:text-2xl font-semibold text-amber-100 leading-relaxed px-4">
          {displayedQuestion}
        </p>
      </div>
    </div>
  );
}
