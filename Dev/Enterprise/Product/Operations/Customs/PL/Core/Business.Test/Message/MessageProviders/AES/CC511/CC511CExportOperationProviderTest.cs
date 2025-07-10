using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC511CExportOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC511CExportOperationProvider>
{
	public void TestLRN() => AssertEquals("ABC", Provider.LRN);

	public void TestStoringFlag() => CombineAssertions(() =>
	{
		AssertEquals("The value is not assigned", null, GetProvider().StoringFlag);

		jobDeclaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
		jobDeclaration.JE_CustomsOffice = "asd";
		jobDeclaration.JE_OfficeOfEntryExit = "asd";
		entryInstruction.ZG_ExportManifest = false;
		AssertEquals("Export Manifest is false", false, GetProvider().StoringFlag);

		entryInstruction.ZG_ExportManifest = true;
		AssertEquals("Export Manifest is true", true, GetProvider().StoringFlag);

		jobDeclaration.JE_OfficeOfEntryExit = "zxc";
		AssertNull("Export Manifest different JE_OfficeOfEntryExit and GoodsLocationCustomsOffice", GetProvider().StoringFlag);

		jobDeclaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;
		AssertNull("Export Manifest JE_LocationQualifier is not office type", GetProvider().StoringFlag);
	});

	protected override CC511CExportOperationProvider GetProvider() => new CC511CExportOperationProvider(entryHeader);

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var invoice = jobDeclaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		entryHeader = jobDeclaration.CustomsEntryHeaders.Single();
		entryHeader.EntryNumber = "1234";
		entryHeader.CH_BGMReference = "ABC";

		sendingObject = new BaseMessageSendingObject(entryHeader);
		sendingObject.AmendmentInvalidationReason = "Something something";
	}

	JobDeclaration jobDeclaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	BaseMessageSendingObject sendingObject;
}
