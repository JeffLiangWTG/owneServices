using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsControlBag))]
sealed class EntryInstructionDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(EntryInstructionDetailsControlBag.ExportManifestCheckBox);
			yield return nameof(EntryInstructionDetailsControlBag.PostExportTransitCheckBox);
			yield return nameof(EntryInstructionDetailsControlBag.EADPrintOutDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.TemporaryLocationDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.TemporaryLocationTextBox);
			yield return nameof(EntryInstructionDetailsControlBag.OfficeOfExitArrivalTimeLimitDateEdit);
			yield return nameof(EntryInstructionDetailsControlBag.DeclarationDateDateEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsControlBag.Instance;
}
