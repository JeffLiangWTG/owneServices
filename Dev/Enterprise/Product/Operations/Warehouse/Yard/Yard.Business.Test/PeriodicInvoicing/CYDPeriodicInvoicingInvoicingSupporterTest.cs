using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Invoicing.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPeriodicInvoicingInvoicingSupporter))]
	public class CYDPeriodicInvoicingInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<PeriodicInvoicing>();
		}
	}
}
