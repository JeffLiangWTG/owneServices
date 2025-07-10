using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentCollection : ActiveBusinessObjectCollection<DtbConsignment>
	{
		public DtbConsignmentCollection(DtbConsignment consignment)
			: base(consignment)
		{
		}

		protected DtbConsignmentCollection(DtbConsignment consignment, ZQuery filter)
			: base(consignment, filter)
		{
		}

		public DtbConsignmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
