using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(ExportJobDeclarationValidation))]
sealed class ExportJobDeclarationValidationTest : BaseExportJobDeclarationValidationTest
{
	public void TestCheckJE_TransportIDInland_Mandatory_UCC6Validation()
	{
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;
		jobDeclaration.JE_TransportMeans = ExportInlandTransportTypeList.Codes._10;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportIDInlandInfo);
	}

	public void TestCheckJE_Trailer1RegNo()
	{
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_Trailer1RegNoInfo);
		}
	}

	public void TestCheckJE_AircraftRegistrationInland()
	{
		jobDeclaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_AircraftRegistrationInlandInfo);
	}

	public void TestCheckRuleR0007E()
	{
		Factory.AddCodeToCusMap_EUNAU(Constants.CusAuthorizationUsageType.C513);
		var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
		var office = jobDeclaration.CustomsOfficesForBinding.AddNew();
		var messageError = "(R0007E) A Decl. Customs Office code cannot be the same as a Customs Office of Presentation";

		jobDeclaration.JE_CustomsOffice = "PL1234";
		office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		office.CY_Data = "PL1111";
		cusAuthorizationUsage.AGC_Code = CusAuthorizationUsageType.C513;
		CombineAssertions(() =>
		{
			jobDeclaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageError("Different OfficeOfPresentation and JE_CustomsOffice", jobDeclaration.JE_CustomsOfficeInfo, messageError);

			office.CY_Data = "PL1234";
			jobDeclaration.Validation.ValidateJE_CustomsOffice();
			AssertHasMessageError("Same OfficeOfPresentation and JE_CustomsOffice", jobDeclaration.JE_CustomsOfficeInfo, messageError);

			office.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			jobDeclaration.Validation.ValidateJE_CustomsOffice();
			AssertNoMessageError("Same ActualExitOffice and JE_CustomsOffice", jobDeclaration.JE_CustomsOfficeInfo, messageError);
		});
	}

	public void TestCheckRuleR0002E()
	{
		var messageError = "[R0002E] The Customs Office code must start with PL.";

		CombineAssertions(() =>
		{
			jobDeclaration.JE_CustomsOffice = "EU123124";
			AssertHasMessageError("Don't start with PL", jobDeclaration.JE_CustomsOfficeInfo, messageError);

			jobDeclaration.JE_CustomsOffice = "PL123124";
			AssertNoMessageError("Start with PL", jobDeclaration.JE_CustomsOfficeInfo, messageError);
		});
	}

	public void TestCheckRuleR0840() => CombineAssertions(() =>
	{
		const string messageError = "[R0840] In CC515C, CC513C, CC613C or CC615C message for Carrier, ‘EOR’ or ‘TCU’ Registration Number is required.";

		jobDeclaration.JE_OH_ShippingLine = GetNewOrgHeader().PK;
		AssertHasMessageError("No Registration Number for Carrier", jobDeclaration.JE_OH_ShippingLineInfo, messageError);

		jobDeclaration.JE_OH_ShippingLine = GetNewOrgHeader("EOR", "AA 12345").PK;
		AssertNoMessageError("EORI Registration Number for Carrier present", jobDeclaration.JE_OH_ShippingLineInfo, messageError);

		jobDeclaration.JE_OH_ShippingLine = GetNewOrgHeader("TCU", "123451234512345").PK;
		AssertNoMessageError("TCUIN Registration Number for Carrier present", jobDeclaration.JE_OH_ShippingLineInfo, messageError);

		jobDeclaration.JE_OH_ShippingLine = GetNewOrgHeader("DRV", "ABC 123456").PK;
		AssertHasMessageError("Invalid type of Registration Number for Carrier", jobDeclaration.JE_OH_ShippingLineInfo, messageError);
	});

	public void TestCheckRuleR0940() => CombineAssertions(() =>
	{
		const string messageError = "[R0940] One of Registration Number: ‘EOR’, ‘TCU’, ‘PES’, ‘PAS’ or ‘DRV’ is required.";

		jobDeclaration.JE_OA_Representative = GetNewOrgHeader().Addresses.First().PK;
		AssertHasMessageError("No Registration Number for Representative", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

		jobDeclaration.JE_OA_Representative = GetNewOrgHeader("EOR", "AA 12345").Addresses.First().PK;
		AssertNoMessageError("EORI Registration Number for Representative present", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

		jobDeclaration.JE_OA_Representative = GetNewOrgHeader("TCU", "123451234512345").Addresses.First().PK;
		AssertNoMessageError("TCUIN Registration Number for Representative present", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

		jobDeclaration.JE_OA_Representative = GetNewOrgHeader("PAS", "AA 123456").Addresses.First().PK;
		AssertNoMessageError("Passport Number for Representative present", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

		jobDeclaration.JE_OA_Representative = GetNewOrgHeader("PES", "01020300456").Addresses.First().PK;
		AssertNoMessageError("PESEL Number for Representative present", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

		jobDeclaration.JE_OA_Representative = GetNewOrgHeader("DRV", "ABC 123456").Addresses.First().PK;
		AssertNoMessageError("Driver's License Number for Representative present", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

		jobDeclaration.JE_OA_DeclarantAddress = GetNewOrgHeader().Addresses.First().PK;
		AssertHasMessageError("No Registration Number for Declarant", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

		jobDeclaration.JE_OA_DeclarantAddress = GetNewOrgHeader("EOR", "AA 12345").Addresses.First().PK;
		AssertNoMessageError("EORI Registration Number for Declarant present", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

		jobDeclaration.JE_OA_DeclarantAddress = GetNewOrgHeader("TCU", "123451234512345").Addresses.First().PK;
		AssertNoMessageError("TCUIN Registration Number for Declarant present", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

		jobDeclaration.JE_OA_DeclarantAddress = GetNewOrgHeader("PAS", "AA 123456").Addresses.First().PK;
		AssertNoMessageError("Passport Number for Declarant present", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

		jobDeclaration.JE_OA_DeclarantAddress = GetNewOrgHeader("PES", "01020300456").Addresses.First().PK;
		AssertNoMessageError("PESEL Number for Declarant present", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

		jobDeclaration.JE_OA_DeclarantAddress = GetNewOrgHeader("DRV", "ABC 123456").Addresses.First().PK;
		AssertNoMessageError("Driver's License Number for Declarant present", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);
	});

	OrgHeader GetNewOrgHeader(string code = "", string codeValue = "")
	{
		var result = Factory.New<OrgHeader>();
		result.Addresses.AddNew();
		result.OH_Category = OrgConstants.Category.Business;

		if (code is not null && codeValue is not null)
		{
			var customsCode = result.CustomsCodes.AddNew();
			customsCode.OK_CustomsRegNo = codeValue;
			customsCode.OK_CodeType = code;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		}
		return result;
	}

	public void TestCheckRuleR343()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", eun);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeType(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "PL000EXP", "Office of Export", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExport);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "PL000EXT", "Office of Export", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
		Factory.Save();

		var messageError = "The code you have selected is not in the list.";

		CombineAssertions(() =>
		{
			jobDeclaration.JE_CustomsOffice = "PL000EXP";
			AssertNoMessageError("Valid", jobDeclaration.JE_CustomsOfficeInfo, messageError);

			jobDeclaration.JE_CustomsOffice = "PL000EXT";
			AssertHasMessageError("Invalid", jobDeclaration.JE_CustomsOfficeInfo, messageError);
		});
	}

	public void TestCheckJE_OA_Representative_R0010E()
	{
		var messageError = "[R0010E] Representative cannot be given.";
		var supplierHeader = Factory.New<OrgHeader>();
		var supplierPort = Factory.New<RefUNLOCO>();
		supplierPort.RL_Code = "1C3BB";
		supplierPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
		supplierHeader.OH_RL_NKClosestPort = supplierPort.RL_Code;
		var exporterHeader = Factory.New<OrgHeader>();
		var exporterPort = Factory.New<RefUNLOCO>();
		exporterPort.RL_Code = "E6B2A";
		exporterPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		exporterHeader.OH_RL_NKClosestPort = exporterPort.RL_Code;
		var exporterAddress = exporterHeader.Addresses.AddNew();
		var representativeHeader = Factory.New<OrgHeader>();
		var representativeAddress = representativeHeader.Addresses.AddNew();
		jobDeclaration.JE_OA_Representative = representativeAddress.PK;
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError("No Supplier And No Exporter", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

			jobDeclaration.JE_OH_Supplier = supplierHeader.PK;
			jobDeclaration.ExporterDocAddress.E2_OA_Address = exporterAddress.PK;
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError("EUN Countries", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

			exporterPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageError("Exporter is not EUN Country - representative not empty", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

			jobDeclaration.ExporterDocAddress.E2_OA_Address = ZGuid.Empty;
			supplierPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageError("Supplier as fallback is not EUN Country - representative not empty", jobDeclaration.JE_OA_RepresentativeInfo, messageError);

			jobDeclaration.JE_OA_Representative = ZGuid.Empty;
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError("Not EUN Country - representative empty", jobDeclaration.JE_OA_RepresentativeInfo, messageError);
		});
	}

	public void TestCheckJE_OA_Representative_DifferentEORI()
	{
		var messageError = "[R0893] EORI number for Representative must be different from Declarant EORI number.";
		var declarantHeader = Factory.New<OrgHeader>();
		var declarantAddress = declarantHeader.Addresses.AddNew();
		jobDeclaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		var representativeHeader = Factory.New<OrgHeader>();
		var representativeAddress = representativeHeader.Addresses.AddNew();
		jobDeclaration.JE_OA_Representative = representativeAddress.PK;
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError("No EORI", jobDeclaration.JE_OA_RepresentativeInfo, messageError);
			var declarantEORI = declarantAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Poland);
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError("One EORI", jobDeclaration.JE_OA_RepresentativeInfo, messageError);
			representativeAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "321", Core.Constants.CountryCodes.Poland);
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError("Different EORI", jobDeclaration.JE_OA_RepresentativeInfo, messageError);
			declarantEORI.OK_CustomsRegNo = "321";
			jobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertHasMessageError("Same EORI", jobDeclaration.JE_OA_RepresentativeInfo, messageError);
		});
	}

	public void TestCheckJE_RL_NKFinalDestination()
	{
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		var invoiceline1 = invoiceHeader.JobComInvoiceLines.AddNew();
		var invoiceline2 = invoiceHeader.JobComInvoiceLines.AddNew();

		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			invoiceline1.ZG_CountryOfDestination = Core.Constants.CountryCodes.India;
			invoiceline2.ZG_CountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			jobDeclaration.JE_RL_NKFinalDestination = "DEHAM";

			AssertHasWarningContaining("\"Destination\" value in Shipment Details section is different than values indicated in \"Country of Destination\" on Invoice Lines.", jobDeclaration.JE_RL_NKFinalDestinationInfo, "\"Destination\" value in Shipment Details section is different than values indicated in \"Country of Destination\" on Invoice Lines.");

			jobDeclaration.JE_RL_NKFinalDestination = "INMUM";
			AssertNoWarningContaining("\"Destination\" value in Shipment Details section is different than values indicated in \"Country of Destination\" on Invoice Lines.", jobDeclaration.JE_RL_NKFinalDestinationInfo, "\"Destination\" value in Shipment Details section is different than values indicated in \"Country of Destination\" on Invoice Lines.");

			jobDeclaration.JE_RL_NKFinalDestination = "";
			AssertNoWarningContaining("\"Destination\" value in Shipment Details section is different than values indicated in \"Country of Destination\" on Invoice Lines.", jobDeclaration.JE_RL_NKFinalDestinationInfo, "\"Destination\" value in Shipment Details section is different than values indicated in \"Country of Destination\" on Invoice Lines.");

			invoiceline1.ZG_CountryOfDestination = Core.Constants.CountryCodes.India;
			invoiceline2.ZG_CountryOfDestination = Core.Constants.CountryCodes.India;
			jobDeclaration.JE_RL_NKFinalDestination = "FRMRS";
			AssertHasWarningContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo,"The Country of Destination declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.");

			invoiceline2.ZG_CountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertNoWarningContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, "The Country of Destination declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.");

			invoiceline2.ZG_CountryOfDestination = Core.Constants.CountryCodes.India;
			jobDeclaration.JE_RL_NKFinalDestination = "";
			AssertNoWarningContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, "The Country of Destination declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.");

			invoiceline2.ZG_CountryOfDestination = "";
			jobDeclaration.JE_RL_NKFinalDestination = "FRMRS";
			AssertNoWarningContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, "The Country of Destination declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.");

			invoiceline2.ZG_CountryOfDestination = Core.Constants.CountryCodes.India;
			jobDeclaration.JE_RL_NKFinalDestination = "INMUM";
			AssertNoWarningContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, "The Country of Destination declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.");

			invoiceline1.ZG_CountryOfDestination = "";
			invoiceline2.ZG_CountryOfDestination = "";
			jobDeclaration.JE_RL_NKFinalDestination = "";
			jobDeclaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertHasMessageErrorContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, "A Country of Destination must be declared at header or line level.");

			jobDeclaration.JE_RL_NKFinalDestination = "INMUM";
			AssertNoMessageErrorContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, "A Country of Destination must be declared at header or line level.");

			invoiceline1.ZG_CountryOfDestination = Core.Constants.CountryCodes.India;
			jobDeclaration.JE_RL_NKFinalDestination = "";
			AssertNoMessageErrorContaining(jobDeclaration.JE_RL_NKFinalDestinationInfo, "A Country of Destination must be declared at header or line level.");
		});
	}

	public void TestCheckJE_OA_DeclarantAddress()
	{
		var messageError = "[R0010E] Declarant EORI number starting with EU country code is required";
		var supplierHeader = Factory.New<OrgHeader>();
		var supplierPort = Factory.New<RefUNLOCO>();
		supplierPort.RL_Code = "1C3BB";
		supplierPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
		supplierHeader.OH_RL_NKClosestPort = supplierPort.RL_Code;
		var exporterHeader = Factory.New<OrgHeader>();
		var exporterPort = Factory.New<RefUNLOCO>();
		exporterPort.RL_Code = "E6B2A";
		exporterPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		exporterHeader.OH_RL_NKClosestPort = exporterPort.RL_Code;
		var exporterAddress = exporterHeader.Addresses.AddNew();
		var declarantHeader = Factory.New<OrgHeader>();
		var declarantAddress = declarantHeader.Addresses.AddNew();
		jobDeclaration.JE_OA_DeclarantAddress = declarantAddress.PK;
		jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("No Supplier And No Exporter", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

			jobDeclaration.JE_OH_Supplier = supplierHeader.PK;
			jobDeclaration.ExporterDocAddress.E2_OA_Address = exporterAddress.PK;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("Supplier And Exporter EUN", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

			exporterPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError("Exporter not EUN", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

			jobDeclaration.ExporterDocAddress.E2_OA_Address = ZGuid.Empty;
			supplierPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError("Supplier as fallback not EUN", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

			var eori = declarantAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.UnitedStates);
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasMessageError("EORI not EUN", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);

			jobDeclaration.ExporterDocAddress.E2_OA_Address = exporterAddress.PK;
			exporterPort.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			jobDeclaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError("EORI EUN", jobDeclaration.JE_OA_DeclarantAddressInfo, messageError);
		});
	}

	public void TestCheckGNSS()
	{
		var messageError = "GNSS does not exists for the UNLOCO selected";
		CombineAssertions(() =>
		{
			jobDeclaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.X;
			jobDeclaration.Validation.ValidateGoodsLocationUNLocode();
			AssertNoMessageErrorContaining("QualifierOfTheIdentificationList.Codes.X", jobDeclaration.GoodsLocationUNLocodeInfo, messageError);

			jobDeclaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;
			jobDeclaration.Validation.ValidateGoodsLocationUNLocode();
			AssertHasMessageErrorContaining("QualifierOfTheIdentificationList.Codes.W", jobDeclaration.GoodsLocationUNLocodeInfo, messageError);

			jobDeclaration.GoodsLocationUNLocode = "DECLE";
			AssertHasMessageErrorContaining("Invalid GNSS", jobDeclaration.GoodsLocationUNLocodeInfo, messageError);

			jobDeclaration.GoodsLocationUNLocode = "BEAAA";
			AssertHasMessageErrorContaining("Invalid UNLocode", jobDeclaration.GoodsLocationUNLocodeInfo, messageError);
		});
	}

	public void TestCheckAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.X;
			jobDeclaration.Validation.ValidateJE_LocationOfGoods();
			AssertNoMessageErrorContaining("QualifierOfTheIdentificationList.Codes.X", jobDeclaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.Y;
			jobDeclaration.Validation.ValidateJE_LocationOfGoods();
			AssertHasMessageErrorContaining("QualifierOfTheIdentificationList.Codes.V", jobDeclaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			jobDeclaration.JE_LocationOfGoods = "abc";
			AssertNoMessageErrorContaining("Not empty AuthorisationNumber", jobDeclaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_VoyageFlightNo_Empty()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining("Empty Declaration", jobDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			jobDeclaration.ZG_BorderTransportMeans = "40";
			jobDeclaration.JE_VoyageFlightNo = ZString.Empty;
			jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining("Empty message error", jobDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_VoyageFlightNo = "123";
			jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining("No message error - not empty", jobDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_VoyageFlightNo = ZString.Empty;
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining("No message error - not air", jobDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			jobDeclaration.ZG_BorderTransportMeans = ZString.Empty;
			jobDeclaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining("No message error - empty ZG_BorderTransportMeans", jobDeclaration.JE_VoyageFlightNoInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_RN_NKTransportNationality_Empty()
	{
		var errorMessage = "You have not entered a [21] Nationality.";
		CombineAssertions(() =>
		{
			jobDeclaration.ZG_BorderTransportMeans = "40";
			jobDeclaration.JE_RN_NKTransportNationality = ZString.Empty;
			jobDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertHasMessageError("Empty message error", jobDeclaration.JE_RN_NKTransportNationalityInfo, errorMessage);
			jobDeclaration.JE_RN_NKTransportNationality = "PL";
			jobDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageError("No message error - not empty", jobDeclaration.JE_RN_NKTransportNationalityInfo, errorMessage);
			jobDeclaration.JE_RN_NKTransportNationality = ZString.Empty;
			jobDeclaration.ZG_BorderTransportMeans = ZString.Empty;
			jobDeclaration.Validation.ValidateJE_RN_NKTransportNationality();
			AssertNoMessageError("No message error - empty ZG_BorderTransportMeans", jobDeclaration.JE_RN_NKTransportNationalityInfo, errorMessage);
		});
	}

	public void TestCheckJE_VesselName_Empty()
	{
		CombineAssertions(() =>
		{
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("Empty Declaration", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_VesselName = ZString.Empty;
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			jobDeclaration.ZG_BorderTransportMeans = "10";
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertHasMessageErrorContaining("Empty message error - Sea", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			jobDeclaration.ZG_BorderTransportMeans = "21";
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("Empty message error - Rai", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Road;
			jobDeclaration.ZG_BorderTransportMeans = "30";
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("Empty message error - Roa", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("Empty message error - Mai", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("Empty message error - Fix", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			jobDeclaration.ZG_BorderTransportMeans = "80";
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("Empty message error - Iwt", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
			jobDeclaration.ZG_BorderTransportMeans = "10";
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("Empty message error - Own", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_VesselName = "123";
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("No message error - Not empty", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.JE_VesselName = ZString.Empty;
			jobDeclaration.ZG_BorderTransportMeans = ZString.Empty;
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("No message error - empty ZG_BorderTransportMeans", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
			jobDeclaration.ZG_BorderTransportMeans = "40";
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			jobDeclaration.Validation.ValidateJE_VesselName();
			AssertNoMessageErrorContaining("No message error - Air", jobDeclaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJE_TransportModeInland()
	{
		var messageError = "[C0843] Inland M.O.T. is required.";
		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;

		var case1ProcedureCodes = new ZString[] { Constants.ProcedureCodes._10 };
		var case2ProcedureCodes = new ZString[] { Constants.ProcedureCodes._76, Constants.ProcedureCodes._77 };

		var case1SubStyles = new ZString[] { Constants.SubStyleCodes.B, Constants.SubStyleCodes.C, Constants.SubStyleCodes.E, Constants.SubStyleCodes.F };
		var case2SubStyles = new ZString[] { Constants.SubStyleCodes.D };

		CombineAssertions(() =>
		{
			jobDeclaration.Validation.ValidateJE_TransportModeInland();
			AssertNoMessageError("Empty setup", jobDeclaration.JE_TransportModeInlandInfo, messageError);

			var euOfficeCode = jobDeclaration.CustomsOfficesForBinding.AddNew();
			euOfficeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			euOfficeCode.CY_Data = "AAA";

			jobDeclaration.JE_OfficeOfEntryExit = "AAA";
			CheckC0843("PRE equals EXT case 1", case1SubStyles, case1ProcedureCodes, false);
			CheckC0843("PRE equals EXT case 2", case2SubStyles, case2ProcedureCodes, false);

			jobDeclaration.JE_OfficeOfEntryExit = "BBB";
			CheckC0843("PRE different EXT case 1", case1SubStyles, case1ProcedureCodes);
			CheckC0843("PRE different EXT case 2", case2SubStyles, case2ProcedureCodes);

			euOfficeCode.CY_Data = ZString.Empty;
			jobDeclaration.JE_CustomsOffice = "CCC";
			CheckC0843("JE_CustomsOffice != JE_OfficeOfEntryExit case 1", case1SubStyles, case1ProcedureCodes);
			CheckC0843("JE_CustomsOffice != JE_OfficeOfEntryExit case 2", case2SubStyles, case2ProcedureCodes);

			jobDeclaration.JE_CustomsOffice = "BBB";
			CheckC0843("JE_CustomsOffice == JE_OfficeOfEntryExit case 1", case1SubStyles, case1ProcedureCodes, false);
			CheckC0843("JE_CustomsOffice == JE_OfficeOfEntryExit case 2", case2SubStyles, case2ProcedureCodes, false);
		});

		void CheckC0843(ZString assertMessagePrefix, ZString[] subStyles, ZString[] procedureCodes, bool shouldHaveMessageError = true)
		{
			entryInstruction.CEI_SubStyle = ZString.Empty;
			foreach (var procedureCode in procedureCodes)
			{
				entryInstruction.CEI_Procedure = ZString.Empty;
				jobDeclaration.Validation.ValidateJE_TransportModeInland();
				if (shouldHaveMessageError)
				{
					AssertHasMessageError($"{assertMessagePrefix} CEI_Procedure is not one of {string.Join(",", procedureCodes)}", jobDeclaration.JE_TransportModeInlandInfo, messageError);
				}
				else
				{
					AssertNoMessageError($"{assertMessagePrefix} CEI_Procedure is not one of {string.Join(",", procedureCodes)}", jobDeclaration.JE_TransportModeInlandInfo, messageError);
				}

				entryInstruction.CEI_Procedure = procedureCode;
				jobDeclaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageError($"{assertMessagePrefix} CEI_Procedure {procedureCode}", jobDeclaration.JE_TransportModeInlandInfo, messageError);
			}

			entryInstruction.CEI_Procedure = ZString.Empty;
			foreach (var subStyle in subStyles)
			{
				entryInstruction.CEI_SubStyle = subStyle;
				jobDeclaration.Validation.ValidateJE_TransportModeInland();
				AssertNoMessageError($"{assertMessagePrefix} CEI_SubStyle is {subStyle}", jobDeclaration.JE_TransportModeInlandInfo, messageError);
			}
		}
	}

	public void TestCheckJE_VesselName_RuleC0890()
	{
		const string errorMessage = "[C0890] Transport ID/Flight number is required.";

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			foreach (var (mode, flightNoIsRequired) in new (string, bool)[] {
						(Core.Constants.TransportModes.InlandWaterwayTransport, true),
						(Core.Constants.TransportModes.OwnPropulsion, true),
						(Core.Constants.TransportModes.Road, true),
						(Core.Constants.TransportModes.Sea, true),

						(Core.Constants.TransportModes.Air, false),
						(Core.Constants.TransportModes.FixedTransportInstallations, false),
						(Core.Constants.TransportModes.Mail, false),
						(Core.Constants.TransportModes.Rail, false) })
			{
				declaration.JE_TransportMode = mode;
				foreach (var (description, style, code, errorMessageIsExpected) in testCasesForC0890)
				{
					declaration.JE_EntryStyle = style;

					entryInstruction.CEI_Procedure = code;
					declaration.JE_VesselName = string.Empty;
					if (flightNoIsRequired && errorMessageIsExpected)
					{
						AssertHasMessageError($"{mode}, {style}: Vessel Name is empty and procedure code is {description}", declaration.JE_VesselNameInfo, errorMessage);
					}
					else
					{
						AssertNoMessageError($"{mode}, {style}: Vessel Name is empty and procedure code is {description}", declaration.JE_VesselNameInfo, errorMessage);
					}

					declaration.JE_VesselName = "12345";
					AssertNoMessageError($"{mode}, {style}: Vessel Name is not empty and procedure code is {description}", declaration.JE_VesselNameInfo, errorMessage);
				}
			}
		});
	}

	public void TestCheckJE_VoyageFlightNo_RuleC0890()
	{
		const string errorMessage = "[C0890] Transport ID/Flight number is required.";

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			foreach (var (mode, flightNoIsRequired) in new (string, bool)[] {
						(Core.Constants.TransportModes.Air, true),

						(Core.Constants.TransportModes.FixedTransportInstallations, false),
						(Core.Constants.TransportModes.InlandWaterwayTransport, false),
						(Core.Constants.TransportModes.Mail, false),
						(Core.Constants.TransportModes.OwnPropulsion, false),
						(Core.Constants.TransportModes.Rail, false),
						(Core.Constants.TransportModes.Road, false),
						(Core.Constants.TransportModes.Sea, false) })
			{
				declaration.JE_TransportMode = mode;
				foreach (var (description, style, code, errorMessageIsExpected) in testCasesForC0890)
				{
					declaration.JE_EntryStyle = style;
					entryInstruction.CEI_Procedure = code;

					declaration.JE_VoyageFlightNo = string.Empty;
					if (flightNoIsRequired && errorMessageIsExpected)
					{
						AssertHasMessageError($"{mode}, {style}: Flight No. is empty and procedure code is {description}", declaration.JE_VoyageFlightNoInfo, errorMessage);
					}
					else
					{
						AssertNoMessageError($"{mode}, {style}: Flight No. is empty and procedure code is {description}", declaration.JE_VoyageFlightNoInfo, errorMessage);
					}

					declaration.JE_VoyageFlightNo = "12345";
					AssertNoMessageError($"{mode}, {style}: Flight No. is not empty and procedure code is {description}", declaration.JE_VoyageFlightNoInfo, errorMessage);
				}
			}
		});
	}

	readonly (string, string, string, bool)[] testCasesForC0890 = new[]
	{
		("10", "EX", ProcedureCodes._10, true),
		("11", "EX", ProcedureCodes._11, true),
		("23", "EX", ProcedureCodes._23, true),
		("31", "EX", ProcedureCodes._31, true),
		("not in [10, 11, 23, 31]", "EX", ProcedureCodes._22, false),

		("76", "CO", ProcedureCodes._76, true),
		("77", "CO", ProcedureCodes._77, true),
		("not in [76, 77]", "CO", ProcedureCodes._22, false),
	};

	public void TestCheckJE_OfficeOfEntryExit_RuleR0082E() => CombineAssertions(() =>
	{
		const string errorMessage = "[R0082E] For Export Declaration with Sub Style ’R’ Customs Office of Export must be equal to Customs Office of Exit Declared";

		jobDeclaration.JE_CustomsOffice = "PL1234";
		jobDeclaration.JE_OfficeOfEntryExit = "PL4321";
		AssertNoMessageError("No error message when no CEI_SubStyle", jobDeclaration.JE_OfficeOfEntryExitInfo, errorMessage);

		var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = SubStyleCodes.B;
		jobDeclaration.Validation.ValidateJE_OfficeOfEntryExit();
		AssertNoMessageError("No error message when CEI_SubStyle is not R", jobDeclaration.JE_OfficeOfEntryExitInfo, errorMessage);

		entryInstruction.CEI_SubStyle = SubStyleCodes.R;
		jobDeclaration.Validation.ValidateJE_OfficeOfEntryExit();
		AssertHasMessageError("Error message when CEI_SubStyle is R and JE_CustomsOffice,JE_OfficeOfEntryExit different", jobDeclaration.JE_OfficeOfEntryExitInfo, errorMessage);

		jobDeclaration.JE_OfficeOfEntryExit = "PL1234";
		AssertNoMessageError("No error message when CEI_SubStyle is R and JE_CustomsOffice,JE_OfficeOfEntryExit same", jobDeclaration.JE_OfficeOfEntryExitInfo, errorMessage);

		var entryInstruction2 = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.CEI_SubStyle = SubStyleCodes.R;
		jobDeclaration.JE_OfficeOfEntryExit = "PL4321";
		AssertHasMessageError("Error message when CEI_SubStyle is R and JE_CustomsOffice,JE_OfficeOfEntryExit different", jobDeclaration.JE_OfficeOfEntryExitInfo, errorMessage);

		entryInstruction2.CEI_SubStyle = SubStyleCodes.B;
		jobDeclaration.Validation.ValidateJE_OfficeOfEntryExit();
		AssertNoMessageError("No error message when CEI_SubStyle is not all R", jobDeclaration.JE_OfficeOfEntryExitInfo, errorMessage);
	});

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration.JE_MessageType = MessageType;
	}

	string MessageType => MessageTypeList.Codes.Export;
}
