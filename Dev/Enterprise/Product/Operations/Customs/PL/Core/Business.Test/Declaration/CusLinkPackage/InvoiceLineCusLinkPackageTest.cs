using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineCusLinkPackage))]
sealed class InvoiceLineCusLinkPackageTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var collection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
		var package = Factory.New<EU.Business.Declaration.Package>();

		var linkPackage = collection.AddNew();
		linkPackage.Package = package;
		linkPackage.IsLinked = true;

		return linkPackage;
	}

	public void TestIsPackQty_ReadOnly()
	{
		var linkPackage = (InvoiceLineCusLinkPackage)GetNewBusinessObject();
		var package = linkPackage.Package;

		CombineAssertions(() =>
		{
			linkPackage.IsLinked = false;
			AssertEquals("Quantity is readOnly when Islinked is false.", true, linkPackage.IsPackQty_ReadOnly);

			linkPackage.IsLinked = true;
			AssertEquals("Quantity is not readOnly when Islinked is true.", false, linkPackage.IsPackQty_ReadOnly);

			package.CW_PackType = "VQ";
			AssertEquals("Quantity is readOnly when Package type is one of Bulk Codes", true, linkPackage.IsPackQty_ReadOnly);

			package.CW_PackType = "A1";
			AssertEquals("Quantity is not readOnly when Package type is not a Bulk Code", false, linkPackage.IsPackQty_ReadOnly);
		});
	}
}
