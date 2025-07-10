using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class AccExchangeRateConfigurationsQueryProviderFactoryTest : TestCase
	{
		public void TestCreateProvider()
		{
			var companyLevelProvider = AccExchangeRateConfigurationsQueryProviderFactory.CreateCompanyLevelProvider(ZGuid.Empty, new ZQuery());
			AssertType<CompanyLevelExchangeRateConfigurationsQueryProvider>(companyLevelProvider);

			var organizationGroupLevelProvider = AccExchangeRateConfigurationsQueryProviderFactory.CreateOrganizationGroupLevelProvider(ZGuid.Empty, "AP", ZGuid.Empty, new ZQuery());
			AssertType<OrganizationGroupLevelExchangeRateConfigurationsQueryProvider>(organizationGroupLevelProvider);

			var organizationLevelProvider = AccExchangeRateConfigurationsQueryProviderFactory.CreateOrganizationLevelProvider(ZGuid.Empty, new ZQuery());
			AssertType<OrganizationLevelExchangeRateConfigurationsQueryProvider>(organizationLevelProvider);
		}
	}
}
