using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(NLTransportDetailsControlBag))]
sealed class NLTransportDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(NLTransportDetailsControlBag.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl);
			yield return nameof(NLTransportDetailsControlBag.ImportTransportInlandRoadUserControl);
			yield return nameof(NLTransportDetailsControlBag.FlightAndNationalityUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => NLTransportDetailsControlBag.Instance;
}
