using System.IO.Pipes;
using System.Text;

namespace DumbTerminal;

public class DumbTerminalClient {
    //named pipes edition
    public static bool QuitRequested { get; set; } = false;
    public static void Main() {
        Console.Title = "HBD8080A Dumb Terminal";
        Console.TreatControlCAsInput = true;
        Thread? clientKB;
        Thread? clientDisplay;
        //Console.WriteLine("\n*** Named pipe server stream with impersonation example ***\n");
        //Console.WriteLine("Waiting for client connect...\n");
        clientKB = new Thread(ClientThreadKB);
        clientKB?.Start();
        clientDisplay = new Thread(ClientThreadDisplay);
        clientDisplay?.Start();
        Thread.Sleep(250);
        while (clientKB != null || clientDisplay != null) {
            if (clientKB?.Join(100) ?? false) {
                //Console.WriteLine("Named pipe keyboard service thread[{0}] finished.", clientKB);
                clientKB = null;
                QuitRequested = true;
            }
            if (clientDisplay?.Join(100) ?? false) {
                //Console.WriteLine("Named pipe display service thread[{0}] finished.", clientDisplay);
                clientDisplay = null;
                QuitRequested = true;
            }
        }
        //Console.WriteLine("\nPipe threads closed, exiting.\nPress Enter to close.\n");
        //Console.ReadLine();
    }

    private static void ClientThreadKB(object? _) {
        using var pipeServerKB = new NamedPipeClientStream(".", "dumbterminalkeyboardpipe", PipeDirection.Out);
        pipeServerKB.Connect();
        //Console.WriteLine("Client connected on thread[{0}].", threadId);
        try {
            while (!QuitRequested) {
                //try to collect input from kb
                if (Console.KeyAvailable) {
                    var kbCode = Console.ReadKey(true);
                    var ch = kbCode.KeyChar;
                    if (ch == '\x1A') {
                        QuitRequested = true;
                    } else {
                        pipeServerKB.WriteByte((byte)ch);
                    }
                }
            }
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e) {
            Console.WriteLine("ERROR: {0}\nPress Enter to close.", e.Message);
            Console.ReadLine();
        } finally {
            pipeServerKB.Close();
            QuitRequested = true;
        }
    }
    private static void ClientThreadDisplay(object? data) {
        using var pipeServerDisplay = new NamedPipeClientStream(".", "dumbterminaldisplaypipe", PipeDirection.In);
        // Wait for a client to connect
        pipeServerDisplay.Connect();
        //Console.WriteLine("Client connected on thread[{0}].", threadId);
        try {
            while (!QuitRequested && pipeServerDisplay.IsConnected) {
                var charCode = pipeServerDisplay.ReadByte();
                if (charCode == -1 || charCode == 26) { //eof or ctrl+z
                    QuitRequested = true;
                } else {
                    Console.Write((char)charCode);
                }
            }
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e) {
            QuitRequested = true;
            Console.WriteLine("ERROR: {0}\nPress Enter to close.", e.Message);
            Console.ReadLine();
        } finally { 
            pipeServerDisplay.Close();
            QuitRequested = true;
        }
    }
}
