using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsControlBag))]
	sealed class ShipmentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsFinalDestinationUserControl);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsGoodsLocationUserControl);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsOriginUserControl);
				yield return nameof(ShipmentDetailsControlBag.ShipmentDetailsWeightAndVolumeUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentDetailsControlBag.Instance;
	}
}
