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
// WPF Template mit Minimalfunktionen
// </summary>
//-----------------------------------------------------------------------

namespace System.Windows
{

    public static class MessageExtension
    {
        public static MessageBoxResult Hinweis(this IMessageBase self, string titel, string message, bool withSound = false)
        {
            MessageBoxResult result = self.ShowMessage(titel, message, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
            return result;
        }

        public static MessageBoxResult StopMessage(this IMessageBase self, string titel, string message, bool withSound = false)
        {
            MessageBoxResult result = self.ShowMessage(titel, message, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK);
            return result;
        }

        public static MessageBoxResult Information(this IMessageBase self, string titel, string message, bool withSound = false)
        {
            MessageBoxResult result = self.ShowMessage(titel, message, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            return result;
        }

        public static MessageBoxResult CancelQuestion(this IMessageBase self, string titel, string message, bool withSound = false)
        {
            MessageBoxResult result = self.ShowMessage(titel, message, MessageBoxButton.YesNoCancel, MessageBoxImage.Hand, MessageBoxResult.None);
            return result;
        }

        public static MessageBoxResult Question(this IMessageBase self, string titel, string message)
        {
            MessageBoxResult result = self.ShowMessage(titel, message, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            return result;
        }

        public static MessageBoxResult Warning(this IMessageBase self, string titel, string message)
        {
            MessageBoxResult result = self.ShowMessage(titel, message, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            return result;
        }

        public static MessageBoxResult AppExitMessage(this IMessageBase self, string args = null)
        {
            MessageBoxResult result;

            string msgBoxTitle = LocalizationValue.Get("MessageExit_Titel_DE");
            if (args != null)
            {
                string msgBoxDescription = LocalizationValue.Get("MessageExit_Text_DE", args);
                result = self.ShowMessage(msgBoxTitle, msgBoxDescription, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            }
            else
            {
                string msgBoxDescription = LocalizationValue.Get("MessageExit_Text_DE");
                result = self.ShowMessage(msgBoxTitle, msgBoxDescription, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            }

            return result;
        }
    }
}