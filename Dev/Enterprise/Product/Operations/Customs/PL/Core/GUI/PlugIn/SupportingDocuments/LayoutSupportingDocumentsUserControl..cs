using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public partial class LayoutSupportingDocumentsUserControl : EU.GUI.PlugIn.LayoutSupportingDocumentsUserControl
{
	public LayoutSupportingDocumentsUserControl()
	{
		InitializeComponent();
		ReplaceUnitOfQuantityColumn();
	}

	protected override EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration)
		=> new LayoutSupportingDocumentsFieldsControl(declaration as Business.Declaration.JobDeclaration);

	protected override string GetSupportingDocumentsFieldsControlBindingString() => SupportingDocumentsGrid.DataMember;

	protected override IReadOnlyList<string> AvailableColumnNames => JobDeclaration?.IsImport ?? true
		? (IsBoundToInvoiceLines ? ImportInvoiceLineAvailableColumnNames : ImportAvailableColumnNames)
		: ExportAvailableColumnNames;

	protected virtual string[] ExportAvailableColumnNames => new[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_ItemNumber,
		SupportingDocument.Schema.CSI_AdditionalDescription,
		SupportingDocument.Schema.CSI_DateOfIssue,
		SupportingDocument.Schema.CSI_DateOfExpiry,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_Value,
		SupportingDocument.Schema.CSI_RX_NKCurrency
	};

	protected virtual string[] ImportAvailableColumnNames => new[]
	{
		SupportingDocument.Schema.CSI_Code,
		PL.Business.Declaration.SupportingDocument.Schema.CSI_CodeDescription,
		SupportingDocument.Schema.CSI_Description,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_ReferenceNumber2,
		SupportingDocument.Schema.CSI_DateOfIssue
	};

	protected virtual string[] ImportInvoiceLineAvailableColumnNames => new[]
	{
		SupportingDocument.Schema.CSI_Code,
		PL.Business.Declaration.SupportingDocument.Schema.CSI_CodeDescription,
		SupportingDocument.Schema.CSI_Description,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_ReferenceNumber2,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_DateOfIssue
	};

	bool IsBoundToInvoiceLines => SupportingDocumentsGrid.DataMember
								== nameof(Business.Declaration.JobDeclaration.FilteredInvoiceLines) + "." + nameof(Business.Declaration.JobComInvoiceLine.SupportingDocuments);

	void ReplaceUnitOfQuantityColumn()
	{
		var unitOfQunatityForDisplayColumnStyle = new ZDropEditColumnStyleInfo
		{
			CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("A4D1F2E5-0C8B-4A3E-9F6C-7D1B2A0E5F3A", "Quantity Unit"),
			ColumnName = SupportingDocument.Schema.CSI_UnitOfQuantity,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		};
		var unitOfQuantityColumnStyle = SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity);
		if (unitOfQuantityColumnStyle != null)
		{
			var index = SupportingDocumentsGrid.ColumnStyles.IndexOf(unitOfQuantityColumnStyle);
			SupportingDocumentsGrid.ColumnStyles.Insert(index, unitOfQunatityForDisplayColumnStyle);
			SupportingDocumentsGrid.ColumnStyles.Remove(unitOfQuantityColumnStyle);
		}
	}
}
