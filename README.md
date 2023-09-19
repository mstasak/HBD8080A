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

Status: running well from the front panel.  Speed equivalent to approximately
40MHz 8080A (on a Ryzen 5500 host).  Host load about 1.0 logical core.

*Update: tested speed for a release build, got 638-645 MHz!  This is a small program though, 
mostly mov, dcr, jnz, jmp instructions.  Results should vary for a different mix of instructions.

Next: implement a dumb terminal (teletype-ish).  Make a monitor program, or try to run
a significant program, like IMSAI SCS or Palo Alto Tiny Basic.

![image](https://github.com/mstasak/MFComputer/assets/39843617/fe0ce289-b7da-4ed7-814a-4bbbf9c7bebb)





