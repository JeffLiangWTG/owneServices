using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	class CampaignItemContactCrossReferences : NonPersistentBusinessObjectCollection<OrgContactCampaignReferences>
	{
		public CampaignItemContactCrossReferences()
			: base()
		{
		}

		public void Load(GlbCompanyCampaignItem campaignItem)
		{
			RemoveAndDeleteAll();

			if (campaignItem == null)
			{
				return;
			}

			var campaign = campaignItem.CompanyCampaign;
			if (campaign == null)
			{
				return;
			}

			var client = campaignItem.ClientOrg;
			if (client == null)
			{
				return;
			}

			ILookup<ZGuid, GlbCompanyCampaignItem> campaignItemsSentOrgContactByPk =
				campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Where(i => i != campaignItem)
					.Select(x => new { Item = x, OrgContactPk = GetOrgContactPk(x) })
					.Where(x => !x.OrgContactPk.IsEmpty)
					.ToLookup(x => x.OrgContactPk, x => x.Item);

			var campaignChildrenGroupedByOrgContactPk =
				campaign.RelatedChildActivityPivotCollection.Activities
					.Where(activity => activity.Contact != null)
					.ToLookup(activity => activity.Contact.PK);

			var contactPk = GetOrgContactPk(campaignItem);
			foreach (var crossReferenceContact in client.Contacts.Cast<OrgContact>().Where(x => x.PK != contactPk))
			{
				var crossReferenceCampaignItems = campaignItemsSentOrgContactByPk[crossReferenceContact.PK];
				var crossReferenceCampaignChildren = campaignChildrenGroupedByOrgContactPk[crossReferenceContact.PK];

				if (crossReferenceCampaignItems.Any() || crossReferenceCampaignChildren.Any())
				{
					var crossReferenceCampaignItemsWithNullIfEmpty = crossReferenceCampaignItems.Any() ? crossReferenceCampaignItems : new GlbCompanyCampaignItem[] { null };
					foreach (var crossReferenceCampaignItem in crossReferenceCampaignItemsWithNullIfEmpty)
					{
						var crossReference = new OrgContactCampaignReferences(crossReferenceContact);
						crossReference.CampaignItem = crossReferenceCampaignItem;
						crossReference.CampaignChildren = new HashSet<IRelatableActivity>(crossReferenceCampaignChildren);
						Add(crossReference);
					}
				}
			}
		}

		static ZGuid GetOrgContactPk(GlbCompanyCampaignItem item)
		{
			if (item.G8_RecipientTableCode == OrgContactSchema.Constants.Prefix)
			{
				return item.G8_RecipientID;
			}
			else if (item.G8_RecipientTableCode == OrgColdCallRegisterSchema.Constants.Prefix)
			{
				var inquiry = item.Recipient as SalesEnquiry;
				if (inquiry != null)
				{
					return inquiry.O1_OC_LinkedContact;
				}
			}

			return ZGuid.Empty;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}
	}
}
