using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(PreviousProcedureHeaderLayout))]
sealed class PreviousProcedureHeaderLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	static IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)> FirstColumnControls
	{
		get
		{
			yield return (PreviousProcedureControlBag.Instance.PreviousProcedureDropEdit, ControlWidthClass.Auto);
		}
	}

	static IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)> SecondColumnControls
	{
		get
		{
			yield return (PreviousProcedureControlBag.Instance.ImportFromTemporaryStorageRegisterButton, ControlWidthClass.Auto);
		}
	}

	public void TestImportFromTemporaryStorageRegisterButtonVisibility() => CombineAssertions(() =>
	{
		var entryInstruction = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew();
		var previousDocumentMaster = entryInstruction.PreviousDocumentMaster;

		previousDocumentMaster.CSI_Procedure = "ABCD";
		AssertEquals("Previous proc 'ABCD' => button NOT visible", expected: false, Layout.IsVisible(PreviousProcedureControlBag.Instance.ImportFromTemporaryStorageRegisterButton, previousDocumentMaster));

		previousDocumentMaster.CSI_Procedure = "71A";
		AssertEquals("Previous proc '71A' => button visible", expected: true, Layout.IsVisible(PreviousProcedureControlBag.Instance.ImportFromTemporaryStorageRegisterButton, previousDocumentMaster));

		previousDocumentMaster.CSI_Procedure = "71A8";
		AssertEquals("Previous proc '71A8' => button visible", expected: true, Layout.IsVisible(PreviousProcedureControlBag.Instance.ImportFromTemporaryStorageRegisterButton, previousDocumentMaster));

		previousDocumentMaster.CSI_Procedure = "71E";
		AssertEquals("Previous proc '71E' => button visible", expected: true, Layout.IsVisible(PreviousProcedureControlBag.Instance.ImportFromTemporaryStorageRegisterButton, previousDocumentMaster));

		previousDocumentMaster.CSI_Procedure = ZString.Empty;
		AssertEquals("Previous proc '' => button NOT visible", expected: false, Layout.IsVisible(PreviousProcedureControlBag.Instance.ImportFromTemporaryStorageRegisterButton, previousDocumentMaster));
	});

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousProcedureLayoutBuilder();

	PanelLayout Layout => layout ??= ((IPanelLayoutProvider)new PreviousProcedureHeaderLayout()).Layout;
	PanelLayout layout;
}
