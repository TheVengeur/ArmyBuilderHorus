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
        _rite = catalog.rites.FirstOrDefault(r => string.Equals(r.id, ctx.RiteId, StringComparison.OrdinalIgnoreCase));
    }

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

    public static int AllowedForGroup(OptionGroup g, int currentSize)
    {
        if (g.limit_formula != null)
        {
            var step = Math.Max(1, g.limit_formula.step);
            return (currentSize / step) * g.limit_formula.per_step;
        }
        return g.max ?? int.MaxValue;
    }
}
