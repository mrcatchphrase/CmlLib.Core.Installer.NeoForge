using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CmlLib.Core.Installer.NeoForge.Versions;

public class NeoForgeVersionLoader(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<NeoForgeVersion>> GetNeoForgeVersionsAsync()
    {
        var json = await _httpClient.GetStringAsync("https://maven.neoforged.net/api/maven/versions/releases/net/neoforged/neoforge");
        return [.. JsonDocument.Parse(json).RootElement.GetProperty("versions")
            .EnumerateArray()
            .Select(v => v.GetString()!)
            .Select(v => new NeoForgeVersion(
                v.Contains('.') ? $"1.{v.Split('.')[0]}.{v.Split('.')[1]}" : $"1.{v}", v))];
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
