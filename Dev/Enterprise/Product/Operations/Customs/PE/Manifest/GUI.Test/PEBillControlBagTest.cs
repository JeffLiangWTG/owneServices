using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.GUI.Testing
{
	[TestedType(typeof(PEBillControlBag))]
	sealed class PEBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PEBillControlBag.BillIssueDateEdit);
				yield return nameof(PEBillControlBag.CargoNatureDropEdit);
				yield return nameof(PEBillControlBag.CargoConditionDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => PEBillControlBag.Instance;
	}
}
