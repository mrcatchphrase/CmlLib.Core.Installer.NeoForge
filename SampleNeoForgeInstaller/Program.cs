using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.NeoForge;
using CmlLib.Core.Installer.NeoForge.Installers;
using CmlLib.Core.Installer.NeoForge.Versions;
using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;

var path = new MinecraftPath(); // use default directory
var launcher = new MinecraftLauncher(path);

// show launch progress to console
var fileProgress = new SyncProgress<InstallerProgressChangedEventArgs>(e =>
    Console.WriteLine($"[{e.EventType}][{e.ProgressedTasks}/{e.TotalTasks}] {e.Name}"));
var byteProgress = new SyncProgress<ByteProgress>(e =>
    Console.WriteLine(e.ToRatio() * 100 + "%"));
var installerOutput = new SyncProgress<string>(e =>
    Console.WriteLine(e));

//Initialize NeoForgeInstaller
var neoForge = new NeoForgeInstaller(launcher);
var loader = new NeoForgeVersionLoader(new());

var versions = await loader.GetNeoForgeVersionsAsync();
var version = versions.First(x => x.MinecraftVersionName == "1.20.4" && x.NeoForgeVersionName == "20.4.248");
//OR var version = versions.Last(); // latest NeoForgeVersion
var version_name = await neoForge.Install(version, new NeoForgeInstallOptions
{
    FileProgress = fileProgress,
    ByteProgress = byteProgress,
    InstallerOutput = installerOutput,
});
//var version_name = await neoForge.Install(version.MinecraftVersionName); // install the recommended neoforge version for mcVersion
//OR var version_name = await neoForge.Install(version.MinecraftVersionName, version.NeoForgeVersionName);

//Start Minecraft
var launchOption = new MLaunchOption
{
    MaximumRamMb = 2048,
    Session = MSession.CreateOfflineSession("GodAlphaBs"),
};

var process = await launcher.CreateProcessAsync(version_name, launchOption);

// print game logs
var processUtil = new ProcessWrapper(process);
processUtil.OutputReceived += (s, e) => Console.WriteLine(e);
processUtil.StartWithEvents();
await processUtil.WaitForExitTaskAsync();