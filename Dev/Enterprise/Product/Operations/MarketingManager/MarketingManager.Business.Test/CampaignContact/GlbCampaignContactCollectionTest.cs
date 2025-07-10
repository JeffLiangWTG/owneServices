using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	public sealed class GlbCampaignContactCollectionTest : GlbCampaignContactCollection
	{
		public GlbCampaignContactCollectionTest(GlbCompanyCampaign master)
	: base(master)
		{
		}
		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(ViewCampaignContactSchema.VCC_ContactName, SQLComparisonOperator.StartsWith, "Test User Only -");
		}
	}
}
