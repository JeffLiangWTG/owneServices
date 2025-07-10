using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.PL.GUI.PlugIn;

namespace Enterprise.Customs.PL.GUI;

public partial class ImportEntryInstructionDetailsUserControl : EntryInstructionDetailsUserControl
{
	public ImportEntryInstructionDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override Type GetDetailsUserControlType() => typeof(ImportEntryInstructionDetailBasicUserControl);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(LayoutSupportingDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(LayoutPreviousDocumentsUserControl);

	protected override ResourceStringData GetAdditionalInfosTabCaption() => Enterprise.Customs.PL.GUI.Res.GetData("DBA39EC0-D14A-465E-B98E-661861BA50CD", "[44] Additional Info");
}
