import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { GuessReveal } from './GuessReveal';
import { renderWithLanguage } from '../test/renderWithLanguage';

vi.mock('../services/gameService', () => ({
  recordMiss: vi.fn().mockResolvedValue(undefined),
}));

vi.mock('../hooks/useWikipediaImage', () => ({
  useWikipediaImage: () => ({ imageUrl: null, isLoading: false }),
}));

describe('GuessReveal', () => {
  const defaultProps = {
    sessionId: 'session-123',
    guess: 'Dog',
    onCorrect: vi.fn(),
    onWrong: vi.fn(),
    isLoading: false,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('displays the guessed subject', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => expect(screen.getByText('Dog')).toBeInTheDocument());
  });

  it('displays "Was I right?" prompt', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => expect(screen.getByText('Was I right?')).toBeInTheDocument());
  });

  it('calls onCorrect when "Yes!" is clicked', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => screen.getByText('Yes!'));
    await userEvent.click(screen.getByText('Yes!'));

    expect(defaultProps.onCorrect).toHaveBeenCalledTimes(1);
  });

  it('shows correction input when "No" is clicked', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => screen.getByText('No'));
    await userEvent.click(screen.getByText('No'));

    expect(screen.getByPlaceholderText('Enter the correct answer...')).toBeInTheDocument();
  });

  it('shows correct answer prompt after clicking No', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => screen.getByText('No'));
    await userEvent.click(screen.getByText('No'));

    expect(screen.getByText('What were you thinking of?')).toBeInTheDocument();
  });

  it('submit button is disabled when input is empty', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => screen.getByText('No'));
    await userEvent.click(screen.getByText('No'));

    expect(screen.getByText('Submit')).toBeDisabled();
  });

  it('submit button is enabled after typing a correct answer', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => screen.getByText('No'));
    await userEvent.click(screen.getByText('No'));
    await userEvent.type(screen.getByPlaceholderText('Enter the correct answer...'), 'Cat');

    expect(screen.getByText('Submit')).toBeEnabled();
  });

  it('calls onWrong after submitting correction', async () => {
    const { recordMiss } = await import('../services/gameService');
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => screen.getByText('No'));
    await userEvent.click(screen.getByText('No'));
    await userEvent.type(screen.getByPlaceholderText('Enter the correct answer...'), 'Cat');
    await userEvent.click(screen.getByText('Submit'));

    await waitFor(() => expect(defaultProps.onWrong).toHaveBeenCalledTimes(1));
    expect(recordMiss).toHaveBeenCalledWith('session-123', 'Dog', 'Cat');
  });

  it('calls onWrong immediately when Skip is clicked', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => screen.getByText('No'));
    await userEvent.click(screen.getByText('No'));
    await userEvent.click(screen.getByText('Skip'));

    expect(defaultProps.onWrong).toHaveBeenCalledTimes(1);
  });

  it('disables Yes and No buttons when isLoading is true', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} isLoading={true} />);

    await waitFor(() => screen.getByText('Yes!'));
    expect(screen.getByText('Yes!')).toBeDisabled();
    expect(screen.getByText('No')).toBeDisabled();
  });

  it('submits correction on Enter key press', async () => {
    renderWithLanguage(<GuessReveal {...defaultProps} />);

    await waitFor(() => screen.getByText('No'));
    await userEvent.click(screen.getByText('No'));
    await userEvent.type(screen.getByPlaceholderText('Enter the correct answer...'), 'Cat{Enter}');

    await waitFor(() => expect(defaultProps.onWrong).toHaveBeenCalledTimes(1));
  });
});
