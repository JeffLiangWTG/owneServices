using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public static class CaptionStrings
		{
			public static ResourceStringData MergedLineNumber => Res.GetData("9b25d2ca-25c3-48a8-8b42-216a08a5f944", "Merged Ln. #");
		}

		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();

			AddColumnsToGrid();
		}

		protected override ZBool DynamicLayoutApplied => true;

		protected override void OnLoad(EventArgs e)
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(DefaultColumns);
				base.OnLoad(e);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(DefaultColumns);
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, DefaultColumns);
			}
		}

		void AddColumnsToGrid()
		{
			AddColumnToGrid<ZDropEditColumnStyleInfo>(JobComInvoiceLine.Schema.JI_Procedure, 80);
			AddColumnToGrid<ZDropEditColumnStyleInfo>(JobComInvoiceLine.Schema.JI_PrimaryPreference, 80);
			AddColumnToGrid<ZGuidDropEditColumnStyleInfo>(JobComInvoiceLine.Schema.JI_CEI, 33);
			AddColumnToGrid<ZDropEditColumnStyleInfo>(JobComInvoiceLine.Schema.EntryInstructionDescription, 200);
			AddColumnToGrid<ZDropEditColumnStyleInfo>(JobComInvoiceLine.Schema.JI_ValuationCode, 56, isVisible: false);
			AddColumnToGrid<ZTextBoxColumnStyleInfo>(JobComInvoiceLine.Schema.MergedLineNumber, 80, caption: CaptionStrings.MergedLineNumber);
		}

		string[] defaultColumns;
		string[] DefaultColumns => defaultColumns ??= new[] {
			JobComInvoiceLine.Schema.JI_LineNo,
			JobComInvoiceLine.Schema.MergedLineNumber,
			JobComInvoiceLine.Schema.JI_Calc_Invoice,
			JobComInvoiceLine.Schema.JI_CEI,
			JobComInvoiceLine.Schema.EntryInstructionDescription,
			JobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_Description,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			JobComInvoiceLine.Schema.JI_Tariff,
			JobComInvoiceLine.Schema.JI_PrimaryPreference,
			JobComInvoiceLine.Schema.JI_LinePrice,
			JobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr,
			JobComInvoiceLine.Schema.JI_Weight,
			JobComInvoiceLine.Schema.JI_WeightUQ,
			JobComInvoiceLine.Schema.JI_NetWeight,
			JobComInvoiceLine.Schema.JI_NetWeightUQ,
			JobComInvoiceLine.Schema.JI_Procedure,
			JobComInvoiceLine.Schema.JI_InvoiceQuantity,
			JobComInvoiceLine.Schema.JI_InvoiceUQ,
			JobComInvoiceLine.Schema.JI_CustomsQuantity,
			JobComInvoiceLine.Schema.JI_CustomsUnitQty,
		};

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new InvoiceLineDetailsLayout();
	}
}
