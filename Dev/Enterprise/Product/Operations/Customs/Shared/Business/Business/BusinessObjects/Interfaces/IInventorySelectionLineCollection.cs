using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface IInventorySelectionLineCollection<out TInventorySelectionLine> :
		IBusinessObjectCollection<TInventorySelectionLine>
		where TInventorySelectionLine : InventorySelectionLine
	{
		OrgAddress GetFirstWarehouseAddress();

		bool HasALeastOneLineWithDrawQty { get; }

		bool WillDrawLineFromDifferentWarehouses { get; }
	}
}
