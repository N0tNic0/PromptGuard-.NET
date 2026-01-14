namespace PromptGuard.Core.IO;

public sealed class PromptGuardRuntime
{
    public string RootDirectory { get; }
    public PromptGuardConfig Config { get; }

    public string PromptsRootPath => Path.Combine(RootDirectory, Config.PromptsRoot);

    private PromptGuardRuntime(string rootDirectory, PromptGuardConfig config)
    {
        RootDirectory = rootDirectory;
        Config = config;
    }

    public static PromptGuardRuntime Discover()
    {
        var cwd = Directory.GetCurrentDirectory();
        var root = FindRootDirectory(cwd);

        var configPath = Path.Combine(root, ".promptguard", "config.yaml");
        var config = PromptGuardConfig.LoadFrom(configPath);

        return new PromptGuardRuntime(root, config);
    }

    private static string FindRootDirectory(string startDir)
    {
        var dir = new DirectoryInfo(startDir);

        while (dir != null)
        {
            var toolConfig = Path.Combine(dir.FullName, ".promptguard", "config.yaml");
            var sln = Directory.GetFiles(dir.FullName, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();

            // Priorità: config PromptGuard
            if (File.Exists(toolConfig))
                return dir.FullName;

            // fallback: solution root
            if (sln != null)
                return dir.FullName;

            dir = dir.Parent;
        }

        // Ultimo fallback: cwd
        return startDir;
    }
}
