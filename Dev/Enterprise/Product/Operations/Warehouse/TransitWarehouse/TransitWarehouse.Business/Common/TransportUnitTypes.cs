using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business.Common
{
	public static class TransportUnitTypes
	{
		public const string ULD = "ULD"; // Transport Unit Types
		public const string Container = "CNT"; // Transport Unit Types
		public const string Vehicle = "VEH"; // Transport Unit Types
		public const string None = "NON"; // Transport Unit
		public const string Mix = "MIX"; // Transport Unit Types

		public static string ConvertTypeToTransportUnitType(RefContainer containerType)
		{
			switch (containerType.RC_ShippingMode)
			{
				case "AIR":
					return TransportUnitTypes.ULD;
				case "SEA":
					return TransportUnitTypes.Container;
				case "ROA":
					return TransportUnitTypes.Vehicle;
				default:
					throw new NotSupportedException(Res.GetString("F94D8B04-665E-4702-986C-C4B4925D5D69", $"Container shipping mode is not supported."));
			}
		}
	}
}
