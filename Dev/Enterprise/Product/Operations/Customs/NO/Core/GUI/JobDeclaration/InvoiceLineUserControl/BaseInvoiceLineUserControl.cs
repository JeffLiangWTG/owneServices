using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;

namespace Enterprise.Customs.NO.GUI;

partial class BaseInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
{
	public BaseInvoiceLineUserControl()
	{
		InitializeComponent();
		RemoveUnusedComponents();
	}

	void RemoveUnusedComponents()
	{
		LineSummaryPanel.Controls.Remove(JI_Calc_DutyConvertToLocalCurrencyControl);
		LineSummaryPanel.Controls.Remove(JI_Calc_FOBConvertToLocalCurrencyControl);
	}

	protected void AddColumnToGrid<T>(ZString column, ZInt length, bool isVisible = true, ResourceStringData caption = null)
		where T : ZGridColumnInfo, new()
	{
		var zGridColumnInfo = new T();
		zGridColumnInfo.ColumnName = column;
		zGridColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
		zGridColumnInfo.IsVisible = isVisible;
		zGridColumnInfo.CaptionResourceString = caption;
		CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGridColumnInfo);
	}
}
