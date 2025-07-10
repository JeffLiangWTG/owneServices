using System;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public partial class NctsPreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl, IDisposable
	{
		public NctsPreviousDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayoutCore();
			SetFields();
		}

		void SetFields()
		{
			this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 36, true);
			this.PrevDocsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 36, true);
		}

		protected sealed override void InitializeGridLayoutCore()
		{
			PreviousDocumentsGrid.ColumnStyles.Clear();
			var columnStylesToAdd = new[] {
				new ZCodeFindBoxColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_Code, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZTextBoxColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_ReferenceNumber, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150) },
				new ZCalcEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_LineNo, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150) },
				new ZCalcEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_Value, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZDropEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_RX_NKCurrency, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZCalcEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_Quantity, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZCalcEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_Quantity2, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZDropEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_UnitOfQuantity2, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZCalcEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_Quantity3, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZDropEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_UnitOfQuantity3, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZDropEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.Incoterm, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZDropEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_SubType, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140) },
				new ZDropEditColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_Procedure, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120) },
				new ZCodeFindBoxColumnStyleInfo() { ColumnName = NctsPreviousDocument.Schema.CSI_RN_NKCountryCode, Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100) }
			};
			PreviousDocumentsGrid.ColumnStyles.AddRange(columnStylesToAdd);
		}
	}
}
