using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsJobServiceDependentCollection : JobServiceDependentCollection
	{
		public WhsJobServiceDependentCollection(WhsDocket docket, BusinessObjectFactory factory)
			: base(docket, factory)
		{
		}

		public WhsJobServiceDependentCollection(WhsAdHocServiceJob adhocServiceJob, BusinessObjectFactory factory)
			: base(adhocServiceJob, factory)
		{
		}

		public WhsJobServiceDependentCollection(WhsVASOrder vasOrder, BusinessObjectFactory factory)
			: base(vasOrder, factory)
		{
		}

		protected override bool AllowNewCore => base.AllowNewCore && (Master is not WhsDocket docket || !docket.IsCreatedFromPickByBOM);

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
