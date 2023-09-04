using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace MFComputer.Views;


/// <summary>
/// Utility class to provide 2way bool property bindings for up to eight
/// switch/button controls, and allow access to the switch positions in a
/// single byte.  This serves the binding role of a viewmodel for a
/// UserControl, allowing XAML input controls to bind to observable
/// properties in this class and raising events so an interested object
/// (in this case, the front panel view model) can get updated by event
/// with a whole byte value whenever a switch changes, or a button is
/// pressed.
/// </summary>
public partial class FrontPanelInputRowHelper : ObservableObject {

    //event to tell subscriber that either a switch changed or a button was pressed
    public delegate void FrontPanelInputRowChangedHandler(object sender, FrontPanelInputRowEventArgs e);
    public event FrontPanelInputRowChangedHandler? OnFrontPanelInputRowChanged;

    
    /// <summary>
    /// whenever a switch changes, send event to notify observer
    /// </summary>
    /// <param name="value"></param>
    private void InputSwitchRowChanged(byte value) {
        // Make sure someone is listening to event
        OnFrontPanelInputRowChanged?.Invoke(
            this, 
            new FrontPanelInputRowEventArgs(Switches: value, 
                Presses: null, 
                UpPresses: null, 
                DownPresses: null));
    }

    /// <summary>
    /// whenever a simple button is pressed, send observer an event
    /// </summary>
    /// <param name="value"></param>
    private void InputSwitchRowSimpleButtonTapped(byte value) {
        // Make sure someone is listening to event
        OnFrontPanelInputRowChanged?.Invoke(
            this, 
            new FrontPanelInputRowEventArgs(Switches: null, 
                Presses: value, 
                UpPresses: null, 
                DownPresses: null));
    }

    /// <summary>
    /// whenever a toggle button is pressed, send observer an event
    /// </summary>
    /// <param name="upTaps"></param>
    /// <param name="downTaps"></param>
    private void InputSwitchRowBidirectionalButtonTapped(byte? upTaps, byte? downTaps) {
        // Make sure someone is listening to event
        OnFrontPanelInputRowChanged?.Invoke(
            this, 
            new FrontPanelInputRowEventArgs(Switches: null, 
                Presses: null, 
                UpPresses: (upTaps == 0) ? null : upTaps, 
                DownPresses: (downTaps == 0) ? null : downTaps));
    }

    //used for setting up bindings
    private readonly string[] switchBindPropertyNames = {
        nameof(Switch7On), nameof(Switch6On), nameof(Switch5On), nameof(Switch4On),
        nameof(Switch3On), nameof(Switch2On), nameof(Switch1On), nameof(Switch0On)
    };

    //backing var
    private byte switchRowValues;
    //position of switches, in format 0bHHHHLLLL, suitable as a binding target (source in a viewmodel)
    public byte SwitchRowValues {
        get => switchRowValues;
        set { //probably never used except to set initial switch positions
            var bitPropertyIndex = 0;
            for (var maskBit = 0x80; maskBit > 0; maskBit >>= 1, bitPropertyIndex++) {
                var oldPos = (switchRowValues & maskBit) != 0;
                var newPos = (value & maskBit) != 0;
                if (oldPos != newPos) {
                    SetProperty(oldPos, newPos, (newVal) => switchRowValues = (byte)(switchRowValues ^ maskBit), switchBindPropertyNames[bitPropertyIndex]);
                }
            }
        }
    }

    //receive events from xaml input buttons
    private void Button_Pressed(object sender, EventArgs e) {
        //if (sender is ToggleButton toggleButton) {
        //    //sender.Tag is 0..7 indicating switch pos
        //    var mask = (byte)(0x80 >> ((int)toggleButton.Tag));

        //    //need to inspect event args and determine if top or bottom half of button was clicked.
        //    var isUp = false;



        //    InputSwitchRowBidirectionalButtonTapped(upTaps: (byte)(isUp ? mask : 0), downTaps: (byte)(isUp ? 0 : mask));
        //} else
        if (sender is Button button) {
            if ((int)button.Tag < 8) {
                //button.Tag is 0..7 indicating which simple button was tapped, or which togglebutton was tapped up
                var mask = (byte)(0x80 >> ((int)button.Tag));
                InputSwitchRowSimpleButtonTapped(mask);
            } else {
                //button.Tag is 8..15 indicating which toggle button was tapped down
            }
        }
    }

    //process a single switch's changed event; update bank byte value and send event 
    private void saveSwitchPosAndNotify(bool value, int switchNumber) {
        var mask = (byte)(1 << switchNumber);
        var oldPos = (switchRowValues & mask) != 0;
        if (value != oldPos) {
            SetProperty(oldPos, value, (newVal) => SwitchRowValues = (byte)(SwitchRowValues ^ mask), switchBindPropertyNames[switchNumber]);
            //OnPropertyChanged(nameof(SwitchValues));
            InputSwitchRowChanged(SwitchRowValues);
        }
    }

    //single switch bool bindable properties
    public bool Switch7On {
        get => (switchRowValues & 0x80) != 0;
        set => saveSwitchPosAndNotify(value, 7);
    }
    public bool Switch6On {
        get => (switchRowValues & 0x40) != 0;
        set => saveSwitchPosAndNotify(value, 6);
    }
    public bool Switch5On {
        get => (switchRowValues & 0x20) != 0;
        set => saveSwitchPosAndNotify(value, 5);
    }
    public bool Switch4On {
        get => (switchRowValues & 0x10) != 0;
        set => saveSwitchPosAndNotify(value, 4);
    }
    public bool Switch3On {
        get => (switchRowValues & 0x08) != 0;
        set => saveSwitchPosAndNotify(value, 3);
    }
    public bool Switch2On {
        get => (switchRowValues & 0x04) != 0;
        set => saveSwitchPosAndNotify(value, 2);
    }
    public bool Switch1On {
        get => (switchRowValues & 0x02) != 0;
        set => saveSwitchPosAndNotify(value, 1);
    }
    public bool Switch0On {
        get => (switchRowValues & 0x01) != 0;
        set => saveSwitchPosAndNotify(value, 0);
    }

}
