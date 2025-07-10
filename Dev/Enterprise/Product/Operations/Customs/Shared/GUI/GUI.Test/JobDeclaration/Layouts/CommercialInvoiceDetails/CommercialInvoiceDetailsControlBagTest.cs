using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceDetailsControlBag))]
	sealed class CommercialInvoiceDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommercialInvoiceDetailsControlBag.InvoiceNumberTextBox);
				yield return nameof(CommercialInvoiceDetailsControlBag.GroupInvoiceDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.InvoiceAmountConvertToLocalCurrencyControl);
				yield return nameof(CommercialInvoiceDetailsControlBag.InvoiceCurrExRateCalcEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.IncoTermsUserControl);
				yield return nameof(CommercialInvoiceDetailsControlBag.IncoTermPlaceTextBox);
				yield return nameof(CommercialInvoiceDetailsControlBag.AdditionalTermsTextBox);
				yield return nameof(CommercialInvoiceDetailsControlBag.GrossWeightCalcDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.NetWeightCalcDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.InvoiceCurrLandedCostExRateCalcEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.NoOfPacksCalcDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.InvoiceDateEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.ValuationCodeDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.UCRTextBox);
				yield return nameof(CommercialInvoiceDetailsControlBag.ExporterAddressControl);
				yield return nameof(CommercialInvoiceDetailsControlBag.IncoTermDropEdit);
				yield return nameof(CommercialInvoiceDetailsControlBag.PaymentMethodDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommercialInvoiceDetailsControlBag.Instance;
	}
}
