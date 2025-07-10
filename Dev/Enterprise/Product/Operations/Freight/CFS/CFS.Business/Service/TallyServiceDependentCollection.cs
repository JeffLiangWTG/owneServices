
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyServiceDependentCollection : CFSServiceDependentCollection
	{
		public TallyServiceDependentCollection(BusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public new TallyService this[int index]
		{
			get { return (TallyService)(Elements[index]); }
		}

		public new TallyService AddNew()
		{
			return (TallyService)base.AddNew();
		}
	}
}
