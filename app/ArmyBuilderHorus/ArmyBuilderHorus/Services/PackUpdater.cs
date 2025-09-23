using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using ArmyBuilderHorus.Models;
using ArmyBuilderHorus.Services.Crypto;
using Microsoft.Maui.Storage;

namespace ArmyBuilderHorus.Services;

public sealed class PackUpdater
{
    private readonly HttpClient _http = new();
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private readonly string _cacheDir;

    public PackUpdater()
    {
        _cacheDir = Path.Combine(FileSystem.AppDataDirectory, "packs");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task<string> UpdateAsync(string indexUrl, string passphrase, CancellationToken ct = default)
    {
        using var resp = await _http.GetAsync(indexUrl, ct);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"GET {indexUrl} => {(int)resp.StatusCode} {resp.ReasonPhrase}");
        var idx = await resp.Content.ReadFromJsonAsync<PacksIndex>(_json, ct)
                  ?? throw new InvalidDataException("Invalid index");

        foreach (var f in idx.files)
        {
            var url = idx.base_url.TrimEnd('/') + "/" + f.path;
            using var r = await _http.GetAsync(url, ct);
            if (!r.IsSuccessStatusCode)
                throw new HttpRequestException($"GET {url} => {(int)r.StatusCode} {r.ReasonPhrase}");

            var blob = await r.Content.ReadAsByteArrayAsync(ct);

            if (blob.LongLength != f.bytes) throw new InvalidDataException($"Size mismatch {f.path}");
            var sha = Convert.ToHexString(SHA256.HashData(blob)).ToLowerInvariant();
            if (sha != f.sha256.ToLowerInvariant()) throw new InvalidDataException($"SHA mismatch {f.path}");

            try
            {
                var plain = AesGcmService.Decrypt(blob, passphrase);

                var basename = Path.GetFileName(f.path).Replace(".enc", "");
                var outFile = Path.Combine(_cacheDir, basename);
                var tmp = outFile + ".tmp";
                await File.WriteAllBytesAsync(tmp, plain, ct);
                File.Move(tmp, outFile, true);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Decrypt failed for {f.path}: {ex.Message}");
            }
        }
        return idx.version;
    }
}
