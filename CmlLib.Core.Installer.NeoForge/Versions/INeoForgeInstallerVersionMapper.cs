namespace CmlLib.Core.Installer.NeoForge.Versions;

public interface INeoForgeInstallerVersionMapper
{
    INeoForgeInstaller CreateInstaller(NeoForgeVersion version);
}
