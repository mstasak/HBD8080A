using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public static readonly DependencyProperty SwitchValuesProperty = DependencyProperty.Register(
        "SwitchValues", typeof(byte), typeof(SwitchRow), new PropertyMetadata(null));

    //private byte switchValues;
    public byte SwitchValues {
        get {
            Debug.WriteLine($"In SwitchValues getter: DepProp value = 0x{(byte)GetValue(SwitchValuesProperty):2x}, SwitchBank value = 0x{SwitchBank0.SwitchValues:2x}");
            var currentValue = (byte)GetValue(SwitchValuesProperty);
            if (currentValue != SwitchBank0.SwitchValues) {
                SwitchValues = SwitchBank0.SwitchValues;
            }
            return (byte)GetValue(SwitchValuesProperty);
        }
        set {
            SwitchBank0.SwitchValues = value;
            SetValue(SwitchValuesProperty, value);
        }
    }
    public string TopTitle { get; set; } = "Title";
    public string ButtonLabels { get; set; } = "A7,A6,A5,A4,A3,A2,A1,A0"; //can be set from XAML, but no hot-reload available without observation?
    public int NumSwitches { get; set; } = 8;

    private SwitchBank SwitchBank0 { get; set; }

    /*
        _testClass.OnUpdateStatus += new TestClass.StatusUpdateHandler(UpdateStatus);
    */

    public SwitchRow() {
        InitializeComponent();
        SwitchBank0 = new SwitchBank();
        SwitchBank0.OnSwitchBankChanged += SwitchBank0_OnSwitchBankChanged;
    }

    private void SwitchBank0_OnSwitchBankChanged(object sender, SwitchBank.SwitchBankChangedEventArgs e) {
        SwitchValues = e.SwitchStates;
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e) {
        var labels = ButtonLabels.Split(',');
        var i = 0;
        foreach (var tblock in Grid.Children.Where(e => Grid.GetRow((FrameworkElement)e) == 2).OfType<TextBlock>()) {
            tblock.Text = labels[i++];
        }

        //remove extra switches if needed
        var switches =  Grid.Children.Where(e => Grid.GetRow((FrameworkElement)e) == 1).OfType<ToggleSwitch>().ToArray();
        if (NumSwitches < 8) { 
            var topLabels = Grid.Children.Where(e => Grid.GetRow((FrameworkElement)e) == 0).OfType<TextBlock>().ToArray();
            var bottomLabels = Grid.Children.Where(e => Grid.GetRow((FrameworkElement)e) == 2).OfType<TextBlock>().ToArray();
            for (i = NumSwitches; i < 8; i++) {
                Grid.Children.Remove(switches[i]);
                Grid.Children.Remove(bottomLabels[i]);
            }
            
            foreach (var label in topLabels) {
                Grid.SetRowSpan(label, NumSwitches);
            }
            Grid.Width = 600 * NumSwitches / 8;
        }
        //for (i = 0; i < NumSwitches; i++) {
        //    var Switch = switches[i];
        //    if (Switch is ToggleSwitch toggleSwitch) {
        //        var b = new Binding();
        //        b.Source = i switch {
        //            0 => SwitchBank0.Switch0On,
        //            1 => SwitchBank0.Switch1On,
        //            2 => SwitchBank0.Switch2On,
        //            3 => SwitchBank0.Switch3On,
        //            4 => SwitchBank0.Switch4On,
        //            5 => SwitchBank0.Switch5On,
        //            6 => SwitchBank0.Switch6On,
        //            7 => SwitchBank0.Switch7On,
        //            _ => null
        //        };
        //        b.Mode = BindingMode.TwoWay;
        //        b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        //        toggleSwitch.SetBinding(ToggleSwitch.IsOnProperty, b);
        //    }
        //}
    }
}
