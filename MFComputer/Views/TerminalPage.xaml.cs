using HBD8080A.ViewModels;

using Microsoft.UI.Xaml.Controls;
//using Microsoft.UI.Content;

namespace HBD8080A.Views;

public sealed partial class TerminalPage : Page
{

    public TerminalViewModel ViewModel
    {
        get;
    }

    public TerminalPage()
    {
        ViewModel = App.GetService<TerminalViewModel>();
        InitializeComponent();
    }

}
