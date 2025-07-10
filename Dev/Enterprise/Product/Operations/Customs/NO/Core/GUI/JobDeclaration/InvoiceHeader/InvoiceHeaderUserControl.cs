using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public partial class InvoiceHeaderUserControl : LayoutDeclarationInvoiceHeaderUserControl
	{
		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
			InitializeGrid();
			InitializeBottomPanel();
		}

		void InitializeGrid()
		{
			ChangeExistingColumnsInGrid();
			AddColumnsToGrid();
			ReOrderColumns();
		}

		void ChangeExistingColumnsInGrid()
		{
			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceNumber).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceDate).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_InvoiceAmount).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_IncoTerm).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_IncoTermPlace).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
		}

		void AddColumnsToGrid()
		{
			CreateNewZDropEditColumnStyleInfo(JobComInvoiceHeader.Schema.JZ_ValuationCode, 95);
			CreateNewZDropEditColumnStyleInfo(JobComInvoiceHeader.Schema.JZ_ValuationMethod, 95);
		}

		void CreateNewZDropEditColumnStyleInfo(ZString column, ZInt length, bool defaultColumn = true)
		{
			var zDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo.ColumnName = column;
			zDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zDropEditColumnStyleInfo.IsVisible = defaultColumn;
			InvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
		}

		void ReOrderColumns()
		{
			InvoiceHeadersBoundGrid.ReOrderColumns(defaultColumns);
			InvoiceHeadersBoundGrid.SetAllColumnsVisible(false);
			InvoiceHeadersBoundGrid.SetColumnVisible(true, defaultColumns);
		}

		readonly string[] defaultColumns =
		{
			nameof(JobComInvoiceHeader.Schema.JZ_InvoiceNumber),
			nameof(JobComInvoiceHeader.Schema.JZ_InvoiceDate),
			nameof(JobComInvoiceHeader.Schema.JZ_InvoiceAmount),
			nameof(JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency),
			nameof(JobComInvoiceHeader.Schema.JZ_IncoTerm),
			nameof(JobComInvoiceHeader.Schema.JZ_IncoTermPlace),
			nameof(JobComInvoiceHeader.Schema.JZ_ValuationCode),
			nameof(JobComInvoiceHeader.Schema.JZ_ValuationMethod),
			nameof(JobComInvoiceHeader.Schema.JZ_OH_Supplier),
			nameof(JobComInvoiceHeader.Schema.InvoiceLineTotal),
			nameof(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString)
		};

		void InitializeBottomPanel()
		{
			RightBottomPanel.Controls.Remove(JZ_CIFAmountBoundCurrencyControl);
			RightBottomPanel.Controls.Remove(JZ_Calc_TNIBoundInvoiceCurrencyControl);
			ApportionmentPendingLabel.AllowOverlap(JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl);
		}
	}
}
