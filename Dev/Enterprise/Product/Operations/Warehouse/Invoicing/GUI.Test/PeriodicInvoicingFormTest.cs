using System.Windows.Forms;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.GUI.Test
{
	[TestedType(typeof(PeriodicInvoicingForm))]
	public class PeriodicInvoicingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var invoicing = Factory.NewWithValidTestData<PeriodicInvoicing>();
			invoicing.HasChanges = false;
			return new PeriodicInvoicingForm(invoicing);
		}
	}
}
