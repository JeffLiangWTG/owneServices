using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	static class WhsLocationExtensions
	{
		public static bool CanTransferToOrFromLocation(this WhsLocation locationFrom, WhsLocation locationToCompare)
		{
			return locationFrom != null && locationToCompare != null
				&& WhsLocation.CanTransferToOrFromAreaType(locationFrom.WLV_PickingAreaType, locationToCompare.WLV_PickingAreaType);
		}
	}
}
