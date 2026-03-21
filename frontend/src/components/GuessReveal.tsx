import { useEffect, useState } from 'react';

interface GuessRevealProps {
  guess: string;
  onCorrect: () => void;
  onWrong: () => void;
  isLoading: boolean;
}

export function GuessReveal({ guess, onCorrect, onWrong, isLoading }: GuessRevealProps) {
  const [revealed, setRevealed] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setRevealed(true), 400);
    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="flex flex-col items-center justify-center text-center px-4">
      {/* Dramatic orb */}
      <div className="relative mb-8">
        <div
          className={`
            w-48 h-48 rounded-full
            bg-gradient-to-br from-amber-300 via-yellow-500 to-amber-600
            flex items-center justify-center
            transition-all duration-1000 ease-out
            ${revealed ? 'animate-orb-float glow-gold-intense opacity-100 scale-100' : 'opacity-0 scale-50'}
          `}
        >
          <div className="w-36 h-36 rounded-full bg-gradient-to-br from-amber-100 to-amber-400 opacity-60" />
        </div>
        <div className="absolute inset-0 rounded-full bg-amber-400 opacity-20 blur-3xl animate-glow-pulse" />
      </div>

      <p
        className={`
          font-cinzel text-sm uppercase tracking-widest text-amber-600 mb-3
          transition-all duration-700 delay-300
          ${revealed ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}
        `}
      >
        The Oracle declares...
      </p>

      <h2
        className={`
          font-cinzel text-3xl md:text-4xl font-black text-amber-400 mb-2 text-shadow-gold
          transition-all duration-700 delay-500
          ${revealed ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}
        `}
      >
        {guess}
      </h2>

      <p
        className={`
          font-lato text-lg text-slate-300 mb-10
          transition-all duration-700 delay-700
          ${revealed ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}
        `}
      >
        Was I right?
      </p>

      <div
        className={`
          flex gap-4 w-full max-w-xs
          transition-all duration-700 delay-1000
          ${revealed ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}
        `}
      >
        <button
          onClick={onCorrect}
          disabled={isLoading}
          className="
            flex-1 font-cinzel font-bold tracking-wide text-lg py-3.5 rounded-xl
            bg-gradient-to-r from-emerald-700 to-emerald-600
            border border-emerald-500 text-white
            hover:from-emerald-600 hover:to-emerald-500 hover:scale-105
            transition-all duration-200
            disabled:opacity-50 disabled:cursor-not-allowed
            focus:outline-none focus:ring-2 focus:ring-emerald-400
          "
        >
          Yes!
        </button>
        <button
          onClick={onWrong}
          disabled={isLoading}
          className="
            flex-1 font-cinzel font-bold tracking-wide text-lg py-3.5 rounded-xl
            bg-gradient-to-r from-red-900 to-red-800
            border border-red-700 text-white
            hover:from-red-800 hover:to-red-700 hover:scale-105
            transition-all duration-200
            disabled:opacity-50 disabled:cursor-not-allowed
            focus:outline-none focus:ring-2 focus:ring-red-400
          "
        >
          No
        </button>
      </div>
    </div>
  );
}
