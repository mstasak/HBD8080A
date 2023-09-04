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

    private void Control_BankChanged(object sender, FrontPanelInputRowEventArgs e) {
        if (e.ButtonPresses.HasValue) { 
            
        }
        if (e.ButtonUpPresses.HasValue) {
            
        }
        if (e.ButtonDownPresses.HasValue) {
            
        }
        if (e.SwitchStates.HasValue) {
            
        }
    }
}
