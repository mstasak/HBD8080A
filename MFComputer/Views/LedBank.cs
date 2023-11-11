using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json.Linq;
using Windows.UI;

namespace HBD8080A.Views;

/// <summary>
/// Storage class for a row of up to 8 LEDs.
/// Supports Observation for XAML x:Bind
/// Exposes a brush property for each LED, which toggles with its data bit value (on=Red, off=Black)
/// </summary>
public class LedBank : ObservableObject {
    public static Lazy<Brush> OnBrush { get; set; }
    public static Lazy<Brush> OffBrush { get; set; }

    private static Brush InitBrush(Color color)
    {
        return new SolidColorBrush(color);
    }

    static LedBank() {
        OffBrush = new Lazy<Brush>(InitBrush(Colors.Black));
        OnBrush = new Lazy<Brush>(InitBrush(Colors.Red));
    }


    private byte ledValues;
    public byte LedValues {
        get => ledValues;
        set {
            var oldValue = ledValues;
            ledValues = value;
            if ((oldValue & 0x80) != (value & 0x80)) { OnPropertyChanged(nameof(LedBrush7)); } //LedBrush7 property changed
            if ((oldValue & 0x40) != (value & 0x40)) { OnPropertyChanged(nameof(LedBrush6)); } //LedBrush6 property changed
            if ((oldValue & 0x20) != (value & 0x20)) { OnPropertyChanged(nameof(LedBrush5)); } //LedBrush5 property changed
            if ((oldValue & 0x10) != (value & 0x10)) { OnPropertyChanged(nameof(LedBrush4)); } //LedBrush4 property changed
            if ((oldValue & 0x08) != (value & 0x08)) { OnPropertyChanged(nameof(LedBrush3)); } //LedBrush3 property changed
            if ((oldValue & 0x04) != (value & 0x04)) { OnPropertyChanged(nameof(LedBrush2)); } //LedBrush2 property changed
            if ((oldValue & 0x02) != (value & 0x02)) { OnPropertyChanged(nameof(LedBrush1)); } //LedBrush1 property changed
            if ((oldValue & 0x01) != (value & 0x01)) { OnPropertyChanged(nameof(LedBrush0)); } //LedBrush0 property changed
        }
    }
    public Brush LedBrush7 => ((ledValues & 0x80) != 0) ? OnBrush.Value : OffBrush.Value;
    public Brush LedBrush6 => ((ledValues & 0x40) != 0) ? OnBrush.Value : OffBrush.Value;
    public Brush LedBrush5 => ((ledValues & 0x20) != 0) ? OnBrush.Value : OffBrush.Value;
    public Brush LedBrush4 => ((ledValues & 0x10) != 0) ? OnBrush.Value : OffBrush.Value;
    public Brush LedBrush3 => ((ledValues & 0x08) != 0) ? OnBrush.Value : OffBrush.Value;
    public Brush LedBrush2 => ((ledValues & 0x04) != 0) ? OnBrush.Value : OffBrush.Value;
    public Brush LedBrush1 => ((ledValues & 0x02) != 0) ? OnBrush.Value : OffBrush.Value;
    public Brush LedBrush0 => ((ledValues & 0x01) != 0) ? OnBrush.Value : OffBrush.Value;
    public LedBank() {
    }
}
