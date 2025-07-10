using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportAdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRuleR206()
	{
		var errorMessage = "(R206) – Additional Info code 00500 is not allowed for Indirect Representation.";
		var header = declaration.Invoices.AddNew();
		var line = header.InvoiceLines.AddNew();
		var addInfo = declaration.AdditionalInfos.AddNew();
		CombineAssertions(() =>
		{
			addInfo.CSI_Code = AdditionalInfoCodes._00500;
			AssertNoMessageError("No declarant type", addInfo.CSI_CodeInfo, errorMessage);
			declaration.JE_DeclarantType = PLRepresentationTypeList.Codes._5Indirect;
			addInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("Error message - declaration", addInfo.CSI_CodeInfo, errorMessage);
			addInfo = header.AdditionalInfos.AddNew();
			addInfo.CSI_Code = AdditionalInfoCodes._00500;
			AssertHasMessageError("Error message - invoice header", addInfo.CSI_CodeInfo, errorMessage);
			addInfo = line.AdditionalInfos.AddNew();
			addInfo.CSI_Code = AdditionalInfoCodes._00500;
			AssertHasMessageError("Error message - invoice line", addInfo.CSI_CodeInfo, errorMessage);
		});
	}

	public void TestCheckForRuleR862()
	{
		var secondLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();

		var messageError = "R862 - Additional Info with Code '00100' is required with Additional Info code 0PL05...0PL10.";

		CombineAssertions(() =>
		{
			CheckCSICodeWithDifferentLevel(secondLineAdditionalInfo);
			CheckCSICodeWithDifferentLevel(headerAdditionalInfo);
			CheckCSICodeWithDifferentLevel(entryAdditionalInfo);
		});

		void CheckCSICodeWithDifferentLevel(AdditionalInfo levelInfo)
		{
			lineAdditionalInfo.CSI_Code = "0PL05";
			entry.CEI_SubStyle = "A";
			levelInfo.CSI_Code = "00100";
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoRowMessageError("not R862 setup should not contain message error", invoiceLine, messageError);

			var ceiSubStyleList = new List<ZString> { Constants.SubStyleCodes.A, Constants.SubStyleCodes.D };
			var csiCodeList = new List<ZString>() { Constants.AdditionalInfoCodes._0PL05, Constants.AdditionalInfoCodes._0PL06
				, Constants.AdditionalInfoCodes._0PL07, Constants.AdditionalInfoCodes._0PL08
				, Constants.AdditionalInfoCodes._0PL09, Constants.AdditionalInfoCodes._0PL10 };

			levelInfo.CSI_Code = "00101";
			foreach (var subStyle in ceiSubStyleList)
			{
				entry.CEI_SubStyle = subStyle;
				foreach (var csiCode in csiCodeList)
				{
					lineAdditionalInfo.CSI_Code = csiCode;
					lineAdditionalInfo.Validation.ValidateCSI_Code();
					AssertHasRowMessageError($"{subStyle} {csiCode} should have message error", invoiceLine, messageError);
				}
			}
		}
	}

	public void TestCheckForRuleR633()
	{
		const string messageError = "(R633) - Additional Info with Code '0PL05'...'0PL10' is required when additional information 00100 is present.";
		var itemList = new List<ZString>() { Constants.AdditionalInfoCodes._0PL05, Constants.AdditionalInfoCodes._0PL06
			, Constants.AdditionalInfoCodes._0PL07, Constants.AdditionalInfoCodes._0PL08
			, Constants.AdditionalInfoCodes._0PL09, Constants.AdditionalInfoCodes._0PL10 };

		CombineAssertions(() =>
		{
			var secondLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
			CheckCSICodeWithDifferentLevel(secondLineAdditionalInfo);
			CheckCSICodeWithDifferentLevel(headerAdditionalInfo);
			CheckCSICodeWithDifferentLevel(entryAdditionalInfo);
		});

		void CheckCSICodeWithDifferentLevel(AdditionalInfo levelInfo)
		{
			foreach (var item in itemList)
			{
				lineAdditionalInfo.CSI_Code = "00101";
				levelInfo.CSI_Code = item;
				lineAdditionalInfo.Validation.ValidateCSI_Code();
				AssertNoRowMessageError($"{item} - 00101 should not have message error", invoiceLine, messageError);

				lineAdditionalInfo.CSI_Code = "00100";
				lineAdditionalInfo.Validation.ValidateCSI_Code();
				AssertNoRowMessageError($"{item} - 00100 should not have message error", invoiceLine, messageError);
			}

			levelInfo.CSI_Code = "1PL00";
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertHasRowMessageError("1PL00 should have message error", invoiceLine, messageError);

			levelInfo.CSI_Code = ZString.Empty;
		}
	}

	public void TestCheckForRuleR634()
	{
		const string messageError = "(R634) – The 00100 Additional Information code cannot exist for requested procedure code 44 with Supporting Document N990.";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N990;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document is N990", additionalInfo.CSI_CodeInfo, messageError);

			supportingDocument.CSI_ReferenceNumber = "ABC";
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document CSI_ReferenceNumber is not empty", additionalInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._44;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is 44", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00100;
			AssertHasMessageError("Additional Document code is 00100", additionalInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._46;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is not 44", additionalInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._44;
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N380;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document is not N990", additionalInfo.CSI_CodeInfo, messageError);

			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.N990;
			supportingDocument.CSI_ReferenceNumber = ZString.Empty;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document CSI_ReferenceNumber is empty", additionalInfo.CSI_CodeInfo, messageError);

			supportingDocument.CSI_ReferenceNumber = "ABC";
			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00200;
			AssertNoMessageError("Additional Document code is not 00100", additionalInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR820()
	{
		const string messageError = "(R820) – Additional Information code 00100 or Supporting Document code C019 is required.";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		var additionalInfo2 = declaration.AdditionalInfos.AddNew();
		var supportingDocument = declaration.SupportingDocuments.AddNew();
		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is 48", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL09;
			AssertHasMessageError("Additional Document code is 4PL09", additionalInfo.CSI_CodeInfo, messageError);

			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C019;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document is C019", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo2.CSI_Code = Constants.AdditionalInfoCodes._00100;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Other Additional code is 00100", additionalInfo.CSI_CodeInfo, messageError);

			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C512;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document is not C019", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo2.CSI_Code = Constants.AdditionalInfoCodes._00200;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("Other Additional code is not 00100", additionalInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._46;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is not 48", additionalInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._4PL10;
			AssertNoMessageError("Additional Document code is not 4PL09", additionalInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR1540()
	{
		const string messageError = "(R1540) – For end use procedure with 00100 code, Additional Information code 4PL15 or 4PL07 must exist.";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		var additionalInfo2 = declaration.AdditionalInfos.AddNew();
		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._44;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is 44", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00100;
			AssertHasMessageError("Additional Document code is 00100", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo2.CSI_Code = Constants.AdditionalInfoCodes._4PL07;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Other Additional code is 4PL07", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo2.CSI_Code = Constants.AdditionalInfoCodes._4PL15;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Other Additional code is 4PL15", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo2.CSI_Code = Constants.AdditionalInfoCodes._00200;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("Other Additional code is not 4PL07 or 4PL15", additionalInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is not 44", additionalInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._44;
			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00200;
			AssertNoMessageError("Additional Document code is not 00100", additionalInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR1586()
	{
		const string messageError = "(R1586) – If there is an Additional Information code 00100, the Code of the Supervising Customs Office (SCO) is required.";
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		CombineAssertions(() =>
		{
			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00100;
			AssertHasMessageError("Additional Document code is 00100", additionalInfo.CSI_CodeInfo, messageError);

			var office = declaration.CustomsOfficesForBinding.AddNew();
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("Custom Office Collection is not empty", additionalInfo.CSI_CodeInfo, messageError);

			office.CY_Code = EuOfficeCodesTypes.Codes.AuthorityControlCode;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Custom Office code is SCO and type is EUO", additionalInfo.CSI_CodeInfo, messageError);

			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("Custom Office code is not SCO", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00200;
			AssertNoMessageError("Additional Document code is not 00100", additionalInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR1630()
	{
		const string messageError = "(R1630) – For requested procedure code 46, the Additional Information code 00100 is not allowed.";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._46;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is 46", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00100;
			AssertHasMessageError("Additional Document code is 00100", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00200;
			AssertNoMessageError("Additional Document code is not 00100", additionalInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._44;
			additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._00100;
			AssertNoMessageError("Entry Instruction Procedure is not 46", additionalInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR954_InvoiceLine()
	{
		const string messageError = "R954 - Additional Information code '04PL07' is valid for Requested Procedure code '51', '53' or '48'.";

		CombineAssertions(() =>
		{
			lineAdditionalInfo.CSI_Code = "4PL07";
			invoiceLine.JI_Procedure = "51";
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoRowMessageError("No message error when Code is 4PL07 and Procedure is 51", invoiceLine, messageError);

			invoiceLine.JI_Procedure = "53";
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoRowMessageError("No message error when Code is 4PL07 and Procedure is 53", invoiceLine, messageError);

			invoiceLine.JI_Procedure = "48";
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoRowMessageError("No message error when Code is 4PL07 and Procedure is 48", invoiceLine, messageError);

			lineAdditionalInfo.CSI_Code = "4PL06";
			invoiceLine.JI_Procedure = ZString.Empty;
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoRowMessageError("No message error when Code is 4PL06", invoiceLine, messageError);

			lineAdditionalInfo.CSI_Code = "4PL07";
			invoiceLine.JI_Procedure = ZString.Empty;
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertHasRowMessageError("Has message error when Code is 4PL07 and Procedure is not 51 or 53 or 48", invoiceLine, messageError);
		});
	}

	public void TestCheckForRuleR954_InvoiceHeader()
	{
		const string messageError = "R954 - Additional Information code '04PL07' is valid for Requested Procedure code '51', '53' or '48'.";

		CombineAssertions(() =>
		{
			headerAdditionalInfo.CSI_Code = "4PL07";
			invoiceLine.JI_Procedure = "51";
			headerAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("No message error when Code is 4PL07 and Procedure is 51", headerAdditionalInfo.CSI_CodeInfo, messageError);

			invoiceLine.JI_Procedure = "53";
			headerAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("No message error when Code is 4PL07 and Procedure is 53", headerAdditionalInfo.CSI_CodeInfo, messageError);

			invoiceLine.JI_Procedure = "48";
			headerAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("No message error when Code is 4PL07 and Procedure is 48", headerAdditionalInfo.CSI_CodeInfo, messageError);

			headerAdditionalInfo.CSI_Code = "4PL06";
			invoiceLine.JI_Procedure = ZString.Empty;
			headerAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("No message error when Code is 4PL06", headerAdditionalInfo.CSI_CodeInfo, messageError);

			headerAdditionalInfo.CSI_Code = "4PL07";
			invoiceLine.JI_Procedure = ZString.Empty;
			headerAdditionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("Has message error when Code is 4PL07 and Procedure is not 51 or 53 or 48", headerAdditionalInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR954_EntryInstruction()
	{
		const string messageError = "R954 - Additional Information code '04PL07' is valid for Requested Procedure code '51', '53' or '48'.";
		var invLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault();

		CombineAssertions(() =>
		{
			entryAdditionalInfo.CSI_Code = "4PL07";
			invLine.JI_Procedure = "51";
			entryAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("No message error when Code is 4PL07 and Procedure is 51", entryAdditionalInfo.CSI_CodeInfo, messageError);

			invLine.JI_Procedure = "53";
			entryAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("No message error when Code is 4PL07 and Procedure is 53", entryAdditionalInfo.CSI_CodeInfo, messageError);

			invLine.JI_Procedure = "48";
			entryAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("No message error when Code is 4PL07 and Procedure is 48", entryAdditionalInfo.CSI_CodeInfo, messageError);

			entryAdditionalInfo.CSI_Code = "4PL06";
			invLine.JI_Procedure = ZString.Empty;
			entryAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("No message error when Code is 4PL06", entryAdditionalInfo.CSI_CodeInfo, messageError);

			entryAdditionalInfo.CSI_Code = "4PL07";
			invLine.JI_Procedure = ZString.Empty;
			entryAdditionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("Has message error when Code is 4PL07 and Procedure is not 51 or 53 or 48", entryAdditionalInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckForRuleR978()
	{
		const string messageError = "R978 - Additional Info with Code '1PL12', '1PL13' and '1PL14' is not valid when Sub Style is 'D', 'E' and 'F'.";

		var csi_CodeitemList = new List<ZString>() { "1PL12", "1PL13", "1PL14" };
		var subStyleitemList = new List<ZString>() { "D", "E", "F" };

		foreach (var csiCode in csi_CodeitemList)
		{
			entry.CEI_SubStyle = "A";
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoRowMessageErrorContaining(invoiceLine, messageError);

			foreach (var subStyle in subStyleitemList)
			{
				lineAdditionalInfo.CSI_Code = csiCode;
				entry.CEI_SubStyle = subStyle;
				lineAdditionalInfo.Validation.ValidateCSI_Code();
				AssertHasRowMessageErrorContaining(invoiceLine, messageError);

				lineAdditionalInfo.CSI_Code = "ASD";
				lineAdditionalInfo.Validation.ValidateCSI_Code();
				AssertNoRowMessageErrorContaining(invoiceLine, messageError);
			}
		}
	}

	public void TestCheckForRuleR979()
	{
		const string messageError = "R979 - Additional Info with Code '1PL15' is not allowed for Sub Style code 'B', 'C', 'E', 'F', 'X', 'Y' and 'Z'.";

		var itemList = new List<ZString>() { "B", "C", "E", "F", "X", "Y", "Z" };
		foreach (var item in itemList)
		{
			lineAdditionalInfo.CSI_Code = "1PL15";
			entry.CEI_SubStyle = item;
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertHasRowMessageErrorContaining(invoiceLine, messageError);

			lineAdditionalInfo.CSI_Code = "1PL14";
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoRowMessageErrorContaining(invoiceLine, messageError);
		}

		lineAdditionalInfo.CSI_Code = "1PL15";
		entry.CEI_SubStyle = "A";
		lineAdditionalInfo.Validation.ValidateCSI_Code();
		AssertNoRowMessageErrorContaining(invoiceLine, messageError);
	}

	public void TestCheckForRuleR981()
	{
		const string messageError = "R981 - Additional Info with Code '00100' is not allowed for Sub Style code 'B', 'C', 'E', 'F', 'X', 'Y' and 'Z'.";
		var itemList = new List<ZString>() { "B", "C", "E", "F", "X", "Y", "Z" };
		foreach (var item in itemList)
		{
			lineAdditionalInfo.CSI_Code = "00100";
			entry.CEI_SubStyle = item;
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertHasRowMessageErrorContaining(invoiceLine, messageError);

			lineAdditionalInfo.CSI_Code = "1PL15";
			lineAdditionalInfo.Validation.ValidateCSI_Code();
			AssertNoRowMessageErrorContaining(invoiceLine, messageError);
		}

		lineAdditionalInfo.CSI_Code = "00100";
		entry.CEI_SubStyle = "A";
		lineAdditionalInfo.Validation.ValidateCSI_Code();
		AssertNoRowMessageErrorContaining(invoiceLine, messageError);
	}

	public void TestCSI_CodeDuplicates()
	{
		const string messageError = "The Code:\'ABC\' has already been specified for this Invoice Line or corresponding Invoice Header's / Declaration's Additional Info list";
		var entry = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entry.PK;
		var headerAdditionalInfo = invoice.AdditionalInfos.AddNew();
		var lineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();

		lineAdditionalInfo.CSI_Code = "ABC";
		headerAdditionalInfo.CSI_Code = "ABC";
		headerAdditionalInfo.Validation.ValidateCSI_Code();
		lineAdditionalInfo.Validation.ValidateCSI_Code();
		AssertHasMessageError(lineAdditionalInfo.CSI_CodeInfo, messageError);
		AssertHasMessageError(headerAdditionalInfo.CSI_CodeInfo, messageError);

		headerAdditionalInfo.CSI_Code = "CBA";
		headerAdditionalInfo.Validation.ValidateCSI_Code();
		lineAdditionalInfo.Validation.ValidateCSI_Code();
		AssertNoMessageError(lineAdditionalInfo.CSI_CodeInfo, messageError);
		AssertNoMessageError(headerAdditionalInfo.CSI_CodeInfo, messageError);

		lineAdditionalInfo.CSI_Code = "CBA";
		headerAdditionalInfo.CSI_Code = "ABC";
		headerAdditionalInfo.Validation.ValidateCSI_Code();
		lineAdditionalInfo.Validation.ValidateCSI_Code();
		AssertNoMessageError(lineAdditionalInfo.CSI_CodeInfo, messageError);
		AssertNoMessageError(headerAdditionalInfo.CSI_CodeInfo, messageError);

		var lineAdditionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
		lineAdditionalInfo.CSI_Code = "CBA";
		lineAdditionalInfo2.CSI_Code = "ABC";
		headerAdditionalInfo.CSI_Code = "ABC";
		headerAdditionalInfo.Validation.ValidateCSI_Code();
		lineAdditionalInfo.Validation.ValidateCSI_Code();
		AssertNoMessageError(lineAdditionalInfo.CSI_CodeInfo, messageError);
		AssertHasMessageError(lineAdditionalInfo2.CSI_CodeInfo, messageError);
		AssertHasMessageError(headerAdditionalInfo.CSI_CodeInfo, messageError);
	}

	public void TestCheckRuleR626()
	{
		var messageError = "(R626) Additional Information code 00100 and Supporting Document/Authorization code C601 cannot be used together.";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var addInfo = declaration.AdditionalInfos.AddNew();
		var suppDoc = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty declaration", addInfo.CSI_CodeInfo, messageError);

			suppDoc.CSI_Code = Constants.SupportingDocumentCodes.C601;
			addInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document is C601", addInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			addInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is 51", addInfo.CSI_CodeInfo, messageError);

			addInfo.CSI_Code = Constants.AdditionalInfoCodes._00100;
			AssertHasMessageError("Additional Document code is 00100", addInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			addInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is not 51", addInfo.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			suppDoc.CSI_Code = Constants.SupportingDocumentCodes.C651;
			addInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document is not C601", addInfo.CSI_CodeInfo, messageError);

			suppDoc.CSI_Code = Constants.SupportingDocumentCodes.C601;
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._00200;
			AssertNoMessageError("Additional Document code is not 00100", addInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR629()
	{
		var messageError = "(R629) Additional Information code 00100 and Supporting Document/Authorization code C516 cannot be used together.";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalDocument = declaration.AdditionalInfos.AddNew();
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNoMessageError("Empty declaration", additionalDocument.CSI_CodeInfo, messageError);

			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C516;
			additionalDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document is C516", additionalDocument.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			additionalDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is 53", additionalDocument.CSI_CodeInfo, messageError);

			additionalDocument.CSI_Code = Constants.AdditionalInfoCodes._00100;
			AssertHasMessageError("Additional Document code is 00100", additionalDocument.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._51;
			additionalDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Entry Instruction Procedure is not 53", additionalDocument.CSI_CodeInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._53;
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C651;
			additionalDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Supporting Document is not C516", additionalDocument.CSI_CodeInfo, messageError);

			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C516;
			additionalDocument.CSI_Code = Constants.AdditionalInfoCodes._00200;
			AssertNoMessageError("Additional Document code is not 00100", additionalDocument.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR632()
	{
		var messageError = "(R632) Additional Information code 00100 and Supporting Document/Authorization code C019 can coexist only with Additional Information code 4PL09.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var additionalDocument = declaration.AdditionalInfos.AddNew();

		AssertNoMessageError("Empty declaration", additionalDocument.CSI_CodeInfo, messageError);

		foreach (string procedureCode in Constants.ValidationLists.R632Procedures(Factory))
		{
			AssertCheckRule632ForProcedureCode(procedureCode);
		}
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
			additionalDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError($"Entry Instruction Procedure is {procedureCode}", additionalDocument.CSI_CodeInfo, messageError);

			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes.C019;
			additionalDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError($"Procedure = {procedureCode}, Supporting Document is C019", additionalDocument.CSI_CodeInfo, messageError);

			additionalDocument.CSI_Code = Constants.AdditionalInfoCodes._00100;
			AssertHasMessageError($"Procedure = {procedureCode}, Additional Document code is 00100", additionalDocument.CSI_CodeInfo, messageError);

			var additionalDocument2 = declaration.AdditionalInfos.AddNew();
			additionalDocument2.CSI_Code = Constants.AdditionalInfoCodes._4PL09;
			additionalDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError($"Procedure = {procedureCode}, Additional Document code 4PL09 exists", additionalDocument.CSI_CodeInfo, messageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		entry = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entry.PK;
		headerAdditionalInfo = invoice.AdditionalInfos.AddNew();
		lineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
		entryAdditionalInfo = entry.AdditionalInfos.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entry;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	AdditionalInfo headerAdditionalInfo;
	AdditionalInfo lineAdditionalInfo;
	AdditionalInfo entryAdditionalInfo;
}
