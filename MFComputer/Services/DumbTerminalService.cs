using System.Diagnostics;
using System.IO.Pipes;
using Microsoft.UI.Dispatching;
using Windows.Media.Audio;
//using System.Threading;
//using System.Threading.Tasks;
//using MFComputer.Hardware.Computer;

namespace MFComputer.Services;

public sealed class DumbTerminalService {

    public string Eol {
        get; set;
    } = "\n";

    private bool isOn = false;
    public bool IsOn {
        get => isOn;
        set {
            isOn = false;
            if (value) {
                Start();
            } else {
                Stop();
            }
        }
    }
    public bool QuitRequested { get; set; } = false;

    public Queue<char> InputQueue { get; } = new();
    public Queue<char> OutputQueue { get; } = new();
    private void Start() {
        if (!isOn) {
            isOn = true;
            RunTerminalTask();
        }
    }

    private void Stop() {
        if (isOn) {
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

    private static readonly Lazy<DumbTerminalService> lazy =
        new(() => new DumbTerminalService());

    public static DumbTerminalService Instance => lazy.Value;

    private DumbTerminalService() {
    }

    private bool shutdownDispatcherQueueRequested = false;
    private void RunTerminalTask() {
        if (RunDispatcherQueueController == null) {
            RunDispatcherQueueController = DispatcherQueueController.CreateOnDedicatedThread();
            RunDispatcherQueue = RunDispatcherQueueController.DispatcherQueue;
        }
        RunDispatcherQueue?.TryEnqueue(
            DispatcherQueuePriority.High,
            TerminalTask
        );
    }

    private Process? pipeClient = null;
    private Thread? serverKB;
    private Thread? serverDisplay;
    private void TerminalTask() {
        try {
            //configure client
            if (pipeClient == null) {
                pipeClient = new Process();
                pipeClient.StartInfo.FileName = "F:\\dev\\MFComputer\\DumbTerminal\\bin\\Debug\\net7.0\\DumbTerminal.exe";
                //pipeClient.StartInfo.FileName = "DumbTerminal.exe";
                pipeClient.StartInfo.UseShellExecute = false;
                pipeClient.Start();
            }

            //create worker threads
            //Console.WriteLine("Waiting for client connect...\n");
            serverKB = new Thread(ServerThreadKB);
            serverKB?.Start();
            serverDisplay = new Thread(ServerThreadDisplay);
            serverDisplay?.Start();
            Thread.Sleep(250);
            while (serverKB != null || serverDisplay != null) {
                if (serverKB?.Join(100) ?? false) {
                    //Console.WriteLine("Server thread[{0}] finished.", server!.ManagedThreadId);
                    serverKB = null;
                    QuitRequested = true;
                }
                if (serverDisplay?.Join(100) ?? false) {
                    //Console.WriteLine("Server thread[{0}] finished.", server!.ManagedThreadId);
                    serverDisplay = null;
                    QuitRequested = true;
                }
                if (!QuitRequested && serverKB != null && serverDisplay != null) {
                    var ch = ReadChar();
                    if (ch is not null) {
                        WriteChar(ch.Value);
                        if (ch.Value == '\x1A') {
                            QuitRequested = true;
                        }
                    }
                }
            }

        } catch (Exception e) {
            Debug.WriteLine($"Exception in DumbTerminalService: {e.Message}");    
        } finally {
            //shutdown terminal. if needed
            pipeClient?.WaitForExit();
            pipeClient?.Close();
            pipeClient = null;
            isOn = false;
            //Console.WriteLine("[SERVER] Client quit. Server terminating.");
        }
    }

    public void WriteChar(char ch) {
        if (!QuitRequested && serverKB != null && serverDisplay != null) {
            OutputQueue.Enqueue(ch);
        }
    }
    public char? ReadChar() {
        if (!QuitRequested && serverKB != null && serverDisplay != null) {
            if (InputQueue.Count == 0) {
                return null;
            }
            return InputQueue.Dequeue();
        } else {
            return null; 
        }
    }

    private void ServerThreadKB(object? data) {
        var pipeServerKB = new NamedPipeServerStream("dumbterminalkeyboardpipe", PipeDirection.In, 1);
        //var threadId = Thread.CurrentThread.ManagedThreadId;
        // Wait for a client to connect
        pipeServerKB.WaitForConnection();
        //Console.WriteLine("Client connected on thread[{0}].", threadId);
        try {
            // Read the request from the client. Once the client has
            // written to the pipe its security token will be available.
            //var ssIn = new StreamString(pipeServerKB);
            // Verify our identity to the connected client using a
            // string that the client anticipates.
            //ss.WriteString("I am the one true server!");
            //string filename = ss.ReadString();

            // Read in the contents of the file while impersonating the client.
            //ReadFileToStream fileReader = new ReadFileToStream(ss, filename);

            // Display the name of the user we are impersonating.
            //Console.WriteLine("Reading file: {0} on thread[{1}] as user: {2}.",
            //    filename, threadId, pipeServer.GetImpersonationUserName());
            //pipeServerKB.RunAsClient(fileReader.Start);
            while (!QuitRequested) {
                //try to queue input from kb
                //var s = ssIn.ReadString();
                //foreach(var c in s) {
                //    InputQueue.Enqueue(c);
                //}
                var key = (char)pipeServerKB.ReadByte();
                InputQueue.Enqueue(key);
                if (key == '\x1A') {
                    QuitRequested = true;
                }
            }
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e) {
            Debug.WriteLine("ERROR: {0}", e.Message);
        } finally {
            pipeServerKB.Close();
        }
    }
    private void ServerThreadDisplay(object? data) {
        var pipeServerDisplay = new NamedPipeServerStream("dumbterminaldisplaypipe", PipeDirection.Out, 1);
        //var threadId = Thread.CurrentThread.ManagedThreadId;
        // Wait for a client to connect
        pipeServerDisplay.WaitForConnection();
        //Console.WriteLine("Client connected on thread[{0}].", threadId);
        try {
            while (!QuitRequested && pipeServerDisplay.IsConnected) {
                //try to write output to display
                if (OutputQueue.Count > 0) {
                    var ch = OutputQueue.Dequeue();
                    pipeServerDisplay.WriteByte((byte)ch);
                    if (ch == '\x1A') {
                        QuitRequested = true;
                    }
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
        pipeServerDisplay.Close();
    }
}

