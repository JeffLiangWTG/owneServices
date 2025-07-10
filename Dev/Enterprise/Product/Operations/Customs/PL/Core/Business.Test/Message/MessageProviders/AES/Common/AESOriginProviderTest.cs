using System;
using System.Linq;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESOriginProviderTest : Customs.Business.Testing.DataProviderTestCase<AESOriginProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null InvoiceLine", "Value cannot be null.\r\nParameter name: invoiceLine", () => new AESOriginProvider(null));
	}

	public void TestCountryOfOrigin()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty declaration", string.Empty, GetProvider().CountryOfOrigin);

			invoiceLine.JI_CountryOfOrigin = CountryCodes.Poland;
			AssertEquals("Not empty JI_CountryOfOrigin", CountryCodes.Poland, GetProvider().CountryOfOrigin);
		});
	}

	public void TestRegionOfDispatch() => AssertNull(GetProvider().RegionOfDispatch);

	protected override AESOriginProvider GetProvider() => new AESOriginProvider(entryLine.RandomLine);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		instruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_Description = "invoice line 1";

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		entryHeader = declaration.CustomsEntryHeaders.First();
		entryLine = entryHeader.AllEntryLines.FirstOrDefault();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	CusEntryInstruction instruction;
}
