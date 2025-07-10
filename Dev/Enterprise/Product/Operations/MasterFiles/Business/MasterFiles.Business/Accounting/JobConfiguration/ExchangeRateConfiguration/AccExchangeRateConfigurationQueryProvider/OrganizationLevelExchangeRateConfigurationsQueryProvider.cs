using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrganizationLevelExchangeRateConfigurationsQueryProvider : IAccExchangeRateConfigurationsQueryProvider
	{
		public OrganizationLevelExchangeRateConfigurationsQueryProvider(ZGuid orgHeaderPK, ZQuery additionalCompanySubQuery)
		{
			this.orgHeaderPK = orgHeaderPK;
			this.additionalSubQuery = additionalCompanySubQuery;
		}

		public ZQuery GetQuery()
		{
			var query = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_ParentTableCode, OrgHeaderSchema.Constants.Prefix);
			query.AddToFilter(AccExchangeRateConfigurationViewSchema.JCE_ParentID, orgHeaderPK);
			query.AddToFilter(additionalSubQuery);
			return query;
		}

		readonly ZGuid orgHeaderPK;
		readonly ZQuery additionalSubQuery;
	}
}
