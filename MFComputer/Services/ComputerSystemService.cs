using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFComputer.Hardware.Computer;

namespace MFComputer.Services;


public sealed class ComputerSystemService {

    public Thread? CpuThread {
        get; set; 
    }

    private static readonly Lazy<ComputerSystemService> lazy =
        new(() => new ComputerSystemService());

    public static ComputerSystemService Instance => lazy.Value;

    private ComputerSystemService() {
        Cpu = new();
    }

    public Cpu8080A Cpu {
        get; set;
    }

    public void RunStart() {
    }

    public void Run() {
        if (CpuThread != null) {
            CpuThread.Resume();
        } else {
            CpuThread = new Thread(new ThreadStart(RunStart));
            CpuThread.Start();
        }
    }

    public void Stop() {
        if (CpuThread != null) {
            CpuThread.Suspend();
        }
    }

    public void Reset() {
        Cpu.Reset();
        CpuThread = null;
    }

    public void SingleStep() {
    }

    public void ShutDown() {
    }

    public void StartUp() {
    }

    //TO DO: implement state management
    //TO DO: implement threaded run operation
    //TO DO: implement peripheral attachments
    //TO DO: implement global events

    /*
     run
     pause
     resume (=run)
     off
     on
     halt?
     load
     loadandrun
     runmonitor
     enablehostenhancements(bool)
     attachsound
     attachtty
     attachconsole
     attachrastergraphics(mode=default)
     savecheckpoint
     restorecheckpoint
     
     */
}