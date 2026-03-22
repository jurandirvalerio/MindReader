import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { QuestionDisplay } from './QuestionDisplay';
import { renderWithLanguage } from '../test/renderWithLanguage';

describe('QuestionDisplay', () => {
  it('renders the question text', () => {
    renderWithLanguage(<QuestionDisplay question="Is it a person?" questionNumber={1} />);

    expect(screen.getByText('Is it a person?')).toBeInTheDocument();
  });

  it('renders the question number label', () => {
    renderWithLanguage(<QuestionDisplay question="Is it alive?" questionNumber={3} />);

    expect(screen.getByText('Question 3 of 20')).toBeInTheDocument();
  });

  it('renders question number 1 correctly', () => {
    renderWithLanguage(<QuestionDisplay question="First question?" questionNumber={1} />);

    expect(screen.getByText('Question 1 of 20')).toBeInTheDocument();
  });

  it('renders question number 20 correctly', () => {
    renderWithLanguage(<QuestionDisplay question="Last question?" questionNumber={20} />);

    expect(screen.getByText('Question 20 of 20')).toBeInTheDocument();
  });

  it('updates displayed question when prop changes', async () => {
    const { rerender } = renderWithLanguage(
      <QuestionDisplay question="First question?" questionNumber={1} />,
    );

    rerender(
      <LanguageProvider>
        <QuestionDisplay question="Second question?" questionNumber={2} />
      </LanguageProvider>,
    );

    // Component has a 200ms fade-in delay before updating displayed text
    await waitFor(() => expect(screen.getByText('Second question?')).toBeInTheDocument());
  });
});

import { LanguageProvider } from '../i18n/LanguageContext';
