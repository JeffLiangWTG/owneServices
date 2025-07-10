using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class ExportAWBHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAWBTypeList()
		{
			AssertNotNull(lookups.AWBTypeList);
		}

		public void TestMessagingStatusList()
		{
			AssertNotNull(lookups.MessagingStatusList);
		}

		public void TestAirlines()
		{
			AssertNotNull(lookups.Airlines);
		}

		public void TestAgentApprovalCategoryList()
		{
			AssertNotNull(lookups.AgentApprovalCategoryList);
		}

		#region Implementation

		protected override void SetUp()
		{
			lookups = new ExportAWBHeaderLookups(Factory.New<ExportAWBHeader>());
		}

		ExportAWBHeaderLookups lookups;

		#endregion
	}
}
