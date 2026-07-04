namespace MinimalWPF.Core
{
    using System;

    public sealed class WindowsTitelEvent
    {
        public WindowsTitelEvent(string dialogTitel)
        {
            this.Id = Guid.CreateVersion7();
            this.DialogTitel = dialogTitel;
        }

        public Guid Id { get; private set; }
        public string DialogTitel { get; private set; }
    }
}
