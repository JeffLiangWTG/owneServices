using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class ResultsByRecipientFilterControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridColumns()
		{
			using (var form = new ZForm())
			using (var filterControl = new ResultsByRecipientFilterControlForTest(testCampaign, new LearningCentreCampaignItemFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				Assert(filterControl.Grid_Exposed.Columns.Contains("ContactName"));
				Assert(filterControl.Grid_Exposed.Columns.Contains("EmailAddress"));
			}
		}

		public void TestFilterWithCurrentCampaign()
		{
			var otherCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var otherContact = Factory.NewWithValidTestData<OrgContact>();

			var otherItem = otherCampaign.CampaignsItemsSentForDisplayOnly.AddNew();
			otherItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			otherItem.G8_RecipientID = otherContact.PK;
			otherItem.G8_G0 = otherCampaign.PK;

			Factory.Save();

			AssertFilterControlItemCount("Should only include one campaign item for other campaign", 1, otherCampaign);
		}

		public void TestPerformSearchRetrievesCorrectRecordsAfterError()
		{
			using (SetMaxNumberOfRecordsToShowInDisplayGrids(1))
			{
				AssertFilterControlItemCount("Number of campaign items exceeds max for display so should show none", 0, testCampaign);
			}

			AssertFilterControlItemCount("Number of campaign items no longer exceeds max for display so should show correct number", 2, testCampaign);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();

			var item1 = testCampaign.CampaignsItemsSentForDisplayOnly.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			item1.G8_G0 = testCampaign.PK;

			var item2 = testCampaign.CampaignsItemsSentForDisplayOnly.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			item2.G8_G0 = testCampaign.PK;

			Factory.Save();
		}

		void AssertFilterControlItemCount(string assertMessage, int expectedCount, GlbCompanyCampaign campaign)
		{
			using (var form = new ZForm())
			using (var filterControl = new ResultsByRecipientFilterControlForTest(campaign, new LearningCentreCampaignItemFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				filterControl.FirePerformSearch();
				AssertEquals(assertMessage, expectedCount, filterControl.GridCollection.Count);
			}
		}

		IDisposable SetMaxNumberOfRecordsToShowInDisplayGrids(int value)
		{
			return SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		GlbCompanyCampaign testCampaign;

		class ResultsByRecipientFilterControlForTest : ResultsByRecipientFilterControl
		{
			public ResultsByRecipientFilterControlForTest(GlbCompanyCampaign campaign, LearningCentreCampaignItemFilterBusinessObject filterBusinessObject)
				: base(campaign, filterBusinessObject)
			{
			}

			public ZFilterGrid Grid_Exposed => Grid;

			protected override void Dispose(bool disposing)
			{
				SearchManager.Dispose();
				base.Dispose(disposing);
			}
		}
	}
}
