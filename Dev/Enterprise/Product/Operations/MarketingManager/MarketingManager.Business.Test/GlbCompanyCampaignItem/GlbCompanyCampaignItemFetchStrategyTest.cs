using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignItemFetchStrategyTest : TestCaseWithFactory
	{
		public void TestAddFetchHintWithHRJobApplicant()
		{
			var bizO = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			bizO.G8_RecipientID = ZGuid.NewZGuid();
			bizO.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;

			var strategy = new GlbCompanyCampaignItemFetchStrategy(bizO);
			var column = new TableColumn("", GlbCompanyCampaignItem.Schema.ContactName);

			AssertNoExceptionThrown("No exception should be thrown here", () => strategy.FetchForView(new TableColumn[] { column }));
		}

		public void TestCampaignContactTouches()
		{
			const int contactsCount = 100;
			var campaignPk = CreateCampaignAndItems(contactsCount);

			var cleanFactory = new BusinessObjectFactory();
			var campaign = cleanFactory.Load<GlbCompanyCampaign>(campaignPk);
			var items = campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>();

			var expectedHitCounts = new Dictionary<string, int>
				{
					{ GlbCompanyCampaignSchema.Constants.TableName, 1 },
					{ GlbCompanyCampaignItemSchema.Constants.TableName, 1 },
				};
			AssertDbHits(expectedHitCounts, cleanFactory);

			var contacts = items.Select(item => item.RecipientFromView).ToArray();

			expectedHitCounts[ViewCampaignContactSchema.Constants.TableName] = 2;
			AssertDbHits(expectedHitCounts, cleanFactory);
			AssertEquals("Contacts count", contactsCount, contacts.Length);
		}

		ZGuid CreateCampaignAndItems(int contactsCount)
		{
			var staffCor = Factory.NewWithValidTestData<GlbStaff>();
			staffCor.GS_EmailAddress = $"{nameof(staffCor)}@test.com";
			staffCor.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var companyCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			companyCampaign.G0_CampaignName = "touch1";
			companyCampaign.G0_Type = "PREAP";
			companyCampaign.G0_Category = "PRINT";
			companyCampaign.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			companyCampaign.G0_EstimatedStartedDate = ZDateTime.Now;
			companyCampaign.G0_GS_NKCampaignCoordinator = staffCor.GS_Code;
			companyCampaign.G0_GS_NKCampaignManager = staffCor.GS_Code;

			var contacts = Enumerable.Range(1, contactsCount).Select(i =>
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_Email = $"{nameof(contact)}{i}@test.com";
				return contact;
			}).ToArray();

			var items = contacts.Select(contact =>
			{
				var item = Factory.New<GlbCompanyCampaignItem>();
				item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
				item.G8_RecipientID = contact.PK;
				item.G8_G0 = companyCampaign.PK;
				item.G8_GS_NKSender = companyCampaign.CampaignCoordinator.GS_Code;
				item.G8_SenderEmailAddress = companyCampaign.CampaignCoordinator.GS_EmailAddress;
				return item;
			}).ToArray();

			Factory.Save();

			AssertEquals("Conatacts count", contactsCount, contacts.Length);
			AssertEquals("Items count", contactsCount, items.Length);

			return companyCampaign.PK;
		}
	}
}
