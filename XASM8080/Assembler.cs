using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XASM8080;

/**
 * Multi-pass assembler, using "standard"-ish Microsoft/Intel syntax.
 * Not to pleased with the first cut, may scrap most and do over.
 * The details of a simple process do add up.  Too big for a reasonable
 * single-file program, should probably make distinct classes for 
 * instruction set, output generator(s), expression evaluation, symbol table.
**/



//internal struct  AssemblyCodeLine {
//    internal string rawLine;
//    internal string? Label;
//    internal string? Mnemonic;
//    internal List<Operand>? OperandList;
//    internal string? Comment;
//}


internal class Assembler {

    public static readonly char[] WhitespaceChars = new char[] { ' ', '\t' };
    public SymbolTable SymbolTable;
    public CodeGenerator CodeGenerator;
    public List<string> InputFilePaths;

    private int Pass;
    private int PriorPassUnresolvedSymbolRefs;
    //private bool finalPass = false;
    //private int passErrorCount = 0;
    //private int passUnresolvedSymbols = 0; //symbol value is uncertain; must reach 0 for successful compile
    //private int passUndefinedSymbols = 0; //symbol not known; ok on pass 1 for forward reference

    ////private int? minAddressWritten;
    ////private int? maxAddressWritten;
    ////private byte[] outputBuffer = new byte[65536];

    private string? currentFileName;
    private int currentLineNumber; //within file
    ////private string currentLinePart; // label, opcode, operand n, comment
    ////private string? currentLineLabel;
    ////private ushort? currentLineLabelValue;
    ////private string? currentLineInstruction;
    ////private byte? currentLineData;
    ////private string? currentLineOperandText;
    ////private List<byte>? currentLineOperandData;
    ////private string? currentLineComment;
    ////private string? currentLineError;
    //private SourceCodeLine? currentLine;
    ////private int? priorPassTotalSymbols;
    //private int passUnresolvedSymbolRefs;

    //public Dictionary<string, ushort?> Symbols {
    //    get; set;
    //}

    internal Assembler(IEnumerable<string> filePaths) {
        SymbolTable = new();
        CodeGenerator = new();
        InputFilePaths = new(filePaths);
    }

    internal void Assemble() {
        Pass = 1;
        PriorPassUnresolvedSymbolRefs = 0;
        bool needAnotherPass;
        //bool readyForFinalPass = false; //will be set when no issues remain, or last pass made no progress
        do {
            CodeGenerator.Reset(Pass: Pass, Address: 0, FinalPass: false);
            AssemblePass();
            var passUnresolvedSymbolRefs = SymbolTable.UnresolvedSymbolRefCount();
            //int passUnknownSymbolCount = SymbolTable.UnknownSymbolCount();
            needAnotherPass = (passUnresolvedSymbolRefs > 0) && (passUnresolvedSymbolRefs < PriorPassUnresolvedSymbolRefs);
            PriorPassUnresolvedSymbolRefs = passUnresolvedSymbolRefs;
            Pass++;
        } while (needAnotherPass);
        if (PriorPassUnresolvedSymbolRefs > 0) {
            Pass--;
            DisplayMessage($"Assembly aborted after {Pass} passes.  There are {PriorPassUnresolvedSymbolRefs} unresolvable symbols.");
        } else {
            //begin final pass
            CodeGenerator.Reset(Pass: Pass, Address: 0, FinalPass: true);
            AssemblePass();
            DisplayMessage($"Assembly completed in {Pass} passes.");
            //CodeGenerator.DisplayOutputStatistics();
        }
    }

    private void DisplayMessage(string v) {
        Console.WriteLine(v);
    }

    private void AssemblePass() {
        //throw new NotImplementedException();
        //passErrorCount = 0;
        foreach (var fileName in InputFilePaths) {
            currentFileName = fileName;
            currentLineNumber = 1;
            using var inFile = File.OpenText(fileName);
            if (inFile != null) {
                var s = GetLineWithContinuations(inFile);
                while (s != null) {
                    AssembleLine(s);
                    s = GetLineWithContinuations(inFile);
                }
            }
        }
    }

    private string? GetLineWithContinuations(StreamReader inFile) {
        var s = inFile.ReadLine();
        if (s != null) {
            while (s.EndsWith("\\") && !inFile.EndOfStream) {
                s = string.Concat(s.AsSpan(0, s.Length - 1), inFile.ReadLine());
            }
        }
        return s;
    }

