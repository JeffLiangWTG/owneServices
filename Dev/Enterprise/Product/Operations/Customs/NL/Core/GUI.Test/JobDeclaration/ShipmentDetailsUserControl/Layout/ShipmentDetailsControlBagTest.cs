using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ShipmentDetailsControlBag))]
sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsIncoTermsUserControl);
			yield return nameof(ShipmentDetailsControlBag.AgreedPlaceCodeFindBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
}
