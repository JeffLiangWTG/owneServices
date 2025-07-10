using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAirlineMAWBStockManagement))]
	sealed class OrgAirlineMAWBStockManagementTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationAndLookups()
		{
			var airlineData = Factory.New<OrgAirlineMAWBStockManagement>();
			AssertEquals(typeof(OrgAirlineMAWBStockManagementValidation), airlineData.Validation.GetType());
			AssertEquals(typeof(OrgAirlineMAWBStockManagementLookups), airlineData.Lookups.GetType());
		}
	}
}
