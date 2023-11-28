using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBD8080A.Core.Services;
internal class DisassemblerService {
    private ushort pc;
    private List<ushort> knownInstrAddrs;

    /// <summary>
    /// Disassemble instruction at current PC (1-3 bytes)
    /// </summary>
    /// <returns>A string including address, opcode, operand bytes if any, label if known, mnemonic and argument values (including AKA symbol if possible)</returns>
    internal string Disassemble() {
        var rslt = "NOP";

        return rslt;
    }
}
