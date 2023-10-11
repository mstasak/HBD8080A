using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
internal class Assembler {
    private static readonly char[] WhitespaceChars = new char[] { ' ', '\t' };
    private int pass;
    private string currentFileName;
    private int currentLineNumber;
    private int? priorPassUnresolvedSymbols;
    private int? priorPassTotalSymbols;
    private int passUnresolvedSymbols;
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

}
