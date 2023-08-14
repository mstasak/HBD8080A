using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MFComputer.Hardware.Computer;
interface IComputer
{
    ICPU cpu
    {
        get; set;
    }
    IMemory RAM
    {
        get; set;
    }
    IMemory ROM
    {
        get; set;
    }

    //Log logSystem;
    //Log logApp;
    //Log logError;
    //Log logUser;
    //Terminal console;
    //Terminal vTerm;
    //Terminal xTerm;
    //Terminal gTerm;
}
