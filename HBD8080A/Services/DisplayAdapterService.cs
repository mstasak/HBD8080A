using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using Microsoft.UI.Dispatching;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBD8080A.Services;


public sealed class DisplayAdapterService {

    //lazy singleton implementation
    private static readonly Lazy<DisplayAdapterService> lazy =
        new(() => new DisplayAdapterService());

    public static DisplayAdapterService Instance => lazy.Value;

    private DisplayAdapterService() {
    }


    /*
     * Display a window which functions as a graphical display adapter.  This should display
     * graphics only (no text mode, text must be drawn).  Basic functionality to include:
     * - set mode (Wpixels, Hpixels, color depth)
     * - clear screen
     * - set bkg color
     * - set border color
     * - set fill color
     * - draw dot
     * - draw circle/ellipse
     * - draw rectangle/square
     * - draw rounded rect
     * - mouse cursor?
     * - draw polygon
     * - draw path
     * - composition layers?
     * - get color of pixel
     * - draw text (font, style, size, string)
     * - copy rectangle
     * - draw bitmap
     * - draw line
     * - draw bezier
     * - draw circle slice (i.e. pie chart segment)
     * 
     * Obviously pixel painting requires raw pixel byte access, so we can't use WINUI 3 Shape subclasses
     * (working with millions of 1x1 shapes would be impractical).
     * Unfortunately, WINUI 3/WinAppSDK is conspicuously lacking in a drawable image capability.
     * There is a writeable bitmap class, but setting bytes in its storage seems to have no effect.  May
     * work on that, or experiment with NUGET extensions (WriteableImage extensions? KyGSoft.Drawing?).  One can't even use
     * GDI+ in a WINUI app.  There is a Win2D package, but I'm having trouble getting to work post .net 5.
     * Nothing is easy, documentation is cryptic, and discussion tends to go along the lines of "Q: how can
     * I use X? A: Don't use X, try a different approach altogether" :/
     */
    public string Eol {
        get; set;
    } = "\n";

    private bool isOn = false;
    public bool IsOn {
        get => RunDispatcherQueue is not null && !QuitRequested && !shutdownDispatcherQueueRequested;
        set {
            if (value) {
                Start();
            } else {
                Stop();
            }
        }
    }
    public bool QuitRequested { get; set; } = false;

    public System.Collections.Concurrent.ConcurrentQueue<char> KBInputQueue { get; } = new();
    public System.Collections.Concurrent.ConcurrentQueue<byte> GAInputQueue { get; } = new();
    public System.Collections.Concurrent.ConcurrentQueue<byte> OutputQueue { get; } = new();
    private void Start() {
        if (!isOn) {
            QuitRequested = false;
            shutdownDispatcherQueueRequested = false;
            KBInputQueue.Clear();
            GAInputQueue.Clear();
            OutputQueue.Clear();
            isOn = true;
            RunTerminalTask();
        }
    }

    private void Stop() {
        if (isOn) {
            QuitRequested = true;
            KBInputQueue.Clear();
            GAInputQueue.Clear();
            OutputQueue.Clear();
            shutdownDispatcherQueueRequested = true;
        }
    }

    private DispatcherQueueController? RunDispatcherQueueController {
        get; set;
    }

    private DispatcherQueue? RunDispatcherQueue {
        get; set;
    }

    //public DispatcherQueue? AppUIDispatcherQueue {
    //    get; private set;
    //} = DispatcherQueue.GetForCurrentThread();

    private bool shutdownDispatcherQueueRequested = false;
    private async void RunTerminalTask() {
        if (RunDispatcherQueueController == null) {
            RunDispatcherQueueController = DispatcherQueueController.CreateOnDedicatedThread();
            RunDispatcherQueue = RunDispatcherQueueController.DispatcherQueue;
        }
        RunDispatcherQueue?.TryEnqueue(
            DispatcherQueuePriority.High,
            TerminalTask
        );
        await RunDispatcherQueueController.ShutdownQueueAsync();
        RunDispatcherQueue = null;
        RunDispatcherQueueController = null;
    }

