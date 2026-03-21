import { StartScreen } from '../components/StartScreen';

interface HomePageProps {
  onStart: () => void;
  isLoading: boolean;
  error: string | null;
}

export function HomePage({ onStart, isLoading, error }: HomePageProps) {
  return <StartScreen onStart={onStart} isLoading={isLoading} error={error} />;
}
