using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFComputer.Hardware.Computer;
using Microsoft.UI.Dispatching;
//using Windows.System;

namespace MFComputer.Services;


public sealed class ComputerSystemService {

    DispatcherQueueController? runDispatcherQueueController {
        get; set;
    }
    DispatcherQueue? runDispatcherQueue {
        get; set;
    }
    public DispatcherQueue? AppUIDispatcherQueue {
        get; private set;
    } = DispatcherQueue.GetForCurrentThread();

    //public Thread? RunThread {
    //    get; set;
    //}

    private static readonly Lazy<ComputerSystemService> lazy =
        new(() => new ComputerSystemService());

    public static ComputerSystemService Instance => lazy.Value;

    private ComputerSystemService() {
    //    Debug.Assert(AppUIDispatcherQueue != null);
        Cpu = new Cpu8080A(AppUIDispatcherQueue: AppUIDispatcherQueue);
        DoRunRun();
    //    //Debug.WriteLine($"ComputerSystemService on thread \"{Thread.CurrentThread.Name}\", #{Thread.CurrentThread.ManagedThreadId}");
    //    IsOn = false;
    //    IsTurbo 
    }

    //public bool IsOn {
    //    get; set;
    //}

    //public bool IsRunning {
    //    get; set;
    //}

    //public bool IsTurbo {
    //    get; set;
    //}

    public Cpu8080A Cpu {
        get; set;
    }

    //public void RunStart() {
    //    Cpu.Run();
    //}

    public void DoRunRun() {
        if (runDispatcherQueueController == null) {
            runDispatcherQueueController = DispatcherQueueController.CreateOnDedicatedThread();
            runDispatcherQueue = runDispatcherQueueController.DispatcherQueue;
        }
        runDispatcherQueue?.TryEnqueue(
            DispatcherQueuePriority.High,
            () => {
                Cpu.RunLoop();
                //RunStart();    
            }
        );

        //if (CpuThread != null) {
        //    CpuThread.Resume();
        //} else {
        //    CpuThread = new Thread(new ThreadStart(RunStart));
        //    //var CpuDispatcherQueue = DispatcherQueueController.CreateOnDedicatedThread();
        //    CpuThread.Start();
        //}
    }

    //public async void Stop() {
    //    //if (CpuThread != null) {
    //    //    CpuThread.Suspend();
    //    //}
    //    if (runDispatcherQueueController != null) {
    //        await runDispatcherQueueController.ShutdownQueueAsync();
    //    }
    //}

    //public void Reset() {
    //    Cpu.Reset();
    //    //CpuThread = null;
    //}

    //public void SingleStep() {
    //}

    //public void ShutDown() {
    //}

    //public void StartUp() {
    //}

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