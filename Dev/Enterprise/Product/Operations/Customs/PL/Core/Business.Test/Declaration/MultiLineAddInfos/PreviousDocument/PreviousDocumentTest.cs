using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(PreviousDocument))]
sealed class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
{
	public void TestLookupsType()
	{
		AssertType<PreviousDocumentLookups>(previousDocument.Lookups);
	}

	public void TestValidationType()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<PreviousDocumentValidation>(previousDocument.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportPreviousDocumentValidation>(previousDocument.Validation);
		});
	}

	public void TestCSI_Quantity()
	{
		AssertEquals("Goods Quantity", DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_QuantityInfo).Caption);
	}

	public void TestCSI_PackQty()
	{
		AssertEquals("Packs Quantity", DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_PackQtyInfo).Caption);
	}

	public void TestCSI_PackType()
	{
		AssertEquals("Pack Type Code", DataBoundResourceStrings.GetDataForProperty(previousDocument.CSI_PackTypeInfo).Caption);
	}

	public void TestCSI_ReferenceNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocumentDeclaration = declaration.PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var previousDocumentInvoice = invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var previousDocumentLine = invoiceLine.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			using (declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("When the declaration is export and not full AES UCC6 , the max length of CSI_ReferenceNumber should be:", previousDocumentDeclaration.CSI_ReferenceNumberInfo.MaxLength, 35);
				AssertEquals("When the declaration is export and not full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentInvoice.CSI_ReferenceNumberInfo.MaxLength, 35);
				AssertEquals("When the declaration is export and not full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentLine.CSI_ReferenceNumberInfo.MaxLength, 35);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("When the declaration is import and not full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentDeclaration.CSI_ReferenceNumberInfo.MaxLength, 35);
				AssertEquals("When the declaration is import and not full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentInvoice.CSI_ReferenceNumberInfo.MaxLength, 35);
				AssertEquals("When the declaration is import and not full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentLine.CSI_ReferenceNumberInfo.MaxLength, 35);
			}
			using (declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("When the declaration is export and full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentDeclaration.CSI_ReferenceNumberInfo.MaxLength, 70);
				AssertEquals("When the declaration is export and full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentInvoice.CSI_ReferenceNumberInfo.MaxLength, 70);
				AssertEquals("When the declaration is export and full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentLine.CSI_ReferenceNumberInfo.MaxLength, 70);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("When the declaration is import and full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentDeclaration.CSI_ReferenceNumberInfo.MaxLength, 35);
				AssertEquals("When the declaration is import and full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentInvoice.CSI_ReferenceNumberInfo.MaxLength, 35);
				AssertEquals("When the declaration is import and full AES UCC6, the max length of CSI_ReferenceNumber should be:", previousDocumentLine.CSI_ReferenceNumberInfo.MaxLength, 35);
			}
		});
	}

	public void TestCSI_ReferenceNumber2()
	{
		var propertyInfo = previousDocument.CSI_ReferenceNumber2Info;
		CombineAssertions(() =>
		{
			AssertEquals("Length:", 5, propertyInfo.MaxLength);
			AssertEquals("Caption:", "Goods Shipment Number", propertyInfo.HumanReadableName);
		});
	}

	public void TestCSI_ReferenceNumber2_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var previousDocumentDeclaration = declaration.PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var previousDocumentInvoice = invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var previousDocumentLine = invoiceLine.PreviousDocuments.AddNew();
		CombineAssertions(() =>
		{
			AssertReadOnlyOnSpecialProcedureCode("Declaration", previousDocumentDeclaration, true);
			AssertReadOnlyOnSpecialProcedureCode("Invoice header", previousDocumentInvoice, false);
			AssertReadOnlyOnSpecialProcedureCode("Invoice line", previousDocumentLine, false);
		});

		void AssertReadOnlyOnSpecialProcedureCode(string description, PreviousDocument previousDocument, bool expectedReadOnly)
		{
			const string testSpecialProcedureCode = "MRN";
			var propertyInfo = previousDocument.CSI_ReferenceNumber2Info;
			AssertEquals($"{description} - ReferenceNumber2_ReadOnly by default:", true, propertyInfo.ReadOnly);
			previousDocument.CSI_Code = testSpecialProcedureCode;
			AssertEquals($"{description} - ReferenceNumber2_ReadOnly when CSI_Code is special procedure code:", expectedReadOnly, propertyInfo.ReadOnly);
		}
	}

	public void TestParentInfo()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationDocument = declaration.PreviousDocuments.AddNew();
		var entryInstructionDocument = declaration.CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceDocument = invoice.PreviousDocuments.AddNew();
		var invoiceLineDocument = invoice.InvoiceLines.AddNew().PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			CheckDocumentParent("Declaration level", declarationDocument, isDeclarationLevel: true);
			CheckDocumentParent("EntryInstruction level", entryInstructionDocument, isEntryInstructionLevel: true);
			CheckDocumentParent("Invoice level", invoiceDocument, isInvoiceLevel: true);
			CheckDocumentParent("Invoice Line level", invoiceLineDocument, isInvoiceLineLevel: true);
		});

		void CheckDocumentParent(string message, CusSupportingInfo document, bool isDeclarationLevel = false, bool isEntryInstructionLevel = false, bool isInvoiceLevel = false, bool isInvoiceLineLevel = false)
		{
			AssertEquals($"{message} ParentAsJobDeclaration", isDeclarationLevel, document.Parent is JobDeclaration);
			AssertEquals($"{message} ParentAsEntryInstruction", isEntryInstructionLevel, document.Parent is CusEntryInstruction);
			AssertEquals($"{message} ParentAsInvoiceHeader", isInvoiceLevel, document.Parent is JobComInvoiceHeader);
			AssertEquals($"{message} ParentAsInvoiceLine", isInvoiceLineLevel, document.Parent is JobComInvoiceLine);
		}
	}

	public void TestParentIsJobComInvoiceLineOrHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationDocument = declaration.PreviousDocuments.AddNew();
		var entryInstructionDocument = declaration.CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceDocument = invoice.PreviousDocuments.AddNew();
		var invoiceLineDocument = invoice.InvoiceLines.AddNew().PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("no parent", false, Factory.New<PreviousDocument>().ParentIsJobComInvoiceLineOrHeader);
			AssertEquals("declarationDocument", false, declarationDocument.ParentIsJobComInvoiceLineOrHeader);
			AssertEquals("entryInstructionDocument", false, entryInstructionDocument.ParentIsJobComInvoiceLineOrHeader);
			AssertEquals("invoiceDocument", true, invoiceDocument.ParentIsJobComInvoiceLineOrHeader);
			AssertEquals("invoiceLineDocument", true, invoiceLineDocument.ParentIsJobComInvoiceLineOrHeader);
		});
	}

	protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.PreviousDocuments.AddNew();
		yield return declaration.CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.PreviousDocuments.AddNew();
		yield return invoice.JobComInvoiceLines.AddNew().PreviousDocuments.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		previousDocument = declaration.Invoices.AddNew().PreviousDocuments.AddNew();
	}

	JobDeclaration declaration;
	PreviousDocument previousDocument;
}
