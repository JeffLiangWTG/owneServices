using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class NonContainerizedNumberSynchroniser : Customs.Business.NonContainerizedNumberSynchroniser
	{
		public NonContainerizedNumberSynchroniser(CusInBondContainer destination, ForwardingShipment source)
			: base(destination, source)
		{ }

		public new CusInBondContainer Destination
		{
			get { return (CusInBondContainer)base.Destination; }
		}

		protected override void HookCargoDescCollSynchroniser()
		{
			Synchronisers.Add(new CusInBondCargoDescCollectionSynchroniser(Source, Destination));
		}
	}
}
