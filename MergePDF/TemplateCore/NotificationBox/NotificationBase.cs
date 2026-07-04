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
    public class NotificationBase : INotificationBase
    {
        public MessageBoxResult Show(string messageBoxText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Image = MessageBoxImage.Information
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult Show(string caption, string messageBoxText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Image = MessageBoxImage.Information
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult Show(Window owner, string messageBoxText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Owner = owner,
                Image = MessageBoxImage.Information
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult Show(Window owner, string caption, string messageBoxText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Owner = owner,
                Image = MessageBoxImage.Information
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult Show(string caption, string messageBoxText, MessageBoxButton button)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = button,
                Image = MessageBoxImage.Information
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult Show(string caption, string messageBoxText, MessageBoxButton button, MessageBoxImage icon)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = button,
                Image = icon
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowOK(string caption, string messageBoxText, string okButtonText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.YesNoCancel,
                OkButtonCaption = okButtonText,
                Image = MessageBoxImage.Information
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowOK(string caption, string messageBoxText, string okButtonText, MessageBoxImage icon)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.OK,
                Image = icon,
                OkButtonCaption = okButtonText
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowOKCancel(string caption, string messageBoxText, string okButtonText, string cancelButtonText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.OKCancel,
                OkButtonCaption = okButtonText,
                CancelButtonCaption = cancelButtonText,
                Image = MessageBoxImage.Question
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowOKCancel(string caption, string messageBoxText, string okButtonText, string cancelButtonText, MessageBoxImage icon)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.OKCancel,
                Image = icon,
                OkButtonCaption = okButtonText,
                CancelButtonCaption = cancelButtonText
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowYesNo(string caption, string messageBoxText, string yesButtonText, string noButtonText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.YesNo,
                YesButtonCaption = yesButtonText,
                NoButtonCaption = noButtonText,
                Image = MessageBoxImage.Question
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowYesNo(string caption, string messageBoxText, string yesButtonText, string noButtonText, MessageBoxImage icon)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.YesNo,
                Image = icon,
                YesButtonCaption = yesButtonText,
                NoButtonCaption = noButtonText,
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowYesNoCancel(string caption, string messageBoxText, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.YesNoCancel,
                YesButtonCaption = yesButtonText,
                NoButtonCaption = noButtonText,
                CancelButtonCaption = cancelButtonText
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowYesNoCancel(string caption, string messageBoxText, string yesButtonText, string noButtonText, string cancelButtonText, MessageBoxImage icon)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.YesNoCancel,
                Image = icon,
                YesButtonCaption = yesButtonText,
                NoButtonCaption = noButtonText,
                CancelButtonCaption = cancelButtonText
            };

            return msgData.ShowMessageBox();
        }

        public MessageBoxResult ShowAbortRetryIgnore(string caption, string messageBoxText, string yesButtonText, string noButtonText, string cancelButtonText, MessageBoxImage icon)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.AbortRetryIgnore,
                Image = icon,
                YesButtonCaption = yesButtonText,
                NoButtonCaption = noButtonText,
                CancelButtonCaption = cancelButtonText
            };

            return msgData.ShowMessageBox();
        }
    }
}
