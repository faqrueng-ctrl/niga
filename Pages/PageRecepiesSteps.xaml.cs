using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
            var editor = new StepEditorWindow();
            if (editor.ShowDialog() != true)
            {
                return;
            }

            var step = new RecipeStep
            {
                RecipeId = _recipeId,
                StepNumber = AppConnect.Model.RecipeSteps.Count(s => s.RecipeId == _recipeId) + 1,
                Description = editor.StepDescription,
                ImagePath = editor.ImagePath
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
            if (sender is not Button btn || !int.TryParse(btn.Tag?.ToString(), out var id))
            {
                return;
            }

            var step = await AppConnect.Model.RecipeSteps.FirstAsync(s => s.RecipeStepId == id);
            var editor = new StepEditorWindow(step.Description, step.ImagePath);
            if (editor.ShowDialog() != true)
            {
                return;
            }

            step.Description = editor.StepDescription;
            step.ImagePath = editor.ImagePath;

            await AppConnect.Model.SaveChangesAsync();
            await LoadStepsAsync();
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
            if (sender is not Button btn || !int.TryParse(btn.Tag?.ToString(), out var id))
            {
                return;
            }

            if (MessageBox.Show("Удалить шаг?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

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
            if (sender is not Button btn || !int.TryParse(btn.Tag?.ToString(), out var id))
            {
                return;
            }

            var steps = await AppConnect.Model.RecipeSteps.Where(s => s.RecipeId == _recipeId).OrderBy(s => s.StepNumber).ToListAsync();
            var idx = steps.FindIndex(s => s.RecipeStepId == id);
            var newIdx = idx + delta;
            if (idx < 0 || newIdx < 0 || newIdx >= steps.Count)
            {
                return;
            }

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

public class StepEditorWindow : Window
{
    private readonly TextBox _tbDescription;
    private readonly TextBox _tbImagePath;

    public string StepDescription => _tbDescription.Text.Trim();

    public string? ImagePath => string.IsNullOrWhiteSpace(_tbImagePath.Text) ? null : _tbImagePath.Text.Trim();

    public StepEditorWindow(string description = "", string? imagePath = null)
    {
        Title = "Редактирование шага";
        Width = 520;
        Height = 320;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var title = new TextBlock
        {
            Text = "Описание шага",
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 6)
        };
        Grid.SetRow(title, 0);

        var content = new StackPanel();
        _tbDescription = new TextBox
        {
            Text = description,
            AcceptsReturn = true,
            Height = 120,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var imageRow = new DockPanel { Margin = new Thickness(0, 10, 0, 0) };
        _tbImagePath = new TextBox
        {
            Text = imagePath ?? string.Empty,
            IsReadOnly = true,
            Margin = new Thickness(0, 0, 8, 0)
        };
        DockPanel.SetDock(_tbImagePath, Dock.Left);

        var btnImage = new Button
        {
            Content = "Выбрать изображение",
            Width = 180
        };
        btnImage.Click += SelectImage;

        imageRow.Children.Add(_tbImagePath);
        imageRow.Children.Add(btnImage);

        content.Children.Add(_tbDescription);
        content.Children.Add(imageRow);
        Grid.SetRow(content, 1);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var btnSave = new Button { Content = "Сохранить", Width = 110 };
        btnSave.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(StepDescription))
            {
                MessageBox.Show("Введите описание шага.");
                return;
            }

            DialogResult = true;
        };

        var btnCancel = new Button { Content = "Отмена", Width = 110 };
        btnCancel.Click += (_, _) => DialogResult = false;

        buttonPanel.Children.Add(btnSave);
        buttonPanel.Children.Add(btnCancel);
        Grid.SetRow(buttonPanel, 2);

        root.Children.Add(title);
        root.Children.Add(content);
        root.Children.Add(buttonPanel);
        Content = root;
    }

    private void SelectImage(object? sender, RoutedEventArgs e)
    {
        try
        {
            var ofd = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp" };
            if (ofd.ShowDialog() != true)
            {
                return;
            }

            var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            var imageDir = Path.Combine(resourcesPath, "Images");
            Directory.CreateDirectory(imageDir);

            var fileName = $"step_{Guid.NewGuid()}{Path.GetExtension(ofd.FileName)}";
            var absolutePath = Path.Combine(imageDir, fileName);
            File.Copy(ofd.FileName, absolutePath, true);

            _tbImagePath.Text = Path.Combine("Resources", "Images", fileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка выбора изображения: {ex.Message}");
        }
    }
}
