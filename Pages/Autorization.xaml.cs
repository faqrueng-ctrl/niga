using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using RecipeBook.ApplicationData;

namespace RecipeBook.Pages;

public partial class Autorization : Page
{
    public Autorization()
    {
        InitializeComponent();
    }

    private async void BtnLogin_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var login = TbLogin.Text.Trim();
            var pass = PbPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            var author = await AppConnect.Model.Authors.FirstOrDefaultAsync(a => a.Login == login && a.Password == pass);
            if (author == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            AppConnect.CurrentAuthor = author;
            MessageBox.Show("Успешный вход.");
            AppFrame.MainFrame.Navigate(new PageTask());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка входа: {ex.Message}");
        }
    }

    private void BtnReg_OnClick(object sender, RoutedEventArgs e)
    {
        AppFrame.MainFrame.Navigate(new Reg());
    }
}
