import { useTranslation } from '../i18n/LanguageContext';

export function LanguageToggle() {
  const { language, setLanguage } = useTranslation();

  return (
    <div className="fixed top-4 right-4 z-50 flex rounded-full overflow-hidden border border-amber-800/50 bg-navy-900/80 backdrop-blur-sm">
      <button
        onClick={() => setLanguage('en')}
        className={`
          px-3 py-1.5 text-xs font-bold font-lato tracking-widest uppercase transition-all duration-200
          ${language === 'en'
            ? 'bg-amber-500 text-navy-950'
            : 'text-slate-400 hover:text-amber-400'}
        `}
      >
        EN
      </button>
      <button
        onClick={() => setLanguage('pt')}
        className={`
          px-3 py-1.5 text-xs font-bold font-lato tracking-widest uppercase transition-all duration-200
          ${language === 'pt'
            ? 'bg-amber-500 text-navy-950'
            : 'text-slate-400 hover:text-amber-400'}
        `}
      >
        PT
      </button>
    </div>
  );
}
