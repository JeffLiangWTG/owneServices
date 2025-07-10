using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(TransportDetailsControlBag))]
sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(TransportDetailsControlBag.TransportInlandRailUserControl);
			yield return nameof(TransportDetailsControlBag.VesselUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
}
