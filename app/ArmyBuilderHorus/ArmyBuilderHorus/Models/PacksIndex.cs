namespace ArmyBuilderHorus.Models;

public sealed class PacksIndex
{
    public int format { get; set; }
    public string version { get; set; } = "";
    public string base_url { get; set; } = "";
    public List<PackEntry> files { get; set; } = new();
}
public sealed class PackEntry
{
    public string path { get; set; } = "";
    public long bytes { get; set; }
    public string sha256 { get; set; } = "";
}
