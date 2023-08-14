using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using Windows.Data.Text;

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
    public ushort bc { get => makeWord(b, c); set => (b, c) = splitWord(value); }
    public ushort de { get => makeWord(d, e); set => (d, e) = splitWord(value); }
    public ushort hl { get => makeWord(h, l); set => (h, l) = splitWord(value); }
    public ushort aflags { get => makeWord(a, flags); set => (a, flags) = splitWord(value); }

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

    void DoInstruction()
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
                unchecked
                {
                    bc++;
                }
                cycles = 5;
                break; //inx b
            case 0x04:
                unchecked
                {
                    b++;
                }
                cycles = 5;
                break; //inr b
            case 0x05:
                unchecked
                {
                    b--;
                }
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
                hl = (ushort)(iTmp & 0xffff); //use unchecked?
                if (iTmp > 0xffff)
                {
                    flags = (byte)(flags | carryflag);
                }
                else
                {
                    flags = (byte)(flags & ~carryflag);
                }
                cycles = 10;
                break; //dad b
            case 0x0a:
                a = memory[bc];
                cycles = 7;
                break; //ldax b
            case 0x0b:
                unchecked
                {
                    bc--;
                }
                cycles = 5;
                break; //dcx b
            case 0x0c:
                unchecked
                {
                    c++;
                }
                cycles = 5;
                break; //inr c
            case 0x0d:
                unchecked
                {
                    c--;
                }
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
                cycles = 5;
                break; //inr d
            case 0x15:
                unchecked { d--; }
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
                hl = lowWord(iTmp);
                if (iTmp > 0xffff) {
                    flags = (byte)(flags | carryflag);
                } else {
                    flags = (byte)(flags & ~carryflag);
                }
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
                cycles = 5;
                break; //inr e
            case 0x1d:
                unchecked { e--; }
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
                cycles = 5;
                break; //inr h
            case 0x25:
                unchecked { h--; }
                cycles = 5;
                break; //dcr h
            case 0x26:
                putH(fetchByte());
                cycles = 7;
                break; //mvi h,d8
            case 0x27:
                flagsTmp = flags;
                bTmp = a;
                if (((bTmp & 0x0f) > 9) || ((flagsTmp & auxcarryflag) != 0))
                {
                    flagsTmp = (flagsTmp & (~auxcarryflag)) | (((bTmp & 0x0f) > 9) ? auxcarryflag : 0);
                    bTmp = (byte)(bTmp + 6);
                }
                else
                {
                    flagsTmp = flagsTmp & (~auxcarryflag);
                }
                if (((bTmp & 0xf0) > 0x90) || (flagsTmp & carryflag))
                {
                    flagsTmp = (flagsTmp & (~carryflag)) | (((bTmp & 0xf0) > 0x90) ? carryflag : 0);
                    bTmp = (byte)(bTmp + 0x60);
                }
                else
                {
                    flagsTmp = flagsTmp & (~carryflag);
                }
                putFlags(flagsTmp);
                putA(bTmp);
                setFlagsZPS(bTmp);
                cycles = 4;
                break; //daa
            case 0x28: break;
            case 0x29:
                iTmp = getHL() + getHL();
                putHL((WORD)iTmp);
                if (iTmp & 0xffff0000)
                {
                    putFlags(getFlags() | (carryflag));
                }
                else
                {
                    putFlags(getFlags() & (~carryflag));
                }
                cycles = 10;
                break; //dad h
            case 0x2a:
                wTmp = fetchWord();
                wTmp = (memory[wTmp++]) | (memory[wTmp] << 8);
                putHL(wTmp);
                cycles = 16;
                break; //lhld a16
            case 0x2b:
                putHL(getHL() - 1);
                cycles = 5;
                break; //dcx h
            case 0x2c:
                inr_mac(L);
                cycles = 5;
                break; //inr l
            case 0x2d:
                dcr_mac(L);
                cycles = 5;
                break; //dcr l
            case 0x2e:
                putL(fetchByte());
                cycles = 7;
                break; //mvi l,d8
            case 0x2f:
                putA(~getA());
                //cycles = 4;
                break; //cma
            case 0x30: break;
            case 0x31:
                putSP(fetchWord());
                cycles = 10;
                break; //lxi sp
            case 0x32:
                memory[fetchWord()] = getA();
                cycles = 13;
                break; //sta a16
            case 0x33:
                putSP(getSP() + 1);
                cycles = 5;
                break; //inx sp
            case 0x34:
                inr_mac(M);
                cycles = 10;
                break; //inr m
            case 0x35:
                dcr_mac(M);
                cycles = 10;
                break; //dcr m
            case 0x36:
                memory[getHL()] = fetchByte();
                cycles = 10;
                break; //mvi m,d8
            case 0x37:
                putFlags(getFlags() | carryflag);
                //cycles = 4;
                break; //STC
            case 0x38: break;
            case 0x39:
                iTmp = (int)getHL() + (int)getSP();
                putHL((WORD)iTmp);
                if (iTmp & 0xffff0000)
                {
                    putFlags(getFlags() | (carryflag));
                }
                else
                {
                    putFlags(getFlags() & (~carryflag));
                }
                cycles = 10;
                break; //dad sp
            case 0x3a:
                putA(memory[fetchWord()]);
                cycles = 13;
                break; //lda a16
            case 0x3b:
                putSP(getSP() - 1);
                cycles = 5;
                break; //dcx sp
            case 0x3c:
                inr_mac(A);
                cycles = 5;
                break; //inr a
            case 0x3d:
                dcr_mac(A);
                cycles = 5;
                break; //dcr a
            case 0x3e:
                putA(fetchByte());
                cycles = 7;
                break; //mvi a,d8
            case 0x3f:
                putFlags(getFlags() ^ carryflag);
                break; //CMC
            case 0x40:
                cycles = 5;
                break; //mov b,b
            case 0x41:
                mov_mac(B, C);
                cycles = 5;
                break; //mov b,c
            case 0x42:
                mov_mac(B, D);
                cycles = 5;
                break; //mov b,d
            case 0x43:
                mov_mac(B, E);
                cycles = 5;
                break; //mov b,e
            case 0x44:
                mov_mac(B, H);
                cycles = 5;
                break; //mov b,h
            case 0x45:
                mov_mac(B, L);
                cycles = 5;
                break; //mov b,l
            case 0x46:
                mov_mac_from_m(B);
                cycles = 7;
                break; //mov b,m
            case 0x47:
                mov_mac(B, A);
                cycles = 5;
                break; //mov b,a
            case 0x48:
                mov_mac(C, B);
                cycles = 5;
                break; //mov c,b
            case 0x49:
                cycles = 5;
                break; //mov c,c
            case 0x4a:
                mov_mac(C, D);
                cycles = 5;
                break; //mov c,d
            case 0x4b:
                mov_mac(C, E);
                cycles = 5;
                break; //mov c,e
            case 0x4c:
                mov_mac(C, H);
                cycles = 5;
                break; //mov c,h
            case 0x4d:
                mov_mac(C, L);
                cycles = 5;
                break; //mov c,l
            case 0x4e:
                mov_mac_from_m(C);
                cycles = 7;
                break; //mov c,m
            case 0x4f:
                mov_mac(C, A);
                cycles = 5;
                break; //mov c,a
            case 0x50:
                mov_mac(D, B);
                cycles = 5;
                break; //mov d,b
            case 0x51:
                mov_mac(D, C);
                cycles = 5;
                break; //mov d,c
            case 0x52:
                cycles = 5;
                break; //mov d,d
            case 0x53:
                mov_mac(D, E);
                cycles = 5;
                break; //mov d,e
            case 0x54:
                mov_mac(D, H);
                cycles = 5;
                break; //mov d,h
            case 0x55:
                mov_mac(D, L);
                cycles = 5;
                break; //mov d,l
            case 0x56:
                mov_mac(D, M);
                cycles = 7;
                break; //mov d,m
            case 0x57:
                mov_mac(D, A);
                cycles = 5;
                break; //mov d,a
            case 0x58:
                mov_mac(E, B);
                cycles = 5;
                break; //mov e,b
            case 0x59:
                mov_mac(E, C);
                cycles = 5;
                break; //mov e,c
            case 0x5a:
                mov_mac(E, D);
                cycles = 5;
                break; //mov e,d
            case 0x5b:
                cycles = 5;
                break; //mov e,e
            case 0x5c:
                mov_mac(E, H);
                cycles = 5;
                break; //mov e,h
            case 0x5d:
                mov_mac(E, L);
                cycles = 5;
                break; //mov e,l
            case 0x5e:
                mov_mac(E, M);
                cycles = 7;
                break; //mov e,m
            case 0x5f:
                mov_mac(E, A);
                cycles = 5;
                break; //mov e,a
            case 0x60:
                mov_mac(H, B);
                cycles = 5;
                break; //mov h,b
            case 0x61:
                mov_mac(H, C);
                cycles = 5;
                break; //mov h,c
            case 0x62:
                mov_mac(H, D);
                cycles = 5;
                break; //mov h,d
            case 0x63:
                mov_mac(H, E);
                cycles = 5;
                break; //mov h,e
            case 0x64:
                cycles = 5;
                break; //mov h,h
            case 0x65:
                mov_mac(H, L);
                cycles = 5;
                break; //mov h,l
            case 0x66:
                mov_mac(H, M);
                cycles = 7;
                break; //mov h,m
            case 0x67:
                mov_mac(H, A);
                cycles = 5;
                break; //mov h,a
            case 0x68:
                mov_mac(L, B);
                cycles = 5;
                break; //mov l,b
            case 0x69:
                mov_mac(L, C);
                cycles = 5;
                break; //mov l,c
            case 0x6a:
                mov_mac(L, D);
                cycles = 5;
                break; //mov l,d
            case 0x6b:
                mov_mac(L, E);
                cycles = 5;
                break; //mov l,e
            case 0x6c:
                mov_mac(L, H);
                cycles = 5;
                break; //mov l,h
            case 0x6d:
                cycles = 5;
                break; //mov l,l
            case 0x6e:
                mov_mac(L, M);
                cycles = 7;
                break; //mov l,m
            case 0x6f:
                mov_mac(L, A);
                cycles = 5;
                break; //mov l,a
            case 0x70:
                mov_mac(M, B);
                cycles = 7;
                break; //mov m,b
            case 0x71:
                mov_mac(M, C);
                cycles = 7;
                break; //mov m,c
            case 0x72:
                mov_mac(M, D);
                cycles = 7;
                break; //mov m,d
            case 0x73:
                mov_mac(M, E);
                cycles = 7;
                break; //mov m,e
            case 0x74:
                mov_mac(M, H);
                cycles = 7;
                break; //mov m,h
            case 0x75:
                mov_mac(M, L);
                cycles = 7;
                break; //mov m,l
            case 0x76:
                cycles = 7;
                break; //hlt
            case 0x77:
                mov_mac(M, A);
                cycles = 7;
                break; //mov m,a
            case 0x78:
                mov_mac(A, B);
                cycles = 5;
                break; //mov a,b
            case 0x79:
                mov_mac(A, C);
                cycles = 5;
                break; //mov a,c
            case 0x7a:
                mov_mac(A, D);
                cycles = 5;
                break; //mov a,d
            case 0x7b:
                mov_mac(A, E);
                cycles = 5;
                break; //mov a,e
            case 0x7c:
                mov_mac(A, H);
                cycles = 5;
                break; //mov a,h
            case 0x7d:
                mov_mac(A, L);
                cycles = 5;
                break; //mov a,l
            case 0x7e:
                mov_mac(A, M);
                cycles = 7;
                break; //mov a,m
            case 0x7f:
                cycles = 5;
                break; //mov a,a
            case 0x80:
                add_mac(B);
                //cycles = 4;
                break; //add b
            case 0x81:
                add_mac(C);
                //cycles = 4;
                break; //add c
            case 0x82:
                add_mac(D);
                //cycles = 4;
                break; //add d
            case 0x83:
                add_mac(E);
                //cycles = 4;
                break; //add e
            case 0x84:
                add_mac(H);
                //cycles = 4;
                break; //add h
            case 0x85:
                add_mac(L);
                //cycles = 4;
                break; //add l
            case 0x86:
                add_mac(M);
                cycles = 7;
                break; //add m
            case 0x87:
                add_mac(A);
                //cycles = 4;
                break; //add a
            case 0x88:
                adc_mac(B);
                //cycles = 4;
                break; //adc b
            case 0x89:
                adc_mac(C);
                //cycles = 4;
                break; //adc c
            case 0x8a:
                adc_mac(D);
                //cycles = 4;
                break; //adc d
            case 0x8b:
                adc_mac(E);
                //cycles = 4;
                break; //adc e
            case 0x8c:
                adc_mac(H);
                //cycles = 4;
                break; //adc h
            case 0x8d:
                adc_mac(L);
                //cycles = 4;
                break; //adc l
            case 0x8e:
                adc_mac(M);
                cycles = 7;
                break; //adc m
            case 0x8f:
                adc_mac(A);
                //cycles = 4;
                break; //adc a
            case 0x90:
                sub_mac(B);
                //cycles = 4;
                break; //sub b
            case 0x91:
                sub_mac(C);
                //cycles = 4;
                break; //sub c
            case 0x92:
                sub_mac(D);
                //cycles = 4;
                break; //sub d
            case 0x93:
                sub_mac(E);
                //cycles = 4;
                break; //sub e
            case 0x94:
                sub_mac(H);
                //cycles = 4;
                break; //sub h
            case 0x95:
                sub_mac(L);
                //cycles = 4;
                break; //sub l
            case 0x96:
                sub_mac(M);
                cycles = 7;
                break; //sub m
            case 0x97:
                sub_mac(A);
                //cycles = 4;
                break; //sub a
            case 0x98:
                sbb_mac(B);
                //cycles = 4;
                break; //sbb b
            case 0x99:
                sbb_mac(C);
                //cycles = 4;
                break; //sbb c
            case 0x9a:
                sbb_mac(D);
                //cycles = 4;
                break; //sbb d
            case 0x9b:
                sbb_mac(E);
                //cycles = 4;
                break; //sbb e
            case 0x9c:
                sbb_mac(H);
                //cycles = 4;
                break; //sbb h
            case 0x9d:
                sbb_mac(L);
                //cycles = 4;
                break; //sbb l
            case 0x9e:
                sbb_mac(M);
                cycles = 7;
                break; //sbb m
            case 0x9f:
                sbb_mac(A);
                //cycles = 4;
                break; //sbb a
            case 0xa0:
                ana_mac(B);
                //cycles = 4;
                break; //ana b
            case 0xa1:
                ana_mac(C);
                //cycles = 4;
                break; //ana c
            case 0xa2:
                ana_mac(D);
                //cycles = 4;
                break; //ana d
            case 0xa3:
                ana_mac(E);
                //cycles = 4;
                break; //ana e
            case 0xa4:
                ana_mac(H);
                //cycles = 4;
                break; //ana h
            case 0xa5:
                ana_mac(L);
                //cycles = 4;
                break; //ana l
            case 0xa6:
                ana_mac(M);
                cycles = 7;
                break; //ana m
            case 0xa7:
                ana_mac(A);
                //cycles = 4;
                break; //ana a
            case 0xa8:
                xra_mac(B);
                //cycles = 4;
                break; //xra b
            case 0xa9:
                xra_mac(C);
                //cycles = 4;
                break; //xra c
            case 0xaa:
                xra_mac(D);
                //cycles = 4;
                break; //xra d
            case 0xab:
                xra_mac(E);
                //cycles = 4;
                break; //xra e
            case 0xac:
                xra_mac(H);
                //cycles = 4;
                break; //xra h
            case 0xad:
                xra_mac(L);
                //cycles = 4;
                break; //xra l
            case 0xae:
                xra_mac(M);
                cycles = 7;
                break; //xra m
            case 0xaf:
                xra_mac(A);
                //cycles = 4;
                break; //xra a
            case 0xb0:
                ora_mac(B);
                //cycles = 4;
                break; //ora b
            case 0xb1:
                ora_mac(C);
                //cycles = 4;
                break; //ora c
            case 0xb2:
                ora_mac(D);
                //cycles = 4;
                break; //ora d
            case 0xb3:
                ora_mac(E);
                //cycles = 4;
                break; //ora e
            case 0xb4:
                ora_mac(H);
                //cycles = 4;
                break; //ora h
            case 0xb5:
                ora_mac(L);
                //cycles = 4;
                break; //ora l
            case 0xb6:
                ora_mac(M);
                cycles = 7;
                break; //ora m
            case 0xb7:
                ora_mac(a);
                //cycles = 4;
                break; //ora a
            case 0xb8:
                cmp_mac(b);
                //cycles = 4;
                break; //cmp b
            case 0xb9:
                cmp_mac(c);
                //cycles = 4;
                break; //cmp c
            case 0xba:
                cmp_mac(d);
                //cycles = 4;
                break; //cmp d
            case 0xbb:
                cmp_mac(e);
                //cycles = 4;
                break; //cmp e
            case 0xbc:
                cmp_mac(h);
                //cycles = 4;
                break; //cmp h
            case 0xbd:
                cmp_mac(l);
                //cycles = 4;
                break; //cmp l
            case 0xbe:
                cmp_mac(M);
                cycles = 7;
                break; //cmp m
            case 0xbf:
                cmp_mac(A);
                //cycles = 4;
                break; //cmp a
            case 0xc0:
                cycles = 5;
                if (!(getFlags() & zeroflag))
                {
                    putIP(popWord());
                    cycles = 11;
                }
                break; //rnz
            case 0xc1:
                putBC(popWord());
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
    }

    void putA(byte newValue)
    {
        a = newValue;
    }
    void putFlags(byte newValue)
    {
        flags = newValue;
    }
    void putB(byte newValue)
    {
        b = newValue;
    }
    void putC(byte newValue)
    {
        c = newValue;
    }
    void putD(byte newValue)
    {
        d = newValue;
    }
    void putE(byte newValue)
    {
        e = newValue;
    }
    void putH(byte newValue)
    {
        h = newValue;
    }
    void putL(byte newValue)
    {
        l = newValue;
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

    bool CPU::parityEven(WORD d)
    {
    auto rslt{ true };
    while (d != 0) {
        if (d & 1) rslt = !rslt;
        d = d >> 1;
    }
    return rslt;
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

    #define setFlagsZPS(r) {\
        BYTE tmpFlags; \
        tmpFlags = getFlags() & (~(zeroflag | parityflag | signflag)); \
        if (r == 0) tmpFlags |= zeroflag; \
        if (parityEven(r)) tmpFlags |= parityflag; \
        if (r & 0x80) tmpFlags |= signflag; \
        putFlags(tmpFlags); \
    }     
         */

}
