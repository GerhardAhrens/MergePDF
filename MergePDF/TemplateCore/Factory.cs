namespace System.Windows
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.Versioning;

    [DebuggerStepThrough]
    [Serializable]
    [SupportedOSPlatform("windows")]
    public static class Factory
    {
        // Registrierung:
        // (EnumTyp, EnumWert) -> Registrierung
        private static readonly ConcurrentDictionary<(Type EnumType, object Key), Registration> _registrations = new();

        // Singleton Cache
        private static readonly ConcurrentDictionary<(Type EnumType, object Key), Lazy<object>> _singletons = new();

        public static int Count { get { return _registrations.Count; } }

        public static string[] Names { get { return _registrations.Keys.Select(k => k.Key.ToString()).ToArray(); } }


        // Registrierungseintrag
        private sealed class Registration
        {
            public required Func<object, object> FactoryMethod { get; init; }

            public required Lifetime Lifetime { get; init; }
        }

        public enum Lifetime
        {
            Singleton,
            Transient
        }

        /// <summary>
        /// Registrierung als Singleton
        /// </summary>
        public static void RegisterSingleton<TEnum>(TEnum id, Func<object> factory) where TEnum : struct, Enum
        {
            Register(id, factory, Lifetime.Singleton);
        }

        /// <summary>
        /// Registrierung als Transient
        /// </summary>
        public static void RegisterTransient<TEnum>(TEnum id, Func<object, object> factory) where TEnum : struct, Enum
        {
            Register(id, factory, Lifetime.Transient);
        }

        // Overload für parameterlose Factory (z.B. Singleton)
        private static void Register<TEnum>(TEnum id, Func<object> factory, Lifetime lifetime) where TEnum : struct, Enum
        {
            var key = (typeof(TEnum), (object)id);

            _registrations[key] = new Registration
            {
                FactoryMethod = _ => factory(),
                Lifetime = lifetime
            };
        }

        // Overload für Factory mit Parameter (z.B. Transient)
        private static void Register<TEnum>(TEnum id, Func<object, object> factory, Lifetime lifetime) where TEnum : struct, Enum
        {
            var key = (typeof(TEnum), (object)id);

            _registrations[key] = new Registration
            {
                FactoryMethod = factory,
                Lifetime = lifetime
            };
        }

        /// <summary>
        /// Liefert eine Instanz
        /// </summary>
        public static T Get<T, TEnum>(TEnum id, object parameter = null) where T : class where TEnum : struct, Enum
        {
            var key = (typeof(TEnum), (object)id);

            if (!_registrations.TryGetValue(key, out Registration registration))
            {
                throw new InvalidOperationException($"Keine Registrierung für '{typeof(TEnum).Name}.{id}'.");
            }

            object instance;

            switch (registration.Lifetime)
            {
                case Lifetime.Singleton:

                    Lazy<object> lazy = _singletons.GetOrAdd(key, _ =>
                    {
                        return new Lazy<object>(() => registration.FactoryMethod(parameter), isThreadSafe: true);
                    });

                    instance = lazy.Value;
                    break;

                case Lifetime.Transient:

                    instance = registration.FactoryMethod(parameter);
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (instance is not T result)
            {
                throw new InvalidCastException($"Instanz ist nicht vom Typ '{typeof(T).Name}'.");
            }

            return result;
        }
    }
}
