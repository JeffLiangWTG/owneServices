using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentScanning.GUI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignItemContactEDocsControlFormForTest))]
	class CampaignItemContactEDocsControlTest : ZFormBasherTest
	{
		public void TestEdocsSwappableControlUpdateEDocs()
		{
			CreateCampaignItemsForTest();

			CampaignItem1.StorageMain.AddFileOrDocument(new byte[3] { 1, 2, 3 }, "tifFile1.tif", "MSC");
			CampaignItem1.StorageMain.AddFileOrDocument(new byte[3] { 1, 2, 3 }, "textFile1.txt", "MSC");

			CampaignItem2.StorageMain.AddFileOrDocument(new byte[3] { 1, 2, 3 }, "tifFile2.tif", "MSC");
			CampaignItem2.StorageMain.AddFileOrDocument(new byte[3] { 1, 2, 3 }, "textFile2.txt", "MSC");

			using (var form = new CampaignItemContactEDocsControlFormForTest(CampaignItem1))
			{
				form.Show();

				form.CampaignItemContactEDocsControl.UpdateEDocs(CampaignItem1);
				var documentsGrid = GetDocumentsZGrid(form.CampaignItemContactEDocsControl.eDocsUserControl);
				AssertEquals(0, form.CampaignItemContactEDocsControl.eDocsUserControl.StorageDocsListManager_CurrentChangedHints);
				documentsGrid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals(1, form.CampaignItemContactEDocsControl.eDocsUserControl.StorageDocsListManager_CurrentChangedHints);

				form.CampaignItemContactEDocsControl.UpdateEDocs(CampaignItem2);
				documentsGrid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals(2, form.CampaignItemContactEDocsControl.eDocsUserControl.StorageDocsListManager_CurrentChangedHints);

				form.CampaignItemContactEDocsControl.UpdateEDocs(CampaignItem1);
				documentsGrid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals(3, form.CampaignItemContactEDocsControl.eDocsUserControl.StorageDocsListManager_CurrentChangedHints);
			}

			DocumentsZGrid GetDocumentsZGrid(Control control)
			{
				foreach (Control item in control.Controls)
				{
					if (item is DocumentsZGrid documentsGrid)
					{
						return documentsGrid;
					}
					else
					{
						var childControl = GetDocumentsZGrid(item);
						if (childControl != null)
						{
							return childControl;
						}
					}
				}
				return null;
			}
		}

		public void TestNoBoundObject()
		{
			using (var form = new ZForm())
			using (var control = new EdocsSwappableControl())
			{
				control.Dock = DockStyle.Fill;

				form.Controls.Add(control);
				form.Show();

				Assert("When there is no bizo bound, the HasChanges should be false", !control.HasChanges);
			}
		}

		public void TestNoContactHintInitiallyShown()
		{
			CreateCampaignItemsForTest();

			using (var form = new CampaignItemContactEDocsControlFormForTest(CampaignItem1))
			{
				form.Show();

				AssertNull(form.CampaignItemContactEDocsControl.eDocsUserControl);
				Assert("NoContactSelectedLabel should be visible", form.CampaignItemContactEDocsControl.NoContactSelectedLabel.Visible);

				form.CampaignItemContactEDocsControl.UpdateEDocs(CampaignItem1);

				Assert("eDocsControl should be visible", form.CampaignItemContactEDocsControl.eDocsUserControl.Visible);
				Assert("NoContactSelectedLabel should NOT be visible", !form.CampaignItemContactEDocsControl.NoContactSelectedLabel.Visible);
			}
		}

		public void TestShowNoContactSelectedHint()
		{
			CreateCampaignItemsForTest();

			using (var form = new CampaignItemContactEDocsControlFormForTest(CampaignItem1))
			{
				form.Show();

				form.CampaignItemContactEDocsControl.UpdateEDocs(CampaignItem1);

				Assert("eDocsControl should be visible", form.CampaignItemContactEDocsControl.eDocsUserControl.Visible);
				Assert("NoContactSelectedLabel should NOT be visible", !form.CampaignItemContactEDocsControl.NoContactSelectedLabel.Visible);

				form.CampaignItemContactEDocsControl.ShowEdocUserControl(false);

				Assert("eDocsControl should NOT be visible", !form.CampaignItemContactEDocsControl.eDocsUserControl.Visible);
				Assert("NoContactSelectedLabel should be visible", form.CampaignItemContactEDocsControl.NoContactSelectedLabel.Visible);
			}
		}

		public void TestUpdateContactEDocs()
		{
			CreateCampaignItemsForTest();

			using (var form = new CampaignItemContactEDocsControlFormForTest(CampaignItem1))
			{
				form.Show();

				form.CampaignItemContactEDocsControl.UpdateEDocs(CampaignItem1);

				AssertEquals(CampaignItem1, (GlbCompanyCampaignItem)form.CampaignItemContactEDocsControl.eDocsUserControl.PlugIn.HostBusinessObject);

				form.CampaignItemContactEDocsControl.UpdateEDocs(CampaignItem2);

				AssertEquals(CampaignItem2, (GlbCompanyCampaignItem)form.CampaignItemContactEDocsControl.eDocsUserControl.PlugIn.HostBusinessObject);
			}
		}

		#region Implementation

		void CreateCampaignItemsForTest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAAX";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			Factory.Save();

			CampaignItem1 = campaignItem1;
			CampaignItem2 = campaignItem2;
		}

		GlbCompanyCampaignItem CampaignItem1;
		GlbCompanyCampaignItem CampaignItem2;

		protected override Form GetFormToBashCore()
		{
			var campaignItem = Factory.New<GlbCompanyCampaignItem>();
			return new CampaignItemContactEDocsControlFormForTest(campaignItem);
		}

		public class CampaignItemContactEDocsControlFormForTest : ZForm
		{
			public CampaignItemContactEDocsControlFormForTest(GlbCompanyCampaignItem dataSource)
				: base(dataSource)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Size = new Size(1284, 620);

				Controls.Add(CampaignItemContactEDocsControl);

				CaptionRenderingEnabled = true;
			}

			internal EdocsSwappableControl CampaignItemContactEDocsControl = new EdocsSwappableControl();
		}

		#endregion
	}
}
