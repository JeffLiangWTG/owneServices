using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business.Testing
{
	public sealed class VoteExamSurveyPreviewBizOForTest : VoteExamSurveyPreviewBizO
	{
		public VoteExamSurveyPreviewBizOForTest(GlbCompanyCampaign campaign) : base(campaign)
		{
		}

		protected override BusinessObjectFactory CreateNonSavableLocalFactory()
		{
			return new BusinessObjectFactoryLoadCount();
		}

		public BusinessObjectFactory NonSavableLocalFactory_Exposed => NonSavableLocalFactory;
	}
}
