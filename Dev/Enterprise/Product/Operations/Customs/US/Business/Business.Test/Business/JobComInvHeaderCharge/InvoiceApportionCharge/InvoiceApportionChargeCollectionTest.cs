using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceApportionChargeCollection))]
	sealed class InvoiceApportionChargeCollectionTest : SubsetBusinessObjectCollectionTestCase<InvoiceApportionChargeCollection, InvoiceApportionCharge>
	{
		protected override InvoiceApportionChargeCollection GetCollectionToTest()
		{
			var invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			return invoice.GroupCharges;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<InvoiceApportionCharge>();
	}
}
