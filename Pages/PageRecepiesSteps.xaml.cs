using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using RecipeBook.ApplicationData;

namespace RecipeBook.Pages;

public partial class PageRecepiesSteps : Page
{
    private readonly int _recipeId;

    public PageRecepiesSteps(int recipeId)
    {
        InitializeComponent();
        _recipeId = recipeId;
        _ = LoadStepsAsync();
    }

    private async Task LoadStepsAsync()
    {
        try
        {
            LvSteps.ItemsSource = await AppConnect.Model.RecipeSteps
                .Where(s => s.RecipeId == _recipeId)
                .OrderBy(s => s.StepNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки шагов: {ex.Message}");
        }
    }

    private async void Add_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var step = new RecipeStep
            {
                RecipeId = _recipeId,
                StepNumber = AppConnect.Model.RecipeSteps.Count(s => s.RecipeId == _recipeId) + 1,
                Description = "Новый шаг"
            };
            AppConnect.Model.RecipeSteps.Add(step);
            await AppConnect.Model.SaveChangesAsync();
            await LoadStepsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка добавления шага: {ex.Message}");
        }
    }

    private async void Edit_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button btn || !int.TryParse(btn.Tag?.ToString(), out var id)) return;
            var step = await AppConnect.Model.RecipeSteps.FirstAsync(s => s.RecipeStepId == id);
            var text = ShowStepEditor(step.Description);
            if (!string.IsNullOrWhiteSpace(text))
            {
                step.Description = text.Trim();
                await AppConnect.Model.SaveChangesAsync();
                await LoadStepsAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка редактирования шага: {ex.Message}");
        }
    }

    private async void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button btn || !int.TryParse(btn.Tag?.ToString(), out var id)) return;
            if (MessageBox.Show("Удалить шаг?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var step = await AppConnect.Model.RecipeSteps.FirstAsync(s => s.RecipeStepId == id);
            AppConnect.Model.RecipeSteps.Remove(step);
            await AppConnect.Model.SaveChangesAsync();
            await NormalizeOrderAsync();
            await LoadStepsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления шага: {ex.Message}");
        }
    }

    private async void MoveUp_OnClick(object sender, RoutedEventArgs e) => await ChangeOrderAsync(sender, -1);
    private async void MoveDown_OnClick(object sender, RoutedEventArgs e) => await ChangeOrderAsync(sender, 1);

    private async Task ChangeOrderAsync(object sender, int delta)
    {
        try
        {
            if (sender is not Button btn || !int.TryParse(btn.Tag?.ToString(), out var id)) return;

            var steps = await AppConnect.Model.RecipeSteps.Where(s => s.RecipeId == _recipeId).OrderBy(s => s.StepNumber).ToListAsync();
            var idx = steps.FindIndex(s => s.RecipeStepId == id);
            var newIdx = idx + delta;
            if (idx < 0 || newIdx < 0 || newIdx >= steps.Count) return;

            (steps[idx], steps[newIdx]) = (steps[newIdx], steps[idx]);
            for (var i = 0; i < steps.Count; i++)
            {
                steps[i].StepNumber = i + 1;
            }

            await AppConnect.Model.SaveChangesAsync();
            await LoadStepsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка изменения порядка: {ex.Message}");
        }
    }


    private static string? ShowStepEditor(string currentText)
    {
        var window = new Window
        {
            Title = "Редактирование шага",
            Width = 420,
            Height = 240,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        var panel = new StackPanel { Margin = new Thickness(12) };
        var box = new TextBox { Text = currentText, AcceptsReturn = true, Height = 120, TextWrapping = TextWrapping.Wrap };
        var ok = new Button { Content = "Сохранить", Width = 110, HorizontalAlignment = HorizontalAlignment.Right };
        ok.Click += (_, _) => window.DialogResult = true;

        panel.Children.Add(box);
        panel.Children.Add(ok);
        window.Content = panel;

        return window.ShowDialog() == true ? box.Text : null;
    }

    private async Task NormalizeOrderAsync()
    {
        var steps = await AppConnect.Model.RecipeSteps.Where(s => s.RecipeId == _recipeId).OrderBy(s => s.StepNumber).ToListAsync();
        for (var i = 0; i < steps.Count; i++)
        {
            steps[i].StepNumber = i + 1;
        }
        await AppConnect.Model.SaveChangesAsync();
    }

    private void Back_OnClick(object sender, RoutedEventArgs e) => AppFrame.MainFrame.Navigate(new PageTask());
}
