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
using Newtonsoft.Json.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MFComputer.Views;
public sealed partial class SwitchButtonRow : UserControl {
    public delegate void PanelChangedHandler(object sender, FrontPanelInputRowEventArgs e);
    public event PanelChangedHandler? OnPanelChanged;
    public static readonly DependencyProperty SwitchValuesProperty = DependencyProperty.Register(
    "SwitchValues", typeof(byte), typeof(SwitchButtonRow), new PropertyMetadata(null));

    public byte SwitchValues {
        get {
            //Debug.WriteLine($"In SwitchValues getter: DepProp value = 0x{(byte)GetValue(SwitchValuesProperty):2x}, SwitchBank value = 0x{SwitchBank0.SwitchValues:2x}");
            var currentValue = (byte)GetValue(SwitchValuesProperty);
            if (currentValue != SwitchBank0.SwitchRowValues) {
                SwitchValues = SwitchBank0.SwitchRowValues;
            }
            return (byte)GetValue(SwitchValuesProperty);
        }
        set {
            SwitchBank0.SwitchRowValues = value;
            SetValue(SwitchValuesProperty, value);
        }
    }

    public string ControlTypes { get; set; } = "Button,ToggleButton,ToggleSwitch";
    public string TopTitle { get; set; } = "Title";
    public string ControlLabels { get; set; } = "A7,A6,A5,A4,A3,A2,A1,A0"; //can be set from XAML, but no hot-reload available without observation?
    public int NumControls { get; set; } = 0;
    
    public FrontPanelInputRowHelper SwitchBank0 { get; set; }

    public SwitchButtonRow() {
        InitializeComponent();
        SwitchBank0 = new FrontPanelInputRowHelper();
        SwitchBank0.OnFrontPanelInputRowChanged += SwitchBank0_OnSwitchBankChanged;
    }

