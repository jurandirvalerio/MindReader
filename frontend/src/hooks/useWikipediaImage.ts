import { useEffect, useState } from 'react';

interface WikipediaImageResult {
  imageUrl: string | null;
  isLoading: boolean;
}

export function useWikipediaImage(subject: string | null): WikipediaImageResult {
  const [imageUrl, setImageUrl] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (!subject) return;

    let cancelled = false;
    setIsLoading(true);
    setImageUrl(null);

    // Strip possessives so "seu cachorro" searches "cachorro", "my dog" searches "dog", etc.
    const possessivePattern = /^(meu|minha|meus|minhas|seu|sua|seus|suas|my|your|his|her|our|their)\s+/i;
    const searchTerm = subject.trim().replace(possessivePattern, '');

    const searchUrl =
      `https://en.wikipedia.org/w/api.php?action=query&list=search` +
      `&srsearch=${encodeURIComponent(searchTerm)}&format=json&origin=*&srlimit=1`;

    fetch(searchUrl)
      .then((res) => (res.ok ? res.json() : null))
      .then((searchData) => {
        if (cancelled) return null;
        const title: string | undefined = searchData?.query?.search?.[0]?.title;
        if (!title) return null;
        const slug = encodeURIComponent(title.replace(/ /g, '_'));
        return fetch(`https://en.wikipedia.org/api/rest_v1/page/summary/${slug}`, {
          headers: { Accept: 'application/json' },
        }).then((res) => (res.ok ? res.json() : null));
      })
      .then((data) => {
        if (cancelled) return;
        const url: string | undefined = data?.thumbnail?.source;
        setImageUrl(url ?? null);
      })
      .catch(() => {
        if (!cancelled) setImageUrl(null);
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [subject]);

  return { imageUrl, isLoading };
}
