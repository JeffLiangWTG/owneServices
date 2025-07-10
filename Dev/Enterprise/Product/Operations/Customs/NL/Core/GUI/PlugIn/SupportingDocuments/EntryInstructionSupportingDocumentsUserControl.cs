using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.NL.GUI;

public partial class EntryInstructionSupportingDocumentsUserControl : InvoiceLayoutSupportingDocumentsUserControl
{
	public EntryInstructionSupportingDocumentsUserControl() : base()
	{
		InitializeComponent();
	}

	protected override LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(JobDeclaration declaration) => new EntryInstructionSupportingDocumentsFieldsControl(declaration);

	protected override IReadOnlyList<string> UCC6AndExportAvailableColumnNames => new string[5] { AutoCusSupportingInfo.Schema.CSI_Code, AutoCusSupportingInfo.Schema.CSI_ReferenceNumber, AutoCusSupportingInfo.Schema.CSI_ItemNumber, AutoCusSupportingInfo.Schema.CSI_AdditionalDescription, AutoCusSupportingInfo.Schema.CSI_DateOfExpiry };

	protected override IReadOnlyList<string> UCC6AndImportAvailableColumnNames => new string[5] { AutoCusSupportingInfo.Schema.CSI_Code, AutoCusSupportingInfo.Schema.CSI_ReferenceNumber, AutoCusSupportingInfo.Schema.CSI_ItemNumber, AutoCusSupportingInfo.Schema.CSI_AdditionalDescription, AutoCusSupportingInfo.Schema.CSI_DateOfExpiry };
}
