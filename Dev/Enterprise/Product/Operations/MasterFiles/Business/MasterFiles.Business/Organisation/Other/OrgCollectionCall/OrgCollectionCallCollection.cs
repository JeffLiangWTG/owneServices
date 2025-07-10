using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCollectionCallCollection : BusinessObjectCollection<OrgCollectionCall>
	{
		public OrgCollectionCallCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCollectionCallCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(vw_OrgCollectionCallSchema.CC_GC, GlbCompany.CurrentCompany.PK);
			return filter;
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This bizobj is based on a view, you cannot create a new one");
		}
	}
}
