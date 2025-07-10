using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface IPackableItem
	{
		// BizO properties for PkgPackageItemDivot to Link to this
		ZGuid PK { get; }
		string TablePrefix { get; }
		bool IsDeleted { get; }
		bool IsInDatabase { get; }

		// Packing Properties
		ZDecimal Quantity { get; }
		GroupingKey Key { get; }

		// Required for Packing Divots when Packing Item's Qty is more than what is required to Pack / Unpack
		IPackableItem Split(ZDecimal qtyToSplit);
		// Allow consumer to re-consolidate PackedItem if necessary
		void ReMerge();
	}
}
