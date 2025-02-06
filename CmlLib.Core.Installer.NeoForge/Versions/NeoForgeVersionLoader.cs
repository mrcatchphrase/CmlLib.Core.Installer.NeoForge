using CmlLib.Core.Installer.NeoForge.Versions;
using HtmlAgilityPack;

namespace CmlLib.Core.Installer.NeoForge.Versions;

public class NeoForgeVersionLoader
{
    private readonly HttpClient _httpClient;

    public NeoForgeVersionLoader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public NeoForgeVersion GetNeoForgeVersionFile(string mcVersion, string neoForgeVersion)
    {
        return new NeoForgeVersion(mcVersion, neoForgeVersion)
        {            
            File = new NeoForgeVersionFile
            {
                DirectUrl = $"https://maven.neoforged.net/releases/net/neoforged/neoforge/{neoForgeVersion}/neoforge-{neoForgeVersion}-installer.jar",
                Type = "Installer",
                MD5 = $"https://maven.neoforged.net/releases/net/neoforged/neoforge/{neoForgeVersion}/neoforge-{neoForgeVersion}-installer.jar.md5",
                SHA1 = $"https://maven.neoforged.net/releases/net/neoforged/neoforge/{neoForgeVersion}/neoforge-{neoForgeVersion}-installer.jar.sha1"
            }
            
        };
    }
}
