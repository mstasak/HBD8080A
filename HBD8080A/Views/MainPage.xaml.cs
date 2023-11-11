using HBD8080A.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace HBD8080A.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
