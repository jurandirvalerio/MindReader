using FluentAssertions;
using MindReader.Application.UseCases;

namespace MindReader.Tests.Application;

public class GamePromptBuilderTests
{
    [Fact]
    public void BuildSystemPrompt_English_ShouldContainEnglishInstruction()
    {
        var prompt = GamePromptBuilder.BuildSystemPrompt("en");

        prompt.Should().Contain("English");
        prompt.Should().NotContain("Portuguese");
    }

    [Fact]
    public void BuildSystemPrompt_Portuguese_ShouldContainPortugueseInstruction()
    {
        var prompt = GamePromptBuilder.BuildSystemPrompt("pt");

        prompt.Should().Contain("Portuguese");
        prompt.Should().NotContain("LANGUAGE: all questions and guesses MUST be in English");
    }

    [Fact]
    public void BuildSystemPrompt_ShouldContainTodaysDate()
    {
        var prompt = GamePromptBuilder.BuildSystemPrompt("en");
        var today = DateTime.UtcNow.ToString("MMMM dd, yyyy");

        prompt.Should().Contain(today);
    }

    [Fact]
    public void BuildSystemPrompt_ShouldRequireJsonOnlyOutput()
    {
        var prompt = GamePromptBuilder.BuildSystemPrompt("en");

        prompt.Should().Contain("Never output anything outside the JSON");
    }

    [Fact]
    public void BuildSystemPrompt_ShouldInstructWebSearchForCurrentFacts()
    {
        var prompt = GamePromptBuilder.BuildSystemPrompt("en");

        prompt.Should().Contain("web_search");
        prompt.Should().Contain("MANDATORY");
    }

    [Fact]
    public void BuildSystemPrompt_ShouldMentionPersonalSubjects()
    {
        var prompt = GamePromptBuilder.BuildSystemPrompt("en");

        prompt.Should().Contain("personal");
    }

    [Fact]
    public void BuildSystemPrompt_ShouldStartWithCategoryStrategy()
    {
        var prompt = GamePromptBuilder.BuildSystemPrompt("en");

        prompt.Should().Contain("STRATEGY");
        prompt.Should().Contain("person");
    }

    [Fact]
    public void BuildForcedGuessPrompt_English_ShouldForceGuess()
    {
        var prompt = GamePromptBuilder.BuildForcedGuessPrompt("en");

        prompt.Should().Contain("MUST");
        prompt.Should().Contain("isGuess");
        prompt.Should().Contain("true");
    }

    [Fact]
    public void BuildForcedGuessPrompt_Portuguese_ShouldContainPortugueseInstruction()
    {
        var prompt = GamePromptBuilder.BuildForcedGuessPrompt("pt");

        prompt.Should().Contain("Portuguese");
    }

    [Fact]
    public void BuildForcedGuessPrompt_ShouldProhibitMoreQuestions()
    {
        var prompt = GamePromptBuilder.BuildForcedGuessPrompt("en");

        prompt.Should().Contain("Do NOT ask another question");
    }

    [Theory]
    [InlineData("en", "Let's play")]
    [InlineData("pt", "Vamos jogar")]
    public void StartMessage_ShouldReturnCorrectLanguage(string language, string expected)
    {
        var message = GamePromptBuilder.StartMessage(language);

        message.Should().Contain(expected);
    }
}
