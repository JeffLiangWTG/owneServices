using System.Windows.Forms;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class EntryLineTaxAndFeeUserControl : EU.GUI.EntryLineTaxAndFeeUserControl
{
	public EntryLineTaxAndFeeUserControl()
	{
		InitializeComponent();
		ReplaceBaseAmountColumn();
		ReplaceRateColumn();
	}

	void ReplaceBaseAmountColumn()
	{
		var baseValueForDisplayColumnStyle = new ZMultiControlColumnStyleInfo
		{
			BindToDecimalPlaces = nameof(CusEntryLineFee.CF_BaseValueForDisplayDecimalPlaces),
			CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("66E237E7-CABB-4AD7-B0E6-3196E933E591", "Base Amount"),
			ColumnName = CusEntryLineFee.Schema.CF_BaseValueForDisplay,
			FieldTypeColumnName = nameof(CusEntryLineFee.CF_BaseValueFieldType),
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		};
		var baseAmountColumnStyle = EntryLineDutyAndTaxGrid.GetColumnStyle(Customs.Business.AutoCusEntryLineFee.Schema.CF_BaseValue);
		if (baseAmountColumnStyle != null)
		{
			var index = EntryLineDutyAndTaxGrid.ColumnStyles.IndexOf(baseAmountColumnStyle);
			EntryLineDutyAndTaxGrid.ColumnStyles.Insert(index, baseValueForDisplayColumnStyle);
			EntryLineDutyAndTaxGrid.ColumnStyles.Remove(baseAmountColumnStyle);
		}
	}

	void ReplaceRateColumn()
	{
		var rateForDisplayColumnStyle = new ZMultiControlColumnStyleInfo
		{
			BindToDecimalPlaces = nameof(CusEntryLineFee.CF_RateDecimalPlaces),
			ColumnName = CusEntryLineFee.Schema.CF_RateForDisplay,
			CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("6BE91FE9-39D5-4FF1-8C7E-CAFF2DF8B371", "Tax Rate"),
			FieldTypeColumnName = nameof(CusEntryLineFee.CF_RateFieldType),
			TextAlign = HorizontalAlignment.Right,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		};
		var rateColumnStyle = EntryLineDutyAndTaxGrid.GetColumnStyle(Customs.Business.AutoCusEntryLineFee.Schema.CF_Rate);
		if (rateColumnStyle != null)
		{
			var index = EntryLineDutyAndTaxGrid.ColumnStyles.IndexOf(rateColumnStyle);
			EntryLineDutyAndTaxGrid.ColumnStyles.Insert(index, rateForDisplayColumnStyle);
			EntryLineDutyAndTaxGrid.ColumnStyles.Remove(rateColumnStyle);
		}
	}
}
