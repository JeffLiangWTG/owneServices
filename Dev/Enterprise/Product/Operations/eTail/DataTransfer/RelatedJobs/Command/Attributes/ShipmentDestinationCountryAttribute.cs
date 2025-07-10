using System;

namespace Enterprise.eTail.DataTransfer
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ShipmentDestinationCountryAttribute : Attribute
	{
		public ShipmentDestinationCountryAttribute(string countryCode)
		{
			CountryCode = countryCode;
		}

		public readonly string CountryCode;
	}
}
