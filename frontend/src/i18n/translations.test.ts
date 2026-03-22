import { describe, it, expect } from 'vitest';
import { translations } from './translations';

describe('translations', () => {
  const enKeys = Object.keys(translations.en) as Array<keyof typeof translations.en>;

  it('Portuguese has all keys that English has', () => {
    for (const key of enKeys) {
      expect(translations.pt).toHaveProperty(key);
    }
  });

  it('English has all keys that Portuguese has', () => {
    const ptKeys = Object.keys(translations.pt);
    for (const key of ptKeys) {
      expect(translations.en).toHaveProperty(key);
    }
  });

  it('no English value is empty', () => {
    for (const key of enKeys) {
      const value = translations.en[key];
      if (typeof value === 'string') {
        expect(value.trim()).not.toBe('');
      }
    }
  });

  it('no Portuguese value is empty', () => {
    for (const key of enKeys) {
      const value = translations.pt[key];
      if (typeof value === 'string') {
        expect(value.trim()).not.toBe('');
      }
    }
  });

  it('questionOf returns correct string in English', () => {
    expect(translations.en.questionOf(5, 20)).toBe('Question 5 of 20');
  });

  it('questionOf returns correct string in Portuguese', () => {
    expect(translations.pt.questionOf(5, 20)).toBe('Pergunta 5 de 20');
  });

  it('English and Portuguese taglines are different', () => {
    expect(translations.en.tagline).not.toBe(translations.pt.tagline);
  });
});
