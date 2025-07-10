using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Invoicing.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPeriodicInvoicingStorageLinesCollection))]
	public class CYDPeriodicInvoicingStorageLinesCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDPeriodicInvoicingStorageLinesCollection>
	{
		#region Implementation

		protected override CYDPeriodicInvoicingStorageLinesCollection GetCollectionToTest()
		{
			return new CYDPeriodicInvoicingStorageLinesCollection(Factory.NewWithValidTestData<PeriodicInvoicing>());
		}

		#endregion
	}
}
