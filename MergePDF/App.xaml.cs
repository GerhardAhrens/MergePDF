//-----------------------------------------------------------------------
// <copyright file="App.cs" company="Lifeprojects.de">
//     Class: App
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>gerhard.ahrens@lifeprojects.de</email>
// <date>04.06.2026</date>
//
// <summary>
// Startklasse App.cs
// </summary>
//-----------------------------------------------------------------------

namespace MergePDF
{
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Threading;

    using MinimalWPF.Core;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string DEFAULTLANGUAGE = "de-DE";
        public const string SHORTNAME = "PDF_Split_Merge";
        private static readonly string MessageBoxTitle = "PDF Split & Merge";
        private static readonly string UnexpectedError = "An unexpected error occured.";
        private string exePath = string.Empty;
        private string exeName = string.Empty;

        /// <summary>
        /// Initialisiert eine neue Instanz der App-Klasse und richtet globale Anwendungsressourcen sowie
        /// Fehlerbehandlung ein.
        /// </summary>
        /// <remarks>Der Konstruktor legt den Namen und Pfad der ausführbaren Datei fest, konfiguriert die
        /// Synchronisierung von Texteingaben für Windows-Kompatibilität und registriert einen globalen Handler für
        /// nicht abgefangene Ausnahmen auf Dispatcher-Ebene. Dies stellt sicher, dass unerwartete Fehler zentral
        /// behandelt und angezeigt werden.</remarks>
        public App()
        {
            try
            {
                /* Name der EXE Datei*/
                exeName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
                /* Pfad der EXE-Datei*/
                exePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

                /* Synchronisieren einer Textenigabe mit dem primären Windows (wegen Validierung von Eingaben)*/
                FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;

                /* Alle nicht explicit abgefangene Exception spätesten hier abfangen und anzeigen */
                this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;
            }
            catch (Exception ex)
            {
                ex.Data.Add("UserDomainName", Environment.UserDomainName);
                ex.Data.Add("UserName", Environment.UserName);
                ex.Data.Add("exePath", exePath);
                ErrorMessage(ex, "General Error: ");
                ApplicationExit();
            }
        }

        /// <summary>
        /// Bereitstellung des Event Aggregators als statische Eigenschaft, damit er in der gesamten Anwendung verwendet werden kann, 
        /// um lose gekoppelte Kommunikation zwischen verschiedenen Komponenten zu ermöglichen, z.B. um Ereignisse zu veröffentlichen 
        /// und zu abonnieren, ohne direkte Abhängigkeiten zwischen den Komponenten zu schaffen.
        /// </summary>
        public static EventAggregator EventAgg { get; } = new();

        /// <summary>
        /// Statische Eigenschaft für die globalen Einstellungen der Anwendung, hier können alle Einstellungen gespeichert werden, 
        /// die in der gesamten Anwendung benötigt werden, z.B. Spracheinstellung, Benutzername, etc.
        /// </summary>
        public static ApplicationSettings Settings { get; set; }

        /// <summary>
        /// Verwaltet die Startlogik der Anwendung, einschließlich der Initialisierung der Ländereinstellungen und der Konfiguration der Benutzereinstellungen.
        /// </summary>
        /// <remarks>
        /// In Debug-Builds werden zusätzliche Trace-Listener für die Datenbindung, weitergeleitete Ereignisse und Ressourcenverzeichnisse 
        /// konfiguriert, um die Fehlersuche zu erleichtern.Ausnahmen während des Startvorgangs werden abgefangen und behandelt,
        /// um eine korrekte Fehlermeldung und das ordnungsgemäße Herunterfahren der Anwendung sicherzustellen.
        /// </remarks>
        /// <param name="e">Ein Objekt, das die Ereignisdaten für das Startereignis enthält.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
#if DEBUG
                PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
                PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
                //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical | SourceLevels.Error | SourceLevels.Warning;
                //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.All;
                PresentationTraceSources.RoutedEventSource.Listeners.Add(new ConsoleTraceListener());
                PresentationTraceSources.RoutedEventSource.Switch.Level = SourceLevels.All;
                PresentationTraceSources.ResourceDictionarySource.Listeners.Add(new ConsoleTraceListener());
                PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.All;
#endif

                /* Initalisierung Spracheinstellung */
                InitializeCultures(DEFAULTLANGUAGE);

