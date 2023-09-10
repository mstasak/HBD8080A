using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Newtonsoft.Json.Linq;

namespace MFComputer.Hardware.Computer;
public class Cpu8080A {

    public DispatcherQueue? AppUIDispatcherQueue {
        get;
    }

    public Cpu8080A(DispatcherQueue? AppUIDispatcherQueue) {
        this.AppUIDispatcherQueue = AppUIDispatcherQueue;
        //Debug.WriteLine($"CPU8080A:ctor on thread \"{Thread.CurrentThread.Name}\", #{Thread.CurrentThread.ManagedThreadId}");
    }

    #region Flag Bits
    /// <summary>
    /// Sign flag constant.  This bit set indicates a negative value result from some instructions (MSB set).
    /// </summary>
    public const byte signflag = 0x80;
    
    /// <summary>
    /// Zero flag constant.  This bit set indicates a zero value result from some instructions.
    /// </summary>
    public const byte zeroflag = 0x40;

    /// <summary>
    /// Auxillary flag constant.  This bit set indicates a lower half-byte (nibble) carry/borrow result from some instructions.
    /// This is sometimes useful for BCD arithmetic.
    /// </summary>
    public const byte auxcarryflag = 0x10;

    /// <summary>
    /// Parity flag constant.  This bit set indicates an even number of 1 bits in the result of some instructions.
    /// </summary>
    public const byte parityflag = 0x04;

    /// <summary>
    /// Carry flag bit.  This indicates a carry/overflow or borrow/underflow from some arithmetic and comaprison instructions.
    /// </summary>
    public const byte carryflag = 0x01;

    /// <summary>
    /// These bits are always set to zero in the flags register.
    /// </summary>
    public const byte alwayszeroflags = 0x28;

    /// <summary>
    /// These bits are always set to one in the flags register.
    /// </summary>
    public const byte alwaysoneflags = 0x02;
    #endregion Flag Bits

    #region Machine State
    /// <summary>
    /// Is the PC running?  Usually false unless .Run() method is executing.
    /// </summary>
    public bool IsRunning;
    

    /// <summary>
    /// Are interrupts enabled?  Don't know if we will even simulate this.
    /// </summary>
    public bool IsInterruptsEnabled;

    public void Reset() {
        var running = IsRunning;
        Stop();
        A = 0;
        BC = 0;
        DE = 0;
        HL = 0;
        PC = 0;
        SP = 0;
        Flags = 0;
        IsInterruptsEnabled = false;
        if (running) {
            Run();
        }
    }

    public void Stop() {
        IsRunning = false;
    }
    #endregion Machine State

    #region Registers
    /// <summary>
    /// The "A" register (accumulator)
    /// </summary>
    public byte A;

    /// <summary>
    /// The "Flags" register (Sign,Zero,0,AuxCarry,0,Parity,1,Carry)
    /// </summary>
    public byte Flags;

    /// <summary>
    /// The "B" register (general purpose)
    /// </summary>
    public byte B;

    /// <summary>
    /// The "C" register (general purpose)
    /// </summary>
    public byte C;

    /// <summary>
    /// The "D" register (general purpose)
    /// </summary>
    public byte D;

    /// <summary>
    /// The "E" register (general purpose)
    /// </summary>
    public byte E;

    /// <summary>
    /// The "H" register (general purpose)
    /// </summary>
    public byte H;

    /// <summary>
    /// The "L" register (general purpose)
    /// </summary>
    public byte L;

    /// <summary>
    /// The "PC" register (Program Counter, aka Instruction Pointer)
    /// </summary>
    public ushort PC;

    /// <summary>
    /// The "SP" register (Stack Pointer)
    /// </summary>
    public ushort SP;

    /// <summary>
    /// Register pair BC.
    /// </summary>
    public ushort BC {
        get => BitManipulation.MakeWord(B, C); set => (B, C) = BitManipulation.SplitWord(value);
    }

    /// <summary>
    /// Register pair DE.
    /// </summary>
    public ushort DE {
        get => BitManipulation.MakeWord(D, E); set => (D, E) = BitManipulation.SplitWord(value);
    }

    /// <summary>
    /// Register pair HL.
    /// </summary>
    public ushort HL {
        get => BitManipulation.MakeWord(H, L); set => (H, L) = BitManipulation.SplitWord(value);
    }

    /// <summary>
    /// Processor status word, consisting of (high byte) accumulator and (low byte) flags.
    /// </summary>
    public ushort PSW {
        get => BitManipulation.MakeWord(A, Flags); set => (A, Flags) = BitManipulation.SplitWord(value);
    }
    #endregion Registers

    #region Memory
    /// <summary>
    /// The entire addressable memory space.  We treat 0x0000-0xefff as RAM, 0xf000-0xffff as ROM.
    /// </summary>
    public byte[] Memory = new byte[65536];

    /// <summary>
    /// Test if an address is in ROM (read-only memory).
    /// </summary>
    /// <param name="addr">Address to test</param>
    /// <returns>True if address points to read-only memory.</returns>
    public static bool IsROM(ushort addr) {
        return (addr & 0xf000) == 0xf000;
    }
    /// <summary>
    /// Byte of memory addressed by register pair HL.
    /// </summary>
    public byte M {
        get => Memory[HL];
        set {
            if (!IsROM(HL)) {
                Memory[HL] = value;
            }
        }
    }

    private byte FetchByte() { //can c# designate or hint an inline function/method?
        return Memory[PC++];
    }

    private ushort FetchWord() {
        //return (ushort)(fetchByte() | (ushort)(fetchByte() << 8));
        var newPC = PC;
        var lowByte = Memory[newPC++];
        var highByte = Memory[newPC++];
        PC = newPC;
        return BitManipulation.MakeWord(highByte, lowByte);
    }

