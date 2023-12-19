using System.Drawing.Configuration;
using KGySoft.Drawing;
using System.IO.Pipes;
using System.Xml;

namespace GraphicsAdapter;

public partial class frmGraphicsAdapter : Form {

    internal class GraphicsCommand {
        internal byte gCommand = 0;
        internal int paramByteCount = 0;
        internal byte[]? paramBytes;
        internal int paramIx = 0;


        internal void PerformCommand(Graphics gr) {
            switch (gCommand) {
                case 0: //clear screen
                    gr.Clear(Color.Black);
                    break;
                case 1: //set pixel
                    if (paramByteCount == 5) {
                        var x = pWord();
                        var y = pWord();
                        var c = pColor();
                        var p = new Pen(c);
                        gr.DrawLine(p, x, y, x, y);
                    }
                    break;
            }
        }

        internal int pWord() {
            return paramBytes[paramIx++] | paramBytes[paramIx++] << 8;
        }
        internal byte pByte() {
            return paramBytes[paramIx++];
        }
        internal Color pColor() {
            var rawC = pByte();
            return Color.FromArgb(rawC / 36 * 255 / 6, rawC / 6 % 6 * 255 / 6, rawC % 6 * 255 / 6);
        }

    }


    internal bool QuitRequested { get; set; } = false;
    internal Thread? clientKB;
    internal Thread? clientDisplay;
    internal readonly System.Collections.Concurrent.ConcurrentQueue<GraphicsCommand> GraphicsCommandQueue = new();

    public frmGraphicsAdapter() {
        InitializeComponent();
        using var gr = picDisplayPanel.CreateGraphics();
        gr.Clear(Color.Black);
    }

    internal void PerformGraphicsCommands(int maxCommands = int.MaxValue) {
        int commandsDone = 0;
        using var gr = picDisplayPanel.CreateGraphics();
        while (!GraphicsCommandQueue.IsEmpty) {
            GraphicsCommand cmd;
            if (GraphicsCommandQueue.TryDequeue(out cmd)) {
                cmd.PerformCommand(gr);
            }
            commandsDone++;
            if (commandsDone >= maxCommands) {
                break;
            }
        }
    }

    private void BtnAddRandomElement_Click(object sender, EventArgs e) {
        using (var gr = picDisplayPanel.CreateGraphics()) {
            gr.DrawRectangle(Pens.Blue, 0, 0, 5, 5);
            for (var i = 0; i < 1000; i++) {
                switch (Rnd(3)) {
                    case 0:
                        //gr.DrawRectangle(System.Drawing.Pens.Firebrick, 100, 100, 150, 125);
                        gr.DrawEllipse(RandomPen(), RandomRect());
                        break;
                    case 1:
                        gr.DrawRectangle(RandomPen(), RandomRect());
                        break;
                    case 3:

                        break;
                    default:
                        break;
                }
            }

            var img = picDisplayPanel.Image;
            //var bitmap = img as Bitmap;
            var bmp = gr.ToBitmap(false);
            if (bmp != null) {
                var c = bmp?.GetPixel(0, 0);
            }

        }
    }
    internal Rectangle RandomRect() {
        return Rectangle.FromLTRB(Rnd(1024), Rnd(768), Rnd(1024), Rnd(768));
    }
    internal Pen RandomPen() {
        var c = Color.FromArgb(Rnd(256), Rnd(256), Rnd(256));
        var w = Rnd(4) + 1;
        return new Pen(c, w);
    }
    internal int Rnd(int limit = 256) {
        return System.Random.Shared.Next() % limit;
    }

    private void frmGraphicsAdapter_Load(object sender, EventArgs e) {
        //startup threads
        clientKB = new Thread(ClientThreadKB);
        clientKB?.Start();
        clientDisplay = new Thread(ClientThreadDisplay);
        clientDisplay?.Start();
        Thread.Sleep(250);
    }

    internal void ShutdownThreads() {
        QuitRequested = true;
        if (!clientKB?.IsAlive ?? false) {
            clientKB = null;
        }
        if (!clientDisplay?.IsAlive ?? false) {
            clientDisplay = null;
        }
        while (clientKB != null || clientDisplay != null) {
            if (clientKB?.Join(100) ?? false) {
                clientKB = null;
                //QuitRequested = true;
            }
            if (clientDisplay?.Join(100) ?? false) {
                clientDisplay = null;
                //QuitRequested = true;
            }
        }
    }

    internal void ClientThreadKB(object? _) {
        using var pipeServerKB = new NamedPipeClientStream(".", "graphicsterminalkeyboardpipe", PipeDirection.Out);
        try {
            pipeServerKB.Connect(2000);
            //Console.WriteLine("Client connected on thread[{0}].", threadId);
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
        } catch (TimeoutException e) {
            //could do something to notify user, ask whether to retry, etc.
        } catch (IOException e) {
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            Console.WriteLine("ERROR: {0}\nPress Enter to close.", e.Message);
            Console.ReadLine();
        } finally {
            pipeServerKB.Close();
            QuitRequested = true;
        }
    }

    internal enum cmdReadState {
        GetEsc, GetPeriod, GetCommandCode, GetParamsSize, GetParamsSizeExLow, GetParamsSizeExHigh, GetParams, GotCmd
    }
    internal void ClientThreadDisplay(object? data) {
        using var pipeServerDisplay = new NamedPipeClientStream(".", "graphicsterminaldisplaypipe", PipeDirection.In);
        try {
            // Wait for a client to connect
            pipeServerDisplay.Connect(2000);
            //Console.WriteLine("Client connected on thread[{0}].", threadId);
            GraphicsCommand cmd = new();
            var ParamsBytesRead = 0;
            cmdReadState CRState = cmdReadState.GetEsc;
            while (!QuitRequested && pipeServerDisplay.IsConnected) {
                var charCode = pipeServerDisplay.ReadByte();
                if (charCode == -1 || charCode == 26) { //eof or ctrl+z
                    QuitRequested = true;
                }
                else {
                    //Console.Write((char)charCode);
                    //should run this on ui thread or send with an event?
                    //GraphicsCommandQueue.Enqueue((byte)charCode);
                    //assemble command to enqueue
                    switch (CRState) {
                        case cmdReadState.GetEsc:
                            if (charCode == 27) {
                                CRState = cmdReadState.GetPeriod;
                            }
                            else {
                                //we are out of sync, so discard byte and wait for Esc char
                            }
                            break;
                        case cmdReadState.GetPeriod:
                            if (charCode == (byte)'.') {
                                CRState = cmdReadState.GetCommandCode;
                            }
                            else {
                                //lost sync, wait for Esc+'.'
                                CRState = cmdReadState.GetEsc;
                            }
                            break;
                        case cmdReadState.GetCommandCode:
                            cmd = new();
                            cmd.gCommand = (byte)charCode;
                            CRState = cmdReadState.GetParamsSize;
                            ParamsBytesRead = 0;
                            break;
                        case cmdReadState.GetParamsSize:
                            if (charCode == 255) {
                                CRState = cmdReadState.GetParamsSizeExLow;
                            }
                            else {
                                cmd.paramByteCount = charCode;
                                CRState = cmdReadState.GetParams;
                            }
                            break;
                        case cmdReadState.GetParamsSizeExLow:
                            cmd.paramByteCount = charCode;
                            CRState = cmdReadState.GetParamsSizeExHigh;
                            break;
                        case cmdReadState.GetParamsSizeExHigh:
                            cmd.paramByteCount |= charCode << 8;
                            CRState = cmdReadState.GetParams;
                            break;
                        case cmdReadState.GetParams:
                            if (cmd.paramByteCount == 0) {
                                CRState = cmdReadState.GotCmd;
                            }
                            if (ParamsBytesRead == 0) {
                                cmd.paramBytes = new byte[cmd.paramByteCount];
                            }
                            cmd.paramBytes[ParamsBytesRead++] = (byte)charCode;
                            if (ParamsBytesRead == cmd.paramByteCount) {
                                CRState = cmdReadState.GotCmd;
                            }
                            break;
                        case cmdReadState.GotCmd:
                            break;
                    }
                    if (CRState == cmdReadState.GotCmd) {
                        GraphicsCommandQueue.Enqueue(cmd);
                        CRState = cmdReadState.GetEsc;
                    }
                }
            }
        } catch (TimeoutException e) {
            //could do something to notify user, ask whether to retry, etc.
        } catch (IOException e) {
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            QuitRequested = true;
            Console.WriteLine("ERROR: {0}\nPress Enter to close.", e.Message);
            Console.ReadLine();
        } finally {
            pipeServerDisplay.Close();
            QuitRequested = true;
        }
    }

    private void timer1_Tick(object sender, EventArgs e) {
        PerformGraphicsCommands();
        if (QuitRequested && ((clientKB==null || !clientKB.IsAlive) || (clientDisplay==null || !clientDisplay.IsAlive))) {
            timer1.Stop();
            Close(); //shutdown app
        }
    }
}

