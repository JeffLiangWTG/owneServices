using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignDripMarketingCollection : ActiveBusinessObjectCollection<GlbCompanyCampaignDripMarketing>
	{
		public GlbCompanyCampaignDripMarketingCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery())
		{
		}

		public GlbCompanyCampaignDripMarketingCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbCompanyCampaignDripMarketingCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}

	public class EmptyDripMarketingCollection : GlbCompanyCampaignDripMarketingCollection
	{
		public EmptyDripMarketingCollection(BusinessObjectFactory factory) : base(factory, ZQuery.NoResultQuery)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
