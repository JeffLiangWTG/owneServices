using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobTradeLane))]
	sealed class JobTradeLaneTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEJ_Location1()
		{
			AssertHasCustomAttribute(typeof(JobTradeLane),
				JobTradeLaneSchema.Constants.EJ_Location1, false, (CargoWise.ComponentModel.ListAttribute la) => la.ListDataSourceMember == "Lookups.Locations");
		}

		public void TestEJ_Location2()
		{
			AssertHasCustomAttribute(typeof(JobTradeLane),
				JobTradeLaneSchema.Constants.EJ_Location2, false, (CargoWise.ComponentModel.ListAttribute la) => la.ListDataSourceMember == "Lookups.Locations");
		}

		public void TestEJ_Direction()
		{
			AssertHasCustomAttribute(typeof(JobTradeLane),
				JobTradeLaneSchema.Constants.EJ_Direction, false, (CargoWise.ComponentModel.ListAttribute la) => la.ListDataSourceMember == "Lookups.DirectionTypes");
		}

		public void TestIsAutoLogged()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "Carrier";
			carrier.OH_IsShippingProvider = true;
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			tradeLane.EJ_OH_RelatedOrg = carrier.PK;
			tradeLane.EJ_Code = "STL";
			tradeLane.EJ_Location1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "HKHKG").RL_Code;
			tradeLane.EJ_Location2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE").RL_Code;
			Factory.Save();

			Assert(tradeLane.Logs.GetAllLogs().Count > 0);
		}

		public void TestHumanReadableName()
		{
			JobTradeLane lane = Factory.New<JobTradeLane>();
			AssertEquals("TradeLane without EJ_Code", "Trade Lane", lane.HumanReadableName);
			lane.EJ_Code = "1234";
			AssertEquals("TradeLane with EJ_Code", "Trade Lane 1234", lane.HumanReadableName);
		}
	}
}
