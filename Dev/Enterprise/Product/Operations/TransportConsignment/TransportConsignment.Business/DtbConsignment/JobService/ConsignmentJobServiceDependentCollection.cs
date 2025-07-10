using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class ConsignmentJobServiceDependentCollection : JobServiceDependentCollection
	{
		public ConsignmentJobServiceDependentCollection(DtbConsignment dtbConsignment, BusinessObjectFactory factory)
			: base(dtbConsignment, factory)
		{
		}

		public new ConsignmentJobService this[int index]
		{
			get { return (ConsignmentJobService)base[index]; }
		}

		public new ConsignmentJobService AddNew()
		{
			return (ConsignmentJobService)base.AddNew();
		}
	}
}
