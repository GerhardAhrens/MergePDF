namespace System.Windows
{
    using System;

    public sealed class InputBoxOptions<T>
    {
        public string Title { get; set; } = "Eingabe";

        public string Message { get; set; } = string.Empty;

        public string OkButtonText { get; set; } = "OK";

        public string CancelButtonText { get; set; } = "Abbrechen";

        public bool IsRequired { get; set; } = true;

        public T DefaultValue { get; set; }

        public string FormatString { get; set; }

        public int? MinInt { get; set; }

        public int? MaxInt { get; set; }

        public double? MinDouble { get; set; }

        public double? MaxDouble { get; set; }

        public DateTime? MinDate { get; set; }

        public DateTime? MaxDate { get; set; }
    }
}
