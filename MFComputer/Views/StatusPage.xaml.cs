using HBD8080A.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace HBD8080A.Views;

public sealed partial class StatusPage : Page
{
    public StatusViewModel ViewModel
    {
        get;
    }

    public StatusPage()
    {
        ViewModel = App.GetService<StatusViewModel>();
        InitializeComponent();
    }
}
