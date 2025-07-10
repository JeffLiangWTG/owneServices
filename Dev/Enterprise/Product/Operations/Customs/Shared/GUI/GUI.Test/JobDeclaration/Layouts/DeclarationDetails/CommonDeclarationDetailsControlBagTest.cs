using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonDeclarationDetailsControlBag))]
	sealed class CommonDeclarationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonDeclarationDetailsControlBag.DeclarationNumberTextBox);
				yield return nameof(CommonDeclarationDetailsControlBag.StatusTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonDeclarationDetailsControlBag.Instance;
	}
}
