using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class SetValidValueHelper
	{
		public static void SetDateIfValid(string dateISOValue, Action<ZDateTime> setValue)
		{
			ZDateTime value;
			if (ZDateTime.TryParseISO8601Date(dateISOValue, out value))
			{
				setValue(value);
			}
		}

		public static void SetStringIfValid(string value, Action<string> setValue)
		{
			if (!string.IsNullOrEmpty(value))
			{
				setValue(value);
			}
		}

		public static void SetGuidIfValid(string guidValue, Action<ZGuid> setValue)
		{
			ZGuid value;
			if (ZGuid.TryParse(guidValue, out value))
			{
				setValue(value);
			}
		}

		public static void SetDecimalIfValid(string decimalValue, Action<ZDecimal> setValue)
		{
			ZDecimal value;
			if (ZDecimal.TryParse(decimalValue, out value))
			{
				setValue(value);
			}
		}

		public static void SetByteIfValid(string byteValue, Action<ZByte> setValue)
		{
			ZByte value;
			if (ZByte.TryParse(byteValue, out value))
			{
				setValue(value);
			}
		}

		public static void SetBoolIfValid(string boolValue, Action<ZBool> setValue)
		{
			ZBool value;
			if (boolValue != null && ZBool.TryParse(boolValue, out value))
			{
				setValue(value);
			}
		}
	}
}
