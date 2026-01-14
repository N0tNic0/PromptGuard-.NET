using PromptGuard.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("pg");

    config.AddCommand<ValidateCommand>("validate")
        .WithDescription("Validate a prompt YAML file or a directory containing prompts")
        .WithExample(new[] { "validate", "prompts" })
        .WithExample(new[] { "validate", "prompts/invoice.extractor/1.0.0.yaml" });

    config.AddCommand<DiffCommand>("diff")
        .WithDescription("Diff two versions of the same prompt")
        .WithExample(new[] { "diff", "invoice.extractor@1.0.0", "invoice.extractor@1.1.0" });

    config.AddCommand<InitCommand>("init")
        .WithDescription("Initialize PromptGuard in the current directory")
        .WithExample(new[] { "init" });

});

return app.Run(args);