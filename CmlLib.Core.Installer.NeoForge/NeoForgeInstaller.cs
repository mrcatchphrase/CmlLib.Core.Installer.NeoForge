using CmlLib.Core.Installer.Forge.Versions;
using CmlLib.Core.Installer.NeoForge.Versions;
using CmlLib.Core.Installers;
using CmlLib.Core.Version;
using System.Diagnostics;

namespace CmlLib.Core.Installer.NeoForge;

public class NeoForgeInstaller
{
    private readonly MinecraftLauncher _launcher;
    private readonly INeoForgeInstallerVersionMapper _installerMapper;
    private readonly NeoForgeVersionLoader _versionLoader;

    public NeoForgeInstaller(MinecraftLauncher launcher)
    {
        _installerMapper = new NeoForgeInstallerVersionMapper();
        _versionLoader = new NeoForgeVersionLoader(new HttpClient());
        _launcher = launcher;
    }

    public Task<string> Install(string mcVersion, string forgeVersion) =>
        Install(mcVersion, forgeVersion, new NeoForgeInstallOptions());

    public async Task<string> Install(
        string mcVersion,
        string forgeVersion,
        NeoForgeInstallOptions options)
    {
        var version = _versionLoader.GetNeoForgeVersionFile(mcVersion, forgeVersion);

        return await Install(version, options);
    }

    public async Task<string> Install(
        NeoForgeVersion forgeVersion,
        NeoForgeInstallOptions options)
    {
        var installer = _installerMapper.CreateInstaller(forgeVersion);
        if (options.SkipIfAlreadyInstalled && await checkVersionInstalled(installer.VersionName))
            return installer.VersionName;

        var version = await checkAndDownloadVanillaVersion(
            forgeVersion.MinecraftVersionName,
            options.FileProgress,
            options.ByteProgress);

        if (string.IsNullOrEmpty(options.JavaPath))
            options.JavaPath = getJavaPath(version);

        await installer.Install(_launcher.MinecraftPath, _launcher.GameInstaller, options);
        await _launcher.GetAllVersionsAsync();
        return installer.VersionName;
    }

    private async Task<IVersion> checkAndDownloadVanillaVersion(
        string mcVersion,
        IProgress<InstallerProgressChangedEventArgs>? fileProgress,
        IProgress<ByteProgress>? byteProgress)
    {
        var version = await _launcher.GetVersionAsync(mcVersion);
        await _launcher.InstallAsync(version, fileProgress, byteProgress);
        return version;
    }

    private async Task<bool> checkVersionInstalled(string versionName)
    {
        try
        {
            await _launcher.GetVersionAsync(versionName);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    private string getJavaPath(IVersion version)
    {
        var javaPath = _launcher.GetJavaPath(version);
        if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            javaPath = _launcher.GetDefaultJavaPath();
        if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            throw new InvalidOperationException("Cannot find any java binary. Set java binary path");

        return javaPath;
    }
}
