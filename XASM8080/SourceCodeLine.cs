using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace XASM8080;
public class SourceCodeLine {
    /// <summary>
    /// This is the complete source code line; it will include line continuations (using \\\n)
    /// </summary>
    public string Source;
    private int LinePosition;
    public int? ErrorPosition;
    public string? ErrorMessage;
    /// <summary>
    /// Label defined at start of line:
    /// 
    /// ; global example (define symbols which may referenced by any file in the project)
    /// SOMELABEL: MOV A,B      ;COMMENT
    ///            ...
    ///            JMP SOMELABEL
    ///         
    /// ; local example (define symbol which may only be referenced between the last and next global symbol definition)
    /// .LOOP:     IN 0FFH      ;READ SENSE SWITCHES
    ///            ANI 01H      ;TEST switch0 position
    ///            JNZ .LOOP    ;SWITCH IS UP, PAUSE
    /// block-local example (define a range in which blocksymbols may be defined and referenced.  Useful in macros.)
    /// BLOCK TESTLOOP
    /// TESTLOOP.LOOP1: DCR C
    ///                 JNZ TESTLOOP.LOOP1
    /// ENDBLOCK TESTLOOP
    /// </summary>
    public SymbolDefinition? Label;
    public InstructionDefinition? Instruction;
    public byte? Opcode;
    public List<Operand> Operands;
    //private string AssemblerDirective; //future? listing options, strict switch, pass limits, forward ref disallowed flag, etc.
    public string? Comment;

    //lookup tables:
    internal static string[] regs8 = { "B", "C", "D", "E", "H", "L", "M", "A" };
    internal static string[] regs16SP = { "B", "D", "H", "SP" };
    internal static string[] regs16PSW = { "B", "D", "H", "PSW" };
    internal static string[] regs16BD = { "B", "D" };
    internal static string[] rstNums = { "0", "1", "2", "3", "4", "5", "6", "7" };

    public SourceCodeLine(string source) {
        Source = source;
        LinePosition = 0;
        Label = null;
        Instruction = null;
        Opcode = null;
        Operands = new();
        Comment = null;
    }
    public bool Parse() {
        ParseLabel();
        ParseInstruction(); //includes operands
        ParseComment();
        return ErrorMessage != null;
    }

    private void SkipSpace() {
    }

    private void ParseLabel() {
        SkipSpace();
        var labelSize = MatchRegExp("0-9A-Za-z_$.");
        if (labelSize > 0) {
            var LabelStr = Source[LinePosition..(LinePosition + labelSize - 1)];
            Debug.Assert(LabelStr != null); //not sure why type is string?
            //construct a very raw symbol definition - nothing but a name
            Label = new() {
                Name = LabelStr
            };
            Munch(labelSize);
        }
    }

