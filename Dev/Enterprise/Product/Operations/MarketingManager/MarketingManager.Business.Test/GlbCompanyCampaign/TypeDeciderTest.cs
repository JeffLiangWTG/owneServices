using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForNewAndBinding()
		{
			AssertEquals(typeof(GlbCompanyCampaign), GlbCompanyCampaign.TypeDecider.GetTypeForNew());
			AssertEquals(typeof(GlbCompanyCampaign), GlbCompanyCampaign.TypeDecider.GetTypeForBinding());
		}

		public void TestLoad()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaign learningCentreCampaign = (GlbCompanyCampaign)Factory.New<ILearningCentreCampaign>();
			learningCentreCampaign.FillWithValidTestData();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals(typeof(GlbCompanyCampaign), newFactory.Load<GlbCompanyCampaign>(campaign.PK).GetType());
			AssertEquals(ObjectFactory.GetType<ILearningCentreCampaign>(), newFactory.Load<GlbCompanyCampaign>(learningCentreCampaign.PK).GetType());
		}
	}
}
