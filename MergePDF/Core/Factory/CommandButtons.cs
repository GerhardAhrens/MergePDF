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
        [Description("PDF Datei splitten oder extrahieren")]
        PDFSplit = 4,
        [Description("PDF Dateien zusammenführen")]
        PDFMerge = 5,
        [Description("Ordner mit PDF Dateien öffnen")]
        OpenFolder = 6,
        [Description("Optionen zum PDF splitten und extrahieren löschen")]
        ResetInput = 7,
        [Description("PDF Drucken")]
        PDFPrint = 8,
        [Description("Informationen")]
        InformationPopup = 20,
        [Description("Einstellungen")]
        SettingsPopup = 21,
        [Description("Zurück zur vorherigen Dialog")]
        GoBack = 22,
        [Description("Eine Seite weiter")]
        PlusPage = 23,
        [Description("Eine Seite zurück")]
        MinusPage = 24,
        [Description("PDF speichern")]
        SavePDF = 25,
    }
}
