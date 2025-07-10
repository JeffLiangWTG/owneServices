using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SentCampaignItemsRelatedChildActivityPivotCollection))]
	sealed class SentCampaignItemsRelatedChildActivityPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<SentCampaignItemsRelatedChildActivityPivotCollection>
	{
		public void TestGeneralUsage()
		{
			var campaign1 = Factory.New<GlbCompanyCampaign>();
			var campaign1ItemA = campaign1.CampaignsItemsSent.AddNew();
			var campaign1ItemAChildPivot1 = campaign1ItemA.RelatedChildActivityPivotCollection.AddNew();

			var campaign2 = Factory.New<GlbCompanyCampaign>();
			var campaign2ItemA = campaign2.CampaignsItemsSent.AddNew();
			var campaign2ItemAChildPivot = campaign2ItemA.RelatedChildActivityPivotCollection.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { campaign1ItemAChildPivot1 }, campaign1.PostCampaignItemPivotCollection);
			AssertContainsExactElementsInAnyOrder(new[] { campaign2ItemAChildPivot }, campaign2.PostCampaignItemPivotCollection);

			var campaign1ItemAChildPivot2 = campaign1ItemA.RelatedChildActivityPivotCollection.AddNew();
			var campaign1ItemB = campaign1.CampaignsItemsSent.AddNew();
			var campaign1ItemBChildPivot1 = campaign1ItemB.RelatedChildActivityPivotCollection.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { campaign1ItemAChildPivot1, campaign1ItemAChildPivot2, campaign1ItemBChildPivot1 }, campaign1.PostCampaignItemPivotCollection);
			AssertContainsExactElementsInAnyOrder(new[] { campaign2ItemAChildPivot }, campaign2.PostCampaignItemPivotCollection);
		}

		public void TestGeneralUsage_ShouldUseTableValuedParameters()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Unit Test Campaign";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "test 1";
			contact1.OC_Email = "test1@live.com";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "test 2";
			contact2.OC_Email = "test2@live.com";

			var rating = Factory.NewWithValidTestData<Quote>();
			rating.TH_QuoteNumber = "003";
			rating.TH_OH = org.PK;
			rating.TH_OneTimeQuote = false;
			rating.TH_GC = Env.CurrentCompanyPK;

			var campaignItemA = campaign.CampaignsItemsSent.AddNew();
			campaignItemA.G8_RecipientID = contact1.PK;
			campaignItemA.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var campaignItemAChildPivot1 = campaignItemA.RelatedChildActivityPivotCollection.AddNew();
			campaignItemAChildPivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			campaignItemAChildPivot1.RAP_ChildActivityID = rating.PK;

			var campaignItemB = campaign.CampaignsItemsSent.AddNew();
			campaignItemB.G8_RecipientID = contact2.PK;
			campaignItemB.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var campaignItemBChildPivot1 = campaignItemB.RelatedChildActivityPivotCollection.AddNew();
			campaignItemBChildPivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			campaignItemBChildPivot1.RAP_ChildActivityID = rating.PK;

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");
					var reloadedCampaign = Factory.LoadTop1<GlbCompanyCampaign>(new ZQuery(GlbCompanyCampaignSchema.PK, campaign.PK));

					AssertEquals("Should be two items in the collection", 2, reloadedCampaign.PostCampaignItemPivotCollection.Count);
					var executedCommand = Db.Connection.ExecutedCommands.LastOrDefault(c => c.StartsWith("SELECT \r\nRAP_PK") && c.Contains("FROM dbo.ViewRelatedActivityPivot"));

					AssertNotNull(executedCommand);
					AssertContains("Should use TVPs", "(RAP_ParentActivityID in (SELECT Value FROM", executedCommand);
					Assert("Should not use explicit values", !Regex.IsMatch(executedCommand, @"\(RAP_ParentActivityID in \((@(.*?),)*?@(.*?)\)"));
				}
			}
		}

		public void TestRefreshAdditionalFilter()
		{
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign1Item = campaign1.CampaignsItemsSent.AddNew();
			var campaign1ItemChildPivot1 = campaign1Item.RelatedChildActivityPivotCollection.AddNew();

			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign2Item = campaign2.CampaignsItemsSent.AddNew();
			var campaign2ItemChildPivot = campaign2Item.RelatedChildActivityPivotCollection.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { campaign1ItemChildPivot1 }, campaign1.PostCampaignItemPivotCollection);

			List<ViewRelatedActivityPivot> viewRelatedActivityPivotList = null;
			((IBindingList)campaign1.PostCampaignItemPivotCollection).ListChanged += (sender, e) => viewRelatedActivityPivotList = campaign1.PostCampaignItemPivotCollection.ToList();

			var campaign1ItemChildPivot2 = campaign1Item.RelatedChildActivityPivotCollection.AddNew();
			campaign1.CampaignsItemsSent.AddNew();

			AssertContainsExactElementsInAnyOrder("campaign2ItemChildPivot must not appear in viewRelatedActivityPivotList.", new[] { campaign1ItemChildPivot1, campaign1ItemChildPivot2 }, viewRelatedActivityPivotList);
		}

		#region Overrides

		protected override SentCampaignItemsRelatedChildActivityPivotCollection GetCollectionToTest()
		{
			return new SentCampaignItemsRelatedChildActivityPivotCollection(Campaign);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = (ViewRelatedActivityPivot)base.GetNewElementToAddToTheCollection();
			result.ParentActivity = campaignItem;
			return result;
		}

		GlbCompanyCampaign Campaign
		{
			get
			{
				if (campaign == null)
				{
					campaign = Factory.New<GlbCompanyCampaign>();
					campaignItem = campaign.CampaignsItemsSent.AddNew();
				}

				return campaign;
			}
		}
		GlbCompanyCampaign campaign;
		GlbCompanyCampaignItem campaignItem;

		#endregion
	}
}
