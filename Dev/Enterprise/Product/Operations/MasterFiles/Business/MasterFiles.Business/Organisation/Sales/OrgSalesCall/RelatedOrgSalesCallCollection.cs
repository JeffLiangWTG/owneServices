using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedOrgSalesCallCollection : OrgSalesCallCollection
	{
		public RelatedOrgSalesCallCollection(IRelatableActivity master)
			: base(master.Factory, new RelatedOrgSalesCallRelationship(master, new ZQuery()))
		{
		}

		class RelatedOrgSalesCallRelationship : ManyToManyRelationship
		{
			public RelatedOrgSalesCallRelationship(IRelatableActivity master, ZQuery filter)
				: base((BusinessObject)master, typeof(OrgSalesCall), typeof(ViewRelatedActivityPivot), filter, ViewRelatedActivityPivotSchema.RAP_ParentActivityID, ViewRelatedActivityPivotSchema.RAP_ChildActivityID)
			{
			}

			new IRelatableActivity Master
			{
				get { return (IRelatableActivity)base.Master; }
			}

			protected override string GetPivotDataViewRowFilter()
			{
				return base.GetPivotDataViewRowFilter() + " OR " + PivotTableFKToElements.Name + "='" + Master.PK + "'";
			}

			protected override ZGuid GetElementFkOfPivot(BusinessObject pivot)
			{
				var result = base.GetElementFkOfPivot(pivot);
				if (result == Master.PK)
				{
					result = (ZGuid)pivot[PivotTableFKToMaster];
				}
				return result;
			}

			protected override void InitPivots(BusinessObjectFactory factory, SchemaGuidColumn pivotTableFKToMaster, SchemaGuidColumn pivotTableFKToElements)
			{
				base.InitPivots(factory, pivotTableFKToMaster, pivotTableFKToElements);
				base.InitPivots(factory, pivotTableFKToElements, pivotTableFKToMaster);
			}
		}
	}
}
