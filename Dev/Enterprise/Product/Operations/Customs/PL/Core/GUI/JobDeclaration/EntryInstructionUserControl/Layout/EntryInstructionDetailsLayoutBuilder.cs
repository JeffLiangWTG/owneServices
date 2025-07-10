using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public class EntryInstructionDetailsLayoutBuilder : EU.GUI.EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>
{
	public EntryInstructionDetailsControlBag PLBag { get; } = EntryInstructionDetailsControlBag.Instance;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		SetVisibility(PLBag.EADPrintOutDropEdit, c => c.JobDeclaration.IsExport);
		SetVisibility(PLBag.PostExportTransitCheckBox, c => c.JobDeclaration.IsExport);
		SetVisibility(PLBag.ExportManifestCheckBox, c => ExportManifestCheckBoxVisibility(c));
		SetVisibility(PLBag.TemporaryLocationDropEdit, c => c.IsExportManifest, c => c.ZG_ExportManifestInfo);
		SetVisibility(PLBag.TemporaryLocationTextBox, c => c.IsExportManifest, c => c.ZG_ExportManifestInfo);
		SetVisibility(PLBag.OfficeOfExitArrivalTimeLimitDateEdit, c => c.JobDeclaration.IsExport);
	}

	static bool ExportManifestCheckBoxVisibility(CusEntryInstruction cusEntryInstruction)
	{
		var declaration = cusEntryInstruction.JobDeclaration;
		return !declaration.IsImport
				&& !(declaration.JE_CustomsOffice.IsEmpty && declaration.JE_OfficeOfEntryExit.IsEmpty)
				&& declaration.JE_CustomsOffice == declaration.JE_OfficeOfEntryExit;
	}
}
