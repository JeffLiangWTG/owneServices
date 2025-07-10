using Enterprise.Freight.Integration;

namespace Enterprise.ContractManagement.GUI
{
	public class ContractAndAllocationsAttachFormFactory : IContractAndAllocationsAttachFormFactory
	{
		public object CreateForm(IContractSimulationFormConfiguration configurationFactory)
		{
			return new ContractAndAllocationsAttachForm(configurationFactory);
		}
	}
}
