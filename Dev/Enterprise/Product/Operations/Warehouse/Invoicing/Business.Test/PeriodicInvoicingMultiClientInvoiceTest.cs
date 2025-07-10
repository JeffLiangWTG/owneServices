using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	[TestedType(typeof(PeriodicInvoicingMultiClientInvoice))]
	public class PeriodicInvoicingMultiClientInvoiceTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PeriodicInvoicingMultiClientInvoice(PeriodicInvoicingStorageTypes.Codes.ContainerYard);
		}
	}
}
