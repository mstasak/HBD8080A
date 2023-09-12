using System.Diagnostics;
using System.Reflection.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using MFComputer.Hardware.Computer;
using MFComputer.Services;
using MFComputer.Views;
using Microsoft.UI.Dispatching;

namespace MFComputer.ViewModels;

public class FrontPanelViewModel : ObservableRecipient {
    private byte outputLEDs;
    private byte flagLEDs;
    private byte addressHighLEDs;
    private byte addressLowLEDs;
    private byte memoryDataLEDs;
    private byte statusLEDs;
    private byte addressHighInputSwitches;
    private byte addressLowDataSwitches;
    private byte controlSwitches;
    public DispatcherQueue FrontPanelDispatcherQueue { // used by CPU8080A and/or ComputerSystemService code to access FP port values on UI thread
        get; private set;
    } = DispatcherQueue.GetForCurrentThread(); //(ViewModel is constructed in call from UI thread)

    private DispatcherQueueTimer RepeatingTimer {
        get; set;
    }
    private bool isFast = false;

    private enum PanelState {
        Off,
        Paused,
        Running,
        Frozen
    }
    private PanelState panelState = PanelState.Off;
    private int PanelUpdateMSec = 250;

    public byte OutputLEDs {
        get => outputLEDs;
        set => SetProperty(ref outputLEDs, value);
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
        set {
            SetProperty(ref addressHighInputSwitches, value);
            Cpu8080A.InPortFF = value;
        }
    }
    public byte AddressLowDataSwitches {
        get => addressLowDataSwitches;
        set => SetProperty(ref addressLowDataSwitches, value);
    }
    public byte ControlSwitches {
        get => controlSwitches;
        set => SetProperty(ref controlSwitches, value);
    }

    public ComputerSystemService Computer {
        get;
    }
    public Cpu8080A Cpu {
        get;
    }

    public FrontPanelViewModel() {
        OutputLEDs = 0;
        AddressHighLEDs = 0;
        AddressLowLEDs = 0;
        MemoryDataLEDs = 0;
        FlagLEDs = 0;
        StatusLEDs = 0;

        Computer = App.GetService<ComputerSystemService>();
        Cpu = Computer.Cpu;
        ConfigureRefresh(false);
        //Cpu.Outputter += PortOutput;
        //Cpu.Inputter += PortInput; //instead we push switch input values into Cpu8080A.InPortFF field
        //Debug.WriteLine($"FrontPanelViewModel on thread \"{Thread.CurrentThread.Name}\", #{Thread.CurrentThread.ManagedThreadId}");
    }
    //private void PortInput(byte port, Cpu8080A.InputReceiver handler) {

    //    var dq = App.MainWindow.DispatcherQueue;
    //    if (dq.HasThreadAccess) {
    //        if (port == 0xff) {
    //            handler(port: 0xff, handled: true, value: AddressHighInputSwitches);
    //            //return AddressHighInputSwitches;
    //        }
    //        else {
    //            handler(port: 0xff, handled: false, value: 0xff);
    //            //return 0xff;
    //        }
    //    } else {
    //        if (port == 0xff) {
    //            dq.TryEnqueue(
    //                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
    //                () => handler(port: 0xff, handled: true, value: AddressHighInputSwitches)
    //            );
    //            //return AddressHighInputSwitches;
    //        }
    //        else {
    //            dq.TryEnqueue(
    //                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
    //                () => handler(port: 0xff, handled: false, value: 0xff)
    //            );
    //            //return 0xff;
    //        }
    //    }
    //}

    //Currently this is not used; output port ff has a value stored in the CPU object as LatchedOutputValues[0xff]
    //a running program should periodically (no more than say 60 times per second?) check this value and copy it to the output leds.
    private void PortOutput(byte port, byte value) {
        if (port == 0xff) {
            OutputLEDs = value;
        }
    }

    public void LEDLoop_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        //OutputLEDs = (byte)(OutputLEDs + 1);
        if (Cpu.CurrentState != Cpu8080A.CpuState.Stopped) {
            return;
        }
        Cpu.Reset();
        ushort pc = 0;

