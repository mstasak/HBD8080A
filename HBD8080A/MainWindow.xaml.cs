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
        Title = "???"; //"AppDisplayName".GetLocalized();
        Closed += MainWindow_Closed;
        //SystemBackdrop = Microsoft.UI.Xaml.Media.MicaBackdrop;
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            Microsoft.UI.Xaml.Media.MicaBackdrop micaBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            bool useMicaAlt = false;
            micaBackdrop.Kind = useMicaAlt ? Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt : Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
            this.SystemBackdrop = micaBackdrop;
        }
    }

    private void MainWindow_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args) {
        App.GetService<IActivationService>().CloseOtherWindows();
    }
}
