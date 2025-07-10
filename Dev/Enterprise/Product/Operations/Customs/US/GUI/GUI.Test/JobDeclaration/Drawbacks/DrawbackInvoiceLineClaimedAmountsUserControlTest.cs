using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class DrawbackInvoiceLineClaimedAmountsUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var invocieLineUserControl = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					invocieLineUserControl.LineDetailTabControl.SelectedTab = invocieLineUserControl.ClaimedAmountsTabPage;
					var claimsAmountUserControl = invocieLineUserControl.DrawbackInvoiceLineClaimedAmountsUserControl;
					AssertNotNull(claimsAmountUserControl);
					var acsClaimedPanel = claimsAmountUserControl.Controls.Find("ACSClaimedPanel", true)[0];
					var aceClaimedPanel = claimsAmountUserControl.Controls.Find("ACEClaimedPanel", true)[0];
					AssertEquals("ACSClaimedPanel is visible for ACS", true, acsClaimedPanel.Visible);
					AssertEquals("ACEClaimedPanel is not visible for ACS", false, aceClaimedPanel.Visible);
					AssertEquals("_99ClaimedLabel is visible for DRW", true, claimsAmountUserControl.Controls.Find("_99ClaimedLabel", true)[0].Visible);
					AssertEquals("_99ClaimedHMFCalcEdit is visible for DRW", true, claimsAmountUserControl.Controls.Find("_99ClaimedHMFCalcEdit", true)[0].Visible);
					AssertEquals("_99ClaimedMPFCalcEdit is visible for DRW", true, claimsAmountUserControl.Controls.Find("_99ClaimedMPFCalcEdit", true)[0].Visible);
					AssertEquals("_99ClaimedTaxCalcEdit is visible for DRW", true, claimsAmountUserControl.Controls.Find("_99ClaimedTaxCalcEdit", true)[0].Visible);
					AssertEquals("_99ClaimedOtherFeesCaclEdit is visible for DRW", true, claimsAmountUserControl.Controls.Find("_99ClaimedOtherFeesCaclEdit", true)[0].Visible);
					AssertEquals("_99ClaimedDutyCalcEdit is visible for DRW", true, claimsAmountUserControl.Controls.Find("_99ClaimedDutyCalcEdit", true)[0].Visible);
					AssertEquals("MPFEligibleLabel is visible for DRW", true, claimsAmountUserControl.Controls.Find("MPFEligibleLabel", true)[0].Visible);
					AssertEquals("HMFEligibleLabel is visible for DRW", true, claimsAmountUserControl.Controls.Find("HMFEligibleLabel", true)[0].Visible);
					AssertEquals("ClaimedHMFCalcEdit is not visible for DRW", false, claimsAmountUserControl.Controls.Find("ClaimedHMFCalcEdit", true)[0].Visible);
					AssertEquals("ClaimedMPFCalcEdit is not visible for DRW", false, claimsAmountUserControl.Controls.Find("ClaimedMPFCalcEdit", true)[0].Visible);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					AssertEquals("ACSClaimedPanel is not visible for ACE", false, acsClaimedPanel.Visible);
					AssertEquals("ACEClaimedPanel is visible for ACE", true, aceClaimedPanel.Visible);
					var aceAmountsTopPanel = aceClaimedPanel.Controls.Find("ACEAmountsTopPanel", true)[0];
					AssertEquals("ACEQuarterlyHMFCheckBox is visible for ACE", true, aceAmountsTopPanel.Controls.Find("ACEQuarterlyHMFCheckBox", true)[0].Visible);
					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
					AssertEquals("_99ClaimedLabel is not visible for 7552", false, claimsAmountUserControl.Controls.Find("_99ACEClaimedLabel", true)[0].Visible);
					AssertEquals("_99ClaimedHMFCalcEdit is not visible for 7552", false, claimsAmountUserControl.Controls.Find("_99ACEClaimedHMFCalcEdit", true)[0].Visible);
					AssertEquals("_99ClaimedMPFCalcEdit is not visible for 7552", false, claimsAmountUserControl.Controls.Find("_99ACEClaimedMPFCalcEdit", true)[0].Visible);
					AssertEquals("_99ClaimedTaxCalcEdit is not visible for 7552", false, claimsAmountUserControl.Controls.Find("_99ACEClaimedTaxCalcEdit", true)[0].Visible);
					AssertEquals("_99ClaimedDutyCalcEdit is not visible for 7552", false, claimsAmountUserControl.Controls.Find("_99ACEClaimedDutyCalcEdit", true)[0].Visible);
					AssertEquals("MPFEligibleLabel is not visible for 7552", false, claimsAmountUserControl.Controls.Find("ACEMPFEligibleLabel", true)[0].Visible);
					AssertEquals("HMFEligibleLabel is not visible for 7552", false, claimsAmountUserControl.Controls.Find("ACEHMFEligibleLabel", true)[0].Visible);
					AssertEquals("ClaimedHMFCalcEdit is visible for 7552", true, claimsAmountUserControl.Controls.Find("ACEClaimedHMFCalcEdit", true)[0].Visible);
					AssertEquals("ClaimedMPFCalcEdit is visible for 7552", true, claimsAmountUserControl.Controls.Find("ACEClaimedMPFCalcEdit", true)[0].Visible);
				}
			}
		}
	}
}
