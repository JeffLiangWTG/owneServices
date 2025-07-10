using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	public static class PropertyInfoTestHeapler
	{
		public static void SetNotEmptyValue(this ZPropertyInfo info)
		{
			if (info.PropertyType == typeof(ZString))
			{
				info.Value = new ZString("A");
				if ((ZString)(object)info.Value == ZString.Empty)
				{
					info.Value = new ZString("9");
				}
			}
			else if (info.PropertyType == typeof(ZInt))
			{
				info.Value = (ZInt)1;
			}
			else if (info.PropertyType == typeof(ZDecimal))
			{
				info.Value = (ZDecimal)1;
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				info.Value = ZDateTime.Now;
			}
			else if (info.PropertyType == typeof(ZDateTimeOffset))
			{
				info.Value = ZDateTimeOffset.Now;
			}
			else if (info.PropertyType == typeof(ZDate))
			{
				info.Value = ZDate.Today;
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				info.Value = ZGuid.NewZGuid();
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				info.Value = ZBool.True;
			}
			else if (info.PropertyType == typeof(ZShort))
			{
				info.Value = (ZShort)2;
			}
			else if (info.PropertyType == typeof(ZByte))
			{
				info.Value = (ZByte)1;
			}
			else
			{
				throw new NotSupportedException("Not supported type: " + info.PropertyType);
			}
		}
	}
}
