using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Text;

namespace MFComputer.Hardware.Computer;
interface IMemory
{
    ushort StartAddress
    {
        get; set;
    }
    ushort Length
    {
        get; set;
    }
    bool ReadOnly
    {
        get; set;
    }
    byte[] Data
    {
        get; set;
    }

    //short page {get;set;}
}
