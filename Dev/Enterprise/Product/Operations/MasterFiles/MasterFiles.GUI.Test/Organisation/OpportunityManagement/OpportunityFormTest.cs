using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OpportunityForm))]
	sealed class OpportunityFormTest : ZFormBasherTest
	{
		#region Implementation

		[GuiTest]
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

				Assert(
					"Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(),
testForm.MinimumSize.Width <= maxSizeWidth);
				Assert(
					"Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(),
					testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var form = new OpportunityForm(opportunity);
			form.ControllerID = ControllerIDs.Opportunity;
			return form;
		}

		#endregion

		#region Navigate

		public void TestNavigateToCommissionAgreement()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement1 = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement1.FillWithValidTestData();
			var agreement2 = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();

			Factory.Save();

			using (var form = new OpportunityForm(opportunity))
			{
				form.Show();
				form.NavigateToCommissionAgreement(agreement2);

				var agreementGridCurrent = form.DetailsControl.CommissionAgreementsControl.AgreementsGrid.ListManager.GetCurrent();
				AssertType(typeof(OrgCommissionAgreement), agreementGridCurrent);
				AssertEquals(agreement2.PK, ((OrgCommissionAgreement)agreementGridCurrent).MainVersion.PK);
			}
		}

		#endregion

		#region ZForm Overrides

		public void TestFormCaption()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Organisation";

			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			opp.P8_OpportunityID = "OP12345";
			using (OpportunityForm form = new OpportunityForm(opp))
			{
				AssertEquals("Opportunity OP12345", form.FormCaption);
				opp.P8_OH = org.PK;
				AssertEquals("Opportunity OP12345 - Some Organisation", form.FormCaption);
				form.SetDataBinding(null, "");
				AssertEquals("Opportunity", form.FormCaption);
			}
		}

		#endregion

		public void TestPlugInsAdded()
		{
			using (var form = (OpportunityForm)GetFormToBash())
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}
	}
}
