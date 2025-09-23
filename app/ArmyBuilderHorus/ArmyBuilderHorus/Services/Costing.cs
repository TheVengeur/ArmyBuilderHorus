using ArmyBuilderHorus.Models;

namespace ArmyBuilderHorus.Services;

public static class Costing
{

    public static int AllowedForGroup(OptionGroup g, int currentSize)
    {
        if (g.limit_formula != null)
        {
            var step = Math.Max(1, g.limit_formula.step);
            return (currentSize / step) * g.limit_formula.per_step;
        }
        return g.max ?? int.MaxValue;
    }

    public static int UnitCost(ArmyUnit u, ArmyListItem it)
    {
        var total = u.base_cost;

        if (u.size != null && it.size.HasValue)
        {
            var sz = u.size!;
            var s = Math.Clamp(it.size!.Value, sz.min, sz.max);
            var basePts = sz.base_points;
            var extraCount = Math.Max(0, s - sz.base_models);
            total = basePts + extraCount * sz.extra_model_points;
        }

        if (u.options != null && it.choices != null)
        {
            foreach (var g in u.options.Where(o => o.type == "choice"))
            {
                if (it.choices.TryGetValue(g.id, out var choiceId))
                {
                    var ch = g.choices.FirstOrDefault(c => c.id == choiceId);
                    if (ch != null) total += ch.points;
                }
            }
        }

        if (u.options != null && it.counts != null)
        {
            foreach (var g in u.options.Where(o => o.type == "counted"))
            {
                if (it.counts.TryGetValue(g.id, out var bucket))
                {
                    foreach (var (choiceId, n) in bucket)
                    {
                        var ch = g.choices.FirstOrDefault(c => c.id == choiceId);
                        if (ch != null) total += ch.points * n;
                    }
                }
            }
        }

        total *= Math.Max(1, it.qty);
        return total;
    }
}
