using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(TWBillControlBag))]
	sealed class TWBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TWBillControlBag.PortOfLoadingCodeFindBox);
				yield return nameof(TWBillControlBag.GoodsDescriptionLongTextControl);
				yield return nameof(TWBillControlBag.BagNumberDropEdit);
				yield return nameof(TWBillControlBag.ManifestQtyCalcDropEdit);
				yield return nameof(TWBillControlBag.SplitQuantityCalcDropEdit);
				yield return nameof(TWBillControlBag.MarksAndNumbersLongTextControl);
				yield return nameof(TWBillControlBag.TariffFindBox);
				yield return nameof(TWBillControlBag.DGUNNOCodeFindBox);
				yield return nameof(TWBillControlBag.IsEscortRequiredCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TWBillControlBag.Instance;
	}
}
