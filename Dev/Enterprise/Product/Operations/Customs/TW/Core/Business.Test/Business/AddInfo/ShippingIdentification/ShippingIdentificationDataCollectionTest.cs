using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ShippingIdentificationDataCollection))]
	sealed class ShippingIdentificationDataCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLines = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			invoiceLines.ShippingIdentificationDataCollection.AddNew();
			return invoiceLines.ShippingIdentificationDataCollection;
		}
	}
}
