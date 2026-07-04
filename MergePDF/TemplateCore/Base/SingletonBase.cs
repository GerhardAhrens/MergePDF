namespace System.Windows
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Versioning;

    [DebuggerStepThrough]
    [Serializable]
    [SupportedOSPlatform("windows")]
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Generic Singleton Pattern")]
    public abstract class SingletonBase<T> where T : class
    {
        private static readonly object _reloadLock = new();

        /// <summary>
        /// Optionales Event nach erfolgreichem Reload
        /// </summary>
        public event Action Reloaded;

        private static readonly Lazy<T> _instance =
            new Lazy<T>(() =>
            {
                var type = typeof(T);

                // Erstellt Instanz über privaten/protected Konstruktor
                var obj = (T)Activator.CreateInstance(type, nonPublic: true)!;

                // Falls Initialisierung unterstützt wird
                if (obj is ISingletonInitializable init)
                {
                    try
                    {
                        init.Initialize();
                    }
                    catch (Exception ex)
                    {
                        string errorText = $"Fehler bei der Initialisierung der Singleton-Instanz vom Typ {type.FullName}: {ex.Message}";
                        throw;
                    }
                }

                return obj;
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        public static T Instance => _instance.Value;

        /// <summary>
        /// Führt Reload auf der bestehenden Instanz aus.
        /// </summary>
        public static void ReloadInstance()
        {
            lock (_reloadLock)
            {
                if (_instance.Value is not SingletonBase<T> singleton)
                {
                    throw new InvalidOperationException(
                        $"{typeof(T).Name} muss von Singleton<{typeof(T).Name}> erben.");
                }

                if (_instance.Value is ISingletonReloadable reloadable)
                {
                    reloadable.ReloadContent();

                    singleton.OnReloaded();
                }
                else
                {
                    throw new InvalidOperationException($"{typeof(T).Name} implementiert ISingletonReloadable nicht.");
                }
            }
        }

        /// <summary>
        /// Event auslösen
        /// </summary>
        protected virtual void OnReloaded()
        {
            var handler = Reloaded;
            handler?.Invoke();
        }
    }

    public interface ISingletonInitializable
    {
        void Initialize();
    }

    public interface ISingletonReloadable
    {
        void ReloadContent();
    }
}
