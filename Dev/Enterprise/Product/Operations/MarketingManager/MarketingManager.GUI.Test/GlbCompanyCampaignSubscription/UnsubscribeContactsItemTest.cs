using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(UnsubscribeContactsItem))]
	class UnsubscribeContactsItemTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new UnsubscribeContactsItem(campaignItem);

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			Factory.Save();
		}

		GlbCompanyCampaignItem campaignItem;
	}
}
