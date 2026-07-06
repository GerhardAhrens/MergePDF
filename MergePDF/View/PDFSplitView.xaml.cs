//-----------------------------------------------------------------------
// <copyright file="PDFSplitView.cs" company="Lifeprojects.de">
//     Class: PDFSplitView
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>GERHARD-G6\gerha - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>04.07.2026</date>
//
// <summary>
// Dialog zum Aufteilen von PDF Dateien
// </summary>
//-----------------------------------------------------------------------

namespace MergePDF.View
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    using MergePDF.Core;

    using Microsoft.Win32;

    using PDFiumCore;

    /// <summary>
    /// Interaktionslogik für PDFSplitView.xaml
    /// </summary>
    public partial class PDFSplitView : UserControlBase
    {
        private FpdfDocumentT _document;
        private int _currentPage;
        private Point _dragStartPoint;

        public PDFSplitView(ChangeViewEventArgs args) : base(typeof(PDFSplitView))

        {
            this.InitializeComponent();

            fpdfview.FPDF_InitLibrary();

            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            this.CurrentCtorArgs = args;

            this.GoBackCommand = new CommandBase(commandParam => this.OnGoBack(commandParam), () => true);
            this.OpenFolderCommand = new CommandBase(commandParam => this.OnOpenFolder(commandParam), () => true);

            this.DataContext = this;
        }

        #region Properties
        public CommandBase GoBackCommand { get; private set; }
        public CommandBase OpenFolderCommand { get; private set; }

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
                LoadFileToListbox(selectedFolderPath);
            }
        }

        #endregion Command Events

        #region Laden der PDF Dateien zum Rendern
        private async void LoadFileToListbox(string folderPath)
        {

        }

        private void SelectedPdfFileHandler(PDFFileItem item, string arg2)
        {
            this.LoadPdf(item.Fullname);
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

            this.PDFPageInfo = $"{fpdfview.FPDF_GetPageCount(this._document)}";
            this._currentPage = 0;
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

                //this.PdfImage.Source = image;

                fpdfview.FPDFBitmapDestroy(bitmap);
            }
        }

        #endregion Image from PDF
    }
}
