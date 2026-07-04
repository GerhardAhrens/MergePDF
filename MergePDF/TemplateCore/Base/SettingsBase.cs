/*
 * <copyright file="SettingsManagerBase.cs" company="Lifeprojects.de">
 *     Class: SettingsManagerBase
 *     Copyright © Lifeprojects.de 2023
 * </copyright>
 *
 * <author>Gerhard Ahrens - Lifeprojects.de</author>
 * <email>developer@lifeprojects.de</email>
 * <date>12.05.2023 15:47:23</date>
 * <Project>EasyPrototypingNET</Project>
 *
 * <summary>
 * Abstrakte Klassen zum Bearbeiten von Settings
 * </summary>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by the Free Software Foundation, 
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
*/

namespace System.Windows
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Base class for settings.
    /// </summary>
    //[DebuggerStepThrough]
    [Serializable]
    [SupportedOSPlatform("windows")]
    public abstract class SettingsBase : DisposableCoreBase
    {

        /// <summary>
        /// Initializes an instance of <see cref="SettingsBase" />.
        /// </summary>
        protected SettingsBase(string filePath = "", string namePrefix = "Application")
        {
            this.NamePrefix = namePrefix;

            if (string.IsNullOrEmpty(filePath) == true)
            {
                this.SettingsLocation = SettingsLocation.ProgramData;
                string settingsPath = this.CurrentSettingsPath();
                string settingsName = $"{namePrefix}.{UserSettingsName()}";
                string settingsFile = Path.Combine(settingsPath, settingsName);
                this.FilePath = settingsFile;
            }
            else
            {
                this.SettingsLocation = SettingsLocation.AssemblyLocation;
                this.FilePath = filePath;
            }

            GetProperties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.DeclaringType != typeof(SettingsBase))
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() is null)
                .ToArray();

            // Default values for properties are initialized before the constructor is called,
            // so we can safely retrieve them here.
            GetDefaults = GetProperties.ToDictionary(p => p, p => p.GetValue(this));
        }

        public string Filename
        {
            get
            {
                return $"{this.NamePrefix}.{UserSettingsName()}";
            }
        }

        public string Pathname
        {
            get
            {
                return $"{this.CurrentSettingsPath()}\\";
            }
        }

        public IReadOnlyList<PropertyInfo> GetProperties { get; }

        public IReadOnlyDictionary<PropertyInfo, object> GetDefaults { get; }

        public int Count
        {
            get
            {
                return this.GetProperties != null ? this.GetProperties.Count : 0;
            }
        }

        private string NamePrefix { get; set; }

        private SettingsLocation SettingsLocation { get; set; }

        private string FilePath { get; }

        /// <summary>
        /// Resets the settings to their default values.
        /// </summary>
        public virtual void Reset()
        {
            foreach (var property in GetProperties)
            {
                property.SetValue(this, GetDefaults[property]);
            }
        }

        public void SetSetting(SettingsBase settings)
        {
            try
            {
                foreach (PropertyInfo property in settings.GetProperties)
                {
                    property.SetValue(this, property.GetValue(settings));
                }
            }
            catch (Exception ex)
            {
                string errorText = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// Saves the settings to file.
        /// </summary>
        public virtual void Save()
        {
            try
            {
                if (File.Exists(this.FilePath) == true)
                {
                    File.Delete(this.FilePath);
                }

                var dirPath = Path.GetDirectoryName(this.FilePath);
                if (!string.IsNullOrWhiteSpace(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                using var stream = File.Create(this.FilePath);
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                {
                    Indented = true
                });

                writer.WriteStartObject();

                JsonSerializerOptions options = new JsonSerializerOptions();

                foreach (var property in GetProperties)
                {

                    // Use custom converter if set
                    if (property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType is { } converterType &&
                        Activator.CreateInstance(converterType) is JsonConverter converter)
                    {
                        options.Converters.Add(converter);
                    }

                    // Use custom name if set
                    writer.WritePropertyName(property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name);

                    JsonSerializer.Serialize(writer, property.GetValue(this), property.PropertyType, options);
                }

                writer.WriteEndObject();
            }
            catch (Exception ex)
            {
                string errorText = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// Loads the settings from file.
        /// Returns true if the file was loaded, false if it didn't exist.
        /// </summary>
        public virtual bool Load()
        {
            try
            {
                if (File.Exists(this.FilePath) == false)
                {
                    return false;
                }

                using var stream = File.OpenRead(this.FilePath);
                using var document = JsonDocument.Parse(stream, new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                });

                JsonSerializerOptions options = new JsonSerializerOptions();

                foreach (var jsonProperty in document.RootElement.EnumerateObject())
                {
                    // Use custom name if set
                    var property = GetProperties
                        .FirstOrDefault(p => string.Equals(
                            p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name, jsonProperty.Name, StringComparison.Ordinal
                    ));

                    if (property is null)
                    {
                        continue;
                    }

                    // Use custom converter if set
                    if (property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType is { } converterType &&
                        Activator.CreateInstance(converterType) is JsonConverter converter)
                    {
                        options.Converters.Add(converter);
                    }

                    property.SetValue(
                        this,
                        JsonSerializer.Deserialize(jsonProperty.Value.GetRawText(), property.PropertyType, options)
                    );
                }

                return true;
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes the settings file.
        /// Returns true if the file was deleted, false if it didn't exist.
        /// </summary>
        public virtual bool Delete()
        {
            try
            {
                if (File.Exists(this.FilePath))
                {
                    // This doesn't throw if the file doesn't exist, but
                    // does throw if the directory doesn't exist.
                    File.Delete(this.FilePath);
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                return false;
            }
        }

        public bool IsExitSettings()
        {
            if (File.Exists(this.FilePath) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string CurrentSettingsPath()
        {
            string settingsPath = string.Empty;

            if (this.SettingsLocation == SettingsLocation.ProgramData)
            {
                string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                settingsPath = $"{rootPath}\\{ApplicationName()}\\Settings";
            }
            else
            {
                string rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                settingsPath = $"{rootPath}\\{ApplicationName()}\\Settings";
            }

            if (string.IsNullOrEmpty(settingsPath) == false)
            {
                try
                {
                    if (Directory.Exists(settingsPath) == false)
                    {
                        Directory.CreateDirectory(settingsPath);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return settingsPath;
        }

        private static string ApplicationName()
        {
            return AppDomain.CurrentDomain.FriendlyName;
        }

        private static string UserSettingsName()
        {
            return $"{Environment.UserName}.Setting";
        }
    }
}
