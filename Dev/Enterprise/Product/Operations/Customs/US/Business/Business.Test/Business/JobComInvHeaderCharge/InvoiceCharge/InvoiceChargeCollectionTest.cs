using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceChargeCollection))]
	sealed class InvoiceChargeCollectionTest : SubsetBusinessObjectCollectionTestCase<InvoiceChargeCollection, InvoiceCharge>
	{
		protected override InvoiceChargeCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return invoice.Charges;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<InvoiceCharge>();
	}
}
