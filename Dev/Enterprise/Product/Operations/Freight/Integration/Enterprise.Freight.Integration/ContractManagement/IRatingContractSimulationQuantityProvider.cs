using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContractSimulationQuantityProvider
	{
		ZDecimal GetTotalTEUQuantityForAllocation();
		ZDecimal GetTotalCNQuantityForAllocation();
		ZDecimal GetTotalTEUQuantityNotAllocatedToContract(IRatingContract ratingContract);
		ZDecimal GetTotalCNQuantityNotAllocatedToContract(IRatingContract ratingContract);
	}
}
