using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

class AttachmentProviderTest : Customs.Business.Testing.DataProviderTestCase<AttachmentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObjectParent", "Value cannot be null.\r\nParameter name: messageSendingObjectParent", () => new AttachmentProvider(null));
	}

	public void TestNumber() => AssertNotNullOrEmpty(GetProvider().Number);

	public void TestReferenceNumber()
	{
		CombineAssertions(() =>
		{
			sendingObjectParent.PurposeOfSending = MessageSendingPurposeOfSendingList.Codes._0;
			AssertEquals("Empty ReferenceNumber", string.Empty, GetProvider().ReferenceNumber);

			sendingObjectParent.RefNumber = "asd";
			AssertEquals("Not Empty ReferenceNumber", "asd", GetProvider().ReferenceNumber);

			sendingObjectParent.PurposeOfSending = MessageSendingPurposeOfSendingList.Codes._1;
			AssertEquals("PurposeOfSending is not 'Wymagania Formalne'", string.Empty, GetProvider().ReferenceNumber);
		});
	}

	public void TestReferenceNumber2()
	{
		CombineAssertions(() =>
		{
			sendingObjectParent.PurposeOfSending = MessageSendingPurposeOfSendingList.Codes._1;
			AssertEquals("Empty ReferenceNumber", string.Empty, GetProvider().ReferenceNumber2);

			sendingObjectParent.MrnNumber = "abcdef";
			AssertEquals("Not Empty ReferenceNumber2", "abcdef", GetProvider().ReferenceNumber2);

			sendingObjectParent.PurposeOfSending = MessageSendingPurposeOfSendingList.Codes._0;
			AssertEquals("PurposeOfSending is not 'Na wezwanie urzedu'", string.Empty, GetProvider().ReferenceNumber2);
		});
	}

	public void TestOffice()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty Office", string.Empty, GetProvider().Office);

			sendingObjectParent.CustomsOffice = "PL01";
			AssertEquals("Not Empty Office", "01", GetProvider().Office);
		});
	}

	public void TestSystem()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export declaration", "Eksport", GetProvider().System);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import declaration", "Import", GetProvider().System);
		});
	}

	public void TestPurposeOfSending()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty PurposeOfSending", string.Empty, GetProvider().PurposeOfSending);

			sendingObjectParent.PurposeOfSending = MessageSendingPurposeOfSendingList.Codes._0;
			AssertEquals("Not Empty PurposeOfSending", MessageSendingPurposeOfSendingList.Codes._0, GetProvider().PurposeOfSending);
		});
	}

	public void TestProcedure()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty Procedure", string.Empty, GetProvider().Procedure);

			sendingObjectParent.Procedure = "zxc";
			AssertEquals("Not Empty Procedure", "zxc", GetProvider().Procedure);
		});
	}

	public void TestComments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty Comments", string.Empty, GetProvider().Comments);

			sendingObjectParent.Comments = "some kind of comment";
			AssertEquals("Not Empty Comments", "some kind of comment", GetProvider().Comments);
		});
	}

	public void TestFiles()
	{
		CombineAssertions(() =>
		{
			AssertNotNull("Empty list", GetProvider().Files);
			AssertEquals("0 eDoc on List", 0, GetProvider().Files.Count);

			sendingObjectParent.EDocs.AddNew();
			AssertEquals("1 eDoc on List", 1, GetProvider().Files.Count);

			sendingObjectParent.EDocs.AddNew();
			AssertEquals("2 eDocs on List", 2, GetProvider().Files.Count);
		});
	}

	protected override AttachmentProvider GetProvider() => new AttachmentProvider(sendingObjectParent);

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var lineMerger = new Declaration.LineMerger(declaration);
		lineMerger.DoMerge();
		sendingObjectParent = new CustomsDeclarationMessageSendingObjectParent(declaration);
	}

	JobDeclaration declaration;
	CustomsDeclarationMessageSendingObjectParent sendingObjectParent;
}
