using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(Package))]
class PackageTest : BasePackageTest
{
	public void TestPackageTypeCreatedByFactory()
	{
		AssertType<Package>(Factory.New<BasePackage>());
	}

	public void TestCW_PackQty_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var package = declaration.Packages.AddNew();

		CombineAssertions(() =>
		{
			package.CW_PackType = "1A";
			AssertEquals("Package PackQty should not be readonly", false, package.CW_PackQtyInfo.ReadOnly);

			package.CW_PackType = "VQ";
			AssertEquals("Package CW_PackQty should be readonly, package is bulk code", true, package.CW_PackQtyInfo.ReadOnly);
		});
	}

	public void TestUpdateInvoiceLinePivotQuantityForBulkCodes()
	{
		var declaration = Factory.New<JobDeclaration>();

		var package = declaration.Packages.AddNew();
		var invoiceLinePackage1 = package.InvoiceLinePivotCollection.AddNew();
		var invoiceLinePackage2 = package.InvoiceLinePivotCollection.AddNew();
		var invoiceLinePackage3 = package.InvoiceLinePivotCollection.AddNew();

		CombineAssertions(() =>
		{
			package.CW_PackQty = 3;

			invoiceLinePackage1.CHC_NumberOfPacks = 1;
			invoiceLinePackage2.CHC_NumberOfPacks = 2;
			invoiceLinePackage3.CHC_NumberOfPacks = 0;
			package.CW_PackType = "1A";

			AssertEquals("Package PackQty should still be 3", 3, package.CW_PackQty);
			AssertEquals("invoiceLinePackage1 CHC_NumberOfPacks should be 1", 1, invoiceLinePackage1.CHC_NumberOfPacks);
			AssertEquals("invoiceLinePackage2 CHC_NumberOfPacks should be 2", 2, invoiceLinePackage2.CHC_NumberOfPacks);
			AssertEquals("invoiceLinePackage3 CHC_NumberOfPacks should be 0", 0, invoiceLinePackage3.CHC_NumberOfPacks);

			package.CW_PackType = "VQ";

			AssertEquals("Package PackQty should be 0, package is bulk code", 0, package.CW_PackQty);
			AssertEquals("invoiceLinePackage1 CHC_NumberOfPacks should be 0, package is bulk code", 0, invoiceLinePackage1.CHC_NumberOfPacks);
			AssertEquals("invoiceLinePackage2 CHC_NumberOfPacks should be 0, package is bulk code", 0, invoiceLinePackage2.CHC_NumberOfPacks);
			AssertEquals("invoiceLinePackage3 CHC_NumberOfPacks should be 0, package is bulk code", 0, invoiceLinePackage3.CHC_NumberOfPacks);
		});
	}

	public void TestValidationType()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var package = declaration.Packages.AddNew();
			AssertType<ExportPackageValidation>("Validation Type when Export declaration", package.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<PackageValidation>("Validation updated when declaration type changed", package.Validation);

			AssertType<PackageValidation>("Default Validation Type", Factory.New<Package>().Validation);
		});
	}
}
