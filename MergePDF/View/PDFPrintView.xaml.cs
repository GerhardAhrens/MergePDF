//-----------------------------------------------------------------------
// <copyright file="PDFPrintView.cs" company="Lifeprojects.de">
//     Class: PDFPrintView
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>GERHARD-G6\gerha - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>04.07.2026</date>
//
// <summary>
// Dialog zum zusammenfügen von PDF-Dateien.
// </summary>
//-----------------------------------------------------------------------

namespace MergePDF.View
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using MergePDF.Core;

    using Microsoft.Win32;

    using PDFiumCore;

    using PdfSharpCore.Pdf;
    using PdfSharpCore.Pdf.IO;

    using static System.Windows.Forms.VisualStyles.VisualStyleElement;

    /// <summary>
    /// Interaktionslogik für PDFPrintView.xaml
    /// </summary>
    public partial class PDFPrintView : UserControlBase
    {
        private FpdfDocumentT _document;
        private int _currentPage;
        private Point _dragStartPoint;

        public PDFPrintView(ChangeViewEventArgs args) : base(typeof(PDFMergeView))

        {
            this.InitializeComponent();

            fpdfview.FPDF_InitLibrary();

            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            this.CurrentCtorArgs = args;

            this.GoBackCommand = new CommandBase(commandParam => this.OnGoBack(commandParam), () => true);
            this.OpenFolderCommand = new CommandBase(commandParam => this.OnOpenFolder(commandParam), () => true);
            this.PrintPDFCommand = new CommandBase(commandParam => this.PDFPrint(commandParam), () => true);
            this.PlusPageCommand = new CommandBase(commandParam => this.OnPlusMinusPage(commandParam), () => true);
            this.MinusPageCommand = new CommandBase(commandParam => this.OnPlusMinusPage(commandParam), () => true);
            this.ResetCommand = new CommandBase(commandParam => this.OnReset(commandParam), () => true);

            this.DataContext = this;
        }

        #region Properties
        public CommandBase GoBackCommand { get; private set; }
        public CommandBase OpenFolderCommand { get; private set; }
        public CommandBase PlusPageCommand { get; private set; }
        public CommandBase MinusPageCommand { get; private set; }
        public CommandBase PrintPDFCommand { get; private set; }
        public CommandBase ResetCommand { get; private set; }

        public ObservableCollection<PDFFileItem> PDFFilesSource
        {
            get => base.GetValue<ObservableCollection<PDFFileItem>>();
            set => base.SetValue(value);
        }

        public PDFFileItem SelectedPdfFile
        {
            get => base.GetValue<PDFFileItem>();
            set => base.SetValue(value, this.SelectedPdfFileHandler);
        }

        public string DragDropTooltipText
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string FileSizeTooltip
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string PDFPageInfo
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public bool PrintAllPages
        {
            get => base.GetValue<bool>();
            set => base.SetValue(value, this.BoolHandler);
        }

        public string SinglePages
        {
            get => base.GetValue<string>();
            set => base.SetValue(value, this.StringHandler);
        }

        public string ExtractRangePages
        {
            get => base.GetValue<string>();
            set => base.SetValue(value, this.StringHandler);
        }

        private PrintVariante PrintPageVarinate { get; set; }

        private ChangeViewEventArgs CurrentCtorArgs { get; set; }

        private MessageBase Message { get; } = new MessageBase();
        #endregion Properties

        #region Windows Events

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.EventAgg.IsSubscription<StatusEvent>() == true)
            {
                await App.EventAgg.PublishAsync(new StatusEvent("Bereit"));
            }
        }
        #endregion Windows Events

        #region Command Events
        private async void OnGoBack(object commandParam)
        {
            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.GoBack)
                {
                    // 4. Bibliothek sauber schließen (am Ende des Programms)
                    fpdfview.FPDF_DestroyLibrary();

                    ChangeViewEventArgs args = new();
                    args.MenuButton = this.CurrentCtorArgs.FromPage;
                    args.FromPage = this.CurrentCtorArgs.MenuButton;
                    if (App.EventAgg.IsSubscription<ChangeViewEventArgs>() == true)
                    {
                        await App.EventAgg.PublishAsync(args);
                    }
                }
            }
        }
        private async void OnOpenFolder(object commandParam)
        {
            OpenFolderDialog dlg = new OpenFolderDialog();
            dlg.Title = "Ordner auswählen";
            dlg.AddToRecent = true;
            if (dlg.ShowDialog() == true)
            {
                string selectedFolderPath = dlg.FolderName;
                this.LoadFileToListbox(selectedFolderPath);
            }
        }

        private void OnPlusMinusPage(object commandParam)
        {
            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.PlusPage)
                {
                    if (this._document != null && this._currentPage < fpdfview.FPDF_GetPageCount(this._document) - 1)
                    {
                        this._currentPage++;
                        this.RenderPage();
                    }
                }
                else if (button == CommandButtons.MinusPage)
                {
                    if (this._document != null && this._currentPage > 0)
                    {
                        this._currentPage--;
                        this.RenderPage();
                    }
                }

                this.PDFPageInfo = $"{this._currentPage + 1}/{fpdfview.FPDF_GetPageCount(this._document)}";
            }
        }

        private void PDFPrint(object commandParam)
        {
            if (this.PDFFilesSource.Count(f => f.IsSelectedItem == true) > 1)
            {
                this.Message.Hinweis("PDF Drucken", "Bitte wählen nur eine PDF-Datei zum drucken aus.");
                return;
            }

            if (this.PDFFilesSource.Count(f => f.IsSelectedItem == true) == 0)
            {
                this.Message.Hinweis("PDF Drucken", "Bitte wählen Sie mindestens eine PDF-Datei aus.");
                return;
            }

            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.PDFPrint)
                {
                    if (this.PrintPageVarinate == PrintVariante.AllPages)
                    {
                        this.PrintAllPagesHandler();
                    }
                    else if (this.PrintPageVarinate == PrintVariante.SinglePages)
                    {
                        string splitRange = this.ParseTextBoxInput();
                        if (string.IsNullOrEmpty(splitRange) == false)
                        {
                            this.PrintSelectedPage(splitRange);
                        }
                    }
                    else if (this.PrintPageVarinate == PrintVariante.RangePages)
                    {
                        string splitRange = this.ParseTextBoxInput();
                        if (string.IsNullOrEmpty(splitRange) == false)
                        {
                            this.ExtractPages(splitRange);
                        }
                    }
                }
            }
        }

        private void OnReset(object commandParam)
        {
            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.ResetInput)
                {
                    this.PrintPageVarinate = PrintVariante.None;
                    this.PrintAllPages = false;
                    this.SinglePages = string.Empty;
                    this.ExtractRangePages = string.Empty;
                }
            }
        }

        #endregion Command Events

        #region Laden der PDF Dateien zum Rendern
        private async void LoadFileToListbox(string folderPath)
        {
            PDFFileItem fitem;
            List<PDFFileItem> files = new();
            IEnumerable<string> filesFolder = Directory.EnumerateFiles(folderPath, "*.pdf", SearchOption.AllDirectories);
            if (filesFolder != null && filesFolder.Any() == true)
            {
                if (App.EventAgg.IsSubscription<StatusEvent>() == true)
                {
                    await App.EventAgg.PublishAsync(new StatusEvent($"{folderPath}", $"{folderPath}"));
                    await App.EventAgg.PublishAsync(new StatusEvent("Verzeichnis wird gelesen"));
                }

                int order = 1;
                foreach (string file in filesFolder)
                {
                    fitem = new PDFFileItem();
                    fitem.Fullname = file;
                    fitem.Filename = Path.GetFileName(file);
                    fitem.Order = order++;
                    fitem.IsSelectedItem = false;
                    fitem.FileSize = $"{((double)new FileInfo(file).Length) / (1024 * 1024):F2} MB";
                    files.Add(fitem);
                }

                this.PDFFilesSource = new ObservableCollection<PDFFileItem>(files.OrderBy(f => f.Order));

                if (App.EventAgg.IsSubscription<StatusEvent>() == true)
                {
                    await App.EventAgg.PublishAsync(new StatusEvent("Bereit"));
                }

            }
        }

        private void SelectedPdfFileHandler(PDFFileItem item, string arg2)
        {
            this.LoadPdf(item.Fullname);
        }

        private ListBoxItem GetItemUnderMouse(DragEventArgs e)
        {
            DependencyObject obj = this.ListBoxFiles.InputHitTest(e.GetPosition(this.ListBoxFiles)) as DependencyObject;

            while (obj != null && obj is not ListBoxItem)
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            return obj as ListBoxItem;
        }

        private void ListBoxFiles_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void ListBoxFiles_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Point mousePos = e.GetPosition(null);
            Vector diff = _dragStartPoint - mousePos;

            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            ListBox listBox = sender as ListBox;

            if (listBox.SelectedItem == null)
                return;


            this.DragDropTooltipText = ((PDFFileItem)listBox.SelectedItem).Filename;
            this.FileSizeTooltip = ((PDFFileItem)listBox.SelectedItem).FileSize;
            this.DragDropPopup.IsOpen = true;

            DragDrop.DoDragDrop(listBox, listBox.SelectedItem, DragDropEffects.Move);
        }

        private void ListBoxFiles_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;

            Point p = e.GetPosition(this);
            this.DragDropPopup.HorizontalOffset = p.X + 15;
            this.DragDropPopup.VerticalOffset = p.Y + 15;

            /* Positionslinie */
            var item = GetItemUnderMouse(e);

            if (item == null)
            {
                this.InsertLine.Visibility = Visibility.Collapsed;
                this.LeftArrow.Visibility = Visibility.Collapsed;
                this.RightArrow.Visibility = Visibility.Collapsed;
                return;
            }

            bool insertAbove = p.Y < item.ActualHeight / 2;

            Point location = item.TranslatePoint(new Point(0, 0), this.DrogDropLineOverlay);

            double y = insertAbove ? location.Y : location.Y + item.ActualHeight;

            this.InsertLine.X1 = 0;
            this.InsertLine.X2 = this.DrogDropLineOverlay.ActualWidth;

            this.InsertLine.Y1 = y;
            this.InsertLine.Y2 = y;

            LeftArrow.Points = new PointCollection()
            {
                new Point(2, y),
                new Point(10, y - 6),
                new Point(10, y + 6)
            };

            double x = DrogDropLineOverlay.ActualWidth - 2;

            RightArrow.Points = new PointCollection()
            {
                new Point(x, y),
                new Point(x - 8, y - 6),
                new Point(x - 8, y + 6)
            };

            this.InsertLine.Visibility = Visibility.Visible;
            this.LeftArrow.Visibility = Visibility.Visible;
            this.RightArrow.Visibility = Visibility.Visible;
            e.Handled = true;
        }

        private void ListBoxFiles_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(PDFFileItem)))
                return;

            var droppedData = e.Data.GetData(typeof(PDFFileItem));

            ListBox listBox = sender as ListBox;

            var target = ((FrameworkElement)e.OriginalSource).DataContext;

            if (target == null || droppedData == target)
            {
                return;
            }

            int oldIndex = this.PDFFilesSource.IndexOf((PDFFileItem)droppedData);

            int newIndex;

            if (target is not PDFFileItem)
            {
                newIndex = this.PDFFilesSource.Count - 1;
            }
            else
            {
                newIndex = this.PDFFilesSource.IndexOf((PDFFileItem)target);
            }

            if (oldIndex < 0 || newIndex < 0)
            {
                return;
            }

            this.DragDropPopup.IsOpen = false;
            this.InsertLine.Visibility = Visibility.Collapsed;
            this.LeftArrow.Visibility = Visibility.Collapsed;
            this.RightArrow.Visibility = Visibility.Collapsed;
            this.PDFFilesSource.Move(oldIndex, newIndex);
        }

        private void ListBoxFiles_DragLeave(object sender, DragEventArgs e)
        {
            this.InsertLine.Visibility = Visibility.Collapsed;
            this.LeftArrow.Visibility = Visibility.Collapsed;
            this.RightArrow.Visibility = Visibility.Collapsed;
        }
        #endregion Laden der PDF Dateien zum Rendern

        #region Image from PDF
        private void LoadPdf(string fileName)
        {
            this._document = fpdfview.FPDF_LoadDocument(fileName, null);
            if (this._document == null)
            {
                throw new Exception("PDF konnte nicht geladen werden.");
            }

            this._currentPage = 0;
            this.PDFPageInfo = $"{this._currentPage+1}/{fpdfview.FPDF_GetPageCount(this._document)}";
            this.RenderPage();
        }

        private unsafe void RenderPage()
        {
            if (_document == null)
            {
                return;
            }

            double zoom = 1;
            var page = fpdfview.FPDF_LoadPage(_document, _currentPage);

            int width = (int)fpdfview.FPDF_GetPageWidth(page);
            int height = (int)fpdfview.FPDF_GetPageHeight(page);

            int renderWidth = (int)(width * zoom);
            int renderHeight = (int)(height * zoom);

            int stride = renderWidth * 4;
            byte[] buffer = new byte[stride * renderHeight];

            fixed (byte* ptr = buffer)
            {
                FpdfBitmapT bitmap = fpdfview.FPDFBitmapCreateEx(renderWidth, renderHeight, (int)FPDFBitmapFormat.BGRA, (nint)ptr, stride);

                fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, renderWidth, renderHeight, 0xFFFFFFFF);

                fpdfview.FPDF_RenderPageBitmap(bitmap, page, 0, 0, renderWidth, renderHeight, 0, 0);

                BitmapSource image = BitmapSource.Create(renderWidth, renderHeight, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, buffer, stride);

                image.Freeze();

                this.PdfImage.Source = image;

                fpdfview.FPDFBitmapDestroy(bitmap);
            }
        }

        private BitmapSource RenderPdfPage(int pageIndex, double zoom = 1.0)
        {
            var page = fpdfview.FPDF_LoadPage(this._document, pageIndex);

            try
            {
                double pageWidth = fpdfview.FPDF_GetPageWidth(page);

                double pageHeight = fpdfview.FPDF_GetPageHeight(page);

                int width = (int)(pageWidth * zoom);
                int height = (int)(pageHeight * zoom);

                int stride = width * 4;

                byte[] buffer = new byte[stride * height];

                unsafe
                {
                    fixed (byte* pBuffer = buffer)
                    {
                        var bitmap = fpdfview.FPDFBitmapCreateEx(width, height, 4, (nint)pBuffer, stride);

                        try
                        {
                            fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, width, height, 0xFFFFFFFF);

                            fpdfview.FPDF_RenderPageBitmap(bitmap, page, 0, 0, width, height, 0, 0);
                        }
                        finally
                        {
                            fpdfview.FPDFBitmapDestroy(bitmap);
                        }
                    }
                }

                return BitmapSource.Create(width, height, 96 * zoom, 96 * zoom, System.Windows.Media.PixelFormats.Bgra32, null, buffer, stride);
            }
            finally
            {
                PDFiumCore.fpdfview.FPDF_ClosePage(page);
            }
        }
        #endregion Image from PDF

        private void BoolHandler(bool arg1, string arg2)
        {
            if (arg2.Equals("PrintAllPages") == true)
            {
                this.PrintPageVarinate = arg1 == true ? PrintVariante.AllPages : PrintVariante.None;
            }
            else
            {
                this.PrintPageVarinate = PrintVariante.None;
            }
        }

        private void StringHandler(string arg1, string arg2)
        {
            if (arg2.Equals("SinglePages") == true)
            {
                this.PrintPageVarinate = string.IsNullOrEmpty(arg1) == false ? PrintVariante.SinglePages : PrintVariante.None;
            }
            else if (arg2.Equals("ExtractRangePages") == true)
            {
                this.PrintPageVarinate = string.IsNullOrEmpty(arg1) == false ? PrintVariante.RangePages : PrintVariante.None;
            }
            else
            {
                this.PrintPageVarinate = PrintVariante.None;
            }
        }

        private string ParseTextBoxInput()
        {
            // 1. Text auslesen und Strichpunkte vereinheitlichen (Semikolon als Trenner)
            string rawInput = string.IsNullOrEmpty(this.SinglePages) == false ? this.SinglePages : this.ExtractRangePages;

            if (string.IsNullOrEmpty(rawInput))
            {
                rawInput = string.IsNullOrEmpty(this.ExtractRangePages) == false ? this.ExtractRangePages : this.SinglePages;
            }

            // 2. Zahlen und Bereiche extrahieren
            var parsedNumbers = new HashSet<int>();
            var parts = rawInput.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                // Wenn es ein Bereich (z.B. "1-3") ist
                if (part.Contains("-"))
                {
                    var range = part.Split('-');
                    if (range.Length == 2 &&
                        int.TryParse(range[0], out int start) &&
                        int.TryParse(range[1], out int end))
                    {
                        // Für den Fall, dass Start größer als Ende angegeben wurde
                        if (start > end) { int temp = start; start = end; end = temp; }

                        for (int i = start; i <= end; i++)
                        {
                            parsedNumbers.Add(i);
                        }
                    }
                }
                // Wenn es eine einzelne Zahl ist
                else if (int.TryParse(part, out int singleNumber))
                {
                    parsedNumbers.Add(singleNumber);
                }
            }

            // 3. Sortieren und zu einem String mit ';' zusammensetzen
            string result = string.Join(";", parsedNumbers.OrderBy(n => n));

            // 4. Ergebnis zurück in die TextBox schreiben (oder anderweitig verwenden)
            return result;
        }

        private async void PrintAllPagesHandler()
        {
            if (this.SelectedPdfFile == null)
            {
                return;
            }

            if (App.EventAgg.IsSubscription<StatusEvent>() == true)
            {
                string pageOK = $"PDF Datei {this.SelectedPdfFile} wird gedruckt";
                await App.EventAgg.PublishAsync(new StatusEvent(pageOK));
            }

            // Alle Seiten rendern
            // BitmapSource-Liste füllen
            var pages = new List<BitmapSource>();

            int pageCount = fpdfview.FPDF_GetPageCount(this._document);

            for (int i = 0; i < pageCount; i++)
            {
                pages.Add(RenderPdfPage(i));
            }

            var printer = new PdfPrintService();

            printer.Print(pages, this.SelectedPdfFile.Filename);
        }

        private async void PrintSelectedPage(string splitRange)
        {
            PDFFileItem SelectedPdfFile = this.PDFFilesSource.FirstOrDefault(f => f.IsSelectedItem == true);
            List<int> printRangeSorce = splitRange.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            int endSeite = printRangeSorce.Last();
            int pageCount = fpdfview.FPDF_GetPageCount(this._document);

            if (endSeite > pageCount)
            {
                this.Message.Hinweis("PDF Drucken", $"Sie haben mehr Seiten angegeben, wie das Dokument hat. Anzahl Seiten: {pageCount}.");
                return;
            }


            if (App.EventAgg.IsSubscription<StatusEvent>() == true)
            {
                string pageOK = $"PDF Datei {this.SelectedPdfFile} wird gedruckt";
                await App.EventAgg.PublishAsync(new StatusEvent(pageOK));
            }

            // Ausgewählte Seiten rendern
            // BitmapSource-Liste füllen

            var pages = new List<BitmapSource>();
            foreach (int page in printRangeSorce)
            {
                pages.Add(RenderPdfPage(page));
            }

            var printer = new PdfPrintService();

            printer.Print(pages, this.SelectedPdfFile.Filename);
        }

        private async void ExtractPages(string splitRange)
        {
            List<int> splitRangeSorce = splitRange.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

            int startSeite = splitRangeSorce.First();
            int endSeite = splitRangeSorce.Last();

            if (App.EventAgg.IsSubscription<StatusEvent>() == true)
            {
                string pageOK = $"PDF Datei {this.SelectedPdfFile} wird gedruckt";
                await App.EventAgg.PublishAsync(new StatusEvent(pageOK));
            }

            var pages = new List<BitmapSource>();

            // Sicherstellen, dass die Seiten im Original existieren
            int pageCount = fpdfview.FPDF_GetPageCount(this._document);
            if (endSeite < pageCount)
            {
                // 3. Seiten durchlaufen und zum Zieldokument hinzufügen
                for (int i = startSeite; i <= endSeite; i++)
                {
                    pages.Add(RenderPdfPage(i));
                }
            }

            var printer = new PdfPrintService();

            printer.Print(pages, this.SelectedPdfFile.Filename);
        }

    }
}
