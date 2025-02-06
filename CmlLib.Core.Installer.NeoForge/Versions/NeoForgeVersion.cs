namespace CmlLib.Core.Installer.NeoForge.Versions;

public class NeoForgeVersion
{
    public NeoForgeVersion(string mcVersion, string neoforgeVersion)
    {
        this.MinecraftVersionName = mcVersion;
        this.NeoForgeVersionName = neoforgeVersion;
    }

    public string MinecraftVersionName { get; }
    public string NeoForgeVersionName { get; }
    public NeoForgeVersionFile File { get; set; }

    public NeoForgeVersionFile? GetInstallerFile() => File;
}
