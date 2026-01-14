using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Text;

namespace PromptGuard.Cli.Commands;

public sealed class InitCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken ct)
    {
        try
        {
            var cwd = Directory.GetCurrentDirectory();

            var promptsDir = Path.Combine(cwd, "prompts");
            var exampleDir = Path.Combine(promptsDir, "invoice.extractor");
            var exampleFile = Path.Combine(exampleDir, "1.0.0.yaml");

            var toolDir = Path.Combine(cwd, ".promptguard");
            var configFile = Path.Combine(toolDir, "config.yaml");

            Directory.CreateDirectory(promptsDir);
            Directory.CreateDirectory(exampleDir);
            Directory.CreateDirectory(toolDir);

            if (!File.Exists(configFile))
                File.WriteAllText(configFile, DefaultConfigYaml());

            if (!File.Exists(exampleFile))
                File.WriteAllText(exampleFile, DefaultExamplePromptYaml());

            AnsiConsole.MarkupLine("[green]✓ PromptGuard initialized[/]");
            AnsiConsole.MarkupLine($"  - prompts/ (created)");
            AnsiConsole.MarkupLine($"  - .promptguard/ (created)");

            AnsiConsole.MarkupLine("\nNext:");
            AnsiConsole.MarkupLine("  [bold]pg validate prompts[/]");
            AnsiConsole.MarkupLine("  [bold]pg diff invoice.extractor@1.0.0 invoice.extractor@1.1.0[/] (after you create 1.1.0)");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Init failed:[/] {ex.Message}");
            return 1;
        }
    }

    private static string DefaultConfigYaml() =>
"""
# PromptGuard configuration
prompts_root: prompts
tool_dir: .promptguard
""";

    private static string DefaultExamplePromptYaml() =>
"""
name: invoice.extractor
version: 1.0.0
model: gpt-4.1-mini
parameters:
  temperature: 0.1
  max_tokens: 600
template: |
  Sei un estrattore di dati da fatture.
  Estrai i campi: numero_fattura, data, imponibile, iva, totale.
  Rispondi SOLO in JSON valido, senza testo extra.

  TESTO:
  {{testo}}
variables:
  - testo
policy:
  require_json: true
  max_output_tokens: 500
  forbidden_phrases:
    - "Certo!"
    - "Ecco qui"
""";
}