        //dumb loop - in switches, out leds, repeat
        //Cpu.Memory[pc++] = 0xDB; // in 0FFH
        //Cpu.Memory[pc++] = 0xFF;
        //Cpu.Memory[pc++] = 0xD3; // out 0FFH
        //Cpu.Memory[pc++] = 0xFF;
        //Cpu.Memory[pc++] = 0xC3; // jmp 0
        //Cpu.Memory[pc++] = 0x00;
        //Cpu.Memory[pc++] = 0x00;

        //slightly more complex - rotate a light from right to left endlessly, pacing rotation based on input switches
        Cpu.Memory[pc++] = 0x16; // 0000:   mvi d,01H ; value to display
        Cpu.Memory[pc++] = 0x01; //
        Cpu.Memory[pc++] = 0xDB; // 0002:   in 0FFH   ; get switches (delay loop count value)
        Cpu.Memory[pc++] = 0xFF; //
        Cpu.Memory[pc++] = 0x47; // 0004:   mov b,a   ; save for reuse
        Cpu.Memory[pc++] = 0x7A; // 0005:   mov a,d   ; get display value
        Cpu.Memory[pc++] = 0x07; // 0006:   rlc       ; rotate
        Cpu.Memory[pc++] = 0xD3; // 0007:   out 0FFH  ; display
        Cpu.Memory[pc++] = 0xFF; //
        Cpu.Memory[pc++] = 0x57; // 0009:   mov d,a   ; save new value
        Cpu.Memory[pc++] = 0x48; // 000A:   mov c,b   ; copy the delay count to a temp register
        Cpu.Memory[pc++] = 0x0D; // 000B:   dcr c     ; decrement
        Cpu.Memory[pc++] = 0xC2; // 000C:   jnz 000BH ; repeat until zero
        Cpu.Memory[pc++] = 0x0B; //
        Cpu.Memory[pc++] = 0x00; //
        Cpu.Memory[pc++] = 0xC3; // 000F:   jmp 0002H ; refresh delay count and repeat rotate
        Cpu.Memory[pc++] = 0x02; //
        Cpu.Memory[pc++] = 0x00; //
    }

    public void ControlBank_Changed(object sender, FrontPanelInputRowEventArgs e) {
        if (e.ButtonPresses.HasValue) {
            //Debug.WriteLine($"simple button press: {e.ButtonPresses.Value}");
            var buttons = e.ButtonPresses.Value;
            if ((buttons & 0x10) != 0) { // single step
                Cpu.SingleStep();
            }
            if ((buttons & 0x08) != 0) { // reset
                Cpu.RequestedState = Cpu8080A.CpuState.Reset;
            }
        }
        if (e.ButtonUpPresses.HasValue) {
            //Debug.WriteLine($"toggle button up press: {e.ButtonUpPresses.Value}");
            var buttons = e.ButtonUpPresses.Value;
            if (((buttons & 0x80) != 0) && (Cpu.CurrentState == Cpu8080A.CpuState.Stopped)) { // examine toggle up
                Cpu.PC = (ushort)((AddressHighInputSwitches << 8) | AddressLowDataSwitches);
            }
            if (((buttons & 0x40) != 0) && (Cpu.CurrentState == Cpu8080A.CpuState.Stopped)) { // deposit toggle up
                Cpu.Memory[Cpu.PC] = AddressLowDataSwitches;
                //RefreshMemoryDataDisplay();
            }
            if ((buttons & 0x20) != 0) { // run toggle up
                Cpu.RequestedState = Cpu8080A.CpuState.Running;
                ConfigureRefresh(true);
            }
        }
        if (e.ButtonDownPresses.HasValue) {
            Debug.WriteLine($"toggle button down press: {e.ButtonDownPresses.Value}");
            var buttons = e.ButtonDownPresses.Value;
            if (((buttons & 0x80) != 0) && (Cpu.CurrentState == Cpu8080A.CpuState.Stopped)) { // examine next toggle down
                unchecked {
                    Cpu.PC++;
                }
            }
            if (((buttons & 0x40) != 0) && (Cpu.CurrentState == Cpu8080A.CpuState.Stopped)) { // deposit next toggle down
                unchecked {
                    Cpu.PC++;
                }
                Cpu.Memory[Cpu.PC] = AddressLowDataSwitches;
            }
            if ((buttons & 0x20) != 0) { // stop toggle down
                Cpu.RequestedState = Cpu8080A.CpuState.Stopped;
            }
        }

        // persistent switch states are not useful without prior value to compare to;
        // bindings already keep the property value updated.
        if (e.SwitchStates.HasValue) {
            //if ((e.SwitchStates.Value & 0x04) != (ControlSwitches & 0x04)) {
            //        Debug.WriteLine($"switch value changed: {e.SwitchStates.Value}");
            if ((e.SwitchStates & 0x04) != 0) { // On/Off toggle
                if (Cpu.CurrentState == Cpu8080A.CpuState.Off) {
                    Cpu.RequestedState = Cpu8080A.CpuState.On;
                }
            } else if ((e.SwitchStates & 0x04) == 0) {
                    Cpu.RequestedState = Cpu8080A.CpuState.Off;
            }

            //}
        }
    }

    private void ConfigureRefresh(bool fast) {
        //set to fast pace when running, slow when stopped or off
        if (RepeatingTimer is null) {
            RepeatingTimer = FrontPanelDispatcherQueue.CreateTimer();
            // The tick handler will be invoked repeatedly after every 5
            // seconds on the dedicated thread.
            RepeatingTimer.Tick += (s, e) => {
                RefreshFrontPanel();
            };
        }
        if (fast) {
            RepeatingTimer.Interval = TimeSpan.FromMicroseconds(1000 * 16);
        } else {
            RepeatingTimer.Interval = TimeSpan.FromMicroseconds(1000 * 100);
        }
        if (!RepeatingTimer.IsRunning) {
            // Start the Timer
            RepeatingTimer.Start();
        }
    }

    private void RefreshFrontPanel() {
        var pc = Cpu.PC;
        var flags = Cpu.Flags;
        var intEnb = Cpu.IsInterruptsEnabled;
        //var isFast = RepeatingTimer.Interval < TimeSpan.FromMicroseconds(1000 * 100);
        MemoryDataLEDs = Cpu.Memory[pc];
        AddressHighLEDs = (byte)(pc >> 8);
        AddressLowLEDs = (byte)(pc & 0xff);
        OutputLEDs = Cpu.LatchedOutputValues[0xff];

        //Int Enabled,Zero,Sign,Parity,Carry,Aux Carry
        FlagLEDs = (byte)(
            (intEnb ? 0x80 : 0x00) |
            (((flags & Cpu8080A.zeroflag) != 0) ? 0x40 : 0x00) |
            (((flags & Cpu8080A.signflag) != 0) ? 0x20 : 0x00) |
            (((flags & Cpu8080A.parityflag) != 0) ? 0x10 : 0x00) |
            (((flags & Cpu8080A.carryflag) != 0) ? 0x08 : 0x00) |
            (((flags & Cpu8080A.auxcarryflag) != 0) ? 0x04 : 0x00)
        );

        //Turbo,Halt,Power
        StatusLEDs = (byte)(
            (Cpu.IsTurbo ? 0x80 : 0x00) |
            (((Cpu.CurrentState == Cpu8080A.CpuState.Running ) || (Cpu.CurrentState == Cpu8080A.CpuState.Halt)) ? 0x40 : 0x00) |
            ((Cpu.CurrentState != Cpu8080A.CpuState.Off) ? 0x20 : 0x00)
        );
        if ((Cpu.CurrentState == Cpu8080A.CpuState.Running ) != isFast) {
            isFast = !isFast;
            ConfigureRefresh(isFast);
        }
    }
}
