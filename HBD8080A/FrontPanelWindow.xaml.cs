using HBD8080A.Helpers;

namespace HBD8080A;

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
