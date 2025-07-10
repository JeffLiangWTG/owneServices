using CargoWise.Types;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public static class UnitHelper
	{
		public static ZDecimal GetValueFromValueAndUnit(ZString valueAndUnit)
		{
			var value = valueAndUnit.KeepCharsUntil("0123456789.", " ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
			return ZDecimal.ParseSafe(value, 0);
		}

		public static ZString GetUnitFromValueAndUnit(ZString valueAndUnit)
		{
			var value = GetValueFromValueAndUnit(valueAndUnit);
			return valueAndUnit.ReplaceIgnoringCase(value.ToString(), "").Trim();
		}
	}
}
