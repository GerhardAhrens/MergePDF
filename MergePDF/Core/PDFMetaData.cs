//-----------------------------------------------------------------------
// <copyright file="PDFMetaData.cs" company="Lifeprojects.de">
//     Class: PDFMetaData
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>GERHARD-G6\gerha - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>08.07.2026</date>
//
// <summary>
// Template für eine neue Enum-Klasse
// </summary>
// <Remarks>
// using (PdfDocument document = PdfReader.Open("beispiel.pdf", PdfDocumentOpenMode.Import))
// {
//  string title = document.Info.Title;
// }
// </Remarks>
//-----------------------------------------------------------------------

namespace MergePDF.Core
{
    public record PDFMetaData(string Title,string Author, string Subject, string Creator, string Producer,string CreationDate, string Keywords);
}
