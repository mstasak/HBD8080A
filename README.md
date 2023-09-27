# MFComputer
A minimally functional computer simulator/emulator. (maybe some day moderately
or maximally???) 

I initially wanted to craft my own imaginary cpu with a barebones instruction 
set based on selected features from the 6502, 8080A, and maybe others.  Later 
to be fleshed out with a monitor, assembler, tty terminal interface, text 
video output, APA video output, persistent storage, network capabilities.

This is rapidly changing into an 8080A emulator appliance in C#.  This feels
fitting, since 2024 will be the 50th anniversary of Intel's release of the
8080A MPU.  This was the processor for two of the very earliest home
computers, the Altair 8800 and IMSAI 8080.  Precursors of the TRS-80, IBM PC,
and eventually modern PCs.  And arch-rivals in the great paddle-switch vs 
toggle-switch front panel debate. :)

Status: Running well.  Windows are clumsy, I need to make Front Panel the main window, with
text terminal appearing in the sole (so far) auxillary window.  Emulator runs at maximum speed, which is
about 40-50 8080A MHz equivalent under debug build, or 625-650 MHz equivalent under release build
(running on a vanilla non-overclocked AMD 5500 desktop PC).

Loaded and ran IMSAI SCS (self contained system) and used it to create, edit, assemble, and run a simple program.

Next goals (in no particular order, any or all may never be completed):
- Begin assigning a version number and maintaining a release history
- Implement a graphics output device (monochrome? CGA-like? Dazzler? SVGA?)
- UI refinements
- Code cleanup and refactoring
- Tiny Basic (maybe Dr. Wang Palo Alto, maybe some other(s))
- Full basic (if any major/good commercial BASIC interpreters have been made freeware)
- C compiler? Pascal? Some other tiny language? Roll my own? A micro-Python or micro-PHP would be cool.
- program library, copyright-checked
- smart terminal (i.e. ADM-3A, VT-52, or VT-100)
- speed throttling via calculated run timeslice - sleep-timeslice cycle (i.e. to slow to stock 2MHz speed)
- idle CPU pause when waiting for KB input to reduce emulator host load of about 30% under debug build
- Space Invaders game
- Z80
- Host feature access (get/put files, make sounds, access a simple key-value or category-key-value database, access network)
- Port Chromium to 8080A (just kidding, of course)
- look for resources - would like a debugger, and a good Windows-hosted cross-assembler with macros.
- performance enhancement - implement per-accumulator-value lookup table for sign, zero, parity flags to reduce flag overhead.
- debugging enhancements: registers view, memory view, disassembly around PC, run-to-here capability (or real breakpoints would be better).
- fix up SCS to accept mixed case text, implement resequence and autonumber editing aids and a simple help screen.
- CPM/80

![image](https://github.com/mstasak/MFComputer/assets/39843617/fe0ce289-b7da-4ed7-814a-4bbbf9c7bebb)





