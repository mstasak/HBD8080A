using MFComputer.Helpers;

namespace MFComputer;

public sealed partial class FrontPanelWindow : WindowEx
{
    public FrontPanelWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
    }
}
