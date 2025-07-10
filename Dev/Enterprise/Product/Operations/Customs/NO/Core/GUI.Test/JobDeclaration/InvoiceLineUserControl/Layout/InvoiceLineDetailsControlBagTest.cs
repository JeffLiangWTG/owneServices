using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsControlBag))]
sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceLineDetailsControlBag.CustomsRateOverrideUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.GoodsMarksLongTextControl);
			yield return nameof(InvoiceLineDetailsControlBag.MergeOverrideTextBox);
			yield return nameof(InvoiceLineDetailsControlBag.PackageTypeDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.ProcedureCodeDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.ReducedCustomsFlagDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.SupplementaryCode1DropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.SupplementaryCode2DropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.AdditionalSupplementaryCodesUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.RtRateOverrideCalcEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
}
