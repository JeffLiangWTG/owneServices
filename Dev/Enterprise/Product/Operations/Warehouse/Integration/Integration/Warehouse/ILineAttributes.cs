using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface ILineAttributes : IPartAttributes
	{
		ZString BondedEntryKey { get; }
		ZString AllocationKey { get; }

		void SetAttributes(ILineAttributes src);
	}
}
