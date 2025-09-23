using ArmyBuilderHorus.Models;
using ArmyBuilderHorus.Services;
using Microsoft.Maui.ApplicationModel; // MainThread

namespace ArmyBuilderHorus.Pages;

public sealed class BuilderPage : ContentPage
{
    private readonly CatalogService _catalogSvc = new();
    private readonly ListStore _store = new();
    private readonly ArmyListMeta _meta;

    private Catalog _cat = new();
    private Org? _org;

    private readonly Dictionary<string, int> _slotCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(ArmyUnit unit, ArmyListItem item)> _items = new();

    // UI réutilisable (créée une fois)
    private readonly Label _totalLbl = new() { Margin = new Thickness(16, 0) };
    private readonly Label _statusLbl = new() { Margin = new Thickness(16, 0) };
    private readonly Grid _grid = new() { Padding = 16 };
    private readonly VerticalStackLayout _listStack = new() { Padding = new Thickness(16, 0), Spacing = 6 };
    private readonly VerticalStackLayout _rootStack = new() { Spacing = 12 };

    public BuilderPage(ArmyListMeta meta)
    {
        _meta = meta;
        Title = $"Builder • {meta.name}";

        // Structure fixe une fois pour toutes
        _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        _rootStack.Children.Add(new VerticalStackLayout { Children = { _totalLbl, _statusLbl } });
        _rootStack.Children.Add(new BoxView { HeightRequest = 1, Color = Colors.LightGray, Margin = new Thickness(16, 4) });
        _rootStack.Children.Add(_grid);
        _rootStack.Children.Add(new BoxView { HeightRequest = 1, Color = Colors.LightGray, Margin = new Thickness(16, 0) });
        _rootStack.Children.Add(_listStack);

        Content = new ScrollView { Content = _rootStack };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _cat = await _catalogSvc.LoadAsync();
        _org = _cat.orgs.FirstOrDefault(o => o.id == _meta.focId);

        var full = await _store.LoadFullAsync(_meta.filePath);
        _items.Clear();
        if (full?.items != null)
        {
            foreach (var it in full.items)
            {
                var u = _cat.units.FirstOrDefault(x => x.id == it.unitId);
                if (u != null) _items.Add((u, it));
            }
        }

        if (_org == null)
        {
            Content = new Label { Text = $"FOC introuvable: '{_meta.focId}'", TextColor = Colors.Red, Padding = 16 };
            return;
        }

        await SaveAsync();
        Render(); // premier rendu
    }

    private void Render()
    {
        // S’appuie d’être sur le thread UI
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Recalcule compteurs
            _slotCounts.Clear();
            foreach (var s in _org!.slots.Keys) _slotCounts[s] = 0;
            foreach (var it in _items)
                if (_slotCounts.ContainsKey(it.unit.slot))
                    _slotCounts[it.unit.slot] += it.item.qty;

            _totalLbl.Text = $"Total: {TotalPoints()} / {_meta.points} pts";
            var valid = Valid();
            _statusLbl.Text = valid ? "Liste valide ✅" : "Contraintes non respectées ⚠️";
            _statusLbl.TextColor = valid ? Colors.Green : Colors.OrangeRed;

            // ---- Table des slots (on nettoie puis on remplit) ----
            _grid.Children.Clear();
            _grid.RowDefinitions.Clear();
            void Add(View v, int r, int c) { _grid.Add(v); Grid.SetRow(v, r); Grid.SetColumn(v, c); }

            int r = 0;
            _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Add(new Label { Text = "Slot", FontAttributes = FontAttributes.Bold }, r, 0);
            Add(new Label { Text = "Min", FontAttributes = FontAttributes.Bold }, r, 1);
            Add(new Label { Text = "Max", FontAttributes = FontAttributes.Bold }, r, 2);
            Add(new Label { Text = "Actuel", FontAttributes = FontAttributes.Bold }, r, 3);
            Add(new Label { Text = "" }, r, 4);
            r++;

            foreach (var kv in _org!.slots)
            {
                var slot = kv.Key; var s = kv.Value;
                _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                Add(new Label { Text = slot }, r, 0);
                Add(new Label { Text = s.min.ToString(), HorizontalTextAlignment = TextAlignment.End, WidthRequest = 40 }, r, 1);
                Add(new Label { Text = s.max.ToString(), HorizontalTextAlignment = TextAlignment.End, WidthRequest = 40 }, r, 2);

                var cur = _slotCounts.TryGetValue(slot, out var c) ? c : 0;
                var curLbl = new Label
                {
                    Text = cur.ToString(),
                    HorizontalTextAlignment = TextAlignment.End,
                    WidthRequest = 50,
                    TextColor = cur < s.min ? Colors.OrangeRed : Colors.Green
                };
                Add(curLbl, r, 3);

                var btn = new Button { Text = "Ajouter" };
                btn.Clicked += async (_, __) => await AddUnitToSlot(slot);
                Add(btn, r, 4);
                r++;
            }

            // ---- Liste des items ----
            _listStack.Children.Clear();
            _listStack.Children.Add(new Label { Text = "Unités choisies", FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 8, 0, 4) });
            if (_items.Count == 0)
                _listStack.Children.Add(new Label { Text = "(aucune pour l’instant)", TextColor = Colors.Gray });

