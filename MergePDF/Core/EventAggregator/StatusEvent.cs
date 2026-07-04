namespace MergePDF.Core
{
    using System;

    public sealed class StatusEvent
    {
        public StatusEvent(string notification, string databaseInfo, string databaseInfoTooltip)
        {
            this.Id = Guid.CreateVersion7();
            this.DatabaseInfo = databaseInfo;
            this.DatabaseInfoTooltip = databaseInfoTooltip;
            this.Notification = notification;
        }

        public StatusEvent(string databaseInfo, string databaseInfoTooltip)
        {
            this.Id = Guid.CreateVersion7();
            this.DatabaseInfo = databaseInfo;
            this.DatabaseInfoTooltip = databaseInfoTooltip;
        }

        public StatusEvent(string notification, bool withSound = false)
        {
            this.Id = Guid.CreateVersion7();
            this.Notification = notification;

            if (withSound == true)
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        public Guid Id { get; private set; }
        public string DatabaseInfo { get; private set; }
        public string DatabaseInfoTooltip { get; private set; }
        public string Notification { get; private set; }
    }
}
