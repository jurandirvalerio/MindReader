import { useTranslation } from '../i18n/LanguageContext';

interface StartScreenProps {
  onStart: () => void;
  isLoading: boolean;
  error: string | null;
}

export function StartScreen({ onStart, isLoading, error }: StartScreenProps) {
  const { t } = useTranslation();

  return (
    <div className="flex flex-col items-center justify-center min-h-screen px-4 text-center">
      {/* Starfield decoration */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {Array.from({ length: 50 }).map((_, i) => (
          <div
            key={i}
            className="absolute rounded-full bg-white animate-star-twinkle"
            style={{
              width: Math.random() * 3 + 1 + 'px',
              height: Math.random() * 3 + 1 + 'px',
              top: Math.random() * 100 + '%',
              left: Math.random() * 100 + '%',
              animationDelay: Math.random() * 3 + 's',
              animationDuration: Math.random() * 3 + 2 + 's',
            }}
          />
        ))}
      </div>

      {/* Glowing orb */}
      <div className="relative mb-8">
        <div className="w-40 h-40 rounded-full bg-gradient-to-br from-amber-400 via-yellow-500 to-amber-600 animate-orb-float glow-gold-intense opacity-90 flex items-center justify-center">
          <div className="w-28 h-28 rounded-full bg-gradient-to-br from-amber-200 to-amber-500 opacity-60" />
        </div>
        <div className="absolute inset-0 rounded-full bg-amber-400 opacity-10 blur-2xl animate-glow-pulse" />
      </div>

      <h1 className="font-cinzel text-5xl md:text-7xl font-black text-amber-400 mb-4 text-shadow-gold tracking-wide">
        MindReader
      </h1>
      <p className="font-cinzel text-lg md:text-xl text-amber-200/70 mb-2 tracking-widest uppercase">
        {t.tagline}
      </p>
      <p className="font-lato text-base text-slate-400 mb-10 max-w-md">
        {t.description}
      </p>

      {error && (
        <div className="mb-6 px-4 py-3 rounded-lg bg-red-900/40 border border-red-700/50 text-red-300 font-lato text-sm max-w-sm">
          {error}
        </div>
      )}

      <button
        onClick={onStart}
        disabled={isLoading}
        className="
          font-cinzel text-lg font-bold tracking-wider uppercase
          px-10 py-4 rounded-full
          bg-gradient-to-r from-amber-500 to-amber-600
          text-navy-950
          border-2 border-amber-400
          transition-all duration-300
          hover:from-amber-400 hover:to-amber-500
          hover:scale-105 hover:glow-gold
          disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100
          focus:outline-none focus:ring-2 focus:ring-amber-400 focus:ring-offset-2 focus:ring-offset-navy-950
        "
      >
        {isLoading ? (
          <span className="flex items-center gap-2">
            <span className="inline-block w-4 h-4 border-2 border-navy-950 border-t-transparent rounded-full animate-spin" />
            {t.consultingOracle}
          </span>
        ) : (
          t.beginButton
        )}
      </button>

      <p className="mt-8 font-lato text-xs text-slate-600 tracking-widest uppercase">
        {t.footer}
      </p>
    </div>
  );
}