    /// <summary>
    /// Test current input position against a regular expression (typically begins with ^).
    /// If matched, return length of match.
    /// LinePosition is not changed.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    private int MatchRegExp(string pattern, RegexOptions options = RegexOptions.IgnoreCase) {
        Debug.Assert(pattern.StartsWith("^")); //only interrested in matches at position 0
        var match = Regex.Match(Source[LinePosition..], pattern, options);
        if (match != null) {
            return match.Length;
        }
        return 0;
    }

    private string Munch(int skipCharacters) {
        var rslt = Source[LinePosition..(LinePosition + skipCharacters - 1)];
        LinePosition += skipCharacters;
        return rslt ?? "";
    }

    private void ParseInstruction() {
        SkipSpace();
        var opcodeLength = MatchRegExp("[A-Z]*");
        if (opcodeLength > 0) {
            //instruction found (maybe not valid)
            var opcodeString = Source[LinePosition..(LinePosition + opcodeLength - 1)];
            LinePosition += opcodeLength;
            var opcodeIx = InstructionSet8080.InstructionSet.ToList().FindIndex((instruction) => instruction.Mnemonic == opcodeString);

            if (opcodeIx > 0) {
                Instruction = InstructionSet8080.InstructionSet[opcodeIx];
            }
            if (Instruction.HasValue) {
                Opcode = Instruction.Value.Opcode;
                //parse operand(s)
                switch (Instruction.Value.OperandModel) {
                    case OperandModel.R8Left:
                        var oper = ParseOperandReg8();
                        if (!oper.HasError && Opcode.HasValue) {
                            Opcode = (byte)(Opcode | oper.OpcodeModifier);
                        }
                        break;
                    case OperandModel.R8Right:
                        var oper2 = ParseOperandReg8(false);
                        if (!oper2.HasError && Opcode.HasValue) {
                            Opcode = (byte)(Opcode | oper2.OpcodeModifier);
                        }
                        break;
                    case OperandModel.Imm8:
                        var oper3 = ParseOperandImmByte();
                        break;
                    case OperandModel.RstNum:
                        var oper4 = ParseOperandRst();
                        break;
                    case OperandModel.R16WithSP:
                        var oper5 = ParseOperandReg16WithSP();
                        if (!oper5.HasError && Opcode.HasValue) {
                            Opcode = (byte)(Opcode | oper5.OpcodeModifier);
                        }
                        break;
                    case OperandModel.R16WithPSW:
                        var oper6 = ParseOperandReg16WithPSW();
                        if (!oper6.HasError && Opcode.HasValue) {
                            Opcode = (byte)(Opcode | oper6.OpcodeModifier);
                        }
                        break;
                    case OperandModel.R16OnlyBD:
                        var oper7 = ParseOperandReg16OnlyBD();
                        if (!oper7.HasError && Opcode.HasValue) {
                            Opcode = (byte)(Opcode | oper7.OpcodeModifier);
                        }
                        break;
                    case OperandModel.Imm16:
                        var oper8 = ParseOperandImmWord();
                        break;
                    case OperandModel.DBList:
                        var oper9 = ParseOperandListDB();
                        break;
                    case OperandModel.DWList:
                        var oper10 = ParseOperandListDW();
                        break;
                    case OperandModel.DSSize:
                        var oper11 = ParseOperandImmWord();
                        break;
                    case OperandModel.None:
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void ParseComment() {
        SkipSpace();
        var found = MatchString(";");
        if (found) {
            Comment = Source[LinePosition..]; //rest of line, including ';'            
        }
    }

    /// <summary>
    /// Check if current source location matches a literal string; does not change LinePosition
    /// </summary>
    /// <param name="searchString">string to search for</param>
    /// <param name="caseInsensitive">true to make search case-insensitive</param>
    /// <returns></returns>
    private bool MatchString(string searchString, bool caseInsensitive = false) {
        if (Source[LinePosition..].StartsWith(searchString, caseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)) {
            return true;
        }
        return false;
    }

    private int ParseForLookupEntry(string[] LookupArray, ref string ParsedText) {
        SkipSpace();
        var rv = 0;
        foreach (var reg in LookupArray) {
            var matchLength = MatchRegExp(reg + "");
            if (matchLength >= 0) {
                ParsedText = Munch(matchLength);
                return rv;
            }
            rv++;
        }
        return -1;
    }

    private Operand ParseOperandReg8(bool isLeft = true) {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs8, ref parsedText);
        var rslt = new Operand() {
            Bytes = null,
            IsResolved = true,
            Name = "reg8",
            OperandModel = isLeft ? OperandModel.R8Left : OperandModel.R8Right,
            Text = parsedText
        };
        if (nWhich >= 0) {
            rslt.OpcodeModifier = (byte)((nWhich & 0x07) << 3);
            rslt.HasError = false;
            rslt.ErrorDescription = "";
        } else {
            rslt.OpcodeModifier = (byte)0;
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register name";
        }
        return rslt;
    }

    private Operand ParseOperandReg16WithSP() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16SP, ref parsedText);
        var rslt = new Operand() {
            Bytes = null,
            IsResolved = true,
            Name = "regpair16(BC/DE/HL/SP)",
            OperandModel = OperandModel.R16WithSP,
            Text = parsedText
        };
        if (nWhich >= 0) {
            rslt.OpcodeModifier = (byte)((nWhich & 0x03) << 4);
            rslt.HasError = false;
            rslt.ErrorDescription = "";
        } else {
            rslt.OpcodeModifier = (byte)0;
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register pair name";
        }
        return rslt;
    }
    private Operand ParseOperandReg16WithPSW() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16PSW, ref parsedText);
        var rslt = new Operand() {
            Bytes = null,
            IsResolved = true,
            Name = "regpair16(BC/DE/HL/PSW)",
            OperandModel = OperandModel.R16WithPSW,
            Text = parsedText
        };
        if (nWhich >= 0) {
            rslt.OpcodeModifier = (byte)((nWhich & 0x03) << 4);
            rslt.HasError = false;
            rslt.ErrorDescription = "";
        } else {
            rslt.OpcodeModifier = (byte)0;
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register pair name";
        }
        return rslt;
    }
    private Operand ParseOperandReg16OnlyBD() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16BD, ref parsedText);
        var rslt = new Operand() {
            Bytes = null,
            IsResolved = true,
            Name = "regpair16(BC/DE)",
            OperandModel = OperandModel.R16OnlyBD,
            Text = parsedText
        };
        if (nWhich >= 0) {
            rslt.OpcodeModifier = (byte)((nWhich & 0x03) << 4);
            rslt.HasError = false;
            rslt.ErrorDescription = "";
        } else {
            rslt.OpcodeModifier = (byte)0;
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register pair name";
        }
        return rslt;
    }

    private enum EnumOperatorType {
        Binary,
        Prefix,
        Suffix //not used?
    }

    private class OperatorDef {
        public int Precedence;
        public string Operator;
        public EnumOperatorType OperatorType;
        public Func<ushort, ushort, ushort> Calculate;

    }

    private OperatorDef[] Operators = {
        new OperatorDef { Precedence = 0, Operator = "|", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a | b) },
        new OperatorDef { Precedence = 1, Operator = "^", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a ^ b) },
        new OperatorDef { Precedence = 2, Operator = "&", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a & b) },
        new OperatorDef { Precedence = 3, Operator = "<<", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a << b) },
        new OperatorDef { Precedence = 3, Operator = ">>", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a >> b) },
        new OperatorDef { Precedence = 4, Operator = "+", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a + b) },
        new OperatorDef { Precedence = 4, Operator = "-", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a - b) },
        new OperatorDef { Precedence = 5, Operator = "*", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a * b) },
        new OperatorDef { Precedence = 5, Operator = "/", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a / b) },
        new OperatorDef { Precedence = 5, Operator = "%", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a % b) }
    };

    private Operand ParseNumericExpression() {
        Operand rslt;



        return rslt;
    }
}
