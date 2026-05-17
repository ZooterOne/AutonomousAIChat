using System.ComponentModel;
using OllamaSharp;
using Spectre.Console;

namespace AutonomousAIChat;

/// <summary>
/// Provides utility methods for setting up and configuring an <see cref="AIPersona"/>.
/// </summary>
internal static class AIPersonaSetup
{
    /// <summary>
    /// Interactively prompts the user to configure the attributes of a new AI Persona using a console interface.
    /// </summary>
    /// <param name="name">The name of the AI Persona to create.</param>
    /// <param name="ollamaClient">The Ollama API client used to communicate with the local LLM.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly configured <see cref="AIPersona"/>.</returns>
    public static async Task<AIPersona> GenerateAIPersona(string name, OllamaApiClient ollamaClient, 
        CancellationToken cancellationToken)
    {
        // Select the gender
        var genderPrompt = new SelectionPrompt<AIPersona.GenderIdentity>()
            .Title("Select the gender:")
            .AddChoices((AIPersona.GenderIdentity[])Enum.GetValues(typeof(AIPersona.GenderIdentity)))
            .UseConverter(item => item.ToString())
            .WrapAround()
            .PageSize(ChatCommand.PageSize)
            .MoreChoicesText(ChatCommand.PageFooterText);
        var selectedGender = await AnsiConsole.PromptAsync(genderPrompt, cancellationToken);
        AnsiConsole.MarkupLine($"Gender: [DarkSeaGreen4 bold]{selectedGender.ToString()}[/].");

        // Query endless conversation
        var booleanPrompt = new SelectionPrompt<bool>()
            .Title("Keep talking:")
            .AddChoices(true, false)
            .WrapAround()
            .UseConverter(item => item ? "Yes" : "No");
        var endlessConversation = await AnsiConsole.PromptAsync(booleanPrompt, cancellationToken);
        AnsiConsole.MarkupLine($"Endless Conversation: [DarkSeaGreen4 bold]{(endlessConversation ? "Yes" : "No")}[/].");

        // Query short answers
        booleanPrompt = new SelectionPrompt<bool>()
            .Title("Short answers:")
            .AddChoices(true, false)
            .WrapAround()
            .UseConverter(item => item ? "Yes" : "No");
        var shortAnswers = await AnsiConsole.PromptAsync(booleanPrompt, cancellationToken);
        AnsiConsole.MarkupLine($"Short Answers: [DarkSeaGreen4 bold]{(shortAnswers ? "Yes" : "No")}[/].");

        // Select tone
        var tonePrompt = new SelectionPrompt<AIPersona.Tone>()
            .Title("Select the tone:")
            .AddChoices((AIPersona.Tone[])Enum.GetValues(typeof(AIPersona.Tone)))
            .UseConverter(item => item.ToString())
            .WrapAround()
            .PageSize(ChatCommand.PageSize)
            .MoreChoicesText(ChatCommand.PageFooterText);
        var selectedTone = await AnsiConsole.PromptAsync(tonePrompt, cancellationToken);
        AnsiConsole.MarkupLine($"Tone: [DarkSeaGreen4 bold]{selectedTone.ToString()}[/].");

        // Select interests
        var interestsPrompt = new MultiSelectionPrompt<AIPersona.Interest>()
            .Title("Select the interests:")
            .AddChoices((AIPersona.Interest[])Enum.GetValues(typeof(AIPersona.Interest)))
            .UseConverter(item => item.ToString())
            .WrapAround()
            .PageSize(ChatCommand.PageSize)
            .MoreChoicesText("[Gray23](Use arrow keys to reveal more entries)[/]")
            .InstructionsText("[Grey]Press [DarkSeaGreen4]<space>[/] to select/deselect an entry, " +
                              "and [DarkSeaGreen4]<enter>[/] to accept.[/]");
        var selectedInterests = await AnsiConsole.PromptAsync(interestsPrompt, cancellationToken);
        AnsiConsole.MarkupLine($"Interests: [DarkSeaGreen4 bold]{string.Join(" + ",
            selectedInterests.Select(item => item.ToString()))}[/].");
        
        // Add additional context
        var detailsPrompt =
            new TextPrompt<string>(
                    "Any additional details? for example [DarkSeaGreen4 italic]You want some recommendations on books to read[/]. [Grey italic]Leave empty to ignore[/]: ")
                .AllowEmpty()
                .ClearOnFinish();
        var additionalDetails = await AnsiConsole.PromptAsync(detailsPrompt, cancellationToken);

        return new AIPersona(name, ollamaClient)
        {
            Gender = selectedGender,
            EndlessConversation = endlessConversation,
            ShortAnswers = shortAnswers,
            Spirit = selectedTone,
            Interests = selectedInterests.Aggregate((result, next) => result | next),
            AdditionalDetails = additionalDetails
        };
    }
}