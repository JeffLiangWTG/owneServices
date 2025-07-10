using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OpportunitiesCreationTest : TestCaseWithFactory
	{
		public void TestLoadData_Touch()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = master.AllTouches.AddNew();
			touch.G0_HorizontalId = 1;
			touch.G0_CampaignName = "Touch 1";

			var touch2 = master.AllTouches.AddNew();
			touch2.G0_HorizontalId = 2;
			touch2.G0_CampaignName = "Touch 2";

			var org1 = Factory.NewWithValidTestData<OrgOpportunity>();
			org1.P8_Status = "WON";
			org1.P8_G0 = touch.PK;

			var org2 = Factory.NewWithValidTestData<OrgOpportunity>();
			org2.P8_Status = "WON";
			org2.P8_G0 = touch.PK;

			var org3 = Factory.NewWithValidTestData<OrgOpportunity>();
			org3.P8_Status = "CRT";
			org3.P8_G0 = touch.PK;

			var org4 = Factory.NewWithValidTestData<OrgOpportunity>();
			org4.P8_Status = "LOS";
			org4.P8_G0 = touch2.PK;

			var org5 = Factory.NewWithValidTestData<OrgOpportunity>();
			org5.P8_Status = "WON";
			org5.P8_G0 = touch2.PK;

			var org6 = Factory.NewWithValidTestData<OrgOpportunity>();
			org6.P8_Status = "WON";
			org6.P8_G0 = touch.PK;
			Factory.Save();

			var org7 = Factory.NewWithValidTestData<OrgOpportunity>();
			org7.P8_Status = "ABA";
			org7.P8_G0 = touch.PK;
			Factory.Save();

			var org8 = Factory.NewWithValidTestData<OrgOpportunity>();
			org8.P8_Status = "SUS";
			org8.P8_G0 = touch.PK;
			Factory.Save();

			var dataTouch1 = OpportunitiesCreation.LoadOpportunities(touch);
			AssertNotNull(dataTouch1);

			AssertEquals(dataTouch1.CurrentCount, 1);
			AssertEquals(dataTouch1.WonCount, 3);
			AssertEquals(dataTouch1.LostCount, 0);
			AssertEquals(dataTouch1.OtherCount, 2);
			AssertEquals(dataTouch1.TotalCount, 6);
			AssertEquals(dataTouch1.WinRatio, 0.5m);

			var dataTouch2 = OpportunitiesCreation.LoadOpportunities(touch2);
			AssertNotNull(dataTouch2);

			AssertEquals(dataTouch2.CurrentCount, 0);
			AssertEquals(dataTouch2.WonCount, 1);
			AssertEquals(dataTouch2.LostCount, 1);
			AssertEquals(dataTouch2.OtherCount, 0);
			AssertEquals(dataTouch2.TotalCount, 2);
			AssertEquals(dataTouch2.WinRatio, 0.5m);
		}

		public void TestLoadData_MasterList()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = master.AllTouches.AddNew();
			touch.G0_HorizontalId = 1;
			touch.G0_CampaignName = "Touch 1";

			var touch2 = master.AllTouches.AddNew();
			touch2.G0_HorizontalId = 2;
			touch2.G0_CampaignName = "Touch 2";

			var org1 = Factory.NewWithValidTestData<OrgOpportunity>();
			org1.P8_Status = "WON";
			org1.P8_G0 = touch.PK;

			var org2 = Factory.NewWithValidTestData<OrgOpportunity>();
			org2.P8_Status = "WON";
			org2.P8_G0 = touch.PK;

			var org3 = Factory.NewWithValidTestData<OrgOpportunity>();
			org3.P8_Status = "CRT";
			org3.P8_G0 = touch.PK;

			var org4 = Factory.NewWithValidTestData<OrgOpportunity>();
			org4.P8_Status = "LOS";
			org4.P8_G0 = touch.PK;

			var org5 = Factory.NewWithValidTestData<OrgOpportunity>();
			org5.P8_Status = "WON";
			org5.P8_G0 = touch2.PK;

			var org6 = Factory.NewWithValidTestData<OrgOpportunity>();
			org6.P8_Status = "ABA";
			org6.P8_G0 = touch2.PK;

			var org7 = Factory.NewWithValidTestData<OrgOpportunity>();
			org7.P8_Status = "WON";
			org7.P8_G0 = touch.PK;

			var org8 = Factory.NewWithValidTestData<OrgOpportunity>();
			org8.P8_Status = "SUS";
			org8.P8_G0 = touch2.PK;

			var org9 = Factory.NewWithValidTestData<OrgOpportunity>();
			org9.P8_Status = "WON";
			org9.P8_G0 = master.PK;

			var org10 = Factory.NewWithValidTestData<OrgOpportunity>();
			org10.P8_Status = "SUS";
			org10.P8_G0 = master.PK;
			Factory.Save();

			var data = OpportunitiesCreation.LoadOpportunities(master);
			AssertNotNull(data);

			AssertEquals(data.CurrentCount, 1);
			AssertEquals(data.WonCount, 5);
			AssertEquals(data.LostCount, 1);
			AssertEquals(data.OtherCount, 3);
			AssertEquals(data.TotalCount, 10);
			AssertEquals(data.WinRatio, 0.5m);
		}

		public void TestLoadData_MasterWithoutTouches()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var org1 = Factory.NewWithValidTestData<OrgOpportunity>();
			org1.P8_Status = "WON";
			org1.P8_G0 = master.PK;

			var org2 = Factory.NewWithValidTestData<OrgOpportunity>();
			org2.P8_Status = "WON";
			org2.P8_G0 = master.PK;

			var org3 = Factory.NewWithValidTestData<OrgOpportunity>();
			org3.P8_Status = "CRT";
			org3.P8_G0 = master.PK;

			var org4 = Factory.NewWithValidTestData<OrgOpportunity>();
			org4.P8_Status = "LOS";
			org4.P8_G0 = master.PK;
			Factory.Save();

			var data = OpportunitiesCreation.LoadOpportunities(master);
			AssertNotNull(data);

			AssertEquals(data.CurrentCount, 1);
			AssertEquals(data.WonCount, 2);
			AssertEquals(data.LostCount, 1);
			AssertEquals(data.OtherCount, 0);
			AssertEquals(data.TotalCount, 4);
			AssertEquals(data.WinRatio, 0.5m);
		}

		public void TestLoadEmptyData()
		{
			var data = OpportunitiesCreation.LoadOpportunities(null);
			AssertNotNull(data);

			AssertEquals(data.CurrentCount, 0);
			AssertEquals(data.WonCount, 0);
			AssertEquals(data.LostCount, 0);
			AssertEquals(data.OtherCount, 0);
			AssertEquals(data.TotalCount, 0);
			AssertEquals(data.WinRatio, 0m);
		}
	}
}
