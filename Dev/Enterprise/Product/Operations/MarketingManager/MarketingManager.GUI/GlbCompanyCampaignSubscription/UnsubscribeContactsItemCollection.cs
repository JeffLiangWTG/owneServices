using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	class UnsubscribeContactsItemCollection : NonPersistentBusinessObjectCollection<UnsubscribeContactsItem>
	{
		public UnsubscribeContactsItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
		public override bool ReadOnly => true;

		public void AddItems(IEnumerable<GlbCompanyCampaignItem> glbCompanyCampaignItems)
		{
			AddRange((glbCompanyCampaignItems ?? Array.Empty<GlbCompanyCampaignItem>())
				.Where(item => item?.EmailAddress.IsEmpty == false)
				.Select(item => new UnsubscribeContactsItem(item)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		public IEnumerator<UnsubscribeContactsItem> GetEnumerator()
		{
			return Elements.Cast<UnsubscribeContactsItem>().GetEnumerator();
		}
	}
}
