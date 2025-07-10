using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignItemContactCollectionForTest : GlbCompanyCampaignItemContactCollection
	{
		public GlbCompanyCampaignItemContactCollectionForTest(BusinessObjectFactory factory, OrgContact contact) : base(factory, contact)
		{ }

		public ZQuery ExposedCreateRelationshipFilter()
		{
			return CreateRelationshipFilter();
		}
	}
}
