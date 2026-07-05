namespace System.Windows
{
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class ZoomComboBox : ComboBox
    {
        static ZoomComboBox()
        {
            // Überschreibt die Metadaten der bestehenden SelectedValue-Property
            SelectedValueProperty.OverrideMetadata(typeof(ZoomComboBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedValuePropertyChanged)));
        }

        public ZoomComboBox()
        {
            // Die ComboBox fest mit Zoom-Werten füllen
            this.Items.Add("50");
            this.Items.Add("70");
            this.Items.Add("80");
            this.Items.Add("90");
            this.Items.Add("100");
            this.Items.Add("110");
            this.Items.Add("120");
            this.Items.Add("130");
            this.Items.Add("150");

            // Standardmäßig nichts oder den ersten Wert auswählen (optional)
            this.SelectedIndex = 1;
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
                typeof(double),
                typeof(ZoomComboBox),
                new FrameworkPropertyMetadata(
                    100.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnResultSelectionChanged)));

        public double ResultSelection
        {
            get => (double)GetValue(ResultSelectionProperty);
            set => SetValue(ResultSelectionProperty, value);
        }

        private static void OnResultSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = (ZoomComboBox)d;
            double newValue = (double)e.NewValue;

            newValue = (double)e.NewValue == 0 ? 100 : (double)e.NewValue;

            comboBox.UpdateSelectionByExternalValue(newValue);
        }

        // EVENT A: Wird ausgelöst, wenn sich die interne Auswahl ändert
        private static void OnSelectedValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = (ZoomComboBox)d;
            comboBox.UpdateVisualsAndStateBySelection(e.NewValue);
        }

        private void UpdateVisualsAndStateBySelection(object newVal)
        {
            // 1. Absicherung: Wenn der neue Wert null ist (nichts ausgewählt), ist das Ergebnis false
            if (newVal == null)
            {
                this.Foreground = Brushes.Green;
                this.ResultSelection = 100;
                return;
            }

            // 2. Variante A: Prüfung, wenn die ComboBox Text/Strings enthält
            if (newVal is string selectedText)
            {
                double zoomValue = Convert.ToDouble(selectedText, CultureInfo.CurrentCulture);
                if (zoomValue < 100)
                {
                    this.Foreground = Brushes.Black; 
                    this.ResultSelection = zoomValue;
                    return;
                }
                else if (zoomValue == 100)
                {
                    this.Foreground = Brushes.Green;
                    this.ResultSelection = 100;
                    return;
                }
                else if (zoomValue > 100)
                {
                    this.Foreground = Brushes.Black;
                    this.ResultSelection = zoomValue;
                    return;
                }
            }

            // Standard-Rückfallwert, falls kein Datentyp oben zutrifft
            this.Foreground = Brushes.Green;
            this.ResultSelection = 100;
        }

        private void UpdateSelectionByExternalValue(double externalValue)
        {
            string targetText = externalValue.ToString(CultureInfo.CurrentCulture);

            foreach (var item in this.Items)
            {
                string itemText = string.Empty;

                if (item is ComboBoxItem comboItem)
                {
                    itemText = comboItem.Content?.ToString();
                }
                else
                {
                    itemText = item?.ToString();
                }

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
