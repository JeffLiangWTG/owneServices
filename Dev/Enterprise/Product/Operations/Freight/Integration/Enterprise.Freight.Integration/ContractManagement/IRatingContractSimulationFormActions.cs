namespace Enterprise.Freight.Integration
{
	public interface IRatingContractSimulationFormActions
	{
		bool IsEnabledAllocationToContract { get; }
		bool TryAllocateToContract(IRatingContract contract);
		bool IsEnabledAllocationToRoute { get; }
		bool TryAllocateToAllocationRoute(IRatingContractAllocationLine allocationRoute);
	}
}
