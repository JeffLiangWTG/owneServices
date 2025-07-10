using System.Collections.Generic;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.PlugIn;

public partial class AdditionalInfosUserControlWithGrid : EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid
{
	public AdditionalInfosUserControlWithGrid()
	{
	}

	protected override IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayout() => new AdditionalInfosUserControlLayout();
	protected override IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayout() => new AdditionalInfosUserControlLayout();

	protected override IReadOnlyList<string> AvailableColumnNames => DataSource is JobDeclaration declaration && declaration.IsImport
		? ImportAvailableColumnNames : ExportAvailableColumnNames;

	protected virtual string[] ImportAvailableColumnNames => new[]
	{
		AdditionalInfo.Schema.CSI_SubType,
		AdditionalInfo.Schema.CSI_ReferenceNumber,
		AdditionalInfo.Schema.CSI_ReferenceNumber2,
		AdditionalInfo.Schema.CSI_RX_NKCurrency,
		AdditionalInfo.Schema.CSI_Value
	};

	protected virtual string[] ExportAvailableColumnNames => new[]
	{
		AdditionalInfo.Schema.CSI_ReferenceNumber2,
		AdditionalInfo.Schema.CSI_RX_NKCurrency,
		AdditionalInfo.Schema.CSI_Value
	};
}
