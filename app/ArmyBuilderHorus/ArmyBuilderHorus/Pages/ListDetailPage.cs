using ArmyBuilderHorus.Services;
using Microsoft.Maui.Controls;

namespace ArmyBuilderHorus.Pages;

public sealed class ListDetailPage : ContentPage
{
    private readonly ListStore _store = new();
    private ArmyListMeta _meta;

    public ListDetailPage(ArmyListMeta meta)
    {
        _meta = meta;
        Title = meta.name;

        var lblTitle = new Label { Text = meta.name, FontSize = 22, FontAttributes = FontAttributes.Bold };
        var lblSub = new Label { Text = $"{meta.armyName} • {meta.points} pts", TextColor = Colors.Gray };

        var btnOpen = new Button { Text = "Ouvrir le builder" };
        btnOpen.Clicked += async (_, __) => await Navigation.PushAsync(new BuilderPage(_meta));

        var btnRename = new Button { Text = "Renommer" };
        btnRename.Clicked += async (_, __) =>
        {
            var newName = await DisplayPromptAsync("Renommer", "Nouveau nom :", initialValue: _meta.name);
            if (string.IsNullOrWhiteSpace(newName)) return;
            await _store.RenameAsync(_meta.filePath, newName.Trim());
            _meta = (await _store.GetAsync(_meta.filePath))!;
            Title = lblTitle.Text = _meta.name;
        };

        var btnDelete = new Button { Text = "Supprimer", TextColor = Colors.Red };
        btnDelete.Clicked += async (_, __) =>
        {
            if (await DisplayAlert("Supprimer", "Confirmer la suppression ?", "Oui", "Non"))
            {
                await _store.DeleteAsync(_meta.filePath);
                await Navigation.PopAsync(); // retour à "Mes listes"
            }
        };

        Content = new VerticalStackLayout
        {
            Padding = 16,
            Spacing = 8,
            Children = { lblTitle, lblSub, btnOpen, btnRename, btnDelete }
        };
    }
}
