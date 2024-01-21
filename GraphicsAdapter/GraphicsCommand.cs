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
    private const int pRectSize = 8;
    private const int pPenSize = 4;
    private const int pBrushSize = 1;
    private const int pPointSize = 4;
    private const int pColorSize = 1;
    private const int pRadiusSize = 2;
    private const int pAngleSize = 2;
    private const int pCornerRadiusSize = 2;

    internal void PerformCommand(Bitmap destinationBitmap) {
        try {
            switch ((GCommandCode)gCommand) {
                case GCommandCode.gcClearScreen: //clear screen
                    if (paramByteCount == 0) {
                        destinationBitmap.Clear(GraphicsState.BackgroundColor);
                    }
                    break;
                case GCommandCode.gcClearScreenColor: //clear screen
                    if (paramByteCount == pColorSize) {
                        destinationBitmap.Clear(pColor());
                    }
                    break;
                case GCommandCode.gcDrawPixelXY: //set pixel to current pen color
                    if (paramByteCount == pPointSize) {
                        var point = pPoint();
                        destinationBitmap.SetPixel(point.X, point.Y, GraphicsState.CurrentPen.Color);
                    }
                    break;
                case GCommandCode.gcDrawPixelXYColor: //set pixel
                    if (paramByteCount == pPointSize + pColorSize) {
                        var point = pPoint();
                        destinationBitmap.SetPixel(point.X, point.Y, pColor());
                    }
                    break;
                case GCommandCode.gcDrawLineX1Y1X2Y2:
                    if (paramByteCount == 2 * pPointSize) {
                        var point1 = pPoint();
                        var point2 = pPoint();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawLine(GraphicsState.CurrentPen, point1, point2);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawLineX1Y1X2Y2ColorStrokeWidthRounded:
                    if (paramByteCount ==  2 * pPointSize + pPenSize) {
                        var point1 = pPoint();
                        var point2 = pPoint();
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
                    break;
                case GCommandCode.gcDrawLineXYRTheta:
                    if (paramByteCount == pPointSize + pRadiusSize + pAngleSize) {
                        var point1 = pPoint();
                        var radius = pWord();
                        var theta = pRadians();
                        DrawLineRTheta(destinationBitmap, point1, radius, theta, GraphicsState.CurrentPen);
                    }
                    break;
                case GCommandCode.gcDrawLineXYRThetaColorStrokeWidthRounded:
                    if (paramByteCount == pPointSize + pRadiusSize + pAngleSize + pPenSize) {
                        var point1 = pPoint();
                        var radius = pWord();
                        var theta = pRadians();
                        var pen = pPen();
                        DrawLineRTheta(destinationBitmap, point1, radius, theta, pen);
                    }
                    break;

                case GCommandCode.gcDrawRectXYWH:
                    if (paramByteCount == pRectSize) {
                        var rect = pRect();
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
                    if (paramByteCount == pRectSize + pPenSize) {
                        var rect = pRect();
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
                    if (paramByteCount == pRectSize) {
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
                    if (paramByteCount == pRectSize + pBrushSize) {
                        var rect = pRect();
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
                    if (paramByteCount == pRectSize) {
                        var rect = pRect();
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
                    if (paramByteCount == pRectSize + pPenSize) {
                        var rect = pRect();
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
                    if (paramByteCount == pRectSize + pBrushSize) {
                        var rect = pRect();
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
                    if (paramByteCount == pRectSize + pPenSize + pBrushSize) {
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
                    if (paramByteCount == pPenSize) {
                        GraphicsState.CurrentPen = pPen();
                    }
                    break;
                case GCommandCode.gcSetBrushColor:
                    if (paramByteCount == pBrushSize) {
                        GraphicsState.CurrentBrush = pBrush();
                    }
                    break;
                case GCommandCode.gcDrawPointXY:
                    //draw a point (diameter = pen.strokewidth) (not necessarily a single pixel!)
                    if (paramByteCount == pPointSize) {
                        var point = pPoint();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            //g?.DrawLine(GraphicsState.CurrentPen, point, point); //note: this assumes a zero length line draws a point, not nothing
                            var rect = new Rectangle(point, new Size(1, 1));
                            g?.DrawRectangle(GraphicsState.CurrentPen, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawPointXYColorStrokeWidthRounded:
                    if (paramByteCount == pPointSize + pPenSize) {
                        var point = pPoint();
                        var pen = pPen();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            var rect = new Rectangle(point, new Size(1, 1));
                            g?.DrawRectangle(pen, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;

                case GCommandCode.gcDrawRoundedRectXYWHCornerRadius:
                    if (paramByteCount == pRectSize + pCornerRadiusSize) {
                        var rect = pRect();
                        var cornerRadius = pWord();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRoundedRectangle(GraphicsState.CurrentPen, rect, cornerRadius);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawRoundedRectXYWHColorStrokeWidthCornerRadius:
                    if (paramByteCount == pRectSize + pPenSize + pCornerRadiusSize) {
                        var rect = pRect();
                        var pen = pPen();
                        var cornerRadius = pWord();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRoundedRectangle(pen, rect, cornerRadius);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcFillRoundedRectXYWHCornerRadius:
                    if (paramByteCount == pRectSize + pCornerRadiusSize) {
                        var rect = pRect();
                        var cornerRadius = pWord();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.FillRoundedRectangle(GraphicsState.CurrentBrush, rect, cornerRadius);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcFillRoundedRectXYWHColorCornerRadius:
                    if (paramByteCount == pRectSize + pBrushSize + pCornerRadiusSize) {
                        var rect = pRect();
                        var brush = pBrush();
                        var cornerRadius = pWord();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.FillRoundedRectangle(brush, rect, cornerRadius);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillRoundedRectXYWHCornerRadius:
                    if (paramByteCount == pRectSize + pCornerRadiusSize) {
                        var rect = pRect();
                        var cornerRadius = pWord();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRoundedRectangle(GraphicsState.CurrentPen, rect, cornerRadius); //note: this draws border OUTSIDE of rectangle coords
                            g?.FillRoundedRectangle(GraphicsState.CurrentBrush, rect, cornerRadius);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillRoundedRectXYWHColorStrokeWidthCornerRadius:
                    if (paramByteCount == pRectSize + pPenSize + pCornerRadiusSize) {
                        var rect = pRect();
                        var pen = pPen();
                        var cornerRadius = pWord();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRoundedRectangle(pen, rect, cornerRadius); //note: this draws border OUTSIDE of rectangle coords
                            g?.FillRoundedRectangle(GraphicsState.CurrentBrush, rect, cornerRadius);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillRoundedRectXYWHFillColorCornerRadius:
                    if (paramByteCount == pRectSize + pBrushSize + pCornerRadiusSize) {
                        Rectangle rect = pRect();
                        var brush = pBrush();
                        var cornerRadius = pWord();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRoundedRectangle(GraphicsState.CurrentPen, rect, cornerRadius); //note: this draws border OUTSIDE of rectangle coords
                            g?.FillRoundedRectangle(brush, rect, cornerRadius);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillRoundedRectXYWHColorStrokeWidthFillColorCornerRadius:
                    if (paramByteCount == pRectSize + pPenSize + pBrushSize + pCornerRadiusSize) {
                        var rect = pRect();
                        var pen = pPen();
                        var brush = pBrush();
                        var cornerRadius = pWord();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g?.DrawRoundedRectangle(pen, rect, cornerRadius); //note: this draws border OUTSIDE of rectangle coords
                            g?.FillRoundedRectangle(brush, rect, cornerRadius);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;

                case GCommandCode.gcDrawCircleXYR:
                    if (paramByteCount == pPointSize + pRadiusSize) {
                        var point1 = pPoint();
                        var radius = pWord();
                        Rectangle rect = new() {
                            X = point1.X - radius,
                            Y = point1.Y - radius,
                            Width = 2 * radius,
                            Height = 2 * radius
                        };
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(GraphicsState.CurrentPen, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawCircleXYRColorStrokeWidth:
                    if (paramByteCount == pPointSize + pRadiusSize + pPenSize) {
                        var point1 = pPoint();
                        var radius = pWord();
                        var pen = pPen();
                        Rectangle rect = new() {
                            X = point1.X - radius,
                            Y = point1.Y - radius,
                            Width = 2 * radius,
                            Height = 2 * radius
                        };
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(pen, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcFillCircleXYR:
                    if (paramByteCount == pPointSize + pRadiusSize) {
                        Point point1 = pPoint();
                        var radius = pWord();
                        Rectangle rect = new() {
                            X = point1.X - radius,
                            Y = point1.Y - radius,
                            Width = 2 * radius,
                            Height = 2 * radius
                        };
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.FillEllipse(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcFillCircleXYRColor:
                    if (paramByteCount == pPointSize + pRadiusSize + pBrushSize) {
                        var point1 = pPoint();
                        var radius = pWord();
                        var brush = pBrush();
                        Rectangle rect = new() {
                            X = point1.X - radius,
                            Y = point1.Y - radius,
                            Width = 2 * radius,
                            Height = 2 * radius
                        };
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.FillEllipse(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillCircleXYR:
                    if (paramByteCount == pPointSize + pRadiusSize) {
                        var point1 = pPoint();
                        var radius = pWord();
                        Rectangle rect = new() {
                            X = point1.X - radius,
                            Y = point1.Y - radius,
                            Width = 2 * radius,
                            Height = 2 * radius
                        };
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(GraphicsState.CurrentPen, rect);
                            g.FillEllipse(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillCircleXYRColorStrokeWidth:
                    if (paramByteCount == pPointSize + pRadiusSize + pPenSize) {
                        var point1 = pPoint();
                        var radius = pWord();
                        var pen = pPen();
                        Rectangle rect = new() {
                            X = point1.X - radius,
                            Y = point1.Y - radius,
                            Width = 2 * radius,
                            Height = 2 * radius
                        };
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(pen, rect);
                            g.FillEllipse(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillCircleXYRFillColor:
                    if (paramByteCount == pPointSize + pRadiusSize + pBrushSize) {
                        Point point1 = pPoint();
                        var radius = pWord();
                        var brush = pBrush();
                        Rectangle rect = new();
                        rect.X = point1.X - radius;
                        rect.Y = point1.Y - radius;
                        rect.Width = 2 * radius;
                        rect.Height = 2 * radius;
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(GraphicsState.CurrentPen, rect);
                            g.FillEllipse(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillCircleXYRColorStrokeWidthFillColor:
                    if (paramByteCount == pPointSize + pRadiusSize + pPenSize + pBrushSize) {
                        var point1 = pPoint();
                        var radius = pWord();
                        var pen = pPen();
                        var brush = pBrush();
                        Rectangle rect = new() {
                            X = point1.X - radius,
                            Y = point1.Y - radius,
                            Width = 2 * radius,
                            Height = 2 * radius
                        };
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(pen, rect);
                            g.FillEllipse(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                
                case GCommandCode.gcDrawEllipseXYWH:
                    if (paramByteCount == pRectSize) {
                        var rect = pRect();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(GraphicsState.CurrentPen, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawEllipseXYWHColorStrokeWidthRounded:
                    if (paramByteCount == pRectSize + pPenSize) {
                        var rect = pRect();
                        var pen = pPen();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(pen, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcFillEllipseXYWH:
                    if (paramByteCount == pRectSize) {
                        var rect = pRect();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.FillEllipse(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcFillEllipseXYWHColor:
                    if (paramByteCount == pRectSize + pBrushSize) {
                        var rect = pRect();
                        var brush = pBrush();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(GraphicsState.CurrentPen, rect);
                            g.FillEllipse(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillEllipseXYWH:
                    if (paramByteCount == pRectSize) {
                        var rect = pRect();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(GraphicsState.CurrentPen, rect);
                            g.FillEllipse(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillEllipseXYWHColorStrokeWidthRounded:
                    if (paramByteCount == pRectSize + pPenSize) {
                        var rect = pRect();
                        var pen = pPen();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(pen, rect);
                            g.FillEllipse(GraphicsState.CurrentBrush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillEllipseXYWHFillColor:
                    if (paramByteCount == pRectSize + pBrushSize) {
                        var rect = pRect();
                        var brush = pBrush();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(GraphicsState.CurrentPen, rect);
                            g.FillEllipse(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                case GCommandCode.gcDrawFillEllipseXYWHColorStrokeWidthRoundedFillColor:
                    if (paramByteCount == pRectSize + pPenSize + pBrushSize) {
                        var rect = pRect();
                        var pen = pPen();
                        var brush = pBrush();
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawEllipse(pen, rect);
                            g.FillEllipse(brush, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;

                case GCommandCode.gcSetForegroundColor:
                    if (paramByteCount == pColorSize) {
                        GraphicsState.ForegroundColor = pColor();
                    }
                    break;
                case GCommandCode.gcSetBackgroundColor:
                    if (paramByteCount == pColorSize) {
                        GraphicsState.BackgroundColor = pColor();
                    }
                    break;
                case GCommandCode.gcSetScreenDimensionsWH:
                    if (paramByteCount == pPointSize) {
                        var DisplaySize = (Size)pPoint();
                        GraphicsState.DisplaySize = DisplaySize;
                    }
                    break;
                case GCommandCode.gcSetZoom:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcScrollRegionXYWHdXdY:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcScrollScreendXdY:
                    throw new NotImplementedException(); // break;
                case GCommandCode.gcDrawText:
                    //draw string using placement+wrap+clip parameters.  Use current brush+font+attributes.
                    var minBytes = pRectSize + 1 + 1 + 2;
                    if (paramByteCount >= minBytes) {
                        var rect = pRect();
                        var wrapped = pByte() > 0;
                        var clipped = pByte() > 0;
                        var str = pString();
                        var font = GraphicsState.CurrentFont;
                        Graphics? g = null;
                        try {
                            g = Graphics.FromImage(destinationBitmap);
                            g.DrawString(str, font, GraphicsState.CurrentBrush, rect);
                            //g.DrawString("Hello", SystemFonts.DefaultFont, Brushes.Red, rect);
                        } catch (Exception) {
                            //throw;
                        } finally {
                            g?.Dispose();
                        }
                    }
                    break;
                    //throw new NotImplementedException(); // break;
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
        var rawC = (int)pByte();
        //return Color.FromArgb(rawC / 36 * 255 / 6, rawC / 6 % 6 * 255 / 6, rawC % 6 * 255 / 6);
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

    internal string pString() {
        var rslt = "";
        var strLen = pWord();
        for (var i = 0; i < strLen; i++) {
            var c = (char)pByte();
            rslt += c;
        }
        return rslt;
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