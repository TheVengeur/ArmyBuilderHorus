using ArmyBuilderHorus.Models;

namespace ArmyBuilderHorus.Services;

public sealed class RulesEngine
{
    private readonly Catalog _catalog;
    private readonly ArmyContext _ctx;
    private readonly ArmyListMeta _meta;
    private readonly Rite? _rite;

    public RulesEngine(Catalog catalog, ArmyContext ctx, ArmyListMeta meta)
    {
        _catalog = catalog;
        _ctx = ctx;
        _meta = meta;
        _rite = catalog.rites.FirstOrDefault(r => r.id == ctx.RiteId);
    }

    // -------- Conditions ----------
    private bool Eval(Condition c, UnitState st)
    {
        if (c.army_is != null && !Eq(_ctx.ArmyId, c.army_is)) return false;
        if (c.allegiance_is != null && !Eq(_ctx.Allegiance, c.allegiance_is)) return false;
        if (c.legion_is != null && !Eq(_ctx.LegionId, c.legion_is)) return false;
        if (c.rite_is != null && !Eq(_ctx.RiteId, c.rite_is)) return false;

        if (c.is_allied_detachment.HasValue && _ctx.IsAlliedDetachment != c.is_allied_detachment.Value) return false;

        if (c.primary_army_is != null && !Eq(_ctx.PrimaryArmyId, c.primary_army_is)) return false;
        if (c.primary_legion_is != null && !Eq(_ctx.PrimaryLegionId, c.primary_legion_is)) return false;

        if (c.size_at_least.HasValue && st.Size < c.size_at_least.Value) return false;
        if (c.size_at_most.HasValue && st.Size > c.size_at_most.Value) return false;

        if (c.option_selected != null && !st.IsOptionSelected(c.option_selected)) return false;
        if (c.group_selected != null && !st.IsGroupChosen(c.group_selected)) return false;
        if (c.has_wargear != null && !st.TargetHasWargear(c.has_wargear)) return false;
        if (c.trait_present != null && !st.UnitHasTrait(c.trait_present)) return false;

        return true;
    }

    private bool Eq(string? a, string? b) => string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);

    private bool EvalBlock(ConditionBlock? b, UnitState st)
    {
        if (b == null) return true;
        if (b.all != null && !b.all.All(x => Eval(x, st))) return false;
        if (b.any != null && !b.any.Any(x => Eval(x, st))) return false;
        if (b.none != null && b.none.Any(x => Eval(x, st))) return false;
        return true;
    }

    // -------- Rappel des helpers déjà posés ----------
    public string EffectiveSlot(ArmyUnit u) => u.slot;

    public bool IsCompulsoryEligible(ArmyUnit u, string slot)
    {
        var baseEligible = u.compulsory_eligible ?? true;
        if (!baseEligible) return false;

        if (slot.Equals("TROOPS", StringComparison.OrdinalIgnoreCase) &&
            u.traits.Any(t => string.Equals(t, "Support Squad", StringComparison.OrdinalIgnoreCase)))
        {
            return _rite?.allow_support_troops_compulsory ?? false;
        }
        return true;
    }

    // ► tu continueras ici avec Availability(), TryApply(), etc. (comme on a esquissé plus tôt)
}
