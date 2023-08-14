using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFComputer.Hardware.Computer;
interface ICPU
{
    string Name
    {
        get;
    }

    IRegisters Registers
    {
        get; set;
    }

}
