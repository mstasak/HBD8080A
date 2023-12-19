using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBD8080A.Services;


public sealed class DisplayAdapterService {
    private static readonly Lazy<DisplayAdapterService> lazy =
        new(() => new DisplayAdapterService());

    public static DisplayAdapterService Instance => lazy.Value;

    private DisplayAdapterService() {
    }


    /*
     * Display a window which functions as a graphical display adapter.  This should display
     * graphics only (no text mode, text must be drawn).  Basic functionality to include:
     * - set mode (Wpixels, Hpixels, color depth)
     * - clear screen
     * - set bkg color
     * - set border color
     * - set fill color
     * - draw dot
     * - draw circle/ellipse
     * - draw rectangle/square
     * - draw rounded rect
     * - mouse cursor?
     * - draw polygon
     * - draw path
     * - composition layers?
     * - get color of pixel
     * - draw text (font, style, size, string)
     * - copy rectangle
     * - draw bitmap
     * - draw line
     * - draw bezier
     * - draw circle slice (i.e. pie chart segment)
     * 
     * Obviously pixel painting requires raw pixel byte access, so we can't use WINUI 3 Shape subclasses
     * (working with millions of 1x1 shapes would be impractical).
     * Unfortunately, WINUI 3/WinAppSDK is conspicuously lacking in a drawable image capability.
     * There is a writeable bitmap class, but setting bytes in its storage seems to have no effect.  May
     * work on that, or experiment with NUGET extensions (WriteableImage extensions? KyGSoft.Drawing?).  One can't even use
     * GDI+ in a WINUI app.  There is a Win2D package, but I'm having trouble getting to work post .net 5.
     * Nothing is easy, documentation is cryptic, and discussion tends to go along the lines of "Q: how can
     * I use X? A: Don't use X, try a different approach altogether" :/
     */
}
