using HBD8080A.Contracts.Services;
using HBD8080A.Helpers;
using WinUIEx;

namespace HBD8080A;

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
