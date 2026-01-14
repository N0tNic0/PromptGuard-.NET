using PromptGuard.Core.IO;
using PromptGuard.Core.Models;
using PromptGuard.Core.Validation;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PromptGuard.Cli.Commands;

public sealed class UiCommand : Command<UiCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--root <PROMPTS_ROOT>")]
        [Description("Override prompts root directory (absolute or relative path).")]
        public string? Root { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken ct)
    {
        var runtime = PromptGuardRuntime.Discover();
        var promptsRoot = ResolvePromptsRoot(settings.Root, runtime);

        var catalog = new PromptCatalog();
        var loader = new PromptLoader();
        var writer = new PromptWriter();
        var validator = new PromptValidator();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("PromptGuard").LeftJustified());
            AnsiConsole.MarkupLine($"[grey]Prompts root:[/] [bold]{promptsRoot}[/]\n");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an action")
                    .AddChoices(new[]
                    {
                        "Browse prompts",
                        "Create new prompt (1.0.0)",
                        "Validate all",
                        "Change prompts root",
                        "Exit"
                    }));

            switch (choice)
            {
                case "Browse prompts":
                    BrowsePrompts(promptsRoot, catalog, loader, writer, validator);
                    break;

                case "Create new prompt (1.0.0)":
                    CreateNewPrompt(promptsRoot, catalog, writer, validator);
                    break;

                case "Validate all":
                    ValidateAll(promptsRoot, catalog, loader, validator);
                    break;

                case "Change prompts root":
                    promptsRoot = AskPromptsRoot(promptsRoot);
                    break;

                case "Exit":
                    return 0;
            }
        }
    }

    private static string ResolvePromptsRoot(string? rootOption, PromptGuardRuntime runtime)
    {
        if (!string.IsNullOrWhiteSpace(rootOption))
            return Path.GetFullPath(rootOption);

        // default da config
        return runtime.PromptsRootPath;
    }

    private static string AskPromptsRoot(string current)
    {
        var path = AnsiConsole.Ask<string>($"Enter prompts root path ([grey]current: {current}[/])");
        return Path.GetFullPath(path);
    }

    private static void BrowsePrompts(
        string promptsRoot,
        PromptCatalog catalog,
        PromptLoader loader,
        PromptWriter writer,
        PromptValidator validator)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]Browse prompts[/]\n");

            var names = catalog.ListPromptNames(promptsRoot).ToList();
            if (names.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No prompts found.[/]");
                AnsiConsole.MarkupLine("[grey]Press any key to go back...[/]");
                Console.ReadKey(true);
                return;
            }

            names.Insert(0, "< Back");

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a prompt")
                    .PageSize(15)
                    .AddChoices(names));

            if (selected == "< Back")
                return;

            BrowseVersions(promptsRoot, selected, catalog, loader, writer, validator);
        }
    }

    private static void BrowseVersions(
        string promptsRoot,
        string promptName,
        PromptCatalog catalog,
        PromptLoader loader,
        PromptWriter writer,
        PromptValidator validator)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold]Prompt:[/] {promptName}\n");

            var versions = catalog.ListVersions(promptsRoot, promptName).ToList();
            var menu = new List<string> { "< Back", "Create new version (clone & bump)" };
            menu.AddRange(versions.Select(v => $"View {v}"));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an action")
                    .PageSize(15)
                    .AddChoices(menu));

            if (choice == "< Back")
                return;

            if (choice == "Create new version (clone & bump)")
            {
                CreateNewVersion(promptsRoot, promptName, catalog, loader, writer, validator);
                continue;
            }

            if (choice.StartsWith("View "))
            {
                var version = choice.Substring("View ".Length).Trim();
                ViewPromptFile(promptsRoot, promptName, version, catalog);
            }
        }
    }

    private static void ViewPromptFile(string promptsRoot, string promptName, string version, PromptCatalog catalog)
    {
        var path = catalog.GetPromptFilePath(promptsRoot, promptName, version);
        if (!File.Exists(path))
        {
            AnsiConsole.MarkupLine($"[red]File not found:[/] {path}");
            Console.ReadKey(true);
            return;
        }

        var content = File.ReadAllText(path);

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]{promptName}@{version}[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Panel(content)
            .Header("YAML", Justify.Left)
            .Border(BoxBorder.Rounded));

        AnsiConsole.MarkupLine("\n[grey]Press any key to go back...[/]");
        Console.ReadKey(true);
    }

    private static void CreateNewPrompt(
        string promptsRoot,
        PromptCatalog catalog,
        PromptWriter writer,
        PromptValidator validator)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold]Create new prompt[/]\n");

        var name = AnsiConsole.Ask<string>("Prompt name (folder name, e.g. `my_prompt`):").Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            AnsiConsole.MarkupLine("[red]Invalid name.[/]");
            Console.ReadKey(true);
            return;
        }

        var version = "1.0.0";

        var prompt = new PromptDefinition
        {
            Name = name,
            Version = version,
            Model = "gpt-4.1-mini",
            Parameters = new PromptParameters { Temperature = 0.1, MaxTokens = 600 },
            Template = "Write your prompt here.\n\nINPUT:\n{{input}}\n",
            Variables = new List<string> { "input" },
            Policy = new PromptPolicy
            {
                RequireJson = false,
                MaxOutputTokens = 500,
                ForbiddenPhrases = new List<string>()
            }
        };

        var result = validator.Validate(prompt);
        if (!result.IsValid)
        {
            AnsiConsole.MarkupLine("[red]Cannot create prompt due to validation errors:[/]");
            foreach (var e in result.Errors)
                AnsiConsole.MarkupLine($"  [red]✗[/] {e}");
            Console.ReadKey(true);
            return;
        }

        catalog.EnsurePromptDirectory(promptsRoot, name);
        var path = catalog.GetPromptFilePath(promptsRoot, name, version);
        writer.WriteToFile(prompt, path);

        AnsiConsole.MarkupLine($"[green]✓ Created[/] {name}@{version}");
        AnsiConsole.MarkupLine($"[grey]{path}[/]");
        AnsiConsole.MarkupLine("\n[grey]Press any key...[/]");
        Console.ReadKey(true);
    }

    private static void CreateNewVersion(
        string promptsRoot,
        string promptName,
        PromptCatalog catalog,
        PromptLoader loader,
        PromptWriter writer,
        PromptValidator validator)
    {
        var versions = catalog.ListVersions(promptsRoot, promptName).ToList();
        if (versions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No versions found to clone.[/]");
            Console.ReadKey(true);
            return;
        }

        var fromVersion = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Clone from version")
                .AddChoices(versions));

        var bump = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select bump type")
                .AddChoices(new[] { "patch", "minor", "major" }));

        var toVersion = BumpSemVer(fromVersion, bump);

        var fromPath = catalog.GetPromptFilePath(promptsRoot, promptName, fromVersion);
        var toPath = catalog.GetPromptFilePath(promptsRoot, promptName, toVersion);

        if (File.Exists(toPath))
        {
            AnsiConsole.MarkupLine($"[red]Target version already exists:[/] {toVersion}");
            Console.ReadKey(true);
            return;
        }

        var prompt = loader.LoadFromYamlFile(fromPath);

        prompt.Version = toVersion;
        prompt.Name = promptName;

        var result = validator.Validate(prompt);
        if (!result.IsValid)
        {
            AnsiConsole.MarkupLine("[red]Cannot create version due to validation errors:[/]");
            foreach (var e in result.Errors)
                AnsiConsole.MarkupLine($"  [red]✗[/] {e}");
            Console.ReadKey(true);
            return;
        }

        writer.WriteToFile(prompt, toPath);

        AnsiConsole.MarkupLine($"[green]✓ Created[/] {promptName}@{toVersion} (from {fromVersion})");
        AnsiConsole.MarkupLine($"[grey]{toPath}[/]");
        AnsiConsole.MarkupLine("\n[grey]Press any key...[/]");
        Console.ReadKey(true);
    }

    private static void ValidateAll(
        string promptsRoot,
        PromptCatalog catalog,
        PromptLoader loader,
        PromptValidator validator)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold]Validate all prompts[/]\n");

        var promptNames = catalog.ListPromptNames(promptsRoot);
        if (promptNames.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No prompts found.[/]");
            Console.ReadKey(true);
            return;
        }

        var anyErrors = false;

        foreach (var name in promptNames)
        {
            var versions = catalog.ListVersions(promptsRoot, name);
            foreach (var v in versions)
            {
                var path = catalog.GetPromptFilePath(promptsRoot, name, v);
                AnsiConsole.MarkupLine($"[bold]Validating[/] {name}@{v}");

                try
                {
                    var prompt = loader.LoadFromYamlFile(path);
                    var result = validator.Validate(prompt);

                    if (result.IsValid)
                        AnsiConsole.MarkupLine("[green]OK[/]");
                    else
                    {
                        anyErrors = true;
                        AnsiConsole.MarkupLine("[red]FAILED[/]");
                        foreach (var e in result.Errors)
                            AnsiConsole.MarkupLine($"  [red]✗[/] {e}");
                    }
                }
                catch (Exception ex)
                {
                    anyErrors = true;
                    AnsiConsole.MarkupLine($"[red]✗ Exception:[/] {ex.Message}");
                }

                AnsiConsole.WriteLine();
            }
        }

        AnsiConsole.MarkupLine(anyErrors ? "[red]Validation completed with errors.[/]" : "[green]All prompts valid.[/]");
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(true);
    }

    private static string BumpSemVer(string version, string bump)
    {
        // minimal semver: X.Y.Z
        var parts = version.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor) ||
            !int.TryParse(parts[2], out var patch))
        {
            // fallback: append -copy
            return version + "-copy";
        }

        return bump switch
        {
            "major" => $"{major + 1}.0.0",
            "minor" => $"{major}.{minor + 1}.0",
            _ => $"{major}.{minor}.{patch + 1}"
        };
    }
}
