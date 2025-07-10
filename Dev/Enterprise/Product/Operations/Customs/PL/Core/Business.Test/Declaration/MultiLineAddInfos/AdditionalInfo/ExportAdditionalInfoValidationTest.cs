using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.PL.Business.Constants;
using UniversalReferenceCusSupportingInfoTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.CusSupportingInfoTypes;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportAdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Code_NotEntered()
	{
		const string notEnteredMessageError = "Please enter a Kind of Document.";

		var additionalInfo = GetExportAdditionalInfo();
		CombineAssertions(() =>
		{
			additionalInfo.Validation.ValidateCSI_SubType();
			AssertHasMessageError("Message Error", additionalInfo.CSI_SubTypeInfo, notEnteredMessageError);
			additionalInfo.CSI_SubType = "123";
			AssertNoMessageError("No Message Error", additionalInfo.CSI_SubTypeInfo, notEnteredMessageError);
		});
	}

	public void TestCheckCSI_Code_ListValidation()
	{
		const string wrongValueMessageError = "The code you have selected is not in the list.";

		var addInfo = GetExportAdditionalInfo();
		CombineAssertions(() =>
		{
			addInfo.CSI_SubType = "123";
			AssertHasMessageError("Message Error", addInfo.CSI_SubTypeInfo, wrongValueMessageError);
			addInfo.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			AssertNoMessageError("No Message Error", addInfo.CSI_SubTypeInfo, wrongValueMessageError);
		});
	}

	public void TestCheckCSI_ReferenceNumber()
	{
		const string emptyMessageError = "A reference number is required.";

		var additionalInfo = GetExportAdditionalInfo();
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		CombineAssertions(() =>
		{
			additionalInfo.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError("Message Error", additionalInfo.CSI_ReferenceNumberInfo, emptyMessageError);
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			additionalInfo.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError("No Message Error - Other Sub Type", additionalInfo.CSI_ReferenceNumberInfo, emptyMessageError);
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			additionalInfo.CSI_ReferenceNumber = "Ref";
			AssertNoMessageError("No Message Error - Not Empty", additionalInfo.CSI_ReferenceNumberInfo, emptyMessageError);
		});
	}

	public void TestCheckRuleR0091ECodeRequired()
	{
		const string messageError = "[R0091E] Supervising Customs Office ’SCO’ is required with Additional Information code '00100'.";

		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionDocument = invoiceLine.AdditionalInfos.AddNew();
		var office = declaration.CustomsOfficesForBinding.AddNew();

		CombineAssertions(() =>
		{
			additionDocument.CSI_Code = AdditionalInfoCodes._00100;
			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.CY_Code = EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain;
			office.Validation.ValidateAll();
			AssertHasRowMessageError("SCO code is required ", office, messageError);

			office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("SCO code exists", office, messageError);

			additionDocument.CSI_SubType = "AAA";
			office.CY_Data = ZString.Empty;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The SubType is not INF", office, messageError);

			additionDocument.CSI_Code = AdditionalInfoCodes._POW01;
			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The code of additional document is not 00100", office, messageError);

			additionDocument.CSI_Code = AdditionalInfoCodes._00100;
			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.Validation.ValidateAll();
			AssertHasRowMessageError("The Customs Office is not SupervisingCustomsOffice", office, messageError);

			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.CY_Code = EuOfficeCodesTypes.Codes.CentralOfficeCommonDomain;
			var officeExtra = declaration.CustomsOfficesForBinding.AddNew();
			officeExtra.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.Validation.ValidateAll();
			AssertHasRowMessageError("No SCO Code", office, messageError);

			officeExtra.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("No SCO Code", office, messageError);
		});
	}

	public void TestCheckRuleR0091EDataSame()
	{
		const string messageError = "[R0091E] Supervising Customs Office ’SCO’ must be equal Customs Office of Export with Additional Information code '00100'.";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionDocument = invoiceLine.AdditionalInfos.AddNew();
		var office = declaration.CustomsOfficesForBinding.AddNew();

		CombineAssertions(() =>
		{
			additionDocument.CSI_Code = AdditionalInfoCodes._00100;
			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			declaration.JE_CustomsOffice = "AAAA";
			office.CY_Data = "SSSS";
			office.Validation.ValidateAll();
			AssertHasRowMessageError("The Data is different from Customs Office of Export", office, messageError);

			office.CY_Data = "AAAA";
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The Data is the same with Customs Office of Export", office, messageError);

			additionDocument.CSI_SubType = "TTT";
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The SubType is not INF", office, messageError);

			additionDocument.CSI_Code = AdditionalInfoCodes._POW01;
			additionDocument.CSI_SubType = AdditionalInfoTypes.INF;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The code of additional document is not 00100", office, messageError);

			additionDocument.CSI_Code = AdditionalInfoCodes._00100;
			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office.Validation.ValidateAll();
			AssertNoRowMessageError("The Customs Office is not SupervisingCustomsOffice", office, messageError);

			var officeExtra = declaration.CustomsOfficesForBinding.AddNew();
			officeExtra.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			officeExtra.CY_Data = "SSSS";
			office.Validation.ValidateAll();
			AssertHasRowMessageError("The Data is different from Customs Office of Export", office, messageError);
		});
	}

	public void TestCheckRuleR0097E()
	{
		const string errorMessage = "[R0097E] Description must contain only two digits if Additional Documents contain Kind = ‘INF’ and Full Type = ‘EXP04’ code";
		var additionalInfo = GetExportAdditionalInfo();
		additionalInfo.CSI_Type = Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo;
		additionalInfo.CSI_SubType = Enterprise.Customs.PL.Business.Declaration.AdditionalInfoKindList.Codes.INF;
		additionalInfo.CSI_Code = Enterprise.Customs.PL.Business.Constants.AdditionalInfoCodes._EXP04;

		CombineAssertions(() =>
		{
			additionalInfo.CSI_Description = null;
			additionalInfo.Validation.ValidateCSI_Description();
			AssertHasMessageError("Null description", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Description = "";
			additionalInfo.Validation.ValidateCSI_Description();
			AssertHasMessageError("Empty description", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Description = "1A";
			additionalInfo.Validation.ValidateCSI_Description();
			AssertHasMessageError("Description with non-digits", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Description = "123";
			additionalInfo.Validation.ValidateCSI_Description();
			AssertHasMessageError("Description with more than two digits", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Description = "1";
			additionalInfo.Validation.ValidateCSI_Description();
			AssertHasMessageError("Description with only one digit", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Description = "01";
			additionalInfo.Validation.ValidateCSI_Description();
			AssertNoMessageError("Description with valid two digits", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Type = Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.ImportSad;
			additionalInfo.Validation.ValidateCSI_Description();
			AssertNoMessageError("Different CSI_Type", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Type = Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo;
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
			additionalInfo.Validation.ValidateCSI_Description();
			AssertNoMessageError("Different CSI_SubType", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			additionalInfo.CSI_Code = Enterprise.Customs.PL.Business.Constants.AdditionalInfoCodes._POW01;
			additionalInfo.Validation.ValidateCSI_Description();
			AssertNoMessageError("Different CSI_Code", additionalInfo.CSI_DescriptionInfo, errorMessage);
		});
	}

	public void TestCheckRuleR0029E()
	{
		Factory.AddCodeToCusMap_EUNAU(CusAuthorizationUsageType.C019);
		var messageError = "(R0029E) The Additional Information code '00100' is not allowed with Authorization code 'C019'";
		var additionalInfo = GetExportAdditionalInfo();
		var declaration = additionalInfo.Parent as JobDeclaration;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty declaration", additionalInfo.CSI_CodeInfo, messageError);

			cusAuthorizationUsage.AGC_Code = "C018";
			additionalInfo.CSI_Code = AdditionalInfoCodes._00100;
			AssertNoMessageErrorContaining("AuthorizationUsage Code is not C019", additionalInfo.CSI_CodeInfo, messageError);

			cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C019;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining("AuthorizationUsage Code is C019", additionalInfo.CSI_CodeInfo, messageError);

			additionalInfo.CSI_Code = "00101";
			AssertNoMessageErrorContaining("AdditionalInfo Code is not 00100", additionalInfo.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleR0038E()
	{
		const string errorMessage = "[R0038E] Only one Additional Document with Kind = ’INF’ and Type = ‘EXP15’ is allowed either on Entry Instruction, Invoice Headers or Invoice Lines.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var invoiceAdditionalInfo = invoice.AdditionalInfos.AddNew();
		var entryInstructionAdditionalInfo = entryInstruction.AdditionalInfos.AddNew();
		var invoiceLineAdditionalInfo = invoiceLine.AdditionalInfos.AddNew();
		entryInstructionAdditionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		invoiceAdditionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		invoiceLineAdditionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		CombineAssertions(() =>
		{
			invoiceAdditionalInfo.CSI_Code = AdditionalInfoCodes._EXP15;
			invoiceAdditionalInfo.Validation.ValidateAll();
			entryInstructionAdditionalInfo.Validation.ValidateAll();
			invoiceLineAdditionalInfo.Validation.ValidateAll();
			AssertNoRowMessageError("Entry Instruction Additional Info: Only one additional info document with CSI_Code = EXP15.", entryInstructionAdditionalInfo, errorMessage);
			AssertNoRowMessageError("Invoice Additional Info: Only one additional info document with CSI_Code = EXP15.", invoiceAdditionalInfo, errorMessage);
			AssertNoRowMessageError("Invoice Line Additional Info: Only one additional info document with CSI_Code = EXP15.", invoiceLineAdditionalInfo, errorMessage);

			entryInstructionAdditionalInfo.CSI_Code = AdditionalInfoCodes._EXP15;
			invoiceAdditionalInfo.Validation.ValidateAll();
			entryInstructionAdditionalInfo.Validation.ValidateAll();
			invoiceLineAdditionalInfo.Validation.ValidateAll();
			AssertHasRowMessageError("Entry Instruction Additional Info: Multiple additional info documents with CSI_Code = EXP15 at Invoice and Entry Instruction levels.", entryInstructionAdditionalInfo, errorMessage);
			AssertHasRowMessageError("Invoice Additional Info: Multiple additional info documents with CSI_Code = EXP15 at Invoice and Entry Instruction levels.", invoiceAdditionalInfo, errorMessage);
			AssertNoRowMessageError("Invoice Line Additional Info: Multiple additional info documents with CSI_Code = EXP15 at Invoice and Entry Instruction levels.", invoiceLineAdditionalInfo, errorMessage);

			entryInstructionAdditionalInfo.CSI_Code = AdditionalInfoCodes._4PL03;
			invoiceLineAdditionalInfo.CSI_Code = AdditionalInfoCodes._EXP15;
			invoiceAdditionalInfo.Validation.ValidateAll();
			entryInstructionAdditionalInfo.Validation.ValidateAll();
			invoiceLineAdditionalInfo.Validation.ValidateAll();
			AssertNoRowMessageError("Entry Instruction Additional Info: Multiple additional info documents with CSI_Code = EXP15 at Invoice and Invoice Line levels.", entryInstructionAdditionalInfo, errorMessage);
			AssertHasRowMessageError("Invoice Additional Info: Multiple additional info documents with CSI_Code = EXP15 at Invoice and Invoice Line levels.", invoiceAdditionalInfo, errorMessage);
			AssertHasRowMessageError("Invoice Line Additional Info: Multiple additional info documents with CSI_Code = EXP15 at Invoice and Invoice Line levels.", invoiceLineAdditionalInfo, errorMessage);
		});
	}

	public void TestCheckRuleR0083E_POW01_PCS01_EntryInstructionMissingDescription()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var additionalInfoInstruction = entryInstruction.AdditionalInfos.AddNew();
		AssertRuleR0083E(additionalInfoInstruction);
	}

	public void TestCheckRuleR0083E_POW01_PCS01_InvoiceHeaderMissingDescription()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();

		var additionalInfoHeader = invoice.AdditionalInfos.AddNew();
		AssertRuleR0083E(additionalInfoHeader);
	}

	public void TestCheckRuleR0083E_POW01_PCS01_InvoiceLineMissingDescription()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var additionalInfoInvoiceLine = invoiceLine.AdditionalInfos.AddNew();
		AssertRuleR0083E(additionalInfoInvoiceLine);
	}

	void AssertRuleR0083E(AdditionalInfo additionalInfo)
	{
		const string errorMessage = "[R0083E] Description is required for Additional Information Type ‘POW01’ or ‘PCS01’ and must contain IDSISC or email address.";
		CombineAssertions(() =>
		{
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			additionalInfo.CSI_Code = AdditionalInfoCodes._POW01;
			additionalInfo.Validation.ValidateCSI_Description();
			AssertNoMessageError("CSI_SubType <> INF", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			additionalInfo.CSI_Code = AdditionalInfoCodes._1PL17;
			additionalInfo.Validation.ValidateCSI_Description();
			AssertNoMessageError("CSI_Code <> POW01 or PCS01", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Code = AdditionalInfoCodes._POW01;
			additionalInfo.CSI_Description = "Test";
			AssertNoMessageError("CSI_Description <> Empty", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Code = AdditionalInfoCodes._POW01;
			additionalInfo.CSI_Description = "";
			AssertHasMessageError("CSI_Code = POW01 and CSI_Description is Empty", additionalInfo.CSI_DescriptionInfo, errorMessage);

			additionalInfo.CSI_Code = AdditionalInfoCodes._PCS01;
			additionalInfo.Validation.ValidateCSI_Description();
			AssertHasMessageError("CSI_Code = PCS01 and CSI_Description is Empty", additionalInfo.CSI_DescriptionInfo, errorMessage);

			using (PLCustomsDataRegistry.Instance.PCSEmailChannel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PL701071897800000"))
			{
				additionalInfo.CSI_Code = AdditionalInfoCodes._POW01;
				additionalInfo.Validation.ValidateCSI_Description();
				AssertHasMessageError("CSI_Code = POW01 and CSI_Description is Empty and PCSEmailChannel is set", additionalInfo.CSI_DescriptionInfo, errorMessage);

				additionalInfo.CSI_Code = AdditionalInfoCodes._PCS01;
				additionalInfo.Validation.ValidateCSI_Description();
				AssertNoMessageError("CSI_Code = PCS01 and CSI_Description is Empty and PCSEmailChannel is set", additionalInfo.CSI_DescriptionInfo, errorMessage);
			}
		});
	}

	public void TestCheckRuleR0085E()
	{
		const string errorMessage = "You have not entered Previous Document Type [NCLE].";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "INV1234";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.InvoiceHeader.JZ_InvoiceNumber = "INV1234";
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.EntryInstruction.CEI_SubStyle = SubStyleCodes.A;
		var additionalInfo = invoice.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;

		CombineAssertions(() =>
		{
			additionalInfo.CSI_Code = AdditionalInfoCodes._4PL03;
			AssertNoMessageError("When Substyle is not V or Z and additionalInfo.CSI_Code != 1PL18.", additionalInfo.CSI_CodeInfo, errorMessage);

			additionalInfo.CSI_Code = AdditionalInfoCodes._1PL18;
			AssertNoMessageError("When Substyle is not V or Z and additionalInfo.CSI_Code = 1PL18.", additionalInfo.CSI_CodeInfo, errorMessage);

			invoiceLine.EntryInstruction.CEI_SubStyle = SubStyleCodes.Z;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("When Substyle = Z and additionalInfo.CSI_Code = 1PL18 and PreviousDocument is null.", additionalInfo.CSI_CodeInfo, errorMessage);

			var previousDocument = invoice.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = PreviousDocumentCodes.NCLE;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("When Substyle = Z and additionalInfo.CSI_Code = 1PL18 and PreviousDocument Type = NCLE.", additionalInfo.CSI_CodeInfo, errorMessage);

			previousDocument.CSI_Code = PreviousDocumentCodes.AAD;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("When Substyle = Z and additionalInfo.CSI_Code = 1PL18 and PreviousDocument Type != NCLE.", additionalInfo.CSI_CodeInfo, errorMessage);

			invoiceLine.EntryInstruction.CEI_SubStyle = SubStyleCodes.V;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageError("When Substyle = V and additionalInfo.CSI_Code = 1PL18 and PreviousDocument Type != NCLE.", additionalInfo.CSI_CodeInfo, errorMessage);

			previousDocument.CSI_Code = PreviousDocumentCodes.NCLE;
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageError("When Substyle = V and additionalInfo.CSI_Code = 1PL18 and PreviousDocument Type = NCLE.", additionalInfo.CSI_CodeInfo, errorMessage);
		});
	}

	public void TestCheckRuleR0041E()
	{
		const string messageError = "[R0041E] When Exporter and Declarant both have same EORI then Additional Information with Type = 'EXP15' can't be declared.";

		var declarantHeader = Factory.New<OrgHeader>();
		var declarantAddress = declarantHeader.Addresses.AddNew();
		_ = declarantAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, customsRegNo: "123", countryCode: Core.Constants.CountryCodes.Poland);
		var exporterHeader = Factory.New<OrgHeader>();
		var exporterAddress = exporterHeader.Addresses.AddNew();
		var exporterEORI = exporterAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, customsRegNo: "123", countryCode: Core.Constants.CountryCodes.Germany);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var entryInstructionDocument = declaration.CustomsEntryInstructions.AddNew().AdditionalInfos.AddNew();
		var invoiceLineDocument = declaration.Invoices.AddNew().InvoiceLines.AddNew().AdditionalInfos.AddNew();

		CombineAssertions(() =>
		{
			entryInstructionDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Default setup", entryInstructionDocument.CSI_CodeInfo, messageError);

			entryInstructionDocument.CSI_Code = AdditionalInfoCodes._EXP15;
			entryInstructionDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("EXP15 but exporter and declarant are missing", entryInstructionDocument.CSI_CodeInfo, messageError);

			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			entryInstructionDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("EXP15 but exporter is missing", entryInstructionDocument.CSI_CodeInfo, messageError);

			declaration.ExporterDocAddress.E2_OA_Address = exporterAddress.PK;
			exporterEORI.OK_CustomsRegNo = "534";
			entryInstructionDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("EXP15 but exporter and declarant have different Eori", entryInstructionDocument.CSI_CodeInfo, messageError);

			exporterEORI.OK_CustomsRegNo = "123";
			entryInstructionDocument.Validation.ValidateCSI_Code();
			AssertHasMessageError("EXP15, exporter and declarant have same Eori", entryInstructionDocument.CSI_CodeInfo, messageError);

			entryInstructionDocument.CSI_Code = AdditionalInfoCodes._4PL04;
			entryInstructionDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Not EXP15 but exporter and declarant have same Eori", entryInstructionDocument.CSI_CodeInfo, messageError);

			invoiceLineDocument.CSI_Code = AdditionalInfoCodes._EXP15;
			entryInstructionDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("For the time being only entry instruction should validate this rule", invoiceLineDocument.CSI_CodeInfo, messageError);
		});
	}

	public void TestCheckRuleG0825_JobDeclarationLevel()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertRuleG0825(declaration.AdditionalInfos.AddNew());
	}

	public void TestCheckRuleG0825_InvoiceHeaderLevel()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		AssertRuleG0825(invoice.AdditionalInfos.AddNew());
	}

	public void TestCheckRuleG0825_InvoiceLineLevel()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertRuleG0825(invoiceLine.AdditionalInfos.AddNew());
	}

	public void TestCheckRuleG0825_EntryInstructionLevel()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertRuleG0825(entryInstruction.AdditionalInfos.AddNew(), shouldHaveError: false);
	}

	void AssertRuleG0825(AdditionalInfo additionalInfo1, bool shouldHaveError = true)
	{
		additionalInfo1.CSI_SubType = AdditionalInfoKindList.Codes.INF;

		const string errorMessage = "(G0825) Documents/Information common for the Entry must be entered in Entry Instruction.";
		CombineAssertions(() =>
		{
			additionalInfo1.CSI_Code = AdditionalInfoCodes._4PL03;
			additionalInfo1.Validation.ValidateAll();
			if (shouldHaveError)
			{
				AssertHasRowMessageError("additionalInfo1: CSI_Code = 4PL03. is only allowed on Entry Instruction level", additionalInfo1, errorMessage);
			}
			else
			{
				AssertNoRowMessageError("additionalInfo1: CSI_Code = 4PL03. is only allowed on Entry Instruction level", additionalInfo1, errorMessage);
			}
		});
	}

	public void TestCheckRuleR0039E()
	{
		const string expectedMessageError = "[R0039E] Description is required for Additional Information Type 'EXP15'.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var customsEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = customsEntryInstruction.PK;
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfo.CSI_Code = AdditionalInfoCodes._EXP15;

		CombineAssertions(() =>
		{
			additionalInfo.CSI_Description = "1 - 12345678 - MyCompany";
			AssertNoMessageError("CSI_Description is set", additionalInfo.CSI_DescriptionInfo, expectedMessageError);

			additionalInfo.CSI_Description = ZString.Empty;
			AssertHasMessageError("CSI_Description is not set.", additionalInfo.CSI_DescriptionInfo, expectedMessageError);
		});
	}

	public void TestCheckR0100E() => CombineAssertions(() =>
	{
		const string twoY121_Error = "[R0100E]: You have entered more than one Additional Reference (Kind = 'REF') with Full Type = 'Y121'.";
		const string twoY798_Error = "[R0100E]: You have entered more than one Additional Reference (Kind = 'REF') with Full Type = 'Y798'.";
		const string Y121_and_Y798_Error = "[R0100E]: You have entered Additional Reference (Kind = 'REF') with Full Type = ('Y121' as well as 'Y798').";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var customsEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = customsEntryInstruction.PK;
		var additionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
		additionalInfo1.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
		additionalInfo2.CSI_SubType = AdditionalInfoKindList.Codes.REF;

		additionalInfo1.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y121;
		additionalInfo2.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y121;
		additionalInfo1.Validation.ValidateCSI_Code();
		AssertHasMessageError("Two Y121", additionalInfo1.CSI_CodeInfo, twoY121_Error);

		additionalInfo1.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y798;
		additionalInfo2.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y798;
		additionalInfo1.Validation.ValidateCSI_Code();
		AssertHasMessageError("Two Y798", additionalInfo1.CSI_CodeInfo, twoY798_Error);

		additionalInfo1.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y121;
		additionalInfo2.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y798;
		additionalInfo1.Validation.ValidateCSI_Code();
		AssertHasMessageError("Y121 and Y798", additionalInfo1.CSI_CodeInfo, Y121_and_Y798_Error);

		additionalInfo1.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y121;
		additionalInfo2.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y123;
		additionalInfo1.Validation.ValidateCSI_Code();
		AssertNoMessageError("No two Y121 error", additionalInfo1.CSI_CodeInfo, twoY121_Error);
		AssertNoMessageError("No two Y798 error", additionalInfo1.CSI_CodeInfo, twoY798_Error);
		AssertNoMessageError("No Y121 and Y798 error", additionalInfo1.CSI_CodeInfo, Y121_and_Y798_Error);
	});

	public void TestCheckR0101E() => CombineAssertions(() =>
	{
		const string error = "[R0101E]: For codes Y121 and Y798, the quantity (always expressed in TCE for Y121 (FGAS) and in KGM for Y798 (ODS)) must be entered in the \"Reference number\" element and is mandatory in the format n 16.6. Value must be greater than zero.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var customsEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = customsEntryInstruction.PK;
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		additionalInfo.CSI_ReferenceNumber = ZString.Empty;

		additionalInfo.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y121;
		additionalInfo.Validation.ValidateCSI_ReferenceNumber();
		AssertHasMessageError("Y121", additionalInfo.CSI_ReferenceNumberInfo, error);

		additionalInfo.CSI_Code = UniversalReferenceCusSupportingInfoTypes.Y798;
		additionalInfo.Validation.ValidateCSI_ReferenceNumber();
		AssertHasMessageError("Y798", additionalInfo.CSI_ReferenceNumberInfo, error);

		additionalInfo.CSI_ReferenceNumber = "Test";
		AssertNoMessageError("Not empty ReferenceNumber", additionalInfo.CSI_ReferenceNumberInfo, error);
	});

	AdditionalInfo GetExportAdditionalInfo()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		return additionalInfo;
	}
}
