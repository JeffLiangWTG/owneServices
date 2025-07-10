using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(EntryInstructionBasicDetailsControlBag))]
sealed class EntryInstructionBasicDetailsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => EntryInstructionBasicDetailsControlBag.Instance;
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(EntryInstructionBasicDetailsControlBag.TransNatureDropEdit);
			yield return nameof(EntryInstructionBasicDetailsControlBag.IsHighValueOvrdCheckBox);
		}
	}
}
