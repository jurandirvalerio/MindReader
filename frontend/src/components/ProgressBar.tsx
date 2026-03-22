import { useTranslation } from '../i18n/LanguageContext';

interface ProgressBarProps {
  current: number;
  max: number;
}

export function ProgressBar({ current, max }: ProgressBarProps) {
  const { t } = useTranslation();
  const percentage = Math.min((current / max) * 100, 100);

  return (
    <div className="w-full">
      <div className="flex justify-between items-center mb-2">
        <span className="font-lato text-xs text-slate-500 uppercase tracking-widest">
          {t.questionProgress}
        </span>
        <span className="font-cinzel text-sm font-semibold text-amber-400">
          {current} / {max}
        </span>
      </div>
      <div className="h-2 bg-navy-800 rounded-full overflow-hidden border border-navy-700">
        <div
          className="h-full bg-gradient-to-r from-amber-600 to-amber-400 rounded-full transition-all duration-700 ease-out"
          style={{ width: `${percentage}%` }}
        />
      </div>
    </div>
  );
}