    //private void pushByte(byte b) {
    //    Memory[--SP] = b;
    //}

    private void PushWord(ushort w) {
        var newSP = SP; //minimizing SP get/put calls for speed
        Memory[--newSP] = BitManipulation.HighByte(w);
        Memory[--newSP] = BitManipulation.LowByte(w);
        SP = newSP;
    }

    //private byte popByte() {
    //    return Memory[SP++];
    //}

    private ushort PopWord() {
        var newSP = SP;
        var lowByte = Memory[newSP++];
        var highByte = Memory[newSP++];
        SP = newSP;
        return BitManipulation.MakeWord(highByte, lowByte);
    }
    #endregion Memory

    #region Execution
    /// <summary>
    /// Enter run mode, until a halt instruction is encountered.
    /// We will need some means for external start-stop-reset-poweroff type control inputs.
    /// </summary>
    /// <param name="address">Address to begin execution, null for current PC, or default 0x100.</param>
    public void Run() {
        //Debug.WriteLine($"CPU8080A:Run() on thread \"{Thread.CurrentThread.Name}\", #{Thread.CurrentThread.ManagedThreadId}");
        IsRunning = true;
        while (IsRunning) {
            RunInstruction();
            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// Run one instruction.  PC is advanced past instruction and any operand bytes.
    /// </summary>
    /// <returns>Number of cycles instruction would use on actual 8080A hardware.</returns>
    public int RunInstruction() {
        var opcode = FetchByte();
        var cycles = 4;
        int iTmp;
        byte bTmp;
        byte flagsTmp;
        ushort wTmp;
        bool newCarry;
        bool newAuxCarry;
        //Debug.WriteLine($"Addr: {PC-1:X2}  Opcode: {Memory[PC-1]:X2} Operands?: {Memory[PC]:X2} {Memory[PC+1]:X2}");
        switch (opcode) {
            case 0x00:
                break; //nop (4 cycles)
            case 0x01:
                BC = FetchWord();
                cycles = 10;
                break; //lxi b
            case 0x02:
                Memory[BC] = A;
                cycles = 7;
                break; //stax b
            case 0x03:
                unchecked { BC++; }
                cycles = 5;
                break; //inx b
            case 0x04:
                unchecked { B++; }
                //standard zero, sign, parity handling
                SetFlagsZPS(B);
                //also set ac on right nibl overflow
                Flags = (byte)((Flags & ~auxcarryflag) | ((B & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr b
            case 0x05:
                unchecked { B--; }
                //standard zero, sign, parity handling
                SetFlagsZPS(B);
                //also set ac on right nibl underflow CONCERN: borrows may not set ac as expected - maybe set to 1 then clear when borrow occurs? find a test suite!
                Flags = (byte)((Flags & ~auxcarryflag) | (((B & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr b
            case 0x06:
                B = FetchByte();
                cycles = 7;
                break; //mvi b,d8
            case 0x07: {
                    bTmp = (byte)(((A << 1) & 0xfe) | (A >> 7));
                    Flags = (byte)((Flags & ~carryflag) | (((A & 1) != 0) ? carryflag : 0));
                    A = bTmp;
                }
                break; //rlc (4 cycles)
            case 0x08: break;
            case 0x09:
                iTmp = HL + BC;
                HL = unchecked((ushort)iTmp);
                Flags = (byte)((Flags & ~carryflag) | ((iTmp > 0xffff) ? carryflag : 0));
                cycles = 10;
                break; //dad b
            case 0x0a:
                A = Memory[BC];
                cycles = 7;
                break; //ldax b
            case 0x0b:
                unchecked { BC--; }
                cycles = 5;
                break; //dcx b
            case 0x0c:
                unchecked { C++; }
                SetFlagsZPS(C);
                Flags = (byte)((Flags & ~auxcarryflag) | ((C & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr c
            case 0x0d:
                unchecked { C--; }
                SetFlagsZPS(C);
                Flags = (byte)((Flags & ~auxcarryflag) | (((C & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr c
            case 0x0e:
                C = FetchByte();
                cycles = 7;
                break; //mvi c,d8
            case 0x0f:
                A = (byte)((A >> 1) | (((A & 0x01) != 0) ? 0x80 : 0));
                Flags = (byte)((Flags & ~carryflag) | (((A & 0x80) != 0) ? carryflag : 0));
                break; //rrc (4 cycles)
            case 0x10: break;
            case 0x11:
                DE = FetchWord();
                cycles = 10;
                break; //lxi d
            case 0x12:
                Memory[DE] = A;
                cycles = 7;
                break; //stax d
            case 0x13:
                unchecked { DE++; }
                cycles = 5;
                break; //inx d
            case 0x14:
                unchecked { D++; }
                SetFlagsZPS(D);
                Flags = (byte)((Flags & ~auxcarryflag) | ((D & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr d
            case 0x15:
                unchecked { D--; }
                SetFlagsZPS(D);
                Flags = (byte)((Flags & ~auxcarryflag) | (((D & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr d
            case 0x16:
                D = FetchByte();
                cycles = 7;
                break; //mvi d,d8
            case 0x17:
                bTmp = A;
                newCarry = (bTmp & 0x80) != 0;
                A = (byte)((bTmp << 1) | (((Flags & carryflag) != 0) ? 1 : 0));
                Flags = (byte)((Flags & ~carryflag) | (newCarry ? carryflag : 0));
                break; //ral (4 cycles)
            case 0x18: break;
            case 0x19:
                iTmp = HL + DE;
                HL = unchecked((ushort)iTmp);
                Flags = (iTmp > 0xffff) ? (byte)(Flags | carryflag) : (byte)(Flags & ~carryflag);
                cycles = 10;
                break; //dad d
            case 0x1a:
                A = Memory[DE];
                cycles = 7;
                break; //ldax d
            case 0x1b:
                unchecked { DE--; }
                cycles = 5;
                break; //dcx d
            case 0x1c:
                unchecked { E++; }
                SetFlagsZPS(E);
                Flags = (byte)((Flags & ~auxcarryflag) | ((E & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr e
            case 0x1d:
                unchecked { E--; }
                SetFlagsZPS(E);
                Flags = (byte)((Flags & ~auxcarryflag) | (((E & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr e
            case 0x1e:
                E = FetchByte();
                cycles = 7;
                break; //mvi e,d8
            case 0x1f:
                bTmp = A;
                newCarry = (bTmp & 0x01) != 0;
                A = (byte)((bTmp >> 1) | (((Flags & carryflag) != 0) ? 0x80 : 0));
                Flags = (byte)((Flags & ~carryflag) | (newCarry ? carryflag : 0));
                break; //rar (4 cycles)
            case 0x20: break;
            case 0x21:
                HL = FetchWord();
                cycles = 10;
                break; //lxi h
            case 0x22:
                wTmp = FetchWord();
                Memory[wTmp++] = L; //L register
                Memory[wTmp] = H;
                cycles = 16;
                break; //shld a16
            case 0x23:
                unchecked { HL++; }
                cycles = 5;
                break; //inx h
            case 0x24:
                unchecked { H++; }
                SetFlagsZPS(H);
                Flags = (byte)((Flags & ~auxcarryflag) | ((H & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr h
            case 0x25:
                unchecked { H--; }
                SetFlagsZPS(H);
                Flags = (byte)((Flags & ~auxcarryflag) | (((H & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr h
            case 0x26:
                H = FetchByte();
                cycles = 7;
                break; //mvi h,d8
            case 0x27:
                flagsTmp = Flags;
                bTmp = A;
                if (((bTmp & 0x0f) > 9) || ((flagsTmp & auxcarryflag) != 0)) {
                    flagsTmp = (byte)((flagsTmp & ~auxcarryflag) | (((bTmp & 0x0f) > 9) ? auxcarryflag : 0));
                    bTmp = (byte)(bTmp + 6);
                } else {
                    flagsTmp = (byte)(flagsTmp & (~auxcarryflag));
                }
                if (((bTmp & 0xf0) > 0x90) || ((flagsTmp & carryflag) != 0)) {
                    flagsTmp = (byte)((flagsTmp & (~carryflag)) | (((bTmp & 0xf0) > 0x90) ? carryflag : 0));
                    bTmp = (byte)(bTmp + 0x60);
                } else {
                    flagsTmp = (byte)(flagsTmp & (~carryflag));
                }
                Flags = flagsTmp;
                A = bTmp;
                SetFlagsZPS(bTmp);
                cycles = 4;
                break; //daa
            case 0x28: break;
            case 0x29:
                iTmp = HL + HL;
                HL = unchecked((ushort)iTmp);
                Flags = (iTmp > 0xffff) ? (byte)(Flags | carryflag) : (byte)(Flags & ~carryflag);
                cycles = 10;
                break; //dad h
            case 0x2a:
                wTmp = FetchWord();
                HL = (ushort)((Memory[wTmp++]) | (Memory[wTmp] << 8));
                cycles = 16;
                break; //lhld a16
            case 0x2b:
                unchecked { HL--; }
                cycles = 5;
                break; //dcx h
            case 0x2c:
                unchecked { L++; }
                SetFlagsZPS(L);
                Flags = (byte)((Flags & ~auxcarryflag) | ((L & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr l
            case 0x2d:
                unchecked { L--; }
                SetFlagsZPS(L);
                Flags = (byte)((Flags & ~auxcarryflag) | (((L & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr l
            case 0x2e:
                L = FetchByte();
                cycles = 7;
                break; //mvi l,d8
            case 0x2f:
                A = (byte)~A;
                //cycles = 4;
                break; //cma
            case 0x30: break;
            case 0x31:
                SP = FetchWord();
                cycles = 10;
                break; //lxi sp
            case 0x32:
                Memory[FetchWord()] = A;
                cycles = 13;
                break; //sta a16
            case 0x33:
                unchecked { SP++; }
                cycles = 5;
                break; //inx sp
            case 0x34:
                unchecked { M++; }
                SetFlagsZPS(M);
                Flags = (byte)((Flags & ~auxcarryflag) | ((M & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 10;
                break; //inr m
            case 0x35:
                unchecked { M--; }
                SetFlagsZPS(M);
                Flags = (byte)((Flags & ~auxcarryflag) | (((M & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 10;
                break; //dcr m
            case 0x36:
                M = FetchByte();
                cycles = 10;
                break; //mvi m,d8
            case 0x37:
                Flags = (byte)(Flags | carryflag);
                //cycles = 4;
                break; //STC
            case 0x38: break;
            case 0x39:
                iTmp = HL + SP;
                HL = unchecked((ushort)iTmp);
                Flags = (iTmp > 0xffff) ? (byte)(Flags | carryflag) : (byte)(Flags & ~carryflag);
                cycles = 10;
                break; //dad sp
            case 0x3a:
                A = Memory[FetchWord()];
                cycles = 13;
                break; //lda a16
            case 0x3b:
                unchecked { SP--; }
                cycles = 5;
                break; //dcx sp
            case 0x3c:
                unchecked { A++; }
                SetFlagsZPS(A);
                Flags = (byte)((Flags & ~auxcarryflag) | ((A & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr a
            case 0x3d:
                unchecked { A--; }
                SetFlagsZPS(A);
                Flags = (byte)((Flags & ~auxcarryflag) | (((A & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr a
            case 0x3e:
                A = FetchByte();
                cycles = 7;
                break; //mvi a,d8
            case 0x3f:
                Flags = (byte)(Flags ^ carryflag);
                break; //CMC
            case 0x40:
                cycles = 5;
                break; //mov b,b
            case 0x41:
                B = C;
                cycles = 5;
                break; //mov b,c
            case 0x42:
                B = D;
                cycles = 5;
                break; //mov b,d
            case 0x43:
                B = E;
                cycles = 5;
                break; //mov b,e
            case 0x44:
                B = H;
                cycles = 5;
                break; //mov b,h
            case 0x45:
                B = L;
                cycles = 5;
                break; //mov b,l
            case 0x46:
                B = M;
                cycles = 7;
                break; //mov b,m
            case 0x47:
                B = A;
                cycles = 5;
                break; //mov b,a
            case 0x48:
                C = B;
                cycles = 5;
                break; //mov c,b
            case 0x49:
                cycles = 5;
                break; //mov c,c
            case 0x4a:
                C = D;
                cycles = 5;
                break; //mov c,d
            case 0x4b:
                C = E;
                cycles = 5;
                break; //mov c,e
            case 0x4c:
                C = H;
                cycles = 5;
                break; //mov c,h
            case 0x4d:
                C = L;
                cycles = 5;
                break; //mov c,l
            case 0x4e:
                C = M;
                cycles = 7;
                break; //mov c,m
            case 0x4f:
                C = A;
                cycles = 5;
                break; //mov c,a
            case 0x50:
                D = B;
                cycles = 5;
                break; //mov d,b
            case 0x51:
                D = C;
                cycles = 5;
                break; //mov d,c
            case 0x52:
                cycles = 5;
                break; //mov d,d
            case 0x53:
                D = E;
                cycles = 5;
                break; //mov d,e
            case 0x54:
                D = H;
                cycles = 5;
                break; //mov d,h
            case 0x55:
                D = L;
                cycles = 5;
                break; //mov d,l
            case 0x56:
                D = M;
                cycles = 7;
                break; //mov d,m
            case 0x57:
                D = A;
                cycles = 5;
                break; //mov d,a
            case 0x58:
                E = B;
                cycles = 5;
                break; //mov e,b
            case 0x59:
                E = C;
                cycles = 5;
                break; //mov e,c
            case 0x5a:
                E = D;
                cycles = 5;
                break; //mov e,d
            case 0x5b:
                cycles = 5;
                break; //mov e,e
            case 0x5c:
                E = H;
                cycles = 5;
                break; //mov e,h
            case 0x5d:
                E = L;
                cycles = 5;
                break; //mov e,l
            case 0x5e:
                E = M;
                cycles = 7;
                break; //mov e,m
            case 0x5f:
                E = A;
                cycles = 5;
                break; //mov e,a
            case 0x60:
                H = B;
                cycles = 5;
                break; //mov h,b
            case 0x61:
                H = C;
                cycles = 5;
                break; //mov h,c
            case 0x62:
                H = D;
                cycles = 5;
                break; //mov h,d
            case 0x63:
                H = E;
                cycles = 5;
                break; //mov h,e
            case 0x64:
                cycles = 5;
                break; //mov h,h
            case 0x65:
                H = L;
                cycles = 5;
                break; //mov h,l
            case 0x66:
                H = M;
                cycles = 7;
                break; //mov h,m
            case 0x67:
                H = A;
                cycles = 5;
                break; //mov h,a
            case 0x68:
                L = B;
                cycles = 5;
                break; //mov l,b
            case 0x69:
                L = C;
                cycles = 5;
                break; //mov l,c
            case 0x6a:
                L = D;
                cycles = 5;
                break; //mov l,d
            case 0x6b:
                L = E;
                cycles = 5;
                break; //mov l,e
            case 0x6c:
                L = H;
                cycles = 5;
                break; //mov l,h
            case 0x6d:
                cycles = 5;
                break; //mov l,l
            case 0x6e:
                L = M;
                cycles = 7;
                break; //mov l,m
            case 0x6f:
                L = A;
                cycles = 5;
                break; //mov l,a
            case 0x70:
                M = B;
                cycles = 7;
                break; //mov m,b
            case 0x71:
                M = C;
                cycles = 7;
                break; //mov m,c
            case 0x72:
                M = D;
                cycles = 7;
                break; //mov m,d
            case 0x73:
                M = E;
                cycles = 7;
                break; //mov m,e
            case 0x74:
                M = H;
                cycles = 7;
                break; //mov m,h
            case 0x75:
                M = L;
                cycles = 7;
                break; //mov m,l
            case 0x76:
                cycles = 7;
                IsRunning = false;
                break; //hlt
            case 0x77:
                M = A;
                cycles = 7;
                break; //mov m,a
            case 0x78:
                A = B;
                cycles = 5;
                break; //mov a,b
            case 0x79:
                A = C;
                cycles = 5;
                break; //mov a,c
            case 0x7a:
                A = D;
                cycles = 5;
                break; //mov a,d
            case 0x7b:
                A = E;
                cycles = 5;
                break; //mov a,e
            case 0x7c:
                A = H;
                cycles = 5;
                break; //mov a,h
            case 0x7d:
                A = L;
                cycles = 5;
                break; //mov a,l
            case 0x7e:
                A = M;
                cycles = 7;
                break; //mov a,m
            case 0x7f:
                cycles = 5;
                break; //mov a,a
            case 0x80:
                bTmp = A;
                wTmp = (ushort)(bTmp + B);
                newAuxCarry = (A & 0x0f) + (B & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add b
            case 0x81:
                bTmp = A;
                wTmp = (ushort)(bTmp + C);
                newAuxCarry = (A & 0x0f) + (C & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add c
            case 0x82:
                bTmp = A;
                wTmp = (ushort)(bTmp + D);
                newAuxCarry = (A & 0x0f) + (D & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add d
            case 0x83:
                bTmp = A;
                wTmp = (ushort)(bTmp + E);
                newAuxCarry = (A & 0x0f) + (E & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add e
            case 0x84:
                bTmp = A;
                wTmp = (ushort)(bTmp + H);
                newAuxCarry = (A & 0x0f) + (H & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add h
            case 0x85:
                bTmp = A;
                wTmp = (ushort)(bTmp + L);
                newAuxCarry = (A & 0x0f) + (L & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add l
            case 0x86:
                bTmp = A;
                wTmp = (ushort)(bTmp + M);
                newAuxCarry = (A & 0x0f) + (M & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //add m
            case 0x87:
                bTmp = A;
                wTmp = (ushort)(bTmp + A);
                newAuxCarry = (A & 0x0f) + (A & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add a
            case 0x88:
                bTmp = A;
                wTmp = (ushort)(bTmp + B + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (B & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc b
            case 0x89:
                bTmp = A;
                wTmp = (ushort)(bTmp + C + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (C & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc c
            case 0x8a:
                bTmp = A;
                wTmp = (ushort)(bTmp + D + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (D & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc d
            case 0x8b:
                bTmp = A;
                wTmp = (ushort)(bTmp + E + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (E & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc e
            case 0x8c:
                bTmp = A;
                wTmp = (ushort)(bTmp + H + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (H & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc h
            case 0x8d:
                bTmp = A;
                wTmp = (ushort)(bTmp + L + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (L & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc l
            case 0x8e:
                bTmp = A;
                wTmp = (ushort)(bTmp + M + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (M & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //adc m
            case 0x8f:
                bTmp = A;
                wTmp = (ushort)(bTmp + A + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (A & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc a
            case 0x90:
                bTmp = A;
                wTmp = (ushort)(bTmp - B);
                newAuxCarry = (A & 0x0f) - (B & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub b
            case 0x91:
                bTmp = A;
                wTmp = (ushort)(bTmp - C);
                newAuxCarry = (A & 0x0f) - (C & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub c
            case 0x92:
                bTmp = A;
                wTmp = (ushort)(bTmp - D);
                newAuxCarry = (A & 0x0f) - (D & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub d
            case 0x93:
                bTmp = A;
                wTmp = (ushort)(bTmp - E);
                newAuxCarry = (A & 0x0f) - (E & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub e
            case 0x94:
                bTmp = A;
                wTmp = (ushort)(bTmp - H);
                newAuxCarry = (A & 0x0f) - (H & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub h
            case 0x95:
                bTmp = A;
                wTmp = (ushort)(bTmp - L);
                newAuxCarry = (A & 0x0f) - (L & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub l
            case 0x96:
                bTmp = A;
                wTmp = (ushort)(bTmp - M);
                newAuxCarry = (A & 0x0f) - (M & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //sub m
            case 0x97:
                bTmp = A;
                wTmp = (ushort)(bTmp - A);
                newAuxCarry = (A & 0x0f) - (A & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub a
            case 0x98:
                bTmp = A;
                wTmp = (ushort)(bTmp - B - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (B & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb b
            case 0x99:
                bTmp = A;
                wTmp = (ushort)(bTmp - C - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (C & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb c
            case 0x9a:
                bTmp = A;
                wTmp = (ushort)(bTmp - D - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (D & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb d
            case 0x9b:
                bTmp = A;
                wTmp = (ushort)(bTmp - E - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (E & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb e
            case 0x9c:
                bTmp = A;
                wTmp = (ushort)(bTmp - H - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (H & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb h
            case 0x9d:
                bTmp = A;
                wTmp = (ushort)(bTmp - L - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (L & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb l
            case 0x9e:
                bTmp = A;
                wTmp = (ushort)(bTmp - M - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (M & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //sbb m
            case 0x9f:
                bTmp = A;
                wTmp = (ushort)(bTmp - A - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (A & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb a
            case 0xa0:
                A = (byte)(A & B);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana b
            case 0xa1:
                A = (byte)(A & C);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana c
            case 0xa2:
                A = (byte)(A & D);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana d
            case 0xa3:
                A = (byte)(A & E);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana e
            case 0xa4:
                A = (byte)(A & H);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana h
            case 0xa5:
                A = (byte)(A & L);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana l
            case 0xa6:
                A = (byte)(A & M);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                cycles = 7;
                break; //ana m
            case 0xa7:
                A = (byte)(A & A);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana a
            case 0xa8:
                A = (byte)(A ^ B);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra b
            case 0xa9:
                A = (byte)(A ^ C);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra c
            case 0xaa:
                A = (byte)(A ^ D);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra d
            case 0xab:
                A = (byte)(A ^ E);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra e
            case 0xac:
                A = (byte)(A ^ H);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra h
            case 0xad:
                A = (byte)(A ^ L);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra l
            case 0xae:
                A = (byte)(A ^ M);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                cycles = 7;
                break; //xra m
            case 0xaf:
                A = (byte)(A ^ A);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra a
            case 0xb0:
                A = (byte)(A | B);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora b
            case 0xb1:
                A = (byte)(A | C);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora c
            case 0xb2:
                A = (byte)(A | D);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora d
            case 0xb3:
                A = (byte)(A | E);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora e
            case 0xb4:
                A = (byte)(A | H);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora h
            case 0xb5:
                A = (byte)(A | L);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora l
            case 0xb6:
                A = (byte)(A | M);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                cycles = 7;
                break; //ora m
            case 0xb7:
                A = (byte)(A | A);
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora a
            case 0xb8:
                bTmp = A;
                wTmp = (ushort)(bTmp - B);
                newAuxCarry = (A & 0x0f) - (B & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp b
            case 0xb9:
                bTmp = A;
                wTmp = (ushort)(bTmp - C);
                newAuxCarry = (A & 0x0f) - (C & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp c
            case 0xba:
                bTmp = A;
                wTmp = (ushort)(bTmp - D);
                newAuxCarry = (A & 0x0f) - (D & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp d
            case 0xbb:
                bTmp = A;
                wTmp = (ushort)(bTmp - E);
                newAuxCarry = (A & 0x0f) - (E & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp e
            case 0xbc:
                bTmp = A;
                wTmp = (ushort)(bTmp - H);
                newAuxCarry = (A & 0x0f) - (H & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp h
            case 0xbd:
                bTmp = A;
                wTmp = (ushort)(bTmp - L);
                newAuxCarry = (A & 0x0f) - (L & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp l
            case 0xbe:
                bTmp = A;
                wTmp = (ushort)(bTmp - M);
                newAuxCarry = (A & 0x0f) - (M & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //cmp m
            case 0xbf:
                bTmp = A;
                wTmp = (ushort)(bTmp - A);
                newAuxCarry = (A & 0x0f) - (A & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp a
            case 0xc0:
                if ((Flags & zeroflag) == 0) {
                    PC = PopWord();
                    cycles = 11;
                } else {
                    cycles = 5;
                }
                break; //rnz
            case 0xc1:
                BC = PopWord();
                cycles = 10;
                break; //pop b
            case 0xc2:
                wTmp = FetchWord();
                if ((Flags & zeroflag) == 0) {
                    PC = wTmp;
                }
                cycles = 10;
                break; //jnz a16
            case 0xc3:
                PC = FetchWord();
                cycles = 10;
                break; //jmp a16
            case 0xc4:
                wTmp = FetchWord();
                if ((Flags & zeroflag) == 0) {
                    PushWord(PC);
                    PC = wTmp;
                    cycles = 17;
                } else {
                    cycles = 11;
                }
                break; //cnz a16
            case 0xc5:
                PushWord(BC);
                cycles = 11;
                break; //push b
            case 0xc6:
                bTmp = FetchByte();
                wTmp = (ushort)(A + bTmp);
                newAuxCarry = (A & 0x0f) + (bTmp & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //adi d8
            case 0xc7:
                PushWord(PC);
                PC = 0x0000;
                cycles = 11;
                break; //rst 0
            case 0xc8:
                if ((Flags & zeroflag) != 0) {
                    PC = PopWord();
                    cycles = 11;
                } else {
                    cycles = 5;
                }
                break; //rz
            case 0xc9:
                PC = PopWord();
                cycles = 10;
                break; //ret
            case 0xca:
                wTmp = FetchWord();
                if ((Flags & zeroflag) != 0) {
                    PC = wTmp;
                }
                cycles = 10;
                break; //jz a16
            case 0xcb: break;
            case 0xcc:
                wTmp = FetchWord();
                if ((Flags & zeroflag) != 0) {
                    PushWord(PC);
                    PC = wTmp;
                    cycles = 17;
                } else {
                    cycles = 11;
                }
                break; //cz a16
            case 0xcd:
                wTmp = FetchWord();
                PushWord(PC);
                PC = wTmp;
                cycles = 17;
                break; //call
            case 0xce:
                bTmp = FetchByte();
                wTmp = (ushort)(A + bTmp + (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) + (bTmp & 0x0f) + (((Flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //aci d8
            case 0xcf:
                PushWord(PC);
                PC = 0x0008;
                cycles = 11;
                break; //rst 1
            case 0xd0:
                cycles = 5;
                if ((Flags & carryflag) == 0) {
                    PC = PopWord();
                    cycles = 11;
                }
                break; //rnc
            case 0xd1:
                DE = PopWord();
                cycles = 10;
                break; //pop d
            case 0xd2:
                wTmp = FetchWord();
                if ((Flags & carryflag) == 0) {
                    PC = wTmp;
                }
                cycles = 10;
                break; //jnc a16
            case 0xd3:
                PortOutput(port: FetchByte(), value: A);
                cycles = 10;
                break; //out port8
            case 0xd4:
                wTmp = FetchWord();
                if ((Flags & carryflag) == 0) {
                    PushWord(PC);
                    PC = wTmp;
                    cycles = 17;
                } else {
                    cycles = 11;
                }
                break; //cnc a16
            case 0xd5:
                PushWord(DE);
                cycles = 11;
                break; //push d
            case 0xd6:
                bTmp = FetchByte();
                wTmp = (ushort)(A - bTmp);
                newAuxCarry = (A & 0x0f) - (bTmp & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //sui d8
            case 0xd7:
                PushWord(PC);
                PC = 0x0010;
                cycles = 11;
                break; //rst 2
            case 0xd8:
                cycles = 5;
                if ((Flags & carryflag) != 0) {
                    PC = PopWord();
                    cycles = 11;
                }
                break; //rc
            case 0xd9: break;
            case 0xda:
                wTmp = FetchWord();
                if ((Flags & carryflag) != 0) {
                    PC = wTmp;
                }
                cycles = 10;
                break; //jc a16
            case 0xdb:
                //RequestPortInput(port: FetchByte(), handler: genericInputHandler);
                //A = inputValue;
                A = PortInput(port: FetchByte());
                cycles = 10;
                break; //in port8
            case 0xdc:
                wTmp = FetchWord();
                if ((Flags & carryflag) != 0) {
                    PushWord(PC);
                    PC = wTmp;
                    cycles = 17;
                } else {
                    cycles = 11;
                }
                break; //cc a16
            case 0xdd: break;
            case 0xde:
                bTmp = FetchByte();
                wTmp = (ushort)(A - bTmp - (((Flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (A & 0x0f) - (bTmp & 0x0f) - (((Flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                A = unchecked((byte)wTmp);
                SetFlagsZPS(A);
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //sbi d8
            case 0xdf:
                PushWord(PC);
                PC = 0x0018;
                cycles = 11;
                break; //rst 3
            case 0xe0:
                cycles = 5;
                if ((Flags & parityflag) == 0) {
                    PC = PopWord();
                    cycles = 11;
                }
                break; //rpo
            case 0xe1:
                HL = PopWord();
                cycles = 10;
                break; //pop h
            case 0xe2:
                wTmp = FetchWord();
                if ((Flags & parityflag) == 0) {
                    PC = wTmp;
                }
                cycles = 10;
                break; //jpo a16
            case 0xe3:
                //TODO: optimize? - swap instead of pop-then-push
                //suggest implementing topofstack property, memoryword(lowbyteaddress) ref method, use (tos, hl) = (hl,tos);
                //maybe not, this would get tricky in edge case with SP=0xffff (however unlikely, aside from 33 C7 memory clear program
                //which is INX SP, RST 0)
                wTmp = PopWord();
                PushWord(HL);
                HL = wTmp;
                cycles = 18;
                break; //xtlh
            case 0xe4:
                wTmp = FetchWord();
                if ((Flags & parityflag) == 0) {
                    PushWord(PC);
                    PC = wTmp;
                    cycles = 17;
                } else {
                    cycles = 11;
                }
                break; //cpo a16
            case 0xe5:
                PushWord(HL);
                cycles = 11;
                break; //push h
            case 0xe6:
                A = (byte)(A & FetchByte());
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~(carryflag | auxcarryflag))); //CY,AC
                cycles = 7;
                break; //ani d8
            case 0xe7:
                PushWord(PC);
                PC = 0x0020;
                cycles = 11;
                break; //rst 4
            case 0xe8:
                cycles = 5;
                if ((Flags & parityflag) != 0) {
                    PC = PopWord();
                    cycles = 11;
                }
                break; //rpe
            case 0xe9:
                PC = HL;
                cycles = 5;
                break; //pchl
            case 0xea:
                wTmp = FetchWord();
                if ((Flags & parityflag) != 0) {
                    PC = wTmp;
                }
                cycles = 10;
                break; //jpe a16
            case 0xeb:
                //wTmp = hl;
                //hl = de;
                //de = wTmp;
                (HL, DE) = (DE, HL); //curious, does this optimize to the same code?  does it compile slower?
                //cycles = 4;
                break; //xchg (DE, HL)
            case 0xec:
                wTmp = FetchWord();
                if ((Flags & parityflag) != 0) {
                    PushWord(PC);
                    PC = wTmp;
                    cycles = 17;
                } else {
                    cycles = 11;
                }
                break; //cpe a16
            case 0xed: break;
            case 0xee:
                A = (byte)(A ^ FetchByte());
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~(carryflag | auxcarryflag))); //CY,AC
                cycles = 7;
                break; //xri d8 (note: intel docs are inconsistent on resulting C, AC flags for many arithmetic and logical operators)
            case 0xef:
                PushWord(PC);
                PC = 0x0028;
                cycles = 11;
                break; //rst 5
            case 0xf0:
                cycles = 5;
                if ((Flags & signflag) == 0) {
                    PC = PopWord();
                    cycles = 11;
                }
                break; //rp
            case 0xf1:
                wTmp = PopWord();
                //set fixed flag bits appropriately (flags are in low byte, a in highbyte)
                wTmp = (ushort)((wTmp | alwaysoneflags) & ~alwayszeroflags);
                PSW = wTmp;
                cycles = 10;
                break; //pop psw
            case 0xf2:
                wTmp = FetchWord();
                if ((Flags & signflag) == 0) {
                    PC = wTmp;
                }
                cycles = 10;
                break; //jp a16
            case 0xf3:
                IsInterruptsEnabled = false;
                //cycles = 4;
                break; //di
            case 0xf4:
                wTmp = FetchWord();
                if ((Flags & signflag) == 0) {
                    PushWord(PC);
                    PC = wTmp;
                    cycles = 17;
                } else {
                    cycles = 11;
                }
                break; //cp a16
            case 0xf5:
                PushWord(PSW);
                cycles = 11;
                break; //push psw
            case 0xf6:
                A = (byte)(A | FetchByte());
                SetFlagsZPS(A);
                Flags = (byte)(Flags & (~(carryflag | auxcarryflag))); //CY,AC
                cycles = 7;
                break; //ori d8
            case 0xf7:
                PushWord(PC);
                PC = 0x0030;
                cycles = 11;
                break; //rst 6
            case 0xf8:
                cycles = 5;
                if ((Flags & signflag) != 0) {
                    PC = PopWord();
                    cycles = 11;
                }
                break; //rm
            case 0xf9:
                SP = HL;
                cycles = 5;
                break; //sphl
            case 0xfa:
                wTmp = FetchWord();
                if ((Flags & signflag) != 0) {
                    PC = wTmp;
                }

                cycles = 10;
                break; //jm a16
            case 0xfb:
                IsInterruptsEnabled = true;
                //cycles = 4;
                break; //ei
            case 0xfc:
                wTmp = FetchWord();
                if ((Flags & signflag) != 0) {
                    PushWord(PC);
                    PC = wTmp;
                    cycles = 17;
                } else {
                    cycles = 11;
                }
                break; //cm a16
            case 0xfd: break;
            case 0xfe:
                bTmp = FetchByte();
                wTmp = (ushort)(A - bTmp);
                newAuxCarry = (A & 0x0f) - (bTmp & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                SetFlagsZPS(unchecked((byte)wTmp));
                Flags = (byte)((Flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //cpi d8
            case 0xff:
                PushWord(PC);
                PC = 0x0038;
                cycles = 11;
                break; //rst 7
                       //default: break;
        }
        //Thread.Sleep(1);
        return cycles;
    }

    /// <summary>
    /// Calculate the zero, sign, and parity flags based on value r.  Update Flags with result.
    /// Carry and Aux Carry cannot be calculated without original additional data (opcode and
    /// operands and perhaps (esp. for DAA instruction) AC and C flags before instruction).
    /// </summary>
    /// <param name="r">The value used to set these flags.</param>
    private void SetFlagsZPS(byte r) {
        var tmpFlags = (byte)(Flags & (~(zeroflag | parityflag | signflag)));
        if (r == 0) {
            tmpFlags |= zeroflag;
        }
        if (BitManipulation.ParityEven(r)) {
            tmpFlags |= parityflag;
        }
        if ((r & 0x80) != 0) {
            tmpFlags |= signflag;
        }
        Flags = tmpFlags;
    }
    #endregion Execution

    #region Input/Output

    // ref https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.dispatching.dispatcherqueue?view=windows-app-sdk-1.4
    // ref https://learn.microsoft.com/en-us/windows/communitytoolkit/extensions/dispatcherqueueextensions

    public delegate void OutputAction(byte port, byte value);
    public OutputAction? Outputter;

    public delegate byte InputAction(byte port);
    public InputAction? Inputter;
    public static byte InPortFF;

    //public delegate void InputReceiver(byte port, bool handled, byte value);
    //private bool inputHandled;
    //private byte inputPort;
    //private byte inputValue;
    //private void genericInputHandler(byte port, bool handled, byte value) {
    //    if (handled) {
    //        inputPort = port;
    //        inputValue = value;
    //        inputHandled = true;
    //    }
    //}




    /// <summary>
    /// Send port output data to any enrolled delegates.
    /// </summary>
    /// <param name="port">Port specified by "OUT" instruction.</param>
    /// <param name="value">Value from accumulator.</param>
    private void PortOutput(byte port, byte value) {
        //TODO: MAYBE keep an array or dictionary of outputdelegates keyed by port#?
        //This would optimize output on a system with numerous ports used.
        if (Outputter is not null) {
            //Outputter(port, value);
            //Outputter.Invoke(port, value);
            //var dq = App.MainWindow.DispatcherQueue;
            //var dp = App.MainWindow.Dispatcher;
            //if (dq.HasThreadAccess) {
            //    Outputter(port, value);
            //}
            //else {
            //    bool isQueued = dq.TryEnqueue(
            //    Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
            //    () => Outputter(port, value));
            //}
            
            foreach (var Delegate in Outputter.GetInvocationList()) {
                var outAction = Delegate as OutputAction;
                _ = AppUIDispatcherQueue?.TryEnqueue(
                    DispatcherQueuePriority.Normal,
                    () => (outAction)?.Invoke(port, value)
                );
            }

        }
    }


    // PROBLEM: Making a conceptual error here, as there is no polling mechanism to invoke all input delegates and capture return from only the right one.
    // I guess we should have just one delegate per port, or maybe use a ref parameter to let each delegate indicate whether they returned a response or just said "not my port"?????
    // (I'm probably abusing the terminology as well)
    /// <summary>
    /// Request input port data from any enrolled delegates.
    /// </summary>
    /// <param name="port">Port specified by "IN" instruction.</param>
    /// <returns>Data from input source, to be stored into accumulator.</returns>
    private byte PortInput(byte port) {
        //TODO: MAYBE keep an array or dictionary of outputdelegates keyed by port#?
        //This would optimize input on a system with numerous ports used (may be unlikely).
        if (port == 0xff) {
            return Cpu8080A.InPortFF;    
        }
        byte inputValue = 0xff;
        if (Inputter is not null) {
            if (AppUIDispatcherQueue is not null) {
                foreach (var Delegate in Inputter.GetInvocationList()) {
                    //inputValue = await AppUIDispatcherQueue.EnqueueAsync(
                    //    (Delegate as InputAction),
                    //    Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal
                    //);
                }
            }
        }
        return inputValue;
    }



    #endregion Input/Output

}


//TODO: remove unchecked statements and expressions, as this is default mode
//TODO: add readonly range checks to all memory writes
//TODO: fix input issue
//TODO: run on background thread, with external control inputs to change machine state (run/stop/throttle/single step/startup/shutdown/reset)
//TODO: (in XAML) set up a "front panel" to simulate input switches, output leds, PC and flag displays, and a small region of memory; include single step button
//TODO: optimization - use 256-byte lookup table for Z,S,P flags for speed (parity addition is slow)