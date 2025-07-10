using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeControlBag))]
	sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentTypeControlBag.EntrySubStyleDropEdit);
				yield return nameof(ShipmentTypeControlBag.EntryDateForDutyDateEdit);
				yield return nameof(ShipmentTypeControlBag.BankCodeFindBox);
				yield return nameof(ShipmentTypeControlBag.DutyPaymentTypeDropEdit);
				yield return nameof(ShipmentTypeControlBag.InspectionClerkTextBox);
				yield return nameof(ShipmentTypeControlBag.GoodsAtCustomsAreaCheckBox);
				yield return nameof(ShipmentTypeControlBag.OverTimePaymentCompletedCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;
	}
}
