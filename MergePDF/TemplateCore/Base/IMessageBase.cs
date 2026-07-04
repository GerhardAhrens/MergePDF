//-----------------------------------------------------------------------
// <copyright file="IMessageBase.cs" company="Lifeprojects.de">
//     Class: IMessageBase
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>05.03.2026 18:21:36</date>
//
// <summary>
// Interface zur MassageBox Base
// </summary>
//-----------------------------------------------------------------------

namespace System.Windows
{
    public interface IMessageBase
    {
        MessageBoxResult ShowMessage(string titel, string message, MessageBoxButton mboxButton, MessageBoxImage icon, MessageBoxResult defaultResult);
        MessageBoxResult ShowMessage(string titel, string message, bool withSound = false);
        MessageBoxResult ShowMessage(string titel, string message);
    }
}