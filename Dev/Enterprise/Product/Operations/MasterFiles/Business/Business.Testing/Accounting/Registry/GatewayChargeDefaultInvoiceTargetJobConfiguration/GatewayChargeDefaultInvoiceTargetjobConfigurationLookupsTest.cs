using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GatewayChargeDefaultInvoiceTargetjobConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConsolDirectionList()
		{
			AssertEquals(5, Lookups.ConsolDirectionList.Count);
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("ALL"));
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("IMP"));
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("EXP"));
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("DOM"));
			AssertEquals(true, Lookups.ConsolDirectionList.ContainsCode("OTH"));
		}

		public void TestConsolTransportModeList()
		{
			AssertEquals(5, Lookups.ConsolTransportModeList.Count);
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("ALL"));
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("AIR"));
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("SEA"));
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("ROA"));
			AssertEquals(true, Lookups.ConsolTransportModeList.ContainsCode("RAI"));
		}

		public void TestPreviousSendingAgentTypeList()
		{
			AssertEquals(5, Lookups.PreviousSendingAgentTypeList.Count);
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("ALL"));
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("SGT"));
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("GTA"));
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("GTT"));
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("NON"));
		}

		public void TestChargeGroupList()
		{
			AssertEquals(5, Lookups.PreviousSendingAgentTypeList.Count);
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("ALL"));
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("SGT"));
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("GTA"));
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("GTT"));
			AssertEquals(true, Lookups.PreviousSendingAgentTypeList.ContainsCode("NON"));
		}

		public void TestConsolPaymentTermList()
		{
			AssertEquals(3, Lookups.InvoiceTargetJobTypeList.Count);
			AssertEquals(true, Lookups.InvoiceTargetJobTypeList.ContainsCode("REL"));
			AssertEquals(true, Lookups.InvoiceTargetJobTypeList.ContainsCode("PCL"));
			AssertEquals(true, Lookups.InvoiceTargetJobTypeList.ContainsCode("SCL"));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new GatewayChargeDefaultInvoiceTargetJobConfiguration();
			Lookups = new GatewayChargeDefaultInvoiceTargetJobConfigurationLookups(configuration);
		}
		GatewayChargeDefaultInvoiceTargetJobConfigurationLookups Lookups;
		GatewayChargeDefaultInvoiceTargetJobConfiguration configuration;

		#endregion
	}
}
