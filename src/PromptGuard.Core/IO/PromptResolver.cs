using PromptGuard.Core.Models;

namespace PromptGuard.Core.IO;

public sealed class PromptResolver
{
    private readonly PromptLoader _loader = new();

    public PromptDefinition Resolve(PromptRef reference, string root = "prompts")
    {
        var path = Path.Combine(root, reference.Name, $"{reference.Version}.yaml");

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Prompt '{reference.Name}@{reference.Version}' not found at '{path}'");

        return _loader.LoadFromYamlFile(path);
    }
}
