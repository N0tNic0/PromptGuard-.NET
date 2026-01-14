namespace PromptGuard.Core.Models;

public sealed class PromptDefinition
{
    public string Name { get; init; } = default!;
    public string Version { get; init; } = default!;
    public string? Model { get; init; }

    public PromptParameters Parameters { get; init; } = new();
    public string Template { get; init; } = default!;

    public List<string> Variables { get; init; } = new();
    public PromptPolicy Policy { get; init; } = new();
}

public sealed class PromptParameters
{
    public double? Temperature { get; init; }
    public int? MaxTokens { get; init; }
}

public sealed class PromptPolicy
{
    public bool RequireJson { get; init; }
    public int? MaxOutputTokens { get; init; }
    public List<string> ForbiddenPhrases { get; init; } = new();
}