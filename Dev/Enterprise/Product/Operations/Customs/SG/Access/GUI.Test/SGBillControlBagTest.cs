using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(SGBillControlBag))]
	sealed class SGBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SGBillControlBag.CycleNumberDropEditWithFixedWidth);
				yield return nameof(SGBillControlBag.CycleDateDateEdit);
				yield return nameof(SGBillControlBag.SGPayeeIndicatorDropEdit);
				yield return nameof(SGBillControlBag.SGPartyStatusDropEdit);
				yield return nameof(SGBillControlBag.SGPartyIDTextBox);
				yield return nameof(SGBillControlBag.SGGstAmountCalcEdit);
				yield return nameof(SGBillControlBag.SGDutyAmountCalcEdit);
				yield return nameof(SGBillControlBag.MessageStatusDescriptionTextBox);
				yield return nameof(SGBillControlBag.GSTNReferenceNoTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SGBillControlBag.Instance;
	}
}
