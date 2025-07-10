using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondContainerCollectionShipmentSynchroniser : Customs.Business.CusInBondContainerCollectionSynchroniser
	{
		public CusInBondContainerCollectionShipmentSynchroniser(ForwardingShipment source, CusInBondMoveDetail destination, ForwardingConsol consolSource, bool shouldSynchronise, CusInBondContainerCollection containers)
			: base(source, destination, consolSource, shouldSynchronise, containers)
		{
		}

		protected override Customs.Business.ContainerSynchroniser<Customs.Business.CusInBondContainer> GetContainerSynchroniser(Customs.Business.CusInBondContainer billContainer, ForwardingContainer container)
		{
			return new CusInBondContainerShipmentSynchroniser((CusInBondContainer)billContainer, container, Source);
		}
	}
}
