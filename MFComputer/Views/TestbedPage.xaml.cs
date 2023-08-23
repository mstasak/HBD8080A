using MFComputer.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace MFComputer.Views;

public sealed partial class TestbedPage : Page
{
    public TestbedViewModel ViewModel
    {
        get;
    }

    public TestbedPage()
    {
        ViewModel = App.GetService<TestbedViewModel>();
        InitializeComponent();
    }
}
