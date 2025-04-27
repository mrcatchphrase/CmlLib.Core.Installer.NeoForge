namespace CmlLib.Core.Installer.NeoForge.Versions;

public class NeoForgeVersion(string mcVersion, string neoforgeVersion)
{
    public string MinecraftVersionName { get; } = mcVersion;
    public string NeoForgeVersionName { get; } = neoforgeVersion;
    public NeoForgeVersionFile? File { get; set; }

    public NeoForgeVersionFile? GetInstallerFile() => File;
}
