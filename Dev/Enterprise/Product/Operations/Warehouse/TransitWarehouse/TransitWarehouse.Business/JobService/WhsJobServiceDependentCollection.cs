using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsJobServiceDependentCollection : JobServiceDependentCollection
	{
		public WhsJobServiceDependentCollection(WhsItemReceiveConsignment rcn, BusinessObjectFactory factory)
			: base(rcn, factory)
		{
		}

		public WhsJobServiceDependentCollection(WhsItemDispatchConsignment dcn, BusinessObjectFactory factory)
			: base(dcn, factory)
		{
		}

		public new WhsJobService this[int index]
		{
			get { return (WhsJobService)base[index]; }
		}

		public new WhsJobService AddNew()
		{
			return (WhsJobService)base.AddNew();
		}
	}
}
