namespace MindReader.Application.UseCases;

internal static class GamePromptBuilder
{
    internal static string BuildSystemPrompt(string language)
    {
        var today = DateTime.UtcNow.ToString("MMMM dd, yyyy");

        var langInstruction = language == "pt"
            ? "You MUST ask all questions and make all guesses in Brazilian Portuguese (pt-BR)."
            : "You MUST ask all questions and make all guesses in English.";

        return
            $"Today's date is {today}. {langInstruction} " +
            "You are playing a 20 questions game. The user is thinking of something (a person, animal, fictional character, object, place, food, etc.). " +
            "Ask one yes/no question at a time to figure it out. After each answer, ask the next most strategic question. " +
            "IMPORTANT: When the subject might involve current or recent information (such as who currently holds a position, " +
            "recent events, current champions, living people's roles, etc.), use the web_search tool to verify before guessing. " +
            "When you are confident (after at least 5 questions), make your guess by responding ONLY with a JSON object like: " +
            "{\"isGuess\": true, \"guess\": \"<what you think it is>\", \"question\": \"<your guess question in the correct language>\"}. " +
            "CRITICAL: If the user just answered 'Yes' or 'Probably' to your most recent question and that question was asking " +
            "whether the thing is a specific entity (e.g. 'Is it a Belgian Shepherd?', 'Is it Napoleon?'), " +
            "that means you guessed correctly. You MUST respond with isGuess:true and the correct guess. " +
            "For all other questions, respond ONLY with a JSON object like: {\"isGuess\": false, \"question\": \"<your question here>\"}. " +
            "Never include any text outside the JSON.";
    }

    internal static string StartMessage(string language) =>
        language == "pt"
            ? "Vamos jogar! Estou pensando em algo. Comece a fazer perguntas."
            : "Let's play! I'm thinking of something. Start asking questions.";
}
