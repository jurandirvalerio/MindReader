namespace MindReader.Application.UseCases;

internal static class GamePromptBuilder
{
    internal static string BuildSystemPrompt(string language)
    {
        var today = DateTime.UtcNow.ToString("MMMM dd, yyyy");
        var lang = language == "pt"
            ? "Brazilian Portuguese (pt-BR)"
            : "English";

        return
            $"Today is {today}. Your training data may be outdated — do NOT rely on it for current facts. " +
            $"You are playing 20 questions. The user thinks of something; you ask yes/no questions to guess it. " +
            $"LANGUAGE: all questions and guesses MUST be in {lang}. " +
            "MANDATORY: before guessing or asking about any specific living person, current officeholder, title holder, or recent event, " +
            "you MUST call web_search to verify up-to-date information. Never guess a specific person or role from training data alone. " +
            "After ≥5 questions, if confident, guess with: {\"isGuess\":true,\"guess\":\"<name>\",\"question\":\"<guess question in correct language>\"}. " +
            "CRITICAL: if the user answered Yes/Probably to a question asking about a specific entity, respond immediately with isGuess:true and that entity as the guess. " +
            "All other turns: {\"isGuess\":false,\"question\":\"<next yes/no question>\"}. " +
            "Never output anything outside the JSON.";
    }

    internal static string StartMessage(string language) =>
        language == "pt"
            ? "Vamos jogar! Estou pensando em algo. Comece a fazer perguntas."
            : "Let's play! I'm thinking of something. Start asking questions.";
}
