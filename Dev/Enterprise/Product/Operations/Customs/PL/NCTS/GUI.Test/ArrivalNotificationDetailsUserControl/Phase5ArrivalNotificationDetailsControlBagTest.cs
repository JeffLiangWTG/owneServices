using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(Phase5ArrivalNotificationDetailsControlBag))]
sealed class Phase5ArrivalNotificationDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames => new[] { nameof(Phase5ArrivalNotificationDetailsControlBag.RepresentativeTraderGuidFindBox) };

	protected override ControlBag GetControlBagForTesting() => Phase5ArrivalNotificationDetailsControlBag.Instance;
}
