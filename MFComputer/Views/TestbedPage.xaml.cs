using HBD8080A.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace HBD8080A.Views;

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

    private void Button1_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        var hexStr = this.txtLEDValue.Text;
        var byteVal = Convert.ToByte(hexStr, 16); //hex to int conversion

    }
    private void Button2_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {

    }
    private void Button3_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {

    }
    private void Button4_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {

    }
    private void Button5_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {

    }
    private void Button6_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {

    }
}
