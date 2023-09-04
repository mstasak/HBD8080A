using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFComputer.Services;


public sealed class PeripheralsService {
    private static readonly Lazy<PeripheralsService> lazy =
        new(() => new PeripheralsService());

    public static PeripheralsService Instance => lazy.Value;

    private PeripheralsService() {
    }


    /*
     * Provide simulated peripherals:
     *   raw keyboard
     *   mouse
     *   pass-through joystick or game controller
     *   teletype
     *   basic terminal (ADM-3A? VT-52? VT-100? TV-Typewriter?)
     *   line printer
     *   graphics printer
     *   video text display
     *   video graphics display
     *   specific peripheral like Cromemco Dazzler
     *   sound output
     *   floppy and hard disks
     *   bank-switched RAM
     *   pretend math coprocessor
     */
}