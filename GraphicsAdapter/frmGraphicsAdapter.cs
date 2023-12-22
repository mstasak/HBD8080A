using System.Drawing.Configuration;
using KGySoft.Drawing;
using System.IO.Pipes;
using System.Xml;
using Microsoft.VisualBasic;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

namespace GraphicsAdapter;

public partial class frmGraphicsAdapter : Form {
    internal DateTime? firstMod;
    internal Bitmap bmp = new Bitmap(1024, 768);
    internal TimeSpan cacheTime = new TimeSpan(10 * 150);

    internal class GraphicsCommand {
        internal byte gCommand = 0;
        internal int paramByteCount = 0;
        internal byte[]? paramBytes;
        internal int paramIx = 0;

        internal void PerformCommand(Bitmap bmp) {
            try {
                switch (gCommand) {
                    case 1: //clear screen
                        bmp.Clear(Color.Black);
                        break;
                    case 2: //set pixel
                        if (paramByteCount == 5) {
                            var x = pWord();
                            var y = pWord();
                            var c = pColor();
                            bmp.SetPixel(x, y, c);
                        }
                        break;
                }
            } catch (Exception ex) {
                Debug.WriteLine($"Exception in GraphicsCommand.PerformCommand: {ex.Message}");
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
    internal Thread? clientKBSend;
    internal Thread? clientDisplayReceive;
    internal Thread? clientDisplaySend;
    internal readonly System.Collections.Concurrent.ConcurrentQueue<GraphicsCommand> GraphicsCommandQueue = new();
    internal readonly System.Collections.Concurrent.ConcurrentQueue<byte> GraphicsResponseQueue = new();
    internal readonly System.Collections.Concurrent.ConcurrentQueue<char> KeypressQueue = new();

    public frmGraphicsAdapter() {
        InitializeComponent();
    }

    internal void PerformGraphicsCommands(int maxCommands = int.MaxValue) {
        try {
            var commandsDone = 0;
            while (!GraphicsCommandQueue.IsEmpty) {
                GraphicsCommand? cmd;
                if (GraphicsCommandQueue.TryDequeue(out cmd)) {
                    cmd!.PerformCommand(bmp);
                    firstMod ??= DateTime.Now;
                }
                commandsDone++;
                if (commandsDone >= maxCommands) {
                    break;
                }
            }
            if (DateTime.Now - (firstMod ?? DateTime.Now) > cacheTime) {
                picDisplayPanel.Invalidate();
                firstMod = null;
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Exception in PerformGraphicsCommands: {ex.Message}.");
        }

    }

    private void BtnAddRandomElement_Click(object sender, EventArgs e) {
        //using (var gr = picDisplayPanel.CreateGraphics()) {
        //    gr.DrawRectangle(Pens.Blue, 0, 0, 5, 5);
        //    for (var i = 0; i < 1000; i++) {
        //        switch (Rnd(3)) {
        //            case 0:
        //                //gr.DrawRectangle(System.Drawing.Pens.Firebrick, 100, 100, 150, 125);
        //                gr.DrawEllipse(RandomPen(), RandomRect());
        //                break;
        //            case 1:
        //                gr.DrawRectangle(RandomPen(), RandomRect());
        //                break;
        //            case 3:

        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    var img = picDisplayPanel.Image;
        //    //var bitmap = img as Bitmap;
        //    var bmp = gr.ToBitmap(false);
        //    if (bmp != null) {
        //        var c = bmp?.GetPixel(0, 0);
        //    }

        //}
    }
    //internal Rectangle RandomRect() {
    //    return Rectangle.FromLTRB(Rnd(1024), Rnd(768), Rnd(1024), Rnd(768));
    //}
    //internal Pen RandomPen() {
    //    var c = Color.FromArgb(Rnd(256), Rnd(256), Rnd(256));
    //    var w = Rnd(4) + 1;
    //    return new Pen(c, w);
    //}
    //internal int Rnd(int limit = 256) {
    //    return System.Random.Shared.Next() % limit;
    //}

    private void frmGraphicsAdapter_Load(object sender, EventArgs e) {
        //startup threads
        clientKBSend = new Thread(ClientThreadKBSend);
        clientKBSend?.Start();
        clientDisplaySend = new Thread(ClientThreadDisplaySend);
        clientDisplaySend?.Start();
        clientDisplayReceive = new Thread(ClientThreadDisplayReceive);
        clientDisplayReceive?.Start();
        bmp.Clear(Color.Black);
        picDisplayPanel.Invalidate();
        Thread.Sleep(250); //probably unnecessary
        timer1.Enabled = true;
        timer1.Start();
    }

    internal void ShutdownThreads() {
        QuitRequested = true;
        if (!clientKBSend?.IsAlive ?? false) {
            clientKBSend = null;
        }
        if (!clientDisplaySend?.IsAlive ?? false) {
            clientDisplaySend = null;
        }
        if (!clientDisplayReceive?.IsAlive ?? false) {
            clientDisplayReceive = null;
        }
        while (clientKBSend != null || clientDisplaySend != null || clientDisplayReceive != null) {
            if (clientKBSend?.Join(100) ?? false) {
                clientKBSend = null;
                //QuitRequested = true;
            }
            if (clientDisplaySend?.Join(100) ?? false) {
                clientDisplaySend = null;
                //QuitRequested = true;
            }
            if (clientDisplayReceive?.Join(100) ?? false) {
                clientDisplayReceive = null;
                //QuitRequested = true;
            }
        }
    }

    internal void ClientThreadKBSend(object? _) {
        if (QuitRequested) {
            return;
        }

        using var pipeServerKB = new NamedPipeClientStream(".", "graphicsterminalkeyboardpipe", PipeDirection.Out);
        try {
            pipeServerKB.Connect();
            //Console.WriteLine("Client connected on thread[{0}].", threadId);
            while (!QuitRequested) {
                //try to collect input from kb
                //if (Console.In.Peek() >= 0) {
                //    var kbCodex = Console.ReadKey(true);
                //    var kbCode = Console.In.Read(); // ReadKey(true);
                //    var ch = kbCode.KeyChar;
                //    if (ch == '\x1A') {
                //        QuitRequested = true;
                //    } else {
                //        pipeServerKB.WriteByte((byte)ch);
                //    }
                //}
                if (!KeypressQueue.IsEmpty) {
                    char c;
                    if (KeypressQueue.TryDequeue(out c)) {
                        if (c == '\x1A') {
                            QuitRequested = true;
                        } else {
                            pipeServerKB.WriteByte((byte)c);
                        }
                    }
                }
            }
        } catch (TimeoutException e) {
            //could do something to notify user, ask whether to retry, etc.
            Debug.WriteLine($"Exception in ClientThreadKBSend: {e.Message}");
        } catch (IOException e) {
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            Debug.WriteLine($"Exception in ClientThreadKBSend: {e.Message}");
            Console.WriteLine("ERROR: {0}\nPress Enter to close.", e.Message);
            Console.ReadLine();
        } finally {
            pipeServerKB.Close();
            QuitRequested = true;
        }
    }

    internal void ClientThreadDisplaySend(object? _) {
        if (QuitRequested) {
            return;
        }

        using var pipeServerDisplayResponse = new NamedPipeClientStream(".", "graphicsterminalfromdisplaypipe", PipeDirection.Out);
        try {
            pipeServerDisplayResponse.Connect();
            //Console.WriteLine("Client connected on thread[{0}].", threadId);
            while (!QuitRequested) {
                //try to collect input from kb
                if (!GraphicsResponseQueue.IsEmpty) {
                    byte b;
                    if (GraphicsResponseQueue.TryDequeue(out b)) {
                        pipeServerDisplayResponse.WriteByte(b);
                    }
                }
            }
        } catch (TimeoutException e) {
            //could do something to notify user, ask whether to retry, etc.
            Debug.WriteLine($"Exception in ClientThreadDisplaySend: {e.Message}");
        } catch (IOException e) {
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            //Console.WriteLine("ERROR: {0}\nPress Enter to close.", e.Message);
            //Console.ReadLine();
            Debug.WriteLine($"Exception in ClientThreadDisplaySend: {e.Message}");
        } finally {
            pipeServerDisplayResponse.Close();
            QuitRequested = true;
        }
    }

    internal enum cmdReadState {
        GetEsc, GetPeriod, GetCommandCode, GetParamsSize, GetParamsSizeExLow, GetParamsSizeExHigh, GetParams, GotCmd
    }
    internal void ClientThreadDisplayReceive(object? data) {
        if (QuitRequested) {
            return;
        }
        using var pipeServerDisplay = new NamedPipeClientStream(".", "graphicsterminaltodisplaypipe", PipeDirection.In);
        try {
            // Wait for a client to connect
            pipeServerDisplay.Connect();
            //pipeServerDisplay.ReadMode = PipeTransmissionMode.Byte; *** causes immediate app close (probably throws an exception)
            //Console.WriteLine("Client connected on thread[{0}].", threadId);
            GraphicsCommand cmd = new();
            var ParamsBytesRead = 0;
            cmdReadState CRState = cmdReadState.GetEsc;
            var cmdSetter = new cmdEnqueuer(enqueueCommand);
            int charCode;
            while (!QuitRequested && pipeServerDisplay.IsConnected) {
                if (CRState != cmdReadState.GotCmd) {
                    charCode = pipeServerDisplay.ReadByte();
                } else {
                    charCode = 0;
                }
                if (charCode == -1) {  // || 26 - this caused apparent crashes as 0x1a is valid in bin data!  //eof or ctrl+z
                    QuitRequested = true;
                } else {
                    //Console.Write((char)charCode);
                    //should run this on ui thread or send with an event?
                    //GraphicsCommandQueue.Enqueue((byte)charCode);
                    //assemble command to enqueue
                    switch (CRState) {
                        case cmdReadState.GetEsc:
                            if (charCode == 27) {
                                CRState = cmdReadState.GetPeriod;
                                cmd = new();
                            } else {
                                //we are out of sync, so discard byte and wait for Esc char
                            }
                            break;
                        case cmdReadState.GetPeriod:
                            if (charCode == (byte)'.') {
                                CRState = cmdReadState.GetCommandCode;
                            } else {
                                //lost sync, wait for Esc+'.'
                                CRState = cmdReadState.GetEsc;
                            }
                            break;
                        case cmdReadState.GetCommandCode:
                            cmd.gCommand = (byte)charCode;
                            CRState = cmdReadState.GetParamsSize;
                            ParamsBytesRead = 0;
                            break;
                        case cmdReadState.GetParamsSize:
                            if (charCode == 255) {
                                CRState = cmdReadState.GetParamsSizeExLow;
                            } else {
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
                            } else {
                                if (ParamsBytesRead == 0) {
                                    cmd.paramBytes = new byte[cmd.paramByteCount];
                                }
                                cmd.paramBytes[ParamsBytesRead++] = (byte)charCode;
                                if (ParamsBytesRead == cmd.paramByteCount) {
                                    CRState = cmdReadState.GotCmd;
                                }
                            }
                            break;
                        case cmdReadState.GotCmd:
                            break;
                    }
                    if (CRState == cmdReadState.GotCmd) {
                        //GraphicsCommandQueue.Enqueue(cmd); //unsafe!()

                        picDisplayPanel.Invoke(cmdSetter, new object[] { cmd });

                        CRState = cmdReadState.GetEsc;
                    }
                }
            }
        } catch (TimeoutException e) {
            Debug.WriteLine($"Exception in ClientThreadDisplayReceive: {e.Message}");
            //could do something to notify user, ask whether to retry, etc.
        } catch (IOException e) {
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            QuitRequested = true;
            Debug.WriteLine($"Exception in ClientThreadDisplayReceive: {e.Message}");
            Console.WriteLine("ERROR: {0}\nPress Enter to close.", e.Message);
            Console.ReadLine();
        } catch (Exception e) {
            Debug.WriteLine($"Exception in ClientThreadDisplayReceive: {e.Message}");
        } finally {
            pipeServerDisplay.Close();
            QuitRequested = true;
        }
    }

    internal delegate void cmdEnqueuer(GraphicsCommand cmd);

    internal void enqueueCommand(GraphicsCommand cmd) {
        //called from bg thread in ui context to avoid colliding with command execution/repaint ops
        GraphicsCommandQueue.Enqueue(cmd);
    }
    private void timer1_Tick(object sender, EventArgs e) {
        timer1.Enabled = false;
        PerformGraphicsCommands();
        if (QuitRequested && ((clientKBSend == null || !clientKBSend.IsAlive) ||
                              (clientDisplaySend == null || !clientDisplaySend.IsAlive) ||
                              (clientDisplayReceive == null || !clientDisplayReceive.IsAlive))) {
            timer1.Stop();
            Close(); //shutdown app
            return;
        }
        timer1.Enabled = true;
    }


    private void frmGraphicsAdapter_OnPaint(object sender, PaintEventArgs e) {
        //var gr = picDisplayPanel.CreateGraphics();
        var gr = e.Graphics;
        try {
            gr.DrawImage(bmp, 0, 0, 1024, 768);
        } catch (Exception) {
        } finally { 
            //gr.Dispose();
        }
    }
    private void frmGraphicsAdapter_KeyPress(object sender, KeyPressEventArgs e) {
        //queue up for send to emulator host
        var kc = e.KeyChar;
        KeypressQueue.Enqueue(kc);
        e.Handled = true;
    }
}

