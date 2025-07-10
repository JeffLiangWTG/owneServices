#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class ReflectionExtensions
	{
		public static void SetPropertyValue(this object target, string propertyName, object newValue)
		{
			var property = GetPropertyInfo(ref target, propertyName);
			property.SetValue(target, newValue, null);
		}

		public static object GetPropertyValue(this object target, string propertyName)
		{
			var property = GetPropertyInfo(ref target, propertyName);
			return property.GetValue(target, null);
		}

		public static Type GetPropertyType(this object target, string propertyName)
		{
			var property = GetPropertyInfo(ref target, propertyName);
			return property.PropertyType;
		}

		public static bool IsBool(this object target, string propertyName)
		{
			var propertyType = GetPropertyType(target, propertyName);
			return propertyType == typeof(bool) || propertyType == typeof(ZBool);
		}

		public static bool IsString(this object target, string propertyName)
		{
			var propertyType = GetPropertyType(target, propertyName);
			return propertyType == typeof(string) || propertyType == typeof(string) || propertyType == typeof(ZString);
		}

		static PropertyInfo GetPropertyInfo(ref object target, string propertyName)
		{
			var propertyPath = propertyName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			var property = target.GetType().GetProperty(propertyPath[0]);

			if (propertyPath.Length == 1)
			{
				return property;
			}
			else
			{
				var subPropertyPath = string.Join(".", propertyPath.Skip(1));
				target = property.GetValue(target, null);

				return GetPropertyInfo(ref target, subPropertyPath);
			}
		}

		public static IEnumerable<string> GetConstantValues(this Type t)
			=> t.GetFields(BindingFlags.Static | BindingFlags.Public)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly)
				.Select(fi => (string)fi.GetValue(null));
	}
}

#endif
