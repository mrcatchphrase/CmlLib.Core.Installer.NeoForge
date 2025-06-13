﻿using CmlLib.Core.FileExtractors;
using CmlLib.Core.Files;
using CmlLib.Core.Installers;
using CmlLib.Core.Rules;
using CmlLib.Core.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CmlLib.Core.Installer.NeoForge.Installers;

public class NeoForgeLibraryInstaller(
    IGameInstaller installer,
    RulesEvaluatorContext context,
    string libraryServer)
{
    private readonly RulesEvaluatorContext _rulesContext = context;
    private readonly IGameInstaller _installer = installer;
    private readonly string _libraryServer = libraryServer;

    public async Task Install(
        MinecraftPath path, 
        JsonElement element,
        IProgress<InstallerProgressChangedEventArgs>? fileProgress,
        IProgress<ByteProgress>? byteProgress,
        CancellationToken cancellationToken)
    {
        var libs = ExtractLibraries(element);
        var files = ExtractGameFile(path, libs);
        await _installer.Install(files, fileProgress, byteProgress, cancellationToken);
    }

    public IEnumerable<MLibrary> ExtractLibraries(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var item in element.EnumerateArray())
        {
            var lib = JsonLibraryParser.Parse(item);
            if (lib != null)
                yield return lib;
        }
    }

    public IEnumerable<GameFile> ExtractGameFile(MinecraftPath path, IEnumerable<MLibrary> libraries)
    {
        return libraries.SelectMany(library => 
            LibraryFileExtractor.Extractor.ExtractTasks(_libraryServer, path, library, _rulesContext));
    }
}