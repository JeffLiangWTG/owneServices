using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class AdditionalDocumentsUserControl : EU.GUI.PlugIn.AdditionalInfosUserControl
{
	public AdditionalDocumentsUserControl()
	{
		InitializeComponent();

		AdditionalInfosGrid.AllowOutsideOfParent();

		AdditionalInfosGrid.AllowOverlap(AddiInfoDescriptionTextBox);
	}
	protected override void ChangeGridColumnsVisibility()
	{
		AdditionalInfosGrid.SetAvailability(false, [AdditionalInfo.Schema.CSI_RN_NKCountryCode, AdditionalInfo.Schema.CSI_NctsExportFromEC, AdditionalInfo.Schema.CSI_Status]);
		AdditionalInfosGrid.SetAvailability(true, [AdditionalInfo.Schema.CSI_SubType, AdditionalInfo.Schema.CSI_Code, AdditionalInfo.Schema.CSI_Description, AdditionalInfo.Schema.CSI_ReferenceNumber]);
		AdditionalInfosGrid.ReOrderColumns(columns);
	}

	readonly string[] columns =
	{
			AdditionalInfo.Schema.CSI_SubType,
			AdditionalInfo.Schema.CSI_Code,
			AdditionalInfo.Schema.CSI_ReferenceNumber,
			AdditionalInfo.Schema.CSI_Description
	};
}
