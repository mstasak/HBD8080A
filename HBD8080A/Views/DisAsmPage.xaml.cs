using HBD8080A.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace HBD8080A.Views;

public sealed partial class DisAsmPage : Page
{
    public DisAsmViewModel ViewModel
    {
        get;
    }

    public DisAsmPage()
    {
        ViewModel = App.GetService<DisAsmViewModel>();
        InitializeComponent();
    }
}
