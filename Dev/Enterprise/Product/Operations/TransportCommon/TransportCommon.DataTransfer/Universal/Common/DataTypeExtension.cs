using CargoWise.Types;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public static class DataTypeExtension
	{
		public static int GetHashCodeForNullableDataTypes<T>(this T? nullableValue) where T : struct, IZType
		{
			return nullableValue.HasValue ? nullableValue.Value.GetHashCode() : 0;
		}
	}
}
