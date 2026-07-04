namespace MergePDF.View
{
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;

    using MergePDF;

    /// <summary>
    /// Interaktionslogik für InformationUC.xaml
    /// </summary>
    public partial class InformationUC : UserControlBase
    {
        public static readonly DependencyProperty IsParentOpenProperty =
    DependencyProperty.Register(nameof(IsParentOpen), typeof(bool), typeof(InformationUC), new PropertyMetadata(false, OnIsParentOpenChanged));

        public InformationUC()
        {
            this.InitializeComponent();
            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) == false)
            {
                this.WindowTitel = LocalizationValue.Get("WindowsTitelZeile");
                this.ApplikationVersion = base.ApplicationVersion.ToString();
                this.LaufzeitVersion = base.RuntimeVersion;
                this.WinVersion = base.WindowsVersion;
            }

            this.DataContext = this;
        }

        #region Properties
        public string WindowTitel
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public bool IsParentOpen
        {
            get { return (bool)GetValue(IsParentOpenProperty); }
            set { SetValue(IsParentOpenProperty, value); }
        }

        public string ApplikationVersion
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string LaufzeitVersion
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string WinVersion
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string InstallFolder
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string SettingsFolder
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string TotalRAM
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string FreeRAM
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public string UsedRAM
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
                /* hier ist Source, der im Design Mode nicht ausgeführt werden darf */
            }
        }
        #endregion WindowEventHandler

        private static void OnIsParentOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Hier optional Code ausführen, wenn sich der Zustand ändert
            var control = (InformationUC)d;
            bool popupIsOpen = (bool)e.NewValue;
            if (popupIsOpen == true)
            {
                string[] readRAM = control.ReadAllRAM();
                if (readRAM != null)
                {
                    control.TotalRAM = readRAM[0];
                    control.FreeRAM = readRAM[1];
                    control.UsedRAM = readRAM[2];
                }

                control.InstallFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                control.SettingsFolder = App.Settings.Pathname;
            }
        }

        private string[] ReadAllRAM()
        {
            string[] result = new string[] { string.Empty, string.Empty, string.Empty };
            ulong totalKb = 0;
            ulong freeKb = 0;
            long usedKb = 0;

            try
            {
                ObjectQuery wql = new ObjectQuery("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                if (searcher != null)
                {
                    foreach (ManagementObject resultMO in searcher.Get())
                    {
                        totalKb = (ulong)resultMO["TotalVisibleMemorySize"];
                        freeKb = (ulong)resultMO["FreePhysicalMemory"];
                    }
                }

                using (Process currentProcess = Process.GetCurrentProcess())
                {
                    // Der belegte Speicher in Bytes
                    usedKb = currentProcess.WorkingSet64;
                }

                result[0] = $"{totalKb / (1024.0 * 1024.0):N2} GB";
                result[1] = $"{freeKb / (1024.0 * 1024.0):N2} GB";
                result[2] = $"{usedKb / (1024.0 * 1024.0):N2} MB";

                return result;
            }
            catch (Exception ex)
            {
                string errorText = ex.Message;
                throw;
            }
        }
    }
}
