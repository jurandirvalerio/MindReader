# MindReader — CLAUDE.md

Jogo estilo Akinator onde o usuário pensa em algo e a IA faz perguntas de sim/não para adivinhar.

## Stack

| Camada | Tecnologia |
|---|---|
| Frontend | React 18 + TypeScript + Tailwind CSS 3 + Vite |
| Backend | C# .NET 10 — Clean Architecture |
| IA | Anthropic Claude API (`claude-sonnet-4-6`) com `web_search_20250305` |
| Banco | SQLite + Entity Framework Core 9 |
| Testes Backend | xUnit + Moq + FluentAssertions + coverlet |
| Testes Frontend | Vitest + React Testing Library + @testing-library/jest-dom |

## Estrutura do projeto

```
MindReader/
├── frontend/               # React app (porta 5173)
│   └── src/
│       ├── components/     # GuessReveal, AnswerButtons, GameCard, ...
│       │   ├── AnswerButtons.test.tsx
│       │   ├── GuessReveal.test.tsx
│       │   ├── ProgressBar.test.tsx
│       │   └── QuestionDisplay.test.tsx
│       ├── pages/          # GamePage, HomePage
│       ├── hooks/          # useGame.ts, useWikipediaImage.ts
│       │   └── useWikipediaImage.test.ts
│       ├── services/       # gameService.ts (chamadas à API)
│       │   └── gameService.test.ts
│       ├── i18n/           # LanguageContext, translations (EN/PT)
│       │   └── translations.test.ts
│       ├── types/          # game.types.ts
│       └── test/           # setup.ts, renderWithLanguage.tsx
│
└── backend/
    ├── src/
    │   ├── MindReader.Domain/          # Entidades, enums, interfaces
    │   ├── MindReader.Application/     # Use cases, DTOs, GamePromptBuilder
    │   │   └── InternalsVisibleTo.cs   # Expõe internals para MindReader.Tests
    │   ├── MindReader.Infrastructure/  # EF Core, repositórios, ClaudeAIService
    │   └── MindReader.API/             # Controllers, Program.cs
    └── tests/
        └── MindReader.Tests/
            ├── Domain/
            │   ├── GameSessionTests.cs
            │   └── QuestionAnswerTests.cs
            └── Application/
                ├── AnswerQuestionUseCaseTests.cs
                ├── GamePromptBuilderTests.cs
                ├── RecordMissUseCaseTests.cs
                └── StartGameUseCaseTests.cs
```

## Comandos essenciais

### Frontend
```bash
cd frontend
npm install
npm run dev          # http://localhost:5173
npm run build        # build de produção
npm test             # roda os 52 testes unitários (uma vez)
npm run test:watch   # modo watch
npx vitest run --coverage  # relatório de cobertura
```

### Backend
```bash
cd backend
dotnet run --project src/MindReader.API        # http://localhost:5000
dotnet test tests/MindReader.Tests             # roda os 54 testes unitários
dotnet test tests/MindReader.Tests \
  --collect:"XPlat Code Coverage"              # coleta cobertura (coverlet)
```

### Migrations (EF Core)
```bash
cd backend
dotnet ef migrations add <NomeMigration> \
  --startup-project src/MindReader.API \
  --project src/MindReader.Infrastructure
```
As migrations são aplicadas automaticamente no startup via `MigrateAsync()`.

## CI/CD — SonarCloud

O workflow `.github/workflows/sonarcloud.yml` executa a cada push ou PR na `main`:
1. Roda os testes do frontend com cobertura (lcov).
2. Inicia o SonarScanner for .NET.
3. Builda o backend e roda os testes com cobertura (OpenCover).
4. Envia os resultados para o SonarCloud.

**Secrets necessários no GitHub** (`Settings → Secrets → Actions`):

| Secret | Onde obter |
|---|---|
| `SONAR_TOKEN` | SonarCloud → My Account → Security → Generate Token |
| `SONAR_PROJECT_KEY` | SonarCloud → projeto → Information |
| `SONAR_ORG_KEY` | SonarCloud → Organization → Settings |

## Variável de ambiente obrigatória

```bash
CLAUDE_API_KEY=sk-ant-...
```

Sem essa variável o backend não inicia.

## Arquitetura Clean — regras importantes

- **Domain** não referencia nenhuma outra camada.
- **Application** referencia apenas Domain. Use cases dependem de interfaces (`IGameSessionRepository`, `IClaudeAIService`, `IOracleMissRepository`).
- **Infrastructure** implementa as interfaces do Domain (repositórios, `ClaudeAIService`).
- **API** é o ponto de entrada; registra tudo via `AddInfrastructure()` em `DependencyInjection.cs`.
- `GamePromptBuilder` é `internal` — exposto para testes via `InternalsVisibleTo("MindReader.Tests")` em `InternalsVisibleTo.cs`.

## Fluxo do jogo

