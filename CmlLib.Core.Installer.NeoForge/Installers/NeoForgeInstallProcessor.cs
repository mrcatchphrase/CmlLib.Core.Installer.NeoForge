﻿using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CmlLib.Core.Installer.NeoForge.Installers;

public class NeoForgeInstallProcessor(string javaPath)
{
    private readonly string _javaPath = javaPath;

    public async Task MapAndStartProcessors(
        string installerDir, 
        string vanillaJarPath,
        string libraryPath,
        JsonElement installProfile,
        IProgress<InstallerProgressChangedEventArgs>? progress,
        IProgress<string>? processorOutput)
    {
        Dictionary<string, string?> mapData;
        if (installProfile.TryGetProperty("data", out var dataProp))
            mapData = MapProcessorData(dataProp, vanillaJarPath, libraryPath, installerDir);
        else
            mapData = [];

        if (installProfile.TryGetProperty("processors", out var processorsProp))
            await StartProcessors(processorsProp, mapData, libraryPath, progress, processorOutput);
    }

    public Dictionary<string, string?> MapProcessorData(
        JsonElement data, 
        string minecraftJar, 
        string libraryPath, 
        string installDir)
    {
        var dataMapping = new Dictionary<string, string?>();
        foreach (var item in data.EnumerateObject())
        {
            var key = item.Name;
            var value = item.Value.GetPropertyValue("client");
            if (string.IsNullOrEmpty(value))
                continue;

            var fullPath = NeoForgeMapper.ToFullPath(value, libraryPath);
            if (fullPath == value)
            {
                value = value.Trim('/');
                dataMapping.Add(key, Path.Combine(installDir, value));
            }
            else
                dataMapping.Add(key, fullPath);
        }

        dataMapping.Add("SIDE", "client");
        dataMapping.Add("MINECRAFT_JAR", minecraftJar);
        dataMapping.Add("INSTALLER", Path.Combine(installDir, "installer.jar"));
        return dataMapping;
    }

    public async Task StartProcessors(
        JsonElement processors, 
        Dictionary<string, string?> mapData, 
        string libraryPath,
        IProgress<InstallerProgressChangedEventArgs>? fileProgress,
        IProgress<string>? processorOutput)
    {
        if (processors.ValueKind != JsonValueKind.Array)
            return;

        var count = processors.EnumerateArray().Count();
        var progressed = 0;
        foreach (var processor in processors.EnumerateArray())
        {
            var name = processor.GetPropertyValue("jar");
            fileProgress?.Report(new InstallerProgressChangedEventArgs(count, progressed, name, InstallerEventType.Queued));

            var isProcessed = 
                processor.TryGetProperty("outputs", out var outputs) &&
                CheckProcessorOutputs(outputs, mapData);

            var isClientSide =
                !processor.TryGetProperty("sides", out var sides) || CheckClientSides(sides);

            if (!isProcessed && isClientSide)
                await StartProcessor(processor, mapData, libraryPath, processorOutput);

            progressed++;
            fileProgress?.Report(new InstallerProgressChangedEventArgs(count, progressed, name, InstallerEventType.Done));
        }
    }

    private bool CheckProcessorOutputs(JsonElement outputs, Dictionary<string, string?> mapData)
    {
        if (outputs.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in outputs.EnumerateObject())
            {
                var key = NeoForgeMapper.Interpolation(prop.Name, mapData, true);
                var value = NeoForgeMapper.Interpolation(prop.Value.ToString(), mapData, true);

                if (!File.Exists(key) || !IOUtil.CheckSHA1(key, value))
                    return false;
            }
        }

        return true;
    }

    private bool CheckClientSides(JsonElement sides)
    {
        if (sides.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in sides.EnumerateArray())
            {
                if (item.GetString() == "server")
                    return false;
                else
                    break;
            }
        }

        return true;
    }

    private async Task StartProcessor(
        JsonElement processor, 
        Dictionary<string, string?> mapData, 
        string libraryPath,
        IProgress<string>? processorOutput)
    {
        string? name = null;
        if (processor.TryGetProperty("jar", out var jarProp) && jarProp.ValueKind == JsonValueKind.String)
            name = jarProp.GetString();

        if (string.IsNullOrEmpty(name))
            return;

        // jar
        var jarPath = Path.Combine(libraryPath, NeoForgePackageName.GetPath(name, Path.DirectorySeparatorChar));
        var jarFile = new JarFile(jarPath);
        var jarManifest = jarFile.GetManifest();

        // mainclass
        string? mainClass = null;
        var hasMainclass = jarManifest?.TryGetValue("Main-Class", out mainClass) ?? false;
        if (!hasMainclass || string.IsNullOrEmpty(mainClass))
            return;

        // classpath
        var classpath = new List<string>();
        if (processor.TryGetProperty("classpath", out var classpathProp) && 
            classpathProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var libName in classpathProp.EnumerateArray())
            {
                var libNameString = libName.GetString();
                if (string.IsNullOrEmpty(libNameString))
                    continue;

                var lib = Path.Combine(libraryPath,
                    NeoForgePackageName.GetPath(libNameString, Path.DirectorySeparatorChar));
                lib = lib.Replace("@jar", "");
                
                classpath.Add(lib);
            }
        }
        classpath.Add(jarPath);

        // arg
        IEnumerable<string> args = [];
        if (processor.TryGetProperty("args", out var argsProp) && 
            argsProp.ValueKind == JsonValueKind.Array)
        {
            var arrStrs = argsProp.EnumerateArray().Select(x => x.ToString());
            args = NeoForgeMapper.Map(arrStrs, mapData, libraryPath);
        }

        await StartJava(classpath, mainClass, args, processorOutput);
    }

    private async Task StartJava(
        IEnumerable<string> classpath, 
        string mainClass, 
        IEnumerable<string> args, 
        IProgress<string>? javaOutput)
    {
        var argBuilder = new StringBuilder();
        argBuilder.Append("-cp ");
        argBuilder.Append(IOUtil.CombinePath(classpath));
        argBuilder.Append(" ");
        argBuilder.Append(mainClass);
        foreach (var arg in args)
        {
            argBuilder.Append(" ");
            argBuilder.Append(arg);
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = _javaPath,
                Arguments = argBuilder.ToString(),
            }
        };

        var p = new ProcessWrapper(process);
        p.OutputReceived += (s, e) => javaOutput?.Report(e);
        p.StartWithEvents();
        await p.WaitForExitTaskAsync();
    }
}