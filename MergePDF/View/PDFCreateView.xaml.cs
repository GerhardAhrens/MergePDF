//-----------------------------------------------------------------------
// <copyright file="PDFCreateView.cs" company="Lifeprojects.de">
//     Class: PDFCreateView
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>GERHARD-G6\gerha - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>24.08.2026</date>
//
// <summary>
// Template für eine neues UserControl
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

    using PdfSharpCore.Drawing;
    using PdfSharpCore.Pdf;

    /// <summary>
    /// Interaktionslogik für PDFCreateView.xaml
    /// </summary>
    public partial class PDFCreateView : UserControlBase
    {
        private Point _dragStartPoint;

        public PDFCreateView(ChangeViewEventArgs args) : base(typeof(PDFCreateView))

        {
            this.InitializeComponent();
            this.CurrentCtorArgs = args;

            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            this.GoBackCommand = new CommandBase(commandParam => this.OnGoBack(commandParam), () => true);
            this.OpenFolderCommand = new CommandBase(commandParam => this.OnOpenFolder(commandParam), () => true);
            this.SavePDFCommand = new CommandBase(commandParam => this.OnSavePDF(commandParam), () => true);


            this.DataContext = this;
        }

        #region Properties
        public CommandBase GoBackCommand { get; private set; }
        public CommandBase OpenFolderCommand { get; private set; }
        public CommandBase SavePDFCommand { get; private set; }

        public ObservableCollection<PDFFileItem> ImageFilesSource
        {
            get => base.GetValue<ObservableCollection<PDFFileItem>>();
            set => base.SetValue(value);
        }

        public PDFFileItem SelectedImageFile
        {
            get => base.GetValue<PDFFileItem>();
            set => base.SetValue(value, this.SelectedImageFileHandler);
        }

        public string DragDropTooltipText
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string CreateFilename
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string ImageInfo
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string FileSizeTooltip
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        private ChangeViewEventArgs CurrentCtorArgs { get; set; }
        private MessageBase Message { get; } = new MessageBase();
        private ApplicationSettings Settings { get; set; }

        #endregion Properties

        #region Windows Events

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            string dfFolder = string.IsNullOrEmpty(App.Settings.LastScanFolder) == false ? App.Settings.LastScanFolder : string.Empty;
            this.LoadFileToListbox(dfFolder);

            if (App.EventAgg.IsSubscription<StatusEvent>() == true)
            {
                await App.EventAgg.PublishAsync(new StatusEvent("Bereit"));
            }
        }
        #endregion Windows Events

        #region ListBox Events
        private void SelectedImageFileHandler(PDFFileItem item, string arg2)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(item.Fullname, UriKind.Absolute);
            bitmap.EndInit();

            DefaultImage.Source = bitmap;
        }

        private void ListBoxFiles_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._dragStartPoint = e.GetPosition(null);
        }

        private void ListBoxFiles_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Point mousePos = e.GetPosition(null);
            Vector diff = this._dragStartPoint - mousePos;

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

            int oldIndex = this.ImageFilesSource.IndexOf((PDFFileItem)droppedData);

            int newIndex;

            if (target is not PDFFileItem)
            {
                newIndex = this.ImageFilesSource.Count - 1;
            }
            else
            {
                newIndex = this.ImageFilesSource.IndexOf((PDFFileItem)target);
            }

            if (oldIndex < 0 || newIndex < 0)
            {
                return;
            }

            this.DragDropPopup.IsOpen = false;
            this.InsertLine.Visibility = Visibility.Collapsed;
            this.LeftArrow.Visibility = Visibility.Collapsed;
            this.RightArrow.Visibility = Visibility.Collapsed;
            this.ImageFilesSource.Move(oldIndex, newIndex);
        }

        private void ListBoxFiles_DragLeave(object sender, DragEventArgs e)
        {
            this.InsertLine.Visibility = Visibility.Collapsed;
            this.LeftArrow.Visibility = Visibility.Collapsed;
            this.RightArrow.Visibility = Visibility.Collapsed;
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

        #endregion ListBox Events

        #region Command Events
        private async void OnGoBack(object commandParam)
        {
            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.GoBack)
                {
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

                this.Settings = App.Settings;
                this.Settings.LastScanFolder = selectedFolderPath;
                using (ApplicationSettings settings = new ApplicationSettings())
                {
                    if (settings.IsExitSettings() == true)
                    {
                        settings.SetSetting(Settings);
                        settings.Save();
                    }
                }

                this.LoadFileToListbox(selectedFolderPath);
            }
        }

        private async void OnSavePDF(object commandParam)
        {
            string mergePath = string.Empty;
            if (string.IsNullOrEmpty(this.CreateFilename) == true)
            {
                this.Message.Hinweis("PDF Speichern", "Bitte geben Sie einen Dateinamen ein.");
                return;
            }

            if (this.ImageFilesSource.Count(f => f.IsSelectedItem == true) == 0)
            {
                this.Message.Hinweis("PDF Speichern", "Bitte wählen Sie mindestens eine PDF-Datei aus.");
                return;
            }

            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.SavePDF)
                {
                    var selectedFile = this.ImageFilesSource.Where(f => f.IsSelectedItem == true).ToList();
                    if (selectedFile != null && selectedFile.Count > 0)
                    {
                        this.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

                        PdfImageExporter.CreatePdf($"{this.CreateFilename}.pdf", selectedFile);

                        this.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
                    }
                }
            }

        }
        #endregion Command Events


        private async void LoadFileToListbox(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) == true)
            {
                return;
            }

            PDFFileItem fitem;
            List<PDFFileItem> files = new();

            // Optionen definieren (z. B. Unterverzeichnisse durchsuchen und Zugriffsfehler ignorieren)
            var enumerationOptions = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true
            };

            string[] erlaubteEndungen = { ".png", ".jpg", ".bmp", ".tif" };
            IEnumerable<string> filesFolder = Directory.EnumerateFiles(folderPath, "*.*", enumerationOptions);
            filesFolder = filesFolder.Where(f => erlaubteEndungen.Contains(Path.GetExtension(f).ToLower()));
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

                this.ImageFilesSource = new ObservableCollection<PDFFileItem>(files.OrderBy(f => f.Order));

                if (App.EventAgg.IsSubscription<StatusEvent>() == true)
                {
                    if (ImageFilesSource.Count == 0)
                    {
                        await App.EventAgg.PublishAsync(new StatusEvent($"Bereit: Keine Dateien gefunden"));
                    }
                    else if (ImageFilesSource.Count == 1)
                    {
                        await App.EventAgg.PublishAsync(new StatusEvent($"Bereit: Eine Datei gefunden"));
                    }
                    else if (ImageFilesSource.Count > 1)
                    {
                        await App.EventAgg.PublishAsync(new StatusEvent($"Bereit: {ImageFilesSource.Count} Dateien gefunden"));
                    }
                }

            }
        }
    }

    public static class PdfImageExporter
    { 
        /// <summary> 
        /// Erstellt aus mehreren Bilddateien eine PDF-Datei.Jede Bilddatei wird auf einer eigenen A4-Seite platziert. Bilder, 
        /// die kleiner als der verfügbare Bereich sind, werden in ihrer Originalgröße übernommen. Größere Bilder werden proportional 
        /// verkleinert. 
        /// </summary>
        /// <param name="outputPdfPath">Zielpfad der PDF-Datei.</param>
        /// <param name="imagePaths">Liste der Bilddateien.</param>
        /// <param name="marginMm">Seitenrand in Millimetern.</param> 
        public static void CreatePdf( string outputPdfPath, List<PDFFileItem> imagePaths, double marginMm = 10)
        { 
            if (string.IsNullOrWhiteSpace(outputPdfPath))
            {
                throw new ArgumentException("Es wurde kein Ausgabepfad angegeben.", nameof(outputPdfPath));
            }

            if (imagePaths == null)
            {
                throw new ArgumentNullException(nameof(imagePaths));
            }

            if (marginMm < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(marginMm), "Der Rand darf nicht negativ sein.");
            }

            // A4 in PDF-Punkten 210 x 297 mm = 595.28 x 841.89 pt
            const double pageWidth = 595.2756; 
            const double pageHeight = 841.8898; 
            double margin = MmToPoints(marginMm); 
            double availableWidth = pageWidth - (2 * margin); 
            double availableHeight = pageHeight - (2 * margin); 
            
            if (availableWidth <= 0 || availableHeight <= 0)
            {
                throw new ArgumentException("Der Seitenrand ist für A4 zu groß.", nameof(marginMm));
            }

            var document = new PdfDocument(); 
            foreach (PDFFileItem imagePath in imagePaths)
            { 
                if (string.IsNullOrWhiteSpace(imagePath.Fullname))
                {
                    continue;
                }
                if (!File.Exists(imagePath.Fullname))
                {
                    throw new FileNotFoundException("Die Bilddatei wurde nicht gefunden.", imagePath.Fullname);
                }

                AddImagePage( document, imagePath.Fullname, availableWidth, availableHeight, pageWidth, pageHeight); 
            }
            
            // Falls keine Bilder übergeben wurden,
            // keine leere PDF erzeugen.
            if (document.PageCount == 0)
            {
                throw new InvalidOperationException("Es wurden keine gültigen Bilder übergeben.");
            }

            document.Save(outputPdfPath); 
        } 

        private static void AddImagePage( PdfDocument document, string imagePath, double availableWidth, double availableHeight, double pageWidth, double pageHeight)
        { 
            var page = document.AddPage(); 
            page.Size = PdfSharpCore.PageSize.A4; 
            using var gfx = XGraphics.FromPdfPage(page); 

            // WPF verwenden, um die tatsächliche Pixelgröße 
            // und DPI-Information des Bildes zu ermitteln.
            var bitmap = new BitmapImage(); 
            bitmap.BeginInit(); 
            bitmap.UriSource = new Uri( Path.GetFullPath(imagePath), UriKind.Absolute); 
            bitmap.CacheOption = BitmapCacheOption.OnLoad; 
            bitmap.EndInit(); bitmap.Freeze();
            
            // Bildgröße in PDF-Punkten bestimmen.
            double imageWidth = PixelToPoints( bitmap.PixelWidth, bitmap.DpiX); 
            double imageHeight = PixelToPoints( bitmap.PixelHeight, bitmap.DpiY);
            
            // Falls keine brauchbaren DPI vorhanden sind:
            if (imageWidth <= 0)
            {
                imageWidth = bitmap.PixelWidth;
            }

            if (imageHeight <= 0)
            {
                imageHeight = bitmap.PixelHeight;
            }

            double drawWidth = imageWidth; double drawHeight = imageHeight;

            // ---------------------------------------------------------
            // Bild größer als der verfügbare A4-Bereich?
            // Dann proportional verkleinern.
            // ---------------------------------------------------------

            if (imageWidth > availableWidth || imageHeight > availableHeight)
            { 
                double scaleX = availableWidth / imageWidth; 
                double scaleY = availableHeight / imageHeight;
                
                // Der kleinere Faktor stellt sicher,
                // dass das komplette Bild hineinpasst.
                double scale = Math.Min(scaleX, scaleY); 
                drawWidth = imageWidth * scale; 
                drawHeight = imageHeight * scale; 
            }
            
            // ---------------------------------------------------------
            // Bild auf der Seite zentrieren
            // ---------------------------------------------------------
            double x = (pageWidth - drawWidth) / 2.0;
            double y = (pageHeight - drawHeight) / 2.0; 
            using var xImage = XImage.FromFile(imagePath); 
            gfx.DrawImage( xImage, x, y, drawWidth, drawHeight); 
        } 
        
        private static double PixelToPoints( int pixels, double dpi)
        {
            if (dpi <= 0) return pixels; 
            
            // 1 Inch = 72 PDF-Punkte
            return pixels / dpi * 72.0; 
        } 

        private static double MmToPoints(double mm)
        { 
            return mm / 25.4 * 72.0; 
        } 
    }
}
