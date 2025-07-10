using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(DeclarationDetailsControlBag))]
	sealed class DeclarationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DeclarationDetailsControlBag.LrnTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DeclarationDetailsControlBag.Instance;
	}
}
