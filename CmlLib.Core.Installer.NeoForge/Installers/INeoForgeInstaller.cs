using CmlLib.Core.Installer.NeoForge.Versions;
using CmlLib.Core.Installers;

namespace CmlLib.Core.Installer.NeoForge;

public interface INeoForgeInstaller
{
    string VersionName { get; }
    NeoForgeVersion NeoForgeVersion { get; }
    Task Install(MinecraftPath path, IGameInstaller installer, NeoForgeInstallOptions options);
}