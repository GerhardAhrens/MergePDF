namespace System.Windows
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Fluent DialogService für WPF.
    /// Unterstützt:
    /// - Konstruktorparameter
    /// - Fluent API
    /// - DialogResult Rückgabe
    /// - Owner Support
    /// - Multi-Monitor Centering
    /// </summary>
    /// <typeparam name="TWindow"></typeparam>
    public partial class DialogService<TWindow> where TWindow : Window
    {
        private readonly object[] _constructorParameters;

        private Action<TWindow> _configureAction;

        private Window _owner;

        private bool _centerToOwner;
        private bool _centerToScreen;

        #region Animation Fields

        private bool _enableFadeAnimation;

        private Duration _fadeInDuration = new Duration(TimeSpan.FromMilliseconds(250));

        private Duration _fadeOutDuration = new Duration(TimeSpan.FromMilliseconds(180));

        #endregion

        public DialogService(params object[] constructorParameters)
        {
            this._constructorParameters = constructorParameters;
        }

        #region Fluent API

        public DialogService<TWindow> Configure(Action<TWindow> configure)
        {
            this._configureAction += configure;
            return this;
        }

        public DialogService<TWindow> WithOwner(Window owner)
        {
            this._owner = owner;
            return this;
        }

        /// <summary>
        /// Zentriert relativ zum Owner.
        /// </summary>
        public DialogService<TWindow> CenterToOwner()
        {
            this._centerToOwner = true;
            this._centerToScreen = false;

            return this;
        }

        /// <summary>
        /// Zentriert auf aktuellem Monitor.
        /// </summary>
        public DialogService<TWindow> CenterToScreen()
        {
            this._centerToScreen = true;
            this._centerToOwner = false;

            return this;
        }

        public DialogService<TWindow> WithTitle(string title)
        {
            return Configure(w => w.Title = title);
        }

        public DialogService<TWindow> WithSize(double width, double height)
        {
            return Configure(w => { w.Width = width; w.Height = height; });
        }

        public DialogService<TWindow> WithResizeMode(ResizeMode resizeMode)
        {
            return Configure(w => w.ResizeMode = resizeMode);
        }

        public DialogService<TWindow> WithWindowStyle(WindowStyle style)
        {
            return Configure(w => w.WindowStyle = style);
        }

        public DialogService<TWindow> WithFont(string fontFamily)
        {
            return Configure(w => w.FontFamily = new FontFamily(fontFamily));
        }

        public DialogService<TWindow> WithFontWeight(FontWeight fontWeight)
        {
            return Configure(w => w.FontWeight = fontWeight);
        }

        public DialogService<TWindow> TopMost(bool topMost = true)
        {
            return Configure(w => w.Topmost = topMost);
        }

        /// <summary>
        /// Aktiviert Fade-In / Fade-Out Animation.
        /// </summary>
        public DialogService<TWindow> WithFadeAnimation(int fadeInMilliseconds = 150, int fadeOutMilliseconds = 180)
        {
            _enableFadeAnimation = true;

            _fadeInDuration = new Duration(TimeSpan.FromMilliseconds(fadeInMilliseconds));

            _fadeOutDuration = new Duration(TimeSpan.FromMilliseconds(fadeOutMilliseconds));

            return this;
        }

        #endregion

        #region Show Methods

        public DialogResponse<TWindow> Show()
        {
            var window = this.CreateWindow();

            bool isModal = IsModal(window);

            // Aktiviert den Topmost-Modus und holt das Fenster nach vorne
            window.Topmost = true;
            window.Show();
            window.Activate();
            window.Topmost = false;
            object resultObject = window.Tag; // Optional: Rückgabewert über Tag-Property

            return new DialogResponse<TWindow>(window, null, isModal, resultObject);
        }

        public DialogResponse<TWindow> ShowDialog()
        {
            var window = this.CreateWindow();
            bool isModal = IsModal(window);
            bool? result = window.ShowDialog();
            object resultObject = window.Tag; // Optional: Rückgabewert über Tag-Property

            return new DialogResponse<TWindow>(window, result, isModal, resultObject);
        }

        private static bool IsModal(Window window)
        {
            if (window.Owner != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region Window Creation

        private TWindow CreateWindow()
        {
            int countConstruktor = this._constructorParameters.Length;
            string constrktorParameterArgs = string.Join(", ", this._constructorParameters.Select(p => p.GetType()?.Name ?? "null"));

            try
            {
                var window = (TWindow)Activator.CreateInstance(typeof(TWindow), this._constructorParameters)!;

                this.ApplyDefaultSettings(window);

                this.ApplyOwner(window);

                this._configureAction?.Invoke(window);

                window.Loaded += (_, _) =>
                {
                    this.ApplyCentering(window);

                    if (this._enableFadeAnimation == true)
                    {
                        FadeIn(window);
                    }
                };

                if (this._enableFadeAnimation == true)
                {
                    this.AttachClosingAnimation(window);
                }

                return window;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Window vom Typ '{typeof(TWindow).Name}' konnte nicht erstellt werden. Konstruktorparameter: {countConstruktor}, Werte: {constrktorParameterArgs}", ex);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        private void ApplyDefaultSettings(TWindow window)
        {
            window.FontFamily = new FontFamily("Tahoma");
            window.FontWeight = FontWeights.Medium;

            window.Tag = window.GetType().Name;

            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.ResizeMode = ResizeMode.CanResizeWithGrip;
        }

        private void ApplyOwner(TWindow window)
        {
            if (_owner != null)
            {
                window.Owner = _owner;
            }
        }

        #endregion

        #region Multi Monitor Centering

        private void ApplyCentering(TWindow window)
        {
            if (_centerToOwner && window.Owner != null)
            {
                this.CenterWindowToOwner(window);
                return;
            }

            if (this._centerToScreen == true)
            {
                this.CenterWindowToCurrentScreen(window);
            }
        }

        /// <summary>
        /// Zentriert relativ zum Owner.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        private void CenterWindowToOwner(Window window)
        {
            Window owner = window.Owner!;

            window.Left = owner.Left + (owner.Width - window.ActualWidth) / 2;

            window.Top = owner.Top + (owner.Height - window.ActualHeight) / 2;
        }

        /// <summary>
        /// Zentriert auf dem aktuell aktiven Monitor.
        /// Multi-Monitor fähig.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        private void CenterWindowToCurrentScreen(Window window)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(window);

            Screen screen = Screen.FromHandle(helper.Handle);

            var workingArea = screen.WorkingArea;

            window.Left = workingArea.Left + (workingArea.Width - window.ActualWidth) / 2;

            window.Top = workingArea.Top + (workingArea.Height - window.ActualHeight) / 2;
        }

        #endregion

        #region Animations

        private void FadeIn(Window window)
        {
            window.Opacity = 0;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = _fadeInDuration
            };

            window.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void AttachClosingAnimation(Window window)
        {
            bool isClosingAnimated = false;

            window.Closing += (s, e) =>
            {
                if (isClosingAnimated == true)
                {
                    return;
                }

                e.Cancel = true;

                isClosingAnimated = true;

                var animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = _fadeOutDuration
                };

                animation.Completed += (_, _) =>
                {
                    window.Close();
                };

                window.BeginAnimation(UIElement.OpacityProperty, animation);
            };
        }

        #endregion

    }

    /// <summary>
    /// Rückgabeobjekt für ShowDialog()/.Show().
    /// </summary>
    /// <typeparam name="TWindow"></typeparam>
    public class DialogResponse<TWindow> where TWindow : Window
    {
        public TWindow Window { get; }

        public bool? DialogResult { get; }

        public bool IsModal { get; }

        public object ResponseObject { get; }

        public DialogResponse(TWindow window, bool? dialogResult, bool isModal, object responseObject)
        {
            this.Window = window;
            this.DialogResult = dialogResult;
            this.IsModal = isModal;
            this.ResponseObject = responseObject;
        }
    }
}
