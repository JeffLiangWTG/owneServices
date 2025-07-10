using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsLayout))]
sealed class EntryInstructionDetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EntryInstructionDetailsControlBag.Instance.EADPrintOutDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsControlBag.Instance.DeclarationDateDateEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.CPCDropEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EntryInstructionDetailsControlBag.Instance.ExportManifestCheckBox, ControlWidthClass.Auto);
			yield return (EntryInstructionDetailsControlBag.Instance.PostExportTransitCheckBox, ControlWidthClass.Auto);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto);
			yield return (EntryInstructionDetailsControlBag.Instance.TemporaryLocationDropEdit, ControlWidthClass.Auto);
			yield return (EntryInstructionDetailsControlBag.Instance.TemporaryLocationTextBox, ControlWidthClass.Auto);
			yield return (EntryInstructionDetailsControlBag.Instance.OfficeOfExitArrivalTimeLimitDateEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 3;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionDetailsLayoutBuilder();
}
