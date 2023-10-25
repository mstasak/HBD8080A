using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public class CodeGenerator {

    public ushort? MemoryAddress {
        get; set;
    }

    public byte[] CodeBuffer {
        get; set;
    }
    

    private static readonly Lazy<CodeGenerator> lazy =
        new(() => new CodeGenerator());

    public static CodeGenerator Instance => lazy.Value;

    private CodeGenerator() {
    }

    public void Reset(int Pass, int Address, bool FinalPass) => throw new NotImplementedException();
}


