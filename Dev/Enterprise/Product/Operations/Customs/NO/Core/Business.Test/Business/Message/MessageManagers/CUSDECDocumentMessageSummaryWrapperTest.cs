using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSDECDocumentMessageSummaryWrapper))]
sealed class CUSDECDocumentMessageSummaryWrapperTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => _ = new CUSDECDocumentMessageSummaryWrapper(null, invoiceHeader));
		AssertExceptionThrown<ArgumentNullException>("When invoiceHeader is null", () => _ = new CUSDECDocumentMessageSummaryWrapper(entryHeader, null));
		AssertNoExceptionThrown("When happy path", () => _ = new CUSDECDocumentMessageSummaryWrapper(entryHeader, invoiceHeader));
	});

	public void TestItemDetailsCollection_Type() => CombineAssertions(() =>
	{
		var summary = GetTestSummary();
		var collection = summary.ItemDetailsCollection;
		AssertType<ImmutableArray<CUSDECItemDetailsWrapper>>(collection);
		AssertSame(collection, summary.ItemDetailsCollection);
	});

	public void TestItemDetailsCollection_Count() => CombineAssertions(() =>
	{
		AssertCount("When entry header has no entry line", 0);

		_ = entryHeader.AllEntryLines.AddNew();
		AssertCount("When entry header has one entry line", 1);

		_ = entryHeader.AllEntryLines.AddNew();
		AssertCount("When entry header has two entry lines", 2);

		void AssertCount(string message, int expected)
		{
			var collection = GetTestSummary().ItemDetailsCollection;
			AssertEquals(message, expected, collection.Count);
		}
	});

	public void TestItemDetailsCollection_Values() => CombineAssertions(() =>
	{
		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryHeader.AllEntryLines.AddNew().PK;
		invoiceLine2.JI_CL = entryHeader.AllEntryLines.AddNew().PK;
		invoiceLine1.JI_CountryOfOrigin = "SE";
		invoiceLine2.JI_CountryOfOrigin = "DK";
		var collection = GetTestSummary().ItemDetailsCollection;
		AssertEquals("[PRE-CONDITION] items count", 2, collection.Count);

		var item1 = collection.ElementAt(0);
		var item2 = collection.ElementAt(1);
		AssertType<CUSDECItemDetailsWrapper>("item 1", item1);
		AssertType<CUSDECItemDetailsWrapper>("item 2", item2);
		AssertEquals("item 1 country of origin", "SE", item1.CountryOfOrigin);
		AssertEquals("item 2 country of origin", "DK", item2.CountryOfOrigin);
	});

	public void TestInvoiceNumber()
	{
		invoiceHeader.JZ_InvoiceNumber = "420";
		var summary = GetTestSummary();
		AssertEquals("InvoiceNumber", "420", summary.InvoiceNumber);
	}

	public void TestInvoiceDate()
	{
		invoiceHeader.JZ_InvoiceDate = new (2024, 11, 7);
		var summary = GetTestSummary();
		AssertEquals("InvoiceDate", "20241107", summary.InvoiceDate);
	}

	CUSDECDocumentMessageSummaryWrapper GetTestSummary()
	{
		return new (entryHeader, invoiceHeader);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		invoiceHeader = declaration.Invoices.AddNew();
	}

	CusEntryHeader entryHeader;
	JobComInvoiceHeader invoiceHeader;
}
