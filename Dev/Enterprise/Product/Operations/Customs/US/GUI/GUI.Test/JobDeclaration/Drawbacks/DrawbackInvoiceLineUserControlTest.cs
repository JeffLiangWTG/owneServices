using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class DrawbackInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibilityChanged()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			declaration.JE_DeclarationReference = "HELLO WORLD";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			using (USDrawbackCustomsBrokerageUserControl brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (DrawbackInvoiceLineUserControl control = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					Assert("Line charges tab page removed", !control.LineDetailTabControl.TabPages.Contains(control.LineChargesTabPage));
					Assert("InvoiceLinesSummaryGroupBox is not visible", !control.InvoiceLinesSummaryGroupBox.Visible);
					Assert("Control not visible", !control.VolumeCalcDropEdit.Visible);
					Assert("Control not visible", !control.JI_WeightCalcDropEdit.Visible);
					Assert("Control not visible", !control.JI_LinePriceBoundCurrencyControl.Visible);
				}
			}
		}

		public void TestControlLabelOnDrawbackInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var controls = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					var control = controls.Controls.Find("ACEExportCountryCodeFindBox", true)[0] as IResCaptionedControl;
					AssertEquals("ACEExportCountryCodeFindBox", "Destination", control.CaptionResourceString.Caption);
				}
			}
		}

		public void TestExchangeRateColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var controls = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					var grid = controls.FindSingleOrDefault<ZGrid>("DrawbackNAFTAGrid");
					AssertNotNull(grid);
					var exchangeRateColumn = grid.GetColumnStyle("US_DRWNAFTACountryDutyRate");
					AssertNotNull(exchangeRateColumn);
					AssertEquals("Exchange Rate", exchangeRateColumn.Caption);
					AssertEquals(typeof(ZCalcEditColumnStyleInfo), exchangeRateColumn.GetType());
					AssertEquals(6, ((ZCalcEditColumnStyleInfo)exchangeRateColumn).Decimals);
				}
			}
		}

		public void TestAddDefaultInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_DeclarationReference = "HELLO WORLD";
			AssertEquals("PreCondition: No Invoice", 0, declaration.Invoices.Count);
			using (var control = new DrawbackInvoiceLineUserControl())
			{
				control.SetDataBinding(declaration, "");
				AssertEquals("Invoices Count", 1, declaration.Invoices.Count);
				JobComInvoiceHeader invoice = declaration.Invoices[0];
				AssertEquals("Invoice.JZ_InvoiceNumber", declaration.JE_DeclarationReference, invoice.JZ_InvoiceNumber);
			}
		}

		public void TestColumnNamesInSortOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			using (var form = new ZForm(declaration))
			{
				using (var control = new DrawbackInvoiceLineUserControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					control.ChangeGridColumnsVisibilityInternal();
					Assert("Control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Count);
					for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
					{
						var columnInfo = control.CustomsInvoiceLinesBoundGrid.ColumnStyles[i] as ZGridColumnInfo;
						AssertNotNull(columnInfo);
						var expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
						AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
					}
				}
			}
		}

		public void TestDrawBackControlNotDiplayOrgCustomisedColumn()
		{
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Org2";
			OrgCustomLabels customLabel = org2.CustomLabels.AddNew();
			customLabel.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1;
			customLabel.OT_Caption = "Hair Colour";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (DrawbackInvoiceLineUserControl control = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					var customsInvoiceLinesBoundGrid = control.Controls.Find("CustomsInvoiceLinesBoundGrid", true)[0] as ZGrid;
					AssertNull("Column is NOT available", customsInvoiceLinesBoundGrid.Columns["CustomAttribute1"]);
				}
			}
		}

		public void TestControlVisibilityWhenDrawbackPurposeChanged()
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
				using (DrawbackInvoiceLineUserControl control = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					Assert("Control is visible", control.Controls.Find("ExportSectionGroupBox", true)[0].Visible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("NAFTATabPage", true)[0])).TabVisible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("AdditionalTariffsTabPage", true)[0])).TabVisible);
					Assert("Control is visible", control.Controls.Find("IsForImportSectionCheckBox", true)[0].Visible);
					Assert("Control is not visible", !control.Controls.Find("CDUseDropEdit", true)[0].Visible);
					Assert("Control is not visible", !control.Controls.Find("CDIndicatorCheckBox", true)[0].Visible);
					Assert("Control is not visible", !control.Controls.Find("DateDelFromDateEdit", true)[0].Visible);
					Assert("Control is not visible", !control.Controls.Find("DateDelToDateEdit", true)[0].Visible);
					Assert("Control is not visible", !control.Controls.Find("_7552DrawbackProductCheckBox", true)[0].Visible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("ManufacturedArticlesTabPage", true)[0])).TabVisible);

					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
					Assert("Control is not visible", !control.Controls.Find("ExportSectionGroupBox", true)[0].Visible);
					AssertEquals("Control is not visible", 0, control.Controls.Find("NAFTATabPage", true).Length);
					AssertEquals("Control is not visible", 0, control.Controls.Find("AdditionalTariffsTabPage", true).Length);
					Assert("Control is not visible", !control.Controls.Find("IsForImportSectionCheckBox", true)[0].Visible);
					Assert("Control is visible", control.Controls.Find("CDUseDropEdit", true)[0].Visible);
					Assert("Control is visible", control.Controls.Find("CDIndicatorCheckBox", true)[0].Visible);
					Assert("Control is visible", control.Controls.Find("DateDelFromDateEdit", true)[0].Visible);
					Assert("Control is visible", control.Controls.Find("DateDelToDateEdit", true)[0].Visible);
					Assert("Control is visible", control.Controls.Find("_7552DrawbackProductCheckBox", true)[0].Visible);
					AssertEquals("Control is not visible", 0, control.Controls.Find("ManufacturedArticlesTabPage", true).Length);
				}
			}
		}

		public void TestSerialNumberColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;

			using (var form = new ZForm(declaration))
			{
				using (var control = new DrawbackInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					Assert("Serial Number column is not present for drawbacks", !control.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_SerialNumber));
				}
			}
		}

		public void TestACEControlsVisiblity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var control = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					Assert("Control is visible", control.Controls.Find("ACEImportExportSectionsPanel", true)[0].Visible);
					Assert("Control is visible", control.Controls.Find("ACEExportSectionGroupBox", true)[0].Visible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("NAFTATabPage", true)[0])).TabVisible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("AdditionalTariffsTabPage", true)[0])).TabVisible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("ManufacturedArticlesTabPage", true)[0])).TabVisible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("DocumentDetailsTabPage", true)[0])).TabVisible);
					Assert("Control is not visible", !control.Controls.Find("ACSImportExportSectionsPanel", true)[0].Visible);
					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("DocumentDetailsTabPage", true)[0])).TabVisible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("ManufacturedArticlesTabPage", true)[0])).TabVisible);
					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("DocumentDetailsTabPage", true)[0])).TabVisible);
					AssertEquals("Control is not visible", control.Controls.Find("ManufacturedArticlesTabPage", true).Length, 0);

					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
					Assert("Control is visible", control.Controls.Find("ACSImportExportSectionsPanel", true)[0].Visible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("NAFTATabPage", true)[0])).TabVisible);
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("ManufacturedArticlesTabPage", true)[0])).TabVisible);
					Assert("Control is not visible", !control.Controls.Find("ACEImportExportSectionsPanel", true)[0].Visible);
					Assert("Control is not visible", !control.Controls.Find("ACEExportSectionGroupBox", true)[0].Visible);
					AssertEquals("Control is not visible", control.Controls.Find("DocumentDetailsTabPage", true).Length, 0);
					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
					Assert("Control is visible", ((ZTabPage)(control.Controls.Find("ManufacturedArticlesTabPage", true)[0])).TabVisible);
					AssertEquals("Control is not visible", control.Controls.Find("DocumentDetailsTabPage", true).Length, 0);
					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
					AssertEquals("Control is not visible", control.Controls.Find("ManufacturedArticlesTabPage", true).Length, 0);
					AssertEquals("Control is not visible", control.Controls.Find("DocumentDetailsTabPage", true).Length, 0);
				}
			}
		}

		public void TestColumnAvailability()
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
				using (var control = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					var customsInvoiceLinesBoundGrid = control.Controls.Find("CustomsInvoiceLinesBoundGrid", true)[0] as ZGrid;

					AssertEquals("No. of columns", 61, customsInvoiceLinesBoundGrid.Columns.Count);
					CheckAlwaysAvailableColumns(customsInvoiceLinesBoundGrid);

					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWIsForImportSection]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWIsForExportSection]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyPerUnit]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWExportDate]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWExportAction]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWExportID]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWExportDest]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_FormattedExportTariff]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_OA_DRWExporterOrDestroyer]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_OH_DRWExporterOrDestroyer]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCertOfManufacture]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWPort]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWEntryDate]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCMCDIndicator]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateRcvTo]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateUsedTo]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DeclaredOtherFees]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.OtherFeesPerUnit]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ClaimedOtherFees]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema._99ClaimedOtherFees]);
					AssertNull("Column is not available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWQuarterlyHMF]);

					var lineDetailTabControl = control.Controls.Find("LineDetailTabControl", true)[0] as ZTemplateTabControl;
					var additionalTariffsTabPage = lineDetailTabControl.Controls.Find("AdditionalTariffsTabPage", true)[0] as ZTabPage;
					var additionalTariffsGroupBox = additionalTariffsTabPage.Controls.Find("AdditionalTariffsGroupBox", true)[0] as ZGroupBox;
					var additionalImportTariffNumbersGrid = additionalTariffsGroupBox.Controls.Find("AdditionalImportTariffNumbersGrid", true)[0] as ZGrid;
					Assert(additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_LineNo).IsVisible);
					Assert(additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumber.Schema.US_FormattedTariff).IsVisible);
					Assert(additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_Description).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_Quantity1).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_UQ1).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_AllowableQty1).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_ValuePerUnit1).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_SubstitutedValue1).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_Quantity2).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_UQ2).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_AllowableQty2).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_ValuePerUnit2).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_SubstitutedValue2).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_Quantity3).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_UQ3).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_AllowableQty3).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_ValuePerUnit3).IsVisible);
					Assert(!additionalImportTariffNumbersGrid.GetColumnStyle(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_SubstitutedValue3).IsVisible);

					declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;

					AssertEquals("No. of columns", 55, customsInvoiceLinesBoundGrid.Columns.Count);
					CheckAlwaysAvailableColumns(customsInvoiceLinesBoundGrid);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyPerUnit]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCDInd]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCDUse]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateDelFrom]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateDelTo]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCertOfManufacture]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWPort]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWEntryDate]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCMCDIndicator]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateRcvTo]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateUsedTo]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DeclaredOtherFees]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.OtherFeesPerUnit]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ClaimedOtherFees]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema._99ClaimedOtherFees]);
				}
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			using (var brokerageControl = (USDrawbackCustomsBrokerageUserControl)form.CustomsBrokerageUserControl)
			{
				form.Show();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var control = (DrawbackInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					var customsInvoiceLinesBoundGrid = control.Controls.Find("CustomsInvoiceLinesBoundGrid", true)[0] as ZGrid;
					AssertEquals("No. of columns", 69, customsInvoiceLinesBoundGrid.Columns.Count);
					CheckAlwaysAvailableColumns(customsInvoiceLinesBoundGrid);
					AssertNull("Column is unavailable", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DutyPerUnit]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWAccMethod]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.SubstitutedValuePerUnit]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCMCDIndicator]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCertOfManufacture]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWPort]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWEntryDate]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateRcvTo]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateUsedTo]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWCDUse]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateDelFrom]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateDelTo]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWAllowableQTY]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWGoodsValuePerUQ]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.SubstitutedValuePerUnit]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWImportQuantity2]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWImportUQ2]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWAllowableQTY2]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWGoodsValuePerUQ2]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.SubstitutedValuePerUnit2]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWImportQuantity3]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWImportUQ3]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWAllowableQTY3]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWGoodsValuePerUQ3]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.SubstitutedValuePerUnit3]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWImpTrkID]);
					AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWQuarterlyHMF]);
				}
			}
		}

		void CheckAlwaysAvailableColumns(ZGrid customsInvoiceLinesBoundGrid)
		{
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_LineNo]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_PartNo]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_Description]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_ImportEntryNo]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWImportEntryLine]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateRcvFrom]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWDateUsedFrom]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_FormattedTariff]);

			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.US_DRWClaimAmountOverriden_New]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.WeightedRatio]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.MPFWeightedRatio]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWImportQuantity]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWImportUQ]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWExportQuantity]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DRWExportUQ]);

			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DeclaredVFD]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.LineDuty]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ExportValue]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.LineDutyRateDesc]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ClaimedDuty]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema._99ClaimedDuty]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.AdjClaimDuty]);

			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DeclaredTax]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.TaxPerUnit]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ClaimedTax]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema._99ClaimedTax]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.AdjClaimTax]);

			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DeclaredMPF]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.MPFPerUnit]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ClaimedMPF]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema._99ClaimedMPF]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.AdjClaimMPF]);

			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.DeclaredHMF]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.HMFPerUnit]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.ClaimedHMF]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema._99ClaimedHMF]);
			AssertNotNull("Column is available", customsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.AdjClaimHMF]);
		}

		List<ZString> expectedColumnNamesInSortOrderList;
		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new List<ZString>();
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_LineNo);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWIsForImportSection);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_ImportEntryNo);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWImportEntryLine);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_PartNo);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_FormattedTariff);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_InvoiceQuantity);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_InvoiceUQ);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_CustomsQuantity);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_CustomsUnitQty);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_Description);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWPort);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWEntryDate);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCertOfManufacture);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCMCDIndicator);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateRcvFrom);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateRcvTo);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateUsedFrom);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateUsedTo);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWImpActInd);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWClaimBasis);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWImpManufRuleNo);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWAccMethod);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWIsForExportSection);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExportDate);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExportAction);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExportID);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExportDest);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_TariffType);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_FormattedExportTariff);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExpBOLInd);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExpBOLCarrier);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExpNoticeInd);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExpWavInd);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_OH_DRWExporterOrDestroyer);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_OA_DRWExporterOrDestroyer);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCDInd);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCDUse);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateDelFrom);
					expectedColumnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateDelTo);
				}
				return expectedColumnNamesInSortOrderList;
			}
		}

		sealed class DrawbackInvoiceLineUserControlForTest : DrawbackInvoiceLineUserControl
		{
			public DrawbackInvoiceLineUserControlForTest() : base()
			{
			}

			internal void ChangeGridColumnsVisibilityInternal() => ChangeGridColumnsVisibility();
		}
	}
}
