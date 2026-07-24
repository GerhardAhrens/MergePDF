namespace MergePDF.Core
{
    using System.Diagnostics;

    using WIA;

    [DebuggerDisplay("Name: {this.Name}")]
    public class ScannerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string UniqueDeviceID { get; set; } = string.Empty;
        public DeviceInfo DeviceInfo { get; set; } = null!;


        public override string ToString()
        {
            return $"{this.Name}";
        }
    }
}
