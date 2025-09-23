using System.Text.Json;
using Microsoft.Maui.Storage;

namespace ArmyBuilderHorus.Services;

public class ArmyListMeta
{
    public string name { get; set; } = "";
    public int points { get; set; }
    public string armyId { get; set; } = "";
    public string armyName { get; set; } = "";
    public string focId { get; set; } = "";
    public string? riteId { get; set; }
    public DateTime createdAt { get; set; }
    public string filePath { get; set; } = "";
    public string allegiance { get; set; } = "LOYALIST";
    public string? legionId { get; set; } = null;
    public bool isAlliedDetachment { get; set; } = false;
    public string? primaryArmyId { get; set; } = null;
    public string? primaryLegionId { get; set; } = null;
}

public sealed class ArmyListItem
{
    public string unitId { get; set; } = "";
    public int qty { get; set; } // for non-squad units; squads => 1
    public int @base { get; set; }

    public int? size { get; set; }                           // squad size
    public Dictionary<string, string>? choices { get; set; } // groupId -> choiceId
    public Dictionary<string, Dictionary<string, int>>? counts { get; set; }
}

public sealed class ArmyListFull : ArmyListMeta
{
    public List<ArmyListItem> items { get; set; } = new();
}

public sealed class ListStore
{
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private string Dir => Path.Combine(FileSystem.AppDataDirectory, "lists");

    public async Task<List<ArmyListMeta>> GetAllAsync()
    {
        Directory.CreateDirectory(Dir);
        var items = new List<ArmyListMeta>();
        foreach (var f in Directory.GetFiles(Dir, "*.json"))
        {
            try
            {
                var txt = await File.ReadAllTextAsync(f);
                var meta = JsonSerializer.Deserialize<ArmyListMeta>(txt, _json) ?? new ArmyListMeta();
                meta.filePath = f;
                items.Add(meta);
            }
            catch { /* skip bad file */ }
        }
        return items.OrderByDescending(x => x.createdAt).ToList();
    }

    public async Task<ArmyListMeta?> GetAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var txt = await File.ReadAllTextAsync(filePath);
        var meta = JsonSerializer.Deserialize<ArmyListMeta>(txt, _json) ?? new ArmyListMeta();
        meta.filePath = filePath;
        return meta;
    }

    public async Task<ArmyListFull?> LoadFullAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var txt = await File.ReadAllTextAsync(filePath);
        var full = JsonSerializer.Deserialize<ArmyListFull>(txt, _json);
        if (full != null) full.filePath = filePath;
        return full;
    }

    public async Task SaveFullAsync(string filePath, ArmyListFull full)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var json = JsonSerializer.Serialize(full, _json);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task RenameAsync(string filePath, string newName)
    {
        // essaie de charger la version "full"
        var full = await LoadFullAsync(filePath);
        if (full != null)
        {
            full.name = newName;
            await SaveFullAsync(filePath, full);
            return;
        }

        // fallback: charge juste le meta, puis ré-écris
        if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
        var txt = await File.ReadAllTextAsync(filePath);
        var meta = JsonSerializer.Deserialize<ArmyListMeta>(txt, _json) ?? new ArmyListMeta();
        meta.name = newName;
        var json = JsonSerializer.Serialize(meta, _json);
        await File.WriteAllTextAsync(filePath, json);
    }

    public Task DeleteAsync(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.CompletedTask;
    }
}
