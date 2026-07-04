//-----------------------------------------------------------------------
// <copyright file="MainWindow.cs" company="Lifeprojects.de">
//     Class: MainWindow
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>05.03.2026 18:21:36</date>
//
// <summary>
// WPF Template mit Minimalfunktionen
// </summary>
//-----------------------------------------------------------------------

namespace System.Windows
{
    public static class IntegerExtensions
    {
        // Die 'this'-Anweisung vor 'int value' definiert den Typ, der erweitert wird
        public static bool In(this int value, params int[] allowedValues)
        {
            // Prüft, ob 'value' in der Menge 'allowedValues' enthalten ist
            return allowedValues.Contains(value);
        }

        public static bool NotIn(this int value, params int[] allowedValues)
        {
            // Prüft, ob 'value' in der Menge 'allowedValues' nicht enthalten ist
            return !allowedValues.Contains(value);
        }
    }

}