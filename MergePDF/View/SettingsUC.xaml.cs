namespace MergePDF.View
{
    using System.Windows;
    using System.Windows.Controls;

    using MergePDF;

    using MergePDF.Core;

    /// <summary>
    /// Interaktionslogik für SettingsUC.xaml
    /// </summary>
    public partial class SettingsUC : UserControlBase
    {
        public SettingsUC()
        {
            this.InitializeComponent();
            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);
            this.DataContext = this;
        }

        #region Properties
        public string WindowTitel
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public bool SelectionExitAnswer
        {
            get => base.GetValue<bool>();
            set => base.SetValue(value, this.SetBoolSettingHandler);
        }

        private ApplicationSettings Settings { get; set; }

        #endregion Properties


        #region WindowEventHandler
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) == false)
            {
                this.WindowTitel = LocalizationValue.Get("WindowsTitelZeile");

                this.Settings = App.Settings;
                this.SelectionExitAnswer = this.Settings.FrageExit;
            }
        }
        #endregion WindowEventHandler

        private void SetBoolSettingHandler(bool arg1, string arg2)
        {
            if (arg2 == nameof(this.SelectionExitAnswer))
            {
                App.Settings.FrageExit = arg1;
            }
        }

    }
}
