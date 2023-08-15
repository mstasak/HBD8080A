using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using Windows.Data.Text;
using Windows.UI.ViewManagement.Core;

namespace MFComputer.Hardware.Computer;
internal class Cpu8080A
{

    const byte signflag = 0x80; //1=negative (msb set)
    const byte zeroflag = 0x40;
    const byte auxcarryflag = 0x10;
    const byte parityflag = 0x04; //1=even
    const byte carryflag = 0x01;
    const byte alwayszeroflags = 0x28;
    const byte alwaysoneflags = 0x02;


    bool running;
    byte a, flags, b, c, d, e, h, l;
    ushort pc, sp;
    //ushort _bc;
    //de
    //hl
    //aflags
    byte[] memory = new byte[65536];
    //byte[] rom = new byte[4096];
    bool isROM(ushort addr)
    {
        return (addr & 0xf000) == 0xf000;
    }

    ushort makeWord(byte h, byte l)
    {
        return (ushort)((h << 8) | l);
    }

    byte highByte(ushort word)
    {
        return (byte)((word >> 8) & 0xff);
    }

    byte lowByte(ushort word)
    {
        return (byte)(word & 0xff);
    }

    ushort lowWord(int i)
    {
        return (ushort)(i & 0xff);
    }
    ushort highWord(int i)
    {
        return (ushort)((i >> 16) & 0xff);
    }

    (byte, byte) splitWord(ushort w)
    {
        return (highByte(w), lowByte(w));
    }
    public byte m
    {
        get => memory[hl];
        set {
            if (!isROM(hl)) {
                memory[hl] = value;
            }
        }
    }
    public ushort bc
    {
        get => makeWord(b, c); set => (b, c) = splitWord(value);
    }
    public ushort de
    {
        get => makeWord(d, e); set => (d, e) = splitWord(value);
    }
    public ushort hl
    {
        get => makeWord(h, l); set => (h, l) = splitWord(value);
    }
    public ushort aflags
    {
        get => makeWord(a, flags); set => (a, flags) = splitWord(value);
    }

    void Run(ushort? address = 0x100)
    {
        if (address.HasValue)
        {
            pc = address.Value;
        }
        running = true;
        while (running)
        {
            DoInstruction();
        }
    }

