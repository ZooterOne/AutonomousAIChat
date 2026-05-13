using System.Reflection.Metadata;
using OllamaSharp;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutonomousAIChat;

/// <summary>
/// Defines the main command (detection).
/// </summary>
internal sealed class ChatCommand : AsyncCommand<ChatCommandSettings>
{
    private const string OllamaDefaultUrl = "http://localhost:11434";
    private const int ConversationWaitTime = 2000;
    
    public const string PageFooterText = "[Gray23](Use arrow keys to reveal more entries)[/]";
    public const int PageSize = 5;

    private OllamaApiClient? m_ollamaClient;

    private static async Task<OllamaApiClient?> GetOllamaClientAsync(string url)
    {
        try
        {
            var ollamaClient = new OllamaApiClient(url);
            return await ollamaClient.IsRunningAsync() ? ollamaClient : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> Connect(string url)
    {
        m_ollamaClient = await GetOllamaClientAsync(url);
        if (m_ollamaClient == null)
        {
            m_ollamaClient = await GetOllamaClientAsync(OllamaDefaultUrl);
        }

        return m_ollamaClient != null;
    }

    /// <inheritdoc/>
    protected override ValidationResult Validate(CommandContext context, ChatCommandSettings settings)
    {
        bool connected = settings.OllamaUrl is null
            ? Connect(OllamaDefaultUrl).Result
            : Connect(settings.OllamaUrl).Result;
        return connected ? ValidationResult.Success()
            : ValidationResult.Error($"Unable to connect to Ollama server: {settings.OllamaUrl ?? OllamaDefaultUrl}.");
    }

    private static void WriteSectionTitle(string title)
    {
        AnsiConsole.Write(new Rule($"[CornflowerBlue]{title}[/]")
            .LeftJustified()
            .HeavyBorder()
            .RuleStyle(Style.Parse("DeepSkyBlue4")));
    }

    private async Task<string?> SelectOllamaModel(CancellationToken cancellationToken)
    {
        var localModels = (await m_ollamaClient.ListLocalModelsAsync(cancellationToken))
            .Select(item => item.ModelName!).ToList();
        if (!localModels.Any())
        {
            return null;
        }

        var selectionPrompt = new SelectionPrompt<string>()
            .Title("Select the model to use:")
            .AddChoices(localModels)
            .WrapAround()
            .PageSize(PageSize)
            .MoreChoicesText(PageFooterText);
        var selectedModel = await AnsiConsole.PromptAsync(selectionPrompt, cancellationToken);
        m_ollamaClient.SelectedModel = selectedModel;
        return selectedModel;
    }

    /// <inheritdoc/>
    protected override async Task<int> ExecuteAsync(CommandContext context, ChatCommandSettings settings,
        CancellationToken cancellationToken)
    {
        // Connect to Ollama server
        WriteSectionTitle("Ollama Server");
        AnsiConsole.MarkupLine($"Connected to Ollama server: [DarkSeaGreen4 bold]{m_ollamaClient.Uri.ToString()}[/].");

        // Select the Ollama model to use
        WriteSectionTitle("Ollama Model");
        var selectedModel = await SelectOllamaModel(cancellationToken);
        if (selectedModel is null)
        {
            AnsiConsole.MarkupLine("[Red]⚠ [/] No model available.");
            return 0;
        }

        AnsiConsole.MarkupLine($"Selected model: [DarkSeaGreen4 bold]{selectedModel}[/]");

        // Setup first persona
        WriteSectionTitle("First Agent");
        var agent1 = await AIPersonaSetup.GenerateAIPersona("Agent 1", m_ollamaClient, cancellationToken);

        // Setup second persona
        WriteSectionTitle("Second Agent");
        var agent2 = await AIPersonaSetup.GenerateAIPersona("Agent 2", m_ollamaClient, cancellationToken);

        // Select starting text
        WriteSectionTitle("Starting Text");
        var selectionPrompt = new SelectionPrompt<string>()
            .Title("Select the starting text:")
            .AddChoices("Hello, how are you?",
                "What have you been up to?",
                "Hello there.",
                "Can I ask you something?",
                "Can you help with something?")
            .WrapAround()
            .PageSize(PageSize)
            .MoreChoicesText(PageFooterText);
        var startingText = await AnsiConsole.PromptAsync(selectionPrompt, cancellationToken);
        AnsiConsole.MarkupLine($"Starting Text: [DarkSeaGreen4 bold]{startingText}[/]");

        // Start conversation
        var conversation = new Conversation();
        await conversation.Initialize(agent1, agent2, startingText);
        WriteSectionTitle($"{agent1.Name}");
        AnsiConsole.WriteLine(startingText);
        
        Thread.Sleep(ConversationWaitTime);

        // Conversation loop
        ConsoleKeyInfo? key;
        do
        {
            WriteSectionTitle($"{conversation.CurrentPersonaName}");
            await conversation.GenerateNextOutput(AnsiConsole.Write);

            AnsiConsole.WriteLine();

            Thread.Sleep(ConversationWaitTime);

            key = AnsiConsole.Console.Input.IsKeyAvailable() ? await AnsiConsole.Console.Input.ReadKeyAsync(false, cancellationToken) : null;
        } while (key is not { Key: ConsoleKey.Escape });

        return 0;
    }
}