using CargoWise.EntityFramework;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCYDetailCollection : ActiveBusinessObjectCollection<GateTransportCYDetail>
	{
		public GateTransportCYDetailCollection(GateTransport parent)
			: base(parent)
		{
		}
	}
}
