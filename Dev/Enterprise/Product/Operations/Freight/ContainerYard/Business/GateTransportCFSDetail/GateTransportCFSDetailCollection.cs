using CargoWise.EntityFramework;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCFSDetailCollection : ActiveBusinessObjectCollection<GateTransportCFSDetail>
	{
		public GateTransportCFSDetailCollection(GateTransport parent)
			: base(parent)
		{
		}
	}
}
