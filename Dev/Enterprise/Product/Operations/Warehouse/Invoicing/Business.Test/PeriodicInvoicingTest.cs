using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	[TestedType(typeof(PeriodicInvoicing))]
	public class PeriodicInvoicingTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoicing = Factory.NewWithValidTestData<PeriodicInvoicing>();
			return invoicing;
		}
	}
}
