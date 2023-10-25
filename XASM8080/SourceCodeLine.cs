﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public int LinePosition;
    public int? ErrorPosition;
    public string? ErrorMessage;
    /// <summary>
    /// Label defined at start of line:
    /// abc: instruction... - a "normal" symbol defined uniquely within this file and accessable from only this file
    /// $abc: instruction... - a global symbol which can be referenced from other files; unique to file
    /// .abc: instruction... - a "local" nonunique symbol local to a block which is either the span from 
    /// .blockname.abc: ...    one "normal" label to the next, or a containing block/endblock pair
    /// 
    /// </summary>
    public SymbolDefinition? Label;
    public InstructionDefinition? Instruction;
    public byte? Opcode;
    public List<Operand> Operands;
    //public string AssemblerDirective; //future? listing options, strict switch, pass limits, forward ref disallowed flag, etc.
    public string? Comment;

    //operand parse lookup tables:
    internal static string[] regs8 = { "B", "C", "D", "E", "H", "L", "M", "A" };
    internal static string[] regs16SP = { "B", "D", "H", "SP" };
    internal static string[] regs16PSW = { "B", "D", "H", "PSW" };
    internal static string[] regs16BD = { "B", "D" };
    internal static string[] rstNums = { "0", "1", "2", "3", "4", "5", "6", "7" };

    //public SymbolTable SymbolTable = new();

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
        ParseAddressLabel();
        ParseInstruction(); //includes operands
        ParseComment();
        return ErrorMessage != null;
    }

    public void SkipSpace() {
    }

    public void ParseAddressLabel() {
        SkipSpace();
        var labelSize = MatchRegExp("[0-9A-Za-z_$.]*:");
        if (labelSize > 0) {
            var LabelStr = Munch(labelSize - 1); // all but trailing colon
            Munch(1);
            Debug.Assert(LabelStr != null); //not sure why type is string?
            //construct a very raw symbol definition - nothing but a name
            Label = new() {
                Name = LabelStr,
                WordValue = CodeGenerator.Instance.MemoryAddress,
                ResolvedInPass = Assembler.Instance.Pass
            };
        }
    }

    /// <summary>
    /// Test current input position against a regular expression (typically begins with ^).
    /// If matched, return length of match.
    /// LinePosition is not changed.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public int MatchRegExp(string pattern, RegexOptions options = RegexOptions.IgnoreCase) {
        Debug.Assert(pattern.StartsWith("^")); //only interrested in matches at position 0
        var match = Regex.Match(Source[LinePosition..], pattern, options);
        if (match != null) {
            return match.Length;
        }
        return 0;
    }

    public string Munch(int skipCharacters) {
        var rslt = Source[LinePosition..(LinePosition + skipCharacters - 1)];
        LinePosition += skipCharacters;
        return rslt ?? "";
    }

    public void ParseInstruction() {
        SkipSpace();
        ParseHereLabel();
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

    public void ParseComment() {
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
    public bool MatchString(string searchString, bool caseInsensitive = false) {
        if (Source[LinePosition..].StartsWith(searchString, caseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)) {
            return true;
        }
        return false;
    }

    public int ParseForLookupEntry(string[] LookupArray, ref string ParsedText) {
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

    public Operand ParseOperandReg8(bool isLeft = true) {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs8, ref parsedText);
        var rslt = new Operand() {
            Bytes = null,
            IsResolved = true,
            //Name = "reg8",
            //OperandModel = isLeft ? OperandModel.R8Left : OperandModel.R8Right,
            Text = parsedText
        };
        if (nWhich >= 0) {
            rslt.OpcodeModifier = (byte)((nWhich & 0x07) << 3);
            rslt.HasError = false;
            rslt.ErrorDescription = "";
        } else {
            rslt.OpcodeModifier = 0;
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register name";
        }
        return rslt;
    }

    public Operand ParseOperandReg16WithSP() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16SP, ref parsedText);
        var rslt = new Operand() {
            Bytes = null,
            IsResolved = true,
            //Name = "regpair16(BC/DE/HL/SP)",
            //OperandModel = OperandModel.R16WithSP,
            Text = parsedText
        };
        if (nWhich >= 0) {
            rslt.OpcodeModifier = (byte)((nWhich & 0x03) << 4);
            rslt.HasError = false;
            rslt.ErrorDescription = "";
        } else {
            rslt.OpcodeModifier = 0;
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register pair name";
        }
        return rslt;
    }
    public Operand ParseOperandReg16WithPSW() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16PSW, ref parsedText);
        var rslt = new Operand() {
            Bytes = null,
            IsResolved = true,
            //Name = "regpair16(BC/DE/HL/PSW)",
            //OperandModel = OperandModel.R16WithPSW,
            Text = parsedText
        };
        if (nWhich >= 0) {
            rslt.OpcodeModifier = (byte)((nWhich & 0x03) << 4);
            rslt.HasError = false;
            rslt.ErrorDescription = "";
        } else {
            rslt.OpcodeModifier = 0;
            rslt.HasError = true;
            rslt.ErrorDescription = "Unrecognized register pair name";
        }
        return rslt;
    }
    public Operand ParseOperandReg16OnlyBD() {
        var parsedText = "";
        var nWhich = ParseForLookupEntry(regs16BD, ref parsedText);
        var rslt = new Operand() {
            Bytes = null,
            IsResolved = true,
            //Name = "regpair16(BC/DE)",
            //OperandModel = OperandModel.R16OnlyBD,
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

    public enum EnumOperatorType {
        Binary,
        Prefix,
        Suffix //not used?
    }

    public class OperatorDef {
        public int Precedence;
        public string Operator;
        public EnumOperatorType OperatorType;
        public Func<ushort, ushort, ushort> Calculate;

    }

    public OperatorDef[] Operators = {
        new OperatorDef { Precedence = 0, Operator = "|", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a | b) },
        new OperatorDef { Precedence = 1, Operator = "^", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a ^ b) },
        new OperatorDef { Precedence = 2, Operator = "&", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a & b) },
        new OperatorDef { Precedence = 3, Operator = "<<", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a << b) },
        new OperatorDef { Precedence = 3, Operator = ">>", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a >> b) },
        new OperatorDef { Precedence = 4, Operator = "+", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a + b) },
        new OperatorDef { Precedence = 4, Operator = "-", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a - b) },
        new OperatorDef { Precedence = 5, Operator = "*", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a * b) },
        new OperatorDef { Precedence = 5, Operator = "/", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a / b) },
        new OperatorDef { Precedence = 5, Operator = "%", OperatorType = EnumOperatorType.Binary, Calculate = (a, b) => (ushort)(a % b) },
        new OperatorDef { Precedence = 6, Operator = "+", OperatorType = EnumOperatorType.Prefix, Calculate = (a, _) => (ushort)+a },
        new OperatorDef { Precedence = 6, Operator = "-", OperatorType = EnumOperatorType.Prefix, Calculate = (a, _) => (ushort)-a },
        new OperatorDef { Precedence = 6, Operator = "~", OperatorType = EnumOperatorType.Prefix, Calculate = (a, _) => (ushort)~a },
        new OperatorDef { Precedence = 6, Operator = "<", OperatorType = EnumOperatorType.Prefix, Calculate = (a, _) => (ushort)(a & 0xff) },
        new OperatorDef { Precedence = 6, Operator = ">", OperatorType = EnumOperatorType.Prefix, Calculate = (a, _) => (ushort)(a >> 8) },
    };

    public Operand ParseNumericExpression(int precedenceLeval = 0) {
        Operand leftTerm;
        if (precedenceLeval > 5) {  //get a value
            leftTerm = ParseValue();
        } else { // get a value from next precedence level, parse an operator for this level, if found get another value, calculate, and repeat
            leftTerm = ParseNumericExpression(precedenceLeval + 1);
            if (leftTerm.Text == "") {
                return leftTerm;
            }
            var operators = Operators.Where(op => op.Precedence == precedenceLeval && op.OperatorType == EnumOperatorType.Binary);
            OperatorDef? oper;
            do {
                oper = ParseOperator(operators);
                if (oper == null) {
                    break;
                }
                var rightTerm = ParseNumericExpression(precedenceLeval + 1);
                if (rightTerm.Text == "") { //remember error but continue processing, e.g. DB 2 +   + 3; could skip to next comma or semicolon?
                    leftTerm.Text += " " + oper.Operator + " ";
                    if (!leftTerm.HasError) {
                        leftTerm.ErrorDescription = rightTerm.ErrorDescription;
                        leftTerm.HasError = true;
                    }
                } else {
                    leftTerm.Text += " " + oper.Operator + rightTerm.Text;
                    if (!leftTerm.HasError && rightTerm.HasError) {
                        leftTerm.ErrorDescription = rightTerm.ErrorDescription;
                        leftTerm.HasError = true;
                    }
                    leftTerm.WordValue = oper.Calculate(leftTerm.WordValue, rightTerm.WordValue);

                }

            } while (true);
        }
        return leftTerm;
    }

    public OperatorDef? ParseOperator(IEnumerable<OperatorDef> operators) {
        SkipSpace();
        var rslt = operators.FirstOrDefault(oper => MatchString(oper.Operator, false));
        if (rslt != null) {
            Munch(rslt.Operator.Length);
        }
        return rslt;
    }

    public Operand ParseValue() {
        //return a single value:
        //  a number (decimal or hex)
        //  a symbol
        //  (a parenthesized expression)
        //  prefix value (we'll tolerate unlimited prefixes like ---1)
        Operand value;
        SkipSpace();

        //number - hex
        value = ParseHexadecimalNumber();
        if (!value.HasError) {
            return value;
        }

        //number - decimal
        value = ParseDecimalNumber();
        if (!value.HasError) {
            return value;
        }

        //symbol
        value = ParseLabel();
        if (!value.HasError) {
            return value;
        }

        //(expr)
        SkipSpace();
        if (MatchString("(")) {
            Munch(1);
            value = ParseNumericExpression();
            if (MatchString(")")) {
                Munch(1);
            } else {
                if (!value.HasError) {
                    value.HasError = true;
                    value.ErrorDescription = "Missing right perenthesis in operand expression.";
                }
            }
            return value;
        }

        //prefix val
        var operators = Operators.Where(op => op.Precedence == 6 && op.OperatorType == EnumOperatorType.Prefix);
        OperatorDef? oper;
        oper = ParseOperator(operators);
        if (oper != null) {
            value = ParseValue();
            value.WordValue = oper.Calculate(value.WordValue, 0);
            return value;
        }

        //expression error - generic
        value = new Operand() {
            HasError = true,
            ErrorDescription = "Error evaluating an operand - valid value not found.",
        };
        return value;
    }


    public Operand ParseDecimalNumber() {
        //TODO: implement distinct version for byte-sized operands, range 0 to 255
        var MatchLen = MatchRegExp("^[0-9]+^[H]");
        ushort rsltVal;
        if (MatchLen > 0) {
            var strDecimalNum = Munch(MatchLen);
            if (ushort.TryParse(strDecimalNum, System.Globalization.NumberStyles.HexNumber, null, out rsltVal)) {
                //return result built on rsltVal
                return new Operand() { Text = strDecimalNum, WordValue = rsltVal };
            }
            return new Operand() { HasError = true, ErrorDescription = "Range error?  Could not convert numeric literal to word (0000H - 0FFFFH, or 0-65535)." };
        }
        return new Operand() { HasError = true, ErrorDescription = "Did not find a valid number" };
    }

    public Operand ParseHexadecimalNumber() {
        //TODO: implement distinct version for byte-sized operands, range 0 to FF (255)
        //TODO: implement alternative hex number syntaxes ($ffff, 0xff, &HFFA0)
        var MatchLen = MatchRegExp("^[0-9][0-9ABCDEF]+H");
        ushort rsltVal;
        if (MatchLen > 0) {
            var strHexadecimalNum = Munch(MatchLen);

            if (ushort.TryParse(strHexadecimalNum[0..(strHexadecimalNum.Length - 1)], out rsltVal)) {
                //return result built on rsltVal
                return new Operand() { Text = strHexadecimalNum, WordValue = rsltVal };
            }
            return new Operand() { HasError = true, ErrorDescription = "Range error?  Could not convert numeric literal to word (0-65535)." };
        }
        return new Operand() { HasError = true, ErrorDescription = "Did not find a valid number" };
    }

    //public int MatchRegExp(string pattern, RegexOptions options = RegexOptions.IgnoreCase) {
    //    Debug.Assert(pattern.StartsWith("^")); //only interested in matches at position 0
    //    var matches = Regex.Matches(Source[LinePosition..], pattern, options);
    //    if (match != null) {
    //        return match.Length;
    //    }
    //    return 0;
    //}

    /// <summary>
    /// Parse a label operand value, as part of an operand expression (e.g. JMP label)
    /// </summary>
    /// <returns>An Operand object, with appropriate error properties as needed</returns>
    public Operand ParseLabelRef() {
        //symbol flavors declared:
        // abc: instruction... - a "normal" symbol defined uniquely within this file and accessable from only this file
        // $abc: instruction... - a global symbol which can be referenced from other files; unique to file
        // .abc: instruction... - a "local" nonunique symbol local to a block which is either the span from 
        // .blockname.abc: ...    one "normal" label to the next, or a containing block/endblock pair
        //symbol flavors referenced
        // abc - a "normal" symbol defined uniquely within this file and accessable from only this file
        // $abc - a "global" symbol from any file processed
        // $filename[.ext].abc - a "global" symbol from a specific file processed
        // .abc - a non-unique "local" symbol defined after the most recent normal (filewide) symbol
        // underthislabel.abc - a non-unique "local" symbol defined after the specified normal (filewide) symbol
        // blockname.abc - a non-unique "local" .symbol defined within a 'BLOCK blockname / ENDBLOCK blockname' range (not implemented)
        SkipSpace();
        int symbolLen;
        Regex re;

        //CASE: "normal label" - unique in file, invisible outside file
        re = new("^[a-z_][a-z_0-9]*", RegexOptions.Singleline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        var match = re.Match(Source, LinePosition);
        if (match.Success) {
            // "normal" label like abc_1
            var mLen = match.Length;
            var symbolName = Munch(mLen);
            var sym = new SymbolDefinition() {
                Name = symbolName,
                SymbolType = SymbolType.AddressFile,
                //WordValue = null,
                //TextStringValue = null,
                //bool? BooleanValue = null,
                ReferenceCount = 1,
                //DeclarationFileName = "",
                //DeclarationLineNumber,
                //ResolvedInPass
            };
            var checkedSymbol = SymbolTable.ProcessReference(sym); //add unresolved entry if not present; update ref count and get value, resolved status if found
            //symbol.AddReference(CurrentSourceCodeLocation());
            return checkedSymbol;
        }

        //CASE: "global label" - unique per file, may be forward-referenced (unresolved until later pass, add filename and address later)
        re = new("^\\$[a-z_][a-z_0-9]*[^\\.]", RegexOptions.Singleline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        match = re.Match(Source, LinePosition);
        if (match.Success) {
            // "global" label like $abc_1
            var mLen = match.Length;
            _ = Munch(1);
            var symbolName = Munch(mLen - 1);
            var sym = new SymbolDefinition() {
                Name = symbolName,
                SymbolType = SymbolType.AddressGlobal,
                //WordValue = null,
                //TextStringValue = null,
                //bool? BooleanValue = null,
                ReferenceCount = 1,
                DeclarationFileName = "",
                //DeclarationLineNumber,
                //ResolvedInPass
            };
            var checkedSymbol = SymbolTable.ProcessReference(); //add unresolved entry if not present; update ref count and get value, resolved status if found
            //symbol.AddReference(CurrentSourceCodeLocation());
            return checkedSymbol;
        }

        //CASE: "global filename.label" - unique per file, may be forward-referenced (unresolved until later pass, add filename and address later)
        //should rethink this - file.ext.label is a little bit of an RE parse headache, and should probably allow any valid filename within ""s
        //on second thought, should examine some modern assemblers for better approaches - NASM, Turbo-Asm, MASM, etc.
        re = new("^\\$([a-z_][a-z_0-9]*\\.([a-z_][a-z_0-9]*)", RegexOptions.Singleline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        match = re.Match(Source, LinePosition);
        if (match.Success) {
            // "global" label like $filename.abc_1
            var mLen = match.Length;
            _ = Munch(1);
            var symbolName = Munch(mLen - 1);
            var sym = new SymbolDefinition() {
                Name = symbolName,
                SymbolType = SymbolType.AddressGlobal,
                //WordValue = null,
                //TextStringValue = null,
                //bool? BooleanValue = null,
                ReferenceCount = 1,
                DeclarationFileName = "",
                //DeclarationLineNumber,
                //ResolvedInPass
            };
            var checkedSymbol = SymbolTable.ProcessReference(); //add unresolved entry if not present; update ref count and get value, resolved status if found
            //symbol.AddReference(CurrentSourceCodeLocation());
            return checkedSymbol;
        }

        //CASE: "local label" - non-unique per file, may be forward-referenced (unresolved until later pass, add address later)
        re = new("^\\.[a-z_][a-z_0-9]*[^\\.]", RegexOptions.Singleline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        match = re.Match(Source, LinePosition);
        if (match.Success) {
            // "local" label like .abc_1
            var mLen = match.Length;
            _ = Munch(1);
            var symbolName = Munch(mLen - 1);
            var sym = new SymbolDefinition() {
                Name = Assembler.Instance.MostRecentNormalLineLabel,
                LocalSubLabel = symbolName,
                SymbolType = SymbolType.AddressLocal,
                //WordValue = null,
                //TextStringValue = null,
                //bool? BooleanValue = null,
                ReferenceCount = 1,
                DeclarationFileName = Assembler.Instance.currentFileName,
                //DeclarationLineNumber,
                //ResolvedInPass
            };
            var checkedSymbol = SymbolTable.ProcessReference(); //add unresolved entry if not present; update ref count and get value, resolved status if found
            //symbol.AddReference(CurrentSourceCodeLocation());
            return checkedSymbol;
        }

    }
}