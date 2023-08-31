using CommunityToolkit.Mvvm.ComponentModel;

namespace MFComputer.ViewModels;

public class FrontPanelViewModel : ObservableRecipient
{
    private byte outputLEDs;
    private byte flagLEDs;
    private byte addressHighLEDs;
    private byte addressLowLEDs;
    private byte memoryDataLEDs;
    private byte statusLEDs;
    private byte addressHighInputSwitches;
    private byte addressLowDataSwitches;
    private byte controlSwitches;

    public byte OutputLEDs {
        get => outputLEDs;
        set => SetProperty(ref outputLEDs, value, nameof(OutputLEDs));
    }
    public byte FlagLEDs {
        get => flagLEDs;
        set => SetProperty(ref flagLEDs, value);
    }
    public byte AddressHighLEDs {
        get => addressHighLEDs;
        set => SetProperty(ref addressHighLEDs, value);
    }
    public byte AddressLowLEDs {
        get => addressLowLEDs;
        set => SetProperty(ref addressLowLEDs, value);
    }
    public byte MemoryDataLEDs {
        get => memoryDataLEDs;
        set => SetProperty(ref memoryDataLEDs, value);
    }
    public byte StatusLEDs {
        get => statusLEDs;
        set => SetProperty(ref statusLEDs, value);
    }
    public byte AddressHighInputSwitches {
        get => addressHighInputSwitches;
        set => SetProperty(ref addressHighInputSwitches, value);
    }
    public byte AddressLowDataSwitches {
        get => addressLowDataSwitches;
        set => SetProperty(ref addressLowDataSwitches, value);
    }
    public byte ControlSwitches {
        get => controlSwitches;
        set => SetProperty(ref controlSwitches, value);
    }

    public FrontPanelViewModel()
    {
        OutputLEDs = 0; // 0x38;
        AddressHighLEDs = 0; // 0xF0;
        AddressLowLEDs = 0; // 0x03;
    }

    public void Increment_Output_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        OutputLEDs = (byte)(OutputLEDs + 1);
    }

    public void Increment_HighAddress_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        AddressHighLEDs = (byte)(AddressHighLEDs + 1);
    }

    public void Increment_Flags_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        FlagLEDs = (byte)(FlagLEDs + 4);
    }
    public void Increment_Data_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        MemoryDataLEDs = (byte)(MemoryDataLEDs + 1);
    }
    public void Decrement_LowAddress_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        AddressLowLEDs = (byte)(AddressLowLEDs - 1);
    }

    private string switchString = "click to fetch values";
    public string SwitchString {
        get => switchString;
        set => SetProperty(ref switchString, value, nameof(SwitchString));
    }
    public void FetchSwitches_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        //AddressHighInputSwitches = 0xfa;
        var s = $"{AddressHighInputSwitches:x2} {AddressLowDataSwitches:x2} {ControlSwitches:x2}";
        SwitchString = s;
    }

}
