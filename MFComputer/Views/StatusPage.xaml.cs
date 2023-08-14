using MFComputer.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace MFComputer.Views;

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
