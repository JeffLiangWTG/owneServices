using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ImportInvoiceDetailsControlBag))]
sealed class ImportInvoiceDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ImportInvoiceDetailsControlBag.Instance.TranCircumstanceUserControl);
			yield return nameof(ImportInvoiceDetailsControlBag.Instance.ValuationMethodDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ImportInvoiceDetailsControlBag.Instance;
}
