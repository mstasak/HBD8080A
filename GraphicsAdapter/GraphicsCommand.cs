using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KGySoft.Drawing;

namespace GraphicsAdapter;
internal class GraphicsCommand {

    internal enum GCommandCode {
        gcClearScreen = 1,
        //gcClearScreenColor,
        //gcSetPixelXYColor,
        //gcDrawPixelXY,
        gcDrawPixelXYColor = 10,
        gcDrawPixelXY = 11,
        //gcDrawPointXY,
        //gcDrawPointXYColorStrokeWidthRounded,
        gcDrawLineX1Y1X2Y2 = 20,
        //gcDrawLineXYWHColorStrokeWidthRounded,
        gcDrawLineXYRTheta,
        //gcDrawLineXYRThetaColorStrokeWidthRounded,
        gcDrawRectXYWH,
        gcFillRectXYWH,
        //gcDrawRectXYWHColorStrokeWidthRounded,
        gcDrawFillRectXYWH, //current pen, brush
        //gcDrawFillRectXYWHColorStrokeWidthRounded, //current brush
        //gcDrawFillRectXYWHFillColor, //current pen
        //gcDrawFillRectXYWHColorStrokeWidthRoundedFillColor, //pen, brush specified
        gcDrawRoundedRectXYWHCornerRadius,
        gcDrawFillRoundedRectXYWHCornerRadius,
        gcFillRoundedRectXYWHCornerRadius,
        //gcDrawRoundedRectXYWHColorStrokeWidthRounded,
        //gcDrawFillRoundedRectXYWH, //current pen, brush
        //gcDrawFillRoundedRectXYWHColorStrokeWidthRounded, //current brush
        //gcDrawFillRoundedRectXYWHFillColor, //current pen
        //gcDrawFillRoundedRectXYWHColorStrokeWidthRoundedFillColor, //pen, brush specified

        gcDrawText, //nStringText, nStringFont, wPointSize, bStyles (bold,italic,underline,strike,superscript,subscript,invert,color,bgcolor,bclearfirst)
        /*

        -graphical input/output-
        nn       4               X,Y                C         Read pixel color at X,Y; sends back one byte

                                                              Drawfilledx (ellipse/circle, polygon, path, )
                                                              Drawtext (string, size, rectangle, clip, wrap)
                                                              Putpixels (rect, data)
                                                              Getpixels (rect)
                                                              Getwidth
                                                              Getheight
                                                              Define pen (pen# 0-F, color, width)
                                                              Define brush (brush# 0-F, color)
                                                              Select pen (0-F)
                                                              Select brush (0-F)
                                                              Beginbatch
                                                              Endbatch

        -terminal-style text output-
          : consoleprinttext (scrolls screen if crlf encountered on (or near?) bottom row)
          : setcursorpos
          : home
          : getcursorpos
          : setcursorshape
          : getcursorshape
          : hidecursor
          : showcursor
          : getrows
          : getcolumns

        -configuration
          : querysetting (key)
          : setsetting (key, value)
          : ping
          : setdisplayparameters w,h,colors
          : getrowspacing
          : getcolspacing
          : getcharpixels (monospaced char cell dimensions for terminal font)
          : set text rendering font to a byte enumerated code (1=help/arial, 2=times/roman, 3=courier, 4=script, 5=comic sans, 6=?, 7...)
         * */
    }

    internal byte gCommand = 0;
    internal int paramByteCount = 0;
    internal byte[]? paramBytes;
    internal int paramIx = 0;

    internal void PerformCommand(Bitmap destinationBitmap) {
        try {
            switch ((GCommandCode)gCommand) {
                case GCommandCode.gcClearScreen: //clear screen
                    if (paramByteCount == 0) {
                        destinationBitmap.Clear(GraphicsState.BackgroundColor);
                    }
                    break;
                //case GCommandCode.gcClearScreenColor: //clear screen
                //    destinationBitmap.Clear(pColor());
                //    break;
                case GCommandCode.gcDrawPixelXY: //set pixel to current pen color
                    if (paramByteCount == 4) {
                        var x = pWord();
                        var y = pWord();
                        var c = GraphicsState.CurrentPen.Color;
                        destinationBitmap.SetPixel(x, y, c);
                    }
                    break;
                case GCommandCode.gcDrawPixelXYColor: //set pixel
                    if (paramByteCount == 5) {
                        var x = pWord();
                        var y = pWord();
                        var c = pColor();
                        destinationBitmap.SetPixel(x, y, c);
                    }
                    break;
                case GCommandCode.gcDrawLineX1Y1X2Y2:
                    if (paramByteCount == 8) {
                        Point p1 = new(pWord(), pWord());
                        Point p2 = new(pWord(), pWord());
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawLine(GraphicsState.CurrentPen, p1, p2);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                default:
                    break;
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Exception in GraphicsCommand.PerformCommand: {ex.Message}");
        }
    }

    internal int pWord() => paramBytes[paramIx++] | paramBytes[paramIx++] << 8;
    internal byte pByte() => paramBytes[paramIx++];
    internal Color pColor() {
        var rawC = pByte();
        return Color.FromArgb(rawC / 36 * 255 / 6, rawC / 6 % 6 * 255 / 6, rawC % 6 * 255 / 6);
    }

    internal static Color ToColor(byte remoteColor) {
        return Color.FromArgb(remoteColor / 36 * 255 / 6, remoteColor / 6 % 6 * 255 / 6, remoteColor % 6 * 255 / 6);
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