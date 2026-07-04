namespace System.Windows
{
    using System.Drawing;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    internal static class NotificationBoxHelper
    {
        internal static ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        /// <summary>
        /// Tastaturkürzel werden in Windows verwendet, um einfache Schnellzugriffe auf Steuerelemente wie Schaltflächen und 
        /// Menüelemente zu ermöglichen. Dabei drückt der Benutzer die Alt-Taste, woraufhin eine Tastenkombination auf dem 
        /// Steuerelement hervorgehoben wird. Drückt der Benutzer diese Taste, wird das Steuerelement aktiviert.
        /// Diese Methode prüft, ob eine Zeichenfolge einen Tastaturbefehl enthält. Ist dies nicht der Fall, wird einer am
        /// Anfang der Zeichenfolge hinzugefügt. Wenn zwei Zeichenfolgen denselben Tastaturbefehl haben, übernimmt Windows die Handhabung.
        /// Das Tastaturbefehlszeichen für WPF ist der Unterstrich (_). Es ist nicht sichtbar.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string TryAddKeyboardAccellerator(this string input)
        {
            const string ACCELLERATOR = "_";            // This is the default WPF accellerator symbol - used to be & in WinForms

            // If it already contains an accellerator, do nothing
            if (input.Contains(ACCELLERATOR))
            {
                return input;
            }

            return ACCELLERATOR + input;
        }
    }
}
