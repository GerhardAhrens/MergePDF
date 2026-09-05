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

    using MergePDF.Core;

    using Microsoft.Win32;

    /// <summary>
    /// Interaktionslogik für PDFCreateView.xaml
    /// </summary>
    public partial class PDFCreateView : UserControlBase
    {
        public PDFCreateView(ChangeViewEventArgs args) : base(typeof(PDFCreateView))

        {
            this.InitializeComponent();
            this.CurrentCtorArgs = args;

            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            this.GoBackCommand = new CommandBase(commandParam => this.OnGoBack(commandParam), () => true);
            this.OpenFolderCommand = new CommandBase(commandParam => this.OnOpenFolder(commandParam), () => true);

            this.DataContext = this;
        }

        #region Properties
        public CommandBase GoBackCommand { get; private set; }
        public CommandBase OpenFolderCommand { get; private set; }

        public ObservableCollection<PDFFileItem> ImageFilesSource
        {
            get => base.GetValue<ObservableCollection<PDFFileItem>>();
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

        private ChangeViewEventArgs CurrentCtorArgs { get; set; }
        private MessageBase Message { get; } = new MessageBase();
        private ApplicationSettings Settings { get; set; }

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

        #endregion Command Events


        private async void LoadFileToListbox(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) == true)
            {
                return;
            }

            PDFFileItem fitem;
            List<PDFFileItem> files = new();
            IEnumerable<string> filesFolder = Directory.EnumerateFiles(folderPath, "*.png", SearchOption.AllDirectories);
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
}
