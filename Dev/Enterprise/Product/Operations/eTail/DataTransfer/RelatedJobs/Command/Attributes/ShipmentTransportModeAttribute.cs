using System;

namespace Enterprise.eTail.DataTransfer
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ShipmentTransportModeAttribute : Attribute
	{
		public ShipmentTransportModeAttribute(string[] transportModes)
		{
			TransportModes = transportModes;
		}

		public readonly string[] TransportModes;
	}
}
