namespace ArmyBuilderHorus.Models;

public sealed class Condition
{
    public string? army_is { get; set; }
    public string? allegiance_is { get; set; }
    public string? legion_is { get; set; }              // Légion du détachement courant
    public string? rite_is { get; set; }

    public bool? is_allied_detachment { get; set; }

    // Pont avec l'armée primaire (utile pour alliés)
    public string? primary_army_is { get; set; }        // ex: LEGIONES_ASTARTES
    public string? primary_legion_is { get; set; }      // ex: DARK_ANGELS

    // Unit-level
    public int? size_at_least { get; set; }
    public int? size_at_most { get; set; }
    public string? option_selected { get; set; }
    public string? group_selected { get; set; }
    public string? has_wargear { get; set; }
    public string? trait_present { get; set; }
}

public sealed class ConditionBlock
{
    public List<Condition>? all { get; set; }
    public List<Condition>? any { get; set; }
    public List<Condition>? none { get; set; }
}
