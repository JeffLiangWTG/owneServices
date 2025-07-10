using System.Collections.Generic;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class EntryLineTaxAndFeeUserControl : EU.GUI.EntryLineTaxAndFeeUserControl
	{
		public EntryLineTaxAndFeeUserControl()
		{
			InitializeComponent();
			AddColumnsToGrid();
			InitializeCustomizedGrid();
		}

		void AddColumnsToGrid()
		{
			EntryLineDutyAndTaxGrid.ColumnStyles.Add(
				new ZDropEditColumnStyleInfo(CusEntryLineFee.Schema.NationalFeeTypeCode,
					CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100))
				{
					CaptionResourceString = Res.GetData("D3660479-AE46-4D31-955A-B13D134A0B44", "Duty Type"),
					IsVisible = true
				});

			EntryLineDutyAndTaxGrid.ColumnStyles.Add(
				new ZTextBoxColumnStyleInfo(nameof(CusEntryLineFee.NationalFeeTypeCodeDescription),
					CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160))
				{
					CaptionResourceString = Res.GetData("BEC80694-DBC6-44D8-9262-9F8D57DC3489", "Description"),
					IsReadOnly = true,
					IsVisible = true
				});
		}

		void InitializeCustomizedGrid()
		{
			EntryLineDutyAndTaxGrid.SetAllColumnsVisible(false);
			EntryLineDutyAndTaxGrid.SetColumnVisible(true, ColumnNamesInSortOrder);
			EntryLineDutyAndTaxGrid.ReOrderColumns(ColumnNamesInSortOrder);
		}

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columnList = new List<string>
					{
						CusEntryLineFee.Schema.NationalFeeTypeCode,
						nameof(CusEntryLineFee.NationalFeeTypeCodeDescription),
						CusEntryLineFee.Schema.CF_BaseValue,
						CusEntryLineFee.Schema.CF_MethodOfCalculation,
						CusEntryLineFee.Schema.CF_Rate,
						CusEntryLineFee.Schema.CF_ChargeAmount,
						CusEntryLineFee.Schema.CF_MethodOfPayment
					};
					columnNamesInSortOrder = columnList.ToArray();
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;
	}
}
