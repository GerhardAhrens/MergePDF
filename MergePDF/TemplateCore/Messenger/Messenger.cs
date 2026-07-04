namespace System.Windows
{
    using System.Collections.Concurrent;

    public sealed class Messenger
    {
        public static Messenger Default { get; } = new();

        private readonly ConcurrentDictionary<Type, List<MessengerHandler>> _handlers = new();

        private readonly object _lock = new();

        /// <summary>
        /// Register Action
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable Register<TMessage>(Action<TMessage> handler)
        {
            var wrapper = new MessengerHandler(handler);

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TMessage), out var handlers))
                {
                    handlers = [];
                    _handlers[typeof(TMessage)] = handlers;
                }

                handlers.Add(wrapper);
            }

            return new Subscription(() => this.Unregister(handler));
        }

        /// <summary>
        /// Register Request
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        public IDisposable Register<TMessage, TResult>(Func<TMessage, TResult> handler)
        {
            var wrapper = new MessengerHandler(handler);

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TMessage), out var handlers))
                {
                    handlers = [];
                    _handlers[typeof(TMessage)] = handlers;
                }

                handlers.Add(wrapper);
            }

            return new Subscription(() => this.Unregister(handler));
        }

        public void Unregister<TMessage>(Action<TMessage> handler)
        {
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TMessage), out var handlers))
                {
                    return;
                }

                handlers.RemoveAll(h => Equals(h.Delegate, handler));

                if (handlers.Count == 0)
                {
                    _handlers.TryRemove(typeof(TMessage), out _);
                }
            }
        }

        public void Unregister<TMessage, TResult>(Func<TMessage, TResult> handler)
        {
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TMessage), out var handlers))
                {
                    return;
                }

                handlers.RemoveAll(h => Equals(h.Delegate, handler));

                if (handlers.Count == 0)
                {
                    _handlers.TryRemove(typeof(TMessage), out _);
                }
            }
        }
        /// <summary>
        /// Send Message
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        public void Send<TMessage>(TMessage message)
        {
            List<MessengerHandler> handlers;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TMessage), out handlers))
                {
                    return;
                }

                handlers = handlers.ToList();
            }

            foreach (var handler in handlers)
            {
                if (!handler.IsAlive)
                {
                    continue;
                }

                ((Action<TMessage>)handler.Delegate)(message);
            }

            this.Cleanup();
        }

        public TResult Request<TMessage, TResult>(TMessage message)
        {
            List<MessengerHandler> handlers;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TMessage), out handlers))
                {
                    return default;
                }

                handlers = handlers.ToList();
            }

            var handler = handlers.Select(x => x.Delegate).OfType<Func<TMessage, TResult>>().FirstOrDefault();

            return handler.Invoke(message);
        }

        public IEnumerable<TResult> RequestAll<TMessage, TResult>(TMessage message)
        {
            List<MessengerHandler> handlers;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TMessage), out handlers))
                {
                    return Enumerable.Empty<TResult>();
                }

                handlers = handlers.ToList();
            }

            return handlers.Where(x => x.IsAlive).Select(x => x.Delegate).OfType<Func<TMessage, TResult>>().Select(x => x(message)).ToList();
        }

        public async Task<TResult> RequestAsync<TMessage, TResult>(TMessage message)
        {
            List<MessengerHandler> handlers;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(TMessage), out handlers))
                {
                    return default;
                }

                handlers = handlers.ToList();
            }

            var handler = handlers.Select(x => x.Delegate).OfType<Func<TMessage, Task<TResult>>>().FirstOrDefault();

            if (handler == null)
            {
                return default;
            }

            return await handler(message);
        }

        public void SendOnUiThread<TMessage>(TMessage message)
        {
            var dispatcher = Application.Current.Dispatcher;

            if (dispatcher.CheckAccess())
            {
                Send(message);
                return;
            }

            dispatcher.Invoke(() =>
            {
                Send(message);
            });
        }

        private void Cleanup()
        {
            lock (_lock)
            {
                foreach (var key in _handlers.Keys.ToList())
                {
                    if (!_handlers.TryGetValue(key, out var handlers))
                    {
                        continue;
                    }

                    handlers.RemoveAll(h => !h.IsAlive);

                    if (handlers.Count == 0)
                    {
                        _handlers.TryRemove(key, out _);
                    }
                }
            }
        }
    }
}
