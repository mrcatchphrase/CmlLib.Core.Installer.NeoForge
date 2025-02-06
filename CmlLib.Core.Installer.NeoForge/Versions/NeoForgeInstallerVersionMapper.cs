using CmlLib.Core.Installer.NeoForge;
using CmlLib.Core.Installer.NeoForge.Installers;
using CmlLib.Core.Installer.NeoForge.Versions;

namespace CmlLib.Core.Installer.Forge.Versions;

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
