import type { ReactNode } from 'react';

interface GameCardProps {
  children: ReactNode;
}

export function GameCard({ children }: GameCardProps) {
  return (
    <div className="relative w-full max-w-lg mx-auto">
      {/* Outer glow */}
      <div className="absolute -inset-1 bg-gradient-to-r from-amber-600/20 to-amber-400/20 rounded-2xl blur-xl" />

      {/* Card */}
      <div className="relative bg-navy-900/80 backdrop-blur-sm border border-amber-900/40 rounded-2xl p-6 md:p-8 shadow-2xl">
        {/* Corner decorations */}
        <div className="absolute top-3 left-3 w-6 h-6 border-t-2 border-l-2 border-amber-700/50 rounded-tl-lg" />
        <div className="absolute top-3 right-3 w-6 h-6 border-t-2 border-r-2 border-amber-700/50 rounded-tr-lg" />
        <div className="absolute bottom-3 left-3 w-6 h-6 border-b-2 border-l-2 border-amber-700/50 rounded-bl-lg" />
        <div className="absolute bottom-3 right-3 w-6 h-6 border-b-2 border-r-2 border-amber-700/50 rounded-br-lg" />

        {children}
      </div>
    </div>
  );
}
