//-----------------------------------------------------------------------
// <copyright file="PrintVariante.cs" company="Lifeprojects.de">
//     Class: PrintVariante
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>GERHARD-G6\gerha - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>07.07.2026</date>
//
// <summary>
// Varianten für das Drucken von PDF-Dateien
// </summary>
//-----------------------------------------------------------------------

namespace MergePDF.Core
{
    using System.ComponentModel;

    public enum PrintVariante : int
    {
        [Description("Keine Auswahl")]
        None = 0,
        [Description("Alle Seiten")]
        AllPages = 1,
        [Description("Einzelne Seiten")]
        SinglePages = 2,
        [Description("Seitenbereich")]
        RangePages = 3,
    }
}
