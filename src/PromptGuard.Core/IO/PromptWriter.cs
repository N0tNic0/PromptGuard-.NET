using PromptGuard.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PromptGuard.Core.IO;

public sealed class PromptWriter
{
    private readonly ISerializer _serializer;

    public PromptWriter()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();
    }

    public void WriteToFile(PromptDefinition prompt, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var yaml = _serializer.Serialize(prompt);
        File.WriteAllText(path, yaml);
    }
}
