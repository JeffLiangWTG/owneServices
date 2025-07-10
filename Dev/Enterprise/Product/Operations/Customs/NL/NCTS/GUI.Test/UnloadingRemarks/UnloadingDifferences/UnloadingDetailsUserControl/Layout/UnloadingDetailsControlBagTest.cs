using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

[TestedType(typeof(UnloadingDetailsControlBag))]
sealed class UnloadingDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(UnloadingDetailsControlBag.UnloadingRemarksFreeTextTextBox);
			yield return nameof(UnloadingDetailsControlBag.UnloadingRemarksGrid);
		}
	}

	protected override ControlBag GetControlBagForTesting() => UnloadingDetailsControlBag.Instance;
}