    private void SwitchBank0_OnSwitchBankChanged(object sender, FrontPanelInputRowEventArgs e) {
        if (e.SwitchStates.HasValue) {
            if (SwitchValues != e.SwitchStates.Value) {
                SwitchValues = e.SwitchStates.Value;
                //raise an event for SwitchButtonRow's owner? Or just rely on property changed? Think I heard binding code can bypass accessors, so probably need event.
                OnPanelChanged?.Invoke(this, e);
            }
        }
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
                    btn.Click += Btn_Click;
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
                    Grid.Children.Add(ctl);
                    Grid.SetRow((FrameworkElement)ctl, 1);
                    Grid.SetColumn((FrameworkElement)ctl, i);
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

                    //bind switch[0-7].IsOn to switchbank0.Switch[7-0]On
                    var b = new Binding {
                        Source = SwitchBank0,
                        Path = i switch {
                            0 => new PropertyPath(nameof(SwitchBank0.Switch7On)),
                            1 => new PropertyPath(nameof(SwitchBank0.Switch6On)),
                            2 => new PropertyPath(nameof(SwitchBank0.Switch5On)),
                            3 => new PropertyPath(nameof(SwitchBank0.Switch4On)),
                            4 => new PropertyPath(nameof(SwitchBank0.Switch3On)),
                            5 => new PropertyPath(nameof(SwitchBank0.Switch2On)),
                            6 => new PropertyPath(nameof(SwitchBank0.Switch1On)),
                            7 => new PropertyPath(nameof(SwitchBank0.Switch0On)),
                            _ => null
                        },
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    rockerSwitch.SetBinding(ToggleSwitch.IsOnProperty, b);
                    rockerSwitch.Toggled += RockerSwitch_Toggled;

                    ctl = rockerSwitch;
                    Grid.Children.Add(ctl);
                    Grid.SetRow((FrameworkElement)ctl, 1);
                    Grid.SetColumn((FrameworkElement)ctl, i);
                    break;
                case "ToggleButton":
                    //var bidiButton = new ToggleButton(); //NO!  ToggleButton is not a double throw momentary contact button!
                    //I want a button which can be tapped up or down, like a car window control or an Examine/Examine Next button on
                    //an IMSAI 8080 front panel or a non-rotary light dimmer "switch"!
                    //var bidiButton = new Button {
                    //    Background = ButtonBackground,
                    //    HorizontalAlignment = HorizontalAlignment.Center,
                    //    BorderThickness = new Thickness(0),
                    //    Padding = new Thickness(0),
                    //    Margin = new Thickness(0),
                    //    Tag = i
                    //};
                    //var BiDiRockerImage = new Image();
                    //bitmapImage = new BitmapImage {
                    //    UriSource = new Uri("ms-appx:///Assets//bidirocker.png", UriKind.RelativeOrAbsolute)
                    //};
                    //BiDiRockerImage.Width = bitmapImage.DecodePixelWidth = 53; //natural px width of image source
                    //BiDiRockerImage.Source = bitmapImage;
                    //bidiButton.Content = BiDiRockerImage;
                    ////bidiButton.Width = 53;
                    ////bidiButton.Height = 110;
                    //ctl = bidiButton;
                    //Grid.Children.Add(ctl);
                    //Grid.SetRow((FrameworkElement)ctl, 1);
                    //Grid.SetColumn((FrameworkElement)ctl, i);

                    var upBitmapImage = new BitmapImage {
                        UriSource = new Uri("ms-appx:///Assets//rockerupbright.png", UriKind.RelativeOrAbsolute),
                        DecodePixelWidth = 53
                    };
                    var upRockerImage = new Image() {
                        Width = 53, //natural px width of image source
                        Source = upBitmapImage
                    };
                    var upButton = new Button {
                        Background = ButtonBackground,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Tag = i,
                        Content = upRockerImage
                    };
                    upButton.Click += UpButton_Click;

                    var downBitmapImage = new BitmapImage {
                        UriSource = new Uri("ms-appx:///Assets//rockerdownbright.png", UriKind.RelativeOrAbsolute),
                        DecodePixelWidth = 53
                    };
                    var downRockerImage = new Image() {
                        Width = 53, //natural px width of image source
                        Source = downBitmapImage
                    };
                    var downButton = new Button {
                        Background = ButtonBackground,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        BorderThickness = new Thickness(0),
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Tag = i + 8,
                        Content = downRockerImage
                    };
                    downButton.Click += DownButton_Click;
                    var ButtonContainer = new StackPanel() {
                        Orientation = Orientation.Vertical,
                        Spacing = 0,
                        Width = 53,
                        Height = 110,
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        BorderThickness = new Thickness(0),
                    };
                    ButtonContainer.Children.Add(upButton);
                    ButtonContainer.Children.Add(downButton);
                    ctl = ButtonContainer;
                    Grid.Children.Add(ctl);
                    Grid.SetRow((FrameworkElement)ctl, 1);
                    Grid.SetColumn((FrameworkElement)ctl, i);
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
                    Grid.Children.Add(ctl);
                    Grid.SetRow((FrameworkElement)ctl, 1);
                    Grid.SetColumn((FrameworkElement)ctl, i);
                    break;
            }
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

    private void RockerSwitch_Toggled(object sender, RoutedEventArgs e) {
        var tag = (int)((FrameworkElement)sender).Tag; //8-15 from left
        OnPanelChanged?.Invoke(
            this, 
            new FrontPanelInputRowEventArgs(Switches: SwitchValues, 
                Presses: null, 
                UpPresses: null, 
                DownPresses: null));
    }

    private void DownButton_Click(object sender, RoutedEventArgs e) {
        //Debug.WriteLine($"Down Button was clicked by button with tag {((FrameworkElement)sender).Tag}");
        // Make sure someone is listening to event
        var tag = (int)((FrameworkElement)sender).Tag; //8-15 from left
        OnPanelChanged?.Invoke(
            this, 
            new FrontPanelInputRowEventArgs(Switches: SwitchValues, 
                Presses: null, 
                UpPresses: null, 
                DownPresses: (byte)(0x80 >> (tag - 8))));
    }

    private void UpButton_Click(object sender, RoutedEventArgs e) {
        //Debug.WriteLine($"Up Button was clicked by button with tag {((FrameworkElement)sender).Tag}");
        var tag = (int)((FrameworkElement)sender).Tag; //0-7 from left
        OnPanelChanged?.Invoke(
            this, 
            new FrontPanelInputRowEventArgs(Switches: SwitchValues, 
                Presses: null, 
                UpPresses: (byte)(0x80 >> (tag)), 
                DownPresses: null));
    }

    private void Btn_Click(object sender, RoutedEventArgs e) {
        //Debug.WriteLine($"Button was clicked by button with tag {((FrameworkElement)sender).Tag}");
        var tag = (int)((FrameworkElement)sender).Tag; //0-7 from left
        OnPanelChanged?.Invoke(
            this, 
            new FrontPanelInputRowEventArgs(Switches: SwitchValues, 
                Presses: (byte)(0x80 >> (tag)),
                UpPresses: null, 
                DownPresses: null));
    }
}
