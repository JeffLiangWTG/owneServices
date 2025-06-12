using System;
using System.ComponentModel;
using System.Reflection;

namespace Enterprise.Freight.DistanceCalculation.Service.Extensions
{
	public static class EnumExtensions
	{
		public static string GetDescription(this Type enumType, string name)
		{
			FieldInfo fieldInfo = enumType.GetField(name);

			if (null != fieldInfo)
			{
				object[] attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
				if (attrs != null && attrs.Length > 0) return ((DescriptionAttribute)attrs[0]).Description;
			}

			return "Error status not recognized";
		}
	}
}