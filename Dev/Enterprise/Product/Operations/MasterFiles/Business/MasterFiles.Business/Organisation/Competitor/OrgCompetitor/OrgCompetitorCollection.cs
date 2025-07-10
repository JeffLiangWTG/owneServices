using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompetitorCollection : ActiveBusinessObjectCollection<OrgCompetitor>
	{
		public OrgCompetitorCollection(OrgHeader org) : base(org.Factory, GetOrgCompetitorCollectionQuery(org))
		{
			master = org;
		}

		readonly OrgHeader master;

		protected override void SetDefaultsForNewElementCore(OrgCompetitor newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.OCP_OH_Parent = master.PK;
		}

		static ZQuery GetOrgCompetitorCollectionQuery(OrgHeader org)
		{
			var query = new ZQuery(OrgCompetitorSchema.OCP_OH_Parent, org.PK);

			var companyFilter = new ZQuery(OrgCompetitorSchema.OCP_GC_Company, null);
			companyFilter.AddToFilter(JoinCondition.Or, OrgCompetitorSchema.OCP_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(companyFilter);

			return query;
		}
	}
}
