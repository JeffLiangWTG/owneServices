using System.Windows.Forms;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.GUI.Test
{
	[TestedType(typeof(PeriodicInvoicingMultiClientForm))]
	public class PeriodicInvoicingMultiClientFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PeriodicInvoicingMultiClientForm(new PeriodicInvoicingMultiClientInvoice(PeriodicInvoicingStorageTypes.Codes.ContainerYard));
		}
	}
}
