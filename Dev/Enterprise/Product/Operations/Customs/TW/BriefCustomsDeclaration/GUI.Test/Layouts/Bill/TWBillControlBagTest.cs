using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWBillControlBag))]
	sealed class TWBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TWBillControlBag.SequenceNumberCalcEdit);
				yield return nameof(TWBillControlBag.RemarksLongTextControl);
				yield return nameof(TWBillControlBag.ProcedureDropEdit);
				yield return nameof(TWBillControlBag.ManifestQtyCalcDropEdit);
				yield return nameof(TWBillControlBag.GrossWeightCalcDropEdit);
				yield return nameof(TWBillControlBag.GoodsValueConvertToLocalCurrencyControl);
				yield return nameof(TWBillControlBag.PortOfLoadingCodeFindBox);
				yield return nameof(TWBillControlBag.PortOfDischargeCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TWBillControlBag.Instance;
	}
}
