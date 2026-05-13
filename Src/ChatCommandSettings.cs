using System.ComponentModel;
using Spectre.Console.Cli;

namespace AutonomousAIChat;

/// <summary>
/// Defines settings for the main (chat) command.
/// </summary>
internal sealed class ChatCommandSettings : CommandSettings
{
    [CommandOption("-u|--url")]
    [Description("The url of the Ollama server.")]
    public string? OllamaUrl { get; init; }
}
