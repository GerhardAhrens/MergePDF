namespace System.Windows
{
    internal sealed class MessengerHandler
    {
        public WeakReference Target { get; }

        public Delegate Delegate { get; }

        public MessengerHandler(Delegate @delegate)
        {
            Delegate = @delegate;
            Target = new WeakReference(@delegate.Target!);
        }

        public bool IsAlive
        {
            get
            {
                if (Delegate.Target == null)
                    return true;

                return Target.IsAlive;
            }
        }
    }
}
