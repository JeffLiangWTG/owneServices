using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing
{
	[TestedType(typeof(DeclarationDetailsControlBag))]
	sealed class DeclarationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DeclarationDetailsControlBag.FallbackProcedureCheckBox);
				yield return nameof(DeclarationDetailsControlBag.FallbackUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DeclarationDetailsControlBag.Instance;
	}
}
