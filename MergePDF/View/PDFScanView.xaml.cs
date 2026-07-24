//-----------------------------------------------------------------------
// <copyright file="PDFScanView.cs" company="Lifeprojects.de">
//     Class: PDFScanView
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>GERHARD-G6\gerha - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>23.07.2026</date>
//
// <summary>
// UserControl zum Sannen von Dokumenten
// </summary>
//-----------------------------------------------------------------------

namespace MergePDF.View
{
    using System.Windows;
    using System.Windows.Controls;

    using MergePDF.Core;

    using WIA;

    /// <summary>
    /// Interaktionslogik für PDFScan.xaml
    /// </summary>
    public partial class PDFScanView : UserControlBase
    {
        public PDFScanView(ChangeViewEventArgs args) : base(typeof(PDFScanView))

        {
            this.InitializeComponent();
            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            this.CurrentCtorArgs = args;

            this.GoBackCommand = new CommandBase(commandParam => this.OnGoBack(commandParam), () => true);
            this.SacnPDFCommand = new CommandBase(commandParam => this.OnSanPDF(commandParam), () => true);

            this.DataContext = this;
        }

        #region Properties
        public CommandBase GoBackCommand { get; private set; }
        public CommandBase SacnPDFCommand { get; private set; }

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

        private void OnSanPDF(object commandParam)
        {
            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.PDFScan)
                {
                    try
                    {
                        var dialog = new CommonDialog();

                        Device scanner = dialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);

                        if (scanner == null)
                        {
                            return;
                        }
                        

                        Item item = scanner.Items[1];
                    }
                    catch (Exception ex)
                    {
                        this.Message.Error("Dokument Scannen", $"Es ist ein Problem mit einem Scannner aufgetreten \n{ex.Message}");
                    }
                }
            }
        }

        #endregion Command Events

    }
}
