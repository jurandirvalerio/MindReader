import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useWikipediaImage } from './useWikipediaImage';

const mockSearchResponse = (title: string) =>
  new Response(JSON.stringify({ query: { search: [{ title }] } }), { status: 200 });

const mockSummaryResponse = (imageUrl: string) =>
  new Response(JSON.stringify({ thumbnail: { source: imageUrl } }), { status: 200 });

describe('useWikipediaImage', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('returns null and not loading when subject is null', () => {
    const { result } = renderHook(() => useWikipediaImage(null));

    expect(result.current.imageUrl).toBeNull();
    expect(result.current.isLoading).toBe(false);
  });

  it('returns image URL after successful fetch', async () => {
    vi.spyOn(global, 'fetch')
      .mockResolvedValueOnce(mockSearchResponse('Dog'))
      .mockResolvedValueOnce(mockSummaryResponse('https://example.com/dog.jpg'));

    const { result } = renderHook(() => useWikipediaImage('Dog'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.imageUrl).toBe('https://example.com/dog.jpg');
  });

  it('strips Portuguese possessive "meu" before searching', async () => {
    const fetchSpy = vi.spyOn(global, 'fetch')
      .mockResolvedValueOnce(mockSearchResponse('cachorro'))
      .mockResolvedValueOnce(mockSummaryResponse('https://example.com/dog.jpg'));

    renderHook(() => useWikipediaImage('meu cachorro'));

    await waitFor(() => expect(fetchSpy).toHaveBeenCalled());
    expect(fetchSpy.mock.calls[0][0]).toContain('cachorro');
    expect(fetchSpy.mock.calls[0][0]).not.toContain('meu');
  });

  it('strips Portuguese possessive "seu" before searching', async () => {
    const fetchSpy = vi.spyOn(global, 'fetch')
      .mockResolvedValueOnce(mockSearchResponse('cachorro'))
      .mockResolvedValueOnce(mockSummaryResponse('https://example.com/dog.jpg'));

    renderHook(() => useWikipediaImage('seu cachorro'));

    await waitFor(() => expect(fetchSpy).toHaveBeenCalled());
    expect(fetchSpy.mock.calls[0][0]).toContain('cachorro');
    expect(fetchSpy.mock.calls[0][0]).not.toContain('seu');
  });

  it('strips English possessive "my" before searching', async () => {
    const fetchSpy = vi.spyOn(global, 'fetch')
      .mockResolvedValueOnce(mockSearchResponse('dog'))
      .mockResolvedValueOnce(mockSummaryResponse('https://example.com/dog.jpg'));

    renderHook(() => useWikipediaImage('my dog'));

    await waitFor(() => expect(fetchSpy).toHaveBeenCalled());
    expect(fetchSpy.mock.calls[0][0]).toContain('dog');
    expect(fetchSpy.mock.calls[0][0]).not.toContain('my+dog');
  });

  it('returns null when no search results found', async () => {
    vi.spyOn(global, 'fetch').mockResolvedValueOnce(
      new Response(JSON.stringify({ query: { search: [] } }), { status: 200 }),
    );

    const { result } = renderHook(() => useWikipediaImage('xyzunknown'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.imageUrl).toBeNull();
  });

  it('returns null when summary has no thumbnail', async () => {
    vi.spyOn(global, 'fetch')
      .mockResolvedValueOnce(mockSearchResponse('Something'))
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ extract: 'No image here' }), { status: 200 }),
      );

    const { result } = renderHook(() => useWikipediaImage('Something'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.imageUrl).toBeNull();
  });

  it('returns null on fetch error', async () => {
    vi.spyOn(global, 'fetch').mockRejectedValueOnce(new Error('Network error'));

    const { result } = renderHook(() => useWikipediaImage('Dog'));

    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.imageUrl).toBeNull();
  });

  it('sets isLoading to true while fetching', async () => {
    vi.spyOn(global, 'fetch').mockImplementationOnce(
      () => new Promise(() => {}), // never resolves
    );

    const { result } = renderHook(() => useWikipediaImage('Dog'));

    expect(result.current.isLoading).toBe(true);
  });
});
