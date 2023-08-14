using MFComputer.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace MFComputer.Views;

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
