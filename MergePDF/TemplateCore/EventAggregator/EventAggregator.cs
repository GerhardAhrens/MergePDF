namespace System.Windows
{
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Diagnostics;

    [DebuggerStepThrough]
    [Serializable]
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, ImmutableArray<ISubscription>> _subscriptions = new();

        public int Count { get { return _subscriptions.Count; } }

        public string[] Names { get { return _subscriptions.Keys.Select(k => k.FullName).ToArray(); } }

        public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler)
        {
            ArgumentNullException.ThrowIfNull(handler);

            var subscription = new Subscription<TEvent>(handler);

            this._subscriptions.AddOrUpdate(typeof(TEvent), ImmutableArray<ISubscription>.Empty.Add(subscription), (_, existing) => existing.Add(subscription));

            return new SubscriptionToken(() => RemoveSubscription(typeof(TEvent), subscription));
        }

        public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            if (this._subscriptions.TryGetValue(typeof(TEvent), out var subscribers) == false)
            {
                return;
            }

            foreach (var subscriber in subscribers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await ((Subscription<TEvent>)subscriber).InvokeAsync(@event, cancellationToken).ConfigureAwait(false);
            }
        }

        private void RemoveSubscription(Type eventType, ISubscription subscription)
        {
            while (true)
            {
                if (this._subscriptions.TryGetValue(eventType, out var current) == false)
                {
                    return;
                }

                var updated = current.Remove(subscription);

                if (this._subscriptions.TryUpdate(eventType, updated, current))
                {
                    return;
                }
            }
        }

        public bool IsSubscription<TEvent>()
        {
            return _subscriptions.ContainsKey(typeof(TEvent));
        }

        public void RemoveAll()
        {
            this._subscriptions.Clear();
        }

        public void RemoveFor<TEvent>()
        {
            _subscriptions.TryRemove(typeof(TEvent), out _);
        }

        private interface ISubscription { }

        private sealed class Subscription<TEvent> : ISubscription
        {
            private readonly Func<TEvent, CancellationToken, ValueTask> _handler;

            public Subscription(Func<TEvent, CancellationToken, ValueTask> handler)
            {
                this._handler = handler;
            }

            public ValueTask InvokeAsync(TEvent @event, CancellationToken cancellationToken)
            {
                return this._handler(@event, cancellationToken);
            }
        }

        private sealed class SubscriptionToken : IDisposable
        {
            private readonly Action _unsubscribe;
            private int _disposed;

            public SubscriptionToken(Action unsubscribe)
            {
                this._unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1)
                {
                    return;
                }

                this._unsubscribe();
            }
        }
    }
}
