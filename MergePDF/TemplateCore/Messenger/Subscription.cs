namespace System.Windows
{
    using System;

    internal sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private bool _disposed;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _unsubscribe();
        }
    }
}
