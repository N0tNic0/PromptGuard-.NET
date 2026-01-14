using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PromptGuard.Core.IO;

public sealed class PromptGuardConfig
{
    public string PromptsRoot { get; init; } = "prompts";
    public string ToolDir { get; init; } = ".promptguard";

    public static PromptGuardConfig LoadFrom(string configPath)
    {
        if (!File.Exists(configPath))
            return new PromptGuardConfig();

        var yaml = File.ReadAllText(configPath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<PromptGuardConfig>(yaml) ?? new PromptGuardConfig();
    }
}
