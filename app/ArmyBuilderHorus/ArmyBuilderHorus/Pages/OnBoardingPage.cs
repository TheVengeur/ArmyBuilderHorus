using ArmyBuilderHorus.Services;

namespace ArmyBuilderHorus.Pages;

public class OnBoardingPage : ContentPage
{
    // ⚠️ Mets l’URL RAW GitHub ici :
    private const string IndexUrl = "https://raw.githubusercontent.com/TheVengeur/ArmyBuilderHorus/main/packs/packs.index.json";

    private readonly Entry _entry = new() { Placeholder = "Clé du club", IsPassword = true };
    private readonly Button _btn = new() { Text = "Continuer" };
    private readonly Label _status = new() { TextColor = Colors.Red };
    private readonly PackUpdater _updater = new();

    public OnBoardingPage()
    {
        Title = "ArmyBuilderHorus";
        _btn.Clicked += OnContinue;

        Content = new VerticalStackLayout
        {
            Padding = 16,
            Children = {
                new Label { Text = "Entrer la clé du club", FontSize = 22, FontAttributes = FontAttributes.Bold },
                _entry,
                _btn,
                _status
            }
        };
    }

    private async void OnContinue(object? sender, EventArgs e)
    {
        try
        {
            _btn.IsEnabled = false; _status.Text = "";
            var pass = _entry.Text ?? "";
            if (string.IsNullOrWhiteSpace(pass)) { _status.Text = "Saisis la clé."; return; }

            var version = await _updater.UpdateAsync(IndexUrl, pass);
            await KeyStore.SaveAsync(pass);
            await Navigation.PushAsync(new SuccessPage(version));
        }
        catch (Exception ex) { _status.Text = ex.Message; }
        finally { _btn.IsEnabled = true; }
    }
}

public sealed class SuccessPage : ContentPage
{
    public SuccessPage(string version)
    {
        Title = "Packs";
        var btnLists = new Button { Text = "Mes listes" };
        btnLists.Clicked += async (_, __) => await Navigation.PushAsync(new MyListsPage());

        var btnCreate = new Button { Text = "Créer une liste" };
        btnCreate.Clicked += async (_, __) => await Navigation.PushAsync(new CreateListPage());

        Content = new VerticalStackLayout
        {
            Padding = 16,
            Children = {
                new Label { Text = $"Packs installés ✅  (v{version})", FontSize = 20 },
                new Label { Text = "Prochaine étape :", TextColor = Colors.Gray },
                new HorizontalStackLayout { Spacing = 12, Children = { btnCreate, btnLists } }
              }
        };
    }
}

