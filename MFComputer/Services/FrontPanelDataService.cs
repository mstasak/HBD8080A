using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFComputer.Services;


public sealed class FrontPanelDataService {
    private static readonly Lazy<FrontPanelDataService> lazy =
        new(() => new FrontPanelDataService());

    public static FrontPanelDataService Instance => lazy.Value;

    private FrontPanelDataService() {
    }

    public byte SwitchBankHighAddressInput {
        get; set;
    }
    public byte SwitchBankLowAddressData {
        get; set;
    }
    public byte SwitchBankControl {
        get; set;
    }

    //not really used for anything, except maybe to repopulate the LED state in a new panel instance
    public byte LedBankHighAddress {
        get; set;
    }
    public byte LedBankLowAddress {
        get; set;
    }
    public byte LedBankFlags {
        get; set;
    }
    public byte LedBankMemoryData {
        get; set;
    }
    public byte LedBankStatus {
        get; set;
    }

    //TODO: capture and relay button events
    //TODO: add bool accessors for status and flag bits
    //TODO: consider offline persistence of switch states
    //TODO: implement reset behavior
    //TODO: implement timer-based updates when cpu is running
    //TODO: implement snapshot update when CPU is stopped
}