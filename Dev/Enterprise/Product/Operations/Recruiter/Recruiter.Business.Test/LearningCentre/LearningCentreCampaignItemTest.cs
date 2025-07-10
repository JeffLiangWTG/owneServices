using Enterprise.MarketingManager.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreCampaignItem))]
	sealed class LearningCentreCampaignItemTest : GlbCompanyCampaignItemTest
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			AssertNoExceptionThrown(Factory.Save);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			AssertNoExceptionThrown(Factory.Save);
		}
	}
}
