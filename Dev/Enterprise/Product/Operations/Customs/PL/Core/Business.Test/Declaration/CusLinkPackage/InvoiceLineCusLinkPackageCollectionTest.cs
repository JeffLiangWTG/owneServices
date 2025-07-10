using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineCusLinkPackageCollection))]
sealed class InvoiceLineCusLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InvoiceLineCusLinkPackageCollection>
{
	protected override InvoiceLineCusLinkPackageCollection GetCollectionToTest()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		return new InvoiceLineCusLinkPackageCollection(invoiceLine);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		return new InvoiceLineCusLinkPackage(invoiceLine);
	}
}
