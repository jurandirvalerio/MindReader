import { describe, it, expect } from 'vitest';
import { screen } from '@testing-library/react';
import { ProgressBar } from './ProgressBar';
import { renderWithLanguage } from '../test/renderWithLanguage';

describe('ProgressBar', () => {
  it('displays current and max values', () => {
    renderWithLanguage(<ProgressBar current={5} max={20} />);

    expect(screen.getByText('5 / 20')).toBeInTheDocument();
  });

  it('displays progress label', () => {
    renderWithLanguage(<ProgressBar current={1} max={20} />);

    expect(screen.getByText('Question Progress')).toBeInTheDocument();
  });

  it('renders the fill bar with correct width percentage', () => {
    const { container } = renderWithLanguage(<ProgressBar current={10} max={20} />);

    const fill = container.querySelector('[style]');
    expect(fill).toHaveStyle({ width: '50%' });
  });

  it('caps percentage at 100% when current exceeds max', () => {
    const { container } = renderWithLanguage(<ProgressBar current={25} max={20} />);

    const fill = container.querySelector('[style]');
    expect(fill).toHaveStyle({ width: '100%' });
  });

  it('shows 0% at the start', () => {
    const { container } = renderWithLanguage(<ProgressBar current={0} max={20} />);

    const fill = container.querySelector('[style]');
    expect(fill).toHaveStyle({ width: '0%' });
  });

  it('shows correct text for question 1 of 20', () => {
    renderWithLanguage(<ProgressBar current={1} max={20} />);

    expect(screen.getByText('1 / 20')).toBeInTheDocument();
  });
});
