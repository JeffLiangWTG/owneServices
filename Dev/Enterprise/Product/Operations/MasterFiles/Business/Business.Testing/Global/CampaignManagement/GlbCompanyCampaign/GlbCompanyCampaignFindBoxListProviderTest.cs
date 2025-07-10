using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCompanyCampaignFindBoxListProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetBizObjFromCode()
		{
			CreateCampaigns();
			GlbCompanyCampaignFindBoxListProvider listProvider = new GlbCompanyCampaignFindBoxListProvider(Campaigns);

			BusinessObject obj = listProvider.GetBusinessObjectFromCode("ABC00001000");
			AssertEquals(((BusinessObject)Campaign1).PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCode("DEF00001001");
			AssertEquals(((BusinessObject)Campaign2).PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCode("GHI00001002");
			AssertEquals(((BusinessObject)Campaign3).PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCode("ABCD00001000");
			AssertNull(obj);

			obj = listProvider.GetBusinessObjectFromCode("00001000");
			AssertNull(obj);

			obj = listProvider.GetBusinessObjectFromCode("1001");
			AssertNull(obj);
		}

		[ExpectNoExceptions]
		public void TestGetBizObjFromCodeWithoutFilter()
		{
			CreateCampaigns();
			GlbCompanyCampaignFindBoxListProvider listProvider = new GlbCompanyCampaignFindBoxListProvider(Campaigns);

			BusinessObject obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("ABC00001000");
			AssertEquals(((BusinessObject)Campaign1).PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("DEF00001001");
			AssertEquals(((BusinessObject)Campaign2).PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("GHI00001002");
			AssertEquals(((BusinessObject)Campaign3).PK, obj.PK);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("ABCD00001000");
			AssertNull(obj);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("00001000");
			AssertNull(obj);

			obj = listProvider.GetBusinessObjectFromCodeWithoutFilter("1001");
			AssertNull(obj);
		}

		public void TestNearestMatch()
		{
			CreateCampaigns();
			GlbCompanyCampaignFindBoxListProvider listProvider = new GlbCompanyCampaignFindBoxListProvider(Campaigns);

			var result = listProvider.NearestMatch("ABC00001000", true, -1);
			AssertEquals("ABC00001000", result.Item1);
			AssertEquals("Campaign Exists", true, result.Item2);

			result = listProvider.NearestMatch("DEF00001001", true, -1);
			AssertEquals("DEF00001001", result.Item1);
			AssertEquals("Campaign Exists", true, result.Item2);

			result = listProvider.NearestMatch("GHI00001002", true, -1);
			AssertEquals("GHI00001002", result.Item1);
			AssertEquals("Campaign Exists", true, result.Item2);

			result = listProvider.NearestMatch("GHI0000100", true, -1);
			AssertEquals("GHI0000100", result.Item1);
			AssertEquals("Campaign Does Not Exist", false, result.Item2);
		}

		#region Implementation

		GlbCompanyCampaignCollection Campaigns;
		IGlbCompanyCampaign Campaign1;
		IGlbCompanyCampaign Campaign2;
		IGlbCompanyCampaign Campaign3;

		void CreateCampaigns()
		{
			Campaigns = new GlbCompanyCampaignCollection(Factory);

			Campaign1 = Campaigns.AddNew();
			Campaign1.G0_EstimatedStartedDate = new ZDateTime(2005, 11, 25);
			Campaign1.G0_Category = "ABCXX";
			Campaign1.G0_Type = "DEFYY";
			Campaign1.G0_CampaignName = "Campaign 1";
			Campaign1.G0_CampaignID = "ABC00001000";

			Campaign2 = Campaigns.AddNew();
			Campaign2.G0_EstimatedStartedDate = new ZDateTime(2006, 8, 20);
			Campaign2.G0_Category = "XYZZZ";
			Campaign2.G0_Type = "LMNAA";
			Campaign2.G0_CampaignName = "Campaign 2";
			Campaign2.G0_CampaignID = "DEF00001001";

			Campaign3 = Campaigns.AddNew();
			Campaign3.G0_EstimatedStartedDate = new ZDateTime(2011, 6, 2);
			Campaign3.G0_Category = "DDD";
			Campaign3.G0_Type = "VOWE";
			Campaign3.G0_CampaignName = "Campaign 3";
			Campaign3.G0_CampaignID = "GHI00001002";
		}

		#endregion
	}
}
