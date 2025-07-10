using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	[TestedType(typeof(PeriodicInvoicingInvoicingSupporter))]
	public class PeriodicInvoicingInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var periodicInvoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
			return periodicInvoice;
		}
	}
}
