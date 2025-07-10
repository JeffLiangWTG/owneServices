using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class StowPlanSailingDataLookupsTest : TestCaseWithFactory
	{
		public void TestIssuesTypes()
		{
			var lookups = new StowPlanSailingData(Factory.New<JobVoyage>()).Lookups;
			AssertType(typeof(STWIssueFilterList), lookups.IssueTypes);
		}

		public void TestUSPorts()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUMEL";
			origin.JA_E_DEP = ZDateTime.Today;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(1);
			origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "USCHI";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(2);
			destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(3);

			var sailingData = new StowPlanSailingData(voyage);
			Assert(sailingData.Lookups.USPorts.ContainsCode("USLAX"));
			Assert(sailingData.Lookups.USPorts.ContainsCode("USCHI"));
		}

		public void TestForeignPorts()
		{
			var lookups = new StowPlanSailingData(Factory.New<JobVoyage>()).Lookups;
			AssertType(typeof(RefUNLOCOCollection), lookups.ForeignPorts);
		}
	}
}
