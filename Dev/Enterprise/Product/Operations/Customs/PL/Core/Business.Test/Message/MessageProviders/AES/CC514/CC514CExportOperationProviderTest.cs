using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC514CExportOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC514CExportOperationProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC514CExportOperationProvider(null));
	}

	public void TestLRN() => AssertEquals("ABC", Provider.LRN);

	public void TestMRN() => AssertEquals("1234", Provider.MRN);

	public void TestInvalidationRequestDateAndTime() => AssertNotNull(Provider.InvalidationRequestDateAndTime);

	public void TestInvalidationReason() => AssertEquals("Something something", Provider.InvalidationReason);

	protected override CC514CExportOperationProvider GetProvider() => new CC514CExportOperationProvider(sendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		entryHeader = declaration.CustomsEntryHeaders.Single();
		entryHeader.MovementReferenceNumberSetter("1234", ZDateTime.BrettsBirthday);
		entryHeader.CH_BGMReference = "ABC";

		sendingObject = new BaseMessageSendingObject(entryHeader);
		sendingObject.AmendmentInvalidationReason = "Something something";
	}

	CusEntryHeader entryHeader;
	BaseMessageSendingObject sendingObject;
}