1. `POST /api/game/start` → cria sessão, retorna primeira pergunta.
2. `POST /api/game/answer` → salva `(perguntaAtual, resposta)` → chama Claude → retorna próxima pergunta ou `isGuess: true`.
3. Na **pergunta 20**: se Claude não adivinhou, uma segunda chamada com `BuildForcedGuessPrompt` força o melhor chute.
4. `POST /api/game/miss` → grava a resposta correta quando o oráculo errou (para análise futura).

## Integração com Claude

- Modelo: `claude-sonnet-4-6`, `max_tokens: 1024`.
- Toda resposta deve ser JSON puro: `{"isGuess": false, "question": "..."}` ou `{"isGuess": true, "guess": "...", "question": "..."}`.
- O loop agêntico em `ClaudeAIService.AskAsync` lida com `stop_reason: "tool_use"` para o web search.
- Retry com backoff `[1s, 3s, 8s]` para erros 429; lança `RateLimitException` após esgotar tentativas.

## Banco de dados (SQLite)

Tabelas:
- `GameSessions` — sessão com status (`InProgress`, `Guessed`, `GaveUp`).
- `QuestionAnswers` — pares pergunta/resposta ligados à sessão.
- `OracleMisses` — correções enviadas pelo usuário quando o oráculo erra.

## Internacionalização (i18n)

- Idiomas suportados: `en` (padrão) e `pt` (pt-BR).
- Selecionado via `LanguageContext`; persiste no `localStorage`.
- O idioma é enviado ao backend em cada requisição e injetado no system prompt do Claude.

## Wikipedia (imagem da adivinhação)

O hook `useWikipediaImage` busca a imagem do sujeito adivinhado:
1. Faz search na Wikipedia para normalizar o termo (resolve traduções como "Pastor Belga" → "Belgian Shepherd").
2. Pronomes possessivos (`meu`, `seu`, `my`, `your`, etc.) são removidos antes da busca.
3. Fallback: orbe dourada animada quando nenhuma imagem é encontrada.

## Testes unitários

### Backend (54 testes — xUnit + Moq + FluentAssertions)

| Arquivo | Cobertura | O que testa |
|---|---|---|
| `Domain/GameSessionTests.cs` | Domain | Estado inicial, `AddQuestion`, ordenação, `MarkAsGuessed/GaveUp` |
| `Domain/QuestionAnswerTests.cs` | Domain | Construtores, geração de Id, reconstrução do banco, todos os `AnswerType` |
| `Application/GamePromptBuilderTests.cs` | Application 100% | Idioma, data atual, JSON-only, web search obrigatório, `ForcedGuessPrompt` |
| `Application/StartGameUseCaseTests.cs` | Application 100% | Primeira pergunta, SessionId único, save no repo, idioma propagado |
| `Application/AnswerQuestionUseCaseTests.cs` | Application 94% | Sessão não encontrada, turno normal, adivinhação, Q20 forçado, `ParseAnswer`, escape JSON |
| `Application/RecordMissUseCaseTests.cs` | Application 100% | Salva com trim, ignora resposta vazia |

**Cobertura por camada:** Application **96.5%** · Domain **72.2%** · Infrastructure **0%** (requer testes de integração)

> Infrastructure não é coberta intencionalmente nos testes unitários: `ClaudeAIService`, repositórios EF Core e migrations dependem de HTTP real e banco real.

### Frontend (52 testes — Vitest + React Testing Library)

| Arquivo | Cobertura | O que testa |
|---|---|---|
| `AnswerButtons.test.tsx` | 100% | 5 botões, callbacks, desabilitação, spinner apenas no botão clicado |
| `ProgressBar.test.tsx` | 100% | Texto, largura do fill, cap a 100%, zero |
| `QuestionDisplay.test.tsx` | 100% | Texto, número, atualização com fade-in delay |
| `GuessReveal.test.tsx` | 96% | Adivinhação exibida, `onCorrect`, campo de correção, submit/skip, Enter, loading |
| `useWikipediaImage.test.ts` | 94% | Strip de possessivos PT/EN, sucesso, sem resultados, erro de rede, loading |
| `gameService.test.ts` | 92% | Payload dos 3 endpoints, erros HTTP, 429 |
| `translations.test.ts` | 100% | Paridade de chaves EN/PT, valores não vazios, `questionOf` dinâmico |

**Cobertura geral:** Statements **93.96%** · Lines **98.05%** · Branches **71.21%**

**Utilitários de teste:**
- `src/test/setup.ts` — importa `@testing-library/jest-dom` globalmente.
- `src/test/renderWithLanguage.tsx` — wrapper que envolve o componente em `LanguageProvider`.

## Convenções de código

- **Backend**: C# com records para DTOs, `private set` em entidades de domínio, construtores explícitos de reconstrução nos objetos que vêm do banco.
- **Frontend**: componentes funcionais com hooks, Tailwind para estilo, sem bibliotecas de gerenciamento de estado externas.
- **Testes backend**: padrão Arrange/Act/Assert, mocks via Moq, asserções via FluentAssertions.
- **Testes frontend**: `renderWithLanguage` para componentes que usam `useTranslation`, `vi.mock` para isolar dependências externas (`fetch`, hooks), `waitFor` para efeitos assíncronos e animações.
