using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MFComputer.Views;
public sealed partial class SwitchButtonRow : UserControl {
    public static readonly DependencyProperty SwitchValuesProperty = DependencyProperty.Register(
    "SwitchValues", typeof(byte), typeof(SwitchButtonRow), new PropertyMetadata(null));

    public int NumControls { get; set; } = 0;
    public string ControlTypes { get; set; } = "Button,ToggleButton,ToggleSwitch";
    public string TopTitle { get; set; } = "Title";
    public string ControlLabels { get; set; } = "A7,A6,A5,A4,A3,A2,A1,A0"; //can be set from XAML, but no hot-reload available without observation?
    public int SwitchValues { get; set; } = 0;

    public SwitchBank SwitchBank0 { get; set; }

    public SwitchButtonRow() {
        InitializeComponent();
        SwitchBank0 = new SwitchBank();
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e) {
        //validate params and infer NumControls from # elements in ControlTypes if needed
        var labels = ControlLabels.Split(',');
        var types = ControlTypes.Split(",");
        if (NumControls == 0) {
            NumControls = types.Length;
        }
        else {
            Debug.Assert(types.Length == NumControls, "SwitchButtonRow: ControlTypes list count does not match NumControls");
        }
        Debug.Assert(labels.Length == NumControls, "SwitchButtonRow: ControlLabels list count does not match NumControls");

        //set up variable number of column defs
        Grid.ColumnDefinitions.Clear();
        int i;
        for (i = 0; i < NumControls; i++) {
            ColumnDefinition cDef = new() {
                Width = new GridLength(1, GridUnitType.Star) // <-- this is default width
            };
            Grid.ColumnDefinitions.Add(cDef);
        }
        

        //top title
        GridLabel.Text = TopTitle;
        Grid.SetColumnSpan(GridLabel, NumControls);

        //Grid.ColumnSpacing = 0;
        //Grid.Width = 80 * NumControls + 10;

        BitmapImage bitmapImage;
        //buttons and switches
        //Creating these on the fly did not work well - apparently XAML processing configures controls in ways not readily copied
        //in property code.  So I will make a model control of each type, and clone it into place during construction.
        //Grid.Children.Clear();
        //var ButtonBackground = new SolidColorBrush(Colors.HotPink);
        var ButtonBackground = new SolidColorBrush(Color.FromArgb(0,40,40,40));
        for (i = 0; i < NumControls; i++) {
            UIElement ctl;
            switch (types[i]) {
                case "Button":
                    var btn = new Button {
                        Background = ButtonBackground,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Tag = i
                    };
                    var PushButtonImage = new Image();
                    bitmapImage = new BitmapImage {
                        UriSource = new Uri("ms-appx:///Assets//pushbutton.png", UriKind.RelativeOrAbsolute)
                    };
                    PushButtonImage.Width = bitmapImage.DecodePixelWidth = 53; //natural px width of image source
                    PushButtonImage.Source = bitmapImage;
                    btn.Content = PushButtonImage;
                    //btn.Width = 53;
                    //btn.Height = 110;
                    ctl = btn;
                    break;
                case "ToggleSwitch":
                    var rockerSwitch = new ToggleSwitch {
                        Background = ButtonBackground,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Tag = i
                    };
                    var RockerOffImage = new Image();
                    bitmapImage = new BitmapImage {
                        UriSource = new Uri("ms-appx:///Assets//rockerswitchoff.png", UriKind.RelativeOrAbsolute)
                    };
                    RockerOffImage.Width = bitmapImage.DecodePixelWidth = 53; //natural px width of image source
                    RockerOffImage.Source = bitmapImage;
                    rockerSwitch.OffContent = RockerOffImage;
                    
                    var RockerOnImage = new Image();
                    bitmapImage = new BitmapImage {
                        UriSource = new Uri("ms-appx:///Assets//rockerswitchon.png", UriKind.RelativeOrAbsolute)
                    };
                    RockerOnImage.Width = bitmapImage.DecodePixelWidth = 53; //natural px width of image source
                    RockerOnImage.Source = bitmapImage;
                    rockerSwitch.OnContent = RockerOnImage;

                    //rockerSwitch.Width = 53;
                    //rockerSwitch.Height = 110;
                    //rockerSwitch.Background = ButtonBackground; //Had trouble with this button having a lighter background
                    ctl = rockerSwitch;
                    break;
                case "ToggleButton":
                    //var bidiButton = new ToggleButton(); //NO!  ToggleButton is not a double throw momentary contact button!
                    //I want a button which can be tapped up or down, like a car window control or an Examine/Examine Next button on
                    //an IMSAI 8080 front panel or a non-rotary light dimmer "switch"!
                    var bidiButton = new Button {
                        Background = ButtonBackground,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Tag = i
                    };
                    var BiDiRockerImage = new Image();
                    bitmapImage = new BitmapImage {
                        UriSource = new Uri("ms-appx:///Assets//bidirocker.png", UriKind.RelativeOrAbsolute)
                    };
                    BiDiRockerImage.Width = bitmapImage.DecodePixelWidth = 53; //natural px width of image source
                    BiDiRockerImage.Source = bitmapImage;
                    bidiButton.Content = BiDiRockerImage;
                    //bidiButton.Width = 53;
                    //bidiButton.Height = 110;
                    ctl = bidiButton;
                    break;
                default:
                    var btn2 = new Button {
                        Background = ButtonBackground,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Content = $"error: invalid button type '{types[i]}'",
                        Tag = i
                    };
                    ctl = btn2;
                    break;
            }
            Grid.Children.Add(ctl);
            Grid.SetRow((FrameworkElement)ctl, 1);
            Grid.SetColumn((FrameworkElement)ctl, i);
        }

        //bottom titles
        Brush whiteBrush = new SolidColorBrush(Colors.White); //the front panel is intrinsically dark themed
        for (i = 0; i < NumControls; i++) {
            TextBlock tBlk = new() {
                Text = labels[i],
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = whiteBrush
            };
            Grid.Children.Add(tBlk);
            Grid.SetRow(tBlk, 2);
            Grid.SetColumn(tBlk, i);
        }
    }
}
