using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPartAttributeValidationConsumer
	{
		bool IsRegisteredForUniqueSerialNumberChecking { get; }
		bool IsValidForUniqueSerialNumberChecking(ZString serialNumber);
		bool IsSerialNumberUsedOnThis(ZString serialNumber);
		bool IsSerialNumberUsedOnSiblings(ZString serialNumber);
		bool IsInventoryAdjustedOutOnSiblings(WhsInventoryView inventory);
	}
}
