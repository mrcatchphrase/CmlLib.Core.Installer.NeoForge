using CmlLib.Core.Installer.NeoForge.Versions;
using CmlLib.Core.Installers;
using System.Threading.Tasks;

namespace CmlLib.Core.Installer.NeoForge.Installers;

public interface INeoForgeInstaller
{
    string VersionName { get; }
    NeoForgeVersion NeoForgeVersion { get; }
    Task Install(MinecraftPath path, IGameInstaller installer, NeoForgeInstallOptions options);
}