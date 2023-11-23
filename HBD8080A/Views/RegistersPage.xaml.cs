using HBD8080A.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace HBD8080A.Views;

public sealed partial class RegistersPage : Page
{
    public RegistersViewModel ViewModel
    {
        get;
    }

    public RegistersPage()
    {
        ViewModel = App.GetService<RegistersViewModel>();
        InitializeComponent();
    }
}
