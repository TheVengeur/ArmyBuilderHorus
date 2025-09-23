using ArmyBuilderHorus.Models;

namespace ArmyBuilderHorus.Services;

public static class OverlayComposer
{
    // applique tous les overlays valides pour le contexte donné
    public static Catalog Compose(Catalog baseCat, IEnumerable<OverlayDoc> overlays, ArmyContext ctx)
    {
        // deep copy light
        var cat = new Catalog
        {
            version = baseCat.version,
            armies = baseCat.armies.ToList(),
            orgs = baseCat.orgs.ToList(),
            rites = baseCat.rites.ToList(),
            units = baseCat.units.Select(u => CloneUnit(u)).ToList()
        };

        var rules = new RulesEngine(baseCat, ctx, new ArmyListMeta());
        foreach (var ov in overlays)
        {
            var st = new UnitStateDummy(); // pour EvalBlock sans unité, taille neutre
            bool ok = ov.when == null || (bool)typeof(RulesEngine)
                .GetMethod("EvalBlock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(rules, new object?[] { ov.when!, st })!;
            if (!ok) continue;

            foreach (var p in ov.patch)
            {
                var u = cat.units.FirstOrDefault(x => x.id == p.unit_id);
                if (u == null) continue;
                if (p.add_traits != null) u.traits.AddRange(p.add_traits);
                if (p.set_compulsory_eligible.HasValue) u.compulsory_eligible = p.set_compulsory_eligible.Value;
                if (p.add_options != null) u.options.AddRange(p.add_options);
                if (p.modify != null)
                {
                    foreach (var m in p.modify)
                    {
                        var g = u.options.FirstOrDefault(x => x.id == m.group_id);
                        if (g != null && m.choices_add != null) g.choices.AddRange(m.choices_add);
                    }
                }
            }
        }
        return cat;

        static ArmyUnit CloneUnit(ArmyUnit s) => new()
        {
            id = s.id,
            name = s.name,
            faction = s.faction,
            slot = s.slot,
            base_cost = s.base_cost,
            size = s.size == null ? null : new UnitSize
            {
                min = s.size.min,
                max = s.size.max,
                step = s.size.step,
                base_models = s.size.base_models,
                base_points = s.size.base_points,
                extra_model_points = s.size.extra_model_points
            },
            options = s.options.Select(g => new OptionGroup
            {
                id = g.id,
                label = g.label,
                type = g.type,
                max = g.max,
                limit_formula = g.limit_formula == null ? null : new LimitFormula { step = g.limit_formula.step, per_step = g.limit_formula.per_step },
                choices = g.choices.Select(c => new OptionChoice
                {
                    id = c.id,
                    label = c.label,
                    points = c.points,
                    requires = c.requires,
                    unique_in_army = c.unique_in_army,
                    grants_tags = c.grants_tags
                }).ToList(),
                available_when = g.available_when,
                excludes_groups = g.excludes_groups,
                replaces = g.replaces,
                applies_to_model_set = g.applies_to_model_set
            }).ToList(),
            traits = new List<string>(s.traits),
            compulsory_eligible = s.compulsory_eligible
        };
    }

    // petit type factice pour EvalBlock sans dépendre du vrai UnitState
    private sealed class UnitStateDummy
    {
        public int Size => 0;
        public bool IsOptionSelected(string id) => false;
        public bool IsGroupChosen(string id) => false;
        public bool TargetHasWargear(string w) => false;
        public bool UnitHasTrait(string t) => false;
    }
}
