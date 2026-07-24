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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

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

        public ICollectionView ScannerSource
        {
            get => base.GetValue<ICollectionView>();
            set => base.SetValue(value);
        }

        public ScannerInfo SelectedScanner
        {
            get => base.GetValue<ScannerInfo>();
            set => base.SetValue(value);
        }

        private ChangeViewEventArgs CurrentCtorArgs { get; set; }
        private MessageBase Message { get; } = new MessageBase();
        #endregion Properties

        #region Windows Events

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            List<ScannerInfo> scanners = new();
            DeviceManager manager = new();
            foreach (DeviceInfo deviceInfo in manager.DeviceInfos)
            {
                if (deviceInfo.Type == WiaDeviceType.ScannerDeviceType)
                {
                    foreach (Property prop in deviceInfo.Properties)
                    {
                        Debug.WriteLine($"{prop.Name}: {prop.get_Value()}");
                    }

                    ScannerInfo si = new();
                    si.Name = deviceInfo.Properties["Name"].get_Value().ToString();
                    si.Description = deviceInfo.Properties["Description"].get_Value().ToString();
                    si.Manufacturer = deviceInfo.Properties["Manufacturer"].get_Value().ToString();
                    si.Server = deviceInfo.Properties["Server"].get_Value().ToString();
                    si.UniqueDeviceID = deviceInfo.Properties["Unique Device ID"].get_Value().ToString();
                    si.DeviceInfo = deviceInfo;
                    scanners.Add(si);
                }
            }

            this.ScannerSource = CollectionViewSource.GetDefaultView(scanners);

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
                        Device scanner = SelectedScanner.DeviceInfo.Connect();
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
