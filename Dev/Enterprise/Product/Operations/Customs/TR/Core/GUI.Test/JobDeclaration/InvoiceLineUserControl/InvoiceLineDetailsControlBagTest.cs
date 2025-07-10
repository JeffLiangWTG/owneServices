using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsControlBag))]
	sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		public void TestSingletonType()
		{
			AssertType<InvoiceLineDetailsControlBag>(InvoiceLineDetailsControlBag.Instance);
		}

		public void TestTemplateControlType()
		{
			using (var templateControl = new InvoiceLineDetailsControlBag().TemplateControl)
			{
				AssertType<InvoiceLineDetailsUserControl>(templateControl);
			}
		}

		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineDetailsControlBag.SupplementaryCode1DropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.SupplementaryCode2DropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.BrandNameTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.PartNoCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
	}
}
