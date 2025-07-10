using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContractSimulationFilterDefaults
	{
		FilterBusinessObjectDefaults GetFilterDefaultsForContract();
	}
}