    private Process? pipeClient = null;
    private Thread? serverKB;
    private Thread? serverFromDisplay;
    private Thread? serverToDisplay;
    private void TerminalTask() {
        try {

            //configure client
            if (pipeClient == null) {
                pipeClient = new Process();
                pipeClient.StartInfo.FileName = "F:\\dev\\HBD8080A\\GraphicsAdapter\\bin\\Debug\\net8.0-windows\\GraphicsAdapter.exe";
                //pipeClient.StartInfo.FileName = "DumbTerminal.exe";
                pipeClient.StartInfo.UseShellExecute = false;
                pipeClient.Start();
            }

            //create worker threads
            //Console.WriteLine("Waiting for client connect...\n");
            serverKB = new Thread(ServerThreadKB);
            serverKB?.Start();
            serverFromDisplay = new Thread(ServerThreadFromDisplay);
            serverFromDisplay?.Start();
            serverToDisplay = new Thread(ServerThreadToDisplay);
            serverToDisplay?.Start();
            //Thread.Sleep(250);

            while (serverKB != null || serverFromDisplay != null || serverToDisplay != null) {
                if (shutdownDispatcherQueueRequested) {
                    QuitRequested = true;
                }
                if (serverKB?.Join(100) ?? false) {
                    //Console.WriteLine("Server thread[{0}] finished.", server!.ManagedThreadId);
                    serverKB = null;
                    QuitRequested = true;
                }
                if (serverFromDisplay?.Join(100) ?? false) {
                    //Console.WriteLine("Server thread[{0}] finished.", server!.ManagedThreadId);
                    serverFromDisplay = null;
                    QuitRequested = true;
                }
                if (serverToDisplay?.Join(100) ?? false) {
                    //Console.WriteLine("Server thread[{0}] finished.", server!.ManagedThreadId);
                    serverToDisplay = null;
                    QuitRequested = true;
                }

                //simple echo loop for debug - sends kb input to display
                //note Enter (or Ctrl+M) is carriage return, Ctrl-Enter (or Ctrl+J) is linefeed
                //Ctrl+Z is EoF, which will kill terminal from KB.  It can also be killed by "turning off" the terminal in GUI.
                //if (!QuitRequested && serverKB != null && serverDisplay != null) {
                //    var ch = ReadChar();
                //    if (ch is not null) {
                //        WriteChar(ch.Value);
                //        if (ch.Value == '\x1A') {
                //            QuitRequested = true;
                //        }
                //    }
                //}
            }

        } catch (Exception e) {
            Debug.WriteLine($"Exception in DisplayAdapterService: {e.Message}");    
        } finally {
            //shutdown terminal. if needed
            pipeClient?.WaitForExit();
            pipeClient?.Close();
            pipeClient = null;
            isOn = false;
            //Console.WriteLine("[SERVER] Client quit. Server terminating.");
        }
    }

    public bool KBHit() {
        if (!QuitRequested && serverKB != null && serverFromDisplay != null && serverToDisplay != null) {
            return !KBInputQueue.IsEmpty;
        } else {
            return false;
        }
    }
    public bool GADataReady() {
        if (!QuitRequested && serverKB != null && serverFromDisplay != null && serverToDisplay != null) {
            return !GAInputQueue.IsEmpty;
        } else {
            return false;
        }
    }

    public void WriteByte(byte b) {
        if (!QuitRequested && serverKB != null && serverFromDisplay != null && serverToDisplay != null) {
            OutputQueue.Enqueue(b);
        }
    }
    public void Write(byte[] bytes) {
        foreach (var b in bytes) {
            WriteByte(b);
        }
    }

    //public void WriteLine(string s) {
    //    WriteLine(s + Eol);
    //}

