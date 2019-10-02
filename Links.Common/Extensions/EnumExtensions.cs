using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Links.Common.Extensions
{
    public static class EnumExtensions
    {
        public static T SetFlag<T>(this T flags, T flag, bool set = true)
            where T : struct, IConvertible
        {
            var source = flags.ToInt64(null);
            var value = flag.ToInt64(null);
            var result = set ?
                    source | value :
                    source & ~value;
            return (T)Enum.ToObject(typeof(T), result);
        }

        public static bool IsCombined<T>(this T flags)
            where T : struct, IConvertible
        {
            var value = flags.ToUInt32(null);
            if (value == 0)
            {
                // flags is 0 => not combined
                return false;
            }
            value &= value - 1;
            if (value == 0)
            {
                // flags has only one bit set => not combined
                return false;
            }
            value &= value - 1;
            return value != 0; // if value is not 0 there is another bit set and therefore it is a combined value
        }

        public static string GetDisplayValue(this Enum value)
        {
            if (value == null)
            {
                return String.Empty;
            }

            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null)
            {
                return value.ToString();
            }

            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].GetName() : value.ToString();
        }

        public static string GetDisplayName(this Enum @enum)
        {
            var enumType = @enum.GetType();
            var enumString = @enum.ToString();
            var enumStrings = enumString.Split(',');
            var result = new List<string>();
            foreach (var item in enumStrings)
            {
                var memberInfo = enumType.GetMember(item);
                var attributes = memberInfo.FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>(false);
                var name = attributes?.GetName() ?? item;
                result.Add(name);
            }
            return String.Join(", ", result);
        }

        public static string GetDisplayNames(this Enum flags, bool withoutNone = false)
        {
            var flagNames = new List<string>();
            if (flags == null)
            {
                return string.Empty;
            }
            foreach (Enum value in Enum.GetValues(flags.GetType()))
            {
                if (withoutNone && Convert.ToInt32(value) == 0)
                {
                    continue;
                }

                if (!flags.HasFlag(value))
                {
                    continue;
                }

                var memberInfo = flags.GetType().GetMember(value.ToString());
                var attributes = memberInfo.FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>(false);
                var name = attributes?.GetName();
                if (name != null)
                {
                    flagNames.Add(name);
                }
            }
            return string.Join(", ", flagNames.ToArray());
        }

        public static IReadOnlyDictionary<TEnum, string> GetTranslatedDictionary<TEnum>(this TEnum @enum, Type translations = null, bool ignoreCombinations = false)
            where TEnum : struct, IConvertible
        {
            var result = new Dictionary<TEnum, string>();
            var enumType = typeof(TEnum);
            foreach (TEnum enumValue in Enum.GetValues(enumType).Cast<TEnum>().Where(e => !ignoreCombinations || !e.IsCombined()))
            {
                var enumValueAttr = enumType.GetField(enumValue.ToString()).GetCustomAttribute<DisplayAttribute>();
                if (enumValueAttr != null)
                {
                    enumValueAttr.ResourceType = enumValueAttr.ResourceType ?? translations;
                    enumValueAttr.Name = enumValueAttr.Name ?? enumValue.ToString();
                }
                result.Add(enumValue, enumValueAttr?.GetName() ?? enumValue.ToString());
            }
            return new ReadOnlyDictionary<TEnum, string>(result);
        }
    }
}
