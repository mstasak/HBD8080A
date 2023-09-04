using MFComputer.Contracts.Services;
using MFComputer.Helpers;
using WinUIEx;

namespace MFComputer;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
        Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args) {
        App.GetService<IActivationService>().CloseOtherWindows();
    }
}
