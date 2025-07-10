using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.GUI
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
			ResetColumnsInInvoiceChargesGrid();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				if (Invoice != null)
				{
					Invoice.JZ_IncoTermInfo.ValueChanged += JZ_IncoTermInfo_ValueChanged;
				}
			}
		}

		void ResetColumnsInInvoiceChargesGrid()
		{
			SetDescriptionTextBoxColumnStyle();
			SetIsGSTApplicableColumnStyle();
		}

		void SetDescriptionTextBoxColumnStyle()
		{
			InvoiceChargesGrid.SetColumnVisible(false, Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			InvoiceChargesGrid.RemoveFromAvailableColumns(Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);

			var chargeDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			chargeDescriptionTextBoxColumnStyleInfo.ColumnName = AutoJobComInvHeaderCharge.Schema.J7_ChargeDescription;
			chargeDescriptionTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("62573401-8E42-4500-8C63-944F653419B6", "Description");
			chargeDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			InvoiceChargesGrid.ColumnStyles.Insert(1, chargeDescriptionTextBoxColumnStyleInfo);

			var percentageCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			percentageCalcEditColumnStyleInfo1.ColumnName = AutoJobComInvHeaderCharge.Schema.J7_Percentage;
			percentageCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			InvoiceChargesGrid.ColumnStyles.Add(percentageCalcEditColumnStyleInfo1);
		}

		void SetIsGSTApplicableColumnStyle()
		{
			var isGSTApplicableColumnInfo = InvoiceChargesGrid.GetColumnStyle(BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
			isGSTApplicableColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("41A605A8-459F-47D9-8AFB-DE0DC4A30426", "Incl. in Total Inv. Amt. (16)", "Included in Declaration Total Invoice Amount (16)", "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.");
			isGSTApplicableColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);
		}

		void JZ_IncoTermInfo_ValueChanged(object sender, EventArgs e)
		{
			SetIsGSTApplicableColumnVisibility();
		}

		void SetIsGSTApplicableColumnVisibility()
		{
			InvoiceChargesGrid.SetColumnVisible(IsGSTApplicableColumnVisibel, BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
		}

		bool IsGSTApplicableColumnVisibel => Invoice.JZ_MessageType == JobMessageTypeList.Codes.Export && Invoice.JZ_IncoTerm != IncoTerms.ExWorks;

		protected override void ChangeControlsVisibilityWhenMessageTypeChanges()
		{
			base.ChangeControlsVisibilityWhenMessageTypeChanges();
			SetIsGSTApplicableColumnVisibility();
		}
		protected override string ColumnTitleForGSTApplies => string.Empty;
	}
}
