using ArmyBuilderHorus.Models;
using ArmyBuilderHorus.Services;
using Microsoft.Maui.Storage;

namespace ArmyBuilderHorus.Pages;

public sealed class CreateListPage : ContentPage
{
    private readonly CatalogService _catalogSvc = new();
    private Catalog _catalog = new();

    // UI
    private readonly Entry _name = new() { Text = "Nouvelle liste" };
    private readonly Entry _points = new() { Text = "2000", Keyboard = Keyboard.Numeric };
    private readonly Picker _army = new();
    private readonly Picker _foc = new();
    private readonly Picker _rite = new() { Title = "(optionnel)" };
    private readonly Label _msg = new() { TextColor = Colors.Red };
    private readonly Button _create = new() { Text = "Créer la liste" };

    public CreateListPage()
    {
        Title = "Créer une liste";

        _army.SelectedIndexChanged += (_, __) => RefreshFocsAndRites();
        _foc.SelectedIndexChanged += (_, __) => RefreshRites();

        _create.Clicked += async (_, __) => await OnCreateAsync();

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 12,
                Children = {
                    new Label { Text = "Nom", FontAttributes = FontAttributes.Bold },
                    _name,
                    new Label { Text = "Points", FontAttributes = FontAttributes.Bold },
                    _points,
                    new Label { Text = "Armée", FontAttributes = FontAttributes.Bold },
                    _army,
                    new Label { Text = "Détachement (FOC)", FontAttributes = FontAttributes.Bold },
                    _foc,
                    new Label { Text = "Rite de guerre", FontAttributes = FontAttributes.Bold },
                    _rite,
                    _create,
                    _msg
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            _catalog = await _catalogSvc.LoadAsync();
            _army.ItemsSource = _catalog.armies.Select(a => a.name).ToList();
            if (_army.ItemsSource.Count > 0) _army.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            _msg.Text = ex.Message;
        }
    }

    private void RefreshFocsAndRites()
    {
        var a = GetSelectedArmy();
        _foc.ItemsSource = a?.available_focs.Select(f => $"{f.label} ({f.id})").ToList() ?? new List<string>();
        _foc.SelectedIndex = (_foc.ItemsSource.Count > 0) ? 0 : -1;
        RefreshRites();
    }

    private void RefreshRites()
    {
        var a = GetSelectedArmy();
        var focId = GetSelectedFocId();
        List<string> riteIds = new();
        if (a != null && focId != null && a.available_rites.TryGetValue(focId, out var list)) riteIds = list;
        _rite.ItemsSource = riteIds;
        _rite.SelectedIndex = -1;
    }

    private Army? GetSelectedArmy()
    {
        var i = _army.SelectedIndex;
        return (i >= 0 && i < _catalog.armies.Count) ? _catalog.armies[i] : null;
    }
    private string? GetSelectedFocId()
    {
        var a = GetSelectedArmy();
        var i = _foc.SelectedIndex;
        return (a != null && i >= 0 && i < a.available_focs.Count) ? a.available_focs[i].id : null;
    }
    private string? GetSelectedRiteId()
    {
        return (_rite.SelectedIndex >= 0) ? _rite.ItemsSource![_rite.SelectedIndex]?.ToString() : null;
    }

    private async Task OnCreateAsync()
    {
        try
        {
            _msg.Text = "";
            if (string.IsNullOrWhiteSpace(_name.Text)) { _msg.Text = "Nom requis."; return; }
            if (!int.TryParse(_points.Text, out var pts) || pts <= 0) { _msg.Text = "Points invalides."; return; }

            var army = GetSelectedArmy(); if (army == null) { _msg.Text = "Choisir une armée."; return; }
            var focId = GetSelectedFocId(); if (focId == null) { _msg.Text = "Choisir un FOC."; return; }
            var riteId = GetSelectedRiteId(); // optionnel

            // Enregistre la liste (JSON simple)
            var payload = new
            {
                name = _name.Text!.Trim(),
                points = pts,
                armyId = army.id,
                armyName = army.name,
                focId,
                riteId,
                createdAt = DateTime.UtcNow
            };

            var listsDir = Path.Combine(FileSystem.AppDataDirectory, "lists");
            Directory.CreateDirectory(listsDir);
            var file = Path.Combine(listsDir, SanitizeFileName($"{payload.name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}") + ".json");

            var full = new ArmyListFull
            {
                name = payload.name,
                points = pts,
                armyId = army.id,
                armyName = army.name,
                focId = focId!,
                riteId = riteId,
                createdAt = DateTime.UtcNow,
                filePath = file,
                items = new List<ArmyListItem>() // vide à la création
            };

            // sérialise via ListStore pour garder le même format partout
            var store = new ListStore();
            await store.SaveFullAsync(file, full);

            await DisplayAlert("OK", "Liste créée et sauvegardée.", "Continuer");
            await Navigation.PushAsync(new MyListsPage());

        }
        catch (Exception ex)
        {
            _msg.Text = ex.Message;
        }
    }

    private static string SanitizeFileName(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
        return s;
    }
}
