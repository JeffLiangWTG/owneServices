using System;

namespace CargoWise.eHub.Products.GBCustoms.Core.Providers
{
	public static class EnumExtensions
	{
		public static string ConvertToString(this Enum enumValue)
		{
			return enumValue == null ? "Unknown" : enumValue.ToString();
		}

		public static ProviderType ConvertToEnum(this string stringValue)
		{
			if (Enum.TryParse(stringValue, out ProviderType provider))
			{
				return provider;
			}

			return ProviderType.Unknown;
		}
	}
}
