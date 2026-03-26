using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using RecipeBook.ApplicationData;

namespace RecipeBook.Pages;

public partial class AddRecipe : Page
{
    private readonly int _recipeId;
    private Recipe _recipe = new();
    private readonly List<string> _images = new();
    private int _imageIndex;

    public AddRecipe(int recipeId)
    {
        InitializeComponent();
        _recipeId = recipeId;
        LoadCombos();
        _ = LoadRecipeAsync();
    }

    private void LoadCombos()
    {
        CbCategory.ItemsSource = AppConnect.Model.Categories.OrderBy(c => c.Name).ToList();
        CbAuthor.ItemsSource = AppConnect.Model.Authors.OrderBy(a => a.UserName).ToList();
    }

    private async Task LoadRecipeAsync()
    {
        try
        {
            if (_recipeId == 0)
            {
                if (AppConnect.CurrentAuthor != null)
                {
                    CbAuthor.SelectedValue = AppConnect.CurrentAuthor.AuthorId;
                }
                return;
            }

            _recipe = await AppConnect.Model.Recipes.Include(r => r.Images).FirstAsync(r => r.RecipeId == _recipeId);
            TbTitle.Text = _recipe.Title;
            TbDescription.Text = _recipe.Description;
            TbTime.Text = _recipe.CookingTimeMinutes.ToString();
            CbCategory.SelectedValue = _recipe.CategoryId;
            CbAuthor.SelectedValue = _recipe.AuthorId;

            _images.Clear();
            _images.AddRange(_recipe.Images.Select(i => i.ImagePath));
            RefreshImageViews();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки рецепта: {ex.Message}");
        }
    }

    private void RefreshImageViews()
    {
        LbImages.ItemsSource = null;
        LbImages.ItemsSource = _images;

        if (_images.Count == 0)
        {
            MainImage.Source = null;
            return;
        }

        _imageIndex = Math.Clamp(_imageIndex, 0, _images.Count - 1);
        MainImage.Source = new BitmapImage(new Uri(_images[_imageIndex], UriKind.RelativeOrAbsolute));
    }

    private async void Save_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!int.TryParse(TbTime.Text, out var time) || time <= 0)
            {
                MessageBox.Show("Укажите корректное время приготовления.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TbTitle.Text) || CbCategory.SelectedValue == null || CbAuthor.SelectedValue == null)
            {
                MessageBox.Show("Заполните обязательные поля.");
                return;
            }

            if (_recipeId == 0)
            {
                _recipe = new Recipe();
                AppConnect.Model.Recipes.Add(_recipe);
            }

            _recipe.Title = TbTitle.Text.Trim();
            _recipe.Description = TbDescription.Text.Trim();
            _recipe.CookingTimeMinutes = time;
            _recipe.CategoryId = (int)CbCategory.SelectedValue;
            _recipe.AuthorId = (int)CbAuthor.SelectedValue;

            await AppConnect.Model.SaveChangesAsync();

            if (_recipeId == 0 && _images.Count > 0)
            {
                foreach (var path in _images)
                {
                    AppConnect.Model.RecipeImages.Add(new RecipeImage { RecipeId = _recipe.RecipeId, ImagePath = path });
                }
                await AppConnect.Model.SaveChangesAsync();
            }

            MessageBox.Show("Рецепт сохранен.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}");
        }
    }

    private void GoSteps_OnClick(object sender, RoutedEventArgs e)
    {
        var id = _recipeId == 0 ? _recipe.RecipeId : _recipeId;
        if (id == 0)
        {
            MessageBox.Show("Сначала сохраните рецепт.");
            return;
        }
        AppFrame.MainFrame.Navigate(new PageRecepiesSteps(id));
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e) => AppFrame.MainFrame.Navigate(new PageTask());

    private void UploadImage_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var ofd = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp" };
            if (ofd.ShowDialog() != true) return;

            var imageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images");
            Directory.CreateDirectory(imageDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ofd.FileName)}";
            var target = Path.Combine(imageDir, fileName);
            File.Copy(ofd.FileName, target, true);

            _images.Add(target);
            _imageIndex = _images.Count - 1;
            RefreshImageViews();

            if (_recipeId != 0)
            {
                AppConnect.Model.RecipeImages.Add(new RecipeImage { RecipeId = _recipeId, ImagePath = target });
                AppConnect.Model.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
        }
    }

    private void PrevImage_OnClick(object sender, RoutedEventArgs e)
    {
        if (_images.Count == 0) return;
        _imageIndex = (_imageIndex - 1 + _images.Count) % _images.Count;
        RefreshImageViews();
    }

    private void NextImage_OnClick(object sender, RoutedEventArgs e)
    {
        if (_images.Count == 0) return;
        _imageIndex = (_imageIndex + 1) % _images.Count;
        RefreshImageViews();
    }

    private void LbImages_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LbImages.SelectedIndex < 0) return;
        _imageIndex = LbImages.SelectedIndex;
        RefreshImageViews();
    }
}
