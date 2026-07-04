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
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using MergePDF.Core;

    using Microsoft.Win32;

    using SnippetManager.Core;

    /// <summary>
    /// Interaktionslogik für PDFMergeView.xaml
    /// </summary>
    public partial class PDFMergeView : UserControlBase
    {
        public PDFMergeView(ChangeViewEventArgs args) : base(typeof(PDFMergeView))

        {
            this.InitializeComponent();
            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            this.CurrentCtorArgs = args;

            this.GoBackCommand = new CommandBase(commandParam => this.OnGoBack(commandParam), () => true);
            this.OpenFolderCommand = new CommandBase(commandParam => this.OnOpenFolder(commandParam), () => true);

            this.DataContext = this;
        }

        #region Properties
        public CommandBase GoBackCommand { get; private set; }
        public CommandBase OpenFolderCommand { get; private set; }

        public ICollectionView PDFFilesSource
        {
            get => base.GetValue<ICollectionView>();
            set => base.SetValue(value);
        }

        public PDFFileItem SelectedPdfFile
        {
            get => base.GetValue<PDFFileItem>();
            set => base.SetValue(value, this.SelectedPdfFileHandler);
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

        private void LoadFileToListbox(string folderPath)
        {
            PDFFileItem fitem;
            List<PDFFileItem> files = new();
            IEnumerable<string> filesFolder = Directory.EnumerateFiles(folderPath, "*.pdf", SearchOption.AllDirectories);
            if (filesFolder != null && filesFolder.Any() == true)
            {
                int order = 1;
                foreach (string file in filesFolder)
                {
                    fitem = new PDFFileItem();
                    fitem.Fullname = file;
                    fitem.Filename = Path.GetFileName(file);
                    fitem.Order = order++;
                    fitem.IsSelectedItem = false;
                    files.Add(fitem);
                }

                this.PDFFilesSource = CollectionViewSource.GetDefaultView(files.OrderBy(f => f.Order));
            }
        }

        private void SelectedPdfFileHandler(PDFFileItem item, string arg2)
        {
        }
    }
}
