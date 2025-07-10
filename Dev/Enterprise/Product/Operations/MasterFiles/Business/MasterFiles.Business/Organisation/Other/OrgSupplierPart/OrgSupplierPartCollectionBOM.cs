using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartCollectionBOM : OrgSupplierPartCollection
	{
		public OrgSupplierPartCollectionBOM(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollectionBOM(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport, PartFilterOptions filterOptions = PartFilterOptions.None)
			: base(factory, supplier, owner, isExport, filterOptions)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();

			ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			ZDBOnlySubQuery orgPartBomSubQuery = new ZDBOnlySubQuery(typeof(OrgPartBOM), OrgPartBOMSchema.OE_OP_MainProduct);
			dbQuery.AddSubQuery(orgPartBomSubQuery, JoinCondition.And);

			result.AddToFilter(dbQuery);
			return result;
		}
	}
}
