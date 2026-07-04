namespace System.Windows
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Windows.Controls;
    using System.Windows.Media;

    using Microsoft.Win32;

    [DebuggerStepThrough]
    [Serializable]
    [SupportedOSPlatform("windows")]
    public class UserControlBase : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly ConcurrentDictionary<string, object> values = new();
        private readonly string className;
        private bool _IsPropertyChanged;
        private int rowPosition;

        public UserControlBase() : base()
        {
            this.className = this.GetType().Name;
            this.FontFamily = new FontFamily("Tahoma");
            this.FontWeight = FontWeights.Medium;
            this.className = this.GetType().Name;
        }

        public UserControlBase(Type inheritsType)
        {
            this.BaseType = inheritsType;
            this.className = this.GetType().Name;
            this.FontFamily = new System.Windows.Media.FontFamily("Tahoma");
            this.FontWeight = FontWeights.Normal;
        }

        public Type BaseType { get; set; }

        public bool IsUCLoaded { get; set; }

        public bool IsPropertyChanged
        {
            get { return this._IsPropertyChanged; }
            set
            {
                this._IsPropertyChanged = value;
                this.SetProperty(ref _IsPropertyChanged, value);
            }
        }

        public int RowPosition
        {
            get { return this.rowPosition; }
            set
            {
                this.rowPosition = value;
                this.SetProperty(ref rowPosition, value);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        public Version ApplicationVersion
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                if (string.IsNullOrWhiteSpace(fvi.FileVersion))
                {
                    return assembly.GetName().Version ?? new Version(1, 0, DateTime.Now.Year, 0);
                }

                if (Version.TryParse(fvi.FileVersion, out var parsed))
                {
                    return parsed;
                }

                return assembly.GetName().Version ?? new Version(1, 0, DateTime.Now.Year, 0);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        public string RuntimeVersion
        {
            get
            {
                string netVersion = $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
                string processArchitecture = $"{System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}";
                return $"{netVersion} ({processArchitecture})";
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        public string WindowsVersion
        {
            get
            {
                string osDescription = $"{System.Runtime.InteropServices.RuntimeInformation.OSDescription}";
                return $"{osDescription} ({GetWindowsVersionName()})";
            }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        #region Get/Set Implementierung
        private T GetPropertyValueInternal<T>(string propertyName)
        {
            if (values.ContainsKey(propertyName) == false)
            {
                values[propertyName] = default;
            }

            var value = values[propertyName];
            return value == null ? default : (T)value;
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = "")
        {
            var rightsKey = $"{this.className}.{propertyName}";

            return this.GetPropertyValueInternal<T>(propertyName);
        }

        protected void SetValueUnchecked<T>(T value, [CallerMemberName] string propertyName = "")
        {
            if (this.values.ContainsKey(propertyName) == true)
            {
                this.values[propertyName] = value;
            }
            else
            {
                this.values.TryAdd(propertyName, value);
            }
        }

        protected void SetValue<T>(T value, Func<T, string, bool> preAction, Action<T, string> postAction, [CallerMemberName] string propertyName = "")
        {
            if (preAction?.Invoke(value, propertyName) == true)
            {
                this.SetValue(value, propertyName);
            }

            if (postAction != null)
            {
                postAction?.Invoke(value, propertyName);
            }
        }

        protected void SetValue<T>(T value, Action<T, string> postAction, [CallerMemberName] string propertyName = "")
        {
            this.SetValue(value, propertyName);
            if (postAction != null)
            {
                postAction?.Invoke(value, propertyName);
            }
        }

        protected void SetValue<T>(T value, [CallerMemberName] string propertyName = "")
        {
            bool changed = !object.Equals(value, this.GetPropertyValueInternal<T>(propertyName));
            if (changed == true)
            {
                this.IsPropertyChanged = true;
                var rightsKey = $"{this.className}.{propertyName}";
                this.values[propertyName] = value;
                this.OnPropertyChanged(propertyName);
            }
        }
        #endregion Get/Set Implementierung

        #region INotifyPropertyChanged Implementierung
        protected void SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string property = "")
        {
            if (object.Equals(oldValue, newValue))
            {
                return;
            }

            oldValue = newValue;
            this.OnPropertyChanged(property);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged Implementierung

        #region Windows Productname ermittel
        private static string GetWindowsVersionName()
        {
            try
            {
                var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string currentBuildStr = (string)reg.GetValue("CurrentBuild");
                int currentBuild = int.Parse(currentBuildStr, System.Globalization.CultureInfo.CurrentCulture);
                if (currentBuild >= 22_000)
                {
                    return "Windows 11";
                }
                else if (currentBuild >= 10_240 && currentBuild < 22_000)
                {
                    return "Windows 10";
                }
                else
                {
                    return "Windows 7";
                }
            }
            catch (Exception ex)
            {
                string errorText = ex.Message;
                throw;
            }
        }
        #endregion Windows Productname ermittel

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod(int level = 1)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(level);

            return sf.GetMethod().Name;
        }

        internal void SetValue(string value, Action<int, string> refreshData)
        {
            throw new NotImplementedException();
        }
    }
}
