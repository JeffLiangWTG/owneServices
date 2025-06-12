using System;

namespace CargoWise.eHub.Products.GBCustoms.Core
{
	public static class EnumExtensions
	{
		public static string ConvertToString(this Enum enumValue)
		{
			return enumValue == null ? "Unknown" : enumValue.ToString();
		}

		public static T ConvertToEnum<T>(this string stringValue) where T : struct, IConvertible
		{
			if (Enum.TryParse(stringValue, out T enumValue))
			{
				return enumValue;
			}

			try
			{
				return (T)Enum.ToObject(typeof(T), 255);
			}
			catch
            {
				throw new InvalidCastException($"Cannot cast to {typeof(T)}");
			}
		}
	}
}
