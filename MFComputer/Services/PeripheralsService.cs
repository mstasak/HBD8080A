using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBD8080A.Services;


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
     *   teletype / video text display
     *   basic terminal (ADM-3A? VT-52? VT-100? TV-Typewriter?)
     *   line printer
     *   graphics printer
     *   video graphics display
     *   specific peripheral like Cromemco Dazzler
     *   sound output
     *   floppy and hard disks
     *   bank-switched RAM
     *   pretend math coprocessor
     *   A CRT-like library of program functionality which would overwhelm a
     *   poor little 64K RAM 8-bit CPU - served by host.
     */
}