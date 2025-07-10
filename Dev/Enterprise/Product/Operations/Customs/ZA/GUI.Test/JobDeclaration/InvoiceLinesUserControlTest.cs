using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalReferenceConstants = Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class InvoiceLinesUserControlTest : BaseInvoiceLineUserControlForVirtualPropertiesTest<InvoiceLinesUserControl>
	{
		public void TestInvoiceLineGridAllColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			using (var userControl = new InvoiceLinesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var invoiceLineGrid = userControl.CustomsInvoiceLinesBoundGrid;
				CombineAssertions("InvoiceLineGrid Should Has All Columns", () =>
				{
					AssertEquals(83, invoiceLineGrid.ColumnStyles.Count);
					#region existed columns from Base
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_LineNo));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Calc_Invoice));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_PartNo));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Tariff));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Description));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_LinePrice));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Weight));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_WeightUQ));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_NetWeight));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CC));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_OrderNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.UnitPrice));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ContainerMode));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Volume));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_VolumeUQ));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_PartAttrib1));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_PartAttrib2));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_PartAttrib3));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_SerialNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerSerialNum));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartNo));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_MatchingKey));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ClassUsageComment));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_GS_NKClassUsageCommentReviewer));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_IsClassUsageCommentRead));
					AssertNotNull(invoiceLineGrid.GetColumnStyle("InvoiceHeader+JZ_InvoiceDisplaySequence"));
					#endregion
					#region New added columns - Not null
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsThirdQuantity));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsThirdUnitQty));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_BondedWhsQuantity));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_BondedWhsUnitQty));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CO2Emission));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_EngineCapacity));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Colour));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CommissionNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_Model));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Make));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_YearOfManufacture));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_VIN));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_EngineNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_VehicleType));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_VehicleFormat));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewUsed));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CEI));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI_Description));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Procedure));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_PreviousEntryNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_PreviousEntryLineNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_TakeUpInTradeStatistics));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ROOCert));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ZZF_NKTaxType));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_ValuationMarkup));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsValueOverride));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_TargetEntryLineNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.LinkedEntryLineNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PermitNumber));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.RefundRebateCode));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.RefundRebateValue));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.TradeAgreementCode));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_AdvancePaymentNo));
					AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_BrandName));
					#endregion
				});
			}
		}

		public void TestInvoiceLineGridNewColumnsHasCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageUserControl = form.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
				var linesUserControl = (InvoiceLinesUserControl)brokerageUserControl.InvoiceLinesUserControl;
				var invoiceLineGrid = linesUserControl.CustomsInvoiceLinesBoundGrid;
				CombineAssertions("InvoiceLineGrid Columns Should Has Captions", () =>
				{
					#region New added columns - Caption
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomsThirdQuantity));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CustomsThirdUnitQty));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_BondedWhsQuantity));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_BondedWhsUnitQty));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_CO2Emission));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_EngineCapacity));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_Colour));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_CommissionNumber));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_Model));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_Make));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_YearOfManufacture));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_EngineNumber));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_VehicleType));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_VehicleFormat));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_CEI));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_CEI_Description));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_Procedure));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PreviousEntryNumber));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_PreviousEntryLineNumber).Caption);
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_TakeUpInTradeStatistics));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_ROOCert));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_TargetEntryLineNumber));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.LinkedEntryLineNumber));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_PermitNumber));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.RefundRebateCode));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.RefundRebateValue));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_AdvancePaymentNo));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_BrandName));
					#endregion
				});
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageUserControl = form.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
				var linesUserControl = (InvoiceLinesUserControl)brokerageUserControl.InvoiceLinesUserControl;
				var invoiceLineGrid = linesUserControl.CustomsInvoiceLinesBoundGrid;
				CombineAssertions("InvoiceLineGrid Columns Should Has Captions", () =>
				{
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_VIN));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_NewUsed));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLineSchema.Constants.JI_ValuationMarkup));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_CustomsValueOverride));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.TradeAgreementCode));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_ZZF_NKTaxType));
					AssertNotNullOrEmpty(invoiceLineGrid.GetColumnCaption(JobComInvoiceLine.Schema.JI_AdvancePaymentNo));
				});
			}
		}

		[TestDate(2005, 1, 1, 2, 3, 4)]
		public void TestAssessmentDatePassThroughToTariffSearch()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var kddDataGrouping = helper.CreateNewOrGetExistingDataGrouping("KDD", "KDD DESC");
			Factory.Save();
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			tariffType1P1.ZZI_ZZ9_NKNomenclatureGroupType = "KDD";
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "00876543", new ZDateTime(2000, 1, 1), new ZDateTime(2017, 01, 02));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "00876544", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstructions = new Customs.Business.CusEntryInstruction[4];
			entryInstructions[0] = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstructions[1] = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstructions[2] = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstructions[3] = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.RemoveAndDeleteAll();
			var invoiceLines = new JobComInvoiceLine[4];
			invoiceLines[0] = invoice.InvoiceLines.AddNew();
			invoiceLines[1] = invoice.InvoiceLines.AddNew();
			invoiceLines[2] = invoice.InvoiceLines.AddNew();
			invoiceLines[3] = invoice.InvoiceLines.AddNew();
			invoiceLines[0].JI_CEI = entryInstructions[0].PK;
			invoiceLines[1].JI_CEI = entryInstructions[1].PK;
			invoiceLines[2].JI_CEI = entryInstructions[2].PK;
			invoiceLines[3].JI_CEI = entryInstructions[3].PK;
			var configuredAssessmentDate = new ZDateTime(2017, 01, 01);
			entryInstructions[1].CEI_DateForDuty = configuredAssessmentDate;
			entryInstructions[2].CEI_DateForDuty = ZDateTime.Empty;
			entryInstructions[3].CEI_DateForDuty = configuredAssessmentDate;
			invoiceLines[0].JI_Tariff = "00876543";
			invoiceLines[1].JI_Tariff = "00876543";
			invoiceLines[2].JI_Tariff = "00876544";
			invoiceLines[3].JI_Tariff = "00876544";
			using (var testForm = new JobDeclarationForm(declaration))
			{
				CombineAssertions(() =>
				{
					testForm.Show();
					var brokerageUserControl = testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					var grid = invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
					for (var i = 0; i < 4; i++)
					{
						grid.CurrentRowIndex = i;
						var info = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Tariff) as Universal.GUI.TariffColumnStyleInfo;
						Universal.GUI.TariffFindBoxTreeViewForm tariffSearchForm;
						using (var style = new Universal.GUI.TariffColumnStyle(info))
						{
							style.SetParentGrid(grid);
							style.EditControl.Parent = grid;
							((Universal.GUI.TariffGridFindBox)style.EditControl).SelectFromPopupForm();
							tariffSearchForm = ZFormModaliser.LastFormShownForTest as Universal.GUI.TariffFindBoxTreeViewForm;
							AssertNotNull("Pre-Req", tariffSearchForm);
							if (i == 1 || i == 3)
							{
								AssertEquals("Test the effective date of grid find box (when assessment date is defined) + " + i, configuredAssessmentDate, tariffSearchForm.BusinessEntity.EffectiveDate);
							}
							else if (i == 0 || i == 2)
							{
								AssertEquals("Test the effective date of grid find box (no assessment date, assumes today) + " + i, ZDateTime.Now, tariffSearchForm.BusinessEntity.EffectiveDate);
							}

							tariffSearchForm.Close();
						}

						var tariffFinBox = invoiceLinesUserControl.Controls.Find("TariffFindBox", true)[0] as Universal.GUI.TariffFindBox;
						AssertEquals("TariffFindBox.TariffType", "1P1", tariffFinBox.TariffType);
						AssertEquals("TariffFindBox.GetCountryCode", "ZA", tariffFinBox.GetCountryCode());
						AssertEquals("TariffFindBox.GetDataGrouping", "ZA", tariffFinBox.GetDataGrouping());
						tariffFinBox.PopupButton.PerformClick();
						tariffSearchForm = ZFormModaliser.LastFormShownForTest as Universal.GUI.TariffFindBoxTreeViewForm;
						AssertNotNull("Pre-Req", tariffSearchForm);
						if (i == 1 || i == 3)
						{
							AssertEquals("Test the effective date of grid find box (when assessment date is defined) + " + i, configuredAssessmentDate, tariffSearchForm.BusinessEntity.EffectiveDate);
						}
						else if (i == 0 || i == 2)
						{
							AssertEquals("Test the effective date of grid find box (no assessment date, assumes today) + " + i, ZDateTime.Now, tariffSearchForm.BusinessEntity.EffectiveDate);
						}

						tariffSearchForm.Close();
					}
				});
			}
		}

		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			using (var testForm = new JobDeclarationForm(declaration))
			{
				CombineAssertions(() =>
				{
					testForm.Show();
					var brokerageUserControl = testForm.CustomsBrokerageUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					//Exports
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(false, invoiceLinesUserControl.PreferenceDropEdit.Visible);
					AssertEquals(false, invoiceLinesUserControl.TradeAgreementLabel.Visible);
					AssertEquals(false, invoiceLinesUserControl.JI_ValuationMarkupCalcEdit.Visible);
					AssertEquals(false, invoiceLinesUserControl.TaxOrFeeDropEdit.Visible);
					AssertEquals(false, invoiceLinesUserControl.JI_Calc_ATVConvertToLocalCurrencyControl1.Visible);
					AssertEquals(false, invoiceLinesUserControl.GoodsTypeDropEdit.Visible);
					AssertEquals(true, invoiceLinesUserControl.ROOTypeDropEdit.Visible);
					AssertEquals(true, invoiceLinesUserControl.TradeStatisticsCheckBox.Visible);
					AssertEquals(false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					//Imports
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(true, invoiceLinesUserControl.PreferenceDropEdit.Visible);
					AssertEquals(true, invoiceLinesUserControl.TradeAgreementLabel.Visible);
					AssertEquals(true, invoiceLinesUserControl.JI_ValuationMarkupCalcEdit.Visible);
					AssertEquals(true, invoiceLinesUserControl.TaxOrFeeDropEdit.Visible);
					AssertEquals(true, invoiceLinesUserControl.JI_Calc_ATVConvertToLocalCurrencyControl1.Visible);
					AssertEquals(true, invoiceLinesUserControl.GoodsTypeDropEdit.Visible);
					AssertEquals(false, invoiceLinesUserControl.ROOTypeDropEdit.Visible);
					AssertEquals(false, invoiceLinesUserControl.TradeStatisticsCheckBox.Visible);
					AssertEquals(false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					//ImportByExternalBroker
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(false, invoiceLinesUserControl.PreviousMRNTextBox.Visible);
					AssertEquals("WHS MRN Line", invoiceLinesUserControl.ImportBOELineCalcEdit.CaptionResourceString.Caption);
				});
			}

			using (var testForm = new JobDeclarationForm(declaration))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				CombineAssertions("Builtin vs. interfaced for Import", () =>
				{
					testForm.Show();
					var brokerageUserControl = testForm.CustomsBrokerageUserControl;
					var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(false, invoiceLinesUserControl.IsDisposed);
					AssertEquals("BLT - new classification control", true, invoiceLinesUserControl.classificationDetailsUserControl.Visible);
					AssertEquals(true, invoiceLinesUserControl.PreferenceDropEdit.Visible);
					AssertEquals(true, invoiceLinesUserControl.TradeAgreementLabel.Visible);
					AssertEquals("BLT - LevyDA63 Tab Page", false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					AssertEquals("BLT - CO2 Column", true, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CO2Emission).IsUnavailable);
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(false, invoiceLinesUserControl.IsDisposed);
					AssertEquals("ITF - new classification control", true, invoiceLinesUserControl.classificationDetailsUserControl.Visible);
					AssertEquals("ITF - LevyDA63 Tab Page", false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					AssertEquals("ITF - CO2 Column", false, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CO2Emission).IsUnavailable);
				});
			}

			using (var testForm = new JobDeclarationForm(declaration))
			{
				CombineAssertions("Builtin vs. interfaced for Export", () =>
				{
					testForm.Show();
					var brokerageUserControl = testForm.CustomsBrokerageUserControl;
					var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(false, invoiceLinesUserControl.IsDisposed);
					AssertEquals("BLT - new classification control", true, invoiceLinesUserControl.classificationDetailsUserControl.Visible);
					AssertEquals(false, invoiceLinesUserControl.PreferenceDropEdit.Visible);
					AssertEquals(false, invoiceLinesUserControl.TradeAgreementLabel.Visible);
					AssertEquals("BLT - LevyDA63 Tab Page", false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					AssertEquals("BLT - CO2 Column", true, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CO2Emission).IsUnavailable);
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(false, invoiceLinesUserControl.IsDisposed);
					AssertEquals("ITF - new classification control", true, invoiceLinesUserControl.classificationDetailsUserControl.Visible);
					AssertEquals("ITF - LevyDA63 Tab Page", false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					AssertEquals("ITF - CO2 Column", false, invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CO2Emission).IsUnavailable);
				});
			}

			using (var testForm = new JobDeclarationForm(declaration))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
				{
					testForm.Show();
					var brokerageUserControl = testForm.CustomsBrokerageUserControl;
					var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(nameof(invoiceLinesUserControl.InvoiceQuantityCalcDropEdit), expected: false, invoiceLinesUserControl.InvoiceQuantityCalcDropEdit.Visible);
					AssertEquals(nameof(invoiceLinesUserControl.InvoiceQuantityCalcEdit), expected: true, invoiceLinesUserControl.InvoiceQuantityCalcEdit.Visible);
					AssertEquals(nameof(invoiceLinesUserControl.InvoiceQuantityUNE20CodeFindBox), expected: true, invoiceLinesUserControl.InvoiceQuantityUNE20CodeFindBox.Visible);
				}
			}

			using (var testForm = new JobDeclarationForm(declaration))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false))
				{
					testForm.Show();
					var brokerageUserControl = testForm.CustomsBrokerageUserControl;
					var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
					brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
					invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					AssertEquals(nameof(invoiceLinesUserControl.InvoiceQuantityCalcDropEdit), expected: true, invoiceLinesUserControl.InvoiceQuantityCalcDropEdit.Visible);
					AssertEquals(nameof(invoiceLinesUserControl.InvoiceQuantityCalcEdit), expected: false, invoiceLinesUserControl.InvoiceQuantityCalcEdit.Visible);
					AssertEquals(nameof(invoiceLinesUserControl.InvoiceQuantityUNE20CodeFindBox), expected: false, invoiceLinesUserControl.InvoiceQuantityUNE20CodeFindBox.Visible);
				}
			}
		}

		public void TestDA63TabPageVisibility()
		{
			UniversalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			UniversalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "ZZ", "6", "XXZZ6", "IMP,EXP");
			UniversalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "ZZ", "5", "YYZZ5", "IMP,EXP");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var instructionXX = declaration.CustomsEntryInstructions.AddNew();
			instructionXX.CEI_Style = "XX";
			var instructionYY = declaration.CustomsEntryInstructions.AddNew();
			instructionYY.CEI_Style = "YY";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageUserControl = testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
				CombineAssertions(() =>
				{
					AssertEquals("Empty", false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					invoiceLine.JI_CEI = instructionXX.PK;
					invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "XX";
					AssertEquals("XXXX", false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "YY";
					AssertEquals("XXYY", true, invoiceLinesUserControl.MiscTabPage.TabVisible);
					invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "ZZ";
					AssertEquals("XXZZ", false, invoiceLinesUserControl.MiscTabPage.TabVisible);
					invoiceLine.JI_CEI = instructionYY.PK;
					AssertEquals("YYZZ", true, invoiceLinesUserControl.MiscTabPage.TabVisible);
				});
			}
		}

		public void TestInvoiceLineGrid_Columns()
		{
			CombineAssertions("EXP", () =>
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				declaration1.Invoices.AddNew();
				using (var form1 = new JobDeclarationForm(declaration1))
				{
					form1.Show();
					form1.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form1.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var userControl = form1.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					userControl.LineDetailTabControl.Parent.Visible = true;
					var grid1 = userControl.CustomsInvoiceLinesBoundGrid;
					var columns = grid1.Columns;
					var i = 0;
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LineNo, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_Calc_Invoice, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_CEI, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_CEI_Description, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_Procedure, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_PartNo, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Tariff, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Description, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LinePrice, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PreviousEntryNumber, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PreviousEntryLineNumber, columns, i++);
					AssertEquals("JI_NewOwnerPartNo", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartNo).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib1", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib2", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib3", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3).IsUnavailable);
				}
			});
			CombineAssertions("IMP", () =>
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration1.Invoices.AddNew();
				using (var form1 = new JobDeclarationForm(declaration1))
				{
					form1.Show();
					form1.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form1.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var userControl = form1.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					userControl.LineDetailTabControl.Parent.Visible = true;
					var grid1 = userControl.CustomsInvoiceLinesBoundGrid;
					var columns = grid1.Columns;
					var i = 0;
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LineNo, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_Calc_Invoice, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_CEI, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_CEI_Description, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_Procedure, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_PartNo, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Tariff, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Description, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LinePrice, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.TradeAgreementCode, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PreviousEntryNumber, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PreviousEntryLineNumber, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_NewUsed, columns, i++);
					AssertEquals("JI_NewOwnerPartNo", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartNo).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib1", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib2", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib3", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3).IsUnavailable);
				}
			});
			CombineAssertions("EXW", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				declaration.Invoices.AddNew();
				var owner = Factory.New<OrgHeader>();
				owner.MiscServ.OM_IMPartAttrib1IsMandatory = true;
				owner.MiscServ.OM_IMPartAttrib1Name = "VIN";
				owner.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;
				owner.MiscServ.OM_IMPartAttrib2IsMandatory = true;
				owner.MiscServ.OM_IMPartAttrib2Name = "BATCH NUMBER";
				owner.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
				owner.MiscServ.OM_IMPartAttrib3IsMandatory = true;
				owner.MiscServ.OM_IMPartAttrib3Name = "Serial Number";
				owner.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.InvoiceLinesTabPage;
					var userControl = customsBrokerageUserControl.InvoiceLinesUserControl;
					var grid = userControl.CustomsInvoiceLinesBoundGrid;
					var columns = grid.Columns;
					var i = 0;
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LineNo, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_Calc_Invoice, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_CEI, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_CEI_Description, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_Procedure, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_PartNo, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Tariff, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Description, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LinePrice, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.TradeAgreementCode, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PreviousEntryNumber, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PreviousEntryLineNumber, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_NewUsed, columns, i++);
					AssertEquals("JI_NewOwnerPartNo", true, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartNo).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib1", true, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib2", true, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2).IsUnavailable);
					AssertEquals("JI_NewOwnerPartAttrib3", true, grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3).IsUnavailable);
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.DeclarationTabPage;
					var declarationUserControl = (ZADeclarationUserControl)customsBrokerageUserControl.DeclarationUserControl;
					declarationUserControl.RightTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
					entryInstruction.CEI_OH_Owner = owner.PK;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.InvoiceLinesTabPage;
					userControl = customsBrokerageUserControl.InvoiceLinesUserControl;
					grid = userControl.CustomsInvoiceLinesBoundGrid;
					columns = grid.Columns;
					i = 0;
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LineNo, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_Calc_Invoice, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_CEI, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_CEI_Description, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_Procedure, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_PartNo, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Tariff, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_Description, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_LinePrice, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.TradeAgreementCode, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PreviousEntryNumber, columns, i++);
					AssertDefaultColumn(JobComInvoiceLineSchema.Constants.JI_PreviousEntryLineNumber, columns, i++);
					AssertDefaultColumn(JobComInvoiceLine.Schema.JI_NewUsed, columns, i++);
					AssertColumnStyle(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartNo), "Owner Product", false);
					AssertColumnStyle(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1), "Owner VIN", false);
					AssertColumnStyle(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2), "Owner BATCH NUMBER", false);
					AssertColumnStyle(grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3), "Owner Serial Number", false);
				}
			});
			CombineAssertions("IMX", () =>
			{
				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
				declaration1.Invoices.AddNew();
				using (var form1 = new JobDeclarationForm(declaration1))
				{
					form1.Show();
					form1.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form1.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var userControl = form1.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					userControl.LineDetailTabControl.Parent.Visible = true;
					var grid1 = userControl.CustomsInvoiceLinesBoundGrid;
					var columns = grid1.Columns;
					AssertEquals("JI_PreviousEntryNumber", true, grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_PreviousEntryNumber).IsUnavailable);
					AssertEquals("WHS MRN Line", grid1.GetColumnStyle(JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber).Caption);
				}
			});
		}

		void AssertColumnStyle(Core.Forms.ZGridColumnInfo columnStyle, string caption, bool isUnavailable)
		{
			AssertEquals(columnStyle.ColumnName + ".Caption", caption, columnStyle.Caption);
			AssertEquals(columnStyle.ColumnName + ".IsUnavailable", isUnavailable, columnStyle.IsUnavailable);
		}

		public void TestInvoiceLineGrid_Columns_CustomsProcedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.Parent.Visible = true;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				var column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ZZF_NKTaxType);
				Assert(column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationMarkup);
				Assert(column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsValueOverride);
				Assert(column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride);
				Assert(column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewUsed);
				Assert(column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_TakeUpInTradeStatistics);
				Assert(!column.IsUnavailable);
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.Parent.Visible = true;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				var column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI);
				Assert("IMP", !column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ZZF_NKTaxType);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationMarkup);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsValueOverride);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewUsed);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_TakeUpInTradeStatistics);
				Assert(column.IsUnavailable);
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.Parent.Visible = true;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				var column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI);
				Assert("EXB", !column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ZZF_NKTaxType);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationMarkup);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsValueOverride);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewUsed);
				Assert(!column.IsUnavailable);
				column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_TakeUpInTradeStatistics);
				Assert(column.IsUnavailable);
			}
		}

		public void TestInvoiceLineGrid_Columns_JI_PrimaryPreference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.Parent.Visible = true;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				var jiColumn = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PrimaryPreference);
				Assert("IMP", !jiColumn.IsUnavailable);
			}

			using (var form = new JobDeclarationForm(declaration))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.Parent.Visible = true;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				var jiColumn = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PrimaryPreference);
				Assert("EXP", !jiColumn.IsUnavailable);
			}
		}

		public void TestPermitCodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "070610";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
				var findBox = userControl.PermitNumberCodeFindBox;
				findBox.PopupButton.PerformClick();
				AssertEquals(Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Permits, findBox.ModuleID);
				var modulePopup = ZFormModaliser.LastFormShownForTest;
				var module = ((ZArchitecture.GUI.Internal.EmbeddedModulePopup)modulePopup).Module_ForTest;
				var filters = module.FilterBusinessObject.AlwaysVisibleModuleFilters.OfType<ZArchitecture.Business.ModuleFilter>();
				AssertEquals("Default Filters", 7, filters.Count());
				AssertNotNull("Country Filter", filters.Any(x => x.Code == Customs.Business.CusPermitHeaderCollection.FilterConstants.Country));
				AssertNotNull("PermitHolder Filter", filters.Any(x => x.Code == Customs.Business.CusPermitHeaderCollection.FilterConstants.PermitHolder));
			}
		}

		public void TestDiamondProcessingTabVisible()
		{
			var tariffType1P1 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			//1P1 - Diamond Processing Required
			var tariff1 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991001", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var tariffAttribute = UniversalReferenceDataHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.Diamond, "X", tariff1);
			//1P1 - Diamond Processing Not Required
			var tariff2 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991002", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
				AssertEquals(false, userControl.DiamondProcessingTabPage.TabVisible);
				invoiceLine.JI_Tariff = "991001";
				AssertEquals(true, userControl.DiamondProcessingTabPage.TabVisible);
				invoiceLine.JI_Tariff = "991002";
				AssertEquals(false, userControl.DiamondProcessingTabPage.TabVisible);
			}
		}

		public void TestFieldChangeTriggerDA63()
		{
			UniversalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			UniversalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "ZZ", "5", "XXYY5", "IMP,EXP");
			UniversalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "YY", "5", "XXYY5", "IMP,EXP");
			UniversalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "ZZ", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var inst1 = declaration.CustomsEntryInstructions.AddNew();
			inst1.CEI_Style = "XX";
			var inst2 = declaration.CustomsEntryInstructions.AddNew();
			inst2.CEI_Style = "YY";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CEI = inst1.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
			using (var testForm = new JobDeclarationForm(declaration))
			{
				var notificationInstanse = UnitTestUserNotification.Instance;
				notificationInstanse.ClearMessagesAndAnswers();
				testForm.Show();
				var brokerageUserControl = testForm.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
				invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "ZZ";
				AssertEquals(null, notificationInstanse.LastMessage.Text);
				invoiceLine.JI_CEI = inst2.PK;
				AssertEquals(null, notificationInstanse.LastMessage.Text);
				invoiceLine.JI_CustomsQuantity = 10;
				AssertEquals(null, notificationInstanse.LastMessage.Text);
				invoiceLine.JI_CustomsUnitQty = "LI";
				AssertEquals(null, notificationInstanse.LastMessage.Text);
				Factory.Save();
				notificationInstanse.ClearMessagesAndAnswers();
				CombineAssertions(() =>
				{
					var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
					invoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLinesUserControl.MiscTabPage;
					AssertEquals("1", DA63UserControl.ReCalculationConfirmation, notificationInstanse.LastMessage.Text);
					notificationInstanse.ClearMessagesAndAnswers();
					invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "ZZ";
					AssertEquals("2", DA63UserControl.ReCalculationConfirmation, notificationInstanse.LastMessage.Text);
					notificationInstanse.ClearMessagesAndAnswers();
					invoiceLine.JI_CEI = inst1.PK;
					AssertEquals("3", DA63UserControl.ReCalculationConfirmation, notificationInstanse.LastMessage.Text);
					notificationInstanse.ClearMessagesAndAnswers();
					invoiceLine.JI_CustomsQuantity = 11;
					AssertEquals("4", DA63UserControl.ReCalculationConfirmation, notificationInstanse.LastMessage.Text);
					notificationInstanse.ClearMessagesAndAnswers();
					invoiceLine.JI_CustomsUnitQty = "KG";
					AssertEquals("5", DA63UserControl.ReCalculationConfirmation, notificationInstanse.LastMessage.Text);
				});
			}
		}

		public void TestCustomsQuantityFieldsAndColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageUserControl = form.CustomsBrokerageUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
				var linesUserControl = (InvoiceLinesUserControl)brokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(2, linesUserControl.FindSingle<ZCalcDropEdit>("CustomsQuantityCalcDropEdit").Decimals);
				var detailsUserControl = linesUserControl.classificationDetailsUserControl;
				AssertEquals(2, detailsUserControl.FindSingle<ZCalcDropEdit>("FirstAddUnitCalcDropEdit").Decimals);
				AssertEquals(2, detailsUserControl.FindSingle<ZCalcDropEdit>("SecondAddUnitCalcDropEdit").Decimals);
				AssertEquals(0, detailsUserControl.FindSingle<ZCalcDropEdit>("ThirdAddUnitCalcDropEdit").Decimals);
				var linesGrid = linesUserControl.CustomsInvoiceLinesBoundGrid;
				var customsQuantityColumn = (ZCalcEditColumnStyleInfo)linesGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_CustomsQuantity.Name);
				AssertEquals(2, customsQuantityColumn.Decimals);
				var customsSecondQuantityColumn = (ZCalcEditColumnStyleInfo)linesGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_CustomsSecondQuantity.Name);
				AssertEquals(2, customsSecondQuantityColumn.Decimals);
				var customsThirdQuantityColumn = (ZCalcEditColumnStyleInfo)linesGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_CustomsThirdQuantity.Name);
				AssertEquals(2, customsThirdQuantityColumn.Decimals);
				var bondedWhsQuantityColumn = (ZCalcEditColumnStyleInfo)linesGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_BondedWhsQuantity.Name);
				AssertEquals(0, bondedWhsQuantityColumn.Decimals);
			}
		}

		public void TestInvoiceQuantityCalcEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			_ = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var brokerageUserControl = form.CustomsBrokerageUserControl;
				var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
				invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
				var calcEdit = invoiceLinesUserControl.InvoiceQuantityCalcEdit;
				CombineAssertions(() =>
				{
					AssertType<ZCalcEdit>("Type", calcEdit);
					Assert(calcEdit.Visible);
					_ = calcEdit.AssertThisControl(x => x.WithBindTo("FilteredInvoiceLines.JI_InvoiceQuantity"));
				});
			}
		}

		public void TestInvoiceQuantityUNE20CodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			_ = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				using var form = new JobDeclarationForm(declaration);
				form.Show();
				var brokerageUserControl = form.CustomsBrokerageUserControl;
				var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.DeclarationTabPage;
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
				invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as InvoiceLinesUserControl;
				var findBox = invoiceLinesUserControl.InvoiceQuantityUNE20CodeFindBox;
				CombineAssertions(() =>
				{
					AssertType<ZCodeFindBox>("Type", findBox);
					Assert(findBox.Visible);
					_ = findBox.AssertThisControl(x => x.WithBindTo("FilteredInvoiceLines.JI_InvoiceUQ"));
					AssertEquals(Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.ZZRefCusCodeList, findBox.ModuleID);

					findBox.PopupButton.PerformClick();
					var modulePopup = ZFormModaliser.LastFormShownForTest;
					var module = ((ZArchitecture.GUI.Internal.EmbeddedModulePopup)modulePopup).Module_ForTest;
					var filters = module.FilterBusinessObject.AlwaysVisibleModuleFilters.OfType<ZArchitecture.Business.ModuleFilter>();
					AssertEquals("Default Filters", 4, filters.Count());
					AssertNotNull("List Type", filters.Any(x => x.Code == Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations));
					AssertNotNull("Country/Region or Grouping Filter", filters.Any(x => x.Code == "INVUQ"));
				});
			}
		}

		public void TestInvoiceLineGrid_Columns_JI_InvoiceUQ()
		{
			AssertInvoiceLineGrid_Columns_JI_InvoiceUQ<ZCodeFindBoxColumnStyleInfo>(useAddInvoiceDetailsToCUSDECMessage: true);
			AssertInvoiceLineGrid_Columns_JI_InvoiceUQ<ZDropEditColumnStyleInfo>(useAddInvoiceDetailsToCUSDECMessage: false);
		}

		public void AssertInvoiceLineGrid_Columns_JI_InvoiceUQ<T>(bool useAddInvoiceDetailsToCUSDECMessage)
		{
			var declaration = Factory.New<JobDeclaration>();
			_ = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using var form = new JobDeclarationForm(declaration);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: useAddInvoiceDetailsToCUSDECMessage))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.LineDetailTabControl.Parent.Visible = true;
				var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
				var column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_InvoiceUQ);
				AssertType<T>("Type", column);
				AssertEquals("Visible", expected: true, column.IsVisible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}

		void AssertDefaultColumn(string expectedName, ZGridColumns columns, int index)
		{
			var column = columns[index];
			AssertEquals(index.ToString(), expectedName, column.ColumnStyle.MappingName);
			AssertEquals(expectedName, true, column.IsVisible);
		}

		ZAUniversalReferenceTestDataHelper UniversalReferenceDataHelper
		{
			get
			{
				if (universalReferenceDataHelper == null)
				{
					universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
				}

				return universalReferenceDataHelper;
			}
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
		protected override ZString DefaultUniversalTariffType => UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
	}
}
