namespace System.Windows
{
    using System.Diagnostics;
    using System.Runtime.Versioning;

    [DebuggerStepThrough]
    [Serializable]
    [SupportedOSPlatform("windows")]
    public class NotificationBox
    {
        /// <summary>
        /// Zeigt ein Meldungsfeld mit einer Meldung an und gibt ein Ergebnis zurück.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult Show(string messageBoxText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung und eine Titelzeile enthält und ein Ergebnis zurückgibt.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult Show(string caption, string messageBoxText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt vor dem angegebenen Fenster ein Meldungsfeld an. Das Meldungsfeld zeigt eine Meldung an und gibt ein Ergebnis zurück.
        /// </summary>
        /// <param name="owner">Ein `System.Windows.Window`-Objekt, das das übergeordnete Fenster des Meldungsfelds darstellt.</param>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Owner = owner
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt ein Meldungsfeld vor dem angegebenen Fenster an. Das Meldungsfeld zeigt eine Meldung und eine Titelzeile an und gibt ein Ergebnis zurück.
        /// </summary>
        /// <param name="owner">Ein `System.Windows.Window`-Objekt, das das übergeordnete Fenster des Meldungsfelds darstellt.</param>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult Show(Window owner, string caption, string messageBoxText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Owner = owner
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Titelzeile und eine Schaltfläche enthält und ein Ergebnis zurückgibt.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="button">Ein Wert für „System.Windows.MessageBoxButton“, der angibt, welche Schaltfläche(n) angezeigt werden soll(en).</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult Show(string caption, string messageBoxText, MessageBoxButton button)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = button
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Titelzeile, eine Schaltfläche und ein Symbol enthält und ein Ergebnis zurückgibt.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="button">Ein Wert für „System.Windows.MessageBoxButton“, der angibt, welche Schaltfläche(n) angezeigt werden soll(en).</param>
        /// <param name="icon">Ein System.Windows.MessageBoxImage-Wert, der das anzuzeigende Symbol angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult Show(string caption, string messageBoxText, MessageBoxButton button, MessageBoxImage icon)
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

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Titelzeile und eine OK-Schaltfläche mit einem benutzerdefinierten System.String-Wert für den Text der Schaltfläche enthält und ein Ergebnis zurückgibt.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="okButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „OK“ angezeigt werden soll.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult ShowOK(string caption, string messageBoxText, string okButtonText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.YesNoCancel,
                OkButtonCaption = okButtonText
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Titelzeile, eine OK-Schaltfläche mit einem benutzerdefinierten 
        /// System.String-Wert als Text sowie ein Symbol enthält und ein Ergebnis zurückgibt.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="okButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „OK“ angezeigt werden soll.</param>
        /// <param name="icon">Ein System.Windows.MessageBoxImage-Wert, der das anzuzeigende Symbol angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult ShowOK(string caption, string messageBoxText, string okButtonText, MessageBoxImage icon)
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

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Beschriftung sowie OK-/Abbrechen-Schaltflächen mit benutzerdefinierten 
        /// System.String-Werten für den Text der Schaltflächen enthält;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="okButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „OK“ angezeigt werden soll.</param>
        /// <param name="cancelButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Abbrechen“ angezeigt werden soll.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult ShowOKCancel(string caption, string messageBoxText, string okButtonText, string cancelButtonText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.OKCancel,
                OkButtonCaption = okButtonText,
                CancelButtonCaption = cancelButtonText
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Beschriftung sowie OK-/Abbrechen-Schaltflächen mit benutzerdefinierten System.String-Werten 
        /// für den Text der Schaltflächen und ein Symbol enthält und ein Ergebnis zurückgibt.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="okButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „OK“ angezeigt werden soll.</param>
        /// <param name="cancelButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Abbrechen“ angezeigt werden soll.</param>
        /// <param name="icon">Ein System.Windows.MessageBoxImage-Wert, der das anzuzeigende Symbol angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult ShowOKCancel(string caption, string messageBoxText, string okButtonText, string cancelButtonText, MessageBoxImage icon)
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

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Beschriftung sowie „Ja“- und „Nein“-Schaltflächen mit benutzerdefinierten System.String-Werten 
        /// für den Text der Schaltflächen enthält und ein Ergebnis zurückgibt.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="yesButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Ja“ angezeigt werden soll.</param>
        /// <param name="noButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Nein“ angezeigt werden soll.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult ShowYesNo(string caption, string messageBoxText, string yesButtonText, string noButtonText)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.YesNo,
                YesButtonCaption = yesButtonText,
                NoButtonCaption = noButtonText
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Beschriftung sowie „Ja“- und „Nein“-Schaltflächen mit benutzerdefinierten System.String-Werten 
        /// für den Text der Schaltflächen enthält und ein Ergebnis zurückgibt.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="yesButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Ja“ angezeigt werden soll.</param>
        /// <param name="noButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Nein“ angezeigt werden soll.</param>
        /// <param name="icon">Ein System.Windows.MessageBoxImage-Wert, der das anzuzeigende Symbol angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult ShowYesNo(string caption, string messageBoxText, string yesButtonText, string noButtonText, MessageBoxImage icon)
        {
            var msgData = new MessageBoxData()
            {
                Message = messageBoxText,
                Caption = caption,
                Buttons = MessageBoxButton.YesNo,
                Image = icon,
                YesButtonCaption = yesButtonText,
                NoButtonCaption = noButtonText
            };

            return msgData.ShowMessageBox();
        }

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Überschrift sowie die Schaltflächen „Ja“, „Nein“ und „Abbrechen“ enthält, wobei die Textwerte der Schaltflächen benutzerdefinierte 
        /// System.String-Werte sind, und gibt ein Ergebnis zurück.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="yesButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Ja“ angezeigt werden soll.</param>
        /// <param name="noButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Nein“ angezeigt werden soll.</param>
        /// <param name="cancelButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Abbrechen“ angezeigt werden soll.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult ShowYesNoCancel(string caption, string messageBoxText, string yesButtonText, string noButtonText, string cancelButtonText)
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

        /// <summary>
        /// Zeigt ein Meldungsfeld an, das eine Meldung, eine Überschrift sowie die Schaltflächen „Ja“, „Nein“ und „Abbrechen“ enthält, wobei die Textwerte der Schaltflächen benutzerdefinierte 
        /// System.String-Werte sind, und gibt ein Ergebnis zurück.
        /// </summary>
        /// <param name="messageBoxText">Ein System.String, der den anzuzeigenden Text angibt.</param>
        /// <param name="caption">Ein System.String, der die anzuzeigende Beschriftung der Titelleiste angibt.</param>
        /// <param name="yesButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Ja“ angezeigt werden soll.</param>
        /// <param name="noButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Nein“ angezeigt werden soll.</param>
        /// <param name="cancelButtonText">Ein System.String, der den Text angibt, der auf der Schaltfläche „Abbrechen“ angezeigt werden soll.</param>
        /// <param name="icon">Ein System.Windows.MessageBoxImage-Wert, der das anzuzeigende Symbol angibt.</param>
        /// <returns>Ein Wert vom Typ „System.Windows.MessageBoxResult“, der angibt, welche Schaltfläche im Meldungsfeld vom Benutzer angeklickt wurde.</returns>
        public static MessageBoxResult ShowYesNoCancel(string caption, string messageBoxText, string yesButtonText, string noButtonText, string cancelButtonText, MessageBoxImage icon)
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
    }
}
