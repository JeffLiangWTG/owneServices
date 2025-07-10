using System.Diagnostics;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DebuggerDisplay("Weight: {Weight}, Volume: {Volume}, Quantity: {Quantity}")]
	public class LocationWithCapacity
	{
		public LocationWithCapacity(WhsLocation location, ZDecimal weight, ZDecimal volume, ZDecimal quantity)
		{
			Location = location;
			Weight = Location.WLV_MaxWeight > 0 ? weight : null;
			Volume = Location.WLV_MaxCubic > 0 ? volume : null;
			Quantity = Location.WLV_MaxQuantity > 0 ? quantity : null;
		}

		public readonly WhsLocation Location;
		public readonly ZDecimal? Weight;
		public readonly ZDecimal? Volume;
		public readonly ZDecimal? Quantity;
	}
}
