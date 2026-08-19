namespace MergePDF.View
{
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;

    using MergePDF;

    using MergePDF.Core;

    /// <summary>
    /// Interaktionslogik für SettingsUC.xaml
    /// </summary>
    public partial class SettingsUC : UserControlBase
    {
        public SettingsUC()
        {
            this.InitializeComponent();
            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);
            this.DataContext = this;
        }

        #region Properties
        public string WindowTitel
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public bool SelectionExitAnswer
        {
            get => base.GetValue<bool>();
            set => base.SetValue(value, this.SetBoolSettingHandler);
        }

        public string NetworkScanner
        {
            get => base.GetValue<string>();
            set => base.SetValue(value,this.SetStringSettingHandler);
        }

        public string NetworkScannerRaw
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        #endregion Properties


        #region WindowEventHandler
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) == false)
            {
                this.WindowTitel = LocalizationValue.Get("WindowsTitelZeile");

                this.SelectionExitAnswer = App.Settings.FrageExit;
                this.NetworkScanner = App.Settings.NetworkScanner.Replace("_", string.Empty);
            }
        }
        #endregion WindowEventHandler

        private void SetBoolSettingHandler(bool arg1, string arg2)
        {
            if (arg2 == nameof(this.SelectionExitAnswer))
            {
                App.Settings.FrageExit = arg1;
            }
        }

        private void SetStringSettingHandler(string arg1, string arg2)
        {
            if (arg2 == nameof(this.NetworkScanner))
            {
                App.Settings.NetworkScanner = arg1.Replace("_",string.Empty);
            }
        }

        private long IPToInt(string addr)
        {
            // Beispiel IP-Adresse
            IPAddress ip = IPAddress.Parse(addr);

            // Bytes abrufen
            byte[] bytes = ip.GetAddressBytes();

            // In Integer konvertieren (Netzwerk-Byte-Reihenfolge beachten)
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        private string IntToIP(int addr)
        {
            // Integer zurück in Bytes wandeln
            byte[] bytes = BitConverter.GetBytes(addr);

            // Byte-Reihenfolge korrigieren, falls das System Little-Endian nutzt
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            // IPAddress-Objekt erstellen
            IPAddress ip = new IPAddress(bytes);

            return ip.ToString();
        }
    }
}