                /* Initiale Benutzer Einstellungen speichern */
                InitializeSettings();
            }
            catch (Exception ex)
            {
                string errorText = ex.Message;
                ErrorMessage(ex, "General Error: ");
                ApplicationExit();
            }
        }

        /// <summary>
        /// Beispiel für das Speichern von Einstellungen beim Beenden der Anwendung, hier wird nur die letzte Zugriffzeit aktualisiert
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Settings.LetzterZugriff = DateTime.Now;

            using (ApplicationSettings settings = new ApplicationSettings())
            {
                if (settings.IsExitSettings() == true)
                {
                    settings.Load();
                    settings.SetSetting(Settings);
                    settings.Save();
                }
            }
        }

        /// <summary>
        /// Initialisiert die Kultur des aktuellen Threads und die UI-Kultur auf der Grundlage des angegebenen Sprachcodes.
        /// </summary>
        /// <remarks>
        /// Diese Methode aktualisiert zudem die Standardsprache für WPF-Elemente, indem sie die Metadaten der „Language“-Eigenschaft 
        /// für „FrameworkElement“ überschreibt. Dadurch wird sichergestellt, dass die Ressourcensuche und die Formatierung
        /// in der gesamten Benutzeroberfläche der Anwendung mit der angegebenen Kultur übereinstimmen.
        /// </remarks>
        /// <param name="language">Eine Zeichenfolge, die das IETF-Sprachkennzeichen (z. B. „en-US“) enthält, das als aktuelle 
        /// Kultur und UI-Kultur festgelegt werden soll. Ist der Wert null oder leer, wird die Standardsprache verwendet.
        /// </param>
        private static void InitializeCultures(string language)
        {
            if (string.IsNullOrEmpty(language) == false)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(DEFAULTLANGUAGE);
            }

            if (string.IsNullOrEmpty(language) == false)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(DEFAULTLANGUAGE);
            }

            FrameworkPropertyMetadata frameworkMetadata = new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(new CultureInfo(language).IetfLanguageTag));
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), frameworkMetadata);
        }

        /// <summary>
        /// Erstellen von Standardeinstellungen, hier wird nur der Benutzername und die letzte Zugriffzeit gespeichert, weitere Einstellungen können natürlich hinzugefügt werden
        /// </summary>
        private static void InitializeSettings()
        {
            using (ApplicationSettings settings = new ApplicationSettings())
            {
                if (settings.IsExitSettings() == false)
                {
                    settings.Username = $"{Environment.UserDomainName}\\{Environment.UserName}";
                    settings.LetzterZugriff = DateTime.Now;
                    settings.FrageExit = true;
                    settings.Save();
                }
                else
                {
                    settings.Load();
                }

                Settings = settings;
            }
        }

        /// <summary>
        /// Darstellen einer Fehlermeldung mit eventueller Exception, die Applikationsweit verwendet werden kann, 
        /// um Fehler an den Benutzer anzuzeigen, z.B. bei einem unerwarteten Fehler, etc.
        /// </summary>
        /// <param name="ex">Die Exception, die angezeigt werden soll.</param>
        /// <param name="message">Eine optionale Nachricht, die zusammen mit der Exception angezeigt wird.</param>
        /// <param name="cancelByException">Gibt an, ob die Anwendung bei dieser Exception abgebrochen werden soll.</param>
        public static void ErrorMessage(Exception ex, string message = "", bool cancelByException = true)
        {
            string expMsg = ex.Message;
            var aex = ex as AggregateException;

            if (aex != null && aex.InnerExceptions.Count == 1)
            {
                expMsg = aex.InnerExceptions[0].Message;
            }

            if (string.IsNullOrEmpty(message) == true)
            {
                message = UnexpectedError;
            }

            StringBuilder errorText = new StringBuilder();
            if (ex.Data != null && ex.Data.Count > 0)
            {
                foreach (DictionaryEntry item in ex.Data)
                {
                    errorText.AppendLine(CultureInfo.CurrentCulture, $"{item.Key} : {item.Value}");
                }
            }

            System.Media.SystemSounds.Hand.Play();

            MessageBox.Show(
                message + $"{expMsg}\n{ex.Message}\n{errorText.ToString()}",
                MessageBoxTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            if (cancelByException == true)
            {
                Application.Current.Shutdown();
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Info Message Box. die Applikationsweit verwendet werden kann, 
        /// um Informationen an den Benutzer anzuzeigen, z.B. nach einem erfolgreichen Speichern oder einer erfolgreichen Aktion, etc.
        /// </summary>
        /// <param name="message"></param>
        public static void InfoMessage(string message)
        {
            MessageBox.Show(
                message,
                MessageBoxTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        /// <summary>
        /// Screen zum aktualisieren zwingen, Globale Funktion
        /// </summary>
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        /// <summary>
        /// Behandelt das Ereignis „DispatcherUnhandledException“, das auftritt, wenn im UI-Thread der Anwendung eine nicht abgefangene Ausnahme ausgelöst wird.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses, in der Regel der Anwendungs-Dispatcher.</param>
        /// <param name="e">Ein `DispatcherUnhandledExceptionEventArgs`-Objekt, das die Ereignisdaten enthält, einschließlich der nicht behandelten Ausnahme.</param>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"{exeName}-{(e.Exception as Exception).Message}");
        }

        /// <summary>
        /// Programmende erzwingen
        /// </summary>
        public static void ApplicationExit()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Application.Current.Shutdown(0);
        }
    }
}
