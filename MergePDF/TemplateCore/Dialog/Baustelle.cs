namespace System.Windows
{
    public static class Baustelle
    {
        public static DialogResult Show()
        {
            Window owner = Application.Current.MainWindow;
            return Show(owner, string.Empty, string.Empty);
        }

        public static DialogResult Show(Window owner)
        {
            return Show(owner, string.Empty, string.Empty);
        }

        public static DialogResult Show(Window owner, string title, string message)
        {
            title = string.IsNullOrEmpty(title) == true ? "Information" : title;
            message = string.IsNullOrEmpty(message) == true ? "Die gewünschte Funktion steht aktuellen nicht zur Verfügung!" : message;

            BaustelleDlg dialog = new BaustelleDlg(title, message);
            dialog.ShowInTaskbar = false;
            dialog.Owner = owner;
            if (dialog.ShowDialog() == true)
            {
                return new DialogResult
                {
                    Accepted = true,
                };
            }

            return new DialogResult
            {
                Accepted = false
            };
        }
    }
}
