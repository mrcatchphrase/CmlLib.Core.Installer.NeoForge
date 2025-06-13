﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CmlLib.Core.Installer.NeoForge;

public class NeoForgeMapper
{
    private static readonly Regex argBracket = new(@"\$?\{(.*?)}");

    public static IEnumerable<string> Map(IEnumerable<string> arg, IReadOnlyDictionary<string, string?> dicts, string prepath)
    {
        var checkPath = !string.IsNullOrEmpty(prepath);
        foreach (string item in arg)
        {
            var a = Interpolation(item, dicts, false);
            if (checkPath)
                a = ToFullPath(a, prepath);
            yield return HandleEmptyArg(a);
        }
    }

    public static string ToFullPath(string str, string prepath)
    {
        // [de.oceanlabs.mcp:mcp_config:1.16.2-20200812.004259@zip]
        // \libraries\de\oceanlabs\mcp\mcp_config\1.16.2-20200812.004259\mcp_config-1.16.2-20200812.004259.zip

        // [net.minecraft:client:1.16.2-20200812.004259:slim]
        // /libraries\net\minecraft\client\1.16.2-20200812.004259\client-1.16.2-20200812.004259-slim.jar

        if (str.StartsWith("[") && str.EndsWith("]") && !string.IsNullOrEmpty(prepath))
        {
            var innerStr = str.TrimStart('[').TrimEnd(']').Split('@');
            var pathName = innerStr[0];
            var extension = "jar";

            if (innerStr.Length > 1)
                extension = innerStr[1];

            return Path.Combine(prepath,
                NeoForgePackageName.GetPath(pathName, extension, Path.DirectorySeparatorChar));
        }
        else if (str.StartsWith("\'") && str.EndsWith("\'"))
            return str.Trim('\'');
        else
            return str;
    }

    public static string Interpolation(string str, IReadOnlyDictionary<string, string?> dicts, bool handleEmpty)
    {
        str = argBracket.Replace(str, (match =>
        {
            if (match.Groups.Count < 2)
                return match.Value;

            var key = match.Groups[1].Value;
            if (dicts.TryGetValue(key, out string? value))
            {
                value ??= "";

                return value;
            }

            return match.Value;
        }));

        if (handleEmpty)
            return HandleEmptyArg(str);
        else
            return str;
    }

    // key=value 1 => key="value 1"
    // key="va  l" => key="va  l"
    // va lue => "va lue"
    // "va lue" => "va lue"
    public static string HandleEmptyArg(string input)
    {
        if (input.Contains("="))
        {
            var s = input.Split('=');

            if (s[1].Contains(" ") && !CheckEmptyHandled(s[1]))
                return s[0] + "=\"" + s[1] + "\"";
            else
                return input;
        }
        else if (input.Contains(" ") && !CheckEmptyHandled(input))
            return "\"" + input + "\"";
        else
            return input;
    }

    static bool CheckEmptyHandled(string str)
    {
        return str.StartsWith("\"") || str.EndsWith("\"");
    }
}
