using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class ContactsWithNonDeliveryReportsCollection : BusinessObjectCollection<CampaignContact>
	{
		public ContactsWithNonDeliveryReportsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void AddContacts(IEnumerable<GlbCompanyCampaignItem> campaignItems)
		{
			if (campaignItems != null && campaignItems.Any())
			{
				foreach (GlbCompanyCampaignItem item in campaignItems)
				{
					var contact = Factory.Load<CampaignContact>(item.G8_RecipientID);
					if (contact != null)
					{
						Add(contact);
					}
				}
			}
		}

		public void AddContacts(IEnumerable<CampaignContact> contacts)
		{
			if (contacts != null)
			{
				foreach (CampaignContact campaignContact in contacts)
				{
					campaignContact.SetReadOnlyIncludingChildren(false);
					Add(campaignContact);
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
