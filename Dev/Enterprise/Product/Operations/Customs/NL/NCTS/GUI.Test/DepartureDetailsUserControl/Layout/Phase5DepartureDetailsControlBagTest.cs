using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

[TestedType(typeof(Phase5DepartureDetailsControlBag))]
sealed class Phase5DepartureDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(Phase5DepartureDetailsControlBag.CalCalculationMethodDropEdit);
			yield return nameof(Phase5DepartureDetailsControlBag.DateLimitAndCalculationUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => Phase5DepartureDetailsControlBag.Instance;
}
