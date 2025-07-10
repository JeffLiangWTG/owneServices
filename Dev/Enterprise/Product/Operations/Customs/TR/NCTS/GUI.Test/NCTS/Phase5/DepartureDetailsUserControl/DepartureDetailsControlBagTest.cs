using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(DepartureDetailsControlBag))]
	sealed class DepartureDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DepartureDetailsControlBag.StampDutyStatusDropEdit);
				yield return nameof(DepartureDetailsControlBag.StampDutyCalcEdit);
				yield return nameof(DepartureDetailsControlBag.RegistrationDateEdit);
				yield return nameof(DepartureDetailsControlBag.GoodsShippingLocationAndGIKUserControl);
			}
		}
		protected override ControlBag GetControlBagForTesting() => DepartureDetailsControlBag.Instance;
	}
}
