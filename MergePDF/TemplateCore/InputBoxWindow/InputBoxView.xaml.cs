namespace System.Windows
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;
    using System.Windows.Interop;

    /// <summary>
    /// Interaktionslogik für InputBox.xaml
    /// </summary>
    public partial class InputBoxView : Window
    {
        /* API Importe (GetWindowLong, SetWindowLong) */
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        private Type _targetType;
        private object _options;

        private TextBox _textBox;
        private CheckBox _checkBox;
        private DatePicker _datePicker;
        private bool _hasDateError;

        public object ResultValue { get; private set; }

        public InputBoxView()
        {
            this.InitializeComponent();
            this.ShowInTaskbar = false;
            this.SourceInitialized += (s, e) =>
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                // API Aufruf zum Entfernen des Systemmenüs
                _ = SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            };

        }

        public void Initialize<T>(InputBoxOptions<T> options)
        {
            this._options = options;
            this._targetType = typeof(T);

            this.Title = options.Title;
            this.TxtMessage.Text = options.Message;

            this.BtnOk.Content = options.OkButtonText;
            this.BtnCancel.Content = options.CancelButtonText;

            this.CreateInputControl(options);

            this.ValidateInput();
        }

        public void Initialize<T>(string message, T defaultValue = default)
        {
            this._targetType = typeof(T);

            this.TxtMessage.Text = message;

            this.BtnOk.Content = "Ok";
            this.BtnCancel.Content = "Abbrechen";

            this.CreateInputControl(defaultValue);

            this.ValidateInputDefault();
        }

        private void CreateInputControl<T>(InputBoxOptions<T> options)
        {
            if (this._targetType == typeof(bool))
            {
                this._checkBox = new CheckBox
                {
                    IsChecked = (bool?)(object)options.DefaultValue ?? false
                };

                this.InputHost.Content = _checkBox;
                this._checkBox.Focus();

                return;
            }

            if (this._targetType == typeof(DateTime?))
            {
                if (options.DefaultValue == null)
                {
                    this._datePicker = new DatePicker
                    {
                        SelectedDate = null
                    };
                }
                else
                {
                    this._datePicker = new DatePicker
                    {
                        SelectedDate = (DateTime?)(object)options.DefaultValue
                    };
                }


                this._datePicker.DateValidationError += this.DatePicker_DateValidationError;
                this._datePicker.SelectedDateChanged += (_, _) => this.ValidateInput();
                this._datePicker.PreviewTextInput += (_, _) => this.ValidateInput();

                InputHost.Content = this._datePicker;
                this._datePicker.Focus();

                return;
            }

            this._textBox = new TextBox();

            if (options.DefaultValue != null)
            {
                if (!string.IsNullOrWhiteSpace(options.FormatString))
                {
                    this._textBox.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + options.FormatString + "}", options.DefaultValue);
                }
                else
                {
                    this._textBox.Text = options.DefaultValue.ToString();
                }
            }

            this._textBox.TextChanged += (_, _) => this.ValidateInput();
            this._textBox.Focus();

            InputHost.Content = this._textBox;
        }

        private void CreateInputControl(object defaultValue)
        {
            if (this._targetType == typeof(bool))
            {
                this._checkBox = new CheckBox
                {
                    IsChecked = defaultValue as bool? ?? false
                };

                InputHost.Content = this._checkBox;
                this._checkBox.Focus();
                return;
            }

            if (this._targetType == typeof(DateTime?))
            {
                if (defaultValue == null)
                {
                    this._datePicker = new DatePicker
                    {
                        SelectedDate = null
                    };
                }
                else
                {
                    this._datePicker = new DatePicker
                    {
                        SelectedDate = (DateTime?)(object)defaultValue
                    };
                }

                this._datePicker.DateValidationError += this.DatePicker_DateValidationError;
                this._datePicker.SelectedDateChanged += (_, _) => this.ValidateInputDefault();
                this._datePicker.PreviewTextInput += (_, _) => this.ValidateInputDefault();

                InputHost.Content = this._datePicker;
                this._datePicker.Focus();
                return;
            }

            this._textBox = new TextBox();

            if (defaultValue != null)
            {
                this._textBox.Text = defaultValue.ToString();
            }

            this._textBox.TextChanged += (_, _) => this.ValidateInputDefault();
            this._textBox.Focus();

            InputHost.Content = this._textBox;
        }

        private void DatePicker_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
        {
            this._hasDateError = true;
            e.ThrowException = false;
        }

        private void ValidateInput()
        {
            this.TxtError.Text = string.Empty;

            dynamic options = this._options!;

            if (this._targetType == typeof(string))
            {
                if (options.IsRequired && string.IsNullOrWhiteSpace(this._textBox?.Text))
                {
                    this.TxtError.Text = "Eingabe erforderlich.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }
            }

            if (this._targetType == typeof(int))
            {
                if (!int.TryParse(this._textBox?.Text, out int value))
                {
                    this.TxtError.Text = "Ungültige Ganzzahl.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }

                if (options.MinInt != null && value < options.MinInt)
                {
                    this.TxtError.Text = $"Wert muss >= {options.MinInt} sein.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }

                if (options.MaxInt != null && value > options.MaxInt)
                {
                    this.TxtError.Text = $"Wert muss <= {options.MaxInt} sein.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }
            }

            if (this._targetType == typeof(double))
            {
                if (!double.TryParse(this._textBox?.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out double value))
                {
                    this.TxtError.Text = "Ungültige Zahl.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }

                if (options.MinDouble != null && value < options.MinDouble)
                {
                    this.TxtError.Text = $"Wert muss >= {options.MinDouble} sein.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }

                if (options.MaxDouble != null && value > options.MaxDouble)
                {
                    this.TxtError.Text = $"Wert muss <= {options.MaxDouble} sein.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }
            }

            if (_targetType == typeof(DateTime?))
            {
                if (this._hasDateError)
                {
                    TxtError.Text = "Ungültiges Datum.";
                    BtnOk.IsEnabled = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(_datePicker?.Text) == false)
                {
                    bool validDate = DateTime.TryParse(_datePicker.Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsedDate);

                    if (!validDate)
                    {
                        this.TxtError.Text = "Ungültiges Datum.";
                        this.BtnOk.IsEnabled = false;
                        return;
                    }

                    if (options.MinDate != null && parsedDate < options.MinDate)
                    {
                        this.TxtError.Text = $"Datum ab {options.MinDate:d}";
                        this.BtnOk.IsEnabled = false;
                        return;
                    }

                    if (options.MaxDate != null && parsedDate > options.MaxDate)
                    {
                        this.TxtError.Text = $"Datum bis {options.MaxDate:d}";
                        this.BtnOk.IsEnabled = false;
                        return;
                    }
                }
                else
                {
                    if (options.IsRequired)
                    {
                        TxtError.Text = "Datum auswählen.";
                        BtnOk.IsEnabled = false;
                        return;
                    }
                }
            }
            this.BtnOk.IsEnabled = true;
        }

        private void ValidateInputDefault()
        {
            this.TxtError.Text = string.Empty;

            bool valid = true;

            if (this._targetType == typeof(string))
            {
                valid = true;
            }
            else if (this._targetType == typeof(int))
            {
                valid = int.TryParse(_textBox?.Text, out _);

                if (!valid)
                {
                    this.TxtError.Text = "Bitte eine gültige Ganzzahl eingeben.";
                }
            }
            else if (this._targetType == typeof(double))
            {
                valid = double.TryParse(this._textBox?.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out _);

                if (!valid)
                {
                    this.TxtError.Text = "Bitte eine gültige Dezimalzahl eingeben.";
                }
            }
            else if (this._targetType == typeof(bool))
            {
                valid = true;
            }
            else if (this._targetType == typeof(DateTime?))
            {
                if (this._hasDateError)
                {
                    TxtError.Text = "Ungültiges Datum.";
                    BtnOk.IsEnabled = false;
                    return;
                }

                if (string.IsNullOrEmpty(this._datePicker.Text) == true)
                {
                    this._datePicker?.SelectedDate = null;
                    this.TxtError.Text = "Bitte ein Datum auswählen.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }

                bool validDate = DateTime.TryParse(this._datePicker.Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsedDate);

                if (validDate == false)
                {
                    this._datePicker?.SelectedDate = null;
                    this.TxtError.Text = "Ungültiges Datum.";
                    this.BtnOk.IsEnabled = false;
                    return;
                }
            }

            this.BtnOk.IsEnabled = valid;
        }

        private object ConvertValue()
        {
            if (this._targetType == typeof(string))
            {
                return this._textBox?.Text;
            }

            if (this._targetType == typeof(int))
            {
                return int.Parse(this._textBox!.Text, CultureInfo.CurrentCulture);
            }

            if (this._targetType == typeof(double))
            {
                return double.Parse(this._textBox!.Text, CultureInfo.CurrentCulture);
            }

            if (this._targetType == typeof(bool))
            {
                return this._checkBox?.IsChecked ?? false;
            }

            if (this._targetType == typeof(DateTime))
            {
                return this._datePicker?.SelectedDate;
            }

            return null;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.ResultValue = ConvertValue();
            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        #region Aufruf WIN 32 API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion Aufruf WIN 32 API
    }
}
