//-----------------------------------------------------------------------
// <copyright file="PDFMergeView.cs" company="Lifeprojects.de">
//     Class: PDFMergeView
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

    /// <summary>
    /// Interaktionslogik für PDFMergeView.xaml
    /// </summary>
    public partial class PDFMergeView : UserControlBase
    {
        private FpdfDocumentT _document;
        private int _currentPage;
        private Point _dragStartPoint;

        public PDFMergeView(ChangeViewEventArgs args) : base(typeof(PDFMergeView))

        {
            this.InitializeComponent();

            fpdfview.FPDF_InitLibrary();

            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            this.CurrentCtorArgs = args;

            this.GoBackCommand = new CommandBase(commandParam => this.OnGoBack(commandParam), () => true);
            this.OpenFolderCommand = new CommandBase(commandParam => this.OnOpenFolder(commandParam), () => true);
            this.PlusPageCommand = new CommandBase(commandParam => this.OnPlusMinusPage(commandParam), () => true);
            this.MinusPageCommand = new CommandBase(commandParam => this.OnPlusMinusPage(commandParam), () => true);
            this.SavePDFCommand = new CommandBase(commandParam => this.OnSavePDF(commandParam), () => true);

            this.DataContext = this;
        }

        #region Properties
        public CommandBase GoBackCommand { get; private set; }
        public CommandBase OpenFolderCommand { get; private set; }
        public CommandBase PlusPageCommand { get; private set; }
        public CommandBase MinusPageCommand { get; private set; }
        public CommandBase SavePDFCommand { get; private set; }

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

        public double SelectionZoom
        {
            get => base.GetValue<double>();
            set => base.SetValue(value);
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

        public string MergeFilename
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string PDFPageInfo
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

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

                this.PDFPageInfo = $"{this._currentPage+1}/{fpdfview.FPDF_GetPageCount(this._document)}";
            }
        }

        private async void OnSavePDF(object commandParam)
        {
            string mergePath = string.Empty;
            if (string.IsNullOrEmpty(this.MergeFilename) == true)
            {
                this.Message.Hinweis("PDF Speichern", "Bitte geben Sie einen Dateinamen ein.");
                return;
            }

            if (this.PDFFilesSource.Count(f => f.IsSelectedItem == true) == 0)
            {
                this.Message.Hinweis("PDF Speichern", "Bitte wählen Sie mindestens eine PDF-Datei aus.");
                return;
            }

            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.SavePDF)
                {
                    var selectedFile = this.PDFFilesSource.Where(f => f.IsSelectedItem == true).ToList();
                    if (selectedFile != null && selectedFile.Count > 0)
                    {
                        this.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

                        using (var targetDoc = new PdfDocument())
                        {
                            foreach (var file in selectedFile)
                            {
                                string pdfname = file.Fullname;
                                mergePath = Path.GetDirectoryName(pdfname);

                                if (File.Exists(pdfname) == false)
                                {
                                    continue;
                                }

                                // Quelldatei im Import-Modus öffnen
                                using (var pdfDoc = PdfReader.Open(pdfname, PdfDocumentOpenMode.Import))
                                {
                                    // Alle Seiten des Dokuments durchgehen
                                    for (var i = 0; i < pdfDoc.PageCount; i++)
                                    {
                                        // Seite importieren und dem Zieldokument hinzufügen
                                        targetDoc.AddPage(pdfDoc.Pages[i]);
                                    }
                                }

                                if (App.EventAgg.IsSubscription<StatusEvent>() == true)
                                {
                                    string mergeFileInfo = $"Datei {file.Filename} wurde zu {this.MergeFilename} hinzugefügt.";
                                    await App.EventAgg.PublishAsync(new StatusEvent(mergeFileInfo));
                                }

                            }

                            // Zusammengeführte Datei speichern
                            string fullMergeName = this.MergeFilename;
                            targetDoc.Save(fullMergeName);
                        }

                        this.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

                        this.LoadFileToListbox(mergePath);
                    }

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
            string titel = Path.GetFileName(fileName);

            _ = fpdf_doc.FPDF_GetMetaText(this._document, titel, 0, 0);
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

            double zoom = ((this.SelectionZoom == 0 ? 100 : this.SelectionZoom) / 100);
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

        #endregion Image from PDF
    }
}
