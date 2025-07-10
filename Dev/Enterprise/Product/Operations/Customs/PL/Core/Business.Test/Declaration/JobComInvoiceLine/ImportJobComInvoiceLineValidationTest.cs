using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckProcedureCodeBase() => CombineAssertions(() =>
	{
		invoiceLine.ProcedureCodeBase = "10";
		AssertNoMessageErrorContaining("Non-empty Procedure Code", invoiceLine.ProcedureCodeBaseInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceLine.ProcedureCodeBase = ZString.Empty;
		AssertHasMessageErrorContaining("Empty Procedure Code", invoiceLine.ProcedureCodeBaseInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckPreviousProcedureCode() => CombineAssertions(() =>
	{
		invoiceLine.PreviousProcedureCode = "10";
		AssertNoMessageErrorContaining("Non-empty Previous Procedure Code", invoiceLine.PreviousProcedureCodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceLine.PreviousProcedureCode = ZString.Empty;
		AssertHasMessageErrorContaining("Empty Previous Procedure Code", invoiceLine.PreviousProcedureCodeInfo, MandatoryValidation.YouHaveNotEntered);
	});

	public void TestCheckRuleR1043() => CombineAssertions(() =>
	{
		const string messageError = "(R1043) – An additional info code 00500 is required for each invoice line (it is recommended to add the additional information code 00500 in the invoice header or in Declaration/Misc).";

		var otherInvoice = declaration.Invoices.AddNew();
		var otherLine = otherInvoice.InvoiceLines.AddNew();
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No 00500 add info", invoiceLine, messageError);

		var addInfo = otherLine.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AdditionalInfoCodes._00500;
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError("Other invoice has 00500 add info", invoiceLine, messageError);

		addInfo = invoiceLine.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AdditionalInfoCodes._00500;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("Invoice line 00500 add info", invoiceLine, messageError);

		invoiceLine.AdditionalInfos.RemoveAll();
		addInfo = invoice.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AdditionalInfoCodes._00500;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("Invoice 00500 add info", invoiceLine, messageError);

		invoice.AdditionalInfos.RemoveAll();
		addInfo = entryInstruction.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AdditionalInfoCodes._00500;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("Declaration 00500 add info", invoiceLine, messageError);
	});

	public void TestCheckRuleR1007() => CombineAssertions(() =>
	{
		const string errorMessage = "(R1007) – An additional info code 0PL12 is required for each invoice line (it is recommended to add the additional information code 0PL12 in the invoice header or in Declaration/Misc).";

		var otherInvoice = declaration.Invoices.AddNew();
		var otherInvoiceLine = otherInvoice.InvoiceLines.AddNew();
		otherInvoiceLine.JI_CEI = entryInstruction.PK;
		var supportingDocument = otherInvoiceLine.SupportingDocuments.AddNew();

		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("No supporting document with code C512/C513/C514", invoiceLine, errorMessage);

		supportingDocument.CSI_Code = SupportingDocumentCodes.C512;
		var addInfo = invoiceLine.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AdditionalInfoCodes._0PL10;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError($"Supporting document with code C512, no add info code {AdditionalInfoCodes._0PL12}", invoiceLine, errorMessage);

		var otherAddInfo = otherInvoiceLine.AdditionalInfos.AddNew();
		otherAddInfo.CSI_Code = AdditionalInfoCodes._0PL12;
		invoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError($"Other invoice line has add info code {AdditionalInfoCodes._0PL12}", invoiceLine, errorMessage);

		otherAddInfo.CSI_Code = AdditionalInfoCodes._0PL10;
		addInfo.CSI_Code = AdditionalInfoCodes._0PL12;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("Invoice line add info", invoiceLine, errorMessage);

		invoiceLine.AdditionalInfos.RemoveAll();
		addInfo = invoice.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AdditionalInfoCodes._0PL12;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("Invoice add info", invoiceLine, errorMessage);

		invoice.AdditionalInfos.RemoveAll();
		addInfo = entryInstruction.AdditionalInfos.AddNew();
		addInfo.CSI_Code = AdditionalInfoCodes._0PL12;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError("Declaration add info", invoiceLine, errorMessage);

		addInfo.CSI_Code = AdditionalInfoCodes._0PL10;
		invoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError($"No add info with code {AdditionalInfoCodes._0PL12}", invoiceLine, errorMessage);
	});

	public void TestCheckRuleR916() => CombineAssertions(() =>
	{
		const string messageError = "Supporting document code N935 is required (Invoice number).";

		entryInstruction.CEI_Procedure = ProcedureCodes._40;
		invoice.JZ_ValuationCode = InvoiceHeaderValuationCodes._11;
		invoiceLine.JI_ValuationCode = MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
		invoiceLine.Validation.ValidateJI_ValuationCode();
		AssertHasMessageError("Error message", invoiceLine.JI_ValuationCodeInfo, messageError);

		var supportingDoc = invoiceLine.SupportingDocuments.AddNew();
		supportingDoc.CSI_Code = EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935;
		invoiceLine.Validation.ValidateJI_ValuationCode();
		AssertNoMessageError("No error message - invoice line supporting document", invoiceLine.JI_ValuationCodeInfo, messageError);

		invoiceLine.SupportingDocuments.RemoveAll();
		supportingDoc = invoice.SupportingDocuments.AddNew();
		supportingDoc.CSI_Code = EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935;
		invoiceLine.Validation.ValidateJI_ValuationCode();
		AssertNoMessageError("No error message - invoice supporting document", invoiceLine.JI_ValuationCodeInfo, messageError);

		invoice.SupportingDocuments.RemoveAll();
		supportingDoc = declaration.SupportingDocuments.AddNew();
		supportingDoc.CSI_Code = EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935;
		invoiceLine.Validation.ValidateJI_ValuationCode();
		AssertNoMessageError("No error message - declaration supporting document", invoiceLine.JI_ValuationCodeInfo, messageError);

		declaration.SupportingDocuments.RemoveAll();
		invoice.JZ_ValuationCode = "12";
		invoiceLine.Validation.ValidateJI_ValuationCode();
		AssertNoMessageError("No error message - invoice valuation code different than 11", invoiceLine.JI_ValuationCodeInfo, messageError);

		invoice.JZ_ValuationCode = InvoiceHeaderValuationCodes._11;
		invoiceLine.JI_ValuationCode = "2";
		invoiceLine.Validation.ValidateJI_ValuationCode();
		AssertNoMessageError("No error message - invoice line valuation code different than 1", invoiceLine.JI_ValuationCodeInfo, messageError);

		invoiceLine.JI_ValuationCode = MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;
		entryInstruction.CEI_Procedure = "50";
		invoiceLine.Validation.ValidateJI_ValuationCode();
		AssertNoMessageError("No error message - procedure not starting with 4 or 6", invoiceLine.JI_ValuationCodeInfo, messageError);
	});

	public void TestCheckForRuleR422()
	{
		const string messageError = "R422 – for CPC=53 the Additional Procedure code from the list (D01..D30, D51) is required.";
		additionalProcedure.CY_Code = "D50";

		invoiceLine.JI_Procedure = "52";
		invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
		AssertNoMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);

		invoiceLine.JI_Procedure = "53";
		invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
		AssertHasMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);

		for (var i = 1; i <= 30; i++)
		{
			invoiceLine.JI_Procedure = "53";
			var searchedItem = $"D{i.ToString().PadLeft(2, '0')}";
			additionalProcedure.CY_Code = searchedItem;
			invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
			AssertNoMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);
		}

		invoiceLine.JI_Procedure = "53";
		additionalProcedure.CY_Code = "D51";
		invoiceLine.Validation.ValidateAdditionalProcedureCodesAsString();
		AssertNoMessageError(invoiceLine.AdditionalProcedureCodesAsStringInfo, messageError);
	}

	public void TestCheckRuleR1585()
	{
		const string warning = "(R1585) The EU code will be used in the customs declaration";
		invoiceLine.JI_CountryOfOrigin = CountryCodes.Poland;

		CombineAssertions(() =>
		{
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertHasWarning("Country of origin is member state of EU", invoiceLine.JI_CountryOfOriginInfo, warning);

			invoiceLine.JI_CountryOfOrigin = CountryCodes.India;
			invoiceLine.Validation.ValidateJI_CountryOfOrigin();
			AssertNoWarning("Country of origin is not a member state of EU", invoiceLine.JI_CountryOfOriginInfo, warning);
		});
	}

	public void TestCheckForRuleR436()
	{
		const string messageError = "R436 - Tariff Code is mandatory.";

		invoiceLine.JI_Tariff = string.Empty;
		additionalProcedure.CY_Code = "1PL";
		invoiceLine.JI_Procedure = "7500";

		CombineAssertions(() =>
		{
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("Mandatory Tariff Code is empty", invoiceLine.JI_TariffInfo, messageError);

			invoiceLine.JI_Tariff = "11111111";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("No error as Tariff Code not empty", invoiceLine.JI_TariffInfo, messageError);

			additionalProcedure.CY_Code = "1PL";
			invoiceLine.JI_Tariff = string.Empty;
			invoiceLine.JI_Procedure = "7600";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("Tariff Code not mandatory for Concession Code", invoiceLine.JI_TariffInfo, messageError);

			additionalProcedure.CY_Code = "2PL";
			invoiceLine.JI_Procedure = "7500";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("Tariff Code not mandatory for Procedure Code", invoiceLine.JI_TariffInfo, messageError);
		});
	}

	public void TestCheckForRuleR240()
	{
		const string messageError = "R240 - Missing Additional Information Code for ‘4PL04’ for customs procedure requested ‘49’.";

		additionalInfo.CSI_Code = string.Empty;
		invoiceLine.JI_Procedure = string.Empty;
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertNoMessageError(invoiceLine.JI_ProcedureInfo, messageError);

		invoiceLine.JI_Procedure = "41";
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertNoMessageError(invoiceLine.JI_ProcedureInfo, messageError);

		invoiceLine.JI_Procedure = "49";
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertHasMessageError(invoiceLine.JI_ProcedureInfo, messageError);

		additionalInfo.CSI_Code = "4PL04";
		invoiceLine.JI_Procedure = "41";
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertNoMessageError(invoiceLine.JI_ProcedureInfo, messageError);
	}

	public void TestCheckForRuleR860()
	{
		const string messageError = "R860 - Invalid Additional Information Code 00100 for customs procedure requested ‘71’.";

		additionalInfo.CSI_Code = "00100";
		invoiceLine.JI_Procedure = string.Empty;

		invoiceLine.Validation.ValidateJI_Procedure();
		AssertNoMessageError(invoiceLine.JI_ProcedureInfo, messageError);

		invoiceLine.JI_Procedure = "71";
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertHasMessageError(invoiceLine.JI_ProcedureInfo, messageError);

		additionalInfo.CSI_Code = "00101";
		invoiceLine.Validation.ValidateJI_Procedure();
		AssertNoMessageError(invoiceLine.JI_ProcedureInfo, messageError);
	}

	public void TestCheckForRuleR416_JI_ProcedureInfo()
	{
		TestCheckForRuleR416(invoiceLine.JI_ProcedureInfo, invoiceLine.Validation.ValidateJI_Procedure);
	}

	public void TestCheckForRuleR416_ProcedureCodeBaseInfo()
	{
		TestCheckForRuleR416(invoiceLine.ProcedureCodeBaseInfo, invoiceLine.PLValidationOrNull!.ValidateProcedureCodeBase);
	}

	void TestCheckForRuleR416(ZPropertyInfo propertyInfo, Action validatePropertyInfo) => CombineAssertions(() =>
	{
		const string messageError = "(R416) – The Customs Procedure Detail code is not valid because the declaration doesn't have a goods destination country code.";

		declaration.JE_GoodsDestination = ZString.Empty;
		invoiceLine.JI_Procedure = "1111";
		declaration.JE_GoodsDestination = CountryCodes.Poland;
		validatePropertyInfo();
		AssertNoMessageError($"GoodsDestination is not empty", propertyInfo, messageError);

		declaration.JE_GoodsDestination = ZString.Empty;
		var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();

		additionalProcedureCode.CY_Code = "2PL";
		AssertNoMessageError("GoodsDestination is empty|Valid Additional Procedure 2PL", propertyInfo, messageError);

		additionalProcedureCode.CY_Code = "XXX";
		validatePropertyInfo();
		AssertHasMessageError("GoodsDestination is empty|Invalid Additional Procedure", propertyInfo, messageError);

		declaration.JE_GoodsDestination = CountryCodes.Poland;
		validatePropertyInfo();
		AssertNoMessageError("GoodsDestination is not empty|Invalid Additional Procedure", propertyInfo, messageError);
	});

	public void TestCheckRuleR229()
	{
		const string messageError = "(R229) C651 Supporting document is missing or e-AD position number for C651 is invalid.";
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		var suppDoc = declaration.SupportingDocuments.AddNew();
		CombineAssertions(() =>
		{
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals("Set invoice line be related to instruction", invoiceLine.JI_CEI, instruction.PK);

			instruction.CEI_SubStyle = "A";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoMessageError("Set instruction CEI_SubStyle as 'A', not 'C'", invoiceLine.JI_CEIInfo, messageError);

			additionalInfo.CSI_Code = "4PL12";
			invoiceLine.JI_Procedure = "4500";
			additionalProcedure.CY_Code = "A11";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageError("Set invoice line JI_Procedure start with '45'", invoiceLine.JI_CEIInfo, messageError);

			invoiceLine.JI_Procedure = "9600";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageError("Set invoice line JI_Procedure start with '96'", invoiceLine.JI_CEIInfo, messageError);

			instruction.CEI_SubStyle = "C";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoMessageError("Set instruction CEI_SubStyle as 'C'", invoiceLine.JI_CEIInfo, messageError);

			additionalInfo.CSI_Code = "4PL11";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageError("Set additional info CSI_Code as '4PL11', not '4PL12'", invoiceLine.JI_CEIInfo, messageError);

			invoiceLine.JI_Procedure = "4400";
			additionalProcedure.CY_Code = "F06";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageError("Set invoice line JI_Procedure start with '44', not '45', '68' or '96'", invoiceLine.JI_CEIInfo, messageError);

			suppDoc.CSI_Code = "C651";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageError("Set supporting doc CSI_Code as 'C651'", invoiceLine.JI_CEIInfo, messageError);

			suppDoc.CSI_Description = "523452345";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertNoMessageError("Fill supporting doc CSI_Description only with numbers", invoiceLine.JI_CEIInfo, messageError);

			suppDoc.CSI_Description = "523452345A";
			invoiceLine.Validation.ValidateJI_CEI();
			AssertHasMessageError("Fill supporting doc CSI_Description not only with numbers", invoiceLine.JI_CEIInfo, messageError);
		});
	}

	public void TestCheckJI_ValuationDateOverrideIsValidZDateTime()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("2 InvoiceLines have empty JI_ValuationDateOverride", invoiceLine1.JI_ValuationDateOverrideInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine1.JI_ValuationDateOverride = new ZDateTime(2022, 03, 01);
			invoiceLine1.Validation.ValidateJI_ValuationDateOverride();
			invoiceLine2.Validation.ValidateJI_ValuationDateOverride();
			AssertNoMessageErrorContaining("InvoiceLine1 - Only 1 InvoiceLine have empty JI_ValuationDateOverride", invoiceLine1.JI_ValuationDateOverrideInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("InvoiceLine2 - Only 1 InvoiceLine have empty JI_ValuationDateOverride", invoiceLine2.JI_ValuationDateOverrideInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJI_OA_ExporterAddress()
	{
		var orgHeaderBUS = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderBUS.OH_Category = OrgConstants.Category.Business;
		orgHeaderBUS.OH_FullName = new ZString('1', 80);

		var orgHeaderNAT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNAT.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNAT.OH_FullName = new ZString('1', 40).Insert(3, " ");

		var orgHeaderNATEmpty = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNATEmpty.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNATEmpty.OH_FullName = ZString.Empty;

		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

		PLOrgHeaderValidationHelperTest.AssertValidationOrganizationName(invoiceLine.JI_OA_ExporterAddressInfo, orgHeaderBUS, orgHeaderNAT, orgHeaderNATEmpty, orgAddress);
	}

	public void TestJI_OA_ConsigneeAddress()
	{
		var orgHeaderBUS = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderBUS.OH_Category = OrgConstants.Category.Business;
		orgHeaderBUS.OH_FullName = new ZString('1', 80);

		var orgHeaderNAT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNAT.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNAT.OH_FullName = new ZString('1', 40).Insert(3, " ");

		var orgHeaderNATEmpty = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNATEmpty.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNATEmpty.OH_FullName = ZString.Empty;

		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

		PLOrgHeaderValidationHelperTest.AssertValidationOrganizationName(invoiceLine.JI_OA_ConsigneeAddressInfo, orgHeaderBUS, orgHeaderNAT, orgHeaderNATEmpty, orgAddress);
	}

	public void TestCheckRuleR425()
	{
		const string messageError = "(R425) for Quota only following preference codes are allowed 120, 123, 125, 128, 220, 223, 225, 320, 323, 325, 420";
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty InvoiceLine", invoiceLine.JI_PrimaryPreferenceInfo, messageError);

			invoiceLine.JI_ConcessionOrder = "asd";
			invoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertHasMessageErrorContaining("Not Empty JI_ConcessionOrder", invoiceLine.JI_PrimaryPreferenceInfo, messageError);

			foreach (var primaryNumber in ValidationLists.R425AndR481PrimaryPreferenceCodeList(Factory))
			{
				invoiceLine.JI_ConcessionOrder = "asd";
				invoiceLine.JI_PrimaryPreference = primaryNumber;
				AssertNoMessageErrorContaining($"Primary Number {primaryNumber}", invoiceLine.JI_PrimaryPreferenceInfo, messageError);

				invoiceLine.JI_ConcessionOrder = ZString.Empty;
				AssertNoMessageErrorContaining($"Primary Number {primaryNumber} JI_ConcessionOrder empty", invoiceLine.JI_PrimaryPreferenceInfo, messageError);
			}

			invoiceLine.JI_ConcessionOrder = "asd";
			invoiceLine.JI_PrimaryPreference = "qwe";
			AssertHasMessageErrorContaining("Primary Number not in R425AndR481PrimaryPreferenceCodeList", invoiceLine.JI_PrimaryPreferenceInfo, messageError);

			invoiceLine.JI_ConcessionOrder = ZString.Empty;
			invoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertNoMessageErrorContaining("Empty JI_ConcessionOrder", invoiceLine.JI_PrimaryPreferenceInfo, messageError);
		});
	}

	public void TestCheckRule1502()
	{
		const string messageError = "(R1502)The condition of direct import is not met";
		invoiceLine.JI_CountryOfOrigin = CountryCodes.Poland;
		invoiceLine.ZG_CountryOfSupply = CountryCodes.Germany;
		entryInstruction.CEI_Procedure = ProcedureCodes._40;
		additionalInfo.CSI_Code = AdditionalInfoCodes._00100;
		invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes._220;

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("Error message as condition of direct Import not met", invoiceLine.JI_PrimaryPreferenceInfo, messageError);

			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes._120;
			AssertNoMessageErrorContaining("Primary preference code does not start with 2, 3 or 4", invoiceLine.JI_PrimaryPreferenceInfo, messageError);

			invoiceLine.ZG_CountryOfSupply = CountryCodes.Poland;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes._220;
			AssertNoMessageErrorContaining("Country of supply is same as country of origin", invoiceLine.JI_PrimaryPreferenceInfo, messageError);

			invoiceLine.ZG_CountryOfSupply = CountryCodes.Poland;
			entryInstruction.CEI_Procedure = ProcedureCodes._31;
			invoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertNoMessageErrorContaining("Entry Instruction procedure code does not start with 4 or 6", invoiceLine.JI_PrimaryPreferenceInfo, messageError);

			entryInstruction.CEI_Procedure = ProcedureCodes._40;
			additionalInfo.CSI_Code = AdditionalInfoCodes._1PL17;
			invoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertNoMessageErrorContaining("Additional info with code 1PL17 is present", invoiceLine.JI_PrimaryPreferenceInfo, messageError);
		});
	}

	public void TestCheckRuleR481()
	{
		const string messageError = "(R481) for the given preference code [36], [39] Quota number is required";
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty InvoiceLine", invoiceLine.JI_ConcessionOrderInfo, messageError);

			invoiceLine.JI_ConcessionOrder = "asd";
			AssertNoMessageErrorContaining("Not Empty JI_ConcessionOrder", invoiceLine.JI_ConcessionOrderInfo, messageError);

			foreach (var primaryNumber in ValidationLists.R425AndR481PrimaryPreferenceCodeList(Factory))
			{
				invoiceLine.JI_PrimaryPreference = primaryNumber;
				invoiceLine.JI_ConcessionOrder = "asd";
				AssertNoMessageErrorContaining($"Primary Number {primaryNumber}", invoiceLine.JI_ConcessionOrderInfo, messageError);

				invoiceLine.JI_ConcessionOrder = ZString.Empty;
				AssertHasMessageErrorContaining("Empty JI_ConcessionOrder", invoiceLine.JI_ConcessionOrderInfo, messageError);
			}

			invoiceLine.JI_PrimaryPreference = "qwe";
			invoiceLine.Validation.ValidateJI_ConcessionOrder();
			AssertNoMessageErrorContaining("Primary Number not in R425AndR481PrimaryPreferenceCodeList", invoiceLine.JI_ConcessionOrderInfo, messageError);
		});
	}

	public void TestCheckJI_MarkModel_Invalid()
	{
		var invalidMarkModelMessageError = "Invalid Make, Model name.";
		AddPLCodesForCarDetailsRequired();
		AddCarMarkModelCodesForTests();

		invoiceLine.JI_Tariff = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty declaration", invoiceLine.JI_MarkModelInfo, invalidMarkModelMessageError);

			invoiceLine.JI_MarkModel = "1111";
			AssertNoMessageErrorContaining("Mark Model is valid car id", invoiceLine.JI_MarkModelInfo, invalidMarkModelMessageError);

			invoiceLine.JI_MarkModel = "abc,asd";
			AssertHasMessageError("JI_MarkModel is invalid make/model name", invoiceLine.JI_MarkModelInfo, invalidMarkModelMessageError);

			invoiceLine.JI_Tariff = "87032110";
			invoiceLine.PLValidationOrNull?.ValidateJI_MarkModel();
			AssertHasMessageError("Invalid JI_MarkModel name while Tariff requires car information to be present", invoiceLine.JI_MarkModelInfo, invalidMarkModelMessageError);

			invoiceLine.JI_MarkModel = "mark1,model1";
			AssertNoMessageErrorContaining("Valid JI_MarkModel is present", invoiceLine.JI_MarkModelInfo, invalidMarkModelMessageError);
		});
	}

	public void TestCheckJI_MarkModel_Empty()
	{
		var notEnteredMessageError = "You have not entered a Make, Model.";
		AddPLCodesForCarDetailsRequired();
		AddCarMarkModelCodesForTests();

		invoiceLine.JI_Tariff = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty declaration", invoiceLine.JI_MarkModelInfo, notEnteredMessageError);

			invoiceLine.JI_Tariff = "87032110";
			invoiceLine.JI_MarkModel = ZString.Empty;
			AssertHasMessageError("empty JI_MarkModel while Tariff requires car information to be present", invoiceLine.JI_MarkModelInfo, notEnteredMessageError);

			invoiceLine.JI_MarkModel = "mark1,model1";
			AssertNoMessageErrorContaining("JI_MarkModel is valid make/model name", invoiceLine.JI_MarkModelInfo, notEnteredMessageError);

			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.FirstVehicle.CVH_VehicleIdentificationNumber = "12345667";
			invoiceLine.JI_MarkModel = ZString.Empty;
			AssertHasMessageError("empty JI_MarkModel while CusVehicle/CusEngine properties are not empty", invoiceLine.JI_MarkModelInfo, notEnteredMessageError);

			invoiceLine.JI_MarkModel = "mark1,model1";
			AssertNoMessageErrorContaining("Valid JI_MarkModel is present", invoiceLine.JI_MarkModelInfo, notEnteredMessageError);
		});
	}

	public void TestCheckProcedureCodeBase_R211_R412()
		=> CheckProcedureCodeBase_R211_R412_R411(
			errorMessage: "(R211/R412) Fiscal role code FR2 is missing.",
			getFiscalCodePresentText: cfr_code => cfr_code == FiscalReferenceCodeList.Codes.FR2_Customer
				? "fiscal role code 'FR2' is present in the fiscal references"
				: "fiscal role code 'FR2' is not present in the fiscal references",
			(cfr_code: FiscalReferenceCodeList.Codes.FR2_Customer, procedureCodeBase: ProcedureCodes._42, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR2_Customer, procedureCodeBase: ProcedureCodes._63, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR2_Customer, procedureCodeBase: ProcedureCodes._54, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR5_Vendor, procedureCodeBase: ProcedureCodes._63, errorExpected: true),
			(cfr_code: FiscalReferenceCodeList.Codes.FR5_Vendor, procedureCodeBase: ProcedureCodes._54, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR5_Vendor, procedureCodeBase: ProcedureCodes._42, errorExpected: true),
			(cfr_code: FiscalReferenceCodeList.Codes.FR2_Customer, procedureCodeBase: ProcedureCodes._42, errorExpected: false));

	public void TestCheckProcedureCodeBase_R411()
		=> CheckProcedureCodeBase_R211_R412_R411(
			errorMessage: "(R411) Fiscal role code FR1 or FR3 is required for the requested procedure specified.",
			getFiscalCodePresentText: cfr_code => cfr_code == FiscalReferenceCodeList.Codes.FR1_Importer || cfr_code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative
				? $"fiscal role code '{cfr_code}' is present in the fiscal references"
				: "fiscal role code 'FR1' or 'FR3' is not present in the fiscal references",
			(cfr_code: FiscalReferenceCodeList.Codes.FR1_Importer, procedureCodeBase: ProcedureCodes._42, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR1_Importer, procedureCodeBase: ProcedureCodes._63, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR1_Importer, procedureCodeBase: ProcedureCodes._54, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, procedureCodeBase: ProcedureCodes._63, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, procedureCodeBase: ProcedureCodes._54, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, procedureCodeBase: ProcedureCodes._42, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR5_Vendor, procedureCodeBase: ProcedureCodes._63, errorExpected: true),
			(cfr_code: FiscalReferenceCodeList.Codes.FR5_Vendor, procedureCodeBase: ProcedureCodes._54, errorExpected: false),
			(cfr_code: FiscalReferenceCodeList.Codes.FR5_Vendor, procedureCodeBase: ProcedureCodes._42, errorExpected: true),
			(cfr_code: FiscalReferenceCodeList.Codes.FR1_Importer, procedureCodeBase: ProcedureCodes._42, errorExpected: false));

	public void TestCheckProcedureCodeBase_R417()
	{
		var errorMessage = "(R417) The Supporting document code 3DK5 is required for the requested procedure specified with the Fiscal Role code FR3.";

		var info = invoiceLine.ProcedureCodeBaseInfo;

		invoiceLine.FiscalReferences.RemoveAndDeleteAll();
		var fiscalReference1 = invoiceLine.FiscalReferences.AddNew();
		fiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
		var fiscalReference2 = invoiceLine.FiscalReferences.AddNew();

		var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument1.CSI_Code = "346";
		var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();

		var testValuesAndExpectedError = new (string cfr_code, string csi_code, string procedureCodeBase, bool errorExpected)[] {
			(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, "7664", ProcedureCodes._42, true),
			(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, "7664", ProcedureCodes._63, true),
			(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, "7664", ProcedureCodes._54, false),
			(FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth, "7664", ProcedureCodes._42, false),
			(FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth, "7664", ProcedureCodes._63, false),
			(FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth, "7664", ProcedureCodes._54, false),
			(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, SupportingDocumentCodes._3DK5, ProcedureCodes._42, false),
			(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, SupportingDocumentCodes._3DK5, ProcedureCodes._54, false),
			(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, SupportingDocumentCodes._3DK5, ProcedureCodes._63, false),
			(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, "7664", ProcedureCodes._63, true) };

		CombineAssertions(() =>
		{
			foreach (var (cfr_code, csi_code, procedureCodeBase, errorExpected) in testValuesAndExpectedError)
			{
				if (fiscalReference2.CFR_Code != cfr_code)
				{
					fiscalReference2.CFR_Code = cfr_code;
				}

				if (supportingDocument2.CSI_Code != csi_code)
				{
					supportingDocument2.CSI_Code = csi_code;
				}

				if (invoiceLine.ProcedureCodeBase != procedureCodeBase)
				{
					invoiceLine.ProcedureCodeBase = procedureCodeBase;
				}
				else
				{
					invoiceLine.PLValidationOrNull?.ValidateProcedureCodeBase();
				}

				var fiscalCodePresentText = cfr_code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative
					? "fiscal role code 'FR3' is present in the fiscal references"
					: "fiscal role code 'FR3' is not present in the fiscal references";
				var specialSupporingDocumentCodePresentText = csi_code == SupportingDocumentCodes._3DK5
					? $"supporing document with code '{SupportingDocumentCodes._3DK5}' is present in the inv.Line"
					: $"supporing document with code '{SupportingDocumentCodes._3DK5}' is not present in the inv.Line";
				if (errorExpected)
				{
					AssertHasMessageError($"ProcedureCodeBase is {procedureCodeBase}, {specialSupporingDocumentCodePresentText}, {fiscalCodePresentText}", info, errorMessage);
				}
				else
				{
					AssertNoMessageError($"ProcedureCodeBase is {procedureCodeBase}, {specialSupporingDocumentCodePresentText}, {fiscalCodePresentText}", info, errorMessage);
				}
			}
		});
	}

	void CheckProcedureCodeBase_R211_R412_R411(
		string errorMessage,
		Func<string, string> getFiscalCodePresentText,
		params (string cfr_code, string procedureCodeBase, bool errorExpected)[] testValuesAndExpectedError)
	{
		var info = invoiceLine.ProcedureCodeBaseInfo;

		invoiceLine.FiscalReferences.RemoveAndDeleteAll();
		var fiscalReference1 = invoiceLine.FiscalReferences.AddNew();
		fiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;
		var fiscalReference2 = invoiceLine.FiscalReferences.AddNew();

		CombineAssertions(() =>
		{
			foreach (var (cfr_code, procedureCodeBase, errorExpected) in testValuesAndExpectedError)
			{
				if (fiscalReference2.CFR_Code != cfr_code)
				{
					fiscalReference2.CFR_Code = cfr_code;
				}

				if (invoiceLine.ProcedureCodeBase != procedureCodeBase)
				{
					invoiceLine.ProcedureCodeBase = procedureCodeBase;
				}
				else
				{
					invoiceLine.PLValidationOrNull?.ValidateProcedureCodeBase();
				}

				var fiscalCodePresentText = getFiscalCodePresentText(cfr_code);
				if (errorExpected)
				{
					AssertHasMessageError($"ProcedureCodeBase is {procedureCodeBase}, {fiscalCodePresentText}", info, errorMessage);
				}
				else
				{
					AssertNoMessageError($"ProcedureCodeBase is {procedureCodeBase}, {fiscalCodePresentText}", info, errorMessage);
				}
			}
		});
	}

	void AddPLCodesForCarDetailsRequired()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CarDetailsRequiredCodes;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "PLCodesForCarDetails");
		helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Poland, codeType, "87032110", "tariff for make model is required", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();
	}

	void AddCarMarkModelCodesForTests()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CarMarkModel;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Mark, Model");
		helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Poland, codeType, "1111", "mark1,model1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Poland, codeType, "2222", "mark2,model2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();
	}

	public void TestCheckJI_DateForDutyOverride()
	{
		PopulateExchangeRateData();

		var partialMessageErrro = "The EUR (CUD type) exchange rate for";
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("Empty declaration", invoiceLine.JI_DateForDutyOverrideInfo, partialMessageErrro);

			invoiceLine.JI_DateForDutyOverride = ZDateTime.Today;
			AssertNoMessageErrorContaining("Date with exchange rate declaration", invoiceLine.JI_DateForDutyOverrideInfo, partialMessageErrro);

			invoiceLine.JI_DateForDutyOverride = ZDateTime.Today.AddDays(1);
			AssertHasMessageErrorContaining("Date with no exchange rate declaration", invoiceLine.JI_DateForDutyOverrideInfo, partialMessageErrro);
		});
	}

	void PopulateExchangeRateData()
	{
		var exchangeRate = RefExchangeRate.New(Factory);
		exchangeRate.RE_StartDate = ZDateTime.Today;
		exchangeRate.RE_ExpiryDate = ZDateTime.Today;
		exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		exchangeRate.RE_SellRate = 3.3333m;
		exchangeRate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

		Factory.Save();
	}

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		additionalProcedure = invoiceLine.AdditionalProcedureCodes.AddNew();
		additionalInfo = invoiceLine.AdditionalInfos.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	AdditionalProcedureCode additionalProcedure;
	AdditionalInfo additionalInfo;
	CusEntryInstruction entryInstruction;
}
