using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	class ForwardingUNDGSubstanceCollection : UNDGSubstanceCollection
	{
		public ForwardingUNDGSubstanceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ForwardingUNDGSubstanceCollection(BusinessObjectFactory factory, ForwardingShipment parentShipment)
			: base(factory)
		{
			this.parentShipment = parentShipment;
		}

		readonly ForwardingShipment parentShipment;

		public ForwardingUNDGSubstanceCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override IFindBoxListProvider FindBoxListProvider => new ForwardingUNDGSubstanceCollectionFindBoxListProvider(this, parentShipment);
	}
}
