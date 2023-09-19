using MFComputer.ViewModels;

using Microsoft.UI.Xaml.Controls;
//using Microsoft.UI.Content;

namespace MFComputer.Views;

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
