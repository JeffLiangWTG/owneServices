using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	public partial class DrawbackInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public DrawbackInvoiceLineUserControl()
		{
			InitializeComponent();
			ExportQtyCalcDropEdit.SetReadOnly(true);

			AddAdjClaimFieldToCustomsInvoiceLinesBoundGrid();
		}

		protected override bool UseUniversalTariff
		{
			get { return false; }
		}

		void AddAdjClaimFieldToCustomsInvoiceLinesBoundGrid()
		{
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo_AdjClaimDuty = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo_AdjClaimDuty.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo_AdjClaimDuty.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5eb299fd-be79-4dfb-ba05-02e13ccad456", "Adj Claim Duty");
			zCalcEditColumnStyleInfo_AdjClaimDuty.ColumnName = "AdjClaimDuty";
			zCalcEditColumnStyleInfo_AdjClaimDuty.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo_AdjClaimMPF = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo_AdjClaimMPF.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo_AdjClaimMPF.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f2eb9726-76bb-4783-8fe0-5678ca849cb7", "Adj Claim MPF");
			zCalcEditColumnStyleInfo_AdjClaimMPF.ColumnName = "AdjClaimMPF";
			zCalcEditColumnStyleInfo_AdjClaimMPF.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo_AdjClaimHMF = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo_AdjClaimHMF.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo_AdjClaimHMF.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bffb387d-4ce0-49ad-9011-fc63ee5ac091", "Adj Claim HMF");
			zCalcEditColumnStyleInfo_AdjClaimHMF.ColumnName = "AdjClaimHMF";
			zCalcEditColumnStyleInfo_AdjClaimHMF.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo_AdjClaimTax = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo_AdjClaimTax.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo_AdjClaimTax.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("878b7c37-e622-40ea-b32d-95ef66559226", "Adj Claim Tax");
			zCalcEditColumnStyleInfo_AdjClaimTax.ColumnName = "AdjClaimTax";
			zCalcEditColumnStyleInfo_AdjClaimTax.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo_AdjClaimDuty);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo_AdjClaimMPF);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo_AdjClaimHMF);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo_AdjClaimTax);
		}

		protected override void CustomsInvoiceLinesBoundGrid_AfterBind(object sender, EventArgs e)
		{
			base.CustomsInvoiceLinesBoundGrid_AfterBind(sender, e);

			if (CustomsInvoiceLinesBoundGrid.ListManager != null)
			{
				foreach (JobComInvoiceLine line in CustomsInvoiceLinesBoundGrid.ListManager.List)
				{
					line.US_DRWIsForManufacturerSectionInfo.ValueChanged += new EventHandler(US_DRWIsForManufacturerSectionInfo_ValueChanged);
					line.US_TariffTypeInfo.ValueChanged += US_TariffTypeInfo_ValueChanged;
					ChangeExportTariffFindBoxModuleID();
				}

				CustomsInvoiceLinesBoundGrid.ListManager.CurrentChanged += new EventHandler(CustomsInvoiceLinesBoundGridListManager_CurrentChanged);
				ChangeControlVisibilityForManufacturerSection();
			}
		}

		void US_TariffTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeExportTariffFindBoxModuleID();
		}

		void ChangeExportTariffFindBoxModuleID()
		{
			var exportTariffModuleId = GetExportTariffModuleId();
			if (exportTariffModuleId != null)
			{
				this.ExportTariffFindBox.ModuleID = exportTariffModuleId;
				this.ACEExportTariffFindBox.ModuleID = exportTariffModuleId;
			}
		}

		ModuleIdentifier GetExportTariffModuleId()
		{
			var listManager = CustomsInvoiceLinesBoundGrid.ListManager;
			var currentInvoiceLine = listManager != null && listManager.Count > 0 ? (JobComInvoiceLine)listManager.GetCurrent() : null;

			if (currentInvoiceLine != null)
			{
				return ModuleIDs.Customs.Universal.RefCusTariff;
			}
			return null;
		}

		void CustomsInvoiceLinesBoundGridListManager_CurrentChanged(object sender, EventArgs e)
		{
			ChangeControlVisibilityForManufacturerSection();
		}

		void US_DRWIsForManufacturerSectionInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeControlVisibilityForManufacturerSection();
		}

		void ChangeControlVisibilityForManufacturerSection()
		{
			var listManager = CustomsInvoiceLinesBoundGrid.ListManager;
			var currentInvoiceLine = listManager != null && listManager.Count > 0 ? (JobComInvoiceLine)listManager.GetCurrent() : null;
			DrawbackInvoiceLineClaimedAmountsUserControl.ShowOrHideManufacturerRelatedUserControls(currentInvoiceLine?.US_DRWIsForManufacturerSection ?? false);
		}

		public new IInvoicesProvider CurrentDataItem
		{
			get { return (IInvoicesProvider)base.CurrentDataItem; }
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			AdjustColumnsOnGrid();
			US_DRWPurposeInfo_ValueChanged(null, null);
			ShowOrHideACERelatedColumns();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			if (this.ClassificationDetailsGroupBox.Controls.Contains(this.CustomsQuantityCalcDropEdit))
			{
				this.ClassificationDetailsGroupBox.Controls.Remove(this.CustomsQuantityCalcDropEdit);
			}
			if (LineDetailTabControl.TabPages.Contains(LineChargesTabPage))
			{
				LineDetailTabControl.TabPages.Remove(LineChargesTabPage);
			}
			InvoiceLinesSummaryGroupBox.Visible = false;
			JI_CountryOfOriginBoundFindBox.Visible = false;
			VolumeCalcDropEdit.Visible = false;
			JI_WeightCalcDropEdit.Visible = false;
			JI_LinePriceBoundCurrencyControl.Visible = false;
			JI_RH_NKCommodity_CodeBoundFindBox.Visible = false;
			CustomsQuantityCalcDropEdit.Visible = false;
			InvoiceQuantityCalcDropEdit.Visible = false;
		}

		readonly string[] columnsNotRequiredForDrawback = new string[] {
		JobComInvoiceLine.Schema.JI_CountryOfOrigin,
		JobComInvoiceLine.Schema.JI_Weight,
		JobComInvoiceLine.Schema.JI_WeightUQ,
		JobComInvoiceLine.Schema.JI_NetWeight,
		JobComInvoiceLine.Schema.JI_NetWeightUQ,
		JobComInvoiceLine.Schema.JI_Volume,
		JobComInvoiceLine.Schema.JI_VolumeUQ,
		JobComInvoiceLine.Schema.JI_OrderNumber,
		JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
		JobComInvoiceLine.Schema.JI_PartAttrib1,
		JobComInvoiceLine.Schema.JI_PartAttrib2,
		JobComInvoiceLine.Schema.JI_PartAttrib3,
		JobComInvoiceLine.Schema.JI_SerialNumber,
		JobComInvoiceLine.Schema.UnitPrice,
		JobComInvoiceLine.Schema.JI_CustomAttrib1,
		JobComInvoiceLine.Schema.JI_CustomAttrib2,
		JobComInvoiceLine.Schema.JI_CustomAttrib3,
		JobComInvoiceLine.Schema.JI_CustomAttrib4,
		JobComInvoiceLine.Schema.JI_CustomAttrib5,
		JobComInvoiceLine.Schema.JI_CustomAttrib6,
		JobComInvoiceLine.Schema.JI_CustomTextBlob1,
		JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber,
		JobComInvoiceLine.Schema.JI_Calc_EntryNumber,
		JobComInvoiceLine.Schema.JI_Calc_XTN,
		JobComInvoiceLine.Schema.JI_LinePrice,
		JobComInvoiceLine.Schema.JI_Calc_Invoice,
		JobComInvoiceLine.Schema.JI_InvoiceQuantity,
		JobComInvoiceLine.Schema.JI_InvoiceUQ,
		JobComInvoiceLine.Schema.JI_CustomsQuantity,
		JobComInvoiceLine.Schema.JI_CustomsUnitQty
		};

		void AdjustColumnsOnGrid()
		{
			if (CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff) == null)
			{
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new TariffColumnStyleInfo
				{
					BindToList = "Lookups+ImportTariffs",
					Caption = "HTSUS No",
					ColumnName = JobComInvoiceLine.Schema.JI_FormattedTariff,
					IsMandatory = true,
					ModuleID = ModuleIDs.Customs.US.Tariff
				});
			}

			if (CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.US_TariffType) == null)
			{
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
				{
					Caption = "Tariff Type",
					ColumnName = JobComInvoiceLine.Schema.US_TariffType,
					IsMandatory = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				});
			}

			if (CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.US_FormattedExportTariff) == null)
			{
				var tariffColumnStyleInfo = new TariffColumnStyleInfo
				{
					Caption = "Tariff",
					ColumnName = JobComInvoiceLine.Schema.US_FormattedExportTariff,
					IsMandatory = true
				};

				var exportTariffModuleId = GetExportTariffModuleId();
				if (exportTariffModuleId != null)
				{
					tariffColumnStyleInfo.ModuleID = exportTariffModuleId;
				}
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffColumnStyleInfo);
			}

			if (CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.US_DRWQuarterlyHMF) == null)
			{
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo
				{
					Caption = "Quarterly HMF",
					ColumnName = JobComInvoiceLine.Schema.US_DRWQuarterlyHMF,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88)
				});
			}
			CustomsInvoiceLinesBoundGrid.SetColumnVisible(false, columnsNotRequiredForDrawback);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (JobDeclaration != null)
			{
				JobDeclaration.US_DRWPurposeInfo.ValueChanged -= new EventHandler(US_DRWPurposeInfo_ValueChanged);
				JobDeclaration.US_EntryTypeInfo.ValueChanged -= new EventHandler(US_EntryTypeInfo_ValueChanged);
				JobDeclaration.US_DRWSectionInfo.ValueChanged -= new EventHandler(US_DRWSectionInfo_ValueChanged);
				JobDeclaration.JE_ApplicationCodeInfo.ValueChanged -= new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				AddDefaultInvoiceHeader();
			}
			if (JobDeclaration != null)
			{
				JobDeclaration.US_DRWPurposeInfo.ValueChanged += new EventHandler(US_DRWPurposeInfo_ValueChanged);
				JobDeclaration.US_EntryTypeInfo.ValueChanged += new EventHandler(US_EntryTypeInfo_ValueChanged);
				JobDeclaration.US_DRWSectionInfo.ValueChanged += new EventHandler(US_DRWSectionInfo_ValueChanged);
				JobDeclaration.JE_ApplicationCodeInfo.ValueChanged += new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
			}
		}

		void US_EntryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCurrentDataItem();
		}

		void US_DRWSectionInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCurrentDataItem();
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlVisibility();
			ColumnsAvailability();
			ShowOrHideACERelatedColumns();
			RefreshCurrentDataItem();
		}

		void ShowOrHideACERelatedColumns()
		{
			var aceNAFTAColumns = new string[]
			{
				DrawbackNAFTA.Schema.US_DRWNAFTACountryTariffNumber2,
				DrawbackNAFTA.Schema.US_DRWNAFTACountryTariffNumber3,
				DrawbackNAFTA.Schema.US_DRWNAFTACountryOfExport
			};
			var aceAdditionalImportTariffColumns = new string[]
			{
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_Description,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_Quantity1,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_UQ1,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_AllowableQty1,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_ValuePerUnit1,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_SubstitutedValue1,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_Quantity2,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_UQ2,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_AllowableQty2,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_ValuePerUnit2,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_SubstitutedValue2,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_Quantity3,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_UQ3,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_AllowableQty3,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_ValuePerUnit3,
				AutoDrawbackAdditionalImportTariffNumber.Schema.US_SubstitutedValue3,
			};

			var isACEDrawback = JobDeclaration != null && JobDeclaration.IsACEDrawback;
			if (isACEDrawback)
			{
				DrawbackNAFTAGrid.AddToAvailableColumns(aceNAFTAColumns);
				AdditionalImportTariffNumbersGrid.AddToAvailableColumns(aceAdditionalImportTariffColumns);
			}
			else
			{
				DrawbackNAFTAGrid.RemoveFromAvailableColumns(aceNAFTAColumns);
				AdditionalImportTariffNumbersGrid.RemoveFromAvailableColumns(aceAdditionalImportTariffColumns);
			}
		}

		void RefreshCurrentDataItem()
		{
			var currentItem = CurrentDataItem;
			if (currentItem != null)
			{
				currentItem.RefreshBindingIncludingChildren();
			}
		}

		void AddDefaultInvoiceHeader()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.AddDefaultInvoice();
			}
		}

		protected override bool ShowCustomLabelsForInvoiceLine
		{ get { return false; } }

		#region Adjust Grid and Controls Visibility Based on Drawback Declaration Purpose

		void US_DRWPurposeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlVisibility();
			ColumnsAvailability();
			RefreshCurrentDataItem();
		}

		void ControlVisibility()
		{
			var is7552 = JobDeclaration != null && JobDeclaration.Is7552;
			var isACEDrawback = JobDeclaration != null && JobDeclaration.IsACEDrawback;
			this.ACSImportExportSectionsPanel.Visible = !isACEDrawback;
			this.ACEImportExportSectionsPanel.Visible = isACEDrawback;
			this.ACSManufacturedPanel.Visible = !isACEDrawback;
			this.ACEManufacturedPanel.Visible = isACEDrawback;
			this.DocumentDetailsTabPage.TabVisible = isACEDrawback;

			this.ExportSectionGroupBox.Visible = !is7552;
			this.ACEExportSectionGroupBox.Visible = isACEDrawback && !is7552;
			this.NAFTATabPage.TabVisible = !is7552;
			this.AdditionalTariffsTabPage.TabVisible = !is7552;
			this.IsForImportSectionCheckBox.Visible = !is7552;
			this.NoticeOfIntentGroupBox.SetReadOnly(is7552);

			this.CDUseDropEdit.Visible = is7552;
			this.ACEDocumentCDUseDropEdit.Visible = isACEDrawback && is7552;
			this.CDIndicatorCheckBox.Visible = is7552;
			this.DateDelFromDateEdit.Visible = is7552;
			this.ACEDocumentDeliveryDateEdit.Visible = isACEDrawback && is7552;
			this.DateDelToDateEdit.Visible = is7552;
			this.ACEDocumentDeliveredDateToDateEdit.Visible = isACEDrawback && is7552;
			this._7552DrawbackProductCheckBox.Visible = is7552;
			this.ACEManufRulingNumberTextBox.Visible = isACEDrawback;

			ManufacturedArticlesTabPage.TabVisible = JobDeclaration != null && JobDeclaration.IsManufacturingDrawbackSupported;

			DrawbackInvoiceLineClaimedAmountsUserControl.ShowOrHideACERelatedUserControls(isACEDrawback);
			DrawbackInvoiceLineClaimedAmountsUserControl.ShowOrHide7552RelatedUserControls(isACEDrawback, is7552);

			UpdateColumnCaptions(isACEDrawback);
		}

		void UpdateColumnCaptions(ZBool isACEDrawback)
		{
			UpdateColumnCaption(JobComInvoiceLine.Schema.US_DRWCertOfManufacture, isACEDrawback ? "Cert. of Manuf." : "CM&D");
			UpdateColumnCaption(JobComInvoiceLine.Schema.US_DRWDateRcvFrom, isACEDrawback ? "Manuf. Date Rcv." : "From Rcv Date");
			UpdateColumnCaption(JobComInvoiceLine.Schema.US_DRWDateUsedFrom, isACEDrawback ? "Manuf. Date Used" : "From Used Date");
			UpdateColumnCaption(JobComInvoiceLine.Schema.US_DRWCDInd, isACEDrawback ? "Eligible/CD" : "CD Indicator");
			UpdateColumnCaption(JobComInvoiceLine.Schema.US_DRWPort, isACEDrawback ? "Port Code" : "Entry/CM Port");
		}

		void UpdateColumnCaption(ZString columnName, ZString captionString)
		{
			CustomsInvoiceLinesBoundGrid.SetColumnCaption(columnName, captionString);
		}

		void ColumnsAvailability()
		{
			var invoiceLine = CurrentDataItem as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.US_DRWIsForImportSection = true;
			}
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.SetAllAvailability(false);
				foreach (KeyValuePair<string, bool> pair in GetAvailableColumns())
				{
					CustomsInvoiceLinesBoundGrid.SetAvailability(true, pair.Key);
					CustomsInvoiceLinesBoundGrid.SetColumnVisible(pair.Value, pair.Key);
				}
			}
			CustomsInvoiceLinesBoundGrid.ReOrderColumns(InvoiceLinesGridColumnNamesInSortOrder);

			AdditionalImportTariffNumbersGrid.SetAllColumnsVisible(false);
			AdditionalImportTariffNumbersGrid.SetColumnVisible(true, GetVisibleColumnsForAdditionalTariff());
		}

		IEnumerable<KeyValuePair<string, bool>> GetAvailableColumns()
		{
			var isACEDrawback = JobDeclaration != null && JobDeclaration.IsACEDrawback;
			var is7552 = JobDeclaration != null && JobDeclaration.Is7552;
			var result = new Dictionary<string, bool>();
			result.Add(JobComInvoiceLine.Schema.JI_LineNo, true);

			if (!is7552)
			{
				result.Add(JobComInvoiceLine.Schema.US_DRWIsForImportSection, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWIsForExportSection, true);
			}

			result.Add(JobComInvoiceLine.Schema.JI_Description, true);
			result.Add(JobComInvoiceLine.Schema.US_ImportEntryNo, true);
			result.Add(JobComInvoiceLine.Schema.US_DRWImportEntryLine, true);
			result.Add(JobComInvoiceLine.Schema.US_DRWDateRcvFrom, true);
			result.Add(JobComInvoiceLine.Schema.US_DRWDateUsedFrom, true);
			result.Add(JobComInvoiceLine.Schema.JI_FormattedTariff, true);
			result.Add(JobComInvoiceLine.Schema.US_ImpDecInvoiceNum, false);
			result.Add(JobComInvoiceLine.Schema.JI_PartNo, true);

			result.Add(JobComInvoiceLine.Schema.US_DRWCMCDIndicator, !isACEDrawback);
			result.Add(JobComInvoiceLine.Schema.US_DRWCertOfManufacture, !isACEDrawback);
			result.Add(JobComInvoiceLine.Schema.US_DRWPort, !isACEDrawback);
			result.Add(JobComInvoiceLine.Schema.US_DRWEntryDate, !isACEDrawback);
			result.Add(JobComInvoiceLine.Schema.US_DRWDateRcvTo, !isACEDrawback);
			result.Add(JobComInvoiceLine.Schema.US_DRWDateUsedTo, !isACEDrawback);

			if (isACEDrawback)
			{
				result.Add(JobComInvoiceLine.Schema.US_DRWImpActInd, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWClaimBasis, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWImpManufRuleNo, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWCDInd, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWAccMethod, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWImpTrkID, false);
			}
			else if (is7552)
			{
				result.Add(JobComInvoiceLine.Schema.US_DRWCDInd, true);
			}

			if (is7552)
			{
				result.Add(JobComInvoiceLine.Schema.US_DRWCDUse, !isACEDrawback);
				result.Add(JobComInvoiceLine.Schema.US_DRWDateDelFrom, !isACEDrawback);
				result.Add(JobComInvoiceLine.Schema.US_DRWDateDelTo, !isACEDrawback);
			}
			else
			{
				result.Add(JobComInvoiceLine.Schema.US_DRWExportDate, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWExportAction, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWExportID, true);
				result.Add(JobComInvoiceLine.Schema.US_DRWExportDest, true);
				result.Add(JobComInvoiceLine.Schema.US_TariffType, true);
				result.Add(JobComInvoiceLine.Schema.US_FormattedExportTariff, true);
				result.Add(JobComInvoiceLine.Schema.US_OH_DRWExporterOrDestroyer, true);
				result.Add(JobComInvoiceLine.Schema.US_OA_DRWExporterOrDestroyer, true);

				if (isACEDrawback)
				{
					result.Add(JobComInvoiceLine.Schema.US_DRWExpWavInd, true);
					result.Add(JobComInvoiceLine.Schema.US_DRWExpBOLCarrier, true);
					result.Add(JobComInvoiceLine.Schema.US_DRWExpBOLInd, true);
					result.Add(JobComInvoiceLine.Schema.US_DRWExpNoticeInd, true);
				}
			}

			#region Claimed Amounts

			result.Add(JobComInvoiceLine.Schema.US_DRWClaimAmountOverriden_New, false);
			result.Add(JobComInvoiceLine.Schema.WeightedRatio, false);
			result.Add(JobComInvoiceLine.Schema.MPFWeightedRatio, false);
			result.Add(JobComInvoiceLine.Schema.DRWImportQuantity, false);
			result.Add(JobComInvoiceLine.Schema.DRWImportUQ, false);
			result.Add(JobComInvoiceLine.Schema.DRWExportQuantity, false);
			result.Add(JobComInvoiceLine.Schema.DRWExportUQ, false);

			result.Add(JobComInvoiceLine.Schema.DeclaredVFD, false);
			result.Add(JobComInvoiceLine.Schema.LineDuty, false);
			result.Add(JobComInvoiceLine.Schema.ExportValue, false);
			result.Add(JobComInvoiceLine.Schema.LineDutyRateDesc, false);
			result.Add(JobComInvoiceLine.Schema.ClaimedDuty, false);
			result.Add(JobComInvoiceLine.Schema._99ClaimedDuty, false);
			result.Add(JobComInvoiceLine.Schema.AdjClaimDuty, false);

			result.Add(JobComInvoiceLine.Schema.DeclaredTax, false);
			result.Add(JobComInvoiceLine.Schema.TaxPerUnit, false);
			result.Add(JobComInvoiceLine.Schema.ClaimedTax, false);
			result.Add(JobComInvoiceLine.Schema._99ClaimedTax, false);
			result.Add(JobComInvoiceLine.Schema.AdjClaimTax, false);

			result.Add(JobComInvoiceLine.Schema.DeclaredMPF, false);
			result.Add(JobComInvoiceLine.Schema.MPFPerUnit, false);
			result.Add(JobComInvoiceLine.Schema.ClaimedMPF, false);
			result.Add(JobComInvoiceLine.Schema._99ClaimedMPF, false);
			result.Add(JobComInvoiceLine.Schema.AdjClaimMPF, false);

			result.Add(JobComInvoiceLine.Schema.DeclaredHMF, false);
			result.Add(JobComInvoiceLine.Schema.HMFPerUnit, false);
			result.Add(JobComInvoiceLine.Schema.ClaimedHMF, false);
			result.Add(JobComInvoiceLine.Schema._99ClaimedHMF, false);
			result.Add(JobComInvoiceLine.Schema.AdjClaimHMF, false);

			result.Add(JobComInvoiceLine.Schema.US_DRWCalcDutyWithAdValoremRate, false);
			result.Add(JobComInvoiceLine.Schema.US_DRWAdValoremRate, false);

			if (isACEDrawback)
			{
				result.Add(JobComInvoiceLine.Schema.US_DRWQuarterlyHMF, false);
				result.Add(JobComInvoiceLine.Schema.DRWAllowableQTY, false);
				result.Add(JobComInvoiceLine.Schema.DRWGoodsValuePerUQ, false);
				result.Add(JobComInvoiceLine.Schema.SubstitutedValuePerUnit, false);
				result.Add(JobComInvoiceLine.Schema.DRWImportQuantity2, false);
				result.Add(JobComInvoiceLine.Schema.DRWImportUQ2, false);
				result.Add(JobComInvoiceLine.Schema.DRWAllowableQTY2, false);
				result.Add(JobComInvoiceLine.Schema.DRWGoodsValuePerUQ2, false);
				result.Add(JobComInvoiceLine.Schema.SubstitutedValuePerUnit2, false);
				result.Add(JobComInvoiceLine.Schema.DRWImportQuantity3, false);
				result.Add(JobComInvoiceLine.Schema.DRWImportUQ3, false);
				result.Add(JobComInvoiceLine.Schema.DRWAllowableQTY3, false);
				result.Add(JobComInvoiceLine.Schema.DRWGoodsValuePerUQ3, false);
				result.Add(JobComInvoiceLine.Schema.SubstitutedValuePerUnit3, false);
			}
			else
			{
				result.Add(JobComInvoiceLine.Schema.DeclaredOtherFees, false);
				result.Add(JobComInvoiceLine.Schema.OtherFeesPerUnit, false);
				result.Add(JobComInvoiceLine.Schema.ClaimedOtherFees, false);
				result.Add(JobComInvoiceLine.Schema._99ClaimedOtherFees, false);
				result.Add(JobComInvoiceLine.Schema.DutyPerUnit, false);
			}

			#endregion

			return result;
		}

		string[] GetVisibleColumnsForAdditionalTariff()
		{
			return new[] {
			DrawbackAdditionalImportTariffNumber.Schema.US_FormattedTariff,
			DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_LineNo,
			DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_Description };
		}

		#endregion

		#region InvoiceLinesGridColumnNamesInSortOrder
		string[] InvoiceLinesGridColumnNamesInSortOrder
		{
			get
			{
				if (invoiceLinesGridColumnNamesInSortOrder == null)
				{
					var columnNamesInSortOrderList = new List<string>();
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_LineNo);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWIsForImportSection);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_ImportEntryNo);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWImportEntryLine);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_PartNo);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_FormattedTariff);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_InvoiceQuantity);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_InvoiceUQ);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_CustomsQuantity);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_CustomsUnitQty);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.JI_Description);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWPort);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWEntryDate);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCertOfManufacture);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCMCDIndicator);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateRcvFrom);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateRcvTo);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateUsedFrom);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateUsedTo);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWImpActInd);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWClaimBasis);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWImpManufRuleNo);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWAccMethod);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWSubstituted);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWIsForExportSection);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExportDate);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExportAction);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExportID);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExportDest);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_TariffType);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_FormattedExportTariff);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExpBOLInd);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExpBOLCarrier);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExpNoticeInd);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWExpWavInd);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_OH_DRWExporterOrDestroyer);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_OA_DRWExporterOrDestroyer);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCDInd);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCDUse);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateDelFrom);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWDateDelTo);

					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWClaimAmountOverriden_New);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWImportQuantity);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWImportUQ);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWAllowableQTY);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWGoodsValuePerUQ);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.SubstitutedValuePerUnit);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWImportQuantity2);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWImportUQ2);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWAllowableQTY2);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWGoodsValuePerUQ2);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.SubstitutedValuePerUnit2);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWImportQuantity3);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWImportUQ3);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWAllowableQTY3);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWGoodsValuePerUQ3);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.SubstitutedValuePerUnit3);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWExportQuantity);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DRWExportUQ);

					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DeclaredVFD);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DutyPerUnit);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.ExportValue);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.ClaimedDuty);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema._99ClaimedDuty);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.AdjClaimDuty);

					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DeclaredTax);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.TaxPerUnit);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.ClaimedTax);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema._99ClaimedTax);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.AdjClaimTax);

					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DeclaredMPF);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.MPFPerUnit);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.ClaimedMPF);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema._99ClaimedMPF);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.AdjClaimMPF);

					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DeclaredHMF);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.HMFPerUnit);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.ClaimedHMF);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema._99ClaimedHMF);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.AdjClaimHMF);

					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.DeclaredOtherFees);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.OtherFeesPerUnit);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.ClaimedOtherFees);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema._99ClaimedOtherFees);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_ImpDecInvoiceNum);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWCalcDutyWithAdValoremRate);
					columnNamesInSortOrderList.Add(JobComInvoiceLine.Schema.US_DRWAdValoremRate);
					invoiceLinesGridColumnNamesInSortOrder = columnNamesInSortOrderList.ToArray();
				}
				return invoiceLinesGridColumnNamesInSortOrder;
			}
		}
		string[] invoiceLinesGridColumnNamesInSortOrder;

		#endregion
	}
}
