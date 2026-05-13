using System.Reflection;
using AutonomousAIChat;
using Spectre.Console;
using Spectre.Console.Cli;

AnsiConsole.Clear();

var version = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
AnsiConsole.MarkupLine($"[DeepSkyBlue4_1 bold]Autonomous AI Chat v{version}[/]");

var app = new CommandApp<ChatCommand>();

return app.Run(args);
