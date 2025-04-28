using CmlLib.Core.Installer.NeoForge.Installers;

namespace CmlLib.Core.Installer.NeoForge.Versions;

public interface INeoForgeInstallerVersionMapper
{
    INeoForgeInstaller CreateInstaller(NeoForgeVersion version);
}
