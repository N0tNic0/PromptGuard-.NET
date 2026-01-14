namespace PromptGuard.Core.IO;

public sealed record PromptRef(string Name, string Version)
{
    public static PromptRef Parse(string value)
    {
        var parts = value.Split('@', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            throw new ArgumentException($"Invalid prompt reference '{value}'. Expected format: name@version");

        return new PromptRef(parts[0], parts[1]);
    }
}