using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.LicenseManager;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.US.Business.AgencyRequirementsValidator;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExportAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_LicenseValue()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(declaration.Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32 });

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine.US_LicenseNo = "TRE";
			invoiceLine.US_LicenseValue = ZDecimal.Zero;
			invoiceLine.AddInfoValidation.ValidateUS_LicenseValue();
			AssertHasMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.MissingLicenseValue);

			invoiceLine.US_LicenseValue = 12m;
			AssertNoMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.MissingLicenseValue);

			invoiceLine.US_LicenseNo = ZString.Empty;
			invoiceLine.US_LicenseValue = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.MissingLicenseValue);

			invoiceLine.US_LicenseNo = LicenseExemptionTypeList.Codes.NLR;
			invoiceLine.AddInfoValidation.ValidateUS_LicenseValue();
			AssertNoMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.MissingLicenseValue);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C32;
			invoiceLine.US_LicenseNo = "STA";
			invoiceLine.AddInfoValidation.ValidateUS_LicenseValue();
			AssertNoMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.MissingLicenseValue);

			invoiceLine.JI_LinePrice = 120m;
			invoiceLine.US_LicenseValue = 210m;
			AssertHasMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.GreaterThanLineValue);

			invoiceLine.JI_Tariff = "9801100000";
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.AddInfoValidation.ValidateUS_LicenseValue();
			AssertNoMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.GreaterThanLineValue);

			invoiceLine.JI_Tariff = "98038801";
			invoiceLine.AddInfoValidation.ValidateUS_LicenseValue();
			AssertHasMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.GreaterThanLineValue);

			invoiceLine.US_LicenseValue = 120m;
			AssertNoMessageError(invoiceLine.US_LicenseValueInfo, ExportAddInfoJobComInvoiceLineValidation.GreaterThanLineValue);

			invoiceLine.US_LicenseValue = -180m;
			AssertHasErrors(invoiceLine.US_LicenseValueInfo);
		}

		public void TestCheckUS_IsUsedVehicle()
		{
			declaration.US_ExportCode = ExportInformationCodeList.Codes.HH;
			invoiceLine.US_IsUsedVehicle = true;
			AssertHasMessageError("Vehicle data cannot be sent for Household goods", invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoNotAllowed);
			AssertNoMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoRequired);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.HV;
			invoiceLine.US_IsUsedVehicle = false;
			AssertHasMessageError("Vehicle data is mandatory for HV", invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoRequired);
			AssertNoMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoNotAllowed);

			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._4Postdeparture;
			invoiceLine.US_IsUsedVehicle = true;
			AssertHasMessageError("USED VEH NOT ALLOWED – OPT 4: NOT US, PR", invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoNotAllowedForPostDepartureNotUSOrPR);

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.US_IsUsedVehicle = true;
			AssertNoMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoNotAllowedForPostDepartureNotUSOrPR);

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.PuertoRico;
			invoiceLine.US_IsUsedVehicle = true;
			AssertNoMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoNotAllowedForPostDepartureNotUSOrPR);

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
			invoiceLine.US_IsUsedVehicle = true;
			AssertHasMessageError("USED VEH NOT ALLOWED – OPT 4: NOT US, PR", invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoNotAllowedForPostDepartureNotUSOrPR);

			declaration.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoiceLine.US_IsUsedVehicle = false;
			AssertNoMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.VehicleInfoNotAllowedForPostDepartureNotUSOrPR);

			var startDate = ZDateTime.Now.AddMonths(-1);
			var endDate = ZDateTime.Now.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates).ZZZ_DataGrouping;
			var tariffType1 = helper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.ScheduleB);
			var tariffType2 = helper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(dataGrouping, tariffType1.PK, "3333333333", startDate, endDate);
			var tariff2 = helper.LoadOrCreateNewTariff(dataGrouping, tariffType2.PK, "4444444444", startDate, endDate);
			var tariff3 = helper.LoadOrCreateNewTariff(dataGrouping, tariffType1.PK, "5555555555", startDate, endDate);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, dataGrouping, tariffType1.ZZI_TariffType);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, dataGrouping, tariffType2.ZZI_TariffType);
			var attribute1 = helper.CreateNewOrGetExistingTariffAttribute(attributeName1.ZY6_Name, UniversalReferenceConstants.TariffAttributeTypes.Values.Mandatory, tariff1);
			var attribute2 = helper.CreateNewOrGetExistingTariffAttribute(attributeName2.ZY6_Name, UniversalReferenceConstants.TariffAttributeTypes.Values.Mandatory, tariff2);
			Factory.Save();

			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			invoiceLine.US_IsUsedVehicle = false;
			AssertHasMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.TariffNumberRequiresUsedVehicleReporting);

			invoiceLine.US_IsUsedVehicle = true;
			AssertNoMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.TariffNumberRequiresUsedVehicleReporting);

			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			invoiceLine.US_IsUsedVehicle = false;
			AssertHasMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.TariffNumberRequiresUsedVehicleReporting);

			invoiceLine.US_IsUsedVehicle = true;
			AssertNoMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.TariffNumberRequiresUsedVehicleReporting);

			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.JI_Tariff = tariff3.ZZ1_TariffCode;
			invoiceLine.US_IsUsedVehicle = false;
			AssertNoMessageError(invoiceLine.US_IsUsedVehicleInfo, ExportAddInfoJobComInvoiceLineValidation.TariffNumberRequiresUsedVehicleReporting);
		}

		public void TestCheckUS_ExportCode()
		{
			string messageError = "The code you have selected is not in the list.";
			invoiceLine.US_ExportCode = "";
			AssertNoMessageError(invoiceLine.US_ExportCodeInfo, messageError);

			invoiceLine.US_ExportCode = "BA";
			AssertHasMessageError(invoiceLine.US_ExportCodeInfo, messageError);

			var list = new ExportInformationCodeList();
			foreach (CodeDescriptionPair pair in list)
			{
				invoiceLine.US_ExportCode = pair.Code;
				AssertNoMessageError(invoiceLine.US_ExportCodeInfo, messageError);
			}

			invoiceLine.US_ExportCode = " ";
			AssertNoMessageError(invoiceLine.US_ExportCodeInfo, messageError);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.MS;
			messageError = LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C30, ExportInformationCodeList.Codes.MS);
			AssertHasMessageError(invoiceLine.US_ExportCodeInfo, messageError);

			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.OS;
			AssertNoMessageError(invoiceLine.US_ExportCodeInfo, messageError);

			invoiceLine.US_ExportCode = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_ExportCodeInfo);
		}

		public void TestCheckUS_LicenseNo()
		{
			var factory = declaration.Factory;
			var exportDate = invoiceLine.ExportDateForLicenseType;

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C60 });

			declaration.US_LicenseType = "";
			declaration.US_LicenseNo = "";
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			string messageError = LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C30, "", Factory, exportDate);

			invoiceLine.US_LicenseNo = "";
			AssertHasMessageError(invoiceLine.US_LicenseNoInfo, messageError);
			invoiceLine.US_LicenseNo = "LIC2343";
			AssertNoMessageError(invoiceLine.US_LicenseNoInfo, messageError);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C60;
			invoiceLine.US_LicenseNo = "ABC";
			AssertHasMessageError(invoiceLine.US_LicenseNoInfo, LicenseNumberValidation.C60LicenseInvalid);
			invoiceLine.US_LicenseNo = LicenseExemptionTypeList.Codes.DY6;
			AssertNoMessageError(invoiceLine.US_LicenseNoInfo, LicenseNumberValidation.C60LicenseInvalid);
		}

		public void TestCheckUS_JurisdictionNumber()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESJurisdictionNumber, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				invoiceLine.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.AircraftAndAssociatedEquipment;
				Assert("US_JurisdictionNumber is read-only when US_DDTCUSMLCategoryCode is not equal to 21", invoiceLine.US_JurisdictionNumberInfo.ReadOnly);

				invoiceLine.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.MiscellaneousArticles;
				invoiceLine.US_JurisdictionNumber = "";
				AssertHasMessageError(invoiceLine.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoiceLine.US_JurisdictionNumber = "1CJ1234567";
				AssertHasMessageError(invoiceLine.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoiceLine.US_JurisdictionNumber = "CJ12345678";
				AssertHasMessageError(invoiceLine.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoiceLine.US_JurisdictionNumber = "CJ 1234567";
				AssertHasMessageError(invoiceLine.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoiceLine.US_JurisdictionNumber = "CJ1234567";
				AssertNoMessageError(invoiceLine.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoiceLine.US_JurisdictionNumber = "CJ1234-67";
				AssertHasMessageError(invoiceLine.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);

				invoiceLine.US_JurisdictionNumber = "CJ 1234-67";
				AssertNoMessageError(invoiceLine.US_JurisdictionNumberInfo, USAddInfoValidation.JurisdictionNumberInvalid);
			}
		}

		public void TestCheckUS_LicenseNoWithT10LicenseTypeInInvoiceLine()
		{
			declaration.US_LicenseType = "";
			declaration.US_LicenseNo = "";
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.T10;

			invoiceLine.US_LicenseNo = "12-4678";
			AssertHasMessageError(invoiceLine.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
			invoiceLine.US_LicenseNo = "AA-2014-1";
			AssertHasMessageError(invoiceLine.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
			invoiceLine.US_LicenseNo = "AA20141";
			AssertNoMessageError(invoiceLine.US_LicenseNoInfo, LicenseNumberValidation.T10LicenseInvalid);
		}

		public void TestCheckUS_ECCN()
		{
			var factory = declaration.Factory;
			var exportDate = invoiceLine.ExportDateForLicenseType;

			var helper = new UniversalReferenceTestDataHelper(factory);

			var refCusCode1B613 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "1B613", "1B613", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode1B613.PK, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);

			var refCusCode9A515 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "9A515", "9A515", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCode9A515.PK, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C35, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.E01 });

			factory.Save();

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			string messageError = LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "", factory, exportDate);

			invoiceLine.US_ECCN = "";
			AssertHasMessageError(invoiceLine.US_ECCNInfo, messageError);
			invoiceLine.US_ECCN = "C2343";
			AssertNoMessageError(invoiceLine.US_ECCNInfo, messageError);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C32;
			invoiceLine.US_ECCN = "1C351";
			AssertHasMessageError(invoiceLine.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C351", USAESLicenseCode.Codes.C32));

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine.US_ECCN = "1C354";
			AssertHasMessageError(invoiceLine.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C354", USAESLicenseCode.Codes.C33));

			invoiceLine.US_ECCN = "1C356";
			AssertNoMessageErrors(invoiceLine.US_ECCNInfo);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C35;
			invoiceLine.US_ECCN = "1B613";
			AssertNoMessageErrors(invoiceLine.US_ECCNInfo);

			invoiceLine.US_ECCN = "9A515";
			AssertNoMessageErrors(invoiceLine.US_ECCNInfo);

			invoiceLine.US_ECCN = "1L356";
			AssertHasMessageError(invoiceLine.US_ECCNInfo, "The ECCN number 1L356 is invalid for License Type C35.");

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.E01;
			invoiceLine.US_ECCN = "1C354";
			AssertHasMessageError(invoiceLine.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowedMessage, invoiceLine.US_LicenseType));

			invoiceLine.US_ECCN = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowedMessage, invoiceLine.US_LicenseType));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			invoice.UltimateConsigneeDocAddress.OrganisationPK = org.PK;
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine.US_ECCN = "";
			AssertHasMessageError(invoiceLine.US_ECCNInfo, ECCNValidation.ECCNIsRequiredMessageForSpecialCountry);

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Russia;
			invoiceLine.AddInfoValidation.ValidateUS_ECCN();
			AssertHasMessageError(invoiceLine.US_ECCNInfo, ECCNValidation.ECCNIsRequiredMessageForSpecialCountry);

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Venezuela;
			invoiceLine.AddInfoValidation.ValidateUS_ECCN();
			AssertHasMessageError(invoiceLine.US_ECCNInfo, ECCNValidation.ECCNIsRequiredMessageForSpecialCountry);

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			invoiceLine.AddInfoValidation.ValidateUS_ECCN();
			AssertNoMessageError(invoiceLine.US_ECCNInfo, ECCNValidation.ECCNIsRequiredMessageForSpecialCountry);

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			invoice.UltimateConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
			invoice.IntermediateConsigneeDocAddress.OrganisationPK = org.PK;
			invoiceLine.AddInfoValidation.ValidateUS_ECCN();
			AssertHasMessageError(invoiceLine.US_ECCNInfo, ECCNValidation.ECCNIsRequiredMessageForSpecialCountry);

			invoice.IntermediateConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
			invoice.US_UltimateDestinationCountry = Core.Constants.CountryCodes.China;
			invoiceLine.AddInfoValidation.ValidateUS_ECCN();
			AssertHasMessageError(invoiceLine.US_ECCNInfo, ECCNValidation.ECCNIsRequiredMessageForSpecialCountry);

			invoice.US_UltimateDestinationCountry = Core.Constants.CountryCodes.Canada;
			invoiceLine.AddInfoValidation.ValidateUS_ECCN();
			AssertNoMessageError(invoiceLine.US_ECCNInfo, ECCNValidation.ECCNIsRequiredMessageForSpecialCountry);
		}

		public void TestCheckUS_ECCNWhenUSLicenseTypeC40OrC35()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var refCusCodeS94 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, USAESLicenseCode.Codes.S94, USAESLicenseCode.Codes.S94, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeECCN3C992 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "3C992", "3C992", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeS94PK = refCusCodeS94.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeECCN3C992.PK, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.MEU, "3C992");

			var refCusCodeC30 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C30, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeECCN3C993 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "3C993", "3C993", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC30PK = refCusCodeC30.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeECCN3C993.PK, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.MEU, "3C993");
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC30PK, RefCusCodeListAttributeTypes.Codes.LicenseTypeCodes, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.BureauOfIndustryAndSecurity);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			declaration.US_LicenseType = USAESLicenseCode.Codes.C30;
			declaration.US_ECCN = "3C992";

			var org01 = Factory.New<OrgHeader>();
			org01.FillWithValidTestData();
			org01.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Russia;
			org01.OH_RL_NKClosestPort = "RUTE@";

			var org02 = Factory.New<OrgHeader>();
			org02.FillWithValidTestData();
			org02.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedArabEmirates;
			org02.OH_RL_NKClosestPort = "AETE@";

			var org03 = Factory.New<OrgHeader>();
			org03.FillWithValidTestData();
			org03.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Venezuela;
			org03.OH_RL_NKClosestPort = "VETE@";

			var invoice01 = declaration.Invoices.AddNew();
			invoice01.UltimateConsigneeDocAddress.OrganisationPK = org01.PK;

			var invoice02 = declaration.Invoices.AddNew();
			invoice02.UltimateConsigneeDocAddress.OrganisationPK = org02.PK;
			var line01 = invoice01.InvoiceLines.AddNew();
			var line02 = invoice02.InvoiceLines.AddNew();

			line01.US_LicenseType = USAESLicenseCode.Codes.S94;
			line01.US_ECCN = "3C992";
			line01.AddInfoValidation.ValidateUS_ECCN();
			AssertHasMessageError("RU - Should have error message on LicenseType", line01.US_ECCNInfo, ECCNValidation.LicenseTypeMustGOVorBISWhenECCNHasMEUAttribute);

			invoice01.UltimateConsigneeDocAddress.OrganisationPK = org03.PK;
			line01.AddInfoValidation.ValidateUS_ECCN();
			AssertHasMessageError("VE - Should have error message on LicenseType", line01.US_ECCNInfo, ECCNValidation.LicenseTypeMustGOVorBISWhenECCNHasMEUAttribute);

			line02.US_LicenseType = USAESLicenseCode.Codes.C30;
			line02.US_ECCN = "3C993";
			line02.AddInfoValidation.ValidateUS_ECCN();
			AssertNoMessageError("Should have no error message on LicenseType", line02.US_ECCNInfo, ECCNValidation.LicenseTypeMustGOVorBISWhenECCNHasMEUAttribute);
		}

		public void TestCheckUS_ECCNWhenUSLicenseType32OrC33()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eccnNumber1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "0A606", "0A606", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumber1.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.NotEligible);

			var eccnNumber2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "3A611", "3A611", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumber2.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.CountryCodes.Australia);
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumber2.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.CountryCodes.UnitedKingdom);
			Factory.Save();

			var licenseTypeList = new ZString[] { USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33 };

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			foreach(var licenseType in licenseTypeList)
			{
				line.US_LicenseType = licenseType;
				line.US_ECCN = "0A606";
				AssertHasMessageError(line.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				invoice.US_UltimateDestinationCountry = ZString.Empty;
				line.US_ECCN = "3A611";
				AssertHasMessageError(line.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				invoice.US_UltimateDestinationCountry = Core.Constants.CountryCodes.Australia;
				line.AddInfoValidation.ValidateUS_ECCN();
				AssertNoMessageError(line.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
				invoice.US_UltimateDestinationCountry = Core.Constants.CountryCodes.UnitedKingdom;
				line.AddInfoValidation.ValidateUS_ECCN();
				AssertNoMessageError(line.US_ECCNInfo, ECCNValidation.ECCN600SeriesDotYNotEligible);
			}
		}

		public void TestCheckUS_ECCNForStandaloneInvoice()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33 });

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;

			var invoiceline = invoice.InvoiceLines.AddNew();
			invoiceline.US_LicenseType = USAESLicenseCode.Codes.C30;
			string messageError = LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "", Factory, ZDateTime.Today);

			invoiceline.US_ECCN = "";
			AssertHasMessageError(invoiceline.US_ECCNInfo, messageError);
			invoiceline.US_ECCN = "C2343";
			AssertNoMessageError(invoiceline.US_ECCNInfo, messageError);

			invoiceline.US_LicenseType = USAESLicenseCode.Codes.C32;
			invoiceline.US_ECCN = "1C351";
			AssertHasMessageError(invoiceline.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C351", USAESLicenseCode.Codes.C32));

			invoiceline.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceline.US_ECCN = "1C354";
			AssertHasMessageError(invoiceline.US_ECCNInfo, string.Format(ECCNValidation.ECCNNotAllowed, "1C354", USAESLicenseCode.Codes.C33));

			invoiceline.US_ECCN = "1C356";
			AssertNoMessageErrors(invoiceline.US_ECCNInfo);
		}

		public void TestUS_DDTCITARExemptionNoLookupsItemRemoved()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoice.US_LicenseNo = "NLR";
			var allcodes = invoice.AddInfoLookups.US_DDTCITARExemptionCodes.GetAllCodes();
			AssertEquals("Invalid DDTC ITAR Exemption Codes", false, allcodes.Contains("126.4C"));

			invoice.US_DDTCITARExemptionNo = "126.4C";
			AssertHasMessageErrorContaining(invoice.US_DDTCITARExemptionNoInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckUS_DDTCITARExemptionNo()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS }, false);

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber, "123.11B", "22 CFR 123.11 (b)", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ITARExemptionNumber, "123.12", "22 CFR 123.12", startDate, endDate);
			Factory.Save();

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			invoice.US_LicenseType = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			invoiceLine.US_DDTCITARExemptionNo = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_DDTCITARExemptionNoInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				invoiceLine.US_LicenseType = licenseType;
				invoiceLine.US_DDTCITARExemptionNo = ZString.Empty;
				AssertHasMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
				invoiceLine.US_DDTCITARExemptionNo = "123.11B";
				AssertNoMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					invoiceLine.US_LicenseType = code.Code;
					invoiceLine.US_DDTCITARExemptionNo = ZString.Empty;
					AssertNoMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
					AssertNoMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
					invoiceLine.US_DDTCITARExemptionNo = "123.11B";
					AssertNoMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
					AssertHasMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberShouldNotBeEntered);
				}
			}

			invoice.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoiceLine.US_DDTCITARExemptionNo = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);

			invoiceLine.US_DDTCITARExemptionNo = "ZDFSDF";
			AssertNoMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
			AssertHasMessageErrorContaining(invoiceLine.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_DDTCITARExemptionNo = "123.12";
			AssertNoMessageError(invoiceLine.US_DDTCITARExemptionNoInfo, ClassificationValidator.DDTCITARExemptionNumberIsRequiredMessage);
			AssertNoMessageErrorContaining(invoiceLine.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);

			foreach (var code in new[] { "123.17L", "123.17M" }) // invalid codes not in list should error...
			{
				invoiceLine.US_DDTCITARExemptionNo = code;
				AssertHasMessageErrorContaining(invoiceLine.US_DDTCITARExemptionNoInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_DDTCRegistrationNo()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			var requireDDTCDataList = new ZString[]
			{
				USAESLicenseCode.Codes.SAG,
				USAESLicenseCode.Codes.SAU,
				USAESLicenseCode.Codes.SCA,
				USAESLicenseCode.Codes.SGB,
				USAESLicenseCode.Codes.S05,
				USAESLicenseCode.Codes.S61,
				USAESLicenseCode.Codes.S73,
				USAESLicenseCode.Codes.S85,
				USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS
			};

			var allowedDDTCDataList = new ZString[]
			{
				USAESLicenseCode.Codes.S00
			};

			invoice.US_LicenseType = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			invoiceLine.US_DDTCRegistrationNo = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_DDTCRegistrationNoInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				invoiceLine.US_LicenseType = licenseType;
				invoiceLine.US_DDTCRegistrationNo = ZString.Empty;
				AssertHasMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
				invoiceLine.US_DDTCRegistrationNo = "REG123";
				AssertNoMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code) && !allowedDDTCDataList.Contains(code.Code))
				{
					invoiceLine.US_LicenseType = code.Code;
					invoiceLine.US_DDTCRegistrationNo = ZString.Empty;
					AssertNoMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
					AssertNoMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
					invoiceLine.US_DDTCRegistrationNo = "REG123";
					AssertNoMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
					AssertHasMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberShouldNotBeEntered);
				}
			}

			invoice.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoiceLine.US_DDTCRegistrationNo = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);

			invoiceLine.US_DDTCRegistrationNo = "REG123";
			AssertNoMessageError(invoiceLine.US_DDTCRegistrationNoInfo, ClassificationValidator.DDTCRegistrationNumberIsRequiredMessage);
		}

		public void TestCheckUS_DDTCMilitaryEquipmentIndicator()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};

			invoice.US_LicenseType = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			invoiceLine.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				invoiceLine.US_LicenseType = licenseType;
				invoiceLine.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
				AssertHasMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
				invoiceLine.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
				AssertNoMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					invoiceLine.US_LicenseType = code.Code;
					invoiceLine.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
					AssertNoMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
					AssertNoMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
					invoiceLine.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
					AssertNoMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
					AssertHasMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorShouldNotBeEntered);
				}
			}

			invoice.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoiceLine.US_DDTCMilitaryEquipmentIndicator = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);

			invoiceLine.US_DDTCMilitaryEquipmentIndicator = "Z";
			AssertNoMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
			AssertHasMessageErrorContaining(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.No;
			AssertNoMessageError(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ClassificationValidator.DDTCMilitaryEquipmentIndicatorIsRequiredMessage);
			AssertNoMessageErrorContaining(invoiceLine.US_DDTCMilitaryEquipmentIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DDTCPartyCertificationIndicator()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			invoice.US_LicenseType = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			invoiceLine.US_DDTCPartyCertificationIndicator = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_DDTCPartyCertificationIndicatorInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				invoiceLine.US_LicenseType = licenseType;
				invoiceLine.US_DDTCPartyCertificationIndicator = ZString.Empty;
				AssertHasMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
				invoiceLine.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
				AssertNoMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					invoiceLine.US_LicenseType = code.Code;
					invoiceLine.US_DDTCPartyCertificationIndicator = ZString.Empty;
					AssertNoMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
					AssertNoMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
					invoiceLine.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
					AssertNoMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
					AssertHasMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorShouldNotBeEntered);
				}
			}

			invoice.US_LicenseType = USAESLicenseCode.Codes.SCA;
			invoiceLine.US_DDTCPartyCertificationIndicator = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);

			invoiceLine.US_DDTCPartyCertificationIndicator = "Z";
			AssertNoMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
			AssertHasMessageErrorContaining(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.No;
			AssertNoMessageError(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ClassificationValidator.DDTCPartyCertificationIndicatorIsRequiredMessage);
			AssertNoMessageErrorContaining(invoiceLine.US_DDTCPartyCertificationIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DDTCUnit()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};

			invoice.US_LicenseType = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			invoiceLine.US_DDTCUnit = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_DDTCUnitInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				invoiceLine.US_LicenseType = licenseType;
				invoiceLine.US_DDTCUnit = ZString.Empty;
				AssertHasMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitShouldNotBeEntered);
				invoiceLine.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.Copies;
				AssertNoMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					invoiceLine.US_LicenseType = code.Code;
					invoiceLine.US_DDTCUnit = ZString.Empty;
					AssertNoMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitIsRequiredMessage);
					AssertNoMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitShouldNotBeEntered);
					invoiceLine.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.Copies;
					AssertNoMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitIsRequiredMessage);
					AssertHasMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitShouldNotBeEntered);
				}
			}

			invoice.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoiceLine.US_DDTCUnit = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitIsRequiredMessage);

			invoiceLine.US_DDTCUnit = "ZD";
			AssertNoMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitIsRequiredMessage);
			AssertHasMessageErrorContaining(invoiceLine.US_DDTCUnitInfo, ListValidation.InvalidCodeMessageError);

			foreach (ICodeDescription code in new DDTCUnitOfMeasureList())
			{
				invoiceLine.US_DDTCUnit = code.Code;
				AssertNoMessageError(invoiceLine.US_DDTCUnitInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCUnitIsRequiredMessage);
				AssertNoMessageErrorContaining(invoiceLine.US_DDTCUnitInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_DDTCQuantity()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};

			invoice.US_LicenseType = ZString.Empty;
			invoiceLine.US_LicenseType = ZString.Empty;
			invoiceLine.US_DDTCQuantity = ZDecimal.Zero;
			AssertNoMessageErrors(invoiceLine.US_DDTCQuantityInfo);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString licenseType in requireDDTCDataList)
			{
				invoiceLine.US_LicenseType = licenseType;
				invoiceLine.US_DDTCQuantity = ZDecimal.Zero;
				AssertHasMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityShouldNotBeEntered);
				invoiceLine.US_DDTCQuantity = 10m;
				AssertNoMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityIsRequiredMessage);
				AssertNoMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityShouldNotBeEntered);
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					invoiceLine.US_LicenseType = code.Code;
					invoiceLine.US_DDTCQuantity = ZDecimal.Zero;
					AssertNoMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityIsRequiredMessage);
					AssertNoMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityShouldNotBeEntered);
					invoiceLine.US_DDTCQuantity = 10m;
					AssertNoMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityIsRequiredMessage);
					AssertHasMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityShouldNotBeEntered);
				}
			}

			invoice.US_LicenseType = USAESLicenseCode.Codes.SAG;
			invoiceLine.US_DDTCQuantity = ZDecimal.Zero;
			invoiceLine.US_LicenseType = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityIsRequiredMessage);

			invoiceLine.US_DDTCQuantity = 10m;
			AssertNoMessageError(invoiceLine.US_DDTCQuantityInfo, ExportAddInfoJobComInvoiceLineValidation.DDTCQuantityIsRequiredMessage);
		}

		[TestDate(2016, 09, 20)]
		public void TestLicenseTypeExemptionValidation()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(declaration.Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32 });

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_LicenseType = "";
			declaration.US_LicenseNo = "";
			declaration.US_ECCN = "";
			declaration.US_ExportCode = "";

			invoice.US_LicenseType = "";
			invoice.US_LicenseNo = "";
			invoice.US_ECCN = "";
			invoice.US_ExportCode = "";

			invoiceLine.US_LicenseType = "";
			invoiceLine.US_LicenseNo = "";
			invoiceLine.US_ECCN = "";
			invoiceLine.US_ExportCode = "";

			AssertHasMessageErrors(invoiceLine.US_LicenseTypeInfo);
			AssertNoMessageErrors(invoiceLine.US_LicenseNoInfo);
			AssertNoMessageErrors(invoiceLine.US_ECCNInfo);
			AssertNoMessageErrors(invoiceLine.US_ExportCodeInfo);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			AssertNoMessageErrors(invoiceLine.US_LicenseTypeInfo);
			AssertHasMessageErrors(invoiceLine.US_LicenseNoInfo);
			AssertHasMessageErrors(invoiceLine.US_ECCNInfo);
			AssertHasMessageErrors(invoiceLine.US_ExportCodeInfo);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C32;
			AssertHasMessageErrors(invoiceLine.US_LicenseTypeInfo);
			AssertNoMessageErrors(invoiceLine.US_LicenseNoInfo);
			AssertHasMessageErrors(invoiceLine.US_ECCNInfo);
			AssertHasMessageErrors(invoiceLine.US_ExportCodeInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C32;
			AssertNoMessageErrors(invoiceLine.US_LicenseTypeInfo);
			AssertNoMessageErrors(invoiceLine.US_LicenseNoInfo);
			AssertHasMessageErrors(invoiceLine.US_ECCNInfo);
			AssertHasMessageErrors(invoiceLine.US_ExportCodeInfo);
		}

		[TestDate(2016, 09, 20)]
		public void TestCheckUS_LicenseType()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C46,
				USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.E01,
				USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00,
				USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61, USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94,
				USAESLicenseCode.Codes.VDS });

			declaration.US_LicenseType = "";
			invoice.US_LicenseType = "";
			invoiceLine.US_LicenseType = "";
			AssertEquals("PreCondition: InvoiceLine.US_LicenseType", "", invoiceLine.US_LicenseType);
			AssertHasMessageError(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceLineValidation.LicenseTypeRequired);

			invoice.US_LicenseType = "BA";
			invoiceLine.US_LicenseType = "";
			AssertEquals("PreCondition: InvoiceLine.US_LicenseType", "BA", invoiceLine.US_LicenseType);
			AssertNoMessageError(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceLineValidation.LicenseTypeRequired);

			invoice.US_LicenseType = "";
			invoiceLine.US_LicenseType = "BA";
			AssertEquals("PreCondition: InvoiceLine.US_LicenseType", "BA", invoiceLine.US_LicenseType);
			AssertNoMessageError(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceLineValidation.LicenseTypeRequired);
			AssertHasMessageErrorContaining(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ICodeDescription pair in list)
			{
				invoiceLine.US_LicenseType = pair.Code;
				AssertNoMessageErrorContaining(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);
			}

			invoiceLine.US_LicenseType = " ";
			AssertNoMessageErrorContaining(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobDeclarationValidation.CodeIsInvalid);
		}

		public void TestCheckUS_LicenseTypeForCuba()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(declaration.Factory, new ZString[] { USAESLicenseCode.Codes.C46 });

			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Cuba;
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C46;

			AssertHasMessageErrorContaining(invoiceLine.US_LicenseTypeInfo, LicenseValidationHelper.LicenseTypeForCuba);
		}

		public void TestUS_LicenseType()
		{
			AssertEquals("Declaration.US_LicenseType", USAESLicenseCode.Codes.C33, declaration.US_LicenseType);
			AssertEquals("Invoice.US_LicenseType", USAESLicenseCode.Codes.C33, invoice.US_LicenseType);
			AssertEquals("InvoiceLine.US_LicenseType", USAESLicenseCode.Codes.C33, invoiceLine.US_LicenseType);
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C33;
			AssertNoMessageError(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceLineValidation.LicenseTypeRequired);

			declaration.US_LicenseType = "";
			invoiceLine.US_LicenseType = "";
			AssertHasMessageError(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceLineValidation.LicenseTypeRequired);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C35;
			AssertNoMessageError(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceLineValidation.LicenseTypeRequired);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C38;
			invoiceLine.US_LicenseType = "";
			AssertNoMessageError(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceLineValidation.LicenseTypeRequired);

			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C40;
			AssertNoMessageError(invoiceLine.US_LicenseTypeInfo, ExportAddInfoJobComInvoiceLineValidation.LicenseTypeRequired);
		}

		public void TestUS_IsUsedVehicle()
		{
			invoiceLine.US_IsUsedVehicle = true;
			AssertHasMessageError(invoiceLine.US_VehicleIDTypeInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle ID Type for Used Vehicle.");
			invoiceLine.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			AssertHasMessageError(invoiceLine.US_VehicleIDInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle ID for Used Vehicle.");
			AssertHasMessageError(invoiceLine.US_VehicleTitleStateInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title State for Used Vehicle.");
			AssertHasMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");

			invoiceLine.US_IsUsedVehicle = false;
			AssertNoMessageError(invoiceLine.US_VehicleIDTypeInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle ID Type for Used Vehicle.");
			AssertNoMessageError(invoiceLine.US_VehicleIDInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle ID for Used Vehicle.");
			AssertNoMessageError(invoiceLine.US_VehicleTitleStateInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title State for Used Vehicle.");
			AssertNoMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");
		}

		public void TestCheckUS_VehicleID()
		{
			invoiceLine.US_IsUsedVehicle = true;
			invoiceLine.US_VehicleID = "32";
			string errorMessage = MandatoryValidation.YouHaveNotEntered + " a Vehicle ID for Used Vehicle.";
			AssertNoMessageError(invoiceLine.US_VehicleIDInfo, errorMessage);

			invoiceLine.US_VehicleID = "";
			AssertHasMessageError(invoiceLine.US_VehicleIDInfo, errorMessage);

			invoiceLine.US_IsUsedVehicle = false;
			invoiceLine.US_VehicleID = "32";
			AssertNoMessageError(invoiceLine.US_VehicleIDInfo, errorMessage);

			invoiceLine.US_VehicleID = "";
			AssertNoMessageError(invoiceLine.US_VehicleIDInfo, errorMessage);
		}

		public void TestCheckUS_VehicleIDType()
		{
			invoiceLine.US_IsUsedVehicle = true;
			invoiceLine.US_VehicleIDType = "3";
			AssertNoMessageError(invoiceLine.US_VehicleIDTypeInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle ID Type for Used Vehicle.");
			AssertHasMessageErrorContaining(invoiceLine.US_VehicleIDTypeInfo, "The code you have selected is not in the list.");

			invoiceLine.US_VehicleIDType = "";
			AssertHasMessageError(invoiceLine.US_VehicleIDTypeInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle ID Type for Used Vehicle.");
			AssertNoMessageErrorContaining(invoiceLine.US_VehicleIDTypeInfo, "The code you have selected is not in the list.");

			VehicleIDTypeList list = new VehicleIDTypeList();

			foreach (CodeDescriptionPair pair in list)
			{
				invoiceLine.US_VehicleIDType = pair.Code;
				AssertNoMessageErrorContaining(invoiceLine.US_VehicleIDTypeInfo, "The code you have selected is not in the list.");
			}

			invoiceLine.US_IsUsedVehicle = false;
			invoiceLine.US_VehicleIDType = "";
			AssertNoMessageError(invoiceLine.US_VehicleIDTypeInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle ID Type for Used Vehicle.");
			AssertNoMessageErrorContaining(invoiceLine.US_VehicleIDTypeInfo, "The code you have selected is not in the list.");

			invoiceLine.US_VehicleIDType = "3";
			AssertNoMessageError(invoiceLine.US_VehicleIDTypeInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle ID Type for Used Vehicle.");
			AssertNoMessageErrorContaining(invoiceLine.US_VehicleIDTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckUS_VehicleTitleState()
		{
			invoiceLine.US_IsUsedVehicle = true;
			invoiceLine.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			invoiceLine.US_VehicleTitleState = "32";
			AssertNoMessageError(invoiceLine.US_VehicleTitleStateInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title State for Used Vehicle.");
			AssertHasMessageError(invoiceLine.US_VehicleTitleStateInfo, "The code you have selected is not in the list.");

			invoiceLine.US_VehicleTitleState = "";
			AssertHasMessageError(invoiceLine.US_VehicleTitleStateInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title State for Used Vehicle.");
			AssertNoMessageErrorContaining(invoiceLine.US_VehicleTitleStateInfo, "The code you have selected is not in the list.");

			CodeDescriptionPairList list = invoiceLine.AddInfoLookups.USStateList;

			foreach (RefCountryStates state in list)
			{
				invoiceLine.US_VehicleTitleState = state.RW_Code;
				AssertNoMessageErrorContaining(invoiceLine.US_VehicleTitleStateInfo, "The code you have selected is not in the list.");
			}

			invoiceLine.US_IsUsedVehicle = false;

			invoiceLine.US_VehicleTitleState = "";
			AssertNoMessageError(invoiceLine.US_VehicleTitleStateInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title State for Used Vehicle.");
			AssertNoMessageErrorContaining(invoiceLine.US_VehicleTitleStateInfo, "The code you have selected is not in the list.");

			invoiceLine.US_VehicleTitleState = "32";
			AssertNoMessageError(invoiceLine.US_VehicleTitleStateInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title State for Used Vehicle.");
			AssertNoMessageErrorContaining(invoiceLine.US_VehicleTitleStateInfo, "The code you have selected is not in the list.");

			invoiceLine.US_IsUsedVehicle = true;
			invoiceLine.US_VehicleIDType = VehicleIDTypeList.Codes.ProductID;
			invoiceLine.US_VehicleTitleState = "";
			AssertNoMessageError(invoiceLine.US_VehicleTitleStateInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title State for Used Vehicle.");
			AssertNoMessageErrorContaining(invoiceLine.US_VehicleTitleStateInfo, "The code you have selected is not in the list.");

			invoiceLine.US_VehicleTitleState = "US";
			AssertNoMessageError(invoiceLine.US_VehicleTitleStateInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title State for Used Vehicle.");
			AssertNoMessageErrorContaining(invoiceLine.US_VehicleTitleStateInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckUS_VehicleTitleNo()
		{
			invoiceLine.US_IsUsedVehicle = true;
			invoiceLine.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			invoiceLine.US_VehicleTitleNo = "32";
			AssertNoMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");

			invoiceLine.US_VehicleTitleNo = "";
			AssertHasMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");

			invoiceLine.US_IsUsedVehicle = false;
			invoiceLine.US_VehicleTitleNo = "32";
			AssertNoMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");

			invoiceLine.US_VehicleTitleNo = "";
			AssertNoMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");

			invoiceLine.US_IsUsedVehicle = true;
			invoiceLine.US_VehicleIDType = VehicleIDTypeList.Codes.ProductID;
			invoiceLine.US_VehicleTitleNo = "";
			AssertNoMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");

			invoiceLine.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			AssertHasMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");
			invoiceLine.US_VehicleTitleState = "US";
			AssertNoMessageError(invoiceLine.US_VehicleTitleNoInfo, MandatoryValidation.YouHaveNotEntered + " a Vehicle Title No for Used Vehicle.");
		}

		public void TestCheckUS_AESOriginIndicator()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.AllowExportDefaultOriginIndicatorAtInvoice.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			invoice.US_AESOriginIndicator = "";
			invoiceLine.US_AESOriginIndicator = "";
			AssertEquals("PreCondition: InvoiceLine.US_AESOriginIndicator_Effective", "", invoiceLine.US_AESOriginIndicator);
			AssertNoMessageErrorContaining(invoiceLine.US_AESOriginIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoiceLine.US_AESOriginIndicatorInfo, ExportAddInfoJobComInvoiceLineValidation.AESOriginIndicatorRequired);

			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;
			invoiceLine.US_AESOriginIndicator = "B";
			AssertNotEquals("InvoiceLine.US_AESOriginIndicator_Effective", "", invoiceLine.US_AESOriginIndicator);
			AssertNoMessageErrorContaining(invoiceLine.US_AESOriginIndicatorInfo, ExportAddInfoJobComInvoiceLineValidation.AESOriginIndicatorRequired);
			AssertHasMessageErrorContaining(invoiceLine.US_AESOriginIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoice.US_AESOriginIndicator = "B";
			invoiceLine.US_AESOriginIndicator = "";
			AssertNotEquals("InvoiceLine.US_AESOriginIndicator_Effective", "", invoiceLine.US_AESOriginIndicator);
			AssertNoMessageErrorContaining(invoiceLine.US_AESOriginIndicatorInfo, ExportAddInfoJobComInvoiceLineValidation.AESOriginIndicatorRequired);

			invoice.US_AESOriginIndicator = "";
			AESOriginIndicatorList list = new AESOriginIndicatorList();
			foreach (CodeDescriptionPair pair in list)
			{
				invoiceLine.US_AESOriginIndicator = pair.Code;
				AssertNoMessageErrorContaining(invoiceLine.US_AESOriginIndicatorInfo, ListValidation.InvalidCodeMessageError);
			}

			declaration.US_ExportCode = ExportInformationCodeList.Codes.HH;
			invoice.US_AESOriginIndicator = "";
			invoiceLine.US_AESOriginIndicator = "F";
			AssertHasMessageError(invoiceLine.US_AESOriginIndicatorInfo, ExportAddInfoJobComInvoiceLineValidation.AESOriginIndicatorMustBeBlank);

			invoiceLine.US_AESOriginIndicator = "";
			AssertNoMessageError(invoiceLine.US_AESOriginIndicatorInfo, ExportAddInfoJobComInvoiceLineValidation.AESOriginIndicatorMustBeBlank);
			AssertNoMessageError(invoiceLine.US_AESOriginIndicatorInfo, ExportAddInfoJobComInvoiceLineValidation.AESOriginIndicatorRequired);

			DataRegistry.Business.USCustomsDataRegistry.Instance.AllowExportDefaultOriginIndicatorAtInvoice.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			declaration.US_ExportCode = ExportInformationCodeList.Codes.CH;
			invoiceLine.AddInfoValidation.ValidateUS_AESOriginIndicator();
			AssertHasMessageErrorContaining(invoiceLine.US_AESOriginIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(invoiceLine.US_AESOriginIndicatorInfo, ExportAddInfoJobComInvoiceLineValidation.AESOriginIndicatorRequired);

			invoiceLine.US_AESOriginIndicator = "F";
			AssertNoMessageErrorContaining(invoiceLine.US_AESOriginIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(invoiceLine.US_AESOriginIndicatorInfo, ExportAddInfoJobComInvoiceLineValidation.AESOriginIndicatorRequired);
		}

		public void TestCheckUS_AESOriginIndicatorCanBeBlankForHouseHoldGoods()
		{
			invoice.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			AssertEquals("Should return effective value", AESOriginIndicatorList.Codes.Domestic, invoiceLine.US_AESOriginIndicator);
			invoiceLine.US_AESOriginIndicator = "";
			AssertEquals("Will continue to return effective value if cleared out", AESOriginIndicatorList.Codes.Domestic, invoiceLine.US_AESOriginIndicator);
			invoiceLine.US_ExportCode = ExportInformationCodeList.Codes.HH;
			invoiceLine.US_AESOriginIndicator = "";
			AssertEquals("For household goods, OriginIndicator should allow correct entry of space (ie. no indicator for this code)", "", invoiceLine.US_AESOriginIndicator);
		}

		[TestDate(2015, 12, 9)]
		public void TestCheckUS_HazWasteTrackingNo()
		{
			invoiceLine.US_HazWasteTrackingNo = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_HazWasteTrackingNoInfo, ExportAddInfoJobComInvoiceLineValidation.InvalidHazWasteTrackingNoFormat);

			invoiceLine.US_HazWasteTrackingNo = "123456589";
			AssertHasMessageError(invoiceLine.US_HazWasteTrackingNoInfo, ExportAddInfoJobComInvoiceLineValidation.InvalidHazWasteTrackingNoFormat);

			invoiceLine.US_HazWasteTrackingNo = "123456789AA";
			AssertHasMessageError(invoiceLine.US_HazWasteTrackingNoInfo, ExportAddInfoJobComInvoiceLineValidation.InvalidHazWasteTrackingNoFormat);

			invoiceLine.US_ExportCertificateNo = "12345";

			invoiceLine.US_HazWasteTrackingNo = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_HazWasteTrackingNoInfo);

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.AddInfoValidation.ValidateUS_HazWasteTrackingNo();
			AssertHasMessageErrorContaining(invoiceLine.US_HazWasteTrackingNoInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_HazWasteTrackingNo = "123456789ABC";
			AssertNoMessageErrorContaining(invoiceLine.US_HazWasteTrackingNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2015, 12, 9)]
		public void TestCheckUS_EPAConsentNumber()
		{
			invoiceLine.US_EPAConsentNumber = "12345";
			AssertNoMessageErrors(invoiceLine.US_EPAConsentNumberInfo);

			invoiceLine.US_ExportCertificateNo = "12345";

			invoiceLine.US_EPAConsentNumber = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_EPAConsentNumberInfo);

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.AddInfoValidation.ValidateUS_EPAConsentNumber();
			AssertHasMessageErrorContaining(invoiceLine.US_EPAConsentNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_EPAConsentNumber = "12345";
			AssertNoMessageErrorContaining(invoiceLine.US_EPAConsentNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2015, 12, 9)]
		public void TestCheckUS_EPANetQty()
		{
			invoiceLine.US_EPANetQty = 123m;
			AssertNoMessageErrors(invoiceLine.US_EPANetQtyInfo);

			invoiceLine.US_ExportCertificateNo = "12345";

			invoiceLine.US_EPANetQty = ZDecimal.Zero;
			AssertNoMessageErrors(invoiceLine.US_EPANetQtyInfo);

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.AddInfoValidation.ValidateUS_EPANetQty();
			AssertHasMessageErrorContaining(invoiceLine.US_EPANetQtyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_EPANetQty = 123m;
			AssertNoMessageErrorContaining(invoiceLine.US_EPANetQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2015, 12, 9)]
		public void TestCheckUS_EPANetQtyUQ()
		{
			invoiceLine.US_EPANetQtyUQ = "KG";
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrors(invoiceLine.US_EPANetQtyUQInfo);

			invoiceLine.US_ExportCertificateNo = "12345";

			invoiceLine.US_EPANetQtyUQ = "AB";
			AssertHasMessageErrorContaining(invoiceLine.US_EPANetQtyUQInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_EPANetQtyUQ = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_EPANetQtyUQ();
			AssertHasMessageErrorContaining(invoiceLine.US_EPANetQtyUQInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_EPANetQtyUQ = "KG";
			AssertNoMessageErrorContaining(invoiceLine.US_EPANetQtyUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2015, 12, 9)]
		public void TestCheckUS_ExportCertificateNo()
		{
			invoiceLine.US_ExportCertificateNo = "12345";
			AssertNoMessageErrors(invoiceLine.US_ExportCertificateNoInfo);
			invoiceLine.US_EPAConsentNumber = "12345";
			invoiceLine.US_EPAConsentNumber = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_ExportCertificateNoInfo);

			invoiceLine.US_HazWasteTrackingNo = "123456";
			invoiceLine.US_HazWasteTrackingNo = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_ExportCertificateNoInfo);

			invoiceLine.US_EPANetQty = 123m;
			invoiceLine.US_EPANetQty = ZDecimal.Zero;
			AssertNoMessageErrors(invoiceLine.US_ExportCertificateNoInfo);

			invoiceLine.US_EPANetQtyUQ = "KG";
			invoiceLine.US_EPANetQtyUQ = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.US_ExportCertificateNoInfo);

			invoiceLine.US_ExportCertificateNo = ZString.Empty;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.AddInfoValidation.ValidateUS_ExportCertificateNo();
			AssertHasMessageErrorContaining(invoiceLine.US_ExportCertificateNoInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_ExportCertificateNo = "123456";
			AssertNoMessageErrorContaining(invoiceLine.US_ExportCertificateNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2024, 01, 01)]
		public void TestCheckUS_TTBDEAEPAIndDisclaim()
		{
			CreateSHBTariffWithPGACondition("1100000003", GovernmentAgencyProgramCodeList.Codes.TTB, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			invoiceLine.US_TTBInd = "";
			AssertEquals("No Tariff yet", "", invoiceLine.US_TTBInd);
			invoiceLine.JI_Tariff = "1100000003";
			AssertEquals("Default D as mandatory", "D", invoiceLine.US_TTBInd);
			AssertNoMessageErrorContaining(invoiceLine.US_TTBIndInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(invoiceLine.US_TTBIndInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);

			CreateSHBTariffWithPGACondition("1100000002", GovernmentAgencyProgramCodeList.Codes.DEA, UniversalReferenceConstants.TariffConditionValue.Values.Optional);

			invoiceLine.US_DEAInd = "";
			AssertEquals("No Tariff yet", "", invoiceLine.US_DEAInd);
			invoiceLine.JI_Tariff = "1100000002";
			AssertEquals("Option O ", "", invoiceLine.US_DEAInd);
			AssertNoMessageErrorContaining(invoiceLine.US_DEAIndInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageErrorContaining(invoiceLine.US_DEAIndInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);

			CreateSHBTariffWithPGACondition("1100000001", GovernmentAgencyProgramCodeList.Codes.EPA, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			invoiceLine.US_PSTIndicator = "";
			AssertEquals("No Tariff yet", "", invoiceLine.US_PSTIndicator);
			invoiceLine.JI_Tariff = "1100000001";
			AssertEquals("Default D as mandatory", "D", invoiceLine.US_PSTIndicator);
			AssertNoMessageErrorContaining(invoiceLine.US_PSTIndicatorInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(invoiceLine.US_PSTIndicatorInfo, RequirementConstants.PGA.PGADisclaimedIsNotAllowedForExportMadatoryValue);
		}

		[TestDate(2016, 09, 22)]
		public void TestCheckUS_NMFSHMSInd()
		{
			CreateSHBTariffWithPGACondition("0000000001", GovernmentAgencyProgramCodeList.NMFS, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			invoiceLine.US_NMFSHMSInd = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSHMSIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSHMSIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrorContaining(invoiceLine.US_NMFSHMSIndInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_Tariff = "0000000001";
			invoiceLine.US_NMFSHMSInd = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSHMSIndInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "NMFS"));
		}

		[TestDate(2016, 09, 22)]
		public void TestCheckUS_ATFInd()
		{
			CreateSHBTariffWithPGACondition("0000000001", GovernmentAgencyProgramCodeList.Codes.ATF, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			invoiceLine.US_ATFInd = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_ATFIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(invoiceLine.US_ATFIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrorContaining(invoiceLine.US_ATFIndInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_Tariff = "0000000001";
			invoiceLine.US_ATFInd = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_ATFIndInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "ATF"));
		}

		[TestDate(2016, 09, 22)]
		public void TestCheckUS_DEAInd()
		{
			CreateSHBTariffWithPGACondition("0000000001", GovernmentAgencyProgramCodeList.Codes.DEA, UniversalReferenceConstants.TariffConditionValue.Values.Optional);

			invoiceLine.US_DEAInd = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_DEAIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageErrorContaining(invoiceLine.US_DEAIndInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(invoiceLine.US_DEAIndInfo, string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequiredWhenndicatorIsNotBlank, "DEA"));
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrorContaining(invoiceLine.US_DEAIndInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoiceLine.US_DEAIndInfo, string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequiredWhenndicatorIsNotBlank, "DEA"));
			invoiceLine.JI_Tariff = "0000000001";
			invoiceLine.US_DEAInd = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_DEAIndInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "DEA"));

			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrorContaining(invoiceLine.US_DEAIndInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "DEA"));
		}

		[TestDate(2016, 09, 22)]
		public void TestCheckUS_PSTInd()
		{
			CreateSHBTariffWithPGACondition("0000000001", GovernmentAgencyProgramCodeList.Codes.EPA, UniversalReferenceConstants.TariffConditionValue.Values.Optional);
			CreateSHBTariffWithPGACondition("0000000002", GovernmentAgencyProgramCodeList.Codes.EPA, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			invoiceLine.US_PSTIndicator = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageErrorContaining(invoiceLine.US_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrorContaining(invoiceLine.US_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_Tariff = "0000000001";
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_PSTIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "EPA"));

			invoiceLine.JI_Tariff = "0000000002";
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_PSTIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "EPA"));

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrorContaining(invoiceLine.US_PSTIndicatorInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "EPA"));
		}

		[TestDate(2016, 09, 22)]
		public void TestCheckUS_FWSInd()
		{
			CreateSHBTariffWithPGACondition("0000000001", GovernmentAgencyProgramCodeList.Codes.FWS, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			invoiceLine.US_FWSInd = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_FWSIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageErrorContaining(invoiceLine.US_FWSIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrorContaining(invoiceLine.US_FWSIndInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_Tariff = "0000000001";
			invoiceLine.US_FWSInd = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_FWSIndInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "FWS"));
		}

		[TestDate(2016, 09, 22)]
		public void TestCheckUS_TTBInd()
		{
			CreateSHBTariffWithPGACondition("0000000001", GovernmentAgencyProgramCodeList.Codes.TTB, UniversalReferenceConstants.TariffConditionValue.Values.Mandatory);

			var messageErrorWhenNoTTBLines = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequiredWhenndicatorIsNotBlank, "TTB");
			invoiceLine.US_TTBInd = "~";
			AssertEquals("No TTBLines", false, invoiceLine.HasTTBLines);
			AssertHasMessageErrorContaining(invoiceLine.US_TTBIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageErrorContaining(invoiceLine.US_TTBIndInfo, messageErrorWhenNoTTBLines);
			AssertNoMessageErrorContaining(invoiceLine.US_TTBIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_TTBIndInfo, messageErrorWhenNoTTBLines);
			AssertNoMessageErrorContaining(invoiceLine.US_TTBIndInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_Tariff = "0000000001";
			invoiceLine.US_TTBInd = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_TTBIndInfo, string.Format(RequirementConstants.PGA.PGARequiredButBlank, "TTB"));

			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProcessingCode = "ABC";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("TTB Lines added", true, invoiceLine.HasTTBLines);
			invoiceLine.AddInfoValidation.ValidateUS_TTBInd();
			AssertNoMessageErrorContaining(invoiceLine.US_TTBIndInfo, messageErrorWhenNoTTBLines);
		}

		void CreateSHBTariffWithPGACondition(ZString tariff, ZString conditionValueType, ZString conditionValue)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var shBTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.PGA);
			var shbTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, shBTariffType.PK, tariff, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, shbTariff.PK, "PGA Data requirements", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var refCusConditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, conditionValueType);
			helper.CreateOrGetExistingRefCusConditionValue(refCusConditionValueType.PK, condition.PK, conditionValue);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
