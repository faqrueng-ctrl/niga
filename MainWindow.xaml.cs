using System.Windows;
using RecipeBook.ApplicationData;
using RecipeBook.Pages;

namespace RecipeBook;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AppFrame.MainFrame = MainFrm;
        MainFrm.Navigate(new Autorization());
    }
}
