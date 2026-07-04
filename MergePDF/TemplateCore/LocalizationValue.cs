//-----------------------------------------------------------------------
// <copyright file="ResourcesText.cs" company="Lifeprojects.de">
//     Class: ResourcesText
//     Copyright © Lifeprojects.de 2023
// </copyright>
//
// <Framework>4.8</Framework>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>15.01.2023 09:01:02</date>
//
// <summary>
// Klasse für 
// </summary>
//-----------------------------------------------------------------------

namespace System.Windows
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Versioning;

    [SupportedOSPlatform("windows")]
    public class LocalizationValue
    {
        private const string DICTIONARYNAME = "Resources\\Localization\\Localization.xaml";
        private static ResourceDictionary resourceDict;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationValue"/> class.
        /// </summary>
        static LocalizationValue()
        {
            // Resources\Localization\Localization.xaml
            try
            {
                if (Application.Current != null)
                {
                    resourceDict = Application.Current.Resources.MergedDictionaries.Where(md => md.Source.OriginalString.EndsWith(DICTIONARYNAME, StringComparison.CurrentCulture)).FirstOrDefault();
                }
                else
                {
                    // 1. Pack-URI erstellen
                    // Format: pack://application:,,,/AssemblyName;component/PathTo/File.xaml
                    Uri resourceUri = new Uri($"/SnippetManager.Test;component/{DICTIONARYNAME}", UriKind.Relative);
                    // 2. Dictionary laden
                    resourceDict = (ResourceDictionary)Application.LoadComponent(resourceUri);
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Die Resource Datei '{DICTIONARYNAME}' konnte nicht gefunden werden", ex);
            }
        }

        public LocalizationValue()
        {
        }

        public static int Count { get { return resourceDict.Count; } }

        public static IEnumerable<string> Keys { get { return resourceDict.Keys.Cast<string>().Select(s => s).OrderBy(o => o); } }

        public static Dictionary<string, string> KeyValue
        {
            get
            {
                return resourceDict.Keys.Cast<string>().ToDictionary
                    (x => x,                             // Key selector
                     x => (string)resourceDict[x]        // Value selector
                     );
            }
        }

        public static string Get(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (resourceDict == null)
            {
                return $"ResourceDictionary 'Localization.xaml' nicht gefunden.";
            }

            bool keyFound = resourceDict.Cast<DictionaryEntry>().Any(f => f.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase));
            if (keyFound == false)
            {
                return $"ResourceKey '{key}' nicht gefunden.";
            }

            string value = resourceDict.Cast<DictionaryEntry>().FirstOrDefault(f => f.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase)).Value.ToString();

            return value.Replace("({0})", string.Empty).Replace("'{0}'", string.Empty);
        }

        public static string Get(string key, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (resourceDict == null)
            {
                return $"ResourceDictionary 'Localization.xaml' nicht gefunden.";
            }

            bool keyFound = resourceDict.Cast<DictionaryEntry>().Any(f => f.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase));
            if (keyFound == false)
            {
                return $"ResourceKey '{key}' nicht gefunden.";
            }

            string value = resourceDict.Cast<DictionaryEntry>().FirstOrDefault(f => f.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase)).Value.ToString();

            return string.Format(CultureInfo.CurrentCulture, value, args);
        }

        public static T Get<T>(string key)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (resourceDict == null)
            {
                throw new NotSupportedException($"ResourceDictionary 'Localization.xaml' nicht gefunden.");
            }

            bool keyFound = resourceDict.Cast<DictionaryEntry>().Any(f => f.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase));
            if (keyFound == false)
            {
                throw new NotSupportedException($"ResourceKey '{key}' nicht gefunden.");
            }

            object valueResult = resourceDict.Cast<DictionaryEntry>().FirstOrDefault(f => f.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase)).Value;

            return (T)valueResult;
        }

        public static void SetResources(string resourceFile)
        {
            try
            {
                resourceDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(md => md.Source.OriginalString.EndsWith(resourceFile.Replace(@"\", "/"), StringComparison.CurrentCulture));
                if (resourceDict == null)
                {
                    throw new NotSupportedException($"Die Resource Datei '{resourceFile}' konnte nicht in der Resources Liste gefunden werden");
                }
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"Die Resource Datei '{resourceFile}' konnte nicht gefunden werden", ex);
            }
        }
    }
}
