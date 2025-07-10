using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLineCusLinkPackage))]
	internal sealed class InvoiceLineCusLinkPackageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var collection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var package = Factory.New<Package>();
			var linkPackage = collection.AddNew();
			linkPackage.Package = package;
			linkPackage.IsLinked = true;
			return linkPackage;
		}

		public void TestPackageNumber()
		{
			var npbo = (InvoiceLineCusLinkPackage)GetNewBusinessObject();
			var package = npbo.Package;
			package.CW_PackQty = 14;
			package.CW_PackType = "CTN";
			AssertEquals("14 CTN", npbo.PackageNumber);
			package.CW_MarksAndNos = "A1-A3";
			AssertEquals("A1-A3: 14 CTN", npbo.PackageNumber);
		}

		public void TestPackQty()
		{
			var npbo = (InvoiceLineCusLinkPackage)GetNewBusinessObject();
			npbo.PackQty = 0;
			npbo.IsLinked = true;
			AssertNoMessageErrors(npbo.PackQtyInfo);
		}
	}
}
