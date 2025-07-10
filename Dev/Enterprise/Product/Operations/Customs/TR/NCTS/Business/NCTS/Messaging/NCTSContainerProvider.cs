using CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NCTSContainerProvider : INCTSContainers
	{
		public NCTSContainerProvider(NonPersistentDepartureContainerPivot containers)
		{
			this.containers = containers;
		}
		readonly NonPersistentDepartureContainerPivot containers;

		public string ContainerNumber => containers.ContainerNumber;
	}
}
