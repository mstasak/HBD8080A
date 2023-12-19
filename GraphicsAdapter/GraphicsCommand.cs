using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAdapter;
internal class GraphicsCommand {
    public GraphicsCommand() {
    }

}

/*
 * 
 * Command prefix:
 * Trying to avoid conflicts with common Esc commands.  My command prefix will be "Esc." (2 characters, i.e. 0x1b 0x2e)
 * http://www.braun-home.net/michael/info/misc/VT100_commands.htm
 * 
 * Command structure:
 * prefix, opcode, #operand bytes, operand data bytes
 * all of these are binary, not intended to be very human readable
 * there is no command terminator suffix
 * if the operand length is ff, the next two bytes are a little endian word specifying a potentially large number (0-65535) of operand bytes.
*/