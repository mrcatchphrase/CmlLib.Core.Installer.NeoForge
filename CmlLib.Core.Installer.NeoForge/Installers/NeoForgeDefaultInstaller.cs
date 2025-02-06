using CmlLib.Core.Installer.NeoForge.Versions;
using CmlLib.Core.Installers;
using CmlLib.Utils;
using System.Text.Json;

namespace CmlLib.Core.Installer.NeoForge.Installers;

public class NeoForgeDefaultInstaller : INeoForgeInstaller
{
    public NeoForgeDefaultInstaller(string versionName, NeoForgeVersion neoforgeVersion)
    {
        VersionName = versionName;
        NeoForgeVersion = neoforgeVersion;
    }

    public string VersionName { get; }
    public NeoForgeVersion NeoForgeVersion { get; }

    public async Task Install(MinecraftPath path, IGameInstaller installer, NeoForgeInstallOptions options)
    {
        if (string.IsNullOrEmpty(options.JavaPath))
            throw new ArgumentNullException(nameof(options.JavaPath));
        var processor = new NeoForgeInstallProcessor(options.JavaPath);

        using var extractor = await NeoForgeInstallerExtractor.DownloadAndExtractInstaller(NeoForgeVersion, installer, options);
        using var installerProfileStream = extractor.OpenInstallerProfile();
        using var installerProfile = await JsonDocument.ParseAsync(installerProfileStream);

        await extractMavens(extractor.ExtractedDir, path);
        await installLibraries(installerProfile.RootElement, path, installer, options);
        await processor.MapAndStartProcessors(
            extractor.ExtractedDir, 
            path.GetVersionJarPath(NeoForgeVersion.MinecraftVersionName), 
            path.Library, 
            installerProfile.RootElement, 
            options.FileProgress,
            options.InstallerOutput);
        await copyVersionFiles(extractor.ExtractedDir, path);
    }

    private async Task extractMavens(string installerPath, MinecraftPath minecraftPath)
    {
        var org = Path.Combine(installerPath, "maven");
        if (Directory.Exists(org))
            await IOUtil.CopyDirectory(org, minecraftPath.Library);
    }

    private async Task installLibraries(
        JsonElement installerProfile,
        MinecraftPath path,
        IGameInstaller installer,
        NeoForgeInstallOptions options)
    {
        if (installerProfile.TryGetProperty("libraries", out var libraryProp) &&
            libraryProp.ValueKind == JsonValueKind.Array)
        {
            var libraryInstaller = new NeoForgeLibraryInstaller(installer, options.RulesContext, MojangServer.Library);
            await libraryInstaller.Install(
                path,
                libraryProp,
                options.FileProgress,
                options.ByteProgress,
                options.CancellationToken);
        }
    }

    private async Task copyVersionFiles(string installerDir, MinecraftPath minecraftPath)
    {
        var versionJsonSource = Path.Combine(installerDir, "version.json");
        var versionJsonDest = minecraftPath.GetVersionJsonPath(VersionName);
        IOUtil.CreateDirectoryForFile(versionJsonDest);
        await IOUtil.CopyFileAsync(versionJsonSource, versionJsonDest);

        var m = NeoForgeVersion.MinecraftVersionName;
        var f = NeoForgeVersion.NeoForgeVersionName;
        var jar = Path.Combine(installerDir, $"maven/net/neoforged/neoforge/{f}/neoforge-{f}.jar");
        if (File.Exists(jar)) //fix 1.17+ 
        {
            var jarPath = minecraftPath.GetVersionJarPath(VersionName);
            IOUtil.CreateDirectoryForFile(jarPath);
            await IOUtil.CopyFileAsync(jar, jarPath);
        }
    }
}
