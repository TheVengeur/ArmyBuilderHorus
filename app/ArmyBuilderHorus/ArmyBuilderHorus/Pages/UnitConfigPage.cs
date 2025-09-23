using ArmyBuilderHorus.Models;
using ArmyBuilderHorus.Services;

namespace ArmyBuilderHorus.Pages;

public sealed class UnitConfigPage : ContentPage
{
    private readonly ArmyUnit _unit;
    private readonly int _budget;
    private int _size;
    private readonly Dictionary<string, string> _choices = new();
    private readonly Dictionary<string, Dictionary<string, int>> _counts = new();

    private readonly Label _price = new() { Margin = new Thickness(0, 8) };

    public UnitConfigPage(ArmyUnit unit, int budget, ArmyListItem? existing = null)
    {
        _unit = unit;
        _budget = budget;
        Title = unit.name;

        // Init taille
        if (unit.size != null)
            _size = existing?.size ?? unit.size.base_models;
        else
            _size = 1;

        // Init options
        if (existing?.choices != null) foreach (var kv in existing.choices) _choices[kv.Key] = kv.Value;
        if (existing?.counts != null) foreach (var kv in existing.counts) _counts[kv.Key] = new(kv.Value);

        // UI dynamique
        var stack = new VerticalStackLayout { Padding = 16, Spacing = 10 };

        if (unit.size != null)
        {
            var sz = unit.size!;
            var stepper = new Stepper(sz.min, sz.max, _size, sz.step);
            var lbl = new Label { Text = $"Taille: {_size}" };
            stepper.ValueChanged += (_, e) => { _size = (int)e.NewValue; lbl.Text = $"Taille: {_size}"; RefreshPrice(); };
            stack.Add(new Label { Text = "Taille d'escouade", FontAttributes = FontAttributes.Bold });
            stack.Add(new HorizontalStackLayout { Spacing = 12, Children = { lbl, stepper } });
        }

        foreach (var g in unit.options)
        {
            stack.Add(new Label { Text = g.label, FontAttributes = FontAttributes.Bold });

            if (g.type == "choice")
            {
                var picker = new Picker { Title = "(aucun)" };
                picker.ItemsSource = g.choices.Select(c => c.label).ToList();
                // valeur existante
                if (_choices.TryGetValue(g.id, out var chId))
                {
                    var idx = g.choices.FindIndex(c => c.id == chId);
                    if (idx >= 0) picker.SelectedIndex = idx;
                }
                picker.SelectedIndexChanged += (_, __) =>
                {
                    if (picker.SelectedIndex >= 0) _choices[g.id] = g.choices[picker.SelectedIndex].id;
                    else _choices.Remove(g.id);
                    RefreshPrice();
                };
                stack.Add(picker);
            }
            else if (g.type == "counted")
            {
                var allowedLabel = new Label();
                var rows = new VerticalStackLayout { Spacing = 6 };

                int Allowed() => Costing.AllowedForGroup(g, _size);

                foreach (var ch in g.choices)
                {
                    var cur = 0;
                    if (_counts.TryGetValue(g.id, out var map) && map.TryGetValue(ch.id, out var n)) cur = n;

                    var lbl = new Label { Text = $"{ch.label}: {cur}" };
                    var minus = new Button { Text = "–", WidthRequest = 36 };
                    var plus = new Button { Text = "+", WidthRequest = 36 };

                    minus.Clicked += (_, __) =>
                    {
                        var m = GetBucket(g.id);
                        if (m.TryGetValue(ch.id, out var v) && v > 0) m[ch.id] = v - 1;
                        lbl.Text = $"{ch.label}: {m.GetValueOrDefault(ch.id)}";
                        allowedLabel.Text = $"Max autorisé : {Allowed()}";
                        RefreshPrice();
                    };
                    plus.Clicked += (_, __) =>
                    {
                        var m = GetBucket(g.id);
                        var sum = m.Values.Sum();
                        var max = Allowed();
                        if (sum >= max) return;
                        m[ch.id] = m.GetValueOrDefault(ch.id) + 1;
                        lbl.Text = $"{ch.label}: {m[ch.id]}";
                        allowedLabel.Text = $"Max autorisé : {Allowed()}";
                        RefreshPrice();
                    };

                    rows.Add(new HorizontalStackLayout { Spacing = 6, Children = { lbl, minus, plus } });
                }

                allowedLabel.Text = $"Max autorisé : {Allowed()}";
                stack.Add(allowedLabel);
                stack.Add(rows);
            }
        }

        RefreshPrice();
        stack.Add(_price);

        var save = new Button { Text = "Enregistrer" };
        save.Clicked += async (_, __) =>
        {
            var item = new ArmyListItem
            {
                unitId = _unit.id,
                qty = 1,
                @base = _unit.base_cost,
                size = _unit.size != null ? _size : null,
                choices = _choices.Count > 0 ? new(_choices) : null,
                counts = _counts.Count > 0 ? new(_counts) : null
            };
            await Navigation.PopAsync();                // ferme la page
            Completed?.Invoke(this, item);              // renvoie la config
        };
        stack.Add(save);

        Content = new ScrollView { Content = stack };
    }

    private Dictionary<string, int> GetBucket(string groupId)
    {
        if (!_counts.TryGetValue(groupId, out var map)) { map = new(); _counts[groupId] = map; }
        return map;
    }

    private void RefreshPrice()
    {
        var probe = new ArmyListItem
        {
            unitId = _unit.id,
            qty = 1,
            @base = _unit.base_cost,
            size = _unit.size != null ? _size : null,
            choices = _choices.Count > 0 ? new(_choices) : null,
            counts = _counts.Count > 0 ? new(_counts) : null
        };
        var cost = Costing.UnitCost(_unit, probe);
        _price.Text = $"Coût de l’unité : {cost} pts (budget {_budget})";
    }

    // callback
    public event EventHandler<ArmyListItem>? Completed;
}
