using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLineCusLinkPackageCollection))]
	sealed class InvoiceLineCusLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InvoiceLineCusLinkPackageCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

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

		public void TestShouldAddItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
			AssertEquals(0, invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(0, invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(0, invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Count);
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			AssertEquals(3, invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(3, invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(3, invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Count);
			invoiceLine1.ToggleLinkageWithPackage(package1, true);
			AssertEquals(3, invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(3, invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(2, invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Count);
			invoiceLine2.ToggleLinkageWithPackage(package2, true);
			invoiceLine2.ToggleLinkageWithPackage(package3, true);
			AssertEquals(3, invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(3, invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(0, invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Count);
			invoiceLine2.ToggleLinkageWithPackage(package3, false);
			AssertEquals(3, invoiceLine1.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(3, invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Count);
			AssertEquals(1, invoiceLine3.PackagesForInvoiceLinesForBindingOnly.Count);
		}
	}
}
