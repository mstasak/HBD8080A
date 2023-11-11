using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HBD8080A.Hardware.Computer;
using Microsoft.UI.Dispatching;

namespace HBD8080A.Services;


public sealed class ComputerSystemService {

    private DispatcherQueueController? RunDispatcherQueueController {
        get; set;
    }

    private DispatcherQueue? RunDispatcherQueue {
        get; set;
    }

    public DispatcherQueue? AppUIDispatcherQueue {
        get; private set;
    } = DispatcherQueue.GetForCurrentThread();

    private static readonly Lazy<ComputerSystemService> lazy =
        new(() => new ComputerSystemService());

    public static ComputerSystemService Instance => lazy.Value;

    private ComputerSystemService() {
        //Cpu = new Cpu8080A(AppUIDispatcherQueue: AppUIDispatcherQueue);
        DoRunRun();
    //    //Debug.WriteLine($"ComputerSystemService on thread \"{Thread.CurrentThread.Name}\", #{Thread.CurrentThread.ManagedThreadId}");
    }

    private Cpu8080A? cpu = null;
    public Cpu8080A Cpu {
        get {
            cpu ??= new Cpu8080A(AppUIDispatcherQueue: AppUIDispatcherQueue);
            return cpu;
        }
    }

    public void DoRunRun() { //yeah, the DoRunRun()
            if (RunDispatcherQueueController == null) {
                RunDispatcherQueueController = DispatcherQueueController.CreateOnDedicatedThread();
                RunDispatcherQueue = RunDispatcherQueueController.DispatcherQueue;
            }
            RunDispatcherQueue?.TryEnqueue(
                DispatcherQueuePriority.High,
                () => {
                    Cpu.RunLoop();
                    //RunStart();    
                }
            );

    }

    //TO DO: implement state management
    //TO DO: implement threaded run operation
    //TO DO: implement peripheral attachments
    //TO DO: implement global events

    /*
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