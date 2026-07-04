namespace System.Windows
{
    using System.Runtime.InteropServices;
    using System.Windows.Interop;

    /// <summary>
    /// Interaction logic for ModalDialog.xaml
    /// </summary>
    internal sealed partial class NotificationBoxView : Window
    {
        /* API Importe (GetWindowLong, SetWindowLong) */
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        #region Constructor

        internal NotificationBoxView(string caption, string message, MessageBoxButton button, MessageBoxImage image)
        {
            this.InitializeComponent();

            this.SourceInitialized += (s, e) =>
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                // API Aufruf zum Entfernen des Systemmenüs
                _ = SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            };

            this.Message = message;
            this.Caption = caption;
            this.Image_MessageBox.Visibility = Visibility.Collapsed;

            this.DisplayImage(image);
            this.DisplayButtons(button);
        }

        #endregion

        #region Properties

        internal string Caption
        {
            get
            {
                return this.Title;
            }
            set
            {
                this.Title = value;
            }
        }

        internal string Message
        {
            get
            {
                return this.TextBlock_Message.Text;
            }
            set
            {
                this.TextBlock_Message.Text = value;
            }
        }

        internal string OkButtonText
        {
            get
            {
                return this.Label_Ok.Content.ToString();
            }
            set
            {
                this.Label_Ok.Content = value.TryAddKeyboardAccellerator();
            }
        }

        internal string CancelButtonText
        {
            get
            {
                return this.Label_Cancel.Content.ToString();
            }
            set
            {
                this.Label_Cancel.Content = value.TryAddKeyboardAccellerator();
            }
        }

        internal string YesButtonText
        {
            get
            {
                return this.Label_Yes.Content.ToString();
            }
            set
            {
                this.Label_Yes.Content = value.TryAddKeyboardAccellerator();
            }
        }

        internal string NoButtonText
        {
            get
            {
                return this.Label_No.Content.ToString();
            }
            set
            {
                this.Label_No.Content = value.TryAddKeyboardAccellerator();
            }
        }

        public MessageBoxResult Result { get; set; }

        private MessageBoxButton CurrentButtons { get; set; }

        #endregion

        #region Methods

        private void DisplayButtons(MessageBoxButton button)
        {
            this.CurrentButtons = button;

            switch (button)
            {
                case MessageBoxButton.OKCancel:
                    // Hide all but OK, Cancel
                    this.Button_OK.Visibility = System.Windows.Visibility.Visible;
                    this.Button_OK.Focus();
                    this.Button_Cancel.Visibility = System.Windows.Visibility.Visible;

                    this.Button_Yes.Visibility = System.Windows.Visibility.Collapsed;
                    this.Button_No.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case MessageBoxButton.YesNo:
                    // Hide all but Yes, No
                    this.Button_Yes.Visibility = System.Windows.Visibility.Visible;
                    this.Button_Yes.Focus();
                    this.Button_No.Visibility = System.Windows.Visibility.Visible;

                    this.Button_OK.Visibility = System.Windows.Visibility.Collapsed;
                    this.Button_Cancel.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case MessageBoxButton.YesNoCancel:
                    // Hide only OK
                    this.Button_Yes.Visibility = System.Windows.Visibility.Visible;
                    this.Button_Yes.Focus();
                    this.Button_No.Visibility = System.Windows.Visibility.Visible;
                    this.Button_Cancel.Visibility = System.Windows.Visibility.Visible;

                    this.Button_OK.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case MessageBoxButton.AbortRetryIgnore:
                    // Hide only OK
                    this.Button_Yes.Visibility = System.Windows.Visibility.Visible;
                    this.Button_Yes.Focus();
                    this.Button_No.Visibility = System.Windows.Visibility.Visible;
                    this.Button_Cancel.Visibility = System.Windows.Visibility.Visible;

                    this.Button_OK.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                default:
                    // Hide all but OK
                    this.Button_OK.Visibility = System.Windows.Visibility.Visible;
                    this.Button_OK.Focus();

                    this.Button_Yes.Visibility = System.Windows.Visibility.Collapsed;
                    this.Button_No.Visibility = System.Windows.Visibility.Collapsed;
                    this.Button_Cancel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        private void DisplayImage(MessageBoxImage image)
        {
            Icon icon;

            switch (image)
            {
                case MessageBoxImage.None:
                    return;

                case MessageBoxImage.Exclamation:       // Enumeration value 48 - also covers "Warning"
                    icon = SystemIcons.Exclamation;
                    break;

                case MessageBoxImage.Error:             // Enumeration value 16, also covers "Hand" and "Stop"
                    icon = SystemIcons.Hand;
                    break;

                case MessageBoxImage.Information:       // Enumeration value 64 - also covers "Asterisk"
                    icon = SystemIcons.Information;
                    break;

                case MessageBoxImage.Question:
                    icon = SystemIcons.Question;
                    break;

                default:
                    icon = SystemIcons.Information;
                    break;
            }

            this.Image_MessageBox.Source = icon.ToImageSource();
            this.Image_MessageBox.Visibility = Visibility.Visible;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.OK;
            this.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentButtons == MessageBoxButton.AbortRetryIgnore)
            {
                this.Result = MessageBoxResult.Ignore;
            }
            else
            {
                this.Result = MessageBoxResult.Cancel;
            }

            this.Close();
        }

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentButtons == MessageBoxButton.AbortRetryIgnore)
            {
                this.Result = MessageBoxResult.Abort;
            }
            else
            {
                this.Result = MessageBoxResult.Yes;
            }

            this.Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentButtons == MessageBoxButton.AbortRetryIgnore)
            {
                this.Result = MessageBoxResult.Retry;
            }
            else
            {
                this.Result = MessageBoxResult.No;
            }

            this.Close();
        }

        #endregion

        #region Aufruf WIN 32 API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion Aufruf WIN 32 API
    }
}
