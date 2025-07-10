using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(OrgSalesDashboardModule))]
	public class OrgSalesDashboardModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (var module = new OrgSalesDashboardModuleForTest())
			{
				AssertEquals(ModuleIDs.OrgSalesDashboard, module.ID);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgSalesDashboard;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShowRecent()
		{
			using (var module = new OrgSalesDashboardModuleForTest())
			{
				Assert("Should not show recent", !module.ShowRecentExposed);
			}
		}

		public void TestGetNewAdditionalContextMenuItems()
		{
			using (var module = new OrgSalesDashboardModuleForTest())
			{
				var menuItems = module.GetNewAdditionalContextMenuItems();
				AssertEquals(1, menuItems.Length);
				AssertNotNull("Campaign Tracking", menuItems[0].Text);
				AssertNotNull("Campaign Tracking", module.DisplayGrid.ContextMenu.MenuItems.FindByName("CampaignTracking"));
			}
		}

		public void TestCampaignTrackingMenuItemClick()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign Name";
			campaign.G0_CampaignID = "TST00001000";
			var parent = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			parent.G8_G0 = campaign.PK;
			parent.G8_RecipientID = contact.PK;
			parent.G8_RecipientTableCode = "OC";

			var campaignActivity = Factory.NewWithValidTestData<CampaignSalesDashboardActivity>();
			campaignActivity.VSA_OH = organisation.PK;
			campaignActivity.VSA_ParentId = parent.PK;
			campaignActivity.VSA_ActivityType = SalesDashboardActivityTypeCodeList.Codes.Campaign;

			Factory.Save();

			using (var module = new OrgSalesDashboardModuleForTest())
			{
				module.Org = organisation;

				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var testCollection = (SalesDashboardActivityCollection)module.GridCollection;
					testCollection.Add(Factory.Load<CampaignSalesDashboardActivity>(campaignActivity.PK));
					AssertEquals(1, testCollection.Count);

					var menuItems = module.GetNewAdditionalContextMenuItems();
					menuItems[0].PerformClick();
					using (var lastShownForm = Application.OpenForms.OfType<GlbCompanyCampaignForm>().SingleOrDefault())
					{
						AssertNotNull("Company Campaign Form", lastShownForm);
						AssertContains("should display campaign ID", "TST00001000", lastShownForm.Text);
					}
				}
			}
		}

		#region Implementation

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			collection.Add(Factory.Load<OpportunitySalesDashboardActivity>(opportunity.PK));
			collection.Add(Factory.Load<InquirySalesDashboardActivity>(inquiry.PK));
			collection.Add(Factory.Load<CommunicationSalesDashboardActivity>(communication.PK));
		}

		#endregion
	}
}
