using PromptGuard.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PromptGuard.Core.IO;

public sealed class PromptLoader
{
    private readonly IDeserializer _deserializer;

    public PromptLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public PromptDefinition LoadFromYamlFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Prompt file not found", path);

        var yaml = File.ReadAllText(path);
        var prompt = _deserializer.Deserialize<PromptDefinition>(yaml);

        return prompt;
    }
}
