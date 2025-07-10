using System.Collections.Generic;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class InvoiceLineTaxUserControl : ZUserControl
	{
		public InvoiceLineTaxUserControl()
		{
			InitializeComponent();
			AddColumnsToGrid();
			InitializeCustomizedGrid();
		}

		void AddColumnsToGrid()
		{
			InvoiceLineTaxGrid.ColumnStyles.Add(
				new ZDropEditColumnStyleInfo(nameof(JobComInvoiceLineTax.NationalTypeDescription),
					CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160))
				{
					IsVisible = true
				});
		}

		void InitializeCustomizedGrid()
		{
			InvoiceLineTaxGrid.SetAllColumnsVisible(false);
			InvoiceLineTaxGrid.SetColumnVisible(true, ColumnNamesInSortOrder);
			InvoiceLineTaxGrid.ReOrderColumns(ColumnNamesInSortOrder);
		}

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columnList = new List<string>
					{
						JobComInvoiceLineTax.Schema.NationalType,
						nameof(JobComInvoiceLineTax.NationalTypeDescription),
						JobComInvoiceLineTax.Schema.JLT_RateOverrideReasonCode,
						JobComInvoiceLineTax.Schema.JLT_BaseValue,
						JobComInvoiceLineTax.Schema.JLT_MethodOfCalculation,
						JobComInvoiceLineTax.Schema.JLT_Rate,
						JobComInvoiceLineTax.Schema.JLT_Amount,
						JobComInvoiceLineTax.Schema.JLT_MethodOfPayment
					};
					columnNamesInSortOrder = columnList.ToArray();
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;
	}
}
