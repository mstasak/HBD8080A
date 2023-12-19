using System.Text;
using HBD8080A.Hardware.Computer;
using HBD8080A.Services;
using HBD8080A.ViewModels;
using KGySoft.CoreLibraries;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;

namespace HBD8080A.Views;

public sealed partial class DisplayAdapterPage : Page
{
    public DisplayAdapterViewModel ViewModel
    {
        get;
    }
    //public ComputerSystemService Computer {
    //    get;
    //}
    //public Cpu8080A Cpu {
    //    get;
    //}

    public DisplayAdapterPage()
    {
        ViewModel = App.GetService<DisplayAdapterViewModel>();
        //Computer = App.GetService<ComputerSystemService>();
        //Cpu = Computer.Cpu;
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        //var img = AdapterCanvas.
        //shpImage1.Source = new BitmapImage(new Uri("ms-appx:///Assets/bidirocker.png"));
        //var isrc = shpImage1.Source;
        //var gr = isrc.
    }

    private void shpImage1_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        var imgSrc = shpImage1.Convert<Bitmap>();
    }
}
