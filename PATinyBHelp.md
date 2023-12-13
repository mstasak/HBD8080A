# Help with Palo Alto Tiny Basic (PATB)

## Running PATB

Start the emulator app.  Tap the Text Terminal toolbar button.  Tap the Palo Alto Tiny Basic toolbar button.  OK> should be displayed. this is a prompt from which you can enter program code, execute immediate statements, or issue commands.

You will probably want to keep CAPS LOCK on, as PATB does not understand lower or mixed case commands, statements, or variable names.

## Commands:
- NEW (delete current program)
- LIST (show the program)
- RUN (run the program)
- 100 LET A = 10 (add a line)
- 250 (delete line 250)

## Statements:
LET
INPUT
PRINT
IF
FOR
GOTO
GOSUB
REM
BYE

## Line editing:
Enter each line with a linenumber.  Suggest using multiples of 10, so insertions can be made easily.  Enter a line number alone to delete a line.

## Limitations and quirks
- 16 bit integer math only (-32768-32767)
- 26 variables, A-Z, global in scope
- No strings except literals in INPUT and PRINT statements
- lowercase letters not understood
- @(n) is a single dimension array sized to available RAM
- The not equals operator is #, not <> (because every byte counts!)
- 

## Additional info
See the 4 help pages captured from Dr. Dobbs Journal, in the project source under HBD8080A\Assets\PATBHelp*.jpg

## Sample session
```
SHERRY BROTHERS TINY BASIC VER. 3.1

OK
>10 for
>10

OK
>100 FOR I=1 TO 5
>110 PRINT I,
>120 NEXT I
>130 STOP
>LIST
 100 FOR I=1 TO 5
 110 PRINT I,
 120 NEXT I
 130 STOP

OK
>RUN
     1     2     3     4     5
OK
>
```