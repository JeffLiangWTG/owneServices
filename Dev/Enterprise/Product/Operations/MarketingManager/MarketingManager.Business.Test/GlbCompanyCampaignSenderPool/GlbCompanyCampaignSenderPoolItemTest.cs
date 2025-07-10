using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSenderPoolItem))]
	sealed class GlbCompanyCampaignSenderPoolItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGCP_G0_Campaign_ReadOnly()
		{
			var poolItem = Factory.New<GlbCompanyCampaignSenderPoolItem>();
			Assert("Should always be readonly", poolItem.GCP_G0_CampaignInfo.ReadOnly);
		}

		public void TestSendRatioPercentage_ReadOnly()
		{
			var poolItem = Factory.New<GlbCompanyCampaignSenderPoolItem>();
			Assert("Should always be readonly", poolItem.SendRatioPercentageInfo.ReadOnly);
		}

		public void TestDefaultValues()
		{
			var poolItem = Factory.New<GlbCompanyCampaignSenderPoolItem>();
			AssertEquals("Default SendRatio", (ZShort)1, poolItem.GCP_SendRatio);
		}
	}
}
