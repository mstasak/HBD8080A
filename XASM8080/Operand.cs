using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;

public enum OperandModel {
    None,
    R8Left,
    R8Right,
    R16WithSP,
    R16WithPSW,
    R16OnlyBD, //BC or DE only
    Imm8,
    Imm16,
    RstNum,
    DBList,
    DWList,
    DSSize
}

public class Operand {
    public string Name;
    public bool IsResolved;
    public string Text;
    public OperandModel OperandModel;
    public byte[]? Bytes;
    public byte OpcodeModifier;  //or'ed to opcode to insert a register, rp, or rst code
    public bool HasError;
    public string ErrorDescription;
}
