namespace PromptGuard.Core.IO;

public sealed class PromptCatalog
{
    public IReadOnlyList<string> ListPromptNames(string promptsRootPath)
    {
        if (!Directory.Exists(promptsRootPath))
            return Array.Empty<string>();

        return Directory.GetDirectories(promptsRootPath)
            .Select(Path.GetFileName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Where(n => !n!.StartsWith("."))          
            .OrderBy(n => n)
            .ToList()!;
    }

    public IReadOnlyList<string> ListVersions(string promptsRootPath, string promptName)
    {
        var dir = Path.Combine(promptsRootPath, promptName);
        if (!Directory.Exists(dir))
            return Array.Empty<string>();

        return Directory.GetFiles(dir, "*.yaml", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToList()!;
    }

    public string GetPromptFilePath(string promptsRootPath, string promptName, string version)
        => Path.Combine(promptsRootPath, promptName, $"{version}.yaml");

    public void EnsurePromptDirectory(string promptsRootPath, string promptName)
    {
        Directory.CreateDirectory(Path.Combine(promptsRootPath, promptName));
    }
}
