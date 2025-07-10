using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCollectionForLinkForTest : OrgCollectionForLink
	{
		public OrgCollectionForLinkForTest(BusinessObjectFactory factory, OrgHeader orgMaster, OrgHeader orgTarget) : base(factory, orgMaster, orgTarget)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = base.CreateAdditionalFilter();
			query.AddToFilter(OrgHeaderSchema.OH_IsUserFlag1, SQLComparisonOperator.Equal, true);

			return query;
		}
	}
}
