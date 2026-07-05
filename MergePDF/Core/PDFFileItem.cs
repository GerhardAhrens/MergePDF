namespace MergePDF.Core
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("Filename: {this.Filename}; Order: {this.Order}; IsSelectedItem: {this.IsSelectedItem}")]
    public class PDFFileItem : INotifyPropertyChanged
    {
        private int order;
        private string fullname;
        private string filename;
        private string fileSize;
        private bool isSelectedItem;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Order
        {
            get => this.order;
            set
            {
                if (this.order != value)
                {
                    this.order = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Fullname
        {
            get => this.fullname;
            set
            {
                if (this.fullname != value)
                {
                    this.fullname = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Filename
        {
            get => this.filename;
            set
            {
                if (this.filename != value)
                {
                    this.filename = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string FileSize
        {
            get => this.fileSize;
            set
            {
                if (this.fileSize != value)
                {
                    this.fileSize = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public bool IsSelectedItem
        {
            get => this.isSelectedItem;
            set
            {
                if (this.isSelectedItem != value)
                {
                    this.isSelectedItem = value;
                    this.OnPropertyChanged();
                }
            }
        }


        public override string ToString()
        {
            return $"Name: {this.Filename};IsSelectedItem: {this.IsSelectedItem}";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