    int DoInstruction()
    {
        //byte opCode = ram[pc++];
        var opcode = fetchByte();
        int cycles = 4;
        int iTmp;
        byte bTmp;
        byte flagsTmp;
        //byte accTmp;
        ushort wTmp;
        bool newCarry;
        bool newAuxCarry;
        switch (opcode)
        {
            case 0x00:
                break; //nop (4 cycles)
            case 0x01:
                bc = fetchWord();
                cycles = 10;
                break; //lxi b
            case 0x02:
                memory[bc] = a;
                cycles = 7;
                break; //stax b
            case 0x03:
                unchecked { bc++; }
                cycles = 5;
                break; //inx b
            case 0x04:
                unchecked { b++; }
                //standard zero, sign, parity handling
                setFlagsZPS(b);
                //also set ac on right nibl overflow
                flags = (byte)((flags & ~auxcarryflag) | ((b & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr b
            case 0x05:
                unchecked { b--; }
                //standard zero, sign, parity handling
                setFlagsZPS(b);
                //also set ac on right nibl underflow CONCERN: borrows may not set ac as expected - maybe set to 1 then blear when borrow occurs? find a test suite!
                flags = (byte)((flags & ~auxcarryflag) | (((b & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr b
            case 0x06:
                b = fetchByte();
                cycles = 7;
                break; //mvi b,d8
            case 0x07:
                {
                    bTmp = (byte)(((a << 1) & 0xfe) | (a >> 7));
                    flags = (byte)((flags & ~carryflag) | (((a & 1) != 0) ? carryflag : 0));
                    a = bTmp;
                }
                break; //rlc (4 cycles)
            case 0x08: break;
            case 0x09:
                iTmp = hl + bc;
                hl = unchecked((ushort)iTmp);
                flags = (byte)((flags & ~carryflag) | ((iTmp > 0xffff) ? carryflag : 0));
                cycles = 10;
                break; //dad b
            case 0x0a:
                a = memory[bc];
                cycles = 7;
                break; //ldax b
            case 0x0b:
                unchecked { bc--; }
                cycles = 5;
                break; //dcx b
            case 0x0c:
                unchecked { c++; }
                setFlagsZPS(c);
                flags = (byte)((flags & ~auxcarryflag) | ((c & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr c
            case 0x0d:
                unchecked { c--; }
                setFlagsZPS(c);
                flags = (byte)((flags & ~auxcarryflag) | (((c & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr c
            case 0x0e:
                c = fetchByte();
                cycles = 7;
                break; //mvi c,d8
            case 0x0f:
                a = (byte)((a >> 1) | (((a & 0x01) != 0) ? 0x80 : 0));
                flags = (byte)((flags & ~carryflag) | (((a & 0x80) != 0) ? carryflag : 0));
                break; //rrc (4 cycles)
            case 0x10: break;
            case 0x11:
                de = fetchWord();
                cycles = 10;
                break; //lxi d
            case 0x12:
                memory[de] = a;
                cycles = 7;
                break; //stax d
            case 0x13:
                unchecked { de++; }
                cycles = 5;
                break; //inx d
            case 0x14:
                unchecked { d++; }
                setFlagsZPS(d);
                flags = (byte)((flags & ~auxcarryflag) | ((d & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr d
            case 0x15:
                unchecked { d--; }
                setFlagsZPS(d);
                flags = (byte)((flags & ~auxcarryflag) | (((d & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr d
            case 0x16:
                d = fetchByte();
                cycles = 7;
                break; //mvi d,d8
            case 0x17:
                bTmp = a;
                newCarry = (bTmp & 0x80) != 0;
                a = (byte)((bTmp << 1) | (((flags & carryflag) != 0) ? 1 : 0));
                flags = (byte)((flags & ~carryflag) | (newCarry ? carryflag : 0));
                break; //ral (4 cycles)
            case 0x18: break;
            case 0x19:
                iTmp = hl + de;
                hl = unchecked((ushort)iTmp);
                flags = (iTmp > 0xffff) ? (byte)(flags | carryflag) : (byte)(flags & ~carryflag);
                cycles = 10;
                break; //dad d
            case 0x1a:
                a = memory[de];
                cycles = 7;
                break; //ldax d
            case 0x1b:
                unchecked { de--; }
                cycles = 5;
                break; //dcx d
            case 0x1c:
                unchecked { e++; }
                setFlagsZPS(e);
                flags = (byte)((flags & ~auxcarryflag) | ((e & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr e
            case 0x1d:
                unchecked { e--; }
                setFlagsZPS(e);
                flags = (byte)((flags & ~auxcarryflag) | (((e & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr e
            case 0x1e:
                e = fetchByte();
                cycles = 7;
                break; //mvi e,d8
            case 0x1f:
                bTmp = a;
                newCarry = (bTmp & 0x01) != 0;
                a = (byte)((bTmp >> 1) | (((flags & carryflag) != 0) ? 0x80 : 0));
                flags = (byte)((flags & ~carryflag) | (newCarry ? carryflag : 0));
                break; //rar (4 cycles)
            case 0x20: break;
            case 0x21:
                hl = fetchWord();
                cycles = 10;
                break; //lxi h
            case 0x22:
                wTmp = fetchWord();
                memory[wTmp++] = l; //L register
                memory[wTmp] = h;
                cycles = 16;
                break; //shld a16
            case 0x23:
                unchecked { hl++; }
                cycles = 5;
                break; //inx h
            case 0x24:
                unchecked { h++; }
                setFlagsZPS(h);
                flags = (byte)((flags & ~auxcarryflag) | ((h & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr h
            case 0x25:
                unchecked { h--; }
                setFlagsZPS(h);
                flags = (byte)((flags & ~auxcarryflag) | (((h & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr h
            case 0x26:
                h = fetchByte();
                cycles = 7;
                break; //mvi h,d8
            case 0x27:
                flagsTmp = flags;
                bTmp = a;
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
                flags = flagsTmp;
                a = bTmp;
                setFlagsZPS(bTmp);
                cycles = 4;
                break; //daa
            case 0x28: break;
            case 0x29:
                iTmp = hl + hl;
                hl = unchecked((ushort)iTmp);
                flags = (iTmp > 0xffff) ? (byte)(flags | carryflag) : (byte)(flags & ~carryflag);
                cycles = 10;
                break; //dad h
            case 0x2a:
                wTmp = fetchWord();
                wTmp = (ushort)((memory[wTmp++]) | (memory[wTmp] << 8));
                hl = wTmp;
                cycles = 16;
                break; //lhld a16
            case 0x2b:
                unchecked { hl--; }
                cycles = 5;
                break; //dcx h
            case 0x2c:
                unchecked { l++; }
                setFlagsZPS(l);
                flags = (byte)((flags & ~auxcarryflag) | ((l & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr l
            case 0x2d:
                unchecked { l--; }
                setFlagsZPS(l);
                flags = (byte)((flags & ~auxcarryflag) | (((l & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr l
            case 0x2e:
                l = fetchByte();
                cycles = 7;
                break; //mvi l,d8
            case 0x2f:
                a = (byte)~a;
                //cycles = 4;
                break; //cma
            case 0x30: break;
            case 0x31:
                sp = fetchWord();
                cycles = 10;
                break; //lxi sp
            case 0x32:
                memory[fetchWord()] = a;
                cycles = 13;
                break; //sta a16
            case 0x33:
                unchecked { sp++; }
                cycles = 5;
                break; //inx sp
            case 0x34:
                unchecked { m++; }
                setFlagsZPS(m);
                flags = (byte)((flags & ~auxcarryflag) | ((m & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 10;
                break; //inr m
            case 0x35:
                unchecked { m--; }
                setFlagsZPS(m);
                flags = (byte)((flags & ~auxcarryflag) | (((m & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 10;
                break; //dcr m
            case 0x36:
                memory[hl] = fetchByte();
                cycles = 10;
                break; //mvi m,d8
            case 0x37:
                flags = (byte)(flags | carryflag);
                //cycles = 4;
                break; //STC
            case 0x38: break;
            case 0x39:
                iTmp = hl + sp;
                hl = unchecked((ushort)iTmp);
                flags = (iTmp > 0xffff) ? (byte)(flags | carryflag) : (byte)(flags & ~carryflag);
                cycles = 10;
                break; //dad sp
            case 0x3a:
                a = memory[fetchWord()];
                cycles = 13;
                break; //lda a16
            case 0x3b:
                unchecked { sp--; }
                cycles = 5;
                break; //dcx sp
            case 0x3c:
                unchecked { a++; }
                setFlagsZPS(a);
                flags = (byte)((flags & ~auxcarryflag) | ((a & 0x0f) == 0 ? auxcarryflag : 0));
                cycles = 5;
                break; //inr a
            case 0x3d:
                unchecked { a--; }
                setFlagsZPS(a);
                flags = (byte)((flags & ~auxcarryflag) | (((a & 0x0f) == 0x0f) ? auxcarryflag : 0));
                cycles = 5;
                break; //dcr a
            case 0x3e:
                a = fetchByte();
                cycles = 7;
                break; //mvi a,d8
            case 0x3f:
                flags = (byte)(flags ^ carryflag);
                break; //CMC
            case 0x40:
                cycles = 5;
                break; //mov b,b
            case 0x41:
                b = c;
                cycles = 5;
                break; //mov b,c
            case 0x42:
                b = d;
                cycles = 5;
                break; //mov b,d
            case 0x43:
                b = e;
                cycles = 5;
                break; //mov b,e
            case 0x44:
                b = h;
                cycles = 5;
                break; //mov b,h
            case 0x45:
                b = l;
                cycles = 5;
                break; //mov b,l
            case 0x46:
                b = m;
                cycles = 7;
                break; //mov b,m
            case 0x47:
                b = a;
                cycles = 5;
                break; //mov b,a
            case 0x48:
                c = b;
                cycles = 5;
                break; //mov c,b
            case 0x49:
                cycles = 5;
                break; //mov c,c
            case 0x4a:
                c = d;
                cycles = 5;
                break; //mov c,d
            case 0x4b:
                c = e;
                cycles = 5;
                break; //mov c,e
            case 0x4c:
                c = h;
                cycles = 5;
                break; //mov c,h
            case 0x4d:
                c = l;
                cycles = 5;
                break; //mov c,l
            case 0x4e:
                c = m;
                cycles = 7;
                break; //mov c,m
            case 0x4f:
                c = a;
                cycles = 5;
                break; //mov c,a
            case 0x50:
                d = b;
                cycles = 5;
                break; //mov d,b
            case 0x51:
                d = c;
                cycles = 5;
                break; //mov d,c
            case 0x52:
                cycles = 5;
                break; //mov d,d
            case 0x53:
                d = e;
                cycles = 5;
                break; //mov d,e
            case 0x54:
                d = h;
                cycles = 5;
                break; //mov d,h
            case 0x55:
                d = l;
                cycles = 5;
                break; //mov d,l
            case 0x56:
                d = m;
                cycles = 7;
                break; //mov d,m
            case 0x57:
                d = a;
                cycles = 5;
                break; //mov d,a
            case 0x58:
                e = b;
                cycles = 5;
                break; //mov e,b
            case 0x59:
                e = c;
                cycles = 5;
                break; //mov e,c
            case 0x5a:
                e = d;
                cycles = 5;
                break; //mov e,d
            case 0x5b:
                cycles = 5;
                break; //mov e,e
            case 0x5c:
                e = h;
                cycles = 5;
                break; //mov e,h
            case 0x5d:
                e = l;
                cycles = 5;
                break; //mov e,l
            case 0x5e:
                e = m;
                cycles = 7;
                break; //mov e,m
            case 0x5f:
                e = a;
                cycles = 5;
                break; //mov e,a
            case 0x60:
                h = b;
                cycles = 5;
                break; //mov h,b
            case 0x61:
                h = c;
                cycles = 5;
                break; //mov h,c
            case 0x62:
                h = d;
                cycles = 5;
                break; //mov h,d
            case 0x63:
                h = e;
                cycles = 5;
                break; //mov h,e
            case 0x64:
                cycles = 5;
                break; //mov h,h
            case 0x65:
                h = l;
                cycles = 5;
                break; //mov h,l
            case 0x66:
                h = m;
                cycles = 7;
                break; //mov h,m
            case 0x67:
                h = a;
                cycles = 5;
                break; //mov h,a
            case 0x68:
                l = b;
                cycles = 5;
                break; //mov l,b
            case 0x69:
                l = c;
                cycles = 5;
                break; //mov l,c
            case 0x6a:
                l = d;
                cycles = 5;
                break; //mov l,d
            case 0x6b:
                l = e;
                cycles = 5;
                break; //mov l,e
            case 0x6c:
                l = h;
                cycles = 5;
                break; //mov l,h
            case 0x6d:
                cycles = 5;
                break; //mov l,l
            case 0x6e:
                l = m;
                cycles = 7;
                break; //mov l,m
            case 0x6f:
                l = a;
                cycles = 5;
                break; //mov l,a
            case 0x70:
                m = b;
                cycles = 7;
                break; //mov m,b
            case 0x71:
                m = c;
                cycles = 7;
                break; //mov m,c
            case 0x72:
                m = d;
                cycles = 7;
                break; //mov m,d
            case 0x73:
                m = e;
                cycles = 7;
                break; //mov m,e
            case 0x74:
                m = h;
                cycles = 7;
                break; //mov m,h
            case 0x75:
                m = l;
                cycles = 7;
                break; //mov m,l
            case 0x76:
                cycles = 7;
                running = false;
                break; //hlt
            case 0x77:
                m = a;
                cycles = 7;
                break; //mov m,a
            case 0x78:
                a = b;
                cycles = 5;
                break; //mov a,b
            case 0x79:
                a = c;
                cycles = 5;
                break; //mov a,c
            case 0x7a:
                a = d;
                cycles = 5;
                break; //mov a,d
            case 0x7b:
                a = e;
                cycles = 5;
                break; //mov a,e
            case 0x7c:
                a = h;
                cycles = 5;
                break; //mov a,h
            case 0x7d:
                a = l;
                cycles = 5;
                break; //mov a,l
            case 0x7e:
                a = m;
                cycles = 7;
                break; //mov a,m
            case 0x7f:
                cycles = 5;
                break; //mov a,a
            case 0x80:
                bTmp = a;
                wTmp = (ushort)(bTmp + b);
                newAuxCarry = (a & 0x0f) + (b & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add b
            case 0x81:
                bTmp = a;
                wTmp = (ushort)(bTmp + c);
                newAuxCarry = (a & 0x0f) + (c & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add c
            case 0x82:
                bTmp = a;
                wTmp = (ushort)(bTmp + d);
                newAuxCarry = (a & 0x0f) + (d & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add d
            case 0x83:
                bTmp = a;
                wTmp = (ushort)(bTmp + e);
                newAuxCarry = (a & 0x0f) + (e & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add e
            case 0x84:
                bTmp = a;
                wTmp = (ushort)(bTmp + h);
                newAuxCarry = (a & 0x0f) + (h & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add h
            case 0x85:
                bTmp = a;
                wTmp = (ushort)(bTmp + l);
                newAuxCarry = (a & 0x0f) + (l & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add l
            case 0x86:
                bTmp = a;
                wTmp = (ushort)(bTmp + m);
                newAuxCarry = (a & 0x0f) + (m & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //add m
            case 0x87:
                bTmp = a;
                wTmp = (ushort)(bTmp + a);
                newAuxCarry = (a & 0x0f) + (a & 0x0f) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //add a
            case 0x88:
                bTmp = a;
                wTmp = (ushort)(bTmp + b + (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) + (b & 0x0f) + (((flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc b
            case 0x89:
                bTmp = a;
                wTmp = (ushort)(bTmp + c + (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) + (c & 0x0f) + (((flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc c
            case 0x8a:
                bTmp = a;
                wTmp = (ushort)(bTmp + d + (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) + (d & 0x0f) + (((flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc d
            case 0x8b:
                bTmp = a;
                wTmp = (ushort)(bTmp + e + (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) + (e & 0x0f) + (((flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc e
            case 0x8c:
                bTmp = a;
                wTmp = (ushort)(bTmp + h + (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) + (h & 0x0f) + (((flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc h
            case 0x8d:
                bTmp = a;
                wTmp = (ushort)(bTmp + l + (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) + (l & 0x0f) + (((flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc l
            case 0x8e:
                bTmp = a;
                wTmp = (ushort)(bTmp + m + (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) + (m & 0x0f) + (((flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //adc m
            case 0x8f:
                bTmp = a;
                wTmp = (ushort)(bTmp + a + (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) + (a & 0x0f) + (((flags & carryflag) != 0) ? 1 : 0) > 0x0f;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //adc a
            case 0x90:
                bTmp = a;
                wTmp = (ushort)(bTmp - b);
                newAuxCarry = (a & 0x0f) - (b & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub b
            case 0x91:
                bTmp = a;
                wTmp = (ushort)(bTmp - c);
                newAuxCarry = (a & 0x0f) - (c & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub c
            case 0x92:
                bTmp = a;
                wTmp = (ushort)(bTmp - d);
                newAuxCarry = (a & 0x0f) - (d & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub d
            case 0x93:
                bTmp = a;
                wTmp = (ushort)(bTmp - e);
                newAuxCarry = (a & 0x0f) - (e & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub e
            case 0x94:
                bTmp = a;
                wTmp = (ushort)(bTmp - h);
                newAuxCarry = (a & 0x0f) - (h & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub h
            case 0x95:
                bTmp = a;
                wTmp = (ushort)(bTmp - l);
                newAuxCarry = (a & 0x0f) - (l & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub l
            case 0x96:
                bTmp = a;
                wTmp = (ushort)(bTmp - m);
                newAuxCarry = (a & 0x0f) - (m & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //sub m
            case 0x97:
                bTmp = a;
                wTmp = (ushort)(bTmp - a);
                newAuxCarry = (a & 0x0f) - (a & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sub a
            case 0x98:
                bTmp = a;
                wTmp = (ushort)(bTmp - b - (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) - (b & 0x0f) - (((flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb b
            case 0x99:
                bTmp = a;
                wTmp = (ushort)(bTmp - c - (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) - (c & 0x0f) - (((flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb c
            case 0x9a:
                bTmp = a;
                wTmp = (ushort)(bTmp - d - (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) - (d & 0x0f) - (((flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb d
            case 0x9b:
                bTmp = a;
                wTmp = (ushort)(bTmp - e - (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) - (e & 0x0f) - (((flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb e
            case 0x9c:
                bTmp = a;
                wTmp = (ushort)(bTmp - h - (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) - (h & 0x0f) - (((flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb h
            case 0x9d:
                bTmp = a;
                wTmp = (ushort)(bTmp - l - (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) - (l & 0x0f) - (((flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb l
            case 0x9e:
                bTmp = a;
                wTmp = (ushort)(bTmp - m - (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) - (m & 0x0f) - (((flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //sbb m
            case 0x9f:
                bTmp = a;
                wTmp = (ushort)(bTmp - a - (((flags & carryflag) != 0) ? 1 : 0));
                newAuxCarry = (a & 0x0f) - (a & 0x0f) - (((flags & carryflag) != 0) ? 1 : 0) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                a = unchecked((byte)wTmp);
                setFlagsZPS(a);
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //sbb a
            case 0xa0:
                a = (byte)(a & b);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana b
            case 0xa1:
                a = (byte)(a & c);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana c
            case 0xa2:
                a = (byte)(a & d);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana d
            case 0xa3:
                a = (byte)(a & e);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana e
            case 0xa4:
                a = (byte)(a & h);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana h
            case 0xa5:
                a = (byte)(a & l);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana l
            case 0xa6:
                a = (byte)(a & m);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                cycles = 7;
                break; //ana m
            case 0xa7:
                a = (byte)(a & a);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ana a
            case 0xa8:
                a = (byte)(a ^ b);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra b
            case 0xa9:
                a = (byte)(a ^ c);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra c
            case 0xaa:
                a = (byte)(a ^ d);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra d
            case 0xab:
                a = (byte)(a ^ e);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra e
            case 0xac:
                a = (byte)(a ^ h);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra h
            case 0xad:
                a = (byte)(a ^ l);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra l
            case 0xae:
                a = (byte)(a ^ m);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                cycles = 7;
                break; //xra m
            case 0xaf:
                a = (byte)(a ^ a);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //xra a
            case 0xb0:
                a = (byte)(a | b);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora b
            case 0xb1:
                a = (byte)(a | c);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora c
            case 0xb2:
                a = (byte)(a | d);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora d
            case 0xb3:
                a = (byte)(a | e);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora e
            case 0xb4:
                a = (byte)(a | h);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora h
            case 0xb5:
                a = (byte)(a | l);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora l
            case 0xb6:
                a = (byte)(a | m);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                cycles = 7;
                break; //ora m
            case 0xb7:
                a = (byte)(a | a);
                setFlagsZPS(a);
                flags = (byte)(flags & (~carryflag)); //CY,AC
                //cycles = 4;
                break; //ora a
            case 0xb8:
                bTmp = a;
                wTmp = (ushort)(bTmp - b);
                newAuxCarry = (a & 0x0f) - (b & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                setFlagsZPS(unchecked((byte)wTmp));
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp b
            case 0xb9:
                bTmp = a;
                wTmp = (ushort)(bTmp - c);
                newAuxCarry = (a & 0x0f) - (c & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                setFlagsZPS(unchecked((byte)wTmp));
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp c
            case 0xba:
                bTmp = a;
                wTmp = (ushort)(bTmp - d);
                newAuxCarry = (a & 0x0f) - (d & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                setFlagsZPS(unchecked((byte)wTmp));
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp d
            case 0xbb:
                bTmp = a;
                wTmp = (ushort)(bTmp - e);
                newAuxCarry = (a & 0x0f) - (e & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                setFlagsZPS(unchecked((byte)wTmp));
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp e
            case 0xbc:
                bTmp = a;
                wTmp = (ushort)(bTmp - h);
                newAuxCarry = (a & 0x0f) - (h & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                setFlagsZPS(unchecked((byte)wTmp));
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp h
            case 0xbd:
                bTmp = a;
                wTmp = (ushort)(bTmp - l);
                newAuxCarry = (a & 0x0f) - (l & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                setFlagsZPS(unchecked((byte)wTmp));
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp l
            case 0xbe:
                bTmp = a;
                wTmp = (ushort)(bTmp - m);
                newAuxCarry = (a & 0x0f) - (m & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                setFlagsZPS(unchecked((byte)wTmp));
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                cycles = 7;
                break; //cmp m
            case 0xbf:
                bTmp = a;
                wTmp = (ushort)(bTmp - a);
                newAuxCarry = (a & 0x0f) - (a & 0x0f) < 0;
                newCarry = (wTmp & 0xff00) != 0;
                //a = unchecked((byte)wTmp);
                setFlagsZPS(unchecked((byte)wTmp));
                flags = (byte)((flags & (~(carryflag | auxcarryflag)))
                               | (newCarry ? carryflag : 0)
                               | (newAuxCarry ? auxcarryflag : 0)); //CY,AC
                //cycles = 4;
                break; //cmp a
            case 0xc0:
                cycles = 5;
                if ((flags & zeroflag) == 0)
                {
                    pc = popWord();
                    cycles = 11;
                }
                break; //rnz
            case 0xc1:
                bc = popWord();
                cycles = 10;
                break; //pop b
            case 0xc2:
                wTmp = fetchWord();
                if (!(getFlags() & zeroflag)) putIP(wTmp);
                cycles = 10;
                break; //jnz a16
            case 0xc3:
                putIP(fetchWord());
                cycles = 10;
                break; //jmp a16
            case 0xc4:
                wTmp = fetchWord();
                if (!(getFlags() & zeroflag))
                {
                    pushWord(getIP());
                    putIP(wTmp);
                    cycles = 17;
                }
                else
                {
                    cycles = 11;
                }
                break; //cnz a16
            case 0xc5:
                pushWord(getBC());
                cycles = 11;
                break; //push b
            case 0xc6:
                add_mac(IMM8);
                cycles = 7;
                break; //adi d8
            case 0xc7:
                pushWord(getIP());
                putIP(0x0000);
                cycles = 11;
                break; //rst 0
            case 0xc8:
                cycles = 5;
                if (getFlags() & zeroflag)
                {
                    putIP(popWord());
                    cycles = 11;
                }
                break; //rz
            case 0xc9:
                putIP(popWord());
                cycles = 10;
                break; //ret
            case 0xca:
                wTmp = fetchWord();
                if (getFlags() & zeroflag) putIP(wTmp);
                cycles = 10;
                break; //jz a16
            case 0xcb: break;
            case 0xcc:
                wTmp = fetchWord();
                if (getFlags() & zeroflag)
                {
                    pushWord(getIP());
                    putIP(wTmp);
                    cycles = 17;
                }
                else
                {
                    cycles = 11;
                }
                break; //cz a16
            case 0xcd:
                wTmp = fetchWord();
                pushWord(getIP());
                putIP(wTmp);
                cycles = 17;
                break; //call
            case 0xce:
                adc_mac(IMM8);
                cycles = 7;
                break; //aci d8
            case 0xcf:
                pushWord(getIP());
                putIP(0x0008);
                cycles = 11;
                break; //rst 1
            case 0xd0:
                cycles = 5;
                if (!(getFlags() & carryflag))
                {
                    putIP(popWord());
                    cycles = 11;
                }
                break; //rnc
            case 0xd1:
                putDE(popWord());
                cycles = 10;
                break; //pop d
            case 0xd2:
                wTmp = fetchWord();
                if (!(getFlags() & carryflag)) putIP(wTmp);
                cycles = 10;
                break; //jnc a16
            case 0xd3:
                cycles = 10;
                break; //out port8
            case 0xd4:
                wTmp = fetchWord();
                if (!(getFlags() & carryflag))
                {
                    pushWord(getIP());
                    putIP(wTmp);
                    cycles = 17;
                }
                else
                {
                    cycles = 11;
                }
                break; //cnc a16
            case 0xd5:
                pushWord(getDE());
                cycles = 11;
                break; //push d
            case 0xd6:
                sub_mac(IMM8);
                cycles = 7;
                break; //sui d8
            case 0xd7:
                pushWord(getIP());
                putIP(0x0010);
                cycles = 11;
                break; //rst 2
            case 0xd8:
                cycles = 5;
                if ((getFlags() & carryflag))
                {
                    putIP(popWord());
                    cycles = 11;
                }
                break; //rc
            case 0xd9: break;
            case 0xda:
                wTmp = fetchWord();
                if (getFlags() & carryflag) putIP(wTmp);
                cycles = 10;
                break; //jc a16
            case 0xdb:
                cycles = 10;
                break; //in port8
            case 0xdc:
                wTmp = fetchWord();
                if (getFlags() & carryflag)
                {
                    pushWord(getIP());
                    putIP(wTmp);
                    cycles = 17;
                }
                else
                {
                    cycles = 11;
                }
                break; //cc a16
            case 0xdd: break;
            case 0xde:
                sbb_mac(IMM8);
                cycles = 7;
                break; //sbi d8
            case 0xdf:
                pushWord(getIP());
                putIP(0x0018);
                cycles = 11;
                break; //rst 3
            case 0xe0:
                cycles = 5;
                if (!(getFlags() & parityflag))
                {
                    putIP(popWord());
                    cycles = 11;
                }
                break; //rpo
            case 0xe1:
                putHL(popWord());
                cycles = 10;
                break; //pop h
            case 0xe2:
                wTmp = fetchWord();
                if (!(getFlags() & parityflag)) putIP(wTmp);
                cycles = 10;
                break; //jpo a16
            case 0xe3:
                wTmp = popWord();
                pushWord(getHL());
                putHL(wTmp);
                cycles = 18;
                break; //xtlh
            case 0xe4:
                wTmp = fetchWord();
                if (!(getFlags() & parityflag))
                {
                    pushWord(getIP());
                    putIP(wTmp);
                    cycles = 17;
                }
                else
                {
                    cycles = 11;
                }
                break; //cpo a16
            case 0xe5:
                pushWord(getHL());
                cycles = 11;
                break; //push h
            case 0xe6:
                ana_mac(IMM8);
                cycles = 7;
                break; //ani d8
            case 0xe7:
                pushWord(getIP());
                putIP(0x0020);
                cycles = 11;
                break; //rst 4
            case 0xe8:
                cycles = 5;
                if (getFlags() & parityflag)
                {
                    putIP(popWord());
                    cycles = 11;
                }
                break; //rpe
            case 0xe9:
                putIP(getHL());
                cycles = 5;
                break; //pchl
            case 0xea:
                wTmp = fetchWord();
                if (getFlags() & parityflag) putIP(wTmp);
                cycles = 10;
                break; //jpe a16
            case 0xeb:
                wTmp = getHL();
                putHL(getDE());
                putDE(wTmp);
                //cycles = 4;
                break; //xchg (DE, HL)
            case 0xec:
                wTmp = fetchWord();
                if (getFlags() & parityflag)
                {
                    pushWord(getIP());
                    putIP(wTmp);
                    cycles = 17;
                }
                else
                {
                    cycles = 11;
                }
                break; //cpe a16
            case 0xed: break;
            case 0xee:
                xra_mac(IMM8);
                cycles = 7;
                break; //xri d8
            case 0xef:
                pushWord(getIP());
                putIP(0x0028);
                cycles = 11;
                break; //rst 5
            case 0xf0:
                cycles = 5;
                if (!(getFlags() & signflag))
                {
                    putIP(popWord());
                    cycles = 11;
                }
                break; //rp
            case 0xf1:
                wTmp = popWord();
                //set fixed flag bits appropriately
                wTmp = (wTmp | alwaysoneflags) & (~(WORD)alwayszeroflags);
                putAFlags(wTmp);
                cycles = 10;
                break; //pop psw
            case 0xf2:
                wTmp = fetchWord();
                if (!(getFlags() & signflag)) putIP(wTmp);
                cycles = 10;
                break; //jp a16
            case 0xf3:
                interruptsEnabled = false;
                //cycles = 4;
                break; //di
            case 0xf4:
                wTmp = fetchWord();
                if (!(getFlags() & signflag))
                {
                    pushWord(getIP());
                    putIP(wTmp);
                    cycles = 17;
                }
                else
                {
                    cycles = 11;
                }
                break; //cp a16
            case 0xf5:
                pushWord(getAFlags());
                cycles = 11;
                break; //push psw
            case 0xf6:
                ora_mac(IMM8);
                cycles = 7;
                break; //ori d8
            case 0xf7:
                pushWord(getIP());
                putIP(0x0030);
                cycles = 11;
                break; //rst 6
            case 0xf8:
                cycles = 5;
                if (getFlags() & signflag)
                {
                    putIP(popWord());
                    cycles = 11;
                }
                break; //rm
            case 0xf9:
                putSP(getHL());
                cycles = 5;
                break; //sphl
            case 0xfa:
                wTmp = fetchWord();
                if (getFlags() & signflag) putIP(wTmp);
                cycles = 10;
                break; //jm a16
            case 0xfb:
                interruptsEnabled = true;
                //cycles = 4;
                break; //ei
            case 0xfc:
                wTmp = fetchWord();
                if (getFlags() & signflag)
                {
                    pushWord(getIP());
                    putIP(wTmp);
                    cycles = 17;
                }
                else
                {
                    cycles = 11;
                }
                break; //cm a16
            case 0xfd: break;
            case 0xfe:
                cmp_mac(IMM8);
                cycles = 7;
                break; //cpi d8
            case 0xff:
                pushWord(getIP());
                putIP(0x0038);
                cycles = 11;
                break; //rst 7
            default: break;
        }
        return cycles;
    }

    /*

    void CPU::jumpTo(const WORD address) {
    this->putIP(address);
    }

    void CPU::run() {

    }

    void CPU::stop() {

    }

    void CPU::singleStep() {

    }

    void CPU::enableBreakpoints() {

    }

    void CPU::disableBreakpoints() {

    }

    bool CPU::isRunning() {
    return this->running;
    }

    bool CPU::isBreakpointsEnabled() {
    return this->breakpointsEnabled;
    }

    */
    byte fetchByte()
    {
        var addr = getIP();
        var rslt = memory[addr++];
        putIP(addr);
        return rslt;
    }

    ushort fetchWord()
    {
        return (ushort)(fetchByte() | (ushort)(fetchByte() << 8));
    }

    /*
    void inx(ref ushort w)
    {
        unchecked {
            w++;    
        }
        //flags: none
    }
    void dcx(ref ushort w)
    {
        unchecked {
            w--;    
        }
        //flags: none
    }
    void CPU::pushByte(const BYTE b) {
        WORD addr = getSP();
        memory[--addr] = b;
        putSP(addr);
    }

    void CPU::pushWord(const WORD w) {
        //pushByte(w >> 8);
        //pushByte(w & 0xff);
        WORD addr = getSP();
        memory[--addr] = w >> 8;
        memory[--addr] = w & 0xff;
        putSP(addr);
    };

    BYTE CPU::popByte() {
        WORD addr = getSP();
        BYTE rslt = memory[addr++];
        putSP(addr);
        return rslt;
    }

    WORD CPU::popWord() {
        //return this->popByte() | (this->popByte() << 8);
        WORD addr = getSP();
        WORD rslt = memory[addr++];
        rslt = rslt | (memory[addr++] << 8);
        putSP(addr);
        return rslt;
    }

    #define inr_mac(r) { \
        BYTE v = get##r() + 1; \
        BYTE f = getFlags() & (~(zeroflag | parityflag | signflag | auxcarryflag)); \
        if (v == 0) f |= zeroflag; \
        if (parityEven(v)) f |= parityflag; \
        if (v & 0x80) f |= signflag; \
        if ((v & 0x0f) == 0x00) f |= auxcarryflag; \
        put##r(v); \
        putFlags(f); \
    }

    //#define inr_macm() { \
    //    BYTE v = memory[getHL()] + 1; \
    //    BYTE f = getFlags() & (~(zeroflag | parityflag | signflag | auxcarryflag)); \
    //    if (v == 0) f |= zeroflag; \
    //    if (parityEven(v)) f |= parityflag; \
    //    if (v & 0x80) f |= signflag; \
    //    if ((v & 0x0f) == 0x00) f |= auxcarryflag; \
    //    memory[getHL()] = v; \
    //	putFlags(f); \
    //}

    #define dcr_mac(r) { \
        BYTE v = get##r()-1; \
        BYTE f = getFlags() & (~(zeroflag | parityflag | signflag | auxcarryflag)); \
        if (v == 0) f |= zeroflag; \
        if (parityEven(v)) f |= parityflag; \
        if (v & 0x80) f |= signflag; \
        if ((v & 0x0f) == 0x0f) f |= auxcarryflag; \
        put##r(v); \
        putFlags(f); \
    }

    #define add_mac(r) bTmp = getA(); \
        wTmp = (int)bTmp + get##r(); \
        newAuxCarry = (getA() & 0x0f) + (get##r() & 0x0f) > 0x0f; \
        newCarry = wTmp & 0xff00; \
        putA((BYTE)wTmp); \
        setFlagsZPS(getA()); \
        putFlags((getFlags() & (~(carryflag | auxcarryflag))) \
            | (newCarry ? carryflag : 0) \
            | (newAuxCarry ? auxcarryflag : 0)); //CY,AC

    #define adc_mac(r) bTmp = getA(); \
        wTmp = (int)getA() + get##r() + ((getFlags() & carryflag) ? 1 : 0); \
        newAuxCarry = (getA() & 0x0f) + (get##r() & 0x0f) + ((getFlags() & carryflag) ? 1 : 0) > 0x0f; \
        newCarry = wTmp & 0xff00; \
        putA((BYTE)wTmp); \
        setFlagsZPS(getA()); \
        putFlags((getFlags() & (~(carryflag | auxcarryflag))) \
            | (newCarry ? carryflag : 0) \
            | (newAuxCarry ? auxcarryflag : 0)); //CY,AC

    #define sub_mac(r) bTmp = getA(); \
        wTmp = (int)bTmp - get##r(); \
        newAuxCarry = (getA() & 0x0f) < (get##r() & 0x0f); \
        newCarry = wTmp & 0xff00; \
        putA((BYTE)wTmp); \
        setFlagsZPS(getA()); \
        putFlags((getFlags() & (~(carryflag | auxcarryflag))) \
            | (newCarry ? carryflag : 0) \
            | (newAuxCarry ? auxcarryflag : 0)); //CY,AC

    #define sbb_mac(r) bTmp = getA(); \
        wTmp = (int)bTmp - get##r() - ((getFlags() & carryflag) ? 1 : 0); \
        newAuxCarry = (getA() & 0x0f) < (get##r() & 0x0f) + ((getFlags() & carryflag) ? 1 : 0); \
        newCarry = wTmp & 0xff00; \
        putA((BYTE)wTmp); \
        setFlagsZPS(getA()); \
        putFlags((getFlags() & (~(carryflag | auxcarryflag))) \
            | (newCarry ? carryflag : 0) \
            | (newAuxCarry ? auxcarryflag : 0)); //CY,AC

    #define ana_mac(r) putA(getA() & get##r()); \
        setFlagsZPS(getA()); \
        putFlags((getFlags() & ~(carryflag | auxcarryflag))); //CY,AC

    #define ora_mac(r) putA(getA() | get##r()); \
        setFlagsZPS(getA()); \
        putFlags(getFlags() & ~(carryflag | auxcarryflag)); //CY,AC

    #define xra_mac(r) putA(getA() ^ get##r()); \
        setFlagsZPS(getA()); \
        putFlags(getFlags() & ~(carryflag | auxcarryflag)); //CY,AC

    #define cmp_mac(r) bTmp = getA(); \
        wTmp = (int)bTmp - get##r(); \
        newAuxCarry = (getA() & 0x0f) < (get##r() & 0x0f); \
        newCarry = wTmp & 0xff00; \
        setFlagsZPS(getA()); \
        putFlags((getFlags() & (~(carryflag | auxcarryflag))) \
            | (newCarry ? carryflag : 0) \
            | (newAuxCarry ? auxcarryflag : 0)); //CY,AC

    #define mov_mac(D, S) put##D(get##S());

    #define mov_mac_from_m(D) put##D(memory[getHL()]);
*/
    bool parityEven(ushort d)
    {
        var rslt = true;
        while (d != 0)
        {
            if ((d & 1) != 0) rslt = !rslt;
            d = (ushort)(d >> 1);
        }
        return rslt;
    }

    void setFlagsZPS(byte r)
    {
        var tmpFlags = (byte)(flags & (~(zeroflag | parityflag | signflag)));
        if (r == 0) tmpFlags |= zeroflag;
        if (parityEven(r)) tmpFlags |= parityflag;
        if ((r & 0x80) != 0) tmpFlags |= signflag;
        flags = tmpFlags;
    }


}
