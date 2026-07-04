//-----------------------------------------------------------------------
// <copyright file="MainWindow.cs" company="Lifeprojects.de">
//     Class: MainWindow
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>05.03.2026 18:21:36</date>
//
// <summary>
// Message Basis zur Kapselung der MessageBox Funktionalität
// </summary>
//-----------------------------------------------------------------------

namespace System.Windows
{
    using System.Diagnostics;
    using System.Runtime.Versioning;

    [DebuggerStepThrough]
    [Serializable]
    [SupportedOSPlatform("windows")]
    public class MessageBase : IMessageBase
    {
        public Window CurrentOwner { get; private set; }

        public MessageBoxResult ShowMessage(string titel, string message, MessageBoxButton mboxButton, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            MessageBoxResult result = MessageBox.Show(this.GetActiveWindow(), message, titel, mboxButton, icon, defaultResult, MessageBoxOptions.None);
            return result;
        }

        public MessageBoxResult ShowMessage(string titel, string message, bool withSound = false)
        {
            MessageBoxResult result;

            if (withSound == true)
            {
                System.Media.SystemSounds.Beep.Play();
            }

            result = MessageBox.Show(this.GetActiveWindow(), message, titel, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.None);

            return result;
        }

        public MessageBoxResult ShowMessage(string titel, string message)
        {
            MessageBoxResult result;

            result = MessageBox.Show(this.GetActiveWindow(), message, titel);

            return result;
        }

        private Window GetActiveWindow()
        {
            Window owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(f => f.IsActive == true);
            if (owner == null)
            {
                owner = Application.Current.MainWindow;
            }

            this.CurrentOwner = owner;

            return owner;
        }
    }
}
