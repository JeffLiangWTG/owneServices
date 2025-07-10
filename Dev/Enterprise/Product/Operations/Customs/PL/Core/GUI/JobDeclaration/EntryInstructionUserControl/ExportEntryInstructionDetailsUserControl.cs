using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.PL.GUI.PlugIn;

namespace Enterprise.Customs.PL.GUI;

public partial class ExportEntryInstructionDetailsUserControl : EntryInstructionDetailsUserControl
{
	public ExportEntryInstructionDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override Type GetDetailsUserControlType() => typeof(ExportEntryInstructionDetailBasicUserControl);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(LayoutSupportingDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(LayoutPreviousDocumentsUserControl);

	protected override ResourceStringData GetAdditionalInfosTabCaption() => Enterprise.Customs.PL.GUI.Res.GetData("5D71D568-DC41-48DB-A87E-F9EC7B47BA21", "[44] Additional Documents");
}
