using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using RecipeBook.ApplicationData;

namespace RecipeBook.Pages;

public partial class PageLike : Page
{
    public PageLike()
    {
        InitializeComponent();
        _ = LoadLikeRecipesAsync();
    }

    private async Task LoadLikeRecipesAsync()
    {
        try
        {
            if (AppConnect.CurrentAuthor == null)
            {
                MessageBox.Show("Автор не определен.");
                return;
            }

            var data = await AppConnect.Model.LikeRecipes
                .Include(l => l.Recipe)
                .Where(l => l.AuthorId == AppConnect.CurrentAuthor.AuthorId)
                .Select(l => l.Recipe!)
                .ToListAsync();

            LvLikeRecipes.ItemsSource = data;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки избранного: {ex.Message}");
        }
    }

    private async void DeleteLike_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AppConnect.CurrentAuthor == null) return;
            if (sender is not Button btn || !int.TryParse(btn.Tag?.ToString(), out var recipeId)) return;

            if (MessageBox.Show("Удалить рецепт из избранного?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var like = await AppConnect.Model.LikeRecipes.FirstOrDefaultAsync(l => l.AuthorId == AppConnect.CurrentAuthor.AuthorId && l.RecipeId == recipeId);
            if (like != null)
            {
                AppConnect.Model.LikeRecipes.Remove(like);
                await AppConnect.Model.SaveChangesAsync();
            }

            await LoadLikeRecipesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления: {ex.Message}");
        }
    }

    private void Back_OnClick(object sender, RoutedEventArgs e) => AppFrame.MainFrame.Navigate(new PageTask());
}
