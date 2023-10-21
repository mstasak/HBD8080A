using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public enum SymbolType {
    Address,
    TextString,
    Boolean
}
public class SymbolDefinition {
    public string Name = "";
    public SymbolType SymbolType;
    public ushort? WordValue;
    public string? TextStringValue;
    public bool? BooleanValue;
    public int ReferenceCount;
    public string DeclarationFileName = "";
    public int DeclarationLineNumber;
    public int ResolvedInPass;
}
