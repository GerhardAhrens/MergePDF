/*
 * <copyright file="EnumExtensions.cs" company="Lifeprojects.de">
 *     Class: EnumExtensions
 *     Copyright © Lifeprojects.de 2022
 * </copyright>
 *
 * <author>Gerhard Ahrens - Lifeprojects.de</author>
 * <email>developer@lifeprojects.de</email>
 * <date>25.09.2022 08:40:08</date>
 * <Project>EasyPrototypingNET</Project>
 *
 * <summary>
 * EnumExtensions Definition
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
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    public static partial class EnumExtensions
    {
        /// <summary>
        ///   Return a list of item in Enumeration
        /// </summary>
        public static List<Enum> ToList(this Enum @this)
        {
            return
                @this.GetType()
                    .GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Select(fieldInfo => (Enum)fieldInfo.GetValue(@this))
                    .ToList();
        }

        /// <summary>
        /// Die Methode gibt den nummerischen Wert eines Enum-Elementes als Int zurück
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static int ToInt<TEnum>(this TEnum @this) where TEnum : struct, Enum
        {
            if (typeof(TEnum).IsEnum == false)
            {
                throw new ArgumentException("TEnum must be an enumerated type");
            }

            return (int)(IConvertible)@this;
        }

        /// <summary>
        /// Die Methode gibt den nummerischen Wert eines Enum-Elementes als String zurück
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string ToValueAsString<TEnum>(this TEnum @this) where TEnum : struct, Enum
        {
            if (typeof(TEnum).IsEnum == false)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return ((int)(IConvertible)@this).ToString(CultureInfo.CurrentCulture);
        }

        public static string ToUpperString<TEnum>(this TEnum @this) where TEnum : struct, Enum
        {
            if (typeof(TEnum).IsEnum == false)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return @this.ToString().ToUpper(CultureInfo.CurrentCulture);
        }

        public static string ToLowerString<TEnum>(this TEnum @this) where TEnum : struct, Enum
        {
            if (typeof(TEnum).IsEnum == false)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return @this.ToString().ToLower(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Die Methode gibt die Anzahl der Enum Elemente zurück
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static int Count<TEnum>(this TEnum @this) where TEnum : IConvertible
        {
            if (typeof(TEnum).IsEnum == false)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return Enum.GetNames(typeof(TEnum)).Length;
        }

        /// <summary>
        /// A T extension method to determines whether the object is equal to any of the provided values.
        /// </summary>
        /// <param name="this">The object to be compared.</param>
        /// <param name="values">The value list to compare with the object.</param>
        /// <returns>true if the values list contains the object, else false.</returns>
        public static bool In(this Enum @this, params Enum[] values)
        {
            return Array.IndexOf(values, @this) != -1;
        }

        /// <summary>
        /// A T extension method to determines whether the object is not equal to any of the provided values.
        /// </summary>
        /// <param name="this">The object to be compared.</param>
        /// <param name="values">The value list to compare with the object.</param>
        /// <returns>true if the values list doesn't contains the object, else false.</returns>
        public static bool NotIn(this Enum @this, params Enum[] values)
        {
            return Array.IndexOf(values, @this) == -1;
        }

        public static TEnum FromEnumDescription<TEnum>(this string @this) where TEnum : struct, Enum
        {

            int count = typeof(TEnum).GetFields().Count(f => f.GetCustomAttributes<DescriptionAttribute>()
                                 .Any(a => a.Description.Equals(@this, StringComparison.OrdinalIgnoreCase)));
            if (count > 0)
            {
                return (TEnum)typeof(TEnum)
                    .GetFields()
                    .First(f => f.GetCustomAttributes<DescriptionAttribute>()
                    .Any(a => a.Description.Equals(@this, StringComparison.OrdinalIgnoreCase)))
                    .GetValue(null);
            }

            return default(TEnum);
        }

        public static TEnum FromEnumDescription<TEnum>(this string @this, TEnum defaultEnum) where TEnum : struct
        {

            int count = typeof(TEnum).GetFields().Count(f => f.GetCustomAttributes<DescriptionAttribute>()
                                 .Any(a => a.Description.Equals(@this, StringComparison.OrdinalIgnoreCase)));
            if (count > 0)
            {
                return (TEnum)typeof(TEnum)
                    .GetFields()
                    .First(f => f.GetCustomAttributes<DescriptionAttribute>()
                    .Any(a => a.Description.Equals(@this, StringComparison.OrdinalIgnoreCase)))
                    .GetValue(null);
            }

            return defaultEnum;
        }


        public static string ToDescription<TEnum>(this TEnum @this) where TEnum : struct
        {
            FieldInfo fieldInfo = @this.GetType().GetField(@this.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return @this.ToString();
            }
        }

        public static string ToDescription(this Enum @this)
        {
            FieldInfo fieldInfo = @this.GetType().GetField(@this.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return @this.ToString();
            }
        }

        /// <summary>
        /// Konvertiert ein enum zu einem Dictionary<int,string>()
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Dictionary<int, string> ToDictionary<TEnum>(this TEnum @this, bool setDescription = false) where TEnum : struct
        {
            if (typeof(TEnum).IsEnum == false)
            {
                throw new ArgumentException("Type must be an enumeration");
            }

            var type = typeof(TEnum);
            if (setDescription == true)
            {
                var dict = Enum.GetValues(type).Cast<int>().ToDictionary(e => e, e => Enum.GetName(type, e));

                foreach (TEnum enumItem in (TEnum[])Enum.GetValues(typeof(TEnum)))
                {
                    int index = Convert.ToInt32(enumItem, CultureInfo.CurrentCulture);
                    dict[index] = enumItem.ToDescription();
                }

                return dict;
            }
            else
            {
                return Enum.GetValues(type).Cast<int>().ToDictionary(e => e, e => Enum.GetName(type, e));
            }
        }

        /// <summary>
        /// Gib ein Enum als Dictionary zurüxk
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IDictionary ToDictionary<TEnumValueType>(this Enum @this)
        {
            if (typeof(TEnumValueType).FullName != Enum.GetUnderlyingType(@this.GetType()).FullName)
            {
                throw new ArgumentException("Invalid type specified.");
            }

            return Enum.GetValues(@this.GetType()).Cast<object>().ToDictionary(key => Enum.GetName(@this.GetType(), key), value => (TEnumValueType)value);
        }

        public static int EnumToInt<TValue>(this TValue value) where TValue : struct, IConvertible
        {
            if (typeof(TValue).IsEnum == false)
            {
                throw new ArgumentException(null, nameof(value));
            }

            return (int)(object)value;
        }
    }
}