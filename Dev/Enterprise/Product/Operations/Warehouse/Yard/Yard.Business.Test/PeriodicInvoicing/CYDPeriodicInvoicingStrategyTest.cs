using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.Warehouse.Invoicing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPeriodicInvoicingStrategy))]
	public class CYDPeriodicInvoicingStrategyTest : PeriodicInvoicingStrategyTest
	{
		public CYDPeriodicInvoicingStrategyTest() : base(PeriodicInvoicingStorageTypes.Codes.ContainerYard)
		{
		}
	}
}
