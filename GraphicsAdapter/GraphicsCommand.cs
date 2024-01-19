using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KGySoft.Drawing;

namespace GraphicsAdapter;
internal class GraphicsCommand {

    internal enum GCommandCode {
        gcClearScreen = 1,
        gcClearScreenColor = 2,
        gcDrawPixelXY = 10,
        gcDrawPixelXYColor = 11,
        gcDrawPointXY = 12,
        gcDrawPointXYColorStrokeWidthRounded = 13,
        gcDrawLineX1Y1X2Y2 = 20,
        gcDrawLineX1Y1X2Y2ColorStrokeWidthRounded = 21,
        gcDrawLineXYRTheta = 22,
        gcDrawLineXYRThetaColorStrokeWidthRounded = 23,
        gcDrawRectXYWH = 30,
        gcDrawRectXYWHColorStrokeWidthRounded = 31,
        gcFillRectXYWH = 32,
        gcFillRectXYWHColor = 33,
        gcDrawFillRectXYWH = 34, //current pen, brush
        gcDrawFillRectXYWHColorStrokeWidthRounded = 35, //current brush
        gcDrawFillRectXYWHFillColor = 36, //current pen
        gcDrawFillRectXYWHColorStrokeWidthRoundedFillColor = 37, //pen, brush specified

        gcDrawRoundedRectXYWHCornerRadius = 40,
        gcDrawRoundedRectXYWHColorStrokeWidthCornerRadius = 41,
        gcFillRoundedRectXYWHCornerRadius = 42,
        gcFillRoundedRectXYWHColorCornerRadius = 43,
        gcDrawFillRoundedRectXYWHCornerRadius = 44, //current pen, brush
        gcDrawFillRoundedRectXYWHColorStrokeWidthCornerRadius = 45, //current brush
        gcDrawFillRoundedRectXYWHFillColorCornerRadius = 46, //current pen
        gcDrawFillRoundedRectXYWHColorStrokeWidthFillColorCornerRadius = 47, //pen, brush specified

        gcDrawCircleXYR = 50,
        gcDrawCircleXYRColorStrokeWidth = 51,
        gcFillCircleXYR = 52,
        gcFillCircleXYRColor = 53,
        gcDrawFillCircleXYR = 54, //current pen, brush
        gcDrawFillCircleXYRColorStrokeWidth = 55, //current brush, specific pen
        gcDrawFillCircleXYRFillColor = 56, //current pen, specific brush
        gcDrawFillCircleXYRColorStrokeWidthFillColor = 57, //specific pen and brush

        gcDrawEllipseXYWH = 60,
        gcDrawEllipseXYWHColorStrokeWidthRounded = 61,
        gcFillEllipseXYWH = 62,
        gcFillEllipseXYWHColor = 63,
        gcDrawFillEllipseXYWH = 64, //current pen, brush
        gcDrawFillEllipseXYWHColorStrokeWidthRounded = 65, //current brush
        gcDrawFillEllipseXYWHFillColor = 66, //current pen
        gcDrawFillEllipseXYWHColorStrokeWidthRoundedFillColor = 67, //pen, brush specified

        //gcDrawPolygon = 70,
        //gcDrawArc = 80,
        //gcDrawBezier = 90,
        gcSetPenColorStrokeWidthRounded = 100,
        gcSetBrushColor = 110,
        gcSetForegroundColor = 120,
        gcSetBackgroundColor = 130,
        gcSetScreenDimensionsWH = 140,
        gcSetZoom = 150,
        gcScrollRegionXYWHdXdY = 160,
        gcScrollScreendXdY = 170,

