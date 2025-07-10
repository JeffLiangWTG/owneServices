using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(ShipmentTypeControlBag))]
sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentTypeControlBag.CustomsTransportModeDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;
}
