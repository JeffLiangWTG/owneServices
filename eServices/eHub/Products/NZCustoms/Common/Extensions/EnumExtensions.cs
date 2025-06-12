using System;
using System.Reflection;
using System.Xml.Serialization;

namespace CargoWise.eHub.Products.NZCustoms.Common
{
	public static class EnumExtensions
	{
		public static string ConvertToString(this Enum enumValue)
		{
			if (enumValue == null) return "";

			Type t = enumValue.GetType();
			FieldInfo info = t.GetField(enumValue.ToString("G"));

			if (!info.IsDefined(typeof(XmlEnumAttribute), false))
			{
				return enumValue.ToString("G");
			}

			object[] attributes = info.GetCustomAttributes(typeof(XmlEnumAttribute), false);
			XmlEnumAttribute attribute = (XmlEnumAttribute)attributes[0];
			return attribute.Name;
		}
	}
}
