## MFComputer I/O Ports

## Input

FF - Input Sense Switches (front panel)

	Latest value is available via an IN instruction, but there is no way other
	than polling to detect a change in input value.

## Output

FF - Output LEDs (front panel)

	Simple parallel output - always displays the last value output - no mechanism
	to detect changes when written or to identify/enumerate consecutive outputs of
	the same value.

## References

### Text Terminals

TV Typewriter - https://deramp.com/swtpc.com/RadioElectronics/TV_Typewriter.htm

ASR-33 - 

ADM-3A - 

VT-52 - https://en.wikipedia.org/wiki/VT52 https://github.com/microsoft/terminal/blob/main/doc/specs/%23976%20-%20VT52%20escape%20sequences.md

VT-100 - 

## Roll my own

F0 - TTControl

	Input & 0x01: Read Ready - can now input a byte from TTData

	Input & 0x02: Write Ready - can now write a byte to TTData

	Output & 0x01: Read Completed - clear Read Ready and wait for another Read
	Ready to input another character

	Output & 0x02: Write Completed - clear Write Ready and will wait for
	another Write Ready to output next character

F1 - TTData

    Input or Output: read or write the next character

	### Command Strings (from VT-52?)
	00-ff - simple characters, EXCEPT:
	  08: Tab
	  09: Backspace
	  0A: Line feed (move down a line)
	  0D: Carriage Return (move to leftmost column 0)
	  1B: Esc (introduces a multi-character command string)
	  ??: Clear
	  ??: Home
	  ??: Insert/Overwrite
	      (note: most or all of codes 00-1F have some command functionality 
	      defined in ASCII; for the most part, we don't need these)
    Esc+chars VT-52 strings to move cursor, set color, clear screen, etc
	  
	  
    Code	Name	Meaning
    
    ESCA	Cursor up	            Move cursor one line upwards.
                                    Does not cause scrolling when it reaches the top.
    ESCB	Cursor down	            Move cursor one line downwards.
    ESCC	Cursor right	        Move cursor one column to the right.
    ESCD	Cursor left	            Move cursor one column to the left.
    ESCF	Enter graphics mode	    Use special graphics character set, VT52 and later.
    ESCG	Exit graphics mode	    Use normal US/UK character set
    ESCH	Cursor home	            Move cursor to the upper left corner.
    ESCI	Reverse line feed	    Move cursor one line upwards.
                                    But if it is already in the top line, instead scroll all content down one line.
    ESCJ	Clear to end of screen	Clear screen from cursor onwards.
    ESCK	Clear to end of line	Clear line from cursor onwards.
    ESCYrc	Set cursor position	    Move cursor to position c,r, encoded as single characters.
                                    The VT50H also added the "SO" command that worked identically,
                                    providing backward compatibility with the VT05.
    ESCZ	ident	                Identify what the terminal is, see notes below.
    ESC=	Alternate keypad	    Changes the character codes returned by the keypad.
    ESC>	Exit alternate keypad	Changes the character codes returned by the keypad.
    
    
    ESC E	Clear screen	        Clear screen and place cursor at top left corner.
                                    Essentially the same as ESCHESCJ
    ESC b#	Foreground color	    Set text colour to the selected value
    ESC c#	Background color	    Set background colour
    ESC d	Clear to start of screen	Clear screen from the cursor up to the home position.
    ESC e	Enable cursor	        Makes the cursor visible on the screen.
    ESC f	Disable cursor	        Makes the cursor invisible.
    ESC j	Save cursor	            Saves the current position of the cursor in memory, TOS 1.02 and later.
    ESC k	Restore cursor	        Return the cursor to the settings previously saved with j.
    ESC l	Clear line	            Erase the entire line and positions the cursor on the left.
    ESC o	Clear to start of line	Clear current line from the start to the left side to the cursor.
    ESC p	Reverse video	        Switch on inverse video text.
    ESC q	Normal video	        Switch off inverse video text.
    ESC v	Wrap on	                Enable line wrap, removing the need for CR/LF at line endings.
    ESC w	Wrap off	            Disable line wrap.