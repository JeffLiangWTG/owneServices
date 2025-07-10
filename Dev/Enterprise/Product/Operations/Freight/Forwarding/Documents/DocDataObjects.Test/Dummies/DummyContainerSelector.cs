using CargoWise.Macros;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyContainerSelector : IContainerSelector
	{
		public DummyContainerSelector(CommonContainer[] containersToReturn)
		{
			this.containersToReturn = containersToReturn;
		}

		readonly CommonContainer[] containersToReturn;

		public Either<string, CommonContainer[]> SelectContainers(CommonContainer[] containers, ContainerSelectorMode mode = ContainerSelectorMode.Print) => containersToReturn;
	}
}
