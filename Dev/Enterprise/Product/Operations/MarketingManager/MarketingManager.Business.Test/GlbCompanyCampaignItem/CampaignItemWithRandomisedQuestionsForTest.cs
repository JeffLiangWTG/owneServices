using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class CampaignItemWithRandomisedQuestionsForTest : GlbCompanyCampaignItem
	{
		public CampaignItemWithRandomisedQuestionsForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override GlbCompanyCampaign CompanyCampaign
		{
			get { return Factory.Load<CampaignWithRandomisedQuestionsForTest>(G8_G0); }
		}
	}
}
