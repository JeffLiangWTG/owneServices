using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	[TestedType(typeof(PeriodicInvoicingCollection))]
	public class PeriodicInvoicingCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PeriodicInvoicingCollection(Factory, PeriodicInvoicingStorageTypes.Codes.ContainerYard);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoicing = (PeriodicInvoicing)base.GetNewElementToAddToTheCollection();
			invoicing.ET_StorageType = PeriodicInvoicingStorageTypes.Codes.ContainerYard;
			return invoicing;
		}
	}
}
