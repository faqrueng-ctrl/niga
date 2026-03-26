using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using RecipeBook.ApplicationData;

namespace RecipeBook.Pages;

public partial class Reg : Page
{
    public Reg()
    {
        InitializeComponent();
    }

    private async void BtnCreate_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TbUserName.Text) || string.IsNullOrWhiteSpace(TbLogin.Text) ||
                string.IsNullOrWhiteSpace(PbPassword.Password) || DpBirthDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните обязательные поля.");
                return;
            }

            if (PbPassword.Password != PbPasswordConfirm.Password)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }

            var birthDate = DpBirthDate.SelectedDate!.Value.Date;
            var age = DateTime.Today.Year - birthDate.Year - (birthDate > DateTime.Today.AddYears(- (DateTime.Today.Year - birthDate.Year)) ? 1 : 0);
            if (birthDate > DateTime.Today || age < 14)
            {
                MessageBox.Show("Некорректная дата рождения. Минимальный возраст — 14 лет.");
                return;
            }

            if (!int.TryParse(TbExp.Text, out var exp) || exp < 0)
            {
                MessageBox.Show("Стаж должен быть неотрицательным числом.");
                return;
            }

            var login = TbLogin.Text.Trim();
            var exists = await AppConnect.Model.Authors.AnyAsync(a => a.Login == login);
            if (exists)
            {
                MessageBox.Show("Логин уже занят.");
                return;
            }

            var author = new Author
            {
                UserName = TbUserName.Text.Trim(),
                Login = login,
                Password = PbPassword.Password.Trim(),
                BirthDate = birthDate,
                ExperienceYears = exp,
                Email = TbEmail.Text.Trim(),
                Phone = TbPhone.Text.Trim()
            };

            AppConnect.Model.Authors.Add(author);
            await AppConnect.Model.SaveChangesAsync();
            MessageBox.Show("Регистрация завершена.");
            AppFrame.MainFrame.Navigate(new Autorization());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка регистрации: {ex.Message}");
        }
    }

    private void BtnBack_OnClick(object sender, RoutedEventArgs e)
    {
        AppFrame.MainFrame.Navigate(new Autorization());
    }
}
