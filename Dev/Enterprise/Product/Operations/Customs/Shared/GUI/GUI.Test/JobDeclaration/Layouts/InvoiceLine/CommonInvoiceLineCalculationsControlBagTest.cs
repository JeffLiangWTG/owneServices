using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonInvoiceLineCalculationsControlBag))]
	sealed class CommonInvoiceLineCalculationsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonInvoiceLineCalculationsControlBag.CurrentInvoiceLabel);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.BalanceConvertToLocalCurrencyControl);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.LinesEnteredConvertToLocalCurrencyControl);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.LinesTotalConvertToLocalCurrencyControl);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.SummaryLabel);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.CIFConvertToLocalCurrencyControl);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.InsuranceInInvoiceCurrConvertToLocalCurrencyControl);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.FreightInInvoiceCurrConvertToLocalCurrencyControl);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.FOBConvertToLocalCurrencyControl);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl);
				yield return nameof(CommonInvoiceLineCalculationsControlBag.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonInvoiceLineCalculationsControlBag.Instance;
	}
}
