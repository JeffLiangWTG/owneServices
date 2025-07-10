using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI;

public partial class NLSupportingDocumentsUserControl : EU.GUI.PlugIn.SupportingDocumentsUserControl
{
	public NLSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new SupportingDocumentsFieldsControl();

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		var removeUnitOfQuantityColumn = SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity);
		var removeQuantity2 = SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity2);
		var removeUnitOfQuantity2Column = SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity2);
		var removeStatusColumn = SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_Status);
		var removeDateOfIssueColumn = SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfIssue);
		SupportingDocumentsGrid.ColumnStyles.Remove(removeUnitOfQuantityColumn);
		SupportingDocumentsGrid.ColumnStyles.Remove(removeQuantity2);
		SupportingDocumentsGrid.ColumnStyles.Remove(removeUnitOfQuantity2Column);
		SupportingDocumentsGrid.ColumnStyles.Remove(removeStatusColumn);
		SupportingDocumentsGrid.ColumnStyles.Remove(removeDateOfIssueColumn);

		var unitOfQuantityColumn = new ZArchitecture.GUI.ZDropEditColumnStyleInfo
		{
			CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
			ColumnName = SupportingDocument.Schema.CSI_UnitOfQuantity,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		};
		SupportingDocumentsGrid.ColumnStyles.Insert(4, unitOfQuantityColumn);
		SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
	}

	readonly ZGridColumnInfo[] columnsToAdd =
	{
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			ColumnName = SupportingDocument.Schema.CSI_ReferenceNumber2,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
		},
		new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			ColumnName = SupportingDocument.Schema.CSI_ItemNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
		}
	};
}
