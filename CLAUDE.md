# MindReader — CLAUDE.md

Jogo estilo Akinator onde o usuário pensa em algo e a IA faz perguntas de sim/não para adivinhar.

## Stack

| Camada | Tecnologia |
|---|---|
| Frontend | React 18 + TypeScript + Tailwind CSS 3 + Vite |
| Backend | C# .NET 10 — Clean Architecture |
| IA | Anthropic Claude API (`claude-sonnet-4-6`) com `web_search_20250305` |
| Banco | SQLite + Entity Framework Core 9 |
| Testes | xUnit + Moq + FluentAssertions |

## Estrutura do projeto

```
MindReader/
├── frontend/               # React app (porta 5173)
│   └── src/
│       ├── components/     # GuessReveal, AnswerButtons, GameCard, ...
│       ├── pages/          # GamePage, HomePage
│       ├── hooks/          # useGame.ts, useWikipediaImage.ts
│       ├── services/       # gameService.ts (chamadas à API)
│       ├── i18n/           # LanguageContext, translations (EN/PT)
│       └── types/          # game.types.ts
│
└── backend/
    ├── src/
    │   ├── MindReader.Domain/          # Entidades, enums, interfaces
    │   ├── MindReader.Application/     # Use cases, DTOs, GamePromptBuilder
    │   ├── MindReader.Infrastructure/  # EF Core, repositórios, ClaudeAIService
    │   └── MindReader.API/             # Controllers, Program.cs
    └── tests/
        └── MindReader.Tests/           # Testes unitários
```

## Comandos essenciais

### Frontend
```bash
cd frontend
npm install
npm run dev        # http://localhost:5173
npm run build      # build de produção
```

### Backend
```bash
cd backend
dotnet run --project src/MindReader.API   # http://localhost:5000
dotnet test tests/MindReader.Tests        # roda os 54 testes unitários
```

### Migrations (EF Core)
```bash
cd backend
dotnet ef migrations add <NomeMigration> \
  --startup-project src/MindReader.API \
  --project src/MindReader.Infrastructure
```
As migrations são aplicadas automaticamente no startup via `MigrateAsync()`.

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

## Convenções de código

- **Backend**: C# com records para DTOs, `private set` em entidades de domínio, construtores explícitos de reconstrução nos objetos que vêm do banco.
- **Frontend**: componentes funcionais com hooks, Tailwind para estilo, sem bibliotecas de gerenciamento de estado externas.
- **Testes**: padrão Arrange/Act/Assert, mocks via Moq, asserções via FluentAssertions.
