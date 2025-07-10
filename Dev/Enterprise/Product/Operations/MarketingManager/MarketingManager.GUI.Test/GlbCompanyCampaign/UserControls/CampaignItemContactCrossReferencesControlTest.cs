using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignItemContactCrossReferencesControlFormForTest))]
	class CampaignItemContactCrossReferencesControlTest : ZFormBasherTest
	{
		public void TestGridRefreshedOnCurrentDataItemChanged()
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

			using (var form = new CampaignItemContactCrossReferencesControlFormForTest(campaignItem1))
			{
				form.Show();
				AssertContainsExactElementsInAnyOrder("Should show cross references for campaignItem1", new[] { contact2 }, form.CampaignItemContactCrossReferencesControl.Grid.ListManager.List.Cast<OrgContactCampaignReferences>().Select(reference => reference.Contact));

				form.SetDataBinding(campaignItem2, ".");
				AssertContainsExactElementsInAnyOrder("Should show cross references for campaignItem2", new[] { contact1 }, form.CampaignItemContactCrossReferencesControl.Grid.ListManager.List.Cast<OrgContactCampaignReferences>().Select(reference => reference.Contact));
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var campaignItem = Factory.New<GlbCompanyCampaignItem>();
			return new CampaignItemContactCrossReferencesControlFormForTest(campaignItem);
		}

		public class CampaignItemContactCrossReferencesControlFormForTest : ZForm
		{
			public CampaignItemContactCrossReferencesControlFormForTest(GlbCompanyCampaignItem dataSource)
				: base(dataSource)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Size = new Size(1284, 620);

				Controls.Add(CampaignItemContactCrossReferencesControl);

				CaptionRenderingEnabled = true;
			}

			internal CampaignItemContactCrossReferencesControl CampaignItemContactCrossReferencesControl = new CampaignItemContactCrossReferencesControl();
		}

		#endregion
	}
}
