using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWBillPartiesControlBag))]
	sealed class TWBillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TWBillPartiesControlBag.ShipperAddressUserControl);
				yield return nameof(TWBillPartiesControlBag.ConsigneeAddressUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TWBillPartiesControlBag.Instance;
	}
}
