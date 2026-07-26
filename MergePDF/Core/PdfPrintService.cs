namespace MergePDF.Core
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    internal class PdfPrintService
    {
        public void Print(IEnumerable<BitmapSource> pages, string documentName)
        {
            var printDialog = new PrintDialog();

            if (printDialog.ShowDialog() != true)
            {
                return;
            }

            var document = CreateFixedDocument(pages, printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

            printDialog.PrintDocument(document.DocumentPaginator, documentName);
        }

        private FixedDocument CreateFixedDocument(IEnumerable<BitmapSource> pages, double printableWidth, double printableHeight)
        {
            var document = new FixedDocument();

            document.DocumentPaginator.PageSize =
                new Size(printableWidth, printableHeight);

            foreach (var bitmap in pages)
            {
                var fixedPage = CreatePage(
                    bitmap,
                    printableWidth,
                    printableHeight);

                var pageContent = new PageContent();

                ((IAddChild)pageContent).AddChild(fixedPage);

                document.Pages.Add(pageContent);
            }

            return document;
        }

        private FixedPage CreatePage(BitmapSource bitmap, double pageWidth, double pageHeight)
        {
            var page = new FixedPage
            {
                Width = pageWidth,
                Height = pageHeight
            };

            var image = new Image
            {
                Source = bitmap,
                Width = pageWidth,
                Height = pageHeight,
                Stretch = Stretch.Uniform
            };

            page.Children.Add(image);

            return page;
        }
    }
}
