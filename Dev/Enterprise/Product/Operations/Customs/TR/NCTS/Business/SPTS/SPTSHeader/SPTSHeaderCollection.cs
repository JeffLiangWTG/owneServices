using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSHeaderCollection : ActiveBusinessObjectCollection<SPTSHeader>
	{
		public SPTSHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
		public SPTSHeaderCollection(BusinessObjectFactory factory, GlbCompany company) : base(factory, GetSPTSFilter(company))
		{
		}

		static ZQuery GetSPTSFilter(GlbCompany company)
		{
			var result = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.SPTS);
			var query = new ZDBOnlyQuery(typeof(SPTSHeader));
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusInBondHeaderSchema.BH_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, company.PK);
			query.AddSubQuery(branchQuery, JoinCondition.And);
			result.AddToFilter(query);
			return result;
		}
	}
}
