using CommunityToolkit.Mvvm.ComponentModel;
using HBD8080A.Services;

namespace HBD8080A.ViewModels;

public partial class TerminalViewModel : ObservableRecipient
{

    public DumbTerminalService Terminal {
        get;
    }

    public TerminalViewModel()
    {
        Terminal = App.GetService<DumbTerminalService>();
    }

    //public void Terminal_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
    //    Terminal.IsOn = !Terminal.IsOn;
    //}

}
