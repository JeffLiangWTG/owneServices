using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class StowPlanNotificationCollectorTest : TestCaseWithFactory
	{
		public void TestIssues()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(10);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(20);
			voyage.GenerateSailings();
			var bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			var sailingData = new StowPlanSailingData(voyage);
			sailingData.RebuidIssues();
			var issue = sailingData.IssueCollection.Cast<StowPlanMessageIssue>().FirstOrDefault(x => x.Detail.Contains(StowPlanShipmentDataValidation.NoContainersMsg));
			AssertEquals(bill.PK, issue.TargetPK);
		}
	}
}
