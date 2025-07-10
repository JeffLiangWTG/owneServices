using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	[TestedType(typeof(TRBillControlBag))]
	sealed class TRBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TRBillControlBag.RoRoCheckBox);
				yield return nameof(TRBillControlBag.PaymentTypeDropEdit);
				yield return nameof(TRBillControlBag.TransshipmentTypeDropEdit);
				yield return nameof(TRBillControlBag.BillStampDutyValueCalcEdit);
				yield return nameof(TRBillControlBag.AirBillStampDutyABSValueCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TRBillControlBag.Instance;
	}
}