    public char? ReadKBChar() {
        if (!QuitRequested && serverKB != null && serverFromDisplay != null && serverToDisplay != null) {
            //if (InputQueue.Count == 0) {
            //    return null;
            //}
            //return InputQueue.Dequeue();
            char ch;
            if (KBInputQueue.TryDequeue(out ch)) {
                return ch;
            }
        }
        return null;
    }
    public byte? ReadGAByte() {
        if (!QuitRequested && serverKB != null && serverFromDisplay != null && serverToDisplay != null) {
            //if (InputQueue.Count == 0) {
            //    return null;
            //}
            //return InputQueue.Dequeue();
            byte b;
            if (GAInputQueue.TryDequeue(out b)) {
                return b;
            }
        }
        return null;
    }

    private void ServerThreadKB(object? data) {
        var pipeServerKB = new NamedPipeServerStream("graphicsterminalkeyboardpipe", PipeDirection.In, 1);
        //var threadId = Thread.CurrentThread.ManagedThreadId;
        // Wait for a client to connect
        pipeServerKB.WaitForConnection();
        //Console.WriteLine("Client connected on thread[{0}].", threadId);
        try {
            while (!QuitRequested && pipeServerKB.IsConnected && !shutdownDispatcherQueueRequested) {
                var key = (char)pipeServerKB.ReadByte();
                KBInputQueue.Enqueue(key);
                if (key == '\x1A') {
                    QuitRequested = true;
                }
            }
        }
        catch (IOException e) {
            Debug.WriteLine("ERROR: {0}", e.Message);
        } finally {
            pipeServerKB.Close();
        }
    }

    private void ServerThreadFromDisplay(object? data) {
        var pipeServerFromDisplay = new NamedPipeServerStream("graphicsterminalfromdisplaypipe", PipeDirection.In, 1);
        //var threadId = Thread.CurrentThread.ManagedThreadId;
        // Wait for a client to connect
        pipeServerFromDisplay.WaitForConnection();
        //Console.WriteLine("Client connected on thread[{0}].", threadId);
        try {
            while (!QuitRequested && pipeServerFromDisplay.IsConnected && !shutdownDispatcherQueueRequested) {
                var key = pipeServerFromDisplay.ReadByte();
                GAInputQueue.Enqueue((byte)key);
                if (key == -1) {
                    QuitRequested = true;
                }
            }
        }
        catch (IOException e) {
            Debug.WriteLine("ERROR: {0}", e.Message);
        } finally {
            pipeServerFromDisplay.Close();
        }
    }

    private void ServerThreadToDisplay(object? data) {
        var pipeServerToDisplay = new NamedPipeServerStream("graphicsterminaltodisplaypipe", PipeDirection.Out, 1);
        //var threadId = Thread.CurrentThread.ManagedThreadId;
        // Wait for a client to connect
        pipeServerToDisplay.WaitForConnection();
        //Console.WriteLine("Client connected on thread[{0}].", threadId);
        try {
            //var outCount = 0;
            //var outTotal = 0;
            while (!QuitRequested && pipeServerToDisplay.IsConnected && !shutdownDispatcherQueueRequested) {
                //try to write output to display
                byte b;
                if (OutputQueue.TryDequeue(out b)) {
                    pipeServerToDisplay.WriteByte(b);
                    //outCount++;
                    //outTotal++;
                    //Thread.Sleep(50);
                    //if (outCount > 4096) { //sending around 32K seemed to be causing a pipe io exception - buffer filled maybe?
                    //    pipeServerToDisplay.WaitForPipeDrain();
                    //    //Thread.Sleep(50);
                    //    outCount = 0;
                    //}
                } else {
                    Thread.Sleep(50);
                }
            }
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e) {
            QuitRequested = true;
            Console.WriteLine("ERROR: {0}\nPress Enter to close.", e.Message);
            Console.ReadLine();
        }
        pipeServerToDisplay.Close();
    }
}
