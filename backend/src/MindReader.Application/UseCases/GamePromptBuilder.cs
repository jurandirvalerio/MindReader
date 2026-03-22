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
            "STRATEGY: always start with the most universally clear category questions first: begin by asking 'Is it a person?' (real or fictional), then narrow down from there. Avoid abstract or philosophical first questions like 'Is it tangible?'. " +
            "IMPORTANT: the subject may be a personal/specific instance — e.g. 'my dog', 'my car', 'my father'. " +
            "Once you confirm it belongs personally to the user (e.g. 'Is it your own pet?') AND identify the broad category (e.g. dog, father, car), guess immediately using the possessive — e.g. 'seu cachorro', 'seu pai', 'seu carro'. Do NOT ask further about breed, model, name or other details. " +
            "MANDATORY: before guessing or asking about any specific living person, current officeholder, title holder, or recent event, " +
            "you MUST call web_search to verify up-to-date information. Never guess a specific person or role from training data alone. " +
            "After ≥5 questions, if confident, guess with: {\"isGuess\":true,\"guess\":\"<name>\",\"question\":\"<guess question in correct language>\"}. " +
            "CRITICAL: if the user answered Yes/Probably to a question asking about a specific entity, respond immediately with isGuess:true and that entity as the guess. " +
            "All other turns: {\"isGuess\":false,\"question\":\"<next yes/no question>\"}. " +
            "Never output anything outside the JSON.";
    }

    internal static string BuildForcedGuessPrompt(string language)
    {
        var lang = language == "pt"
            ? "Brazilian Portuguese (pt-BR)"
            : "English";

        return
            $"LANGUAGE: respond MUST be in {lang}. " +
            "You have used all 20 questions. You MUST now make your single best guess based on everything learned so far. " +
            "Do NOT ask another question. Respond ONLY with: " +
            "{\"isGuess\":true,\"guess\":\"<your best guess>\",\"question\":\"<guess statement in correct language>\"}. " +
            "Never output anything outside the JSON.";
    }

    internal static string StartMessage(string language) =>
        language == "pt"
            ? "Vamos jogar! Estou pensando em algo. Comece a fazer perguntas."
            : "Let's play! I'm thinking of something. Start asking questions.";
}
