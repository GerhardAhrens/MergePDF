namespace MergePDF.Core
{
    using System.Diagnostics;

    using WIA;

    [DebuggerDisplay("Name: {this.Name}")]
    public class ScannerInfo
    {
        public string Name { get; set; } = "";
        public DeviceInfo DeviceInfo { get; set; } = null!;


        public override string ToString()
        {
            return $"{this.Name}";
        }
    }
}
