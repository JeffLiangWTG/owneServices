using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class SalesDashboardSalesRelationControlTest : TestCaseWithFactory
	{
		public void TestShowCommunication()
		{
			var opportunity = Factory.New<OrgOpportunity>();

			using (var form = new SalesDashboardSalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				AssertEquals("Should be invisible", false, form.SalesRelationControl.ShowCommunicationCheckBox_Exposed.Visible);
			}
		}

		public void TestEmptyActivityNotePropertyName()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity = organization.SalesOpportunities.AddNew();
			opportunity.P8_OpportunityNotes = ZBlob.FromUTF8("test note");

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			Assert("Precondition", !((ISalesRelationActivity)opportunity).ActivityNotePropertyName.IsEmpty);
			Assert("Precondition", ((ISalesRelationActivity)campaign).ActivityNotePropertyName.IsEmpty);

			using (var form = new SalesDashboardSalesRelationControlFormForTest(null))
			{
				form.Show();
				var activityNoteRichTextBox = form.SalesRelationControl.FindSingle<ZRichTextBox>(c => c.Name == "activityNoteRichTextBox");
#if !WINZOR
				form.SetDataBinding(opportunity.SalesRelationModel, "");
				AssertEquals("test note", ORtfTextUtil.RtfToText(activityNoteRichTextBox.Rtf));

				form.SetDataBinding(campaign.SalesRelationModel, "");
				AssertEquals("", ORtfTextUtil.RtfToText(activityNoteRichTextBox.Rtf));

				form.SetDataBinding(opportunity.SalesRelationModel, "");
				AssertEquals("test note", ORtfTextUtil.RtfToText(activityNoteRichTextBox.Rtf));
#else
				activityNoteRichTextBox.Font = new Font("Tahoma", 20, GraphicsUnit.Pixel);
				form.SetDataBinding(opportunity.SalesRelationModel, "");
				AssertEquals("<p><span style=\"font-family: Tahoma, sans-serif; font-size: 20px;\">test note</span></p>", activityNoteRichTextBox.Html);

				form.SetDataBinding(campaign.SalesRelationModel, "");
				AssertEquals("", activityNoteRichTextBox.Html);

				form.SetDataBinding(opportunity.SalesRelationModel, "");
				AssertEquals("<p><span style=\"font-family: Tahoma, sans-serif; font-size: 20px;\">test note</span></p>", activityNoteRichTextBox.Html);
				#endif
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);
		}

		class SalesDashboardSalesRelationControlFormForTest : ZForm
		{
			public SalesDashboardSalesRelationControlFormForTest(ISalesRelationModel model)
				: base(model)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				SalesRelationControl.Dock = DockStyle.Fill;
				Controls.Add(SalesRelationControl);
				BindingSource.SetBindingMember(SalesRelationControl, ".");
				CaptionRenderingEnabled = true;
			}

			public readonly SalesRelationControlForTest SalesRelationControl = new SalesRelationControlForTest();
		}

		public class SalesRelationControlForTest : SalesDashboardSalesRelationControl
		{
			public SalesRelationControlForTest()
				: base()
			{
			}

			public ZCheckBox ShowCommunicationCheckBox_Exposed
			{
				get { return ShowCommunicationsCheckBox; }
			}
		}
	}
}
