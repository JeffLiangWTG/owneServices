using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TouchReceivedModuleFilter))]
	public class TouchReceivedModuleFilterTest : ModuleTextFilterTest
	{
		public void TestCurrentHorizontal()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var filter = new TouchReceivedModuleFilter("x", helper.Master);
			var horizontals = helper.Master.Horizontals.ToList();

			filter.HorizontalId = "1";
			AssertEquals(horizontals[0], filter.CurrentHorizontal);

			filter.HorizontalId = "2";
			AssertEquals(horizontals[1], filter.CurrentHorizontal);

			filter.HorizontalId = "3";
			AssertEquals(horizontals[2], filter.CurrentHorizontal);

			filter.HorizontalId = "";
			AssertEquals(null, filter.CurrentHorizontal);

			filter.HorizontalId = "5";
			AssertEquals(null, filter.CurrentHorizontal);

			filter.HorizontalId = "f";
			AssertEquals(null, filter.CurrentHorizontal);
		}

		public void TestHorizontalIdCleansVerticalId()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var filter = new TouchReceivedModuleFilter("x", helper.Master);

			filter.HorizontalId = "2";
			filter.VerticalId = "A";

			AssertEquals("2", filter.HorizontalId);
			AssertEquals("A", filter.VerticalId);

			filter.HorizontalId = "1";
			AssertEquals("1", filter.HorizontalId);
			AssertEquals("", filter.VerticalId);
		}

		public void TestHorizontalIdList()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var filter = new TouchReceivedModuleFilter("x", helper.Master);
			AssertEquals(3, filter.HorizontalIdList.Count);
			AssertEquals("1", filter.HorizontalIdList[0].Code);
			AssertEquals("2", filter.HorizontalIdList[1].Code);
			AssertEquals("3", filter.HorizontalIdList[2].Code);
		}

		public void TestVerticalIdList()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var filter = new TouchReceivedModuleFilter("x", helper.Master);
			AssertEquals(0, filter.VerticalIdList.Count);

			filter.HorizontalId = "1";
			AssertEquals(2, filter.VerticalIdList.Count);
			AssertEquals("A", filter.VerticalIdList[0].Code);
			AssertEquals(helper.Touch1A.TouchFullName, filter.VerticalIdList[0].Description);
			AssertEquals("B", filter.VerticalIdList[1].Code);
			AssertEquals(helper.Touch1B.TouchFullName, filter.VerticalIdList[1].Description);

			filter.HorizontalId = "2";
			AssertEquals(2, filter.VerticalIdList.Count);
			AssertEquals("A", filter.VerticalIdList[0].Code);
			AssertEquals(helper.Touch2A.TouchFullName, filter.VerticalIdList[0].Description);
			AssertEquals("B", filter.VerticalIdList[1].Code);
			AssertEquals(helper.Touch2B.TouchFullName, filter.VerticalIdList[1].Description);

			filter.HorizontalId = "3";
			AssertEquals(1, filter.VerticalIdList.Count);
			AssertEquals("A", filter.VerticalIdList[0].Code);
			AssertEquals(helper.Touch3A.TouchFullName, filter.VerticalIdList[0].Description);
		}

		public void TestLittleBitOfQuery()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var filter = new TouchReceivedModuleFilter("x", helper.Master);
			AssertEquals(true, filter.Query.IsEmpty);

			string sql3 = @"G8_PK = '{0}'
";

			filter.HorizontalId = "1";
			filter.HorizontalId = "2";
			filter.VerticalId = "A";
			AssertEquals(string.Format(sql3, helper.ItemMaster2.PK), filter.Query.LiteralTextSqlFormatted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			return new TouchReceivedModuleFilter("Test", campaign);
		}
	}
}