            foreach (var it in _items)
            {
                var name = it.unit.name;
                if (it.item.size.HasValue) name += $" ({it.item.size} figs)";
                var row = new HorizontalStackLayout { Spacing = 8 };
                row.Add(new Label { Text = name, WidthRequest = 240, LineBreakMode = LineBreakMode.TailTruncation });
                row.Add(new Label { Text = $"{Costing.UnitCost(it.unit, it.item)} pts", TextColor = Colors.Gray });

                var edit = new Button { Text = "✎", WidthRequest = 36 };
                edit.Clicked += async (_, __) => await EditItemAsync(it);
                var del = new Button { Text = "–", WidthRequest = 36 };
                del.Clicked += async (_, __) => { _items.Remove(it); await SaveAsync(); Render(); };

                row.Add(edit); row.Add(del);
                _listStack.Children.Add(row);
            }
        });
    }

    private async Task AddUnitToSlot(string slot)
    {
        var options = _cat.units.Where(u => u.faction == _meta.armyId && string.Equals(u.slot, slot, StringComparison.OrdinalIgnoreCase)).ToList();
        if (options.Count == 0) { await DisplayAlert("Aucune unité", $"Pas d’unité pour le slot '{slot}'.", "OK"); return; }

        // si 1 seul type, on passe direct à la config
        ArmyUnit? chosen = null;
        if (options.Count == 1) chosen = options[0];
        else
        {
            var choice = await DisplayActionSheet($"Ajouter ({slot})", "Annuler", null, options.Select(u => u.name).ToArray());
            if (string.IsNullOrEmpty(choice) || choice == "Annuler") return;
            chosen = options.First(u => u.name == choice);
        }

        var page = new UnitConfigPage(chosen!, _meta.points);
        page.Completed += async (_, item) =>
        {
            _items.Add((chosen!, item));
            await SaveAsync();
            Render();
        };
        await Navigation.PushAsync(page);
    }

    private async Task EditItemAsync((ArmyUnit unit, ArmyListItem item) entry)
    {
        var page = new UnitConfigPage(entry.unit, _meta.points, entry.item);
        page.Completed += async (_, item) =>
        {
            var idx = _items.FindIndex(x => x.item == entry.item);
            if (idx >= 0) _items[idx] = (entry.unit, item);
            await SaveAsync();
            Render();
        };
        await Navigation.PushAsync(page);
    }

    private async Task RemoveOneAsync(ArmyUnit unit)
    {
        var idx = _items.FindIndex(x => x.unit.id == unit.id);
        if (idx >= 0)
        {
            var (u, item) = _items[idx];
            if (item.qty <= 1) _items.RemoveAt(idx);
            else
            {
                // Crée une nouvelle instance avec qty décrémenté
                var newItem = new ArmyListItem
                {
                    unitId = item.unitId,
                    qty = item.qty - 1,
                    @base = item.@base,
                    size = item.size,
                    choices = item.choices != null ? new Dictionary<string, string>(item.choices) : null,
                    counts = item.counts != null ? new Dictionary<string, Dictionary<string, int>>(item.counts) : null
                };
                _items[idx] = (u, newItem);
            }
            await SaveAsync();
            Render();
        }
    }

    private int TotalPoints() => _items.Sum(it => Costing.UnitCost(it.unit, it.item));

    private async Task SaveAsync()
    {
        var full = new ArmyListFull
        {
            name = _meta.name,
            points = _meta.points,
            armyId = _meta.armyId,
            armyName = _meta.armyName,
            focId = _meta.focId,
            riteId = _meta.riteId,
            createdAt = _meta.createdAt,
            filePath = _meta.filePath,
            items = _items.Select(x => x.item).ToList()
        };
        await _store.SaveFullAsync(_meta.filePath, full);
    }

    private bool Valid()
    {
        foreach (var kv in _org!.slots)
        {
            var cur = _slotCounts.TryGetValue(kv.Key, out var c) ? c : 0;
            if (cur < kv.Value.min || cur > kv.Value.max) return false;
        }
        return true;
    }
}
