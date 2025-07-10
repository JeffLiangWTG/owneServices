using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreCampaign))]
	sealed class LearningCentreCampaignTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestFetchForLoad()
		{
			AssertNoExceptionThrown(Factory.Save);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			AssertNoExceptionThrown(Factory.Save);
		}
	}
}
