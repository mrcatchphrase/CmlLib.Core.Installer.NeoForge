using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.Installer.NeoForge;
using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;

namespace SampleForgeInstaller;

internal class AllInstaller
{
    MinecraftLauncher _launcher;
    NeoForgeInstaller _neoforge;
    ForgeInstaller _forge;

    public AllInstaller()
    {
        _launcher = new MinecraftLauncher(new MinecraftPath());
        _neoforge = new NeoForgeInstaller(_launcher);
        _forge = new ForgeInstaller(_launcher);
    }

    public async Task InstallAll()
    {

        var versions = new Dictionary<string, string>
        {
            { "21.1.65", "1.21.1" }
        };

        foreach (var version in versions.Reverse())
        {
            try
            {
                //await InstallAndLaunch(version.Value);
                await InstallAndLaunch(version.Value, version.Key);
            }
            catch(Exception ex)
            {
                throw;
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
                Console.ReadLine();
            }
        }
    }
    public async Task InstallAndLaunch(string mcVersion)
    {
        Console.WriteLine("Minecraft: " + mcVersion);
        var versionName = await _forge.Install(mcVersion, new ForgeInstallOptions
        {
            FileProgress = new SyncProgress<InstallerProgressChangedEventArgs>(fileChanged),
            ByteProgress = new SyncProgress<ByteProgress>(progressChanged),
            InstallerOutput = new SyncProgress<string>(logOutput),
            SkipIfAlreadyInstalled = false
        });
        var process = await _launcher.CreateProcessAsync(versionName, new MLaunchOption
        {
            Session = MSession.CreateOfflineSession("tester123")
        });

        var processUtil = new ProcessWrapper(process);
        processUtil.OutputReceived += (s, e) => logOutput(e);
        processUtil.StartWithEvents();
        await Task.WhenAny(Task.Delay(30000), processUtil.WaitForExitTaskAsync());
        if (processUtil.Process.HasExited)
            throw new Exception("Process was dead!");
        else
            processUtil.Process.Kill();
    }

    public async Task InstallAndLaunch(string mcVersion, string neoforgeVersion)
    {
        Console.WriteLine("Minecraft: " + mcVersion);
        var versionName = await _neoforge.Install(mcVersion, neoforgeVersion, new NeoForgeInstallOptions
        {
            FileProgress = new SyncProgress<InstallerProgressChangedEventArgs>(fileChanged),
            ByteProgress = new SyncProgress<ByteProgress>(progressChanged),
            InstallerOutput = new SyncProgress<string>(logOutput),
            SkipIfAlreadyInstalled = false
        });
        var process = await _launcher.CreateProcessAsync(versionName, new MLaunchOption
        {
            Session = MSession.CreateOfflineSession("tester123")
        });

        var processUtil = new ProcessWrapper(process);
        processUtil.OutputReceived += (s, e) => logOutput(e);
        processUtil.StartWithEvents();
        await Task.WhenAny(Task.Delay(30000), processUtil.WaitForExitTaskAsync());
        if (processUtil.Process.HasExited)
            throw new Exception("Process was dead!");
        else
            processUtil.Process.Kill();
    }

    private void logOutput(string e)
    {
        Console.WriteLine(e);
    }

    private void fileChanged(InstallerProgressChangedEventArgs e)
    {
        Console.WriteLine($"[{e.EventType}][{e.ProgressedTasks}/{e.TotalTasks}] {e.Name}");
    }

    private void progressChanged(ByteProgress e)
    {
        Console.WriteLine(e.ToRatio() * 100 + "%");
    }
}
