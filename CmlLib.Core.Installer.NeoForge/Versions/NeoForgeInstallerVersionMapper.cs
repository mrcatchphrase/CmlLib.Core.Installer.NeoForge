using CmlLib.Core.Installer.NeoForge.Installers;

namespace CmlLib.Core.Installer.NeoForge.Versions;

public class NeoForgeInstallerVersionMapper : INeoForgeInstallerVersionMapper
{
    public INeoForgeInstaller CreateDefaultInstaller(string versionName, NeoForgeVersion version) =>
        new NeoForgeDefaultInstaller(versionName, version);

    public INeoForgeInstaller CreateInstaller(NeoForgeVersion version)
    {
        var f = version.NeoForgeVersionName;
        return CreateDefaultInstaller($"neoforge-{f}", version);
    }
}