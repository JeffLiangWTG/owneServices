using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.GUI.Testing
{
	[TestedType(typeof(ACEBillControlBag))]
	sealed class ACEBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ACEBillControlBag.FDAIndicatorCheckBox);
				yield return nameof(ACEBillControlBag.BillStatusTextBox);
				yield return nameof(ACEBillControlBag.BillStatusDescriptionTextBox);
				yield return nameof(ACEBillControlBag.GoodsValueConvertToLocalCurrencyControl);
				yield return nameof(ACEBillControlBag.EntryNumberTypeDropEdit);
				yield return nameof(ACEBillControlBag.EntryNumberTextBox);
				yield return nameof(ACEBillControlBag.GoodsOriginCodeFindBox);
				yield return nameof(ACEBillControlBag.TariffCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ACEBillControlBag.Instance;
	}
}
