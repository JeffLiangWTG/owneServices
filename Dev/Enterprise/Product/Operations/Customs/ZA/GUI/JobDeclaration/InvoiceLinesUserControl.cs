using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class InvoiceLinesUserControl : DeclarationInvoiceLineUserControl
	{
		public InvoiceLinesUserControl()
		{
			InitializeComponent();

			tariffFindBox.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;
			AddColumnsToGrid();

			InvoiceLineUserControlHelper.SetTariffRelated(CustomsInvoiceLinesBoundGrid, Name, tariffFindBox, GetCustomsCountryCode, GetDataGroupingForUniversalTariff, GetUniversalTariffType());

			LineDetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			LineDetailTabControl.SelectedIndexChanged += DA63UserControl_OnShown;

			InvoiceDetailsGroupBox.Controls.Add(tariffFindBox);

#if DEBUG
			TypeDescriptor.AddAttributes(PreferenceDropEdit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(TaxOrFeeDropEdit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(CustomsValueOverrideCalcDropEdit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(InvoiceQuantityCalcDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.SouthAfrica;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.SouthAfrica;
		protected override ZString UniversalTariffType => Business.UniversalReferenceConstants.CusTariffCode.Schedule1Part1;

		public new JobDeclaration CurrentDataItem
		{
			get { return (JobDeclaration)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				JE_MessageTypeInfo_ValueChanged(null, EventArgs.Empty);
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = CurrentDataItem;
			if (declaration != null)
			{
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(Customs.Business.AutoJobComInvoiceLine.Schema.JI_PreviousEntryLineNumber, declaration.IsImportByExternalBroker ? Res.GetString("D148BAAC-3B27-40B0-801A-8F8B6C343867", "WHS MRN Line")
					: Res.GetString("AB4F63BE-8A28-4734-AE77-1808E599ABB1", "Previous MRN Line"));
				CustomsInvoiceLinesBoundGrid.SetAvailability(!declaration.IsImportByExternalBroker, AutoJobComInvoiceLine.Schema.JI_PreviousEntryNumber);
				ImportBOELineCalcEdit.CaptionResourceString = declaration.IsImportByExternalBroker
					? Enterprise.Customs.ZA.GUI.Res.GetData("DA5962EF-993F-4996-A4E7-B971DFDA7969", "WHS MRN Line")
					: Enterprise.Customs.ZA.GUI.Res.GetData("6F509FB8-C3B6-4F4B-9832-AADB7D855914", "Previous MRN Line");
				PreviousMRNTextBox.Visible = !declaration.IsImportByExternalBroker;
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, defaultColumnsForGrid);
			}

			var customsQty = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity);
			if (customsQty != null)
			{
				((ZArchitecture.ZCalcEditColumnStyleInfo)customsQty).Decimals = 2;
			}
		}

		protected void AddColumnsToGrid()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new[]
				{
				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					ColumnName = "JI_CustomsSecondQuantity",
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|B6C4D756-89AA-4D18-AC9F-FD4DC951B8B2", "Additional Quantity 1"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = "JI_CustomsSecondUnitQty",
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|B6C4D756-89AA-4D18-AC9F-FD4DC951B8B2", "Additional Quantity 1"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					ColumnName = "JI_CustomsThirdQuantity",
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|D022909E-31AF-41C6-88C9-ED83EB064A5C", "Additional Quantity 2"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = "JI_CustomsThirdUnitQty",
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|D022909E-31AF-41C6-88C9-ED83EB064A5C", "Additional Quantity 2"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					ColumnName = "JI_BondedWhsQuantity",
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|1F9168F0-0576-47BA-BDF4-47C12F48D9FA", "Countable Quantity"),
					Decimals = 0,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = "JI_BondedWhsUnitQty",
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|1F9168F0-0576-47BA-BDF4-47C12F48D9FA", "Countable Quantity"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					BindToDecimalPlaces = null,
					ColumnName = JobComInvoiceLine.Schema.JI_CO2Emission,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					BindToDecimalPlaces = null,
					ColumnName = JobComInvoiceLine.Schema.JI_EngineCapacity,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_Colour,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_CommissionNumber,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = nameof(JobComInvoiceLine.JI_Model),
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_Make,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDropEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_YearOfManufacture,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_VIN,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_EngineNumber,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_VehicleType,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_VehicleFormat,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_NewUsed,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
				},

				new ZGuidDropEditColumnStyleInfo()
				{
					ColumnName = "JI_CEI",
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|D9A6DD9D-7871-4B6A-A3E1-D616B369112B", "Customs Procedure"),
					ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					ColumnName = "JI_CEI_Description",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},

				new ZCodeFindBoxColumnStyleInfo()
				{
					ColumnName = "JI_Procedure",
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|D9A6DD9D-7871-4B6A-A3E1-D616B369112B", "Customs Procedure"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.ZZRefCusProcedure,
					BindToList = "Lookups+CPCList",
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					ColumnName = "JI_PreviousEntryNumber",
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					BindToDecimalPlaces = null,
					ColumnName = "JI_PreviousEntryLineNumber",
					Decimals = 0,
					IsVisible = false,
					MaxLengthOverride = 4,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},

				new ZArchitecture.ZCheckBoxColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_TakeUpInTradeStatistics,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_ROOCert,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = "JI_ZZF_NKTaxType",
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = "JI_PrimaryPreference",
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					BindToDecimalPlaces = null,
					ColumnName = nameof(JobComInvoiceLine.JI_ValuationMarkup),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					BindToDecimalPlaces = null,
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsValueOverride,
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|225C27D8-93DE-4074-84CE-9F2C580AB765", "Customs Value Override"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZDropEditColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride,
					GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|225C27D8-93DE-4074-84CE-9F2C580AB765", "Customs Value Override"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					BindToDecimalPlaces = null,
					ColumnName = JobComInvoiceLine.Schema.JI_TargetEntryLineNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					BindToDecimalPlaces = null,
					ColumnName = "LinkedEntryLineNumber",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},

				new ZCodeFindBoxColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_PermitNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Permits,
				},

				new ZCodeFindBoxColumnStyleInfo()
				{
					ColumnName = "RefundRebateCode",
					ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.RefCusTariff,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					ColumnName = "RefundRebateValue",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					ColumnName = "TradeAgreementCode",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_AdvancePaymentNo,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121),
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_BrandName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121),
				},
			});
		}

		protected override void OnLoad(EventArgs e)
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				base.OnLoad(e);
			}
		}

		#region Invoice Line Grid Columns

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					List<string> columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);

					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsThirdQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsThirdUnitQty);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_BondedWhsQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_BondedWhsUnitQty);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Weight);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_WeightUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeight);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ);
					columns.Add(JobComInvoiceLine.Schema.JI_ROOCert);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_ValuationMarkup);
					columns.Add(JobComInvoiceLine.Schema.JI_CustomsValueOverride);
					columns.Add(JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride);
					columns.Add(JobComInvoiceLine.Schema.JI_VIN);
					columns.Add(JobComInvoiceLine.Schema.JI_CommissionNumber);
					columns.Add(JobComInvoiceLine.Schema.JI_Colour);
					columns.Add(JobComInvoiceLine.Schema.JI_EngineNumber);
					columns.Add(JobComInvoiceLine.Schema.JI_EngineCapacity);
					columns.Add(JobComInvoiceLine.Schema.JI_CO2Emission);
					columns.Add(JobComInvoiceLine.Schema.JI_VehicleType);
					columns.Add(JobComInvoiceLine.Schema.JI_VehicleFormat);
					columns.Add(JobComInvoiceLine.Schema.JI_YearOfManufacture);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Model);
					columns.Add(JobComInvoiceLine.Schema.JI_Make);
					columns.Add(JobComInvoiceLine.Schema.JI_TargetEntryLineNumber);
					columns.Add(JobComInvoiceLine.Schema.JI_TakeUpInTradeStatistics);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CC);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_OrderNumber);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.UnitPrice);
					columns.Add(JobComInvoiceLine.Schema.JI_ContainerMode);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Volume);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_VolumeUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib2);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib3);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_SerialNumber);
					columns.Add(JobComInvoiceLine.Schema.JI_NewOwnerPartNo);
					columns.Add(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1);
					columns.Add(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2);
					columns.Add(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3);
					columns.Add(JobComInvoiceLine.Schema.JI_NewOwnerSerialNum);
					columns.Add(JobComInvoiceLine.Schema.JI_BrandName);

					columnNamesInSortOrder = columns.ToArray();
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;

		string[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					List<string> columns = new List<string>();
					columns.Add(JobComInvoiceLineSchema.Constants.JI_LineNo);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CEI);
					columns.Add(JobComInvoiceLine.Schema.JI_CEI_Description);
					columns.Add(JobComInvoiceLine.Schema.JI_Procedure);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartNo);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Tariff);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Description);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_LinePrice);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference);
					columns.Add(JobComInvoiceLine.Schema.TradeAgreementCode);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PreviousEntryNumber);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PreviousEntryLineNumber);
					columns.Add(JobComInvoiceLine.Schema.JI_NewUsed);
					columns.Add(JobComInvoiceLine.Schema.JI_AdvancePaymentNo);

					defaultColumnsForGrid = columns.ToArray();
				}

				return defaultColumnsForGrid;
			}
		}
		string[] defaultColumnsForGrid;

		#endregion

		#region Controls

		protected internal ConvertToLocalCurrencyControl JI_Calc_ATVConvertToLocalCurrencyControl1;
		protected internal ConvertToLocalCurrencyControl JI_Calc_ActualPriceCurrencyControl;

		protected internal ZTabPage MiscTabPage;
		protected internal ZGroupBox ImportBOEGroupBox;
		protected internal ClassificationDetailsUserControl classificationDetailsUserControl;
		protected internal ZDropEdit GoodsTypeDropEdit;
		protected internal ZTabPage DiamondProcessingTabPage;
		ZGroupBox diamondProcessingGroupBox;
		ZArchitecture.ZCalcEdit diamondLevyValueCalcEdit;
		ZArchitecture.ZTextBox diamondProducerRegistrationZTextBox;
		ZArchitecture.ZTextBox diamondProducerExemptionZTextBox;
		ZArchitecture.ZTextBox electionsExemptionsLevyZTextBox;
		ZArchitecture.ZTextBox kimberleyCertificateZTextBox;
		ZArchitecture.ZTextBox temporaryBuyersPermitZTextBox;
		ZArchitecture.ZTextBox temporaryExportExemptionZTextBox;
		ZArchitecture.ZTextBox diamondDealerLicenseZTextBox;
		ZArchitecture.ZTextBox diamondBeneficiaryLicenseZTextBox;
		protected internal ZCodeFindBox ProductCodeFindBox;
		ZCalcDropEdit jI_CustomsQuantityCalcDropEdit;
		protected internal ZCodeFindBox JI_ProcedureFindBox;
		protected internal ZGuidDropEdit JI_CEIGuidDropEdit;
		protected internal ZCodeFindBox PermitNumberCodeFindBox;
		protected internal ZArchitecture.ZCalcEdit JI_ValuationMarkupCalcEdit;
		protected internal ZDropEdit ROOTypeDropEdit;
		protected internal ZDropEdit PreferenceDropEdit;
		protected internal ZDropEdit TaxOrFeeDropEdit;
		protected internal ZArchitecture.ZTextBox RulesOfOriginCertificateTextBox;
		protected internal ZCheckBox TradeStatisticsCheckBox;
		protected internal ZArchitecture.ZLabel TradeAgreementLabel;
		protected internal ZCalcDropEdit CustomsValueOverrideCalcDropEdit;
		protected internal ZArchitecture.ZCalcEdit ImportBOELineCalcEdit;
		protected internal DA63UserControl DA63UserControl;
		protected internal ZArchitecture.ZTextBox PreviousMRNTextBox;
		Universal.GUI.TariffFindBox tariffFindBox;
		protected internal ZArchitecture.ZCalcEdit InvoiceQuantityCalcEdit;
		protected internal ZCodeFindBox InvoiceQuantityUNE20CodeFindBox;
		protected internal ZArchitecture.ZTextBox JI_BrandNameTextBox;

		#endregion

		#region Extract From GeneralCountryInvoiceLineUserControl

		protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.HookInvoiceLineEvents(baseInvoiceLine);

			if (baseInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.JI_TariffInfo.ValueChanged += JI_Tariff_ValueChanged;
				invoiceLine.JI_CEIInfo.ValueChanged += JI_Procedure_ValueChanged;
				invoiceLine.JI_ProcedureInfo.ValueChanged += JI_Procedure_ValueChanged;
				invoiceLine.DA63NeedsRecalculationValueChanged += DA63UserControl_OnShown;
			}
			JI_Tariff_ValueChanged(null, null);
			JI_Procedure_ValueChanged(null, null);
			DA63UserControl_OnShown(null, null);
		}

		protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.UnHookInvoiceLineEvents(baseInvoiceLine);

			if (baseInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.JI_TariffInfo.ValueChanged -= JI_Tariff_ValueChanged;
				invoiceLine.JI_CEIInfo.ValueChanged -= JI_Procedure_ValueChanged;
				invoiceLine.JI_ProcedureInfo.ValueChanged -= JI_Procedure_ValueChanged;
				invoiceLine.DA63NeedsRecalculationValueChanged -= DA63UserControl_OnShown;
			}
		}

		#endregion

		void JI_Tariff_ValueChanged(object sender, EventArgs e)
		{
			using (CustomsInvoiceLinesBoundGrid?.SuspendCancelOfNonEditedRowOnLeaving())
			{
				DiamondProcessingTabPage.TabVisible = CurrentInvoiceLine?.IsDiamondProcessingRequired ?? false;
			}
			#region Workaround for ZDropEdit and ZCodeFindBox CodeBox not updating correctly on RefreshBinding.

			BindingSource.SetBindingMember(tariffFindBox, CargoWise.Types.ZString.Empty);
			BindingSource.SetBindingMember(tariffFindBox, "FilteredInvoiceLines.JI_Tariff");

			#endregion
		}

		void JI_Procedure_ValueChanged(object sender, EventArgs e)
		{
			using (CustomsInvoiceLinesBoundGrid?.SuspendCancelOfNonEditedRowOnLeaving())
			{
				MiscTabPage.TabVisible = CurrentInvoiceLine?.IsDA63 ?? false;
			}
		}

		void DA63UserControl_OnShown(object sender, EventArgs e)
		{
			if (LineDetailTabControl.SelectedTab == MiscTabPage)
			{
				DA63UserControl.ReCalculateDA63ValuesIfNeeded();
			}
		}

		protected new JobComInvoiceLine CurrentInvoiceLine => base.CurrentInvoiceLine as JobComInvoiceLine;

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			ClassificationPanel.Visible = false;

			var isImport = JobDeclaration.IsImport;

			TradeAgreementLabel.Visible = isImport;
			JI_ValuationMarkupCalcEdit.Visible = isImport;
			TaxOrFeeDropEdit.Visible = isImport;
			CustomsValueOverrideCalcDropEdit.Visible = isImport;
			JI_Calc_ATVConvertToLocalCurrencyControl1.Visible = isImport;
			GoodsTypeDropEdit.Visible = isImport;

			TradeStatisticsCheckBox.Visible = !isImport;
			ROOTypeDropEdit.Visible = !isImport;
			PreferenceDropEdit.Visible = isImport;

			var zaAddInvoiceDetailsToCUSDECMessageEnabled = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);

			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, JobComInvoiceLine.Schema.JI_ZZF_NKTaxType);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, JobComInvoiceLine.Schema.JI_ValuationMarkup);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, JobComInvoiceLine.Schema.JI_CustomsValueOverride);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, JobComInvoiceLine.Schema.JI_RX_NKCustomsValueCurrencyOverride);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, JobComInvoiceLine.Schema.JI_NewUsed);
				CustomsInvoiceLinesBoundGrid.SetAvailability(!isImport, JobComInvoiceLine.Schema.JI_TakeUpInTradeStatistics);
				CustomsInvoiceLinesBoundGrid.SetAvailability(JobDeclaration.IsDeclarationIntegrated, JobComInvoiceLine.Schema.JI_CO2Emission);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, JobComInvoiceLine.Schema.TradeAgreementCode);

				var rooTypeColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_PrimaryPreference.Name);
				if (rooTypeColumnStyle != null)
				{
					if (isImport)
					{
						rooTypeColumnStyle.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("FD0F0ABB-B19D-4698-9C10-C4BF01B27B23", "Preference");
					}
					else
					{
						rooTypeColumnStyle.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ec3a51ac-e22e-458c-9262-caf59d4c2777", "ROO Type");
					}
				}
				SetNewOwnerPartVisibility();

				if (zaAddInvoiceDetailsToCUSDECMessageEnabled)
				{
					var invoiceUQColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_InvoiceUQ.Name);
					if (invoiceUQColumnStyle != null)
					{
						CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(invoiceUQColumnStyle);

						var newInvoiceUQColumnStyle = new ZCodeFindBoxColumnStyleInfo()
						{
							ColumnName = JobComInvoiceLineSchema.JI_InvoiceUQ.Name,
							Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71),
							ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.ZZRefCusCodeList,
							BindToList = "Lookups+InvoiceUQUNE20CodeList",
						};

						CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(0, newInvoiceUQColumnStyle);
					}
				}
			}

			InvoiceQuantityCalcDropEdit.Visible = !zaAddInvoiceDetailsToCUSDECMessageEnabled;
			InvoiceQuantityCalcEdit.Visible = zaAddInvoiceDetailsToCUSDECMessageEnabled;
			InvoiceQuantityUNE20CodeFindBox.Visible = zaAddInvoiceDetailsToCUSDECMessageEnabled;
		}

		protected override bool SupportNewOwnerPartDetails => true;
		protected override string NewOwnerPartNoColumnName => JobComInvoiceLine.Schema.JI_NewOwnerPartNo;
		protected override string NewOwnerPartAttrib1ColumnName => JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1;
		protected override string NewOwnerPartAttrib2ColumnName => JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2;
		protected override string NewOwnerPartAttrib3ColumnName => JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3;
		protected override string NewOwnerSerialNumberColumnName => JobComInvoiceLine.Schema.JI_NewOwnerSerialNum;
	}
}

