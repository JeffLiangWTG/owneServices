using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CompanyLevelExchangeRateConfigurationsQueryProvider : IAccExchangeRateConfigurationsQueryProvider
	{
		public CompanyLevelExchangeRateConfigurationsQueryProvider(ZGuid companyPK, ZQuery additionalCompanySubQuery)
		{
			this.companyPK = companyPK;
			this.additionalCompanySubQuery = additionalCompanySubQuery;
		}

		public ZQuery GetQuery()
		{
			var query = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_ParentTableCode, string.Empty);
			query.AddToFilter(AccExchangeRateConfigurationViewSchema.JCE_ParentID, null);

			var companySubQuery = new ZQuery();
			companySubQuery.AddToFilter(AccExchangeRateConfigurationViewSchema.JCE_GC, companyPK);
			companySubQuery.AddToFilter(additionalCompanySubQuery, JoinCondition.Or);

			query.AddToFilter(companySubQuery);
			return query;
		}

		readonly ZGuid companyPK;
		readonly ZQuery additionalCompanySubQuery;
	}
}
