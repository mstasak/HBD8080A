using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFComputer.Services;


public sealed class HostEnhancementService {
    private static readonly Lazy<HostEnhancementService> lazy =
        new(() => new HostEnhancementService());

    public static HostEnhancementService Instance => lazy.Value;

    private HostEnhancementService() {
    }

    /*
    internet
    database (simple)
    file system sandbox area
    whole file system (caution!)
    printer - text
    printer - postscript?
    printer - hpgl?
    printer - pdf?
    mouse
    joystick
    logfile
    virtual ram
    little vm (like CLR or JVM, but tiny?)
    remote editor activation
    remote IDE activation

     */
}