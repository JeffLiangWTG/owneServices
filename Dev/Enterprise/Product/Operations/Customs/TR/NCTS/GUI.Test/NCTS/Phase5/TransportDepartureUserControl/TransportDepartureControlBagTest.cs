using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportDepartureControlBag))]
	sealed class TransportDepartureControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportDepartureControlBag.TankerStatusDropEdit);
			}
		}
		protected override ControlBag GetControlBagForTesting() => TransportDepartureControlBag.Instance;
	}
}
