using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;

internal enum OperandModel {
    None,
    OneR8,
    TwoR8,
    OneR16,
    OneR16BD, //BC or DE only
    Imm8,
    Imm16,
    R8Imm8,
    R16Imm16,
    RstNum,
    DBList,
    DWList
}

internal struct InstructionDef {
    public string mnemonic;
    public byte opcode;
    public OperandModel operands;
}

internal class Assembler {
    private static readonly char[] WhitespaceChars = new char[] { ' ', '\t' };
    private int pass;
    private string? currentFileName;
    private int currentLineNumber;
    private int? priorPassUnresolvedSymbols;
    private int? priorPassTotalSymbols;
    private int passUnresolvedSymbols;
    private int? minAddressWritten;
    private int? maxAddressWritten;
    private byte[] outputBuffer = new byte[65536];

    private ushort address;
    public Dictionary<string, ushort?> Symbols {
        get; set;
    }
    public List<string> InputFileNames {
        get; set;
    }

    public Assembler(IEnumerable<string> fNames) {
        Symbols = new();
        InputFileNames = new(fNames);
    }

    public void Assemble() {
        pass = 0;
        priorPassUnresolvedSymbols = null;
        priorPassTotalSymbols = null;
        passUnresolvedSymbols = 0;
        address = 0;
        bool progressMade;
        do {
            pass++;
            AssemblePass();
            passUnresolvedSymbols = Symbols.Count((kvPair) => !kvPair.Value.HasValue);
            progressMade = (passUnresolvedSymbols > 0) && ((passUnresolvedSymbols < priorPassUnresolvedSymbols) || (Symbols.Count > priorPassTotalSymbols));
            priorPassTotalSymbols = Symbols.Count;
            priorPassUnresolvedSymbols = passUnresolvedSymbols;
        } while (progressMade);

    }

    private void AssemblePass() {
        //throw new NotImplementedException();
        address = 0;
        foreach (var fileName in InputFileNames) {
            currentFileName = fileName;
            currentLineNumber = 1;
            using var inFile = File.OpenText(fileName);
            if (inFile != null) {
                var s = inFile.ReadLine();
                while (s != null) {
                    AssembleLine(s);
                    s = inFile.ReadLine();
                }
            }
        }
    }

    private void AssembleLine(string s) {
        string? label;
        string? opcode;
        List<string> operands = new();
        string? comment;
        label = parseLabel(ref s);
        opcode = parseOpcode(ref s);
        parseOperands(ref s);
        comment = parseComment(s);
        emitCodeLine();
        emitListingLine();
        if (label != null) {
            emitSymbolTableLine();
        }
    }

    private string? parseLabel(ref string s) {
        if (s.StartsWith(';')) {
            return null;
        }
        if (s.StartsWith(' ') || s.StartsWith('\t')) {
            s = s.TrimStart();
            return null;
        }
        var labelLength = s.IndexOfAny(WhitespaceChars);
        var label = s.Substring(0, labelLength);
        s = s.Substring(labelLength).TrimStart();
        return label;
    }

    private string? parseOpcode(ref string s) {
        //throw new NotImplementedException();
        if (s.StartsWith(';')) {
            return null;
        }
        return "TBD()";
    }

    private void parseOperands(ref string s) => throw new NotImplementedException();

    private string? parseComment(string s) => throw new NotImplementedException();

    private void emitCode() => throw new NotImplementedException();

    private void emitSymbolTableLine() => throw new NotImplementedException();

    private void emitListingLine() => throw new NotImplementedException();

    private void emitCodeLine() => throw new NotImplementedException();

