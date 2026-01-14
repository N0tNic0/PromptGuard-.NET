using PromptGuard.Core.IO;
using PromptGuard.Core.Validation;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PromptGuard.Cli.Commands;

public sealed class ValidateCommand : Command<ValidateCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Path to a .yaml file or a directory")]
        [CommandArgument(0, "<PATH>")]
        public string Path { get; init; } = default!;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var loader = new PromptLoader();
        var validator = new PromptValidator();

        var runtime = PromptGuardRuntime.Discover();
        var inputPath = ResolveInputPath(settings.Path, runtime);

        var files = ResolveFiles(inputPath);

        if (files.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No .yaml files found.[/]");
            return 0;
        }

        var anyErrors = false;

        foreach (var file in files)
        {
            AnsiConsole.MarkupLine($"\n[bold]Validating[/] {file}");

            try
            {
                var prompt = loader.LoadFromYamlFile(file);
                var result = validator.Validate(prompt);

                if (result.IsValid)
                    AnsiConsole.MarkupLine("[green]OK[/]");
                else
                {
                    anyErrors = true;
                    AnsiConsole.MarkupLine("[red]FAILED[/]");
                }

                foreach (var e in result.Errors)
                    AnsiConsole.MarkupLine($"  [red]✗[/] {e}");

                foreach (var w in result.Warnings)
                    AnsiConsole.MarkupLine($"  [yellow]![/] {w}");
            }
            catch (Exception ex)
            {
                anyErrors = true;
                AnsiConsole.MarkupLine($"  [red]✗ Exception:[/] {ex.Message}");
            }
        }

        return anyErrors ? 1 : 0;
    }

    private static string ResolveInputPath(string input, PromptGuardRuntime runtime)
    {
        var normalized = input.Replace('\\', '/').Trim();

        // caso standard: "prompts" -> usa config e root discovery
        if (normalized is "prompts" or "./prompts")
            return runtime.PromptsRootPath;

        // altrimenti rispetta il path passato (relativo alla cwd o assoluto)
        return input;
    }

    private static List<string> ResolveFiles(string path)
    {
        if (File.Exists(path))
            return new List<string> { path };

        if (Directory.Exists(path))
            return Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories).ToList();

        AnsiConsole.MarkupLine($"[red]Path not found:[/] {path}");
        return new List<string>();
    }
}