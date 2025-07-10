using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsLayoutBuilder))]
sealed class EntryInstructionDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionDetailsLayoutBuilder, CusEntryInstruction, Customs.GUI.EntryInstructionBasicDetailsControlBag>
{
	public void TestControlsVisibility() => CombineAssertions(() =>
	{
		VisibilityCheckForImportExport(false, true, EntryInstructionDetailsControlBag.Instance.PostExportTransitCheckBox);
		VisibilityCheckForImportExport(false, true, EntryInstructionDetailsControlBag.Instance.EADPrintOutDropEdit);
		VisibilityCheckForImportExport(false, true, EntryInstructionDetailsControlBag.Instance.OfficeOfExitArrivalTimeLimitDateEdit);
		VisibilityCheckForImportExport(true, true, EntryInstructionDetailsControlBag.Instance.DeclarationDateDateEdit);
	});

	public void TestTemporaryLocationDropEditVisibility() => CombineAssertions(() => VisibilityCheckForBindingToExportManifest(EntryInstructionDetailsControlBag.Instance.TemporaryLocationDropEdit));

	public void TestTemporaryLocationTextBoxVisibility() => CombineAssertions(() => VisibilityCheckForBindingToExportManifest(EntryInstructionDetailsControlBag.Instance.TemporaryLocationTextBox));

	public void TestExportManifestCheckBoxVisibility()
	{
		instruction.ZG_ExportManifest = false;
		declaration.JE_CustomsOffice = "ABC";
		declaration.JE_OfficeOfEntryExit = "CBA";

		CombineAssertions(() =>
		{
			Assert("ExportManifestCheckBox - ExportManifest::false", !layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ExportManifestCheckBox, instruction));

			instruction.ZG_ExportManifest = true;
			Assert("ExportManifestCheckBox - JE_CustomsOffice and JE_OfficeOfEntryExit not equal", !layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ExportManifestCheckBox, instruction));

			declaration.JE_OfficeOfEntryExit = "ABC";
			Assert("ExportManifestCheckBox", layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ExportManifestCheckBox, instruction));

			declaration.JE_OfficeOfEntryExit = "";
			Assert("ExportManifestCheckBox - JE_CustomsOffice and JE_OfficeOfEntryExit not equal", !layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ExportManifestCheckBox, instruction));

			declaration.JE_CustomsOffice = "";
			Assert("ExportManifestCheckBox - JE_CustomsOffice and JE_OfficeOfEntryExit are empty", !layout.IsVisible(EntryInstructionDetailsControlBag.Instance.ExportManifestCheckBox, instruction));
		});	
	}

	protected override int ExpectedMaxColumns => 3;

	protected override EntryInstructionDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new EntryInstructionDetailsLayoutBuilder();

	void VisibilityCheckForImportExport(bool visibileForImport, bool visibleForExport, ControlReference controlReference)
	{
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals($"{controlReference.Name} Visible, for " + Common.Shared.SharedJobMessageTypeList.Codes.Export, visibleForExport, layout.IsVisible(controlReference, instruction));

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals($"{controlReference.Name} Visible, for " + Common.Shared.SharedJobMessageTypeList.Codes.Import, visibileForImport, layout.IsVisible(controlReference, instruction));
	}

	void VisibilityCheckForBindingToExportManifest(ControlReference controlReference)
	{
		instruction.ZG_ExportManifest = false;
		declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;
		declaration.JE_LocationOfGoods = "ABC";
		declaration.JE_OfficeOfEntryExit = "CBA";

		Assert($"{controlReference.Name} is not visible", !layout.IsVisible(EntryInstructionDetailsControlBag.Instance.TemporaryLocationDropEdit, instruction));

		instruction.ZG_ExportManifest = true;
		Assert($"{controlReference.Name} is not visible", !layout.IsVisible(EntryInstructionDetailsControlBag.Instance.TemporaryLocationDropEdit, instruction));

		declaration.JE_LocationOfGoods = "ABC";
		declaration.JE_OfficeOfEntryExit = "ABC";
		Assert($"{controlReference.Name} is not visible", !layout.IsVisible(EntryInstructionDetailsControlBag.Instance.TemporaryLocationDropEdit, instruction));

		declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
		declaration.JE_LocationOfGoods = "ABC";
		Assert($"{controlReference.Name} is visible", layout.IsVisible(EntryInstructionDetailsControlBag.Instance.TemporaryLocationDropEdit, instruction));

		instruction.ZG_ExportManifest = false;
		Assert($"{controlReference.Name} is not visible", !layout.IsVisible(EntryInstructionDetailsControlBag.Instance.TemporaryLocationDropEdit, instruction));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		instruction = declaration.CustomsEntryInstructions.AddNew();
		layout = ((IPanelLayoutProvider)new EntryInstructionDetailsLayout()).Layout;
	}

	JobDeclaration declaration;
	CusEntryInstruction instruction;
	PanelLayout layout;
}
