using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalAdditionalDetailsControlBag))]
sealed class ArrivalAdditionalDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ArrivalAdditionalDetailsControlBag.TirPageNumberDropEdit);
			yield return nameof(ArrivalAdditionalDetailsControlBag.TirUnloadingNumberDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ArrivalAdditionalDetailsControlBag.Instance;
}
