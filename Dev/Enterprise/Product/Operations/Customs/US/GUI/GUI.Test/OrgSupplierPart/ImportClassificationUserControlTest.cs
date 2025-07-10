using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ImportClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestCusUSClassificationDeletedAfterPivotDeleted()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TESTPROD";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var classPK = pivot.Details.PK;
			Factory.Save();
			using (TestForm form = new TestForm(pivot))
			{
				form.Show();
				ImportClassificationUserControl userControl = form.UserControl;
				userControl.ImportTabControl.SelectedTab = userControl.LicenceNoTabPage;
				pivot.Delete();
				userControl.CurrentPivot = pivot;
				Assert(userControl.CurrentPivot.IsDeleted);
				Factory.Save();
				AssertNull(new BusinessObjectFactory().Load<CusUSClassification>(classPK));
			}
		}

		public void TestTabCaption()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			using (TestForm form = new TestForm(pivot))
			{
				form.Show();
				ImportClassificationUserControl userControl = form.UserControl;
				userControl.ImportTabControl.SelectedTab = userControl.LicenceNoTabPage;
				AssertEquals("Tab Caption should be 'Permits/Licenses'", "Permits/Licenses", userControl.ImportTabControl.SelectedTab.Text);
			}
		}

		public void TestAttributesTabPageVisibility()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			using (var form = new PartImportClassificationForm(pivot))
			{
				form.Show();
				var userControl = form.importClassificationUserControl;
				AssertEquals(false, userControl.AttributesTabPage.TabVisible);
			}
		}

		public void TestPSTTabPageVisibility()
		{
			using (var form = new PartImportClassificationForm())
			{
				var userControl = form.importClassificationUserControl;

				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				userControl.CurrentPivot = pivot;

				form.Show();
				userControl.ImportTabControl.SelectedTab = userControl.pSTTabPage;

				AssertEquals("userControl.PSTTabPage.TabVisible", false, userControl.pSTTabPage.TabVisible);
				AssertEquals("userControl.PSTUserControl.Enabled", false, userControl.pstUserControl.Enabled);
				AssertEquals("userControl.PSTUserControl.Visible", false, userControl.pstUserControl.Visible);
				AssertEquals("userControl.PSTDisclaimControl.Enabled", false, userControl.pstDisclaimControl.Enabled);
				AssertEquals("userControl.PSTDisclaimControl.Visible", false, userControl.pstDisclaimControl.Visible);

				pivot.Details.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.PSTTabPage.TabVisible", true, userControl.pSTTabPage.TabVisible);
				AssertEquals("userControl.PSTUserControl.Enabled", true, userControl.pstUserControl.Enabled);
				AssertEquals("userControl.PSTUserControl.Visible", true, userControl.pstUserControl.Visible);
				AssertEquals("userControl.PSTDisclaimControl.Enabled", false, userControl.pstDisclaimControl.Enabled);
				AssertEquals("userControl.PSTDisclaimControl.Visible", false, userControl.pstDisclaimControl.Visible);

				pivot.Details.CD_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.PSTTabPage.TabVisible", true, userControl.pSTTabPage.TabVisible);
				AssertEquals("userControl.PSTUserControl.Enabled", false, userControl.pstUserControl.Enabled);
				AssertEquals("userControl.PSTUserControl.Visible", false, userControl.pstUserControl.Visible);
				AssertEquals("userControl.PSTDisclaimControl.Enabled", true, userControl.pstDisclaimControl.Enabled);
				AssertEquals("userControl.PSTDisclaimControl.Visible", true, userControl.pstDisclaimControl.Visible);

				pivot.Details.CD_PSTIndicator = "";
				AssertEquals("userControl.PSTTabPage.TabVisible", false, userControl.pSTTabPage.TabVisible);
				AssertEquals("userControl.PSTUserControl.Enabled", false, userControl.pstUserControl.Enabled);
				AssertEquals("userControl.PSTUserControl.Visible", false, userControl.pstUserControl.Visible);
				AssertEquals("userControl.PSTDisclaimControl.Enabled", false, userControl.pstDisclaimControl.Enabled);
				AssertEquals("userControl.PSTDisclaimControl.Visible", false, userControl.pstDisclaimControl.Visible);
			}
		}

		public void TestHFCTabPageVisibility()
		{
			using (var form = new PartImportClassificationForm())
			{
				var userControl = form.importClassificationUserControl;
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				userControl.CurrentPivot = pivot;

				form.Show();
				userControl.ImportTabControl.SelectedTab = userControl.hfcTabPage;

				AssertEquals("userControl.HFCTabPage.TabVisible", false, userControl.hfcTabPage.TabVisible);

				pivot.Details.CD_HFCIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.HFCTabPage.TabVisible", true, userControl.hfcTabPage.TabVisible);

				pivot.Details.CD_HFCIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.HFCTabPage.TabVisible", false, userControl.hfcTabPage.TabVisible);

				var hfc = pivot.HFCHeaders.AddNew();
				hfc.US_ASHRAENumber = "R-123ABC";
				pivot.Details.CD_HFCIndicator = "";
				AssertEquals("userControl.HFCTabPage.TabVisible", true, userControl.hfcTabPage.TabVisible);
			}
		}

		public void TestAMSTabPageVisibility()
		{
			using (var form = new PartImportClassificationForm())
			{
				var userControl = form.importClassificationUserControl;

				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				userControl.CurrentPivot = pivot;

				form.Show();
				userControl.ImportTabControl.SelectedTab = userControl.pSTTabPage;

				AssertEquals("userControl.AMSTabPage.TabVisible", false, userControl.aMSTabPage.TabVisible);
				AssertEquals("userControl.AMSUserControl.Enabled", false, userControl.amsUserControl.Enabled);
				AssertEquals("userControl.AMSUserControl.Visible", false, userControl.amsUserControl.Visible);
				AssertEquals("userControl.AMSDisclaimControl.Enabled", false, userControl.amsDisclaimControl.Enabled);
				AssertEquals("userControl.AMSDisclaimControl.Visible", false, userControl.amsDisclaimControl.Visible);

				pivot.Details.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.AMSTabPage.TabVisible", true, userControl.aMSTabPage.TabVisible);
				AssertEquals("userControl.AMSUserControl.Enabled", true, userControl.amsUserControl.Enabled);
				AssertEquals("userControl.AMSUserControl.Visible", true, userControl.amsUserControl.Visible);
				AssertEquals("userControl.AMSDisclaimControl.Enabled", false, userControl.amsDisclaimControl.Enabled);
				AssertEquals("userControl.AMSDisclaimControl.Visible", false, userControl.amsDisclaimControl.Visible);

				pivot.Details.CD_AMSIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.AMSTabPage.TabVisible", true, userControl.aMSTabPage.TabVisible);
				AssertEquals("userControl.AMSUserControl.Enabled", false, userControl.amsUserControl.Enabled);
				AssertEquals("userControl.AMSUserControl.Visible", false, userControl.amsUserControl.Visible);
				AssertEquals("userControl.AMSDisclaimControl.Enabled", true, userControl.amsDisclaimControl.Enabled);
				AssertEquals("userControl.AMSDisclaimControl.Visible", true, userControl.amsDisclaimControl.Visible);

				pivot.Details.CD_AMSIndicator = "";
				AssertEquals("userControl.AMSTabPage.TabVisible", false, userControl.aMSTabPage.TabVisible);
				AssertEquals("userControl.AMSUserControl.Enabled", false, userControl.amsUserControl.Enabled);
				AssertEquals("userControl.AMSUserControl.Visible", false, userControl.amsUserControl.Visible);
				AssertEquals("userControl.AMSDisclaimControl.Enabled", false, userControl.amsDisclaimControl.Enabled);
				AssertEquals("userControl.AMSDisclaimControl.Visible", false, userControl.amsDisclaimControl.Visible);
			}
		}

		public void TestCPSCTabPageVisibility()
		{
			using (var form = new PartImportClassificationForm())
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				var userControl = form.importClassificationUserControl;
				userControl.CurrentPivot = pivot;
				form.Show();
				userControl.ImportTabControl.SelectedTab = userControl.cPSCTabPage;

				AssertEquals("userControl.cPSCTabPage.TabVisible", false, userControl.cPSCTabPage.TabVisible);

				pivot.Details.CD_CPSCIndicator = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.cPSCTabPage.TabVisible", true, userControl.cPSCTabPage.TabVisible);

				pivot.Details.CD_CPSCIndicator = OGAIndicatorList.Codes.Disclaimed;
				AssertEquals("userControl.cPSCTabPage.TabVisible", false, userControl.cPSCTabPage.TabVisible);

				pivot.Details.CD_CPSCDisclaimReason = PGADisclaimReasonList.Codes.A;
				AssertEquals("userControl.cPSCTabPage.TabVisible", true, userControl.cPSCTabPage.TabVisible);

				pivot.Details.CD_CPSCDisclaimReason = PGADisclaimReasonList.Codes.B;
				AssertEquals("userControl.cPSCTabPage.TabVisible", false, userControl.cPSCTabPage.TabVisible);
			}
		}

		public void TestCPSCTabPageLotsGridVisibility()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART";
			product.OP_Desc = "TEST PART";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_FormattedTariffNum = "11111";
			pivot.CD_CPSCIndicator = OGAIndicatorList.Codes.Declared;
			var cpscLine = pivot.CPSCLines.AddNew();
			cpscLine.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;

			using (var form = new OrgSupplierPartForm(product))
			{
				form.Show();
				form.SelectMainTabPageForTest();
				var userControl = form.Controls.Find("importClassificationUserControl", true).Single() as ImportClassificationUserControl;
				var cpscTabPage = userControl.cPSCTabPage;
				userControl.ImportTabControl.SelectedTab = cpscTabPage;
				var cpscUserControl = cpscTabPage.Controls.Find("CPSCControl", true).Single() as CPSCUserControl;
				Assert("Lots grid should not be shown", !cpscUserControl.LotsGridVisible);
				cpscUserControl.ViewEditButton.PerformClick();
				var cpscForm = ZFormModaliser.LastFormShownDialogForTest as CPSCForm;
				AssertNotNull(cpscForm);
				Assert("Lots grid should not be shown", !cpscForm.LotsGridVisible);
			}
		}

		public void TestImportPGASwitchByCustomLineChange()
		{
			var product = Factory.New<OrgSupplierPart>();
			using (OrgSupplierPartForm form = new OrgSupplierPartForm(product))
			{
				product.OP_PartNum = "PROPGA";
				product.OP_Desc = "PRODUCT PGA";
				var pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_FormattedTariffNum = "1111";
				pivot.CD_DEAIndicator = "D";
				var deaLine = pivot.DEAHeaders.AddNew();
				deaLine.US_CountryOfShipment = "AU";
				var pivot2 = product.PivotsForBinding.AddNew();
				form.Show();
				form.SelectMainTabPageForTest();
				var userControl = form.Controls.Find("importClassificationUserControl", true)[0] as ImportClassificationUserControl;
				var exportTabControl = userControl.Controls.Find("ImportTabControl", true)[0] as ZTabControl;
				exportTabControl.SelectedTab = userControl.DEATabPage;
				var grid = form.Controls.Find("pivotGrid", true)[0] as ZGrid;
				grid.SelectSingleElement(pivot2);
				pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot2.CI_FormattedTariffNum = "2222";
				pivot2.CD_DEAIndicator = "D";
				var deaLine2 = pivot2.DEAHeaders.AddNew();
				deaLine2.US_CountryOfShipment = "US";
				grid.SelectSingleElement(pivot);
				var deaGrid = userControl.DEAControl.Controls.Find("DEAHeaderGrid", true)[0] as ZGrid;
				AssertEquals("AU", deaGrid[0, 0].ToString());
			}
		}

		public void TestADDDepositRateZTextBoxAndCVDDepositRateZTextBox()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TESTPROD";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Factory.Save();
			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				var userControl = form.Controls.Find("importClassificationUserControl", true)[0] as ImportClassificationUserControl;
				AssertNotNull("The ADDDepositRateZTextBox should exist in ImportClassificationUserControl", userControl.Controls.Find("ADDDepositRateZTextBox", true).Single());
				AssertNotNull("The CVDDepositRateZTextBox should exist in ImportClassificationUserControl", userControl.Controls.Find("CVDDepositRateZTextBox", true).Single());
			}
		}

		public void TestCBMARelatedControls()
		{
			var product = Factory.New<OrgSupplierPart>();

			using (OrgSupplierPartForm form = new OrgSupplierPartForm(product))
			{
				form.Show();

				var ttbRateDesignationCodeDropEdit = form.FindSingle<ZDropEdit>("TTBRateDesignationCodeDropEdit");
				var cbmaDefaultTaxRateCalcEdit = form.FindSingle<ZCalcEdit>("CBMADefaultTaxRateCalcEdit");

				AssertEquals("BindingMember", "CD_TTBRateDesignationCode", ttbRateDesignationCodeDropEdit.GetBindingMember());
				AssertEquals("BindingMember", "CD_CBMADefaultTaxRate", cbmaDefaultTaxRateCalcEdit.GetBindingMember());
			}
		}

		public void TestAluminumSmeltAndCastCountryControlsVisibility()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TESTPROD";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Factory.Save();

			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				var userControl = form.Controls.Find("importClassificationUserControl", true)[0] as ImportClassificationUserControl;
				userControl.ImportTabControl.SelectedTab = userControl.LicenceNoTabPage;
				Assert("The AluminumSmeltGroupBox is visible when Smelt functionality is effective", userControl.Controls.Find("AluminumSmeltGroupBox", true)[0].Visible);
				Assert("The PrimaryCountryNotApplicableCheckBox is visible when Smelt functionality is effective", userControl.Controls.Find("PrimaryCountryNotApplicableCheckBox", true)[0].Visible);
				Assert("The PrimaryCountryCodeFindBox is visible when Smelt functionality is effective", userControl.Controls.Find("PrimaryCountryCodeFindBox", true)[0].Visible);
				Assert("The SecondaryCountryNotApplicableCheckBox is visible when Smelt functionality is effective", userControl.Controls.Find("SecondaryCountryNotApplicableCheckBox", true)[0].Visible);
				Assert("The SecondaryCountryCodeFindBox is visible when Smelt functionality is effective", userControl.Controls.Find("SecondaryCountryCodeFindBox", true)[0].Visible);
				Assert("The CastCountryCodeFindBox is visible when Smelt functionality is effective", userControl.Controls.Find("CastCountryCodeFindBox", true)[0].Visible);
			}
		}

		public void TestAdditionalProvTariffColumnAndUserControls()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TESTPROD";
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Factory.Save();

			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				var grid = form.Controls.Find("pivotGrid", true)[0] as ZGrid;
				var additionalProvTariff1Column = grid.Columns["SupFormattedAdditionalTariff1"];
				AssertNotNull("Prov Add. Tariff 1 columns is available", additionalProvTariff1Column);
				var additionalProvTariff2Column = grid.Columns["SupFormattedAdditionalTariff2"];
				AssertNotNull("Prov Add. Tariff 2 columns is available", additionalProvTariff2Column);
				var additionalProvTariff3Column = grid.Columns["SupFormattedAdditionalTariff3"];
				AssertNotNull("Prov Add. Tariff 3 columns is available", additionalProvTariff3Column);
				var additionalProvTariff4Column = grid.Columns["SupFormattedAdditionalTariff4"];
				AssertNotNull("Prov Add. Tariff 4 columns is available", additionalProvTariff4Column);
				var additionalProvTariff5Column = grid.Columns["SupFormattedAdditionalTariff5"];
				AssertNotNull("Prov Add. Tariff 5 columns is available", additionalProvTariff5Column);

				var userControl = form.Controls.Find("importClassificationUserControl", true)[0] as ImportClassificationUserControl;
				userControl.ImportTabControl.SelectedTab = userControl.additionalTariffsTabPage;
				Assert("AdditionalTariff1FindBox is visible", userControl.Controls.Find("additionalTariff1FindBox", true)[0].Visible);
				Assert("AdditionalTariff2FindBox is visible", userControl.Controls.Find("additionalTariff2FindBox", true)[0].Visible);
				Assert("AdditionalTariff3FindBox is visible", userControl.Controls.Find("additionalTariff3FindBox", true)[0].Visible);
				Assert("AdditionalTariff4FindBox is visible", userControl.Controls.Find("additionalTariff4FindBox", true)[0].Visible);
				Assert("AdditionalTariff5FindBox is visible", userControl.Controls.Find("additionalTariff5FindBox", true)[0].Visible);
			}
		}

		sealed class TestForm : ZForm
		{
			public TestForm(CusClassPartPivot pivot) : base(pivot)
			{
			}

			internal ImportClassificationUserControl UserControl;

			protected override void InitializeComponent()
			{
				UserControl = new ImportClassificationUserControl(null);
				Controls.Add(UserControl);
				DataSourceAssemblyName = "Enterprise.Customs.US.Business";
				DataSourceTypeName = "Enterprise.Customs.US.Business.Pivot";
			}
		}
	}
}
