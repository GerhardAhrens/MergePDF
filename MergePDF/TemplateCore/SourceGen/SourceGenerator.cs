namespace System.Windows
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Windows.Resources;

    internal static class SourceGenerator
    {
        static SourceGenerator()
        {
            if (Directory.Exists(TemplatePath) == false)
            {
                Directory.CreateDirectory(TemplatePath);
            }
            else
            {
                foreach (string filePath in Directory.EnumerateFiles(TemplatePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        public static string TemplatePath { get; private set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template");

        public static void CreateSourceFile(string className, string newClassName)
        {
            string rootNamespace = AppDomain.CurrentDomain.FriendlyName;
            StringCollection files = new StringCollection();

            (string, string) sources = GetSourceFromResources(className);
            if (string.IsNullOrEmpty(sources.Item1) == false && string.IsNullOrEmpty(sources.Item2) == false)
            {
                string csFilePath = Path.Combine(TemplatePath, $"{newClassName}.xaml.cs");
                if (string.IsNullOrEmpty(sources.Item1) == false)
                {
                    string csContent = sources.Item1.Replace("[[ClassName]]", newClassName).Replace("[[RootNamespace]]", $"{rootNamespace}.Beispiel");
                    File.WriteAllText(csFilePath, csContent);
                    files.Add(csFilePath);
                }

                string xamlFilePath = Path.Combine(TemplatePath, $"{newClassName}.xaml");
                if (string.IsNullOrEmpty(sources.Item2) == false)
                {
                    string xamlContent = sources.Item2.Replace("[[ClassName]]", newClassName).Replace("[[RootNamespace]]", $"{rootNamespace}.Beispiel");
                    File.WriteAllText(xamlFilePath, xamlContent);
                    files.Add(xamlFilePath);
                }
            }
            else
            {
                string csFilePath = Path.Combine(TemplatePath, $"{className}.cs");
                string csContent = sources.Item1.Replace("[[ClassName]]", newClassName).Replace("[[RootNamespace]]", $"{rootNamespace}.Beispiel");
                File.WriteAllText(csFilePath, csContent);
                files.Add(csFilePath);
            }

            if (files.Count > 0)
            {
                ClipboardHelper.CutFilesToClipboard(files);
            }
        }

        public static (string, string) GetSourceFromResources(string className)
        {
            Uri uriXAMLCS;
            Uri uriXAML;
            Uri uriCS;
            string outCodeCS = string.Empty;
            string outCodeXAML = string.Empty;

            uriXAMLCS = new Uri($"pack://application:,,,/Resources/Source/{className}.xaml.cs.source", UriKind.Absolute);
            if (DoesResourceExist(uriXAMLCS) == true)
            {
                StreamResourceInfo sri = Application.GetResourceStream(uriXAMLCS);
                using StreamReader reader = new StreamReader(sri.Stream);
                outCodeCS = reader.ReadToEnd();

                outCodeCS = new ReplaceContent().Replace(outCodeCS);
            }

            uriCS = new Uri($"pack://application:,,,/Resources/Source/{className}.cs.source", UriKind.Absolute);
            if (DoesResourceExist(uriCS) == true)
            {
                StreamResourceInfo sri = Application.GetResourceStream(uriCS);
                using StreamReader reader = new StreamReader(sri.Stream);
                outCodeCS = reader.ReadToEnd();

                outCodeCS = new ReplaceContent().Replace(outCodeCS);
            }

            uriXAML = new Uri($"pack://application:,,,/Resources/Source/{className}.xaml.source", UriKind.Absolute);
            if (DoesResourceExist(uriXAML) == true)
            {
                StreamResourceInfo sri = Application.GetResourceStream(uriXAML);
                using StreamReader reader = new StreamReader(sri.Stream);
                outCodeXAML = reader.ReadToEnd();
            }


            return (outCodeCS, outCodeXAML);
        }

        public static bool DoesResourceExist(Uri resourceUri)
        {
            try
            {
                // Versucht, den Stream der Ressource abzurufen
                var resourceStream = Application.GetResourceStream(resourceUri);

                // Wenn kein Fehler auftritt und der Stream existiert
                return resourceStream != null;
            }
            catch (IOException)
            {
                // FileNotFoundException (bzw. IOException in WPF) wird geworfen, wenn die Ressource fehlt
                return false;
            }
            catch (ArgumentException)
            {
                // Tritt auf, wenn die Uri fehlerhaft oder ungültig ist
                return false;
            }
        }
    }

    internal sealed class ReplaceContent
    {
        private const string FIRMA = "Lifeprojects.de";
        private const string FULLNAME = "Gerhard Ahrens";
        private const string EMAIL = "developer@lifeprojects.de";

        private readonly List<ReplaceValues> _replaceValues;
        public ReplaceContent()
        {
            this._replaceValues = new List<ReplaceValues>();
            this._replaceValues.Add(new ReplaceValues() { Placeholder = "$firma$", Value = FIRMA });
            this._replaceValues.Add(new ReplaceValues() { Placeholder = "$company$", Value = FIRMA });
            this._replaceValues.Add(new ReplaceValues() { Placeholder = "$name$", Value = FULLNAME });
            this._replaceValues.Add(new ReplaceValues() { Placeholder = "$email$", Value = EMAIL });
            this._replaceValues.Add(new ReplaceValues() { Placeholder = "2026", Value = DateTime.Now.Year.ToString(CultureInfo.CurrentCulture) });
            this._replaceValues.Add(new ReplaceValues() { Placeholder = "$date$", Value = DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) });
        }

        public string Replace(string content)
        {
            if (this._replaceValues != null && this._replaceValues.Count > 0)
            {
                foreach (ReplaceValues item in this._replaceValues)
                {
                    content = content.Replace(item.Placeholder, item.Value, StringComparison.OrdinalIgnoreCase);
                }
            }

            return content;
        }

        private sealed class ReplaceValues
        {
            public string Placeholder { get; set; }
            public string Value { get; set; }
        }
    }

    public enum ClassTypes
    {
        [Description("Keine Auswahl")]
        None = 0,
        [Description("Erstelle UserControl Class mit .xaml und .xaml.cs Datei")]
        UserControlClass = 1,
        [Description("Erstellet eine Window Class mit .xaml und .xaml.cs Datei")]
        WindowClass = 2,
        [Description("Erstellen einer 'enum class'")]
        EnumClass = 3,
        [Description("Erstellen einer 'public class'")]
        PublicClass = 4,
        [Description("Erstellen einer 'public static class'")]
        StaticPublicClass = 5,
        [Description("Erstellen einer 'public static class' die für Extenstion verwendet wird")]
        PublicExtensionClass = 6,
        [Description("Erstellen einer 'public disposable class'")]
        PublicDisposableClass = 7,
    }
}