    private readonly InstructionDef[] InstructionSet = {
        new InstructionDef {mnemonic = "NOP", opcode = 0x00, operands = OperandModel.None},
        new InstructionDef {mnemonic = "LXI", opcode = 0x01, operands = OperandModel.OneR16},
        new InstructionDef {mnemonic = "STAX", opcode = 0x02, operands = OperandModel.OneR16BD},
        new InstructionDef {mnemonic = "INX", opcode = 0x03, operands = OperandModel.OneR16},
        new InstructionDef {mnemonic = "INR", opcode = 0x04, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "DCR", opcode = 0x05, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "MVI", opcode = 0x06, operands = OperandModel.R8Imm8},
        new InstructionDef {mnemonic = "RLC", opcode = 0x07, operands = OperandModel.None},
        // unused opcode 0x08
        new InstructionDef {mnemonic = "DAD", opcode = 0x09, operands = OperandModel.OneR16},
        new InstructionDef {mnemonic = "LDAX", opcode = 0x0A, operands = OperandModel.OneR16BD},
        new InstructionDef {mnemonic = "DCX", opcode = 0x0B, operands = OperandModel.OneR16},
        new InstructionDef {mnemonic = "RRC", opcode = 0x0F, operands = OperandModel.None},
        // unused opcode 0x10
        new InstructionDef {mnemonic = "RAL", opcode = 0x17, operands = OperandModel.None},
        // unused opcode 0x18
        new InstructionDef {mnemonic = "RAR", opcode = 0x1F, operands = OperandModel.None},
        // unused opcode 0x20
        new InstructionDef {mnemonic = "SHLD", opcode = 0x22, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "DAA", opcode = 0x27, operands = OperandModel.None},
        // unused opcode 0x28
        new InstructionDef {mnemonic = "LHLD", opcode = 0x2A, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "CMA", opcode = 0x2F, operands = OperandModel.None},
        new InstructionDef {mnemonic = "STA", opcode = 0x32, operands = OperandModel.Imm16},
        // unused opcode 0x30
        new InstructionDef {mnemonic = "STC", opcode = 0x37, operands = OperandModel.None},
        // unused opcode 0x38
        new InstructionDef {mnemonic = "LDA", opcode = 0x3A, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "CMC", opcode = 0x3F, operands = OperandModel.None},
        new InstructionDef {mnemonic = "MOV", opcode = 0x40, operands = OperandModel.TwoR8},
        new InstructionDef {mnemonic = "HLT", opcode = 0x76, operands = OperandModel.None},
        new InstructionDef {mnemonic = "ADD", opcode = 0x80, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "ADC", opcode = 0x88, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "SUB", opcode = 0x90, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "SBB", opcode = 0x98, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "ANA", opcode = 0xA0, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "XRA", opcode = 0xA8, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "ORA", opcode = 0xB0, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "CMP", opcode = 0xB8, operands = OperandModel.OneR8},
        new InstructionDef {mnemonic = "RNZ", opcode = 0xC0, operands = OperandModel.None},
        new InstructionDef {mnemonic = "POP", opcode = 0xC1, operands = OperandModel.OneR16},
        new InstructionDef {mnemonic = "JNZ", opcode = 0xC2, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "JMP", opcode = 0xC3, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "CNZ", opcode = 0xC4, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "PUSH", opcode = 0xC5, operands = OperandModel.OneR16},
        new InstructionDef {mnemonic = "ADI", opcode = 0xC6, operands = OperandModel.Imm8},
        new InstructionDef {mnemonic = "RST", opcode = 0xC7, operands = OperandModel.RstNum},
        new InstructionDef {mnemonic = "RZ", opcode = 0xC8, operands = OperandModel.None},
        new InstructionDef {mnemonic = "RET", opcode = 0xC9, operands = OperandModel.None},
        new InstructionDef {mnemonic = "JZ", opcode = 0xCA, operands = OperandModel.Imm16},
        // unused opcode 0xCB
        new InstructionDef {mnemonic = "CZ", opcode = 0xCC, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "CALL", opcode = 0xCD, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "ACI", opcode = 0xCE, operands = OperandModel.Imm8},
        new InstructionDef {mnemonic = "RNC", opcode = 0xD0, operands = OperandModel.None},
        new InstructionDef {mnemonic = "JNC", opcode = 0xD2, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "OUT", opcode = 0xD3, operands = OperandModel.Imm8},
        new InstructionDef {mnemonic = "CNC", opcode = 0xD4, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "SUI", opcode = 0xD6, operands = OperandModel.Imm8},
        new InstructionDef {mnemonic = "RC", opcode = 0xD8, operands = OperandModel.None},
        // unused opcode 0xD9
        new InstructionDef {mnemonic = "JC", opcode = 0xDA, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "IN", opcode = 0xDB, operands = OperandModel.Imm8},
        new InstructionDef {mnemonic = "CC", opcode = 0xDC, operands = OperandModel.Imm16},
        // unused opcode 0xDD
        new InstructionDef {mnemonic = "SBI", opcode = 0xDE, operands = OperandModel.Imm8},
        new InstructionDef {mnemonic = "RPO", opcode = 0xE0, operands = OperandModel.None},
        new InstructionDef {mnemonic = "JPO", opcode = 0xE2, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "XTHL", opcode = 0xE3, operands = OperandModel.None},
        new InstructionDef {mnemonic = "CPO", opcode = 0xE4, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "ANI", opcode = 0xE6, operands = OperandModel.Imm8},
        new InstructionDef {mnemonic = "RPE", opcode = 0xE8, operands = OperandModel.None},
        new InstructionDef {mnemonic = "PCHL", opcode = 0xE9, operands = OperandModel.None},
        new InstructionDef {mnemonic = "JPE", opcode = 0xEA, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "XCHG", opcode = 0xEB, operands = OperandModel.None},
        new InstructionDef {mnemonic = "CPE", opcode = 0xEC, operands = OperandModel.Imm16},
        // unused opcode 0xED
        new InstructionDef {mnemonic = "XRI", opcode = 0xEE, operands = OperandModel.Imm8},

        new InstructionDef {mnemonic = "RPO", opcode = 0xF0, operands = OperandModel.None},
        new InstructionDef {mnemonic = "JPO", opcode = 0xF2, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "DI", opcode = 0xF3, operands = OperandModel.None},
        new InstructionDef {mnemonic = "CP", opcode = 0xF4, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "ORI", opcode = 0xF6, operands = OperandModel.Imm8},
        new InstructionDef {mnemonic = "RM", opcode = 0xF8, operands = OperandModel.None},
        new InstructionDef {mnemonic = "SPHL", opcode = 0xF9, operands = OperandModel.None},
        new InstructionDef {mnemonic = "JM", opcode = 0xFA, operands = OperandModel.Imm16},
        new InstructionDef {mnemonic = "EI", opcode = 0xFB, operands = OperandModel.None},
        new InstructionDef {mnemonic = "CM", opcode = 0xFC, operands = OperandModel.Imm16},
        // unused opcode 0xFD
        new InstructionDef {mnemonic = "CFI", opcode = 0xFE, operands = OperandModel.Imm8},
    };
}
