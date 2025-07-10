using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class USACSImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestRefreshMiscLicenceLabel()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000";
			tariff1.UE_PermitLicenseIndicator = "01";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0001";
			tariff2.UE_PermitLicenseIndicator = "02";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0001";
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;
				userControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 0;
				Application.DoEvents();
				AssertEquals("PreCondition", invoiceLine, userControl.CustomsInvoiceLinesBoundGrid.ListManager.GetCurrent());
				AssertEquals("Licence label text", "Steel License No.", userControl.MiscNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				userControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals("PreCondition", invoiceLine2, userControl.CustomsInvoiceLinesBoundGrid.ListManager.GetCurrent());
				AssertEquals("Licence label text", "SG TPL License No.", userControl.MiscNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				invoiceLine2.JI_Tariff = "0000";
				AssertEquals("Licence label text", "Steel License No.", userControl.MiscNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;
				userControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 0;
				Application.DoEvents();
				AssertEquals("PreCondition", invoiceLine, userControl.CustomsInvoiceLinesBoundGrid.ListManager.GetCurrent());
				AssertEquals("Licence label text", "License No.", userControl.MiscNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				userControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals("PreCondition", invoiceLine2, userControl.CustomsInvoiceLinesBoundGrid.ListManager.GetCurrent());
				AssertEquals("Licence label text", "License No.", userControl.MiscNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestPropertiesOfADD_CVD_Fields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0001";
			invoiceLine2.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;
				userControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 0;
				Application.DoEvents();
				AssertEquals("PreCondition", invoiceLine, userControl.CustomsInvoiceLinesBoundGrid.ListManager.GetCurrent());
				AssertEquals("ADDutyCalcEdit", false, userControl.ADDutyCalcEdit.Visible);
				AssertEquals("CVDutyCalcEdit", false, userControl.CVDutyCalcEdit.Visible);
				AssertEquals("ADDDepositRateDropEdit", false, userControl.ADDDepositRateDropEdit.ShowDescriptionBox);
				AssertEquals("CVDDepositRateDropEdit", false, userControl.CVDDepositRateDropEdit.ShowDescriptionBox);
				userControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals("PreCondition", invoiceLine2, userControl.CustomsInvoiceLinesBoundGrid.ListManager.GetCurrent());
				AssertEquals("ADDutyCalcEdit", false, userControl.ADDutyCalcEdit.Visible);
				AssertEquals("CVDutyCalcEdit", true, userControl.CVDutyCalcEdit.Visible);
				AssertEquals("ADDDepositRateDropEdit", false, userControl.ADDDepositRateDropEdit.ShowDescriptionBox);
				AssertEquals("CVDDepositRateDropEdit", false, userControl.CVDDepositRateDropEdit.ShowDescriptionBox);
			}
		}

		public void TestTabsVisibilityForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				Assert(!userControl.PGAFDATabPage.TabVisible);
				Assert(!userControl.FDAOtherTabPage.TabVisible);
			}

			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = "P";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				Assert(userControl.PGAFDATabPage.TabVisible);
				Assert(!userControl.FDAOtherTabPage.TabVisible);
			}

			declaration.US_F_PNMode = "O";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				Assert(!userControl.PGAFDATabPage.TabVisible);
				Assert(userControl.FDAOtherTabPage.TabVisible);
			}

			declaration.US_EnableSPN = false;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				Assert(!userControl.PGAFDATabPage.TabVisible);
				Assert(!userControl.FDAOtherTabPage.TabVisible);
			}
		}

		public void TestFDATabsVisibilityForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;
				Assert("Licence/Permit tab is visible.", userControl.LicencePermitsDetailsTabPage.TabVisible);
				Assert("License Type is visible.", userControl.LicenseTypeCodeDropEdit.Visible);
				Assert("Misc License No. is visible.", userControl.MiscNoTextBox.Visible);
				Assert("Textile Classification Details GroupBox is invisible.", !userControl.TextileClassificationDetailsGroupBox.Visible);
				Assert("Softwood Lumber GroupBox is invisible.", !userControl.TextileClassificationDetailsGroupBox.Visible);
				Assert("ADD/CVD Misc GroupBox is visible.", userControl.ADDCVDMiscGroupBox.Visible);
				Assert("ADD DepositValueCalcFind is invisible", !userControl.ADDDepositValueCalcFindBox.Visible);
				Assert("ADD IsBonded is invisible", !userControl.IsADDBondedCheckBox.Visible);
				Assert("ADD Deposit Rate is invisible", !userControl.ADDDepositRateDropEdit.Visible);
				Assert("ADD Deposit Rate is invisible", !userControl.ADDDepositRateZTextBox.Visible);
				Assert("CVD DepositValueCalcFind is invisible", !userControl.CVDDepositValueCalcFindBox.Visible);
				Assert("CVD IsBonded is invisible", !userControl.IsCVDBondedCheckBox.Visible);
				Assert("CVD Deposit Rate is invisible", !userControl.CVDDepositRateDropEdit.Visible);
				Assert("CVD Deposit Rate is invisible", !userControl.CVDDepositRateZTextBox.Visible);
			}
		}

		public void TestTabsVisibilityForACS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACSImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.LicencePermitsDetailsTabPage;
				Assert("Licence/Permit tab is visible.", userControl.LicencePermitsDetailsTabPage.TabVisible);
				Assert("License Type is invisible.", !userControl.LicenseTypeCodeDropEdit.Visible);
				Assert("Misc License No. is visible.", userControl.MiscNoTextBox.Visible);
				Assert("ADD/CVD Details GroupBox is visible.", userControl.ADDCVDMiscGroupBox.Visible);
				Assert("IsADCVDCertCheckBox should be visible", userControl.IsADCVDCertCheckBox.Visible);
				Assert("ADD GroupBox is visible.", userControl.ADDGroupBox.Visible);
				Assert("CVD GroupBox is visible.", userControl.CVDGroupBox.Visible);
				Assert("IsADDBondedCheckBox should be visible", userControl.IsADDBondedCheckBox.Visible);
				Assert("IsCVDBondedCheckBox should be visible", userControl.IsCVDBondedCheckBox.Visible);
				Assert("Textile Classification Details GroupBox is visible.", userControl.TextileClassificationDetailsGroupBox.Visible);
				Assert("Softwood Lumber GroupBox is visible.", userControl.TextileClassificationDetailsGroupBox.Visible);
			}
		}

		public void TestFDALinesGridColumnNames()
		{
			using (var control = new USACSImportInvoiceLineUserControl())
			{
				AssertEquals("US_UC_NKFDAProduction caption", "Prod. Ctry/Rgn.", control.FDALinesGrid.GetColumnStyle("US_UC_NKFDAProduction").Caption);
			}
		}
	}
}
