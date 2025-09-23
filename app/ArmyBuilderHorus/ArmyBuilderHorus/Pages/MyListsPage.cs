using ArmyBuilderHorus.Services;

namespace ArmyBuilderHorus.Pages;

public sealed class MyListsPage : ContentPage
{
    private readonly ListStore _store = new();
    private readonly CollectionView _cv = new();
    private readonly Label _empty = new() { Text = "Aucune liste pour le moment.", TextColor = Colors.Gray };

    public MyListsPage()
    {
        Title = "Mes listes";

        _cv.SelectionMode = SelectionMode.Single;
        _cv.ItemTemplate = new DataTemplate(() =>
        {
            var name = new Label { FontAttributes = FontAttributes.Bold, FontSize = 16 };
            name.SetBinding(Label.TextProperty, "name");

            var sub = new Label { TextColor = Colors.Gray, FontSize = 13 };
            sub.SetBinding(Label.TextProperty, new Binding(
                path: "armyName",
                stringFormat: "{0} • {1} pts"
            ));
            // astuce: on concatène points via a little trick:
            sub.BindingContextChanged += (s, e) =>
            {
                if (s is Label l && l.BindingContext is ArmyListMeta m)
                    l.Text = $"{m.armyName} • budget {m.points} pts";
            };


            return new Frame
            {
                Padding = 12,
                Margin = new Thickness(12, 6),
                HasShadow = false,
                BorderColor = Colors.LightGray,
                Content = new VerticalStackLayout { Spacing = 4, Children = { name, sub } }
            };
        });

        _cv.SelectionChanged += async (_, e) =>
        {
            var item = e.CurrentSelection.FirstOrDefault() as ArmyListMeta;
            if (item != null) { await Navigation.PushAsync(new ListDetailPage(item)); _cv.SelectedItem = null; }
        };

        var createBtn = new Button { Text = "Créer une nouvelle liste" };
        createBtn.Clicked += async (_, __) => await Navigation.PushAsync(new CreateListPage());

        var overlay = new Grid();
        overlay.Children.Add(_cv);
        overlay.Children.Add(_empty);

        // barre du bas avec le bouton
        var bottomBar = new StackLayout { Padding = 12, Children = { createBtn } };

        // grille racine : 2 lignes (contenu + barre)
        var root = new Grid
        {
            RowDefinitions = new RowDefinitionCollection {
        new RowDefinition(GridLength.Star),
        new RowDefinition(GridLength.Auto)
    }
        };

        // on place chaque vue sur la bonne ligne
        root.Children.Add(overlay);
        Grid.SetRow(overlay, 0);

        root.Children.Add(bottomBar);
        Grid.SetRow(bottomBar, 1);

        // on assigne la page
        Content = root;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var items = await _store.GetAllAsync();
        _cv.ItemsSource = items;
        _empty.IsVisible = items.Count == 0;
    }
}
