using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ZClientIntelligenceForm))]
	sealed class ClientIntelligenceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OrgHeader organization = Factory.New<OrgHeader>();
			return new ZClientIntelligenceForm(organization);
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		public void TestShouldRunDeduplicationShouldBeTrue()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			using (ClientIntelligenceFormForTest form = new ClientIntelligenceFormForTest(organization))
			{
				AssertEquals("The value of ShouldRunDeduplicationis property should be true", true, ((IDeduplicatable)organization).ShouldRunDeduplication);
			}
		}

		[SnailTest()]
		public void TestCorrectTabPagesAreShown()
		{
			OrgHeader organization = Factory.New<OrgHeader>();
			Env.Registry.SetOrgShowARTab(false);
			Env.Registry.SetOrgShowConsigneeConsignorTab(false);
			using (ClientIntelligenceFormForTest form = new ClientIntelligenceFormForTest(organization))
			{
				AssertEquals("Form should show 7 tabs with both registry settings false", 7, form.OrganisationsTabControl.TabCount);
			}

			Env.Registry.SetOrgShowARTab(true);
			Env.Registry.SetOrgShowConsigneeConsignorTab(true);
			using (ClientIntelligenceFormForTest form = new ClientIntelligenceFormForTest(organization))
			{
				AssertEquals("Form should show 10 tabs with both registry settings true", 10, form.OrganisationsTabControl.TabCount);
				AssertEquals("The Receivables tab should be the 5th tab", "ReceivablesTabPage", form.OrganisationsTabControl.TabPages[4].Name);
				AssertEquals("The Consignor tab should be the 6th tab", "ConsignorTabPage", form.OrganisationsTabControl.TabPages[5].Name);
				AssertEquals("The Consignee tab should be the 7th tab", "ConsigneeTabPage", form.OrganisationsTabControl.TabPages[6].Name);
			}

			Env.Registry.SetOrgShowARTab(true);
			Env.Registry.SetOrgShowConsigneeConsignorTab(false);
			using (ClientIntelligenceFormForTest form = new ClientIntelligenceFormForTest(organization))
			{
				AssertEquals("Form should show 8 tabs with only AR registry setting true", 8, form.OrganisationsTabControl.TabCount);
				AssertEquals("The Receivables tab should be the 5th tab", "ReceivablesTabPage", form.OrganisationsTabControl.TabPages[4].Name);
			}

			Env.Registry.SetOrgShowARTab(false);
			Env.Registry.SetOrgShowConsigneeConsignorTab(true);
			using (ClientIntelligenceFormForTest form = new ClientIntelligenceFormForTest(organization))
			{
				AssertEquals("Form should show 9 tabs with only Consignee/Consignor registry setting true", 9, form.OrganisationsTabControl.TabCount);
				AssertEquals("The Consignor tab should be the 5th tab", "ConsignorTabPage", form.OrganisationsTabControl.TabPages[4].Name);
				AssertEquals("The Consignee tab should be the 6th tab", "ConsigneeTabPage", form.OrganisationsTabControl.TabPages[5].Name);
			}

			//restore original registry setting
			Env.Registry.SetOrgShowConsigneeConsignorTab(false);
		}

		public override void TestMinimumSizeNotTooBig()
		{
			string formName;
			using (Form testForm = GetFormToBash())
			{
				formName = testForm.Name;

				int minScreenWidthSupported = ControlDpiScalingHelper.ScaleToCurrentDpiX(1200);
				int minScreenHeightSupported = ControlDpiScalingHelper.ScaleToCurrentDpiY(768);
				int typicalTaskbarHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(43);

				int maxSizeWidth = minScreenWidthSupported;
				int maxSizeHeight = minScreenHeightSupported - typicalTaskbarHeight;

				Assert("Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(), testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(), testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}
	}
}
