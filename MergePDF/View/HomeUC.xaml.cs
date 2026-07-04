//-----------------------------------------------------------------------
// <copyright file="HomeUC.cs" company="Lifeprojects.de">
//     Class: HomeUC
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>17.06.2026</date>
//
// <summary>
// Template für eine neues UserControl
// </summary>
//-----------------------------------------------------------------------

namespace MergePDF.View
{
    using System.Windows;
    using System.Windows.Controls;

    using MergePDF;

    using MergePDF.Core;

    /// <summary>
    /// Interaktionslogik für HomeUC.xaml
    /// </summary>
    public partial class HomeUC : UserControlBase
    {
        public HomeUC() : base(typeof(HomeUC))

        {
            this.InitializeComponent();
            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);

            this.QuitCommand = new CommandBase(commandParam => this.OnQuit(commandParam), () => true);
            this.MergePDFCommand = new CommandBase(commandParam => this.ChangeView(commandParam), () => true);
            this.SplitPDFCommand = new CommandBase(commandParam => this.ChangeView(commandParam), () => true);

            this.InformationCommand = new CommandBase(commandParam => this.OnPopup(commandParam));
            this.SettingsCommand = new CommandBase(commandParam => this.OnPopup(commandParam));
            this.CloseInformationPopupCommand = new CommandBase(commandParam => this.OnPopup(commandParam));
            this.CloseSettingsPopupCommand = new CommandBase(commandParam => this.OnPopup(commandParam));

            this.DataContext = this;
        }

        #region Properties
        public CommandBase QuitCommand { get; private set; }
        public CommandBase MergePDFCommand { get; private set; }
        public CommandBase SplitPDFCommand { get; private set; }

        public CommandBase InformationCommand { get; private set; }
        public CommandBase SettingsCommand { get; private set; }
        public CommandBase CloseInformationPopupCommand { get; private set; }
        public CommandBase CloseSettingsPopupCommand { get; private set; }

        public string WindowTitel
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        #endregion Properties

        #region Windows Events

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.WindowTitel = LocalizationValue.Get("WindowsTitelZeile");

            if (App.EventAgg.IsSubscription<StatusEvent>() == true)
            {
                await App.EventAgg.PublishAsync(new StatusEvent("Bereit"));
            }
        }
        #endregion Windows Events

        #region Command Events
        private async void OnQuit(object commandParam)
        {
            if (commandParam != null && commandParam is CommandButtons button)
            {
                if (button == CommandButtons.AppQuit)
                {
                    ChangeViewEventArgs args = new();
                    args.MenuButton = button;

                    if (App.EventAgg.IsSubscription<ChangeViewEventArgs>() == true)
                    {
                        await App.EventAgg.PublishAsync(args);
                    }
                }
            }
        }

        private void OnPopup(object commandParam)
        {
            CommandButtons cb = (CommandButtons)commandParam;

            switch (cb)
            {
                case CommandButtons.InformationPopup:
                    if (this.InformationPopup.IsOpen == false)
                    {
                        this.InformationPopup.SetValue(MaskLayerBehavior.IsOpenProperty, true);
                    }
                    else
                    {
                        this.InformationPopup.SetValue(MaskLayerBehavior.IsOpenProperty, false);
                    }

                    break;
                case CommandButtons.SettingsPopup:
                    if (this.SettingsPopup.IsOpen == false)
                    {
                        this.SettingsPopup.SetValue(MaskLayerBehavior.IsOpenProperty, true);
                    }
                    else
                    {
                        this.SettingsPopup.SetValue(MaskLayerBehavior.IsOpenProperty, false);
                    }
                    break;
            }
        }


        private async void ChangeView(object commandParam)
        {
            if (commandParam != null && commandParam is CommandButtons button)
            {

            }

        }
        #endregion Command Events

    }
}
