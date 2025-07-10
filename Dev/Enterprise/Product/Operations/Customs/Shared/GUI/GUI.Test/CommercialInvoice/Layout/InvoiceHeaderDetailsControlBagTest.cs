using System.Collections.Generic;
using Enterprise.Customs.GUI.CommercialInvoice;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(InvoiceHeaderDetailsControlBag))]
	sealed class InvoiceHeaderDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(InvoiceHeaderDetailsControlBag.InvoiceNumberBoundTextBox));
				yield return (nameof(InvoiceHeaderDetailsControlBag.InvoiceDateEdit));
				yield return (nameof(InvoiceHeaderDetailsControlBag.InvoiceAmountCalcFindBox));
				yield return (nameof(InvoiceHeaderDetailsControlBag.InvoiceCurrExRateCalcEdit));
				yield return (nameof(InvoiceHeaderDetailsControlBag.IncotermAndIncotermPlaceUserControl));
				yield return (nameof(InvoiceHeaderDetailsControlBag.ValuationCodeDropEdit));
				yield return (nameof(InvoiceHeaderDetailsControlBag.GrossWeightCalcDropEdit));
				yield return (nameof(InvoiceHeaderDetailsControlBag.NetWeightCalcDropEdit));
				yield return (nameof(InvoiceHeaderDetailsControlBag.InvoiceCurrLandedCostExRateCalcEdit));
				yield return (nameof(InvoiceHeaderDetailsControlBag.NoOfPacksCalcDropEdit));
			}
		}
		protected override ControlBag GetControlBagForTesting() => InvoiceHeaderDetailsControlBag.Instance;
	}
}
