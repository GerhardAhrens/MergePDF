namespace MergePDF.Core
{
    using System.Windows;

    public sealed class ApplicationSettings : SettingsBase
    {
        public string Username { get; set; }
        public DateTime LetzterZugriff { get; set; }
        public bool FrageExit { get; set; }
        public string FileSuffix { get; set; }
        public string LastScanFolder { get; set; }
        public string IPAdresseScanner { get; set; }
    }
}
