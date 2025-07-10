using System.Collections.Generic;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocument))]
sealed class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
{
	public void TestGetNewValidation()
	{
		CombineAssertions(() =>
		{
			AssertType<ExportSupportingDocumentValidation>("EXP", GetDocument(MessageTypeList.Codes.Export).Validation);
			AssertType<ImportSupportingDocumentValidation>("IMP", GetDocument(MessageTypeList.Codes.Import).Validation);
		});
	}

	public void TestCSI_ReferenceNumber2()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supportingDocumentDeclaration = declaration.SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var supportingDocumentInvoice = invoice.SupportingDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var supportingDocumentLine = invoiceLine.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("The declaration is export, declaration level", 70, supportingDocumentDeclaration.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals("The declaration is export, invoice header level", 70, supportingDocumentInvoice.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals("The declaration is export, invoice line level", 70, supportingDocumentLine.CSI_ReferenceNumber2Info.MaxLength);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("The declaration is import, declaration level", 35, supportingDocumentDeclaration.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals("The declaration is import, invoice header level", 35, supportingDocumentInvoice.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals("The declaration is import, invoice line level", 35, supportingDocumentLine.CSI_ReferenceNumber2Info.MaxLength);
		});
	}

	public void TestCSI_ReferenceNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supportingDocumentDeclaration = declaration.SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var supportingDocumentInvoice = invoice.SupportingDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var supportingDocumentLine = invoiceLine.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			using (declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("The declaration is export and not full AES UCC6, declaration level", 35, supportingDocumentDeclaration.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("The declaration is export and not full AES UCC6, invoice header level", 35, supportingDocumentInvoice.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("The declaration is export and not full AES UCC6, invoice line level", 35, supportingDocumentLine.CSI_ReferenceNumberInfo.MaxLength);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("The declaration is import and not full AES UCC6, declaration level", 35, supportingDocumentDeclaration.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("The declaration is import and not full AES UCC6, invoice header level", 35, supportingDocumentInvoice.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("The declaration is import and not full AES UCC6, invoice line level", 35, supportingDocumentLine.CSI_ReferenceNumberInfo.MaxLength);
			}
			using (declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("The declaration is export and full AES UCC6, declaration level", 70, supportingDocumentDeclaration.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("The declaration is export and full AES UCC6, invoice header level", 70, supportingDocumentInvoice.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("The declaration is export and full AES UCC6, invoice line level", 70, supportingDocumentLine.CSI_ReferenceNumberInfo.MaxLength);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("The declaration is import and full AES UCC6, declaration level", 35, supportingDocumentDeclaration.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("The declaration is import and full AES UCC6, invoice header level", 35, supportingDocumentInvoice.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("The declaration is import and full AES UCC6, invoice line level", 35, supportingDocumentLine.CSI_ReferenceNumberInfo.MaxLength);
			}
		});
	}

	public void TestCSI_Description() => AssertEquals(35, GetDocument().CSI_DescriptionInfo.MaxLength);

	public void TestCSI_ItemNumber()
	{
		var document = GetDocument();
		CombineAssertions(() =>
		{
			AssertEquals("Resource String", "Line No.", DataBoundResourceStrings.GetDataForProperty(document.CSI_ItemNumberInfo).Caption);
			AssertEquals("Max Length", 5, document.CSI_ItemNumberInfo.MaxLength);
		});
	}

	public void TestCSI_AdditionalDescription()
	{
		var document = GetDocument();
		CombineAssertions(() =>
		{
			AssertEquals("Resource String", "Issuing Authority Name", DataBoundResourceStrings.GetDataForProperty(document.CSI_AdditionalDescriptionInfo).Caption);
			AssertEquals("Max Length", 70, document.CSI_AdditionalDescriptionInfo.MaxLength);
		});
	}

	public void TestCSI_DateOfExpiry()
	{
		var document = GetDocument();
		AssertEquals("Resource String", "Validity date", DataBoundResourceStrings.GetDataForProperty(document.CSI_DateOfExpiryInfo).Caption);
	}

	public void TestCSI_Value()
	{
		var propertyInfo = typeof(SupportingDocument).GetProperty(nameof(SupportingDocument.CSI_Value));
		CombineAssertions(() =>
		{
			AssertEquals("DecimalPlaces", 2, propertyInfo.GetCustomAttribute<DecimalPlacesAttribute>().DecimalPlaces);
			AssertEquals("DecimalPrecision", 16, propertyInfo.GetCustomAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
		});
	}

	public void TestParentInfo()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationDocument = declaration.SupportingDocuments.AddNew();
		var entryInstructionDocument = declaration.CustomsEntryInstructions.AddNew().SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceDocument = invoice.SupportingDocuments.AddNew();
		var invoiceLineDocument = invoice.InvoiceLines.AddNew().SupportingDocuments.AddNew();

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

	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.SupportingDocuments.AddNew();
		yield return declaration.CustomsEntryInstructions.AddNew().SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.SupportingDocuments.AddNew();
		yield return invoice.JobComInvoiceLines.AddNew().SupportingDocuments.AddNew();
	}

	SupportingDocument GetDocument(string messageType = "")
	{
		var declaration = Factory.New<JobDeclaration>();
		if (!string.IsNullOrEmpty(messageType))
		{
			declaration.JE_MessageType = messageType;
		}
		return declaration.SupportingDocuments.AddNew();
	}
}
