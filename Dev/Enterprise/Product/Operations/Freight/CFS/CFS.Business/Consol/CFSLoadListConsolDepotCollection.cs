using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolDepotCollection : DepotCollection
	{
		public CFSLoadListConsolDepotCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery();

			ZDBOnlyQuery companyOrBranchQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.GC_OH_OrgProxy);
			ZDBOnlySubQuery branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_OH_OrgProxy);

			companyOrBranchQuery.AddSubQuery(companyQuery, JoinCondition.Or);
			companyOrBranchQuery.AddSubQuery(branchQuery, JoinCondition.Or);

			query.AddToFilter(companyOrBranchQuery);
			query.AddToFilter(base.CreateAdditionalFilter());

			return query;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			var companyProxies = Factory.GetCachedValue("CFSLoadListConsolDepotCollection.AddNotificationWhenAdditionalFilterNotMet.CompanyProxies",
									() => Factory.Load<GlbCompany>(new ZQuery()).Select(item => item.GC_OH_OrgProxy).ToList());

			var branchProxies = Factory.GetCachedValue("CFSLoadListConsolDepotCollection.AddNotificationWhenAdditionalFilterNotMet.BranchProxies",
									() => Factory.Load<GlbBranch>(new ZQuery()).Select(item => item.GB_OH_OrgProxy).ToList());

			OrgHeader org = (OrgHeader)selectedBusinessObject;
			if (!companyProxies.Contains(org.PK) && !branchProxies.Contains(org.PK))
			{
				errors.Add(Res.GetString("b5ed3a7f-7782-494d-9604-df3c4947c7fd", "Only Organizations which are defined in System -> Companies or System -> Branches can be chosen here."));
			}
			else
			{
				base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			}
		}
	}
}
