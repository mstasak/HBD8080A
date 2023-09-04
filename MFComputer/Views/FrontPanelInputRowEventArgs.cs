namespace MFComputer.Views;

public class FrontPanelInputRowEventArgs : EventArgs
{
    //public string Status { get; private set; }
    public byte? SwitchStates {
        get; private set;
    }
    public byte? ButtonPresses {
        get; private set;
    }
    public byte? ButtonUpPresses {
        get; private set;
    }
    public byte? ButtonDownPresses {
        get; private set;
    }
    public FrontPanelInputRowEventArgs(byte? Switches, byte? Presses, byte? UpPresses, byte? DownPresses)
    {
        SwitchStates = Switches;
        ButtonPresses = Presses;
        ButtonUpPresses = UpPresses;
        ButtonDownPresses = DownPresses;
    }
}
