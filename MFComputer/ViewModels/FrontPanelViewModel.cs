using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using MFComputer.Hardware.Computer;
using MFComputer.Services;
using MFComputer.Views;

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
    public ComputerSystemService Computer {
        get;
    }
    public Cpu8080A Cpu {
        get;
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
        Computer = App.GetService<ComputerSystemService>();
        Cpu = Computer.Cpu;
    }

    public void Increment_Output_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        OutputLEDs = (byte)(OutputLEDs + 1);
    }

    public void LEDLoop_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        OutputLEDs = (byte)(OutputLEDs + 1);
        Computer.Stop();
        Computer.Reset();
        ushort pc = 0;
        Cpu.Memory[pc++] = 0xDB; // in $ff
        Cpu.Memory[pc++] = 0xFF;
        Cpu.Memory[pc++] = 0xD3; // out $ff
        Cpu.Memory[pc++] = 0xFF;
        Cpu.Memory[pc++] = 0xC3; // jmp 0
        Cpu.Memory[pc++] = 0x00;
        Cpu.Memory[pc++] = 0x00;
        Computer.Run();
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

    public void ControlBank_Changed(object sender, FrontPanelInputRowEventArgs e) {
        if (e.ButtonPresses.HasValue) {
            Debug.WriteLine($"simple button press: {e.ButtonPresses.Value}");
            var buttons = e.ButtonPresses.Value;
            if ((buttons & 0x10) != 0) { // single step
            }
            if ((buttons & 0x08) != 0) { // reset
            }
        }
        if (e.ButtonUpPresses.HasValue) {
            Debug.WriteLine($"toggle button up press: {e.ButtonUpPresses.Value}");
            var buttons = e.ButtonUpPresses.Value;
            if (((buttons & 0x80) != 0) && (! Cpu.IsRunning)) { // examine toggle up
                Cpu.PC = (ushort)((AddressHighInputSwitches << 8) | AddressLowDataSwitches);
                AddressHighLEDs = (byte)(Cpu.PC >> 8);
                AddressLowLEDs = (byte)(Cpu.PC & 0xff);
                RefreshMemoryDataDisplay();
            }
            if (((buttons & 0x40) != 0) && (! Cpu.IsRunning)) { // deposit toggle up
                Cpu.Memory[Cpu.PC] = AddressLowDataSwitches;
                RefreshMemoryDataDisplay();
            }
            if ((buttons & 0x20) != 0) { // run toggle up
            }
        }
        if (e.ButtonDownPresses.HasValue) {
            Debug.WriteLine($"toggle button down press: {e.ButtonDownPresses.Value}");
            var buttons = e.ButtonDownPresses.Value;
            if (((buttons & 0x80) != 0) && (! Cpu.IsRunning)) { // examine next toggle down
                unchecked {
                    Cpu.PC++;
                }
                AddressHighLEDs = (byte)(Cpu.PC >> 8);
                AddressLowLEDs = (byte)(Cpu.PC & 0xff);
                RefreshMemoryDataDisplay();
            }
            if (((buttons & 0x40) != 0) && (! Cpu.IsRunning)) { // deposit next toggle down
                unchecked {
                    Cpu.PC++;
                }
                AddressHighLEDs = (byte)(Cpu.PC >> 8);
                AddressLowLEDs = (byte)(Cpu.PC & 0xff);
                Cpu.Memory[Cpu.PC] = AddressLowDataSwitches;
                RefreshMemoryDataDisplay();
            }
            if ((buttons & 0x20) != 0) { // stop toggle down
            }
        }
        
        // persistent switch states are not useful without prior value to compare to;
        // bindings already keep the property value updated.
        //if (e.SwitchStates.HasValue) {
        //    if ((e.SwitchStates.Value & 0x01) != (ControlSwitches & 0x01)) {
        //        Debug.WriteLine($"switch value changed: {e.SwitchStates.Value}");
        //    //if ((buttons & 0x04) != 0) { // On/Off toggle
        //    //}
        //    }
        //}
    }

    private void RefreshMemoryDataDisplay() {
        MemoryDataLEDs = Cpu.Memory[Cpu.PC];
    }
}
