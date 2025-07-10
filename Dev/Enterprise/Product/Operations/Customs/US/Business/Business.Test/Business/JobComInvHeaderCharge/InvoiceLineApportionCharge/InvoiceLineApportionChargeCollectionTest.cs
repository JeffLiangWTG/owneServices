using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceLineApportionChargeCollection))]
	sealed class InvoiceLineApportionChargeCollectionTest : SubsetBusinessObjectCollectionTestCase<InvoiceLineApportionChargeCollection, InvoiceLineApportionCharge>
	{
		protected override InvoiceLineApportionChargeCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.ApportionedCharges;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<InvoiceLineApportionCharge>();
	}
}
