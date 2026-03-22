import { render, type RenderResult } from '@testing-library/react';
import type { ReactNode } from 'react';
import { LanguageProvider } from '../i18n/LanguageContext';

export function renderWithLanguage(ui: ReactNode): RenderResult {
  return render(<LanguageProvider>{ui}</LanguageProvider>);
}
