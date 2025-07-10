using System;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public static class CustomAttributeExtensions
	{
		public static ZDecimal GetDecimalValueByPropertyWithDecimalPlacesAttribute(this object obj, string name, bool normalize = true)
		{
			var result = ZDecimal.Zero;
			var propertyInfo = obj.GetPropertyInfo(name);
			if (propertyInfo != null && (propertyInfo.PropertyType == typeof(ZDecimal) || propertyInfo.PropertyType == typeof(ZDecimal?)))
			{
				var decimalPlaces = propertyInfo.GetCustomAttributeIncludingInterfaces<DecimalPlacesAttribute>();
				result = (ZDecimal)propertyInfo.GetValue(obj, null);
				if (decimalPlaces != null)
				{
					result = result.Round(decimalPlaces.DecimalPlaces);
				}
			}

			if (normalize)
			{
				result = result.Normalize();
			}

			return result;
		}

		static T GetCustomAttributeIncludingInterfaces<T>(this MemberInfo element) where T : Attribute
			=> element.GetCustomAttribute<T>(true)
				?? element.DeclaringType.GetInterfaces().Select(interfaceType => interfaceType.GetProperty(element.Name)?.GetCustomAttribute<T>(true)).FirstOrDefault();

		public static ZString GetStringValueByPropertyWithMaxLengthAttribute(this object obj, string name)
		{
			var result = ZString.Empty;
			var propertyInfo = obj.GetPropertyInfo(name);
			if (propertyInfo != null && propertyInfo.PropertyType == typeof(ZString))
			{
				var maxLength = propertyInfo.GetCustomAttributeIncludingInterfaces<MaxLengthAttribute>();
				result = (ZString)propertyInfo.GetValue(obj, null);
				if (maxLength != null)
				{
					result = result.Left(maxLength.MaxLength);
				}
			}
			return result;
		}

		static PropertyInfo GetPropertyInfo(this object obj, string name)
		{
			var objType = obj.GetType();
			return objType.GetProperty(name) ?? objType.GetInterfaces().Select(interfaceType => interfaceType.GetProperty(name)).FirstOrDefault(prop => prop != null);
		}
	}
}
