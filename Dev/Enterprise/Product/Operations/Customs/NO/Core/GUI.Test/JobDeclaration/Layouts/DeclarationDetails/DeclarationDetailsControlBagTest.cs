using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(DeclarationDetailsControlBag))]
sealed class DeclarationDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(DeclarationDetailsControlBag.PhaseStatusTextBox);
			yield return nameof(DeclarationDetailsControlBag.MessageStatusTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => DeclarationDetailsControlBag.Instance;
}
