using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC511GoodsShipmentProviderTest : DataProviderTestCase<CC511GoodsShipmentProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null EntryHeader", "Value cannot be null.\r\nParameter name: entryHeader",
			() => new CC511GoodsShipmentProvider(null));
	});

	public void TestConsignment() => AssertNotNull(Provider.Consignment);

	protected override CC511GoodsShipmentProvider GetProvider() => new CC511GoodsShipmentProvider(entryHeader);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Description = "invoice line 1";

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	CusEntryInstruction entryInstruction;
}
