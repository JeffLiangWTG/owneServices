using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.GUI.Testing
{
	[TestedType(typeof(ZABillControlBag))]
	sealed class ZABillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ZABillControlBag.CargoReleaseStatusOtherDescriptionTextBox);
				yield return nameof(ZABillControlBag.CargoReleaseStatusDropEdit);
				yield return nameof(ZABillControlBag.CaseNumberBillsGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ZABillControlBag.Instance;
	}
}
