namespace System.Windows
{
    using System.Collections.Specialized;
    using System.IO;

    public static class ClipboardHelper
    {
        public static void CutFilesToClipboard(string filePath)
        {
            // Prüfen, ob überhaupt Dateien in der Zwischenablage liegen
            if (Clipboard.ContainsData(DataFormats.FileDrop) == true)
            {
                Clipboard.Clear();
            }

            // 1. Die Datei(en) als StringCollection für das Clipboard definieren
            StringCollection files = new StringCollection { filePath };

            // 2. Den Effekt auf "Move" (Verschieben) setzen
            byte[] moveEffect = new byte[] { 2, 0, 0, 0 };
            MemoryStream dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);

            // 3. Das DataObject erzeugen und beide Informationen anhängen
            DataObject data = new DataObject();
            data.SetFileDropList(files);
            data.SetData("Preferred DropEffect", dropEffect);

            // 4. Daten an die Zwischenablage übergeben
            Clipboard.SetDataObject(data, true);
        }

        public static void CutFilesToClipboard(StringCollection files)
        {
            // Prüfen, ob überhaupt Dateien in der Zwischenablage liegen
            if (Clipboard.ContainsData(DataFormats.FileDrop) == true)
            {
                Clipboard.Clear();
            }

            // 2. Den Effekt auf "Move" (Verschieben) setzen
            byte[] moveEffect = new byte[] { 2, 0, 0, 0 };
            MemoryStream dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);

            // 3. Das DataObject erzeugen und beide Informationen anhängen
            DataObject data = new DataObject();
            data.SetFileDropList(files);
            data.SetData("Preferred DropEffect", dropEffect);

            // 4. Daten an die Zwischenablage übergeben
            Clipboard.SetDataObject(data, true);
        }

        public static void PasteFilesFromClipboard(string targetFolder)
        {
            // Prüfen, ob überhaupt Dateien in der Zwischenablage liegen
            if (Clipboard.ContainsData(DataFormats.FileDrop) == false)
            {
                return;
            }

            // Dateipfade auslesen (mit der neuen TryGetData<T>-API)
            if (!Clipboard.TryGetData<string[]>(DataFormats.FileDrop, out string[] filePaths) || filePaths == null)
            {
                return;
            }

            // Prüfen, ob es sich um "Ausschneiden" (Verschieben) handelt
            bool isCut = false;
            if (Clipboard.ContainsData("Preferred DropEffect"))
            {
                if (Clipboard.TryGetData<MemoryStream>("Preferred DropEffect", out MemoryStream stream) && stream != null)
                {
                    using (stream)
                    {
                        int effect = stream.ReadByte();
                        // 2 = Move, 5 = Move (Link/Sicherheitskopie)
                        if (effect == 2 || effect == 5)
                        {
                            isCut = true;
                        }
                    }
                }
            }

            // Zielverzeichnis erstellen, falls es nicht existiert
            Directory.CreateDirectory(targetFolder);

            foreach (string sourcePath in filePaths)
            {
                if (!File.Exists(sourcePath)) continue;

                string fileName = Path.GetFileName(sourcePath);
                string targetPath = Path.Combine(targetFolder, fileName);

                try
                {
                    if (isCut == true)
                    {
                        // Wenn die Datei am Ziel schon existiert, vorher löschen (File.Move erlaubt kein direktes Überschreiben)
                        if (File.Exists(targetPath) == true)
                        {
                            File.Delete(targetPath);
                        }

                        File.Move(sourcePath, targetPath);
                    }
                    else
                    {
                        // Normales Kopieren (Copy & Paste)
                        File.Copy(sourcePath, targetPath, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler bei Datei {fileName}: {ex.Message}");
                }
            }

            // Nach dem Ausschneiden und Verschieben die Zwischenablage leeren
            if (isCut == true)
            {
                Clipboard.Clear();
            }
        }
    }
}
