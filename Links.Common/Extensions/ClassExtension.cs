using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Links.Common.Extensions
{
	public static class ClassExtension
	{
        private static string[] _ignore = { "IsActive", "IsInitialized", "CanSave" };

        public static bool IsDirty<T>(this T self, T to) where T : class
		{
            var ignoreProps = typeof(T).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(AlwaysCleanAttribute))).Select(pi => pi.Name);

            return !self.PublicInstancePropertiesSimilar(to, _ignore.Concat(ignoreProps).ToArray());
		}

        /// <summary>
        /// same as equal but string null == empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="to"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static bool PublicInstancePropertiesSimilar<T>(this T self, T to, params string[] ignore) where T : class
        {
            if (self != null && to != null)
            {
                var type = typeof(T);
                var ignoreList = new List<string>(ignore);

                var unsimilarProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(pi => !ignoreList.Contains(pi.Name) && pi.GetUnderlyingType().IsSimpleType() && pi.GetIndexParameters().Length == 0)
                                            .Select(pi => new
                                            {
                                                selfValue = type.GetProperty(pi.Name).GetValue(self, null),
                                                toValue = type.GetProperty(pi.Name).GetValue(to, null),
                                                isString = type.GetProperty(pi.Name).PropertyType == typeof(string),
                                                name = pi.Name
                                            })
                                            //.ToList();
                                            //unsimilarProperties = unsimilarProperties
                                            .Where(
                                                a => (!a.isString
                                                        && a.selfValue != a.toValue
                                                        && (a.selfValue == null || !a.selfValue.Equals(a.toValue)))
                                                    || (a.isString
                                                        && (string)a.selfValue != (string)a.toValue
                                                        && ((string.IsNullOrEmpty((string)a.selfValue) != string.IsNullOrEmpty((string)a.toValue))
                                                            || (!string.IsNullOrEmpty((string)a.selfValue) && !string.IsNullOrEmpty((string)a.toValue)))))
                                            .ToList();

                return unsimilarProperties.Count == 0;
            }
            return self == to;
        }

        public static bool PublicInstancePropertiesEqual<T>(this T self, T to, params string[] ignore) where T : class
		{
			if (self != null && to != null)
			{
				var type = typeof(T);
				var ignoreList = new List<string>(ignore);
				var unequalProperties =
					from pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					where !ignoreList.Contains(pi.Name) && pi.GetUnderlyingType().IsSimpleType() && pi.GetIndexParameters().Length == 0
					let selfValue = type.GetProperty(pi.Name).GetValue(self, null)
					let toValue = type.GetProperty(pi.Name).GetValue(to, null)
					where selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue))
					select selfValue;
                var test = unequalProperties.ToList();
				return !unequalProperties.Any();
			}
			return self == to;
		}
	}

	public static class TypeExtensions
	{
		/// <summary>
		/// Determine whether a type is simple (String, Decimal, DateTime, etc) 
		/// or complex (i.e. custom class with public properties and methods).
		/// </summary>
		/// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
		public static bool IsSimpleType(
		   this Type type)
		{
			return
			   type.IsValueType ||
			   type.IsPrimitive ||
			   new[]
			   {
			   typeof(String),
			   typeof(Decimal),
			   typeof(DateTime),
			   typeof(DateTimeOffset),
			   typeof(TimeSpan),
			   typeof(Guid)
			   }.Contains(type) ||
			   (Convert.GetTypeCode(type) != TypeCode.Object);
		}

		public static Type GetUnderlyingType(this MemberInfo member)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Event:
					return ((EventInfo)member).EventHandlerType;
				case MemberTypes.Field:
					return ((FieldInfo)member).FieldType;
				case MemberTypes.Method:
					return ((MethodInfo)member).ReturnType;
				case MemberTypes.Property:
					return ((PropertyInfo)member).PropertyType;
				default:
					throw new ArgumentException
					(
					   "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
					);
			}
		}
	}
}
