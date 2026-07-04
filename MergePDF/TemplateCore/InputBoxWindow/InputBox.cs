namespace System.Windows
{
    public static class InputBox
    {
        public static InputBoxResult<T> Show<T>(Window owner, InputBoxOptions<T> options)
        {
            var dlg = new InputBoxView
            {
                Owner = owner
            };

            dlg.Initialize(options);

            bool? result = dlg.ShowDialog();

            return new InputBoxResult<T>
            {
                IsOk = result == true,
                Value = result == true ? (T)dlg.ResultValue : default
            };
        }

        public static InputBoxResult<T> Show<T>(Window owner, string message, T defaultValue = default)
        {
            var dlg = new InputBoxView
            {
                Owner = owner
            };

            dlg.Initialize(message, defaultValue);

            bool? result = dlg.ShowDialog();

            return new InputBoxResult<T>
            {
                IsOk = result == true,
                Value = result == true ? (T)dlg.ResultValue : default
            };
        }
    }
}
