using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCampaignContactCollection : BusinessObjectCollection<CampaignContact>
	{
		public GlbCampaignContactCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbCampaignContactCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public GlbCampaignContactCollection(GlbCompanyCampaign master)
			: base(master.Factory)
		{
			this.master = master;
		}

		readonly GlbCompanyCampaign master;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		/// <summary>
		/// For all Contacts with email addresses, removes any duplicate contacts based on email address
		/// i.e. if 2 contacts exists with the same email address, the 2nd contact is removed from this collection and the first remains
		/// </summary>
		public void RemoveDuplicatesBasedOnEmailAddress(ZQuery defaultQuery)
		{
			ZQuery query = new ZQuery();
			if (!defaultQuery.IsEmpty && defaultQuery != ZQuery.NoResultQuery)
			{
				query.AddToFilter(defaultQuery);
			}
			query = master.ExcludeDuplicateQuery(query);
			Load(query);
		}
	}
}
