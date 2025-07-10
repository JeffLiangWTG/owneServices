using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class AccExchangeRateConfigurationsQueryProviderFactory
	{
		public static IAccExchangeRateConfigurationsQueryProvider CreateCompanyLevelProvider(ZGuid companyPK, ZQuery additionalCompanySubQuery)
		{
			return new CompanyLevelExchangeRateConfigurationsQueryProvider(companyPK, additionalCompanySubQuery);
		}

		public static IAccExchangeRateConfigurationsQueryProvider CreateOrganizationGroupLevelProvider(ZGuid companyPK, ZString ledger, ZGuid orgGroupPK, ZQuery additionalCompanySubQuery)
		{
			return new OrganizationGroupLevelExchangeRateConfigurationsQueryProvider(companyPK, orgGroupPK, ledger, additionalCompanySubQuery);
		}

		public static IAccExchangeRateConfigurationsQueryProvider CreateOrganizationLevelProvider(ZGuid orgHeaderPK, ZQuery additionalSubQuery)
		{
			return new OrganizationLevelExchangeRateConfigurationsQueryProvider(orgHeaderPK, additionalSubQuery);
		}
	}
}