        gcDrawText = 200, //nStringText, nStringFont, wPointSize, bStyles (bold,italic,underline,strike,superscript,subscript,invert,color,bgcolor,bclearfirst)
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
                case GCommandCode.gcClearScreenColor: //clear screen
                    if (paramByteCount == 1) {
                        destinationBitmap.Clear(pColor());
                    }
                    break;
                case GCommandCode.gcDrawPixelXY: //set pixel to current pen color
                    if (paramByteCount == 4) {
                        var point = pPoint();
                        var c = GraphicsState.CurrentPen.Color;
                        destinationBitmap.SetPixel(point.X, point.Y, c);
                    }
                    break;
                case GCommandCode.gcDrawPixelXYColor: //set pixel
                    if (paramByteCount == 5) {
                        var point = pPoint();
                        var c = pColor();
                        destinationBitmap.SetPixel(point.X, point.Y, c);
                    }
                    break;
                case GCommandCode.gcDrawLineX1Y1X2Y2:
                    if (paramByteCount == 8) {
                        Point point1 = pPoint();
                        Point point2 = pPoint();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawLine(GraphicsState.CurrentPen, point1, point2);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawLineX1Y1X2Y2ColorStrokeWidthRounded:
                    if (paramByteCount == 12) {
                        Point point1 = pPoint();
                        Point point2 = pPoint();
                        var pen = pPen();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawLine(pen, point1, point2);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    //throw new NotImplementedException();
                    break;
                case GCommandCode.gcDrawLineXYRTheta:
                    if (paramByteCount == 8) {
                        Point point1 = pPoint();
                        var radius = pWord();
                        var theta = pRadians();
                        DrawLineRTheta(destinationBitmap, point1, radius, theta, GraphicsState.CurrentPen);
                    }
                    break;
                case GCommandCode.gcDrawLineXYRThetaColorStrokeWidthRounded:
                    if (paramByteCount == 11) {
                        Point point1 = new(pWord(), pWord());
                        var radius = pWord();
                        var theta = pRadians();
                        var pen = pPen();
                        DrawLineRTheta(destinationBitmap, point1, radius, theta, pen);
                    }
                    //throw new NotImplementedException();
                    break;
                case GCommandCode.gcDrawRectXYWH:
                    if (paramByteCount == 8) {
                        Rectangle rect = pRect();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRectangle(GraphicsState.CurrentPen, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawRectXYWHColorStrokeWidthRounded:
                    if (paramByteCount == 12) {
                        Rectangle rect = pRect();
                        var pen = pPen();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRectangle(pen, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcFillRectXYWH:
                    if (paramByteCount == 8) {
                        Rectangle rect = pRect();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.FillRectangle(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcFillRectXYWHColor:
                    if (paramByteCount == 9) {
                        Rectangle rect = pRect();
                        var brush = pBrush();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.FillRectangle(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillRectXYWH:
                    if (paramByteCount == 8) {
                        Rectangle rect = pRect();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRectangle(GraphicsState.CurrentPen, rect); //note: this draws border OUTSIDE of rectangle coords
                            g?.FillRectangle(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillRectXYWHColorStrokeWidthRounded:
                    if (paramByteCount == 12) {
                        Rectangle rect = pRect();
                        var pen = pPen();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRectangle(pen, rect); //note: this draws border OUTSIDE of rectangle coords
                            g?.FillRectangle(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillRectXYWHFillColor:
                    if (paramByteCount == 9) {
                        Rectangle rect = pRect();
                        var brush = pBrush();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRectangle(GraphicsState.CurrentPen, rect); //note: this draws border OUTSIDE of rectangle coords
                            g?.FillRectangle(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillRectXYWHColorStrokeWidthRoundedFillColor:
                    if (paramByteCount == 13) {
                        var rect = pRect();
                        var pen = pPen();
                        var brush = pBrush();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRectangle(pen, rect); //note: this draws border OUTSIDE of rectangle coords
                            g?.FillRectangle(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcSetPenColorStrokeWidthRounded:
                    if (paramByteCount == 4) {
                        GraphicsState.CurrentPen = pPen();
                    }
                    break;
                case GCommandCode.gcSetBrushColor:
                    if (paramByteCount == 1) {
                        GraphicsState.CurrentBrush = pBrush();
                    }
                    break;
                case GCommandCode.gcDrawPointXY:
                    //draw a point (diameter = pen.strokewidth) (not necessarily a single pixel!)
                    if (paramByteCount == 4) {
                        Point point = pPoint();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            //TODO: verify this draws nothing and draw a rectangle[x,y,1,1];
                            g?.DrawLine(GraphicsState.CurrentPen, point, point); //note: assuming a zero length line draws a point, not nothing
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    //throw new NotImplementedException();
                    break;
                case GCommandCode.gcDrawPointXYColorStrokeWidthRounded:
                    if (paramByteCount == 8) {
                        Point point = new(pWord(), pWord());
                        var color = pColor();
                        var lineWidth = pWord();
                        var rounded = pByte() > 0;
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            //var rFill = r;
                            //rFill.Inflate(-5, -5);
                            //GraphicsState.CurrentPen = new Pen(Color.YellowGreen, 5);
                            //GraphicsState.CurrentPen.Width = 5;
                            //GraphicsState.CurrentPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                            //GraphicsState.CurrentPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                            //GraphicsState.CurrentBrush = Brushes.Red;
                            var pen = new Pen(color, lineWidth);
                            if (rounded) {
                                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                            }
                            var p2 = point;
                            p2.X += 1;
                            p2.Y += 1;
                            g?.DrawLine(pen, point, p2); //note: assuming a zero length line draws a point, not nothing
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    //throw new NotImplementedException();
                    break;
                case GCommandCode.gcDrawRoundedRectXYWHCornerRadius:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawRoundedRectXYWHColorStrokeWidthCornerRadius:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcFillRoundedRectXYWHCornerRadius:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcFillRoundedRectXYWHColorCornerRadius:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillRoundedRectXYWHCornerRadius:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillRoundedRectXYWHColorStrokeWidthCornerRadius:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillRoundedRectXYWHFillColorCornerRadius:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillRoundedRectXYWHColorStrokeWidthFillColorCornerRadius:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawCircleXYR:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawCircleXYRColorStrokeWidth:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcFillCircleXYR:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcFillCircleXYRColor:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillCircleXYR:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillCircleXYRColorStrokeWidth:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillCircleXYRFillColor:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillCircleXYRColorStrokeWidthFillColor:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawEllipseXYWH:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawEllipseXYWHColorStrokeWidthRounded:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcFillEllipseXYWH:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcFillEllipseXYWHColor:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillEllipseXYWH:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillEllipseXYWHColorStrokeWidthRounded:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillEllipseXYWHFillColor:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawFillEllipseXYWHColorStrokeWidthRoundedFillColor:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcSetForegroundColor:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcSetBackgroundColor:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcSetScreenDimensionsWH:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcSetZoom:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcScrollRegionXYWHdXdY:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcScrollScreendXdY:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawText:
                    throw new NotImplementedException(); // break;
                default:
                    break;
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Exception in GraphicsCommand.PerformCommand: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert a word-stored angle to radians
    /// </summary>
    /// <param name="v">signed int16 angle in tenths of a degree (-32768..32767 decidegrees), stored in low word of an int (0..65535) </param>
    /// <returns></returns>
    private double WordToRadians(int v) {
        if (v > 32767) {
            // shift range 32768..65535 to -32768..-1
            v -= 65536;
        }
        var theta = (v + 36000) % 3600 / 10.0; //normalize to 0..360 degrees
        theta = theta * Math.PI / 180.0; //convert to 0..2*PI radians
        return theta;
    }

    private static void DrawLineRTheta(Bitmap destinationBitmap, Point point1, int radius, double theta, Pen pen) {
        var point2 = new Point(x: (int)double.Round(point1.X + radius * Math.Cos(theta)),
                                                       y: (int)double.Round(point1.Y + radius * Math.Sin(theta)));
        Graphics? g = null;
        try {
            g = Graphics.FromImage(destinationBitmap);
            g?.DrawLine(pen, point1, point2);
        } catch (Exception) {
            //throw;
        } finally {
            g?.Dispose();
        }
    }

    internal int pWord() => paramBytes[paramIx++] | paramBytes[paramIx++] << 8;
    internal byte pByte() => paramBytes[paramIx++];
    internal Color pColor() {
        var rawC = pByte();
        return Color.FromArgb(rawC / 36 * 255 / 6, rawC / 6 % 6 * 255 / 6, rawC % 6 * 255 / 6);
    }
    internal Pen pPen() {
        var color = pColor();
        var strokeWidth = pWord();
        var rounded = pByte() > 0;
        var pen = new Pen(color, strokeWidth);
        if (rounded) {
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
        }
        return pen;
    }
    internal Brush pBrush() {
        var color = pColor();
        var brush = new SolidBrush(color);
        return brush;
    }

    internal Point pPoint() {
        return new Point(x: pWord(), y: pWord());
    }

    internal Rectangle pRect() {
        return new(x: pWord(), y: pWord(), width: pWord(), height: pWord());
    }

    internal double pRadians() {
        return WordToRadians(pWord());
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