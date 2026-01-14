using PromptGuard.Core.IO;
using PromptGuard.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PromptGuard.Cli.Commands;

public sealed class DiffCommand : Command<DiffCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Source prompt (name@version)")]
        [CommandArgument(0, "<FROM>")]
        public string From { get; init; } = default!;

        [Description("Target prompt (name@version)")]
        [CommandArgument(1, "<TO>")]
        public string To { get; init; } = default!;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken ct)
    {
        try
        {
            var fromRef = PromptRef.Parse(settings.From);
            var toRef = PromptRef.Parse(settings.To);

            if (fromRef.Name != toRef.Name)
                throw new InvalidOperationException("Cannot diff prompts with different names.");

            var resolver = new PromptResolver();

            var from = resolver.Resolve(fromRef);
            var to = resolver.Resolve(toRef);

            AnsiConsole.MarkupLine($"[bold]Prompt:[/] {fromRef.Name}\n");

            DiffTemplate(from.Template, to.Template);
            DiffParameters(from.Parameters, to.Parameters);
            DiffPolicy(from.Policy, to.Policy);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ {ex.Message}[/]");
            return 1;
        }
    }

    private static void DiffTemplate(string a, string b)
    {
        if (a == b)
            return;

        AnsiConsole.MarkupLine("[underline]Template[/]");

        var left = a.Split('\n');
        var right = b.Split('\n');

        var max = Math.Max(left.Length, right.Length);

        for (int i = 0; i < max; i++)
        {
            var l = i < left.Length ? left[i] : null;
            var r = i < right.Length ? right[i] : null;

            if (l == r) continue;

            if (l != null)
                AnsiConsole.MarkupLine($"[red]- {Markup.Escape(l)}[/]");

            if (r != null)
                AnsiConsole.MarkupLine($"[green]+ {Markup.Escape(r)}[/]");
        }

        AnsiConsole.WriteLine();
    }

    private static void DiffParameters(PromptParameters a, PromptParameters b)
    {
        if (Equals(a, b)) return;

        AnsiConsole.MarkupLine("[underline]Parameters[/]");

        DiffValue("temperature", a.Temperature, b.Temperature);
        DiffValue("max_tokens", a.MaxTokens, b.MaxTokens);

        AnsiConsole.WriteLine();
    }

    private static void DiffPolicy(PromptPolicy a, PromptPolicy b)
    {
        if (Equals(a, b)) return;

        AnsiConsole.MarkupLine("[underline]Policy[/]");

        DiffValue("require_json", a.RequireJson, b.RequireJson);
        DiffValue("max_output_tokens", a.MaxOutputTokens, b.MaxOutputTokens);

        DiffList("forbidden_phrases", a.ForbiddenPhrases, b.ForbiddenPhrases);

        AnsiConsole.WriteLine();
    }

    private static void DiffValue<T>(string name, T? a, T? b)
    {
        if (Equals(a, b)) return;

        if (a is not null)
            AnsiConsole.MarkupLine($"[red]- {name}: {a}[/]");

        if (b is not null)
            AnsiConsole.MarkupLine($"[green]+ {name}: {b}[/]");
    }

    private static void DiffList(string name, List<string> a, List<string> b)
    {
        var removed = a.Except(b);
        var added = b.Except(a);

        foreach (var r in removed)
            AnsiConsole.MarkupLine($"[red]- {name}: {r}[/]");

        foreach (var a2 in added)
            AnsiConsole.MarkupLine($"[green]+ {name}: {a2}[/]");
    }
}
