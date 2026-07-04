namespace System.Windows
{
    using System.Windows.Controls;
    using System.Windows.Media;

    public class YesNoComboBox : ComboBox
    {
        static YesNoComboBox()
        {
            // Überschreibt die Metadaten der bestehenden SelectedValue-Property
            SelectedValueProperty.OverrideMetadata(typeof(YesNoComboBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedValuePropertyChanged)));
        }

        public YesNoComboBox()
        {
            // Die ComboBox fest mit "Ja" und "Nein" füllen
            this.Items.Add("Ja");
            this.Items.Add("Nein");

            // Standardmäßig nichts oder den ersten Wert auswählen (optional)
            this.SelectedIndex = -1;
            this.Width = 100;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            this.Height = 20;
            this.IsEditable = true;
            this.IsReadOnly = true; // Verhindert die Eingabe von benutzerdefiniertem Text
            this.FontWeight = FontWeights.Bold;
            this.DropDownOpened += (s, e) => { this.Foreground = Brushes.Black; };
        }

        // Registrierung der umbenannten Dependency Property (ResultSelection)
        public static readonly DependencyProperty ResultSelectionProperty =
            DependencyProperty.Register(
                nameof(ResultSelection),
                typeof(bool?),
                typeof(YesNoComboBox),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnResultSelectionChanged)));

        public bool? ResultSelection
        {
            get => (bool?)GetValue(ResultSelectionProperty);
            set => SetValue(ResultSelectionProperty, value);
        }

        private static void OnResultSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = (YesNoComboBox)d;
            bool? newValue = (bool?)e.NewValue;

            comboBox.UpdateSelectionByExternalValue(newValue);
        }

        // EVENT A: Wird ausgelöst, wenn sich die interne Auswahl ändert
        private static void OnSelectedValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = (YesNoComboBox)d;
            comboBox.UpdateVisualsAndStateBySelection(e.NewValue);
        }

        private void UpdateVisualsAndStateBySelection(object newVal)
        {
            // 1. Absicherung: Wenn der neue Wert null ist (nichts ausgewählt), ist das Ergebnis false
            if (newVal == null)
            {
                this.Foreground = Brushes.Black;
                this.ResultSelection = false;
                return;
            }

            // 2. Variante A: Prüfung, wenn die ComboBox Text/Strings enthält
            if (newVal is string selectedText)
            {
                if (selectedText.Equals("Ja", StringComparison.OrdinalIgnoreCase))
                {
                    this.Foreground = Brushes.Green; // Text wird grün bei "Ja"
                    this.ResultSelection = true;
                    return;
                }
                else if (selectedText.Equals("Nein", StringComparison.OrdinalIgnoreCase))
                {
                    this.Foreground = Brushes.Red;   // Text wird rot bei "Nein"
                    this.ResultSelection = false;
                    return;
                }
            }

            // Standard-Rückfallwert, falls kein Datentyp oben zutrifft
            this.Foreground = Brushes.Black;
            this.ResultSelection = false;
        }

        private void UpdateSelectionByExternalValue(bool? externalValue)
        {
            string targetText = externalValue == true ? "Ja" : "Nein";

            foreach (var item in this.Items)
            {
                string itemText = string.Empty;

                if (item is ComboBoxItem comboItem)
                    itemText = comboItem.Content?.ToString();
                else
                    itemText = item?.ToString();

                if (itemText != null && itemText.Trim().Equals(targetText, StringComparison.OrdinalIgnoreCase))
                {
                    if (this.SelectedItem != item)
                    {
                        this.SelectedItem = item;
                    }

                    return;
                }
            }
        }
    }
}
