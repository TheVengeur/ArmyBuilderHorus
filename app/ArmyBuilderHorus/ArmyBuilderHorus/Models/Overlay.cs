namespace ArmyBuilderHorus.Models;

public sealed class OverlayDoc
{
    public ConditionBlock? when { get; set; }
    public List<UnitPatch> patch { get; set; } = new();
}

public sealed class UnitPatch
{
    public string unit_id { get; set; } = "";
    public List<OptionGroup>? add_options { get; set; } = null;
    public bool? set_compulsory_eligible { get; set; } = null;
    public List<string>? add_traits { get; set; } = null;
    public List<GroupModification>? modify { get; set; } = null;
}

public sealed class GroupModification
{
    public string group_id { get; set; } = "";
    public List<OptionChoice>? choices_add { get; set; } = null;
}
