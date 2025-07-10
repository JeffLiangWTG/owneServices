using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCollectionForLink : OrganisationsFindBoxCollection
	{
		public OrgCollectionForLink(BusinessObjectFactory factory, OrgHeader orgMaster, OrgHeader orgTarget)
			: base(factory)
		{
			OrgMaster = orgMaster;
			OrgTarget = orgTarget;
		}

		public OrgHeader OrgMaster { get; }
		public OrgHeader OrgTarget { get; }

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, OrgMaster.PK); // Don't show org1
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, OrgTarget.PK); // Don't show org2
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}
	}
}
