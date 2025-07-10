using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class ZPropertyInfoExtensions
	{
		public static void SetValueSafe(this ZPropertyInfo<ZString> propertyInfo, ZString value)
		{
			propertyInfo.Value = value.Left(propertyInfo.MaxLength);
		}

		public static T GetAttribute<T>(this ZPropertyInfo info)
			where T : Attribute
		{
			return (T)info.PropertyDescriptor.Attributes[typeof(T)];
		}
	}
}
