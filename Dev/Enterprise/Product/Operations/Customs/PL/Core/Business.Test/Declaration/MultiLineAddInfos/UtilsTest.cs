using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class UtilsTest : TestCaseWithFactory
{
	public void TestHasSupportingDocumentCodes_InvoiceLine() => TestHasSupportingDocumentCodes(invoiceLine.AdditionalInfos.AddNew());

	public void TestHasSupportingDocumentCodes_Invoice() => TestHasSupportingDocumentCodes(invoice.AdditionalInfos.AddNew());

	public void TestHasSupportingDocumentCodes_Declaration() => TestHasSupportingDocumentCodes(declaration.AdditionalInfos.AddNew());

	public void TestHasSupportingDocumentCodes_EntryInstruction() => TestHasSupportingDocumentCodes(entryInstruction.AdditionalInfos.AddNew());

	public void TestHasSupportingDocumentCodes(CusSupportingInfo parentInfo)
	{
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
		var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
		var invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		var entryInstructionSupportingDocument = entryInstruction.SupportingDocuments.AddNew();

		ZString[] searchedSupportingCodes = ["01", "02"];
		CombineAssertions(() =>
		{
			AssertEquals("No supporting document with searched codes", false, parentInfo.HasSupportingDocumentCodes(searchedSupportingCodes));

			foreach (var supportingDocument in searchedSupportingCodes)
			{
				declarationSupportingDocument.CSI_Code = supportingDocument;
				AssertEquals($"Supporting document with code {supportingDocument} specified on declaration"
					, true, parentInfo.HasSupportingDocumentCodes(searchedSupportingCodes));

				declarationSupportingDocument.CSI_Code = ZString.Empty;
				invoiceSupportingDocument.CSI_Code = supportingDocument;
				AssertEquals($"Supporting document with code {supportingDocument} specified on invoice"
					, true, parentInfo.HasSupportingDocumentCodes(searchedSupportingCodes));

				invoiceSupportingDocument.CSI_Code = ZString.Empty;
				invoiceLineSupportingDocument.CSI_Code = supportingDocument;
				AssertEquals($"Supporting document with code {supportingDocument} specified on invoiceLine"
					, true, parentInfo.HasSupportingDocumentCodes(searchedSupportingCodes));

				invoiceLineSupportingDocument.CSI_Code = ZString.Empty;
				entryInstructionSupportingDocument.CSI_Code = supportingDocument;
				AssertEquals($"Supporting document with code {supportingDocument} specified on entryInstruction"
					, true, parentInfo.HasSupportingDocumentCodes(searchedSupportingCodes));
			}

			entryInstructionSupportingDocument.CSI_Code = "123";
			AssertEquals("Random code exist, but not searched ones", false, parentInfo.HasSupportingDocumentCodes(searchedSupportingCodes));
		});
	}

	public void TestHasSupportingDocumentCode_InvoiceLine() => TestHasSupportingDocumentCode(invoiceLine.AdditionalInfos.AddNew());

	public void TestHasSupportingDocumentCode_Invoice() => TestHasSupportingDocumentCode(invoice.AdditionalInfos.AddNew());

	public void TestHasSupportingDocumentCode_Declaration() => TestHasSupportingDocumentCode(declaration.AdditionalInfos.AddNew());

	public void TestHasSupportingDocumentCode_EntryInstruction() => TestHasSupportingDocumentCode(entryInstruction.AdditionalInfos.AddNew());

	public void TestHasSupportingDocumentCode(CusSupportingInfo parentInfo)
	{
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
		var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
		var invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		var entryInstructionSupportingDocument = entryInstruction.SupportingDocuments.AddNew();

		var supportingCode = "01";
		CombineAssertions(() =>
		{
			AssertEquals("No supporting document with searched codes", false, parentInfo.HasSupportingDocumentCode(supportingCode));

			declarationSupportingDocument.CSI_Code = supportingCode;
			AssertEquals($"Supporting document with code 01 specified on declaration", true, parentInfo.HasSupportingDocumentCode(supportingCode));

			declarationSupportingDocument.CSI_Code = ZString.Empty;
			invoiceSupportingDocument.CSI_Code = supportingCode;
			AssertEquals($"Supporting document with code 01 specified on invoice", true, parentInfo.HasSupportingDocumentCode(supportingCode));

			invoiceSupportingDocument.CSI_Code = ZString.Empty;
			invoiceLineSupportingDocument.CSI_Code = supportingCode;
			AssertEquals($"Supporting document with code 01 specified on invoiceLine", true, parentInfo.HasSupportingDocumentCode(supportingCode));

			invoiceLineSupportingDocument.CSI_Code = ZString.Empty;
			entryInstructionSupportingDocument.CSI_Code = supportingCode;
			AssertEquals($"Supporting document with code 01 specified on entryInstruction", true, parentInfo.HasSupportingDocumentCode(supportingCode));

			entryInstructionSupportingDocument.CSI_Code = "123";
			AssertEquals("Random code exist, but not searched one", false, parentInfo.HasSupportingDocumentCode(supportingCode));
		});
	}

	public void TestHasAdditionalDocumentCode_DocumentCodeOnly_InvoiceLine() => TestHasAdditionalDocumentCode_DocumentCodeOnly(invoiceLine.SupportingDocuments.AddNew());

	public void TestHasAdditionalDocumentCode_DocumentCodeOnly_Invoice() => TestHasAdditionalDocumentCode_DocumentCodeOnly(invoice.SupportingDocuments.AddNew());

	public void TestHasAdditionalDocumentCode_DocumentCodeOnly_Declaration() => TestHasAdditionalDocumentCode_DocumentCodeOnly(declaration.SupportingDocuments.AddNew());

	public void TestHasAdditionalDocumentCode_DocumentCodeOnly_EntryInstruction() => TestHasAdditionalDocumentCode_DocumentCodeOnly(entryInstruction.SupportingDocuments.AddNew());

	public void TestHasAdditionalDocumentCode_DocumentCodeOnly(CusSupportingInfo parentInfo)
	{
		var declarationAdditionalDocument = declaration.AdditionalInfos.AddNew();
		var invoiceAdditionalDocumentt = invoice.AdditionalInfos.AddNew();
		var invoiceLineAdditionalDocument = invoiceLine.AdditionalInfos.AddNew();
		var entryInstructionSupportingDocument = entryInstruction.AdditionalInfos.AddNew();

		var documentCode = "C1";
		CombineAssertions("DocumentCodeOnly", () =>
		{
			AssertEquals("No Additional Document with searched codes", false, parentInfo.HasAdditionalDocumentCode(documentCode));

			declarationAdditionalDocument.CSI_Code = documentCode;
			AssertEquals($"Additional Document with code 01 specified on declaration", true, parentInfo.HasAdditionalDocumentCode(documentCode));

			declarationAdditionalDocument.CSI_Code = ZString.Empty;
			invoiceAdditionalDocumentt.CSI_Code = documentCode;
			AssertEquals($"Additional Document with code 01 specified on invoice", true, parentInfo.HasAdditionalDocumentCode(documentCode));

			invoiceAdditionalDocumentt.CSI_Code = ZString.Empty;
			invoiceLineAdditionalDocument.CSI_Code = documentCode;
			AssertEquals($"Additional Document with code 01 specified on invoiceLine", true, parentInfo.HasAdditionalDocumentCode(documentCode));

			invoiceLineAdditionalDocument.CSI_Code = ZString.Empty;
			entryInstructionSupportingDocument.CSI_Code = documentCode;
			AssertEquals($"Additional Document with code 01 specified on entryInstruction", true, parentInfo.HasAdditionalDocumentCode(documentCode));

			entryInstructionSupportingDocument.CSI_Code = "123";
			AssertEquals("Random code exist, but not searched one", false, parentInfo.HasAdditionalDocumentCode(documentCode));
		});
	}

	public void TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType_InvoiceLine() => TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType(invoiceLine.SupportingDocuments.AddNew());

	public void TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType_Invoice() => TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType(invoice.SupportingDocuments.AddNew());

	public void TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType_Declaration() => TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType(declaration.SupportingDocuments.AddNew());

	public void TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType_EntryInstruction() => TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType(entryInstruction.SupportingDocuments.AddNew());

	public void TestHasAdditionalDocumentCode_DocumentCodeWithDocumenType(CusSupportingInfo parentInfo)
	{
		var declarationAdditionalDocument = declaration.AdditionalInfos.AddNew();
		var invoiceAdditionalDocumentt = invoice.AdditionalInfos.AddNew();
		var invoiceLineAdditionalDocument = invoiceLine.AdditionalInfos.AddNew();
		var entryInstructionAdditionalDocument = entryInstruction.AdditionalInfos.AddNew();

		var documentCode = "C1";
		var documentType = AdditionalInfoKindList.Codes.INF;
		CombineAssertions("DocumentCodeWithDocumenType", () =>
		{
			AssertEquals("No Additional Document with searched codes", false, parentInfo.HasAdditionalDocumentCode(documentType, documentCode));

			declarationAdditionalDocument.CSI_SubType = documentType;
			declarationAdditionalDocument.CSI_Code = documentCode;
			AssertEquals($"Additional Document with code C1 with Type T1 specified on declaration", true, parentInfo.HasAdditionalDocumentCode(documentType, documentCode));

			declarationAdditionalDocument.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			declarationAdditionalDocument.CSI_Code = ZString.Empty;
			invoiceAdditionalDocumentt.CSI_SubType = documentType;
			invoiceAdditionalDocumentt.CSI_Code = documentCode;
			AssertEquals($"Additional Document with code C1 with Type T1 specified on invoice", true, parentInfo.HasAdditionalDocumentCode(documentType, documentCode));

			invoiceAdditionalDocumentt.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			invoiceAdditionalDocumentt.CSI_Code = ZString.Empty;
			invoiceLineAdditionalDocument.CSI_SubType = documentType;
			invoiceLineAdditionalDocument.CSI_Code = documentCode;
			AssertEquals($"Additional Document with code C1 with Type T1 specified on invoiceLine", true, parentInfo.HasAdditionalDocumentCode(documentType, documentCode));

			invoiceLineAdditionalDocument.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			invoiceLineAdditionalDocument.CSI_Code = ZString.Empty;
			entryInstructionAdditionalDocument.CSI_SubType = documentType;
			entryInstructionAdditionalDocument.CSI_Code = documentCode;
			AssertEquals($"Additional Document with code C1 with Type T1 specified on entryInstruction", true, parentInfo.HasAdditionalDocumentCode(documentType, documentCode));

			entryInstructionAdditionalDocument.CSI_Code = "123";
			AssertEquals("Random code exist with Type T1, but not searched one", false, parentInfo.HasAdditionalDocumentCode(documentType, documentCode));

			entryInstructionAdditionalDocument.CSI_Code = documentCode;
			entryInstructionAdditionalDocument.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			AssertEquals("Searched code exist, but type is not the searched one", false, parentInfo.HasAdditionalDocumentCode(documentType, documentCode));
		});
	}

	public void TestHasAdditionalInfoCodeWithSameDescription()
	{
		var additionalInfoHeader = invoice.AdditionalInfos.AddNew();
		const string subTypeINF = AdditionalInfoKindList.Codes.INF;
		const string code4PL03 = AdditionalInfoCodes._4PL03;
		const string description = "Test Description";

		additionalInfoHeader.CSI_Code = ZString.Empty;
		additionalInfoHeader.CSI_SubType = ZString.Empty;
		additionalInfoHeader.CSI_Description = ZString.Empty;
		AssertEquals("All fields are empty", true, invoice.HasAdditionalInfoCodeWithSameDescription("", "", ""));

		additionalInfoHeader.CSI_Code = AdditionalInfoCodes._4PL03;
		additionalInfoHeader.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfoHeader.CSI_Description = "Test Description";
		AssertEquals("All fields are same", true, invoice.HasAdditionalInfoCodeWithSameDescription(subTypeINF, code4PL03, description));

		additionalInfoHeader.CSI_Code = "123";
		AssertEquals("CSI_Code is different", false, invoice.HasAdditionalInfoCodeWithSameDescription(subTypeINF, code4PL03, description));

		additionalInfoHeader.CSI_Code = AdditionalInfoCodes._4PL03;
		additionalInfoHeader.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		AssertEquals("CSI_SubType is different", false, invoice.HasAdditionalInfoCodeWithSameDescription(subTypeINF, code4PL03, description));

		additionalInfoHeader.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfoHeader.CSI_Description = "Different Description";
		AssertEquals("Description is different", false, invoice.HasAdditionalInfoCodeWithSameDescription(subTypeINF, code4PL03, description));
	}

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	CusEntryInstruction entryInstruction;
}
