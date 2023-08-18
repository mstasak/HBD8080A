using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MFComputer.Views;
public sealed partial class SwitchRow : UserControl {
    public string TopTitle { get; set; } = "Title";
    public string ButtonLabels { get; set; } = "A7,A6,A5,A4,A3,A2,A1,A0"; //can be set from XAML, but no hot-reload available without observation?
    public SwitchRow() {
        this.InitializeComponent();
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e) {
        var labels = ButtonLabels.Split(',');
        var i = 0;
        foreach (var tblock in Grid.Children.Where(e => Grid.GetRow((FrameworkElement)e) == 2).OfType<TextBlock>()) {
            tblock.Text = labels[i++];
        }
    }
}
