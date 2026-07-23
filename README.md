# PDF Split und Merge

![NET](https://img.shields.io/badge/NET-10-green.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![VS2026](https://img.shields.io/badge/Visual%20Studio-2026-white.svg)
![Version](https://img.shields.io/badge/Version-1.0.2026.0-yellow.svg)

# Projekt
Das Projekt dient dazu, PDF Dateien zu splitten, zusammenzufügen, Drucken und Scannen.
Der Fokus liegt aber auf der Möglichkeit, PDF Dokumente zu splitten und zusammenzufügen.

![Hauptdialog](MainWindow.png)

## Splitten von PDF Dateien

Für das splitten von PDF Dateien stehen verschiedene Möglichkeiten zur Verfügung. Es können einzelne Seiten, ein Bereich von Seiten oder jede Seite in eine eigene Datei gespeichert werden.<br>
Eine weite Funktion ist, das aus einem bestehenden PDF Dokument, ein Bereich von Seiten extrahiert und in einem neuen PDF Dokument gespeichert werden kann.
![Merge Dialog](SplitPDFView.png)

## Zusammenführen von PDF Dateien
Es können einzelne PDF Dateien in eine neue PDF Datei zusammengeführt werden. Dabei können die einzelnen PDF Dateien in der Reihenfolge sortiert werden, wie sie im neuen PDF Dokument erscheinen sollen.

![Merge Dialog](MergePDFView.png)

## Drucken von PDF Dokumenten

![Print Dialog](PrintPDF.png)

## Scannen von PDF Dokumenten

# Hinweis
Ein bearbeiten der PDF Datei ist mit diesem Tool nicht möglich. Es können nur PDF Dateien zusammengeführt, gesplittet oder extrahiert werden werden.
# zusätzliche NuGet-Pakete
In der Anwendung/Demo werden folgende zusätzliche Pakete verwendet

|NuGet-Paket|Lizenz|Beschreibung|
|:------|:--|:-----------|
|PDFiumCore|Apache License 2.0|PDFiumCore ist eine .NET-Bibliothek zum Rendern und Bearbeiten von PDF-Dokumenten.|
|PdfSharpCore|MIT|PdfSharpCore ist eine .NET-Bibliothek zum Bearbeiten von PDF-Dokumenten.|

![Version](https://img.shields.io/badge/Version-1.0.2026.8-yellow.svg)
- Erste Version
