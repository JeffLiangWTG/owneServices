using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeControlBag))]
	sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.ContainerModeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.MessageTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox);
				yield return nameof(ShipmentTypeControlBag.Instance.CustomsProfileDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.TransportModeDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.InlandModeOfTransportDropEdit);
				yield return nameof(ShipmentTypeControlBag.Instance.DeclarantTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;
	}
}
