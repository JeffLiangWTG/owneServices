using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(TWBillPartiesControlBag))]
	sealed class TWBillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(TWBillPartiesControlBag.ShipperAddressUserControl));
				yield return (nameof(TWBillPartiesControlBag.ConsigneeAddressUserControl));
				yield return (nameof(TWBillPartiesControlBag.NotifyPartyAddressUserControl));
			}
		}

		protected override ControlBag GetControlBagForTesting() => TWBillPartiesControlBag.Instance;
	}
}
