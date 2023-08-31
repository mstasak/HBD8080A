using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MFComputer.Views;


/// <summary>
/// Utility class to provide 2way bool property bindings for up to eight
/// switch controls, and allow access to the switch positions in a
/// single byte.
/// </summary>
public class SwitchBank : ObservableObject {

    public class SwitchBankChangedEventArgs : EventArgs
    {
        //public string Status { get; private set; }
        public byte SwitchStates {
            get; private set;
        }
        public SwitchBankChangedEventArgs(byte Switches)
        {
            SwitchStates = Switches;
        }
    }

    public delegate void SwitchBankChangedHandler(object sender, SwitchBankChangedEventArgs e);
    public event SwitchBankChangedHandler? OnSwitchBankChanged;

    private void SwitchBankChanged(byte value)
    {
        // Make sure someone is listening to event
        OnSwitchBankChanged?.Invoke(this, new SwitchBankChangedEventArgs(value));
    }

    private byte switchValues;
    private readonly string[] propertyNames = {
        nameof(Switch7On), nameof(Switch6On), nameof(Switch5On), nameof(Switch4On),
        nameof(Switch3On), nameof(Switch2On), nameof(Switch1On), nameof(Switch0On)
    };

        public byte SwitchValues {
        get => switchValues;
        set { //probably never used except to set initial switch positions
            var j = 0;
            //var changed = false;
            for (var i = 0x80; i > 0; i >>= 1, j++) {
                var oldPos = (switchValues & i) != 0;
                var newPos = (value & i) != 0;
                if (oldPos != newPos) {
                    SetProperty(oldPos, newPos, (newVal) => switchValues = (byte)(switchValues ^ i), propertyNames[j]);
                    //changed = true;
                }
            }
            //if (changed) {
            //    OnPropertyChanged(nameof(SwitchValues));
            //}
        }
    }

    private void saveSwitchPosAndNotify(bool value, int switchNumber) {
        var mask = (byte)(1 << switchNumber);
        var oldPos = (switchValues & mask) != 0;
        if (value != oldPos) {
            SetProperty(oldPos, value, (newVal) => SwitchValues = (byte)(SwitchValues ^ mask), propertyNames[switchNumber]);
            //OnPropertyChanged(nameof(SwitchValues));
            SwitchBankChanged(SwitchValues);
        }
    }
    public bool Switch7On {
        get => (switchValues & 0x80) != 0;
        set => saveSwitchPosAndNotify(value, 7);
    }
    public bool Switch6On {
        get => (switchValues & 0x40) != 0;
        set => saveSwitchPosAndNotify(value, 6);
    }
    public bool Switch5On {
        get => (switchValues & 0x20) != 0;
        set => saveSwitchPosAndNotify(value, 5);
    }
    public bool Switch4On {
        get => (switchValues & 0x10) != 0;
        set => saveSwitchPosAndNotify(value, 4);
    }
    public bool Switch3On {
        get => (switchValues & 0x08) != 0;
        set => saveSwitchPosAndNotify(value, 3);
    }
    public bool Switch2On {
        get => (switchValues & 0x04) != 0;
        set => saveSwitchPosAndNotify(value, 2);
    }
    public bool Switch1On {
        get => (switchValues & 0x02) != 0;
        set => saveSwitchPosAndNotify(value, 1);
    }
    public bool Switch0On {
        get => (switchValues & 0x01) != 0;
        set => saveSwitchPosAndNotify(value, 0);
    }

}