    private void AssembleLine(string s) {
        //string? Label;
        //string? Opcode;
        //List<Operand> Operands = new();
        //string? Comment;

        ////parse line parts: [label;] [opcode [operand [,operand...]]] [;comment]
        
        ////label
        //var match = Regex.Match(s, "^([0-9A-Z_a-z]*\\:)\\s",RegexOptions.IgnoreCase);
        //if (match.Success) {
        //    Label = match.Groups[1].Value;
        //    s = s.Substring(match.Length);
        //}
        //s = s.TrimStart();

        ////opcode
        //var opcodeEnd = s.IndexOfAny(WhitespaceChars);
        //if (opcodeEnd == -1) {
        //    Opcode = s;
        //} else {
        //    Opcode = s.Substring(0, opcodeEnd);
        //    s = s.Substring(opcodeEnd + 1);
        //}
        //if (s.Length == 0) {
        //    Opcode = null;
        //}

        ////operand(s)
        //s = s.TrimStart();
        //if (Opcode != null) {
        //    while (s.Length > 0 && !s.StartsWith(';')) {
        //        var operand = ParseOperand(ref s);
        //        if (operand == null) {
        //            //error?
        //            DisplayMessage($"Syntax error? Expected: Operand value, comment, or line end.");
        //            break;
        //        }
        //    }
        //}

        ////comment
        //s = s.TrimStart();
        //if (s.StartsWith(';')) { 
        //    Comment = s.Substring(1);
        //    s = "";
        //}
        ////currentLineLabel = null;
        ////currentLineLabelValue = null;
        ////currentLineOpcodeText = null;
        ////currentLineOpcodeData = null;
        ////currentLineOperandText = null;
        ////currentLineOperandData = null;
        ////currentLineComment = null;
        ////currentLineError = null;

        ////parseLabel(ref s);
        ////parseOpcode(ref s);
        ////parseOperands(ref s);
        ////parseComment(s);
        ////emitCodeLine();
        ////emitListingLine();
        ////emitErrorLine();
        ////if (currentLineLabel != null) {
        ////    emitSymbolTableLine();
        ////}
    }

    //private void parseLabel(ref string s) {
    //    if (s.StartsWith(';')) {
    //        return;
    //    }
    //    if (s.StartsWith(' ') || s.StartsWith('\t')) {
    //        s = s.TrimStart();
    //        return;
    //    }
    //    var labelLength = s.IndexOfAny(WhitespaceChars);
    //    var label = s.Substring(0, labelLength).TrimStart().TrimEnd(':');
    //    currentLineLabel = label;
    //    s = s.Substring(labelLength).TrimStart();
    //    return;
    //}

    //private void parseOpcode(ref string s) {
    //    //throw new NotImplementedException();
    //    if (s.StartsWith(';')) {
    //        return;
    //    }

    //    var endOpcd = s.IndexOfAny(WhitespaceChars);
    //    if (endOpcd == -1) {
    //        currentLineOpcodeText = s;
    //        s = "";
    //    } else {
    //        currentLineOpcodeText = s.Substring(0, endOpcd);
    //        s = s.Substring(endOpcd).TrimStart();
    //    }
    //    var NotFound = new InstructionDef();
    //    var instr = InstructionSet8080.FirstOrDefault(i => i.mnemonic == currentLineOpcodeText, NotFound);
    //    if (instr.mnemonic != "") {
    //        currentLineOpcodeData = instr.opcode;
    //    } else {
    //        composeError($"unknown opcode '{currentLineOpcodeText}'");
    //    }
    //    return;
    //}

    //private void composeError(string err) {
    //    var formattedError = $"Error in file {currentFileName}, line {currentLineNumber}: {err}.";
    //    currentLineError = formattedError;
    //}

    //private void parseOperands(ref string s) {

    //}

    //private string? parseComment(string s) => throw new NotImplementedException();

    //private void emitSymbolTableLine() => throw new NotImplementedException();

    //private void emitListingLine() => throw new NotImplementedException();

    //private void emitCodeLine() => throw new NotImplementedException();

    //private void emitErrorLine() => throw new NotImplementedException();

}
