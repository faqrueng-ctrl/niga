using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RecipeBook.ApplicationData;

namespace RecipeBook.Pages;

public partial class PageTask : Page
{
    private readonly ObservableCollection<RecipeListItem> _items = new();

    public PageTask()
    {
        InitializeComponent();
        LoadFilters();
        _ = LoadRecipesAsync();
    }

    private void LoadFilters()
    {
        try
        {
            var categories = AppConnect.Model.Categories.OrderBy(c => c.Name).ToList();
            categories.Insert(0, new Category { CategoryId = 0, Name = "Все категории" });
            CbCategory.ItemsSource = categories;
            CbCategory.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
        }
    }

    private async Task LoadRecipesAsync()
    {
        try
        {
            IQueryable<Recipe> query = AppConnect.Model.Recipes.Include(r => r.Images).Include(r => r.Category);

            var search = TbSearch.Text?.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Title.ToLower().Contains(search) || r.Description.ToLower().Contains(search));
            }

            if (CbCategory.SelectedItem is Category selectedCategory && selectedCategory.CategoryId != 0)
            {
                query = query.Where(r => r.CategoryId == selectedCategory.CategoryId);
            }

            if ((CbSort.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Время ↑")
            {
                query = query.OrderBy(r => r.CookingTimeMinutes);
            }
            else if ((CbSort.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Время ↓")
            {
                query = query.OrderByDescending(r => r.CookingTimeMinutes);
            }

            var data = await query.ToListAsync();
            _items.Clear();
            foreach (var item in data)
            {
                _items.Add(new RecipeListItem
                {
                    RecipeId = item.RecipeId,
                    Title = item.Title,
                    Description = item.Description,
                    CookingTimeMinutes = item.CookingTimeMinutes,
                    PreviewPath = item.Images.FirstOrDefault()?.ImagePath
                });
            }

            LvRecipes.ItemsSource = _items;
            IcRecipes.ItemsSource = _items;
            TbCounter.Text = $"Найдено: {_items.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки рецептов: {ex.Message}");
        }
    }

    private async void FilterChanged(object sender, EventArgs e) => await LoadRecipesAsync();

    private void BtnList_OnClick(object sender, RoutedEventArgs e)
    {
        LvRecipes.Visibility = Visibility.Visible;
        TileView.Visibility = Visibility.Collapsed;
    }

    private void BtnTile_OnClick(object sender, RoutedEventArgs e)
    {
        LvRecipes.Visibility = Visibility.Collapsed;
        TileView.Visibility = Visibility.Visible;
    }

    private void BtnAdd_OnClick(object sender, RoutedEventArgs e) => AppFrame.MainFrame.Navigate(new AddRecipe(0));

    private void BtnLike_OnClick(object sender, RoutedEventArgs e) => AppFrame.MainFrame.Navigate(new PageLike());

    private void EditRecipe_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out var id))
        {
            AppFrame.MainFrame.Navigate(new AddRecipe(id));
        }
    }

    private async void LikeRecipe_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AppConnect.CurrentAuthor == null)
            {
                MessageBox.Show("Сначала выполните вход.");
                return;
            }

            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out var recipeId))
            {
                var exists = await AppConnect.Model.LikeRecipes.AnyAsync(l => l.AuthorId == AppConnect.CurrentAuthor.AuthorId && l.RecipeId == recipeId);
                if (!exists)
                {
                    AppConnect.Model.LikeRecipes.Add(new LikeRecipe { AuthorId = AppConnect.CurrentAuthor.AuthorId, RecipeId = recipeId });
                    await AppConnect.Model.SaveChangesAsync();
                }
                MessageBox.Show("Рецепт добавлен в избранное.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка избранного: {ex.Message}");
        }
    }

    private void LvRecipes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LvRecipes.SelectedItem is RecipeListItem item)
        {
            AppFrame.MainFrame.Navigate(new AddRecipe(item.RecipeId));
        }
    }

    private void IcRecipes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if ((e.OriginalSource as FrameworkElement)?.DataContext is RecipeListItem item)
        {
            AppFrame.MainFrame.Navigate(new AddRecipe(item.RecipeId));
        }
    }
}

public class RecipeListItem
{
    public int RecipeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CookingTimeMinutes { get; set; }
    public string? PreviewPath { get; set; }
}
