using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(TransportDetailsControlBag))]
	sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportDetailsControlBag.TransportDetailsFlightUserControl);
				yield return nameof(TransportDetailsControlBag.TransportDetailsNationalityUserControl);
				yield return nameof(TransportDetailsControlBag.TransportDetailsVoyageUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
	}
}
