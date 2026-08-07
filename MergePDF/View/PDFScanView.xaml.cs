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
    using System.ComponentModel;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.ServiceProcess;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

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
            this.SaveFileCommand = new CommandBase(commandParam => this.OnSaveFile(commandParam), () => true);

            this.DataContext = this;
        }

        #region Properties
        public CommandBase GoBackCommand { get; private set; }
        public CommandBase SacnPDFCommand { get; private set; }
        public CommandBase SaveFileCommand { get; private set; }

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

        public BitmapImage ScannedImage
        {
            get => base.GetValue<BitmapImage>();
            set => base.SetValue(value);
        }

        public string SaveFolder
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string FileSuffix
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public bool IsPNG
        {
            get => base.GetValue<bool>();
            set => base.SetValue(value);
        }

        public bool IsPDF
        {
            get => base.GetValue<bool>();
            set => base.SetValue(value);
        }

        public bool AutomaticSave
        {
            get => base.GetValue<bool>();
            set => base.SetValue(value);
        }

        private ChangeViewEventArgs CurrentCtorArgs { get; set; }
        private MessageBase Message { get; } = new MessageBase();
        #endregion Properties

        #region Windows Events

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.AutomaticSave = true;
            this.IsPNG = false;
            this.IsPDF = true;
            this.FileSuffix = "Document";

            List<ScannerInfo> scanners = new();
            DeviceManager manager = new();
            ServiceController sc = new("stisvc");

            if (sc.Status != ServiceControllerStatus.Running)
            {
                if (App.EventAgg.IsSubscription<StatusEvent>() == true)
                {
                    await App.EventAgg.PublishAsync(new StatusEvent("Windows Image Acquisition (WIA, stisvc) funktioniert nicht"));
                }

                return;
            }

            foreach (DeviceInfo deviceInfo in manager.DeviceInfos)
            {
                if (deviceInfo.Type == WiaDeviceType.ScannerDeviceType)
                {
                    Device device = await ConnectWithTimeoutAsync(deviceInfo, TimeSpan.FromSeconds(5));

                    ScannerInfo si = new();
                    si.Name = deviceInfo.Properties["Name"].get_Value().ToString();
                    si.Description = deviceInfo.Properties["Description"].get_Value().ToString();
                    si.Manufacturer = deviceInfo.Properties["Manufacturer"].get_Value().ToString();
                    si.Server = deviceInfo.Properties["Server"].get_Value().ToString();
                    si.UniqueDeviceID = deviceInfo.Properties["Unique Device ID"].get_Value().ToString();
                    si.DeviceInfo = deviceInfo;
                    si.IP_Adresse = "192.168.178.20";
                    try
                    {
                        /*
                        using HttpClient client = new();
                        await client.GetAsync($"http://{si.IP_Adresse}/");
                        */

                        scanners.Add(si);
                    }
                    catch (COMException)
                    {
                        break;
                    }
                }
            }

            if (scanners.Any() == false)
            {
                ScannerInfo si = new();
                si.Name = "Kein Scanner";
                si.Description = "oder Offline";
                scanners.Add(si);
                this.BtnScanPDF.IsEnabled = false;
                DrawingImage noScan = (DrawingImage)Application.Current.TryFindResource("NoScannerImage");
                this.PdfImage.Height = 500;
                this.PdfImage.Width = 500;
                this.PdfImage.Source = noScan;
                this.PdfImage.Stretch = Stretch.Uniform;
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
                        (bool, string) isOk = IsScannerOnline(this.SelectedScanner.DeviceInfo);
                        if (isOk.Item1 == true)
                        {
                            Device scanner = this.SelectedScanner.DeviceInfo.Connect();
                            Item item = scanner.Items[1];

                            string format = "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}"; // PNG
                            ImageFile image = (ImageFile)new CommonDialog().ShowTransfer(item, format, false);

                            byte[] bytes = (byte[])image.FileData.get_BinaryData();

                            using MemoryStream ms = new(bytes);

                            BitmapImage bitmap = new();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();
                            bitmap.Freeze();

                            this.ScannedImage = bitmap;
                        }
                        else
                        {
                            this.Message.Error("Dokument scannen", isOk.Item2);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Message.Error("Dokument Scannen", $"Es ist ein Problem mit dem Scannner aufgetreten \n{ex.Message}");
                    }
                }
            }
        }

        private void OnSaveFile(object commandParam)
        {
            if (string.IsNullOrEmpty(this.SaveFolder) == true)
            {
                this.Message.Warning("Scan Speichern", "Es wurde keine Verzeichnis zum Speichern ausgewählt.");
                return;
            }

            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.SavePDF)
                {
                    if (this.PdfImage.Source != null)
                    {
                        if (this.AutomaticSave == true)
                        {
                            if (this.IsPDF == true && this.IsPNG == false)
                            {
                                string filenamePattern = $"{DateTime.Now:yyyyMMdd}_{this.FileSuffix}_{{000}}.pdf";
                                string newFilename = FileNameGenerator.GetNextFileName(this.SaveFolder, filenamePattern);

                            }
                            else if (this.IsPDF == false && this.IsPNG == true)
                            {
                                string filenamePattern = $"{DateTime.Now:yyyyMMdd}_{this.FileSuffix}_{{000}}.png";
                                string newFilename = FileNameGenerator.GetNextFileName(this.SaveFolder, filenamePattern);
                                ToPngConverter.SaveImageSourceToPng(this.PdfImage.Source, newFilename);
                            }
                        }
                        else
                        {
                        }
                    }
                }
            }
        }

        #endregion Command Events

        private (bool,string) IsScannerOnline(DeviceInfo deviceInfo)
        {
            bool result = false;
            string errorText = string.Empty;

            try
            {
                Device device = deviceInfo.Connect();
                result = device != null;
                return (result, string.Empty);
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x80210015)
            {
                errorText = "Offline";
                return (result, errorText);
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x80210006)
            {
                errorText = "Belegt";
                return (result, errorText);
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x80210064)
            {
                errorText = "Papierstau";
                return (result, errorText);
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x8021000C)
            {
                errorText = "Gerät nicht verfügbar";
                return (result, errorText);
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x80210016)
            {
                errorText = "Papierfach leer (bei ADF, treiberabhängig)";
                return (result, errorText);
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x80210003)
            {
                errorText = "Allgemeiner WIA-Fehler";
                return (result, errorText);
            }
            catch
            {
                errorText = "Unbekanter Fehler";
                return (result, errorText);
            }
        }

        private async Task<Device> ConnectWithTimeoutAsync(DeviceInfo deviceInfo, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<Device>();

            Thread thread = new(() =>
            {
                try
                {
                    Device device = deviceInfo.Connect();
                    tcs.TrySetResult(device);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            Task finished = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

            if (finished == tcs.Task)
            {
                return await tcs.Task;
            }

            return null;
        }
    }
}
