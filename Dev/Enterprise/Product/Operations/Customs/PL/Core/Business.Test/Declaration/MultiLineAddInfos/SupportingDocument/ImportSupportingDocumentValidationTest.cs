using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportSupportingDocumentValidationTest : SupportingDocumentValidationTest
{
	public void TestCheckRuleR900()
	{
		const string messageError = "(R900) Supporting document number (for C513 code) must contain CCL text.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var suppDoc = declaration.SupportingDocuments.AddNew();

		var subStyleList = new List<ZString> { "A", "B", "C", "D", "E", "F", "X", "Y", "Z" };
		foreach (var subStyle in subStyleList)
		{
			instruction.CEI_SubStyle = subStyle;
			suppDoc.CSI_Code = "C513";
			suppDoc.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError(suppDoc.CSI_ReferenceNumberInfo, messageError);

			suppDoc.CSI_Code = "C513";
			suppDoc.CSI_ReferenceNumber = "CCL";
			AssertNoMessageError(suppDoc.CSI_ReferenceNumberInfo, messageError);

			suppDoc.CSI_Code = "C512";
			suppDoc.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageError(suppDoc.CSI_ReferenceNumberInfo, messageError);
		}
	}

	public void TestCheckRuleR922()
	{
		const string messageError1 = "(R922) Supporting document number (for C512 code) must contain SDE, ZW or ZS text.";
		const string messageError2 = "(R922) Supporting document number (for C514 code) must contain EIR, RW or ZS text.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstr = declaration.CustomsEntryInstructions.AddNew();
		var suppDoc = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			suppDoc.CSI_Code = "C512";
			var subStyleList = new List<ZString> { "C", "F", "Y" };
			var referenceContentListC512 = new List<ZString> { "SDE", "ZW", "ZS" };
			foreach (var subStyle in subStyleList)
			{
				entryInstr.CEI_SubStyle = subStyle;
				foreach (var referenceContent in referenceContentListC512)
				{
					suppDoc.CSI_ReferenceNumber = referenceContent;
					AssertNoMessageError($"{subStyle} - {referenceContent}", suppDoc.CSI_ReferenceNumberInfo, messageError1);
				}
				suppDoc.CSI_ReferenceNumber = ZString.Empty;
				AssertHasMessageError($"{subStyle}", suppDoc.CSI_ReferenceNumberInfo, messageError1);
			}

			suppDoc.CSI_Code = "C514";
			entryInstr.CEI_SubStyle = "Z";
			var referenceContentListC514 = new List<ZString> { "EIR", "RW", "ZS" };
			foreach (var referenceContent in referenceContentListC514)
			{
				suppDoc.CSI_ReferenceNumber = referenceContent;
				AssertNoMessageError($"Z - {referenceContent}", suppDoc.CSI_ReferenceNumberInfo, messageError2);
			}
			suppDoc.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageError("Z", suppDoc.CSI_ReferenceNumberInfo, messageError2);
		});
	}

	public void TestCheckRuleR956_AdditionalInfo()
	{
		var messageError = "(R956) Supporting document C651 cannot be presented. - Additional Info Code '4PL12' & Sub Style 'C' exists.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstr = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var addInfoDeclaration = declaration.AdditionalInfos.AddNew();
		var addInfoInovice = invoice.AdditionalInfos.AddNew();
		var addInfoInvoiceLine = invoiceLine.AdditionalInfos.AddNew();

		CheckRuleR956_AdditionalInfo_ForSpecificSupportingDocument(declaration.SupportingDocuments.AddNew());
		CheckRuleR956_AdditionalInfo_ForSpecificSupportingDocument(invoice.SupportingDocuments.AddNew());
		CheckRuleR956_AdditionalInfo_ForSpecificSupportingDocument(invoiceLine.SupportingDocuments.AddNew());

		void CheckRuleR956_AdditionalInfo_ForSpecificSupportingDocument(SupportingDocument supportingDocument)
		{
			addInfoInovice.CSI_Code = ZString.Empty;
			addInfoInvoiceLine.CSI_Code = ZString.Empty;
			CombineAssertions($"Supporting Document Parent is {supportingDocument.Parent}", () =>
			{
				entryInstr.CEI_SubStyle = "C";
				addInfoDeclaration.CSI_Code = "4PL12";
				supportingDocument.CSI_Code = "C651";
				AssertHasMessageError("C651 - declaration AdditionalInfos", supportingDocument.CSI_CodeInfo, messageError);

				entryInstr.CEI_SubStyle = "A";
				supportingDocument.Validation.ValidateCSI_Code();
				AssertNoMessageError("CEI_SubStyle is not C", supportingDocument.CSI_CodeInfo, messageError);

				entryInstr.CEI_SubStyle = "C";
				addInfoDeclaration.CSI_Code = "4PL11";
				supportingDocument.Validation.ValidateCSI_Code();
				AssertNoMessageError("CSI_Code is not 4PL12", supportingDocument.CSI_CodeInfo, messageError);

				addInfoDeclaration.CSI_Code = "4PL12";
				supportingDocument.CSI_Code = "C652";
				AssertNoMessageError("C652", supportingDocument.CSI_CodeInfo, messageError);

				addInfoDeclaration.CSI_Code = ZString.Empty;
				addInfoInovice.CSI_Code = "4PL12";
				supportingDocument.CSI_Code = "C651";
				AssertHasMessageError("C651 - invoice AdditionalInfos", supportingDocument.CSI_CodeInfo, messageError);

				addInfoInovice.CSI_Code = ZString.Empty;
				addInfoInvoiceLine.CSI_Code = "4PL12";
				supportingDocument.Validation.ValidateCSI_Code();
				AssertHasMessageError("C651 - invoiceLine AdditionalInfos", supportingDocument.CSI_CodeInfo, messageError);
			});
		}
	}

	public void TestCheckRuleR956_ProcedureCodeWithConcession()
	{
		var messageError = "(R956) Supporting document C651 cannot be presented. - Procedure Code '45' or '68' with Concession 'F06' does not exist.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstr = declaration.CustomsEntryInstructions.AddNew();
		var addInfo = declaration.AdditionalInfos.AddNew();
		var suppDoc = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			suppDoc.CSI_Code = "C651";
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_CEI = entryInstr.PK;
			var additionalProcedureCode = line.AdditionalProcedureCodes.AddNew();
			entryInstr.CEI_SubStyle = "A";
			line.JI_Procedure = "4400";
			additionalProcedureCode.CY_Code = "F06";
			suppDoc.Validation.ValidateCSI_Code();
			AssertHasMessageError("All R956 requirements should exist", suppDoc.CSI_CodeInfo, messageError);

			line.JI_Procedure = "4500";
			suppDoc.Validation.ValidateCSI_Code();
			AssertNoMessageError("procedure code is 45", suppDoc.CSI_CodeInfo, messageError);

			line.JI_Procedure = "6700";
			suppDoc.Validation.ValidateCSI_Code();
			AssertHasMessageError("procedure code is 67", suppDoc.CSI_CodeInfo, messageError);

			line.JI_Procedure = "6800";
			suppDoc.Validation.ValidateCSI_Code();
			AssertNoMessageError("procedure code is 68", suppDoc.CSI_CodeInfo, messageError);

			suppDoc.CSI_Code = "C652";
			AssertNoMessageError("CSI_Code is C652", suppDoc.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR626()
	{
		var messageError = "(R626) Additional Information code 00100 and Supporting Document/Authorization code C601 cannot be used together.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var addInfo = declaration.AdditionalInfos.AddNew();
		var suppDoc = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty declaration", suppDoc.CSI_CodeInfo, messageError);

			suppDoc.CSI_Code = Constants.SupportingDocumentCodes.C601;
			AssertNoMessageError("Supporting Document is C601", suppDoc.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			suppDoc.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is 51", suppDoc.CSI_CodeInfo, messageError);

			addInfo.CSI_Code = Constants.AdditionalInfoCodes._00100;
			suppDoc.Validation.ValidateCSI_Code();
			AssertHasMessageError("Additional Document code is 00100", suppDoc.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			suppDoc.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is not 51", suppDoc.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			suppDoc.CSI_Code = Constants.SupportingDocumentCodes.C651;
			AssertNoMessageError("Supporting Document is not C601", suppDoc.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR629()
	{
		var messageError = "(R629) Additional Information code 00100 and Supporting Document/Authorization code C516 cannot be used together.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalDocument = declaration.AdditionalInfos.AddNew();
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty declaration", supportingDocument.CSI_CodeInfo, messageError);

			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C516;
			AssertNoMessageError("Supporting Document is C516", supportingDocument.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is 53", supportingDocument.CSI_CodeInfo, messageError);

			additionalDocument.CSI_Code = Constants.AdditionalInfoCodes._00100;
			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageError("Additional Document code is 00100", supportingDocument.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is not 53", supportingDocument.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C651;
			AssertNoMessageError("Supporting Document is not C516", supportingDocument.CSI_CodeInfo, messageError);

			additionalDocument.CSI_Code = Constants.AdditionalInfoCodes._00200;
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C516;
			AssertNoMessageError("Additional Document code is not 00100", supportingDocument.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR632()
	{
		var messageError = "(R632) Additional Information code 00100 and Supporting Document/Authorization code C019 can coexist only with Additional Information code 4PL09.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		AssertNoMessageError("Empty declaration", supportingDocument.CSI_CodeInfo, messageError);

		foreach (string procedureCode in Constants.ValidationLists.R632Procedures(Factory))
		{
			AssertCheckRule632ForProcedureCode(procedureCode);
		}
	}

	public void TestCheckRuleR258()
	{
		var messageError = "(R258) 'N018' supporting document is not allowed for requested procedure code 40 with preference code 400/420 and procedure details F15/F16.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstr = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
		var invoiceLineSupportingDocumet = invoice.SupportingDocuments.AddNew();
		var invoiceHeaderSupportingDocument = invoice.SupportingDocuments.AddNew();
		var declarationCodeInfo = declarationSupportingDocument.CSI_CodeInfo;
		var invoiceLineCodeInfo = invoiceLineSupportingDocumet.CSI_CodeInfo;
		var invoiceHeaderInfo = invoiceHeaderSupportingDocument.CSI_CodeInfo;
		invoiceLine.JI_CEI = entryInstr.PK;
		var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes._400;
			additionalProcedureCode.CY_Code = ConcessionCodes.F15;
			entryInstr.CEI_Procedure = ProcedureCodes._40;
			declarationSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N018;
			invoiceLineSupportingDocumet.CSI_Code = Constants.SupportingDocumentCodes.N018;
			invoiceHeaderSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N018;
			AssertHasMessageError("Declaration Level: when procedure code 40 with preference code 400/420 and procedure details F15/F16, N018 supporting document is not allowed", declarationCodeInfo, messageError);
			AssertHasMessageError("InvoiceLine Level: when procedure code 40 with preference code 400/420 and procedure details F15/F16, N018 supporting document is not allowed", invoiceLineCodeInfo, messageError);
			AssertHasMessageError("InvoiceHeader Level: when procedure code 40 with preference code 400/420 and procedure details F15/F16, N018 supporting document is not allowed", invoiceHeaderInfo, messageError);

			declarationSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C516;
			invoiceLineSupportingDocumet.CSI_Code = Constants.SupportingDocumentCodes.C516;
			invoiceHeaderSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C516;
			AssertNoMessageError("Declaration Level: supporting domucent code is not N018", declarationCodeInfo, messageError);
			AssertNoMessageError("InvoiceLine Level: supporting domucent code is not N018", invoiceLineCodeInfo, messageError);
			AssertNoMessageError("InvoiceHeader Level: supporting domucent code is not N018", invoiceHeaderInfo, messageError);

			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			declarationSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N018;
			invoiceLineSupportingDocumet.CSI_Code = Constants.SupportingDocumentCodes.N018;
			invoiceHeaderSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N018;
			AssertNoMessageError("Declaration Level: Entry style is not EU", declarationCodeInfo, messageError);
			AssertNoMessageError("InvoiceLine Level: Entry style is not EU", invoiceLineCodeInfo, messageError);
			AssertNoMessageError("InvoiceHeader Level: Entry style is not EU", invoiceHeaderInfo, messageError);

			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes._220;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
			declarationSupportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Declaration Level: PrimaryPreference is not in {400, 420}", declarationCodeInfo, messageError);
			invoiceLineSupportingDocumet.Validation.ValidateCSI_Code();
			AssertNoMessageError("InvoiceLine Level: PrimaryPreference is not in {400, 420}", invoiceLineCodeInfo, messageError);
			invoiceHeaderSupportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("InvoiceHeader Level: PrimaryPreference is not in {400, 420}", invoiceHeaderInfo, messageError);

			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes._400;
			additionalProcedureCode.CY_Code = ConcessionCodes.C20;
			declarationSupportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Declaration Level: Concession code is not in {F15, F16}", declarationCodeInfo, messageError);
			invoiceLineSupportingDocumet.Validation.ValidateCSI_Code();
			AssertNoMessageError("InvoiceLine Level: Concession code is not in {F15, F16}", invoiceLineCodeInfo, messageError);
			invoiceHeaderSupportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("InvoiceHeader Level: Concession code is not in {F15, F16}", invoiceHeaderInfo, messageError);

			additionalProcedureCode.CY_Code = ConcessionCodes.F15;
			declarationSupportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageError("Declaration Level: when procedure code 40 with preference code 400/420 and procedure details F15/F16, N018 supporting document is not allowed", declarationCodeInfo, messageError);
			invoiceLineSupportingDocumet.Validation.ValidateCSI_Code();
			AssertHasMessageError("InvoiceLine Level: when procedure code 40 with preference code 400/420 and procedure details F15/F16, N018 supporting document is not allowed", invoiceLineCodeInfo, messageError);
			invoiceHeaderSupportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageError("InvoiceHeader Level: when procedure code 40 with preference code 400/420 and procedure details F15/F16, N018 supporting document is not allowed", invoiceHeaderInfo, messageError);
		});
	}

	void AssertCheckRule632ForProcedureCode(string procedureCode)
	{
		var messageError = "(R632) Additional Information code 00100 and Supporting Document/Authorization code C019 can coexist only with Additional Information code 4PL09.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalDocument = declaration.AdditionalInfos.AddNew();
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = procedureCode;
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError($"Entry Instruction Procedure is {procedureCode}", supportingDocument.CSI_CodeInfo, messageError);

			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C019;
			AssertNoMessageError($"Procedure = {procedureCode}, Supporting Document is C019", supportingDocument.CSI_CodeInfo, messageError);

			additionalDocument.CSI_Code = Constants.AdditionalInfoCodes._00100;
			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageError($"Procedure = {procedureCode}, Additional Document code is 00100", supportingDocument.CSI_CodeInfo, messageError);

			var additionalDocument2 = declaration.AdditionalInfos.AddNew();
			additionalDocument2.CSI_Code = Constants.AdditionalInfoCodes._4PL09;
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError($"Procedure = {procedureCode}, Additional Document code 4PL09 exists", supportingDocument.CSI_CodeInfo, messageError);
		});
	}
}
