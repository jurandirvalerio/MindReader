import { describe, it, expect, vi } from 'vitest';
import React from 'react';
import { screen, render } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { AnswerButtons } from './AnswerButtons';
import { renderWithLanguage } from '../test/renderWithLanguage';

describe('AnswerButtons', () => {
  it('renders all 5 answer buttons', () => {
    renderWithLanguage(<AnswerButtons onAnswer={vi.fn()} isLoading={false} />);

    expect(screen.getByText('Yes')).toBeInTheDocument();
    expect(screen.getByText('Probably')).toBeInTheDocument();
    expect(screen.getByText("I don't know")).toBeInTheDocument();
    expect(screen.getByText('Probably not')).toBeInTheDocument();
    expect(screen.getByText('No')).toBeInTheDocument();
  });

  it('calls onAnswer with correct value when button is clicked', async () => {
    const onAnswer = vi.fn();
    renderWithLanguage(<AnswerButtons onAnswer={onAnswer} isLoading={false} />);

    await userEvent.click(screen.getByText('Yes'));

    expect(onAnswer).toHaveBeenCalledWith('Yes');
    expect(onAnswer).toHaveBeenCalledTimes(1);
  });

  it('calls onAnswer with correct value for each button', async () => {
    const onAnswer = vi.fn();
    renderWithLanguage(<AnswerButtons onAnswer={onAnswer} isLoading={false} />);

    await userEvent.click(screen.getByText('No'));
    expect(onAnswer).toHaveBeenCalledWith('No');

    await userEvent.click(screen.getByText('Probably'));
    expect(onAnswer).toHaveBeenCalledWith('Probably');
  });

  it('disables all buttons when loading', () => {
    renderWithLanguage(<AnswerButtons onAnswer={vi.fn()} isLoading={true} />);

    const buttons = screen.getAllByRole('button');
    buttons.forEach((button) => expect(button).toBeDisabled());
  });

  it('does not call onAnswer when buttons are disabled', async () => {
    const onAnswer = vi.fn();
    renderWithLanguage(<AnswerButtons onAnswer={onAnswer} isLoading={true} />);

    const buttons = screen.getAllByRole('button');
    for (const button of buttons) {
      await userEvent.click(button);
    }

    expect(onAnswer).not.toHaveBeenCalled();
  });

  it('shows loading spinner only on the clicked button', async () => {
    // Use a wrapper that controls isLoading so the component never sees
    // isLoading=false between the click and the loading state.
    function Wrapper() {
      const [loading, setLoading] = React.useState(false);
      return (
        <LanguageProvider>
          <AnswerButtons
            onAnswer={() => setLoading(true)}
            isLoading={loading}
          />
        </LanguageProvider>
      );
    }
    render(<Wrapper />);

    await userEvent.click(screen.getByText('Yes'));

    expect(screen.getByText('Reading minds...')).toBeInTheDocument();
    expect(screen.queryAllByText('Reading minds...')).toHaveLength(1);
  });

  it('renders buttons in correct order: Yes, Probably, IDontKnow, ProbablyNot, No', () => {
    renderWithLanguage(<AnswerButtons onAnswer={vi.fn()} isLoading={false} />);

    const buttons = screen.getAllByRole('button');
    expect(buttons[0]).toHaveTextContent('Yes');
    expect(buttons[1]).toHaveTextContent('Probably');
    expect(buttons[2]).toHaveTextContent("I don't know");
    expect(buttons[3]).toHaveTextContent('Probably not');
    expect(buttons[4]).toHaveTextContent('No');
  });
});

// needed for rerender with provider
import { LanguageProvider } from '../i18n/LanguageContext';
