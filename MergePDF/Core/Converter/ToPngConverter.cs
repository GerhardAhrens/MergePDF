//-----------------------------------------------------------------------
// <copyright file="ToPngConverter.cs" company="Lifeprojects.de">
//     Class: ToPngConverter
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>2026 - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>05.03.2026 18:21:36</date>
//
// <summary>
// Die Klasse ToPngConverter konvertiert den XAML Code von DrawingImage in ein PNG-Bild.
// </summary>
//-----------------------------------------------------------------------

namespace MergePDF.Core
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    internal sealed class ToPngConverter
    {
        private const double DPI  = 96.0;

        public static void Convert(DrawingImage drawingImage, int width, int height, string outputPath)
        {
            RenderTargetBitmap bitmap = RenderDrawingImage(drawingImage, width, height);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = System.IO.File.Create(outputPath))
            {
                encoder.Save(stream);
            }
        }

        private static RenderTargetBitmap RenderDrawingImage(DrawingImage drawingImage, int width, int height)
        {
            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(drawingImage, new Rect(0, 0, width, height));
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, DPI, DPI, PixelFormats.Pbgra32);

            bitmap.Render(visual);

            return bitmap;
        }

        public static BitmapImage DrawingImageToBitmapImage(DrawingImage drawingImage, int width, int height)
        {
            // 1. DrawingVisual erstellen (erbt von Visual und kann gerendert werden)
            DrawingVisual drawingVisual = new DrawingVisual();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // Zeichnet den Inhalt des DrawingImages (das Drawing-Objekt) in den Context
                drawingContext.DrawDrawing(drawingImage.Drawing);
            }

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(width, height, DPI, DPI, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public static void SaveImageSourceToPng(ImageSource imageSource, string filePath)
        {
            if (imageSource == null)
                throw new ArgumentNullException(nameof(imageSource), "Die ImageSource darf nicht null sein.");

            BitmapSource bitmapSource;

            // Fall 1: Das Bild ist bereits eine BitmapSource (z.B. BitmapImage oder RenderTargetBitmap)
            if (imageSource is BitmapSource source)
            {
                bitmapSource = source;
            }
            // Fall 2: Es ist ein vektorbasiertes Bild (z.B. DrawingImage) -> Muss zuerst gerastert werden
            else if (imageSource is DrawingImage drawingImage)
            {
                // Nutzen Sie die Breite/Höhe des Vektors oder feste Standardwerte
                int width = (int)Math.Ceiling(drawingImage.Width);
                int height = (int)Math.Ceiling(drawingImage.Height);

                // Fallback, falls keine Dimensionen definiert sind
                if (width <= 0) width = 500;
                if (height <= 0) height = 500;

                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawDrawing(drawingImage.Drawing);
                }

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    width, height, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(drawingVisual);

                bitmapSource = renderBitmap;
            }
            else
            {
                throw new NotSupportedException($"Der Typ {imageSource.GetType().Name} wird nicht unterstützt.");
            }

            // PNG-Encoder erstellen und Datei speichern
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
    }
}
