//-----------------------------------------------------------------------
// <copyright file="MainWindow.cs" company="Lifeprojects.de">
//     Class: MainWindow
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>05.03.2026 18:21:36</date>
//
// <summary>
// WPF Template mit Minimalfunktionen
// </summary>
//-----------------------------------------------------------------------

namespace System.Windows
{
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class WpfIconHelper
    {
        public static ImageSource CreateIcon(DrawingImage drawingImage, int size = 32, double dpi = 96)
        {
            if (size.In(32, 48, 64) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Der Wert für die Icon Größe muß 32 oder 64 Pixel sein.");
            }

            if (dpi == 96)
            {
                size = 64;
            }
            else if (dpi == 144)
            {
                size = 48;
            }
            else if (dpi == 192)
            {
                size = 64;
            }

            DrawingVisual visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawImage(drawingImage, new Rect(0, 0, size, size));
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap(size, size, dpi, dpi, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze(); // Performance + Thread-Safety

            return bitmap;
        }

        public static void ApplyIcon(Window window, DrawingImage drawingImage, int size = 32)
        {
            window.Icon = CreateIcon(drawingImage, size);
        }
    }

}