using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl, ISupportMultipleResourceStringDataSupporter
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);

			if (!DesignModeFinder.IsDesigning)
			{
				ImportDetailsUserControl.JI_TariffFindBox.GetCountryCode = () => Core.Constants.CountryCodes.Taiwan;
				ImportDetailsUserControl.JI_TariffFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.Taiwan;
				ImportDetailsUserControl.JI_TariffFindBox.GetTariffType = GetUniversalTariffType;
				ImportDetailsUserControl.JI_TariffFindBox.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;
			}

			PendingApportionmentLabel.AllowOverlap(CurrentInvoiceQuantityGroupPanel);
		}

		public ISupportMultipleResourceStringData SupportMultipleResourceStringData => base.CurrentInvoiceLine;

		protected override void ResetCustomsInvoiceLinesBoundGridColumns()
		{
			base.ResetCustomsInvoiceLinesBoundGridColumns();

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_UseOneTenthCV,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.AlcoholTaxCashTariffCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.AlcoholTaxNonCashTariffCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.CommodityTaxCashTariffCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.CommodityTaxNonCashTariffCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.SpecialTaxCashTariffCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.SpecialTaxNonCashTariffCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.TobaccoTaxCashTariffCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.TobaccoTaxNonCashTariffCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_Calc_RAPRORUnitPrice,
				GroupName = Res.GetData("6B9CC1C4-A5C0-46D0-B613-61C12515ED21", "RAP/ROR Unit Price"),
				CaptionResourceString = Res.GetData("6B9CC1C4-A5C0-46D0-B613-61C12515ED21", "RAP/ROR Unit Price"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_RAPCurr,
				ModuleID = ZArchitecture.Modules.ModuleIDs.RefCurrency,
				GroupName = Res.GetData("6B9CC1C4-A5C0-46D0-B613-61C12515ED21", "RAP/ROR Unit Price"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_EPTDigit1,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_EPTDigit2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_EPTDigit3,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_AntiDumpingDutyRate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_CountervailingDutyRate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_AdditionalDutyRate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_RetaliatoryDutyRate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
			});

			#region Quota

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_ConcessionOrder,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			#endregion

			#region Tariff Rate Quota Certificate

			var groupName = Res.GetData("19F22E3E-3596-4405-9C90-6806662FE3A9", "Tariff Rate Quota Certificate");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.QuotaPermitNumber,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("24707075-C7BC-438C-8FD4-6CF93CC9E717", "Quota Cert LNO", "Tariff Rate Quota Cert LNO", "Tariff Rate Quota Certificate Line Number", ""),
				ColumnName = JobComInvoiceLine.Schema.QuotaPermitNumberItemNumber,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
				BindToDecimalPlaces = null,
				ShowEmptyStringForEmptyValue = true,
				Decimals = 0
			});

			#endregion

			#region CAA Code

			groupName = Res.GetData("9C00CE22-BD41-4682-A62A-74859817B884", "CAA Code");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("7E1B968A-0C9C-45C5-89B5-7DE293A2171A", "CAA Category"),
				ColumnName = JobComInvoiceLine.Schema.TWL_AircraftPartsCategory,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("0D1065F2-44A4-48F0-9F72-85627A3713B0", "CAA Sequence"),
				ColumnName = JobComInvoiceLine.Schema.TWL_AircraftPartsCode,
				GroupName = groupName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			#endregion

			#region AircraftIPC

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZMultiLineTextBoxColumnInfo
			{
				CaptionResourceString = Res.GetData("E87AF22D-6C41-4E19-BFB7-2D29CB7888F5", "IPC", "A/C", "Aircraft IPC", ""),
				ColumnName = JobComInvoiceLine.Schema.TWL_AircraftIPC,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			#endregion

			#region Foreign Manufacturer

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("E17485C6-1A96-40F6-88FF-768467F10895", "Foreign Manuf.", "Foreign Manufacturer", ""),
				ColumnName = JobComInvoiceLine.Schema.ManufacturerDocAddressOrgPK,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				BindToList = "Lookups.ManufacturerList"
			});

			#endregion
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			CarInfoPanel.Visible = true;
		}

		protected override void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.HookInvoiceLineEvents(invoiceLine);

			if (invoiceLine is JobComInvoiceLine line)
			{
				ImportDetailsUserControl.SetControlsVisibilityForRAPROR(CurrentInvoiceLine);
				line.JI_ProcedureInfo.ValueChanged -= JI_ProcedureInfo_ValueChanged;
				line.JI_ProcedureInfo.ValueChanged += JI_ProcedureInfo_ValueChanged;
				var jobTWComInvoiceLine = line.AddInfoChild;
				jobTWComInvoiceLine.TWL_AircraftPartsCodeInfo.ValueChanged -= TWL_AircraftPartsCodeInfo_ValueChanged;
				jobTWComInvoiceLine.TWL_AircraftPartsCodeInfo.ValueChanged += TWL_AircraftPartsCodeInfo_ValueChanged;
			}
		}

		void TWL_AircraftPartsCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			var invoiceLine = CurrentInvoiceLine;
			var jobTWComInvoiceLine = invoiceLine.AddInfoChild;
			var aircraftPartsCode = jobTWComInvoiceLine.TWL_AircraftPartsCode;
			if (!aircraftPartsCode.IsEmpty)
			{
				var aircraftPartsDescription = jobTWComInvoiceLine.Lookups.CAAAircraftPartsCodesList.GetDescriptionFromCode(aircraftPartsCode);
				if (invoiceLine.JI_Description.IsEmpty)
				{
					invoiceLine.JI_Description = aircraftPartsDescription;
				}
				else
				{
					var result = Globals.Message.Show(Res.GetString("41eda7dd-7f88-4234-a185-b427bf15d97c", "Aircraft Parts English description must be declared. Do you want to override the existing English description with the Aircraft Parts English description?"), Res.GetString("6e58e0c7-c105-4804-bc3f-ede109d791cb", "Override Description Request"), MessageBoxButtons.YesNo, DialogResult.No);
					if (result == DialogResult.Yes)
					{
						invoiceLine.JI_Description = aircraftPartsDescription;
					}
				}
			}
		}

		protected override void UnHookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			base.UnHookInvoiceLineEvents(invoiceLine);

			if (invoiceLine is JobComInvoiceLine line)
			{
				line.JI_ProcedureInfo.ValueChanged -= JI_ProcedureInfo_ValueChanged;
				line.AddInfoChild.TWL_AircraftPartsCodeInfo.ValueChanged -= TWL_AircraftPartsCodeInfo_ValueChanged;
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				ImportDetailsUserControl.SetControlsVisibilityForRAPROR(CurrentInvoiceLine);
			}
		}

		void JI_ProcedureInfo_ValueChanged(object sender, EventArgs e)
		{
			ImportDetailsUserControl.SetControlsVisibilityForRAPROR(CurrentInvoiceLine);
		}

		protected override List<string> GetDefaultColumnsForGridCore()
		{
			var columns = new List<string>();
			columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
			columns.Add(JobComInvoiceLine.Schema.InvoiceHeaderDisplaySequence);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_LineNo);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_PartNo);
			columns.Add(JobComInvoiceLine.Schema.JI_FormattedTariff);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_Procedure);
			columns.Add(JobComInvoiceLine.Schema.JI_Group);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_Description);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ);
			columns.Add(JobComInvoiceLine.Schema.JI_EnteredUnitPrice);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_LinePrice);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeight);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty);
			columns.Add(JobComInvoiceLineSchema.Constants.JI_BrandName);
			columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryQty);
			columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryUQ);
			columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryUnitPrice);

			return columns;
		}

		protected override LineDetailsUserControl GetLineDetailsUserControl() => new ImportLineDetailsUserControl();
	}
}
