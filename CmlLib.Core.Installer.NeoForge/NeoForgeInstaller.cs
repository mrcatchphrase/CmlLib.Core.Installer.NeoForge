using CmlLib.Core.Installer.NeoForge.Installers;
using CmlLib.Core.Installer.NeoForge.Versions;
using CmlLib.Core.Installers;
using CmlLib.Core.Version;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CmlLib.Core.Installer.NeoForge;

public class NeoForgeInstaller(MinecraftLauncher launcher)
{
    private readonly MinecraftLauncher _launcher = launcher;
    private readonly INeoForgeInstallerVersionMapper _installerMapper = new NeoForgeInstallerVersionMapper();
    private readonly NeoForgeVersionLoader _versionLoader = new(new HttpClient());

    public Task<string> Install(string mcVersion, string neoForgeVersion) =>
        Install(mcVersion, neoForgeVersion, new NeoForgeInstallOptions());

    public async Task<string> Install(
        string mcVersion,
        string neoForgeVersion,
        NeoForgeInstallOptions options)
    {
        var version = _versionLoader.GetNeoForgeVersionFile(mcVersion, neoForgeVersion);

        return await Install(version, options);
    }

    public async Task<string> Install(
        NeoForgeVersion neoForgeVersion,
        NeoForgeInstallOptions options)
    {
        neoForgeVersion = _versionLoader.GetNeoForgeVersionFile(neoForgeVersion.MinecraftVersionName, neoForgeVersion.NeoForgeVersionName);
        var installer = _installerMapper.CreateInstaller(neoForgeVersion);
        if (options.SkipIfAlreadyInstalled && await CheckVersionInstalled(installer.VersionName))
            return installer.VersionName;

        var version = await CheckAndDownloadVanillaVersion(
            neoForgeVersion.MinecraftVersionName,
            options.FileProgress,
            options.ByteProgress);

        if (string.IsNullOrEmpty(options.JavaPath))
            options.JavaPath = GetJavaPath(version);

        await installer.Install(_launcher.MinecraftPath, _launcher.GameInstaller, options);
        await _launcher.GetAllVersionsAsync();
        return installer.VersionName;
    }

    private async Task<IVersion> CheckAndDownloadVanillaVersion(
        string mcVersion,
        IProgress<InstallerProgressChangedEventArgs>? fileProgress,
        IProgress<ByteProgress>? byteProgress)
    {
        var version = await _launcher.GetVersionAsync(mcVersion);
        await _launcher.InstallAsync(version, fileProgress, byteProgress);
        return version;
    }

    private async Task<bool> CheckVersionInstalled(string versionName)
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

    private string GetJavaPath(IVersion version)
    {
        var javaPath = _launcher.GetJavaPath(version);
        if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            javaPath = _launcher.GetDefaultJavaPath();
        if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            throw new InvalidOperationException("Cannot find any java binary. Set java binary path");

        return javaPath;
    }
}