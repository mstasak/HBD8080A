using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAdapter;
internal class GraphicsState {

    internal static Color ForegroundColor = Color.White;
    internal static Color BackgroundColor = Color.Black;
    internal static Color TextColor = Color.White;
    //internal Color TextBGColor = Color.Black;
    internal static Pen CurrentPen = Pens.White;
    internal static Brush CurrentBrush = Brushes.White;
    internal static Size DisplaySize;
    internal static Font CurrentFont = new(FontFamily.GenericMonospace, 12.0f, FontStyle.Regular);


    //internal static int PixelWidth; //future: with integral zoom in
    //internal static int PixelHeight;
    //internal static byte CurrentDrawColor;
    //internal static byte CurrentFillColor; //need bkgcolor too?
    //internal static int CurrentStrokeWidth;
    //private static byte rawBackgroundColor = 0; //used by ClearScreen() and Pan/Scroll operations

    //protected static Color backgroundColor = Color.Black;
    //internal static Color BackgroundColor => backgroundColor;

    //internal static byte RawBackgroundColor {
    //    get => rawBackgroundColor;
    //    set {
    //        rawBackgroundColor = value;
    //        backgroundColor = GraphicsCommand.ToColor(value);
    //    }
    //}
}
