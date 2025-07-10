using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class UNDGDataItemCollectionSynchroniser : Customs.Business.UNDGDataItemCollectionSynchroniser
	{
		public UNDGDataItemCollectionSynchroniser(ForwardingShipment source, Customs.Business.CusInBondContainer destination)
			: base(source, destination)
		{
		}

		public new CusInBondContainer Destination
		{
			get { return (CusInBondContainer)base.Destination; }
		}

		protected override void SetReadOnlyIncludingChildren(bool isReadOnly)
		{
			Destination.UNDGs.SetReadOnlyIncludingChildren(isReadOnly);
		}
	}
}
