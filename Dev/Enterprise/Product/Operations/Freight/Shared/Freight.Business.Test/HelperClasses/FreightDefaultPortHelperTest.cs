using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightDefaultPortHelperTest : TestCaseWithFactory
	{
		public void TestGetDefaultShipmentOriginPort()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ShipmentOrigin, "AIR", "LSE", "AUMEL");
			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ShipmentOrigin, "SEA", "LCL", "AUBNE");
			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ShipmentOrigin, "ROA", "LTL", "AUADL");
			Factory.Save();

			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultShipmentOriginPort(branch, "AIR", "LSE"));
			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultShipmentOriginPort(branch, "FAS", "LSE"));

			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultShipmentOriginPort(branch, "FAS", "BCN"));
			AssertEquals("AUBNE", FreightDefaultPortHelper.GetDefaultShipmentOriginPort(branch, "SEA", "BCN"));
			AssertEquals("AUADL", FreightDefaultPortHelper.GetDefaultShipmentOriginPort(branch, "ROA", "BCN"));

			AssertEquals("AUSYD", FreightDefaultPortHelper.GetDefaultShipmentOriginPort(branch, "SEA", "FCL"));
		}

		public void TestGetDefaultShipmentDestinationPort()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ShipmentDestination, "SEA", "FCL", "AUMEL");
			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ShipmentDestination, "AIR", "ULD", "AUBNE");
			branch.AddDefaultPort(GlbBranchDefaultToList.Codes.ShipmentDestination, "ROA", "FTL", "AUADL");
			Factory.Save();

			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultShipmentDestinationPort(branch, "SEA", "FCL"));
			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultShipmentDestinationPort(branch, "FSA", "FCL"));

			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultShipmentDestinationPort(branch, "FSA", "BCN"));
			AssertEquals("AUBNE", FreightDefaultPortHelper.GetDefaultShipmentDestinationPort(branch, "AIR", "BCN"));
			AssertEquals("AUADL", FreightDefaultPortHelper.GetDefaultShipmentDestinationPort(branch, "ROA", "BCN"));

			AssertEquals("AUSYD", FreightDefaultPortHelper.GetDefaultShipmentDestinationPort(branch, "SEA", "LCL"));
		}

		public void TestGetDefaultConsolFirstLoadPort()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			GlbBranchDefaultPortTestHelper.AddDefaultPort(branch, GlbBranchDefaultToList.Codes.ConsolFirstLoad, "AIR", "LSE", "AUMEL");
			GlbBranchDefaultPortTestHelper.AddDefaultPort(branch, GlbBranchDefaultToList.Codes.ConsolFirstLoad, "SEA", "LCL", "AUBNE");
			GlbBranchDefaultPortTestHelper.AddDefaultPort(branch, GlbBranchDefaultToList.Codes.ConsolFirstLoad, "ROA", "LTL", "AUADL");
			Factory.Save();

			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultConsolFirstLoadPort(branch, "AIR", "LSE"));
			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultConsolFirstLoadPort(branch, "FAS", "LSE"));

			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultConsolFirstLoadPort(branch, "FAS", "BCN"));
			AssertEquals("AUBNE", FreightDefaultPortHelper.GetDefaultConsolFirstLoadPort(branch, "SEA", "BCN"));
			AssertEquals("AUADL", FreightDefaultPortHelper.GetDefaultConsolFirstLoadPort(branch, "ROA", "BCN"));

			AssertEquals("AUSYD", FreightDefaultPortHelper.GetDefaultConsolFirstLoadPort(branch, "SEA", "FCL"));
		}

		public void TestGetDefaultConsolLastDischargePort()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			GlbBranchDefaultPortTestHelper.AddDefaultPort(branch, GlbBranchDefaultToList.Codes.ConsolLastDischarge, "SEA", "FCL", "AUMEL");
			GlbBranchDefaultPortTestHelper.AddDefaultPort(branch, GlbBranchDefaultToList.Codes.ConsolLastDischarge, "AIR", "ULD", "AUBNE");
			GlbBranchDefaultPortTestHelper.AddDefaultPort(branch, GlbBranchDefaultToList.Codes.ConsolLastDischarge, "ROA", "FTL", "AUADL");
			Factory.Save();

			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultConsolLastDischargePort(branch, "SEA", "FCL"));
			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultConsolLastDischargePort(branch, "FSA", "FCL"));

			AssertEquals("AUMEL", FreightDefaultPortHelper.GetDefaultConsolLastDischargePort(branch, "FSA", "BCN"));
			AssertEquals("AUBNE", FreightDefaultPortHelper.GetDefaultConsolLastDischargePort(branch, "AIR", "BCN"));
			AssertEquals("AUADL", FreightDefaultPortHelper.GetDefaultConsolLastDischargePort(branch, "ROA", "BCN"));

			AssertEquals("AUSYD", FreightDefaultPortHelper.GetDefaultConsolLastDischargePort(branch, "SEA", "LCL"));
		}
	}
}
