//-----------------------------------------------------------------------
// <copyright file="FileNameGenerator.cs" company="Lifeprojects.de">
//     Class: FileNameGenerator
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>GERHARD-G6\gerha - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>26.07.2026</date>
//
// <summary>
// FileNameGenerator zum erstellen neuer Dateinamen
// </summary>
//-----------------------------------------------------------------------

namespace MergePDF.Core
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    public static class FileNameGenerator
    {
        /// <summary>
        /// Erzeugt einen eindeutigen Dateinamen.
        /// Vorlage: yyyyMMdd_Text_{000}.pdf
        /// </summary>
        /// <param name="directory">Zielverzeichnis</param>
        /// <param name="pattern">Namensvorlage</param>
        /// <returns>Vollständiger Dateiname</returns>
        public static string GetNextFileName(string directory, string pattern)
        {
            // Platzhalter suchen
            Match match = Regex.Match(pattern, @"\{(0+)\}");

            if (match.Success == false)
            {
                throw new ArgumentException("Die Vorlage enthält keinen gültigen Zählerplatzhalter.");
            }

            int digits = match.Groups[1].Value.Length;
            int max = (int)Math.Pow(10, digits) - 1;

            string placeholder = match.Value;

            for (int number = 1; number <= max; number++)
            {
                string fileName = pattern.Replace(placeholder, number.ToString(new string('0', digits)));

                string fullPath = Path.Combine(directory, fileName);

                if (File.Exists(fullPath) == false)
                {
                    return fullPath;
                }
            }

            // Falls alle Nummern vergeben sind:
            // erneut ab 1 beginnen und nach einer Lücke suchen.
            for (int number = 1; number <= max; number++)
            {
                string fileName = pattern.Replace(placeholder, number.ToString(new string('0', digits)));

                string fullPath = Path.Combine(directory, fileName);

                if (File.Exists(fullPath) == false)
                {
                    return fullPath;
                }
            }

            throw new IOException("Es konnte kein freier Dateiname gefunden werden.");
        }
    }
}
