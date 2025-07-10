using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESPackagingProviderTest : Customs.Business.Testing.DataProviderTestCase<AESPackagingProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Pivots", "Value cannot be null.\r\nParameter name: packagePivots", () => new AESPackagingProvider(null, null, 1));
			AssertExceptionThrown<ArgumentNullException>("Null Declaration", "Value cannot be null.\r\nParameter name: declaration", () => new AESPackagingProvider(entryLine.PackagingDetails.ToArray(), null, 1));
		});
	}

	public void TestSequenceNumber() => AssertEquals(999, GetProvider().SequenceNumber);

	public void TestTypeOfPackages() => AssertEquals("ABC", GetProvider().TypeOfPackages);

	public void TestNumberOfPackages()
	{
		CombineAssertions(() =>
		{
			package.CW_MarksAndNos = "EFG";
			AssertEquals("Empty NumberOfPackages", 0, GetProvider().NumberOfPackages);
			line1Package.PackQty = 1;
			line2Package.PackQty = 2;
			AssertEquals("Not empty NumberOfPackages", 3, GetProvider().NumberOfPackages);
			package.CW_PackType = "VQ";
			AssertNull("C0060 - Package is one of Bulk Codes", GetProvider().NumberOfPackages);
		});
	}

	public void TestShippingMarks()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty PackagingNumbersAndMarks", GetProvider().ShippingMarks);
			package.CW_MarksAndNos = "EFG";
			AssertEquals("Not empty PackagingNumbersAndMarks", "EFG", GetProvider().ShippingMarks);
		});
	}

	protected override AESPackagingProvider GetProvider() => new AESPackagingProvider(packagePivots, declaration, 999);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		package = declaration.Packages.AddNew();
		package.CW_PackType = "ABC";
		package.CW_PackQty = 4;

		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		line1Package = invoiceLine1.PackagesForInvoiceLinesForBindingOnly.AddNew();
		line1Package.Package = package;
		line1Package.IsLinked = true;
		line1Package.PackQty = 0;

		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		line2Package = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
		line2Package.Package = package;
		line2Package.IsLinked = true;
		line2Package.PackQty = 0;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;

		var lineMerger = new Declaration.LineMerger(declaration);
		lineMerger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders.Single();
		entryLine = entryHeader.AllEntryLines.FirstOrDefault();
		packagePivots = new[] { entryLine.PackagingDetails.First(), entryLine.PackagingDetails.Last() };
	}

	BasePackage package;
	BaseCusLinkPackage line1Package;
	BaseCusLinkPackage line2Package;
	Declaration.CusEntryLine entryLine;
	InvoiceLinePackagePivot[] packagePivots;
	JobDeclaration declaration;
}
