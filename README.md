# MindReader — The Oracle Knows All

An Akinator-style 20-questions game where an AI reads your mind. Think of anything — a person, animal, place, food, fictional character — and the Oracle will guess it in 20 questions or less.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Frontend (React)                     │
│         Vite + TypeScript + Tailwind CSS                 │
│   StartScreen → GamePage → GuessReveal → ResultScreen   │
└────────────────────────┬────────────────────────────────┘
                         │ HTTP (proxy → :5000)
┌────────────────────────▼────────────────────────────────┐
│                   API Layer (.NET 10)                    │
│              ASP.NET Core Web API + Swagger              │
│         POST /api/game/start  |  POST /api/game/answer   │
├─────────────────────────────────────────────────────────┤
│               Application Layer (Use Cases)              │
│          StartGameUseCase  |  AnswerQuestionUseCase      │
├─────────────────────────────────────────────────────────┤
│                  Domain Layer (Pure C#)                  │
│     GameSession · QuestionAnswer · IGameSessionRepository│
├─────────────────────────────────────────────────────────┤
│                Infrastructure Layer                      │
│   ClaudeAIService (HTTP) | GameSessionRepository (EF)   │
│              SQLite Database (mindreader.db)             │
└─────────────────────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│              Anthropic Claude API                        │
│           claude-sonnet-4-20250514                       │
└─────────────────────────────────────────────────────────┘
```

## Prerequisites

- Node.js 18+
- .NET 10 SDK
- Anthropic Claude API key

## Setup & Running

### 1. Set your Claude API key

```bash
export CLAUDE_API_KEY=your_api_key_here
```

### 2. Start the backend

```bash
cd backend
dotnet restore
dotnet run --project src/MindReader.API
```

The API will start on http://localhost:5000. The SQLite database (`mindreader.db`) is created automatically on first run via EF Core migrations.

### 3. Start the frontend

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:5173 in your browser.

## API Endpoints

### POST /api/game/start

Starts a new game session. Returns the first question.

**Response:**
```json
{
  "sessionId": "uuid",
  "question": "Is it a living thing?",
  "questionNumber": 1
}
```

### POST /api/game/answer

Submit an answer to the current question.

**Request:**
```json
{
  "sessionId": "uuid",
  "answer": "Yes" | "No" | "IDontKnow" | "Probably" | "ProbablyNot"
}
```

**Response:**
```json
{
  "sessionId": "uuid",
  "question": "Is it a mammal?",
  "isGuess": false,
  "guessSubject": null,
  "questionNumber": 2,
  "isGameOver": false
}
```

When the AI makes a guess (`isGuess: true`):
```json
{
  "sessionId": "uuid",
  "question": "Is it a cat?",
  "isGuess": true,
  "guessSubject": "cat",
  "questionNumber": 7,
  "isGameOver": true
}
```

## Notes

- The SQLite database file `mindreader.db` is created automatically on first run and persists between sessions.
- Game sessions are stored in the database, allowing review of past games.
- Maximum 20 questions per game — if the AI can't guess, it gives up gracefully.
- No API keys are hardcoded anywhere; the Claude API key is read from the `CLAUDE_API_KEY` environment variable.
