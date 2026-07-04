namespace MergePDF.Core
{
    using System.ComponentModel;

    public enum CommandButtons
    {
        [Description("Keine Auswahl")]
        None = 0,
        [Description("Anwendung beenden")]
        AppQuit = 1,
        [Description("Startseite")]
        Home = 2,
        [Description("Hilfe")]
        Help = 3,
        [Description("PDF Split")]
        PDFSplit = 4,
        [Description("PDF Merge")]
        PDFMerge = 5,
        [Description("Informationen")]
        InformationPopup = 20,
        [Description("Einstellungen")]
        SettingsPopup = 21,
        [Description("Zurück zur vorherigen Seite")]
        GoBack = 22,
    }
}
