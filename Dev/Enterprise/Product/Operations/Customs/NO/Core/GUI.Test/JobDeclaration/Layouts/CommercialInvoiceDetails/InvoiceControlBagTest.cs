using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(InvoiceControlBag))]
	sealed class InvoiceControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceControlBag.InvoiceDateDateEdit);
				yield return nameof(InvoiceControlBag.ExchangeRatePlusFixedRateUserControl);
				yield return nameof(InvoiceControlBag.ValuationMethodDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceControlBag.Instance;
	}
}
