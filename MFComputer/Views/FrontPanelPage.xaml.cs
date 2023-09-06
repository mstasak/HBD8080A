using MFComputer.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace MFComputer.Views;

public sealed partial class FrontPanelPage : Page
{
    public FrontPanelViewModel ViewModel
    {
        get;
    }

    public FrontPanelPage()
    {
        ViewModel = App.GetService<FrontPanelViewModel>();
        InitializeComponent();
    }

}
