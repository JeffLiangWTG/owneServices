using CargoWise.Common;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsCREInvoiceLinesUserControl : CustomsInvoiceLineUserControl
	{
		public CustomsCREInvoiceLinesUserControl()
		{
			InitializeComponent();
			SetupTariffFindBox();
			if (!DesignModeFinder.IsDesigning)
			{
				SetControlVisibility();
				SetColumnPositionsAndCaption();
			}
		}

		public new JobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		void SetupTariffFindBox()
		{
			TariffCodeFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			TariffCodeFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.NewZealand;
			TariffCodeFindBox.GetEffectiveDate = () => (CurrentInvoiceLine as ITariffValidationData)?.DateForDutyRate ?? CargoWise.Types.ZDateTime.Today;
		}

		void SetControlVisibility()
		{
			var useRefDB = UniversalTariffHelper.UseRefDatabaseData;
			NZCClassificationFindBox.Visible = !useRefDB;
			TariffCodeFindBox.Visible = useRefDB;
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_Calc_Invoice);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomsQuantity);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomsUnitQty);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_NetWeight);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_NetWeightUQ);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_Volume);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_VolumeUQ);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_OrderNumber);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_ContainerMode);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_LevyCreditAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_LevyCreditAmountCode);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_AntiDumpingDutyAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CountervailingDutyAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_DutyCreditAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_GSTCreditAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_DepositRefundAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_ExciseDutyCreditAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_QualifiesForPreferentialDuty);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_PreferentialCountryGroup);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_IsZeroRatedDuty);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_IsZeroRatedExcise);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_IsZeroRatedLevies);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_IsZeroRatedGST);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_ConcessionCode);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomAttrib1);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomAttrib2);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomAttrib3);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomAttrib4);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomAttrib5);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomAttrib6);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_PartAttrib1);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_PartAttrib2);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_PartAttrib3);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_SerialNumber);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_CustomTextBlob1);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_SupplementaryQty);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.JI_SupplementaryUQ);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.MergedLineNumber);
				CustomsInvoiceLinesBoundGrid.SetAvailability(false, JobComInvoiceLine.Schema.UnitPrice);
			}
		}

		void SetColumnPositionsAndCaption()
		{
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_LineNo, 0, "Inv. Line#");
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_PartNo, 1, "Product Code");
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_CC, 2, "Lookup Code");
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_Tariff, 3, "Tariff Code");
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_Description, 4, "Goods Description");
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_InvoiceQuantity, 5, "No. Packages");
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_InvoiceUQ, 6, "Package Type");
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_LinePrice, 7, "Item Value");
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_CountryOfOrigin, 8, "Origin");
		}

		void SetColumnPositionAndCaptionBeforeBinding(ZGrid grid, string columnName, int newIndex, string columnCaption)
		{
			Core.Forms.ZGridColumnInfo columnInfo = grid.GetColumnStyle(columnName);
			if (columnInfo == null)
			{
				ErrorReporter.ReportOnce(columnName + ":" + this.Name + "." + grid.Name, "Column [" + columnName + "] does not exist in the Grid [" + this.Name + "." + grid.Name + "]. Cannot set Column Position.");
			}
			else
			{
				if (columnCaption != null)
				{
					columnInfo.Caption = columnCaption;
				}
				grid.ColumnStyles.Remove(columnInfo);
				grid.ColumnStyles.Insert(newIndex, columnInfo);
			}
		}

		protected override void ChangeControlsVisibility()
		{
			JI_RH_NKCommodity_CodeBoundFindBox.Visible = false;
			JI_NetWeightCalcDropEdit.Visible = false;
			CountryOfExportCodeFindBox.Visible = false;
			VolumeCalcDropEdit.Visible = false;
			PermitCodesGrid.Visible = false;
			DutiesLeviesGroupBox.Visible = false;
			ClassificationPanel.Visible = false;
			ClassificationDetailsGroupBox.Visible = false;
			ConcessionCodeFindBox.Visible = false;
			ConcessionCodeDropEdit.Visible = false;
			PreferenceDropEdit.Visible = false;
			PreferentialCountryGroupDropEdit.Visible = false;
			CustomsQuantityCalcDropEdit.Visible = false;
			SupplementaryCalcDropEdit.Visible = false;
			DutyRateTextBox.Visible = false;
			DutiesLeviesGroupBox.Visible = false;
			DutyCreditCalcEdit.Visible = false;
			AntiDumpingDutyCalcEdit.Visible = false;
			ExciseDutyCreditCalcEdit.Visible = false;
			LevyCreditCalcEdit.Visible = false;
			GSTCreditCalcEdit.Visible = false;
			CountervailingDutyCalcEdit.Visible = false;
			DepositRefundCalcEdit.Visible = false;
			CodeInfosTabControl.Visible = false;
			PackagingGroupBox.Visible = false;
			PermitCodesGrid.Visible = false;
			ProhibitedCodesGrid.Visible = false;
			OtherInfosGrid.Visible = false;

			this.LineDetailTabControl.Controls.Remove(this.MiscTabPage);
			this.LineDetailTabControl.Controls.Remove(this.TSWTabPage);
			this.LineDetailTabControl.Controls.Remove(this.DangerousGoodsTabPage);
		}
	}
}
