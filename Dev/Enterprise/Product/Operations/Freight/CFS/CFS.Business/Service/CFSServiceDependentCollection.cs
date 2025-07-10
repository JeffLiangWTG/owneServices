
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSServiceDependentCollection : JobServiceDependentCollection
	{
		public CFSServiceDependentCollection(BusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public new CFSService this[int index]
		{
			get { return (CFSService)(Elements[index]); }
		}

		public new CFSService AddNew()
		{
			return (CFSService)base.AddNew();
		}
	}
}
