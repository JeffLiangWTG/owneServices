using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsCycleCountLocationService
	{
		void MarkInventoriesLostInCycleCount(ZGuid[] cycleCountPKs);
	}
}
