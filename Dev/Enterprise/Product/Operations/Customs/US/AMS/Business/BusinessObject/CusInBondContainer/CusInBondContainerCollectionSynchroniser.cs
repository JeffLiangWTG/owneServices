using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondContainerCollectionSynchroniser : Customs.Business.CusInBondContainerCollectionSynchroniser
	{
		public CusInBondContainerCollectionSynchroniser(ForwardingShipment source, CusInBondBill destination, ForwardingConsol consol, bool shouldSynchronise, CusInBondContainerCollection containers)
			: base(source, destination, consol, shouldSynchronise, containers)
		{
		}

		protected override ContainerSynchroniser<Customs.Business.CusInBondContainer> GetContainerSynchroniser(Customs.Business.CusInBondContainer billContainer, ForwardingContainer container)
		{
			return new CusInBondContainerSynchroniser((CusInBondContainer)billContainer, container, Source);
		}

		protected override GenericNonContainerizedNumberSynchroniser<Customs.Business.CusInBondContainer> GetNewNonContainerisedNumberSynchroniser(Customs.Business.CusInBondContainer cusContainer, ForwardingShipment source)
		{
			return new NonContainerizedNumberSynchroniser((CusInBondContainer)cusContainer, source);
		}
	}
}
