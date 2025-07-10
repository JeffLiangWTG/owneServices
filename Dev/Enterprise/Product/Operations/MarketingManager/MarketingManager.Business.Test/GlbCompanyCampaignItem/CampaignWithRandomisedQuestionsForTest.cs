using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class CampaignWithRandomisedQuestionsForTest : GlbCompanyCampaign
	{
		public CampaignWithRandomisedQuestionsForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool RandomiseQuestionAndMultipleChoiceOrder
		{
			get { return true; }
		}
	}
}
