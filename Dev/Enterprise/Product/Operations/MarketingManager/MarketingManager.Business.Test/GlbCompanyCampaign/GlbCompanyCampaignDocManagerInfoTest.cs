using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignDocManagerInfo))]
	public class GlbCompanyCampaignDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<GlbCompanyCampaign>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<GlbCompanyCampaign>();
		}

		public void TestGetRelatedObjects()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "bb@gmail.com";
			contact2.OC_ContactName = "bb";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;

			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			Factory.Save();

			var list = campaign.DocManagerInfo.RelatedObjects;
			AssertEquals("eDocs should contain items from CampaignsItemsSent.", 2, list.Length);
			Assert(list.Contains(campaignItem1));
			Assert(list.Contains(campaignItem2));
		}
	}
}
