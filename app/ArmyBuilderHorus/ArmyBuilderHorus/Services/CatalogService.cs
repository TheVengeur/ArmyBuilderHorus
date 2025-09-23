using System.Text.Json;
using ArmyBuilderHorus.Models;
using Microsoft.Maui.Storage;

namespace ArmyBuilderHorus.Services;

public sealed class CatalogService
{
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public async Task<Catalog> LoadAsync()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "packs", "catalog.core.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("catalog.core.json introuvable — lance la mise à jour des packs.", path);
        var txt = await File.ReadAllTextAsync(path);
        var cat = JsonSerializer.Deserialize<Catalog>(txt, _json) ?? new Catalog();
        return cat;
    }
}
