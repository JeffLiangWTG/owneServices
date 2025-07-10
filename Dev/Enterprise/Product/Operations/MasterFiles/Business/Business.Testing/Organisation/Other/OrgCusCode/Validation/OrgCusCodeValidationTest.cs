using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.OrganisationRegistry;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateWorldCargoAssociationNumber()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.WorldCargoAssociationNumber;

			orgCusCode.OK_CustomsRegNo = "1CODE";
			AssertHasError(orgCusCode.OK_CustomsRegNoInfo, "A valid World Cargo Association number is composed of 5 or 6 digits.");

			orgCusCode.OK_CustomsRegNo = "1705";
			AssertHasError(orgCusCode.OK_CustomsRegNoInfo, "A valid World Cargo Association number is composed of 5 or 6 digits.");

			orgCusCode.OK_CustomsRegNo = "1705467";
			AssertHasError(orgCusCode.OK_CustomsRegNoInfo, "A valid World Cargo Association number is composed of 5 or 6 digits.");

			orgCusCode.OK_CustomsRegNo = "";
			AssertHasError(orgCusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");

			orgCusCode.OK_CustomsRegNo = "12345";
			AssertNoError(orgCusCode.OK_CustomsRegNoInfo, "A valid World Cargo Association number is composed of 5 or 6 digits.");

			orgCusCode.OK_CustomsRegNo = "123456";
			AssertNoError(orgCusCode.OK_CustomsRegNoInfo, "A valid World Cargo Association number is composed of 5 or 6 digits.");
		}

		public void TestPTIVAOnlyOneRegistrationNumberPerOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var code1 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "123456789", CountryCodes.Portugal);
			AssertNoErrors(code1.OK_CustomsRegNoInfo);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "012345678", CountryCodes.Portugal);
			AssertHasError(code2.OK_CustomsRegNoInfo, "Organizations can only have one PT IVA Number. This organization already has a recorded PT IVA number.");
		}

		public void TestPTIVAFormatValidation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var code = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "123456789", CountryCodes.Portugal);

			var expectedInvalidPaternMessage = @"The IVA registration code pattern is invalid.

Valid patterns are:
	XXnnnnnnnnn
	nnnnnnnnn

where 'X' is a letter from A to Z and indicates the two character country identifier 'PT', and 'n' is a digit from 0 to 9.";

			using (OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue()))
			{
				code.OK_CustomsRegNo = "123456789";
				AssertNoErrors(code.OK_CustomsRegNoInfo);
				AssertNoWarnings(code.OK_CustomsRegNoInfo);
				code.OK_CustomsRegNo = "AU123456789";
				AssertHasError(code.OK_CustomsRegNoInfo, expectedInvalidPaternMessage);

				code.OK_CustomsRegNo = "PT123456789";
				AssertNoErrors(code.OK_CustomsRegNoInfo);
				AssertNoWarnings(code.OK_CustomsRegNoInfo);
			}

			using (OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue()))
			{
				code.OK_CustomsRegNo = "123456789";
				AssertNoErrors(code.OK_CustomsRegNoInfo);
				AssertNoWarnings(code.OK_CustomsRegNoInfo);
				code.OK_CustomsRegNo = "AU123456789";
				AssertNoErrors(code.OK_CustomsRegNoInfo);
				AssertHasWarning(code.OK_CustomsRegNoInfo, expectedInvalidPaternMessage);

				code.OK_CustomsRegNo = "PT123456789";
				AssertNoErrors(code.OK_CustomsRegNoInfo);
				AssertNoWarnings(code.OK_CustomsRegNoInfo);
			}
		}

		public void TestValidateCCP_GB_TheCCPCodeMustStartWithACharacterFromAToF()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AB3";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "AB2";
			var code1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1CODE1", CountryCodes.Italy);
			var code2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1CODE1", CountryCodes.UnitedKingdom);

			AssertHasMessageError(code1.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
			AssertNoMessageError(code2.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
		}

		public void TestAllowDuplicates()
		{
			AssertEquals("When it's empty", false, OrgCusCodeValidation.AllowDuplicates(ZString.Empty, ZString.Empty));
			AssertEquals("MID for 'empty'", true, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.USACodeTypes.ManufacturerID, ZString.Empty));
			AssertEquals("MID for 'AF'", true, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.USACodeTypes.ManufacturerID, CountryCodes.Afghanistan));
			AssertEquals("MID for 'GU'", true, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.USACodeTypes.ManufacturerID, CountryCodes.Guam));
			AssertEquals("MID for 'US'", false, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.USACodeTypes.ManufacturerID, CountryCodes.UnitedStates));
			AssertEquals("MID for 'PR'", true, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.USACodeTypes.ManufacturerID, CountryCodes.PuertoRico));
			AssertEquals("MID for 'MP'", true, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.USACodeTypes.ManufacturerID, CountryCodes.NorthernMarianaIslands));
			AssertEquals("NMF for 'US'", true, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.USACodeTypes.NMFCParticipant, CountryCodes.UnitedStates));
			AssertEquals("CCP for 'empty'", false, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.CodeTypes.ControlledPremisesID, ZString.Empty));
			AssertEquals("CCP for 'AF'", false, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.CodeTypes.ControlledPremisesID, CountryCodes.Afghanistan));
			AssertEquals("GS1 for 'empty'", false, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.CodeTypes.GS1, ZString.Empty));
			AssertEquals("GS1 for 'AF'", false, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.CodeTypes.GS1, CountryCodes.Afghanistan));
			AssertEquals("CCP for Japan", true, OrgCusCodeValidation.AllowDuplicates(OrgCusCode.CodeTypes.ControlledPremisesID, CountryCodes.Japan));
		}

		public void TestGetDuplicateQuery()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AB3";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "AB2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "AB1";
			var code1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", CountryCodes.Colombia);
			var code2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", CountryCodes.Colombia);
			var code3 = org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", CountryCodes.Afghanistan);
			var codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(CountryCodes.Colombia, OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", code2.PK));
			AssertContainsExactElementsInAnyOrder(new[] { code1 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(CountryCodes.Colombia, OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1"));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code2 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(ZString.Empty, OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1"));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code2, code3 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(ZString.Empty, OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", code2.PK));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code3 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(CountryCodes.Afghanistan, OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1"));
			AssertContainsExactElementsInAnyOrder(new[] { code3 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(CountryCodes.UnitedStates, OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1"));
			AssertEquals(0, codes.Length);

			code1.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			code2.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			code3.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(CountryCodes.Colombia, OrgCusCode.CodeTypes.GS1, "CODE1", code2.PK));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code3 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(CountryCodes.Colombia, OrgCusCode.CodeTypes.GS1, "CODE1"));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code2, code3 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(ZString.Empty, OrgCusCode.CodeTypes.GS1, "CODE1"));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code2, code3 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(ZString.Empty, OrgCusCode.CodeTypes.GS1, "CODE1", code2.PK));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code3 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(CountryCodes.Afghanistan, OrgCusCode.CodeTypes.GS1, "CODE1"));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code2, code3 }, codes);
			codes = Factory.Load<OrgCusCode>(OrgCusCodeValidation.GetDuplicateQuery(CountryCodes.UnitedStates, OrgCusCode.CodeTypes.GS1, "CODE1"));
			AssertContainsExactElementsInAnyOrder(new[] { code1, code2, code3 }, codes);
		}

		public void TestValidateDuplicateFromEndToEnd()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AB3~~~";
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "AB2~~~";

			AssertNoExceptionThrown(() =>
			{
				org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", CountryCodes.Colombia);
			});
			factory.Save();

			org1.OH_IsActive = false;
			factory.Save();

			AssertNoExceptionThrown(() =>
			{
				org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", CountryCodes.Colombia);
			});
		}

		public void TestNoValidateDuplicateMessgaeWhenCPW()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CB1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CB2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "CB3";
			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "CB4";

			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CA1", CountryCodes.Canada);
			var code2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CA1", CountryCodes.Canada);
			var code3 = org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "777", CountryCodes.Canada);
			var code4 = org4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "777", CountryCodes.Canada);
			Factory.Save();

			var message1 = "HELLO";
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, message1);
			AssertHasError(code2.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, message1);
			AssertNoError(code3.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code4, code4.OK_RN_NKCodeCountry, message1);
			AssertNoError(code4.OK_CustomsRegNoInfo, message1);
			if (ErrorReporter.LastKeyReported == "Validation:OK_CustomsRegNo")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestValidateConflictingRegistrationCodes()
		{
			var customsRegNo = "987654321";

			var otherOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			otherOrgHeader.OH_Code = "ORGNOTMINE";

			var myOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			myOrgHeader.OH_Code = "ORGMYOWN";

			AssertForCountry(CountryCodes.Australia);
			AssertForCountry(CountryCodes.Canada);
			AssertForCountry(CountryCodes.Spain);
			AssertForCountry(CountryCodes.Turkey);

			void AssertForCountry(string countryCode)
			{
				var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, countryCode));
				var codeType = OrgRegistrationNumberTypeList.GetApplicableNumberTypes(country).First();

				var otherCode = otherOrgHeader.CustomsCodes.AddNew(codeType, customsRegNo, countryCode);
				Factory.Save();

				var myCode = myOrgHeader.CustomsCodes.AddNew(codeType, customsRegNo, countryCode);

				AssertHasWarning(myCode.OK_CustomsRegNoInfo, "This Registration Number is already in use by at least one organization. The organizations are: ORGNOTMINE (Maximum of 10 organizations shown). Please check that the organizations are not the same.");
			}
		}

		public void TestValidateConflictingRegistrationCodes_WithManyDuplicates_ShouldListMax10Organisations()
		{
			var customsRegNo = "99999990000C";

			for (var i = 1; i <= 50; i++)
			{
				var otherOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
				using (otherOrgHeader.GetValidationSuspender())
				{
					otherOrgHeader.OH_Code = "ORGOTHER" + i;
					otherOrgHeader.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, customsRegNo, CountryCodes.Singapore);
				}
			}

			var myOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			myOrgHeader.OH_Code = "ORGMYOWN";

			Factory.Save();

			var myCode = myOrgHeader.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, customsRegNo, CountryCodes.Singapore);
			AssertHasWarningContaining(myCode.OK_CustomsRegNoInfo, "This Registration Number is already in use by at least one organization.");
		}

		public void TestValidateDuplicate()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AB3";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "AB2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "AB1";
			var code1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", CountryCodes.Colombia);
			var code2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", CountryCodes.Colombia);
			var code3 = org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CODE1", CountryCodes.Afghanistan);
			var message1 = "HELLO";
			var message2 = "BYE";
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, message1);
			AssertHasError(code2.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, message1);
			AssertNoError(code3.OK_CustomsRegNoInfo, message1);
			Func<OrgCusCode[], ZString> getMessage = (duplicateCodes) =>
			{
				return message2 + duplicateCodes.OrderBy(x => x.CompanyCodeAndPremisesAddresses).First().CompanyCodeAndPremisesAddresses;
			};
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, getMessage);
			AssertHasErrorContaining(code2.OK_CustomsRegNoInfo, message2);
			AssertHasError(code2.OK_CustomsRegNoInfo, message2 + code1.CompanyCodeAndPremisesAddresses);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, getMessage);
			AssertNoErrorContaining(code3.OK_CustomsRegNoInfo, message2);

			code1.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			code2.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			code3.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, message1);
			AssertHasError(code2.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, message1);
			AssertHasError(code3.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, getMessage);
			AssertHasError(code2.OK_CustomsRegNoInfo, message2 + code3.CompanyCodeAndPremisesAddresses);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, getMessage);
			AssertHasError(code3.OK_CustomsRegNoInfo, message2 + code2.CompanyCodeAndPremisesAddresses);

			code1.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			code1.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			code2.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			code2.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			code3.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, message1);
			AssertHasError(code2.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, message1);
			AssertNoError(code3.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, getMessage);
			AssertHasErrorContaining(code2.OK_CustomsRegNoInfo, message2);
			AssertHasError(code2.OK_CustomsRegNoInfo, message2 + code1.CompanyCodeAndPremisesAddresses);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, getMessage);
			AssertNoErrorContaining(code3.OK_CustomsRegNoInfo, message2);

			code1.OK_CodeType = OrgCusCode.USACodeTypes.NMFCParticipant;
			code2.OK_CodeType = OrgCusCode.USACodeTypes.NMFCParticipant;
			code3.OK_CodeType = OrgCusCode.USACodeTypes.NMFCParticipant;
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, message1);
			AssertNoError(code2.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, message1);
			AssertNoError(code3.OK_CustomsRegNoInfo, message1);
			OrgCusCodeValidation.ValidateDuplicate(code2, code2.OK_RN_NKCodeCountry, getMessage);
			AssertNoErrorContaining(code2.OK_CustomsRegNoInfo, message2);
			OrgCusCodeValidation.ValidateDuplicate(code3, code3.OK_RN_NKCodeCountry, getMessage);
			AssertNoErrorContaining(code3.OK_CustomsRegNoInfo, message2);
			if (ErrorReporter.LastKeyReported == "Validation:OK_CustomsRegNo")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestCheckOK_CodeType_WhenHeaderNotInDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var oldSecurityForNewPrimary = Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed;
			Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed = false;

			var oldSecurityForNewNonPrimary = Env.Security.OrgConfigModifyNonFinancialRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos.IsAllowed = false;

			try
			{
				var cusCode = org.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
				AssertEquals(true, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertHasError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigNewModifyFinancialRegistrationNos, null));

				Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed = true;
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CreditAgencyCode;
				AssertEquals(true, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertNoError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigNewModifyFinancialRegistrationNos, null));

				cusCode.OK_CodeType = OrgCusCode.CodeTypes.WorldCargoAssociationNumber;
				AssertEquals(false, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertHasError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos, null));

				Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos.IsAllowed = true;
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				AssertEquals(false, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertNoError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigNewModifyNonFinancialRegistrationNos, null));
			}
			finally
			{
				Env.Security.OrgConfigNewModifyFinancialRegistrationNos.IsAllowed = oldSecurityForNewPrimary;
				Env.Security.OrgConfigModifyNonFinancialRegistrationNumbers.IsAllowed = oldSecurityForNewNonPrimary;
			}
		}

		public void TestCheckOK_CodeType_WhenHeaderInDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var oldSecurityModifyFinancialARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyFinancialNonARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyNonFinancialARAPRegistrationNumbers = Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyNonFinancialNonARAPRegistrationNumbers = Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = false;

			try
			{
				var cusCode = org.CustomsCodes.AddNew();
				org.OH_IsDebtor = true;
				org.OH_IsCreditor = true;
				var companyARAPInfo = $"This organization is marked as A/R and A/P under Company {Env.CurrentCompany.Code}.";
				cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
				AssertEquals(true, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertHasError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, companyARAPInfo));

				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = true;
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CreditAgencyCode;
				AssertEquals(true, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertNoError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers, companyARAPInfo));

				cusCode.OK_CodeType = OrgCusCode.CodeTypes.WorldCargoAssociationNumber;
				AssertEquals(false, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertHasError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers, companyARAPInfo));

				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = true;
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				AssertEquals(false, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertNoError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers, companyARAPInfo));

				org.OH_IsDebtor = false;
				org.OH_IsCreditor = false;
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.WorldCargoAssociationNumber;
				AssertEquals(false, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertHasError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers, null));

				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				AssertEquals(false, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertNoError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers, null));

				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CreditAgencyCode;
				AssertEquals(true, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertHasError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers, null));

				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
				AssertEquals(true, cusCode.IsCurrentCompanyCodeTypePrimary);
				AssertNoError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers, null));
			}
			finally
			{
				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = oldSecurityModifyFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyFinancialNonARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialNonARAPRegistrationNumbers;
			}
		}

		// GB / UK / EU <-- for Find
		public void TestCheckOK_CustomsRegNoForUnitedKingdom()
		{
			var eoriCodePrefixWarning = "The EORI code does not need to start with the country/region code, this prefix will be added automatically when needed for messaging.";
			var turnSuffixWarning = "The EORI code you have supplied ends with something other than '000'";
			var vatValidationMessage = "UK VAT codes should be 9 digits. For EORI numbers, including dummy or special EORI numbers, select code type TRN or EOR.";
			var vGMInvalidFormatMessage = "A valid VGM number for GB has the format 1234/AA/MMYY. Where 1234 is the approval number, AA is the country/region code and MMYY is the expiry date.";
			var noPremisesAddressMessage = "An address is required for code type ";
			var vGMDifferentNumberMessage = "This number already exists for a different address.";

			// EOR / TRN
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "123456789000";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, eoriCodePrefixWarning);

			cusCode.OK_CustomsRegNo = "GB123456789000";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, eoriCodePrefixWarning);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, eoriCodePrefixWarning);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			cusCode.OK_CustomsRegNo = "123456789123";
			AssertNoWarningContaining(cusCode.OK_CustomsRegNoInfo, turnSuffixWarning);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Turn;
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, turnSuffixWarning);

			cusCode.OK_CustomsRegNo = "603484652006";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, turnSuffixWarning);

			// VAT:
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "123456789012";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, vatValidationMessage);
			cusCode.OK_CustomsRegNo = "123";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, vatValidationMessage);
			cusCode.OK_CustomsRegNo = "PR";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, vatValidationMessage);
			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, vatValidationMessage);

			// AEO:
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
			cusCode.OK_RN_NKCodeCountry = "GB";
			cusCode.OK_CustomsRegNo = "AEOF123456789";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "XXXF123456789";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "'AEO'");
			cusCode.OK_CustomsRegNo = "GBAEOF123456789";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "country");
			cusCode.OK_CustomsRegNo = "AEOX123456789";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "type");
			cusCode.OK_CustomsRegNo = "AEOS";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "short");

			// VGM:
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VGMRegistrationNumber;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			cusCode.OK_CustomsRegNo = "a234/AA/0111";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, vGMInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "1234/12/0111";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, vGMInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "1234/AA/1411";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, vGMInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "1234/gb/0111";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = "1234/AA/0111";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			AssertHasErrorContaining(cusCode.OK_OA_PremisesAddressInfo, noPremisesAddressMessage);
			cusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);

			var newCusCode = org.CustomsCodes.AddNew();
			newCusCode.OK_CodeType = OrgCusCode.CodeTypes.VGMRegistrationNumber;
			newCusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			newCusCode.OK_CustomsRegNo = "1234/AA/0111";
			newCusCode.OK_OA_PremisesAddress = org.Addresses.AddNew().PK;
			AssertHasError(newCusCode.OK_CustomsRegNoInfo, vGMDifferentNumberMessage);
			newCusCode.OK_CustomsRegNo = "1234/AA/1111";
			AssertNoErrors(newCusCode.OK_CustomsRegNoInfo);

			// CCD:

			var newFactory = new BusinessObjectFactory();
			newFactory.Load(ZZRefCusCodeListSchema.Constants.Prefix, ZGuid.NewZGuid()); // cause system to create DataTable
			var table = ((INeedDataSet)newFactory).Data.Tables[ZZRefCusCodeListSchema.Constants.TableName];
			var row = table.NewRow();
			var pk = ZGuid.NewZGuid();
			row[ZZRefCusCodeListSchema.Constants.PK] = pk.ToGuid();
			row[ZZRefCusCodeListSchema.Constants.ZZD_CodeType] = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupervisingOffice;
			row[ZZRefCusCodeListSchema.Constants.ZZD_Code] = "1";
			row[ZZRefCusCodeListSchema.Constants.ZZD_Description] = "1112";
			row[ZZRefCusCodeListSchema.Constants.ZZD_CountryOrGrouping] = CountryCodes.UnitedKingdom;
			row[ZZRefCusCodeListSchema.Constants.ZZD_StartDate] = ZDateTime.Today.AddYears(-1).ToDateTime();
			row[ZZRefCusCodeListSchema.Constants.ZZD_EndDate] = ZDateTime.Today.AddYears(1).ToDateTime();
			table.Rows.Add(row);
			var bizObj = newFactory.Load(ZZRefCusCodeListSchema.Constants.Prefix, pk);
			bizObj[ZZRefCusCodeListSchema.Constants.ZZD_Description] = "1111";
			newFactory.Save();

			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CustomsRegNo = "2";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The code you have selected is not in the list");
			cusCode.OK_CustomsRegNo = "1";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_CustomsRegNoForNorthernIreland()
		{
			var factory = new BusinessObjectFactory();

			var belfast = new RefUNLOCO.Loader(factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "NORTHERN IRELAND";
			}

			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "GBBEL";

			var orgCusCode = org.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;

			var message = "VAT Business Registration Number structures for Northern Ireland are either 'XI999 9999 99', 'XI999 9999 99 999', 'XIDG999', 'XIHA999', '999 9999 99', '999 9999 99 999', 'DG999' or 'HA999'.";

			foreach (var currentCode in new[]
			{
				"1", "12", "123", "1234", "12345", "123456", "1234567",
				"12345678", "1234567890", "12345678901", "1234567890123",
				"DG1234", "HA1234", "XIX12345678", "XI1234567",
				"123456789012XI", "1xi23456789012", "XI-1236578901",
				"XIDGABC", "HAABC"
			})
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				AssertHasWarning("OrgCusCode Should be have a Warning Message.", orgCusCode.OK_CustomsRegNoInfo, message);
			}

			foreach (var currentCode in new[]
			{
				"XI123456789", "XI123456789012", "123456789",
				"123456789012", "741369851234", "XIDG123",
				"DG123", "XIHA123", "HA123"
			})
			{
				orgCusCode.OK_CustomsRegNo = currentCode;
				AssertNoWarning("Assertion shouldn't have a warning message.", orgCusCode.OK_CustomsRegNoInfo, message);
			}
		}

		public void TestCheckOK_RegNoForCCDCSC()
		{
			var errorMessage = "Incorrect Check Digit. Last digit should be '6'";
			var errorMessage2 = "Incorrect Check Digit. Last digit should be '7'";
			var factoryForCCDCSC = new BusinessObjectFactory();

			var org2 = factoryForCCDCSC.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TSTOR2";

			var cusCode2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "10000094", CountryCodes.SouthAfrica);
			org2.MainAddress.OA_RN_NKCountryCode = "DE";
			cusCode2.OK_CustomsRegNo = "10000095";
			AssertNoMessageErrorContaining(cusCode2.OK_CustomsRegNoInfo, errorMessage);

			org2.MainAddress.OA_RN_NKCountryCode = "ZA";
			cusCode2.OK_CustomsRegNo = "10000094";
			AssertHasMessageErrorContaining(cusCode2.OK_CustomsRegNoInfo, errorMessage);

			org2.MainAddress.OA_RN_NKCountryCode = "ZA";
			cusCode2.OK_CustomsRegNo = "10000245";
			cusCode2.Validation.ValidateOK_CodeType();
			AssertHasMessageErrorContaining(cusCode2.OK_CustomsRegNoInfo, errorMessage2);

			var cusCode1 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "10000245", CountryCodes.SouthAfrica);
			cusCode1.Validation.ValidateOK_CodeType();
			AssertHasMessageErrorContaining(cusCode1.OK_CustomsRegNoInfo, errorMessage2);

			cusCode1.OK_CustomsRegNo = "10000247";
			cusCode1.Validation.ValidateOK_CodeType();
			AssertNoErrorContaining(cusCode1.OK_CustomsRegNoInfo, errorMessage2);
		}

		public void TestEoriCountryMixDown()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";

			CombineAssertions(() =>
			{
				var code1 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", CountryCodes.France);
				code1.Validation.ValidateOK_RN_NKCodeCountry();
				AssertNoErrors("When there is only 1 EORI there are no errors", code1.OK_RN_NKCodeCountryInfo);

				var code2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", CountryCodes.UnitedKingdom);
				code1.Validation.ValidateOK_RN_NKCodeCountry();
				code2.Validation.ValidateOK_RN_NKCodeCountry();
				AssertNoErrors("When there are just 1 EU EORI and 1 GB EORI there are no errors, code 1", code1.OK_RN_NKCodeCountryInfo);
				AssertNoErrors("When there are just 1 EU EORI and 1 GB EORI there are no errors, code 2", code2.OK_RN_NKCodeCountryInfo);

				var code3 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", CountryCodes.Italy);
				code1.Validation.ValidateOK_RN_NKCodeCountry();
				code2.Validation.ValidateOK_RN_NKCodeCountry();
				code3.Validation.ValidateOK_RN_NKCodeCountry();
				AssertNoErrorContaining("When there are 3 EORIs (2 EU and 1 GB) there is no error, for EORI living with another EORI, code 1", code1.OK_RN_NKCodeCountryInfo, "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.");
				AssertHasErrorContaining("When there are 3 EORIs (2 EU and 1 GB) there is an error, for EORI distinct countries, code 1", code1.OK_RN_NKCodeCountryInfo, "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB.");
				AssertNoErrorContaining("When there are 3 EORIs (2 EU and 1 GB) there is no error, for EORI living with another EORI, code 2", code2.OK_RN_NKCodeCountryInfo, "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.");
				AssertHasErrorContaining("When there are 3 EORIs (2 EU and 1 GB) there is an error, for EORI distinct countries, code 2", code2.OK_RN_NKCodeCountryInfo, "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB.");
				AssertNoErrorContaining("When there are 3 EORIs (2 EU and 1 GB) there is no error, for EORI living with another EORI, code 3", code3.OK_RN_NKCodeCountryInfo, "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.");
				AssertHasErrorContaining("When there are 3 EORIs (2 EU and 1 GB) there is an error, for EORI distinct countries, code 3", code3.OK_RN_NKCodeCountryInfo, "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB.");

				code3.OK_RN_NKCodeCountry = CountryCodes.Australia;
				code1.Validation.ValidateOK_RN_NKCodeCountry();
				code2.Validation.ValidateOK_RN_NKCodeCountry();
				code3.Validation.ValidateOK_RN_NKCodeCountry();
				AssertNoErrorContaining("When there are 3 EORIs (1 EU, 1 non EU and 1 GB) there is no error, for EORI living with another EORI, code 1", code1.OK_RN_NKCodeCountryInfo, "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.");
				AssertHasErrorContaining("When there are 3 EORIs (1 EU, 1 non EU and 1 GB) there is an error, for EORI distinct countries, code 1", code1.OK_RN_NKCodeCountryInfo, "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB.");
				AssertNoErrorContaining("When there are 3 EORIs (1 EU, 1 non EU and 1 GB) there is no error, for EORI living with another EORI, code 2", code2.OK_RN_NKCodeCountryInfo, "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.");
				AssertHasErrorContaining("When there are 3 EORIs (1 EU, 1 non EU and 1 GB) there is an error, for EORI distinct countries, code 2", code2.OK_RN_NKCodeCountryInfo, "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB.");
				AssertNoErrors("When there are 3 EORIs (1 EU, 1 non EU and 1 GB) there are no errors in the non EU one", code3.OK_RN_NKCodeCountryInfo);

				code1.OK_RN_NKCodeCountry = CountryCodes.Italy;
				code2.OK_RN_NKCodeCountry = CountryCodes.Italy;
				code3.OK_RN_NKCodeCountry = CountryCodes.Italy;
				code1.Validation.ValidateOK_RN_NKCodeCountry();
				code2.Validation.ValidateOK_RN_NKCodeCountry();
				code3.Validation.ValidateOK_RN_NKCodeCountry();
				AssertNoErrors("When there are 3 EU EORIs for the same country there are no errors, code 1", code1.OK_RN_NKCodeCountryInfo);
				AssertNoErrors("When there are 3 EU EORIs for the same country there are no errors, code 1", code2.OK_RN_NKCodeCountryInfo);
				AssertNoErrors("When there are 3 EU EORIs for the same country there are no errors, code 1", code3.OK_RN_NKCodeCountryInfo);

				code1.OK_RN_NKCodeCountry = CountryCodes.Australia;
				code3.Delete();
				code1.Validation.ValidateOK_RN_NKCodeCountry();
				code2.Validation.ValidateOK_RN_NKCodeCountry();
				AssertNoErrors("When there are 2 EORIs (1 EU and 1 non EU) there are no errors in the non EU one", code1.OK_RN_NKCodeCountryInfo);
				AssertHasErrorContaining("When there are 2 EORIs (1 EU and 1 non EU) there is an error, for EORI living with another EORI, code 2", code2.OK_RN_NKCodeCountryInfo, "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.");
				AssertNoErrorContaining("When there are 2 EORIs (1 EU and 1 non EU) there is no error, for EORI distinct countries, code 2", code2.OK_RN_NKCodeCountryInfo, "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB.");

				code1.OK_RN_NKCodeCountry = CountryCodes.France;
				code1.Validation.ValidateOK_RN_NKCodeCountry();
				code2.Validation.ValidateOK_RN_NKCodeCountry();
				AssertHasErrorContaining("When there are 2 EORIs (1 EU and 1 EU) there is an error, for EORI living with another EORI, code 1", code1.OK_RN_NKCodeCountryInfo, "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.");
				AssertNoErrorContaining("When there are 2 EORIs (1 EU and 1 EU) there is no error, for EORI distinct countries, code 1", code1.OK_RN_NKCodeCountryInfo, "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB.");
				AssertHasErrorContaining("When there are 2 EORIs (1 EU and 1 EU) there is an error, for EORI living with another EORI, code 2", code2.OK_RN_NKCodeCountryInfo, "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.");
				AssertNoErrorContaining("When there are 2 EORIs (1 EU and 1 EU) there is no error, for EORI distinct countries, code 2", code2.OK_RN_NKCodeCountryInfo, "You can't have EORI codes set for distinct countries/regions, unless they are one for an EU country and one for GB.");

				code1.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
				code1.Validation.ValidateOK_RN_NKCodeCountry();
				code2.Validation.ValidateOK_RN_NKCodeCountry();
				AssertNoErrors("When there are 2 EORIS (1 EU and 1 GB) there are no errors, code 1", code1.OK_RN_NKCodeCountryInfo);
				AssertNoErrors("When there are 2 EORIS (1 EU and 1 GB) there are no errors, code 2", code2.OK_RN_NKCodeCountryInfo);
			});
		}

		public void TestCheckOK_CodeType_ShouldCheckEori()
		{
			const string error = "An EU country or GB EORI code can't live with other EORI codes whose countries/regions are not an EU country nor GB.";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";

			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", CountryCodes.Germany);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, "123456789", CountryCodes.Spain);
			AssertNoError("Precondition", code2.OK_CodeTypeInfo, error);

			code2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			AssertHasError(code2.OK_RN_NKCodeCountryInfo, error);
		}

		public void TestValidateEORICode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var address1 = Factory.New<OrgAddress>();
			address1.AddressCode = "AAA";

			var code1 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", CountryCodes.France);

			var code2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", CountryCodes.UnitedKingdom);
			code2.OK_OA_PremisesAddress = address1.PK;
			AssertHasError(code2.OK_CustomsRegNoInfo, "This Registration Number is already in use by Organization Customs Code of this type.");

			code2.OK_CustomsRegNo = "223456789";
			AssertNoErrors(code2.OK_CustomsRegNoInfo);
		}

		public void TestValidateUKMCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";

			var address1 = Factory.New<OrgAddress>();
			address1.AddressCode = "AAA";

			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.UKInternalMarketSchemeCode, "UKM123456789", CountryCodes.France);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.UKInternalMarketSchemeCode, "UKM123456789", CountryCodes.UnitedKingdom);

			AssertHasError(code2.OK_CustomsRegNoInfo, "This Registration Number is already in use by Organization Customs Code of this type.");

			code2.OK_CustomsRegNo = "UKM223456789";
			AssertNoErrors(code2.OK_CustomsRegNoInfo);
		}

		public void TestValidateCodesCannotCoexistCountrySpecificForDE()
		{
			var expectedErrorMessage = "The following (DE) Registration codes cannot coexist: EPI, WPI";

			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Germany;
			cusCode.OK_CustomsRegNo = "EMCS1234";
			AssertNoError(cusCode.OK_CodeTypeInfo, expectedErrorMessage);

			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber;
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.Germany;
			cusCode2.OK_CustomsRegNo = "EMCS5678";
			cusCode2.OK_OA_PremisesAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertHasError(cusCode2.OK_CodeTypeInfo, expectedErrorMessage);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode2.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber;
			AssertNoError(cusCode2.OK_CodeTypeInfo, expectedErrorMessage);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Germany;
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber;
			cusCode.OK_OA_PremisesAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertNoError(cusCode.OK_CodeTypeInfo, expectedErrorMessage);
		}

		public void TestValidateCodesCannotCoexistCountrySpecificForTR()
		{
			var expectedErrorMessage = "The following (TR) Registration codes cannot coexist: VTE, VTC";

			cusCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTE;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Turkey;
			cusCode.OK_CustomsRegNo = "VTES1234";
			AssertNoError(cusCode.OK_CodeTypeInfo, expectedErrorMessage);

			cusCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTC;
			cusCode.OK_CustomsRegNo = "VTCS1234";
			AssertNoError(cusCode.OK_CodeTypeInfo, expectedErrorMessage);

			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTE;
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.Turkey;
			cusCode2.OK_CustomsRegNo = "VTE5678";
			cusCode2.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			AssertHasError(cusCode2.OK_CodeTypeInfo, expectedErrorMessage);

			cusCode2.OK_RN_NKCodeCountry = CountryCodes.Australia;
			AssertNoError(cusCode2.OK_CodeTypeInfo, expectedErrorMessage);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Turkey;
			cusCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.VTE;
			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			AssertNoError(cusCode.OK_CodeTypeInfo, expectedErrorMessage);
		}

		public void TestValidationPNR()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_RL_NKClosestPort = "DKAAB";
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "DK";
			cusCode.OK_CodeType = OrgCusCode.DenmarkCodeTypes.ProductionNumber;
			cusCode.OK_CustomsRegNo = "1";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Production Number should consist of 10 digits.");

			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Production Number should consist of 10 digits.");

			cusCode.OK_CustomsRegNo = "123456789a";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Production Number should consist of 10 digits.");

			cusCode.OK_CustomsRegNo = "1234567890";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidationVAT_Denmark()
		{
			var message = "VAT Business Registration Number structures for Denmark are either 'DK99999999' or '99999999'.";
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_RL_NKClosestPort = "DKAAB";
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "DK";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;

			cusCode.OK_CustomsRegNo = "DK12 34 56 78";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "12 34 56 78";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "DK12 34 56 7A";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "12 34 56 7A";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "DK12345678";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "12345678";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);
		}

		public void TestCustomsControlledPremisesID()
		{
			var country1 = Factory.Load<RefCountry>(CountryGuids.Australia);
			var country2 = Factory.Load<RefCountry>(CountryGuids.NewZealand);

			org.OH_FullName = "Org 1";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "Address1";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode.OK_RN_NKCodeCountry = country1.Code;
			cusCode.OK_CustomsRegNo = "1234D";
			AssertNoErrors("CCP is okay - no duplicates", cusCode.OK_CustomsRegNoInfo);

			Factory.Save();

			var org2 = OrgHeader.New(Factory);
			org2.OH_FullName = "Org 2";
			org2.OH_RL_NKClosestPort = "AUSYD";
			org2.MainAddress.OA_Address1 = "Address2";
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode2.OK_RN_NKCodeCountry = country1.Code;
			cusCode2.OK_CustomsRegNo = "1234D";
			AssertHasError("CCP is NOT okay - Duplicate found", cusCode2.OK_CustomsRegNoInfo, string.Format("This Controlled Premises ID is already registered for the organization {0} ({1})", "ORG1SYD", "Org 1"));

			cusCode2.OK_RN_NKCodeCountry = country2.Code;
			cusCode2.RunPreSaveValidation();
			AssertNoErrors("CCP is okay - duplicate in another country is allowed", cusCode2.OK_CustomsRegNoInfo);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode2.OK_RN_NKCodeCountry = country1.Code;
			cusCode2.OK_CustomsRegNo = "1234D";
			AssertHasWarning("CCP is NOT okay - Duplicate found but should now only be a warning", cusCode2.OK_CustomsRegNoInfo, string.Format("This Controlled Premises ID is already registered for the organization {0} ({1})", "ORG1SYD", "Org 1"));
		}

		public void TestValidateCustomsCodeAEOForBY()
		{
			cusCode.OK_CodeType = "AEO";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Belarus;
			cusCode.OK_CustomsRegNo = "12345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "AEO Codes should consist of 4 numeric characters.");

			cusCode.OK_CustomsRegNo = "123A";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "AEO Codes should consist of 4 numeric characters.");

			cusCode.OK_CustomsRegNo = "1234";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateCustomsCodeTRN_IsUniversal()
		{
			var turnSuffixWarning = "The EORI code you have supplied ends with something other than '000'";
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Turn;
			cusCode.OK_CustomsRegNo = "603484652006";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.France;
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, turnSuffixWarning);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, turnSuffixWarning);
		}

		public void TestValidateCustomsCodeEOR_IsUniversal()
		{
			var eoriPrefixWarning = "The EORI code does not need to start with the country/region code, this prefix will be added automatically when needed for messaging.";
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.France;
			cusCode.OK_CustomsRegNo = "FR603484652006";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, eoriPrefixWarning);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CustomsRegNo = "AU603484652006";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, eoriPrefixWarning);
		}

		public void TestValidateCustomsCode_AEO()
		{
			cusCode.OK_CodeType = "AEO";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Singapore;
			cusCode.OK_CustomsRegNo = "12345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "SG AEO number should consist of 12 alphanumeric characters.");

			cusCode.OK_CustomsRegNo = "1234567890ab";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.NewZealand;
			cusCode.OK_CustomsRegNo = "12345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "NZ AEO number should consist of 4 alphanumeric characters.");

			cusCode.OK_CustomsRegNo = "ab12";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.China;
			cusCode.OK_CustomsRegNo = "123456789a";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "CN AEO number should consist of 10 digits.");

			cusCode.OK_CustomsRegNo = "1234567890";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.HongKong;
			cusCode.OK_CustomsRegNo = "12333";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "HK AEO number should consist of 10 alphanumeric characters.");

			cusCode.OK_CustomsRegNo = "123456789a";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.KoreaSouth;
			cusCode.OK_CustomsRegNo = "12333";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "KR AEO number should consist of 7 alphanumeric characters.");

			cusCode.OK_CustomsRegNo = "1234a56";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCustomsControlledPremisesIDForEU()
		{
			var gbCountry = Factory.Load<RefCountry>(CountryGuids.UnitedKingdom);
			var itCountry = Factory.Load<RefCountry>(CountryGuids.Italy);
			var frCountry = Factory.Load<RefCountry>(CountryGuids.France);
			var ieCountry = Factory.Load<RefCountry>(CountryGuids.Ireland);

			CombineAssertions(() =>
			{
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				cusCode.OK_RN_NKCodeCountry = gbCountry.Code;
				cusCode.OK_CustomsRegNo = "1234";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be at most 17 and at least 6.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_CustomsRegNo = "A12GB";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be at most 17 and at least 6.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_CustomsRegNo = "a123GB";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be at most 17 and at least 6.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_CustomsRegNo = "f12345678901234GB";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be at most 17 and at least 6.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_CustomsRegNo = "G123456789012345GB";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be at most 17 and at least 6.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_RN_NKCodeCountry = itCountry.Code;
				cusCode.OK_CustomsRegNo = "1234";
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be 10. And the second to the seventh place should be 6 digits.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_CustomsRegNo = "AIT";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be 10. And the second to the seventh place should be 6 digits.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_CustomsRegNo = "F123WIT";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be 10. And the second to the seventh place should be 6 digits.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_CustomsRegNo = "F01194XWIT";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be 10. And the second to the seventh place should be 6 digits.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_CustomsRegNo = "F011942WIT";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be 10. And the second to the seventh place should be 6 digits.");
				AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect. Current value is: W, expected Q.");

				cusCode.OK_CustomsRegNo = "F011942QIT";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "Total length of the CCP code must be 10. And the second to the seventh place should be 6 digits.");
				AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The check digit in the CCP Code is incorrect.");

				cusCode.OK_RN_NKCodeCountry = frCountry.Code;
				cusCode.OK_CustomsRegNo = "1234";
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");

				cusCode.OK_RN_NKCodeCountry = ieCountry.Code;
				cusCode.OK_CustomsRegNo = "1234IE";
				AssertNoMessageError("IE CCP start with", cusCode.OK_CustomsRegNoInfo, "The CCP code must start with a character from A to F.");
				cusCode.OK_CustomsRegNo = "1234";
				AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
				cusCode.OK_CustomsRegNo = "IETW000050061";
				AssertNoMessageError("IE CCP start with", cusCode.OK_CustomsRegNoInfo, "The CCP code must end with the Country/Region of Issue.");
			});
		}

		public void TestCCPCodeValidation()
		{
			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CustomsRegNo = "1234";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, AUCCPValidator.InvalidEstablishmentCodeFormat);
			cusCode.OK_CustomsRegNo = "1234A";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, AUCCPValidator.InvalidEstablishmentCode + "D");
			cusCode.OK_CustomsRegNo = "1234D";
			AssertNoWarnings(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCustomsCarrierCodeIsUniqueInEachCountry()
		{
			var org1 = Factory.New<OrgHeader>();
			var org1CusCode1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", CountryCodes.UnitedStates);
			var org1CusCode2 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "KDHJ", CountryCodes.UnitedStates);

			var org2 = Factory.New<OrgHeader>();
			var org2CusCode1 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "ABCD", CountryCodes.UnitedStates);
			var message = "This Registration No is used in another organization in the same country/region";
			AssertNoWarning(org2CusCode1.OK_CodeTypeInfo, message);
			org2CusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			AssertHasWarning(org2CusCode1.OK_CodeTypeInfo, message);
			org2CusCode1.OK_CustomsRegNo = "KDHJ";
			org2CusCode1.Validation.ValidateOK_CodeType();
			AssertNoWarning(org2CusCode1.OK_CodeTypeInfo, message);
			org2CusCode1.OK_CodeType = OrgCusCode.CodeTypes.TruckCarrierCode;
			AssertHasWarning(org2CusCode1.OK_CodeTypeInfo, message);
			org2CusCode1.OK_RN_NKCodeCountry = CountryCodes.Canada;
			org2CusCode1.Validation.ValidateOK_CodeType();
			AssertNoWarning(org2CusCode1.OK_CodeTypeInfo, message);
			org1CusCode2.OK_RN_NKCodeCountry = CountryCodes.Canada;
			org2CusCode1.Validation.ValidateOK_CodeType();
			AssertNoWarning(org2CusCode1.OK_CodeTypeInfo, message);
			foreach (string country in CountryCodes.UsaAndTerritoriesList.Concat(new[] { CountryCodes.Peru }).Where(x => x != CountryCodes.UnitedStates))
			{
				org1CusCode2.OK_RN_NKCodeCountry = country;
				org2CusCode1.OK_RN_NKCodeCountry = country;
				org2CusCode1.Validation.ValidateOK_CodeType();
				AssertNoWarning(country, org2CusCode1.OK_CodeTypeInfo, message);
			}
		}

		public void TestValidateRegistrationForCargoWiseOneCarrierCodeIsUnique()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTTSTTST";
			org1.OH_IsShippingProvider = true;

			var cusCode = org1.CustomsCodes.AddNew();
			OrgCusCodeValidation testOrgCusCodeValidation;
			var typeList = new List<string>
			{
				OrgCusCode.CodeTypes.CarrierPrincipalCode,
				OrgCusCode.CodeTypes.CarrierCode,
				OrgCusCode.CodeTypes.CargoWiseOneCarrierCode
			};

			const string dupCode = "C199";

			foreach (var codeType in typeList)
			{
				cusCode.OK_CodeType = codeType;
				cusCode.OK_CustomsRegNo = dupCode;
				cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;

				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				AssertNoErrors(cusCode.OK_CodeTypeInfo);
				testOrgCusCodeValidation = new OrgCusCodeValidation(cusCode);
				testOrgCusCodeValidation.ValidateRegistrationForCargoWiseOneCarrierCodeIsUnique();
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				AssertNoErrors(cusCode.OK_CodeTypeInfo);
			}

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TSTTSTTSX";
			org2.OH_IsShippingProvider = true;

			var message = "C1C Code " + dupCode + " is already entered against Organization " + org1.OH_Code + ". \r\nPlease remove it from that Organization in order to save it against this one";
			cusCode = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, dupCode, CountryCodes.Australia);
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			AssertHasError(cusCode.OK_CodeTypeInfo, message);
		}

		public void TestValidateRegistrationForBoleroTitleRegisterIDIsUnique()
		{
			const string dupCode = "C199";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTTSTTST";
			org1.OH_IsShippingProvider = true;

			var cusCode1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, dupCode, CountryCodes.Australia);

			AssertNoErrors(cusCode1.OK_CustomsRegNoInfo);
			AssertNoErrors(cusCode1.OK_CodeTypeInfo);
			var testOrgCusCodeValidation = new OrgCusCodeValidation(cusCode1);
			testOrgCusCodeValidation.ValidateRegistrationForCargoWiseOneCarrierCodeIsUnique();
			AssertNoErrors(cusCode1.OK_CustomsRegNoInfo);
			AssertNoErrors(cusCode1.OK_CodeTypeInfo);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TSTTSTTSX";
			org2.OH_IsShippingProvider = true;

			var message = "TRI Code " + dupCode + " is already entered against Organization " + org1.OH_Code + ". It can only be added once.";
			var cusCode2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BoleroTitleRegisterID, dupCode, CountryCodes.Australia);
			AssertNoErrors(cusCode2.OK_CustomsRegNoInfo);
			AssertHasError(cusCode2.OK_CodeTypeInfo, message);
		}

		public void TestValidateRegistrationForContainerChainCommunityCodeIsUnique()
		{
			const string dupCode = "C199";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TSTTSTTST";
			org1.OH_FullName = "Test Organisation";

			var cusCode1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, dupCode, string.Empty);
			cusCode1.Validation.ValidateAll();
			AssertNoErrors(cusCode1.OK_CustomsRegNoInfo);
			AssertNoErrors(cusCode1.OK_CodeTypeInfo);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TSTTSTTSX";

			var cusCode2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, dupCode, string.Empty);
			cusCode2.Validation.ValidateAll();
			var message = "This Container Chain community code is already registered for the organization TSTTSTTST (Test Organisation)";
			AssertHasError(cusCode2.OK_CustomsRegNoInfo, message);
			AssertNoErrors(cusCode2.OK_CodeTypeInfo);

			cusCode2.OK_CustomsRegNo = "Unique";
			cusCode2.Validation.ValidateAll();
			AssertNoErrors(cusCode2.OK_CustomsRegNoInfo);
			AssertNoErrors(cusCode2.OK_CodeTypeInfo);
		}

		public void TestValidateAddressForContainerChainCommunityCodeIsUnique()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TSTTSTTST";
			org1.OH_FullName = "Test Organisation";

			var cusCode1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "XYZ", string.Empty);
			cusCode1.OK_OA_PremisesAddress = org1.MainAddress.PK;
			cusCode1.Validation.ValidateAll();
			AssertNoErrors(cusCode1.OK_CustomsRegNoInfo);
			AssertNoErrors(cusCode1.OK_CodeTypeInfo);

			var cusCode2 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "ABC", string.Empty);
			cusCode2.OK_OA_PremisesAddress = org1.MainAddress.PK;

			cusCode2.Validation.ValidateAll();
			var message = "Only one Container Chain community code can be issued per address.";
			AssertNoErrors(cusCode2.OK_CustomsRegNoInfo);
			AssertHasError(cusCode2.OK_CodeTypeInfo, message);

			var secondAddress = org1.Addresses.AddNew();
			cusCode2.OK_OA_PremisesAddress = secondAddress.PK;
			cusCode2.Validation.ValidateAll();
			AssertNoErrors(cusCode2.OK_CustomsRegNoInfo);
			AssertNoErrors(cusCode2.OK_CodeTypeInfo);
		}

		public void TestValidateRegistrationForCargoWiseRoadTransportProviderCodeIsUnique_IsUnique_NoErrors()
		{
			var orgA = Factory.New<OrgHeader>();
			orgA.OH_Code = new string('A', 9);
			orgA.OH_IsShippingProvider = true;
			orgA.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode, "C1RA");

			var orgB = Factory.New<OrgHeader>();
			orgB.OH_Code = new string('B', 9);
			orgB.OH_IsShippingProvider = true;

			var cusCode = orgB.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode, "C1RB");

			AssertNoErrors(cusCode.OK_CodeTypeInfo);
		}

		public void TestValidateRegistrationForCargoWiseRoadTransportProviderCodeIsUnique_IsNotUnique_HasError()
		{
			const string CODE = "CODE";

			var orgA = Factory.New<OrgHeader>();
			orgA.OH_Code = new string('A', 9);
			orgA.OH_IsShippingProvider = true;
			orgA.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode, CODE);

			var orgB = Factory.New<OrgHeader>();
			orgB.OH_Code = new string('B', 9);
			orgB.OH_IsShippingProvider = true;

			var cusCode = orgB.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode, CODE);

			AssertHasError(cusCode.OK_CodeTypeInfo, $"C1R Code {CODE} is already entered against Organization {orgA.OH_Code}. \r\nPlease remove it from that Organization in order to save it against this one");
		}

		public void TestCargoWiseOneCarrierCodeIsUnique()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTTSTTST";
			org1.OH_IsShippingProvider = true;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TSTTSTTSX";
			org2.OH_IsShippingProvider = true;

			const string dupCode = "C199";

			var org1CusCode1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, dupCode, CountryCodes.Australia);
			var org2CusCode1 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, dupCode, CountryCodes.Australia);

			AssertNoErrors(org1CusCode1.OK_CustomsRegNoInfo);
			AssertNoErrors(org2CusCode1.OK_CustomsRegNoInfo);

			var message = "C1C Code " + dupCode + " is already entered against Organization " + org1.OH_Code + ". \r\nPlease remove it from that Organization in order to save it against this one";
			org2CusCode1.Validation.ValidateOK_CodeType();
			AssertNoError(org2CusCode1.OK_CodeTypeInfo, message);

			org2CusCode1.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			AssertNoErrors(org2CusCode1.OK_CustomsRegNoInfo);

			org2CusCode1.Validation.ValidateOK_CodeType();
			AssertHasError(org2CusCode1.OK_CodeTypeInfo, message);

			org2CusCode1.OK_RN_NKCodeCountry = CountryCodes.Canada;
			org2CusCode1.Validation.ValidateOK_CodeType();
			AssertHasError(org2CusCode1.OK_CodeTypeInfo, message);

			org1CusCode1.Validation.ValidateOK_CodeType();
			message = "C1C Code " + dupCode + " is already entered against Organization " + org2.OH_Code + ". \r\nPlease remove it from that Organization in order to save it against this one";
			AssertHasError(org1CusCode1.OK_CodeTypeInfo, message);
		}

		public void TestValidateUniversalCodesExistOnlyOnce()
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.UniversalNettingCode;

			Assert("No errors as UNC exists only once", !cusCode.OK_CodeTypeInfo.HasErrors());

			var cusCode2 = cusCode.Header.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.NewZealand;
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.UniversalOfficeCode;

			Assert("No errors as UNC exists only once", !cusCode.OK_CodeTypeInfo.HasErrors());
			Assert("No errors as UOC exists only once", !cusCode2.OK_CodeTypeInfo.HasErrors());

			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.UniversalNettingCode;
			Assert("Errors as UNC duplicated, even in different countries", cusCode2.OK_CodeTypeInfo.HasErrors());
		}

		public void TestValidateOK_CodeTypeIndonesia()
		{
			var header = Factory.New<OrgHeader>();
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Indonesia;
			cusCode.OK_CodeType = OrgCusCode.IndonesiaCodeTypes.PPN;
			Assert("No errors", !cusCode.OK_CodeTypeInfo.HasErrors());

			var cusCode2 = header.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.Indonesia;
			cusCode2.OK_CodeType = OrgCusCode.IndonesiaCodeTypes.PP2;
			Assert("No errors", !cusCode2.OK_CodeTypeInfo.HasErrors());

			var cusCode3 = header.CustomsCodes.AddNew();
			cusCode3.OK_RN_NKCodeCountry = CountryCodes.Indonesia;
			cusCode3.OK_CodeType = OrgCusCode.IndonesiaCodeTypes.PP3;
			Assert("Error, as PP2 and PP3 cannot coexist", cusCode3.OK_CodeTypeInfo.HasErrors());

			header.CustomsCodes.Remove(cusCode3);
			cusCode2.RunPreSaveValidation();
			Assert("No Error, as PP3 was removed", !cusCode2.OK_CodeTypeInfo.HasErrors());

			header.CustomsCodes.Remove(cusCode);
			cusCode2.RunPreSaveValidation();
			Assert("Error, as to add PP2, PPN must exist. But it was removed", cusCode2.OK_CodeTypeInfo.HasErrors());

			header.CustomsCodes.Add(cusCode);
			cusCode2.RunPreSaveValidation();
			Assert("No Error, as PPN exists", !cusCode2.OK_CodeTypeInfo.HasErrors());

			header.CustomsCodes.Remove(cusCode2);
			header.CustomsCodes.Add(cusCode3);
			cusCode3.RunPreSaveValidation();
			Assert("No Error, as PPN, PP3 can exist together", !cusCode3.OK_CodeTypeInfo.HasErrors());

			header.CustomsCodes.Remove(cusCode);
			cusCode3.RunPreSaveValidation();
			Assert("Error, as to add PP3, PPN must exist. But it was removed", cusCode3.OK_CodeTypeInfo.HasErrors());

			header.CustomsCodes.Add(cusCode);
			cusCode3.RunPreSaveValidation();
			Assert("No Error, as PPN exists", !cusCode3.OK_CodeTypeInfo.HasErrors());
		}

		public void TestABNValidation()
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;

			cusCode.OK_CustomsRegNo = "123";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The entered ABN is not valid.\r\nAn ABN must be 11 or 14 digits with a valid check-digit.");

			cusCode.OK_CustomsRegNo = "21 003 980 130";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestARNValidation()
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.ARN;

			cusCode.OK_CustomsRegNo = "123";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "ARN number should be 12 digits.");

			cusCode.OK_CustomsRegNo = "123456789012";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_CustomsRegNo()
		{
			cusCode.OK_CustomsRegNo = "";
			Assert("OK_CustomsRegNo is mandatory", cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "a12dwer";
			Assert("No Error expected", !cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			Assert("OK_CustomsRegNo.MaxLength should be 4 when Code Type is '" + OrgCusCode.CodeTypes.CarrierCode + "' and country is 'US'", cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "a12d";
			Assert("OK_CustomsRegNo can only contain letters when Code Type is '" + OrgCusCode.CodeTypes.CarrierCode + "' and country is 'US'", cusCode.OK_CustomsRegNoInfo.HasWarnings());

			cusCode.OK_CustomsRegNo = "abcd";
			Assert("No error expected", !cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cusCode.OK_CustomsRegNo = "1234567L";
			cusCode.Validation.ValidateOK_CustomsRegNo();
			Assert("Invalid Supplier Code should have warning", cusCode.OK_CustomsRegNoInfo.HasMessageErrors());

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cusCode.OK_CustomsRegNo = "0074873R";
			cusCode.Validation.ValidateOK_CustomsRegNo();
			Assert("Valid Supplier Code should have no warning", !cusCode.OK_CustomsRegNoInfo.HasMessageErrors());

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CustomsRegNo = "1234567V";
			cusCode.Validation.ValidateOK_CustomsRegNo();
			Assert("Invalid Customs Client Code should have warning", cusCode.OK_CustomsRegNoInfo.HasMessageErrors());

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CustomsRegNo = "0059781A";
			cusCode.Validation.ValidateOK_CustomsRegNo();
			Assert("Valid Customs Client Code should have no warning", !cusCode.OK_CustomsRegNoInfo.HasMessageErrors());

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			cusCode.OK_CustomsRegNo = "005978A";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A GS1 Company prefix can only contain numbers 0-9.");

			cusCode.OK_CustomsRegNo = "0059781";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "005978";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A GS1 Company prefix must have a length of 7-11 digits.");

			cusCode.OK_CustomsRegNo = "00597812";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "005978123";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "0059781234";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "00597812345";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "005978123456";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A GS1 Company prefix must have a length of 7-11 digits.");

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CompanyNumber;
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber;
			cusCode.OK_CustomsRegNo = "123456";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Export number should be two alpha followed by 4 or 5 digits.");

			cusCode.OK_CustomsRegNo = "ABC1234";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Export number should be two alpha followed by 4 or 5 digits.");

			cusCode.OK_CustomsRegNo = "AB123456";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Export number should be two alpha followed by 4 or 5 digits.");

			cusCode.OK_CustomsRegNo = "AB12345";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "AB1234";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_CustomsRegNo = "a13d";
			Assert("A valid CargoWiseOne Carrier Code is composed of the prefix C1 followed by two alpha-numeric characters.", cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "C1B2";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_RN_NKCodeCountry = string.Empty;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			cusCode.OK_CustomsRegNo = "a13d";
			Assert("A valid CargoWiseOne Carrier Code is composed of the prefix C1 followed by two alpha-numeric characters.", cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "C1B2";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateNAICS()
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem;
			cusCode.OK_CustomsRegNo = "abcde";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A valid North American Industry Classification System code is composed of 2 to 6 digits");

			cusCode.OK_CustomsRegNo = "1";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A valid North American Industry Classification System code is composed of 2 to 6 digits");

			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A valid North American Industry Classification System code is composed of 2 to 6 digits");

			cusCode.OK_CustomsRegNo = "123456";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateSIC()
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.StandardIndustrialClassification;
			cusCode.OK_CustomsRegNo = "abcde";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A valid Standard Industrial Classification code is composed of 4 digits");

			cusCode.OK_CustomsRegNo = "123456";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A valid Standard Industrial Classification code is composed of 4 digits");

			cusCode.OK_CustomsRegNo = "1234";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestGS1PrefixesCannotBeDuplicated()
		{
			var factoryForDuplicate = new BusinessObjectFactory();

			var org = factoryForDuplicate.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCORG";

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "1234567";

			factoryForDuplicate.Save();

			this.cusCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			this.cusCode.OK_RN_NKCodeCountry = "NZ";
			this.cusCode.OK_CustomsRegNo = "1234567";
			AssertHasError(this.cusCode.OK_CustomsRegNoInfo, "The GS1 Company prefix '1234567' has already been used for Organization 'ABCORG'.");

			using (OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue()))
			{
				this.cusCode.OK_CustomsRegNo = "1234567";
				AssertHasError("Should *not* be an opt in error message.", this.cusCode.OK_CustomsRegNoInfo, "The GS1 Company prefix '1234567' has already been used for Organization 'ABCORG'.");
			}

			this.cusCode.OK_CustomsRegNo = "1234568";
			AssertNoErrors(this.cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_OA_PremisesAddress()
		{
			var address = Factory.New<OrgAddress>();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ManifestProviderID;
			cusCode.OK_OA_PremisesAddress = address.PK;
			Assert("Cannot have Premises address for this address type", cusCode.OK_OA_PremisesAddressInfo.HasErrors());

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			Assert("Can have Premises address for this address type", !cusCode.OK_OA_PremisesAddressInfo.HasErrors());

			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			Assert("Can have empty Premises address for this address type", !cusCode.OK_OA_PremisesAddressInfo.HasErrors());

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ManifestProviderID;
			Assert("Can have empty Premises address for this address type", !cusCode.OK_OA_PremisesAddressInfo.HasErrors());

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			Assert("Can have Premises address for this address type", !cusCode.OK_OA_PremisesAddressInfo.HasErrors());

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4;
			Assert("Can have Premises address for this address type", !cusCode.OK_OA_PremisesAddressInfo.HasErrors());
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Country Code", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, cusCode.OK_RN_NKCodeCountry);
		}

		public void TestValidateMalaysianOtherBusinessCodes()
		{
			cusCode.OK_CodeType = MalaysiaOrgCusCodeInfo.OrgCusCodes.OtherBusinessCode;
			cusCode.OK_RN_NKCodeCountry = "MY";

			cusCode.OK_CustomsRegNo = "359023";
			AssertEquals("Should be an error for a code that doesn't specify the code type", true, cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "Z";
			AssertEquals("Should not be an error when the code type specified", false, cusCode.OK_CustomsRegNoInfo.HasErrors());
		}

		public void TestValidateMalaysianPersonalIdentificationCardNumber()
		{
			cusCode.OK_CodeType = MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber;
			cusCode.OK_RN_NKCodeCountry = "MY";

			var defaultValue = OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue();
			var mypic = defaultValue.Cast<CodeDescriptionBool>().First(x => x.Code == RegistrationNumberFormatFields.MYPIC);
			mypic.Bool = true;
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			cusCode.OK_CustomsRegNo = "359023";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The entered Personal Identification Card Number '359023' is invalid. It should be in format NNNNNNNNNNNN with 12 numeric digits");

			cusCode.OK_CustomsRegNo = "123456789012";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			mypic.Bool = false;
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
			cusCode.OK_CustomsRegNo = "12";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, "The entered Personal Identification Card Number '12' is invalid. It should be in format NNNNNNNNNNNN with 12 numeric digits");
			cusCode.OK_CustomsRegNo = "123456789012";
			AssertNoWarnings(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateZAVAT()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_RN_NKCodeCountry = "ZA";
			cusCode.OK_CustomsRegNo = "4150161869";
			Assert("!HasWarnings", !cusCode.OK_CustomsRegNoInfo.HasWarnings());
			cusCode.OK_CustomsRegNo = "2150161869";
			Assert("HasWarnings", cusCode.OK_CustomsRegNoInfo.HasWarnings());
			cusCode.OK_CustomsRegNo = "4";
			Assert("HasWarnings", cusCode.OK_CustomsRegNoInfo.HasWarnings());
		}

		public void TestValidateZAIDNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.IDNumber;
			cusCode.OK_RN_NKCodeCountry = "ZA";
			cusCode.OK_CustomsRegNo = "1234567890128";
			Assert("!HasErrors", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "1234567890123";
			Assert("HasErrors", cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "1";
			Assert("HasErrors", cusCode.OK_CustomsRegNoInfo.HasErrors());
		}

		public void TestInvalidSupplierCodeInSouthAfrica()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cusCode.OK_RN_NKCodeCountry = "ZA";
			cusCode.OK_CustomsRegNo = "XYZ123";
			AssertEquals("CusCode.OK_CustomsRegNoInfo.HasErrors()", false, cusCode.OK_CustomsRegNoInfo.HasErrors());
		}

		public void TestValidateCRNForSG()
		{
			cusCode.OK_CodeType = OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber;
			cusCode.OK_RN_NKCodeCountry = "SG";
			cusCode.OK_CustomsRegNo = "1224797000Z";
			AssertEquals(true, cusCode.OK_CustomsRegNoInfo.HasWarnings());
		}

		public void TestValidateQCIForSG()
		{
			cusCode.OK_CodeType = OrgCusCode.SingaporeCodeTypes.QualifiedCompanyIdentificationCode;
			cusCode.OK_RN_NKCodeCountry = "SG";

			cusCode.OK_CustomsRegNo = "123";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasMessageErrors());

			cusCode.OK_CustomsRegNo = "12";
			AssertEquals(true, cusCode.OK_CustomsRegNoInfo.HasMessageErrors());

			cusCode.OK_CustomsRegNo = "UPS";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasMessageErrors());

			cusCode.OK_CustomsRegNo = "1234";
			AssertEquals(true, cusCode.OK_CustomsRegNoInfo.HasMessageErrors());
		}

		public void TestValidatePSTForSG()
		{
			cusCode.OK_CodeType = OrgCusCode.SingaporeCodeTypes.PartyStatusType;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Singapore;

			cusCode.OK_CustomsRegNo = "A";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "X";
			AssertHasMessageErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateIBGForSG()
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Singapore;
			cusCode.OK_CodeType = OrgCusCode.SingaporeCodeTypes.InterbankGIRO;

			// Check error when empty
			AssertHasErrors(cusCode.OK_CustomsRegNoInfo);

			// check error invalid chars
			cusCode.OK_CustomsRegNo = "ABC-def-123";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The Interbank GIRO code contains invalid characters.");

			// check error over 30 chars
			cusCode.OK_CustomsRegNo = "1234567890123456789012345678901";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "A valid Interbank GIRO code must contain no more than 30 characters.");

			cusCode.OK_CustomsRegNo = "ABCdef123";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The Interbank GIRO code contains invalid characters.");
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "A valid Interbank GIRO code must contain no more than 30 characters.");
		}

		public void TestValidateDIRForSG()
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Singapore;
			cusCode.OK_CodeType = OrgCusCode.SingaporeCodeTypes.DirectDelivery;

			// Check no error when empty
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);

			// check warning when not 'Y"
			cusCode.OK_CustomsRegNo = "N";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, "Any value will enable Direct Delivery. Remove the entry to disable.");

			// check no warning when is 'Y'
			cusCode.OK_CustomsRegNo = "Y";
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateUENForSG()
		{
			cusCode.OK_CodeType = OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber;
			cusCode.OK_RN_NKCodeCountry = "SG";
			cusCode.OK_CustomsRegNo = "1987002E";
			AssertEquals(true, cusCode.OK_CustomsRegNoInfo.HasWarnings());
			cusCode.OK_CustomsRegNo = "198700002E";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarnings());
			cusCode.OK_CustomsRegNo = "S65DP0022B";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarnings());
			cusCode.OK_CustomsRegNo = "12345678X";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarnings());

			cusCode.OK_CustomsRegNo = "198700002E";
			AssertEquals("Correct UEN Format", false, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));
			cusCode.OK_CustomsRegNo = "S65DP0022B";
			AssertEquals("Correct UEN Format", false, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));
			cusCode.OK_CustomsRegNo = "12345678X";
			AssertEquals("Correct UEN Format", false, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));
			cusCode.OK_CustomsRegNo = "1987LL002E";
			AssertEquals("Invalid UEN Format", true, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));
			cusCode.OK_CustomsRegNo = "T08SM0001E";
			AssertEquals("Correct UEN Format", false, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));
			cusCode.OK_CustomsRegNo = "T083SM001E";
			AssertEquals("Invalid UEN Format", true, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));

			cusCode.OK_CustomsRegNo = "99991000000G";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));

			cusCode.OK_CustomsRegNo = "99999990000C";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));

			cusCode.OK_CustomsRegNo = "99999000000N";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));

			cusCode.OK_CustomsRegNo = "X1234567890N";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarning(OrgCusCodeValidation.UENInvalidFormat));
		}

		public void TestValidateCodeTypeForCNOnlyWorksForChina()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_RN_NKCodeCountry = "BG";
			cusCode.OK_CustomsRegNo = "12312121";

			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "CN";
			cusCode2.OK_CodeType = OrgCusCode.ChinaCodeTypes.BST;
			cusCode2.OK_CustomsRegNo = "45234543";
			AssertNoError(cusCode2.OK_CodeTypeInfo, "Either VAT or BST code can be entered, but not both.");

			cusCode.OK_RN_NKCodeCountry = "CN";
			cusCode.Validation.ValidateOK_CodeType();
			AssertHasError(cusCode.OK_CodeTypeInfo, "Either VAT or BST code can be entered, but not both.");
			cusCode2.Validation.ValidateOK_CodeType();
			AssertHasError(cusCode2.OK_CodeTypeInfo, "Either VAT or BST code can be entered, but not both.");
		}

		public void TestValidateBSTAndVATForCN()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_RN_NKCodeCountry = "CN";
			cusCode.OK_CustomsRegNo = "12312121";

			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "CN";
			cusCode2.OK_CodeType = OrgCusCode.ChinaCodeTypes.BST;
			cusCode2.OK_CustomsRegNo = "45234543";
			AssertHasError(cusCode2.OK_CodeTypeInfo, "Either VAT or BST code can be entered, but not both.");

			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			AssertNoError(cusCode2.OK_CodeTypeInfo, "Either VAT or BST code can be entered, but not both.");
		}

		public void TestValidateVAGAndVASForCN()
		{
			cusCode.OK_RN_NKCodeCountry = "CN";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "12312121";

			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "CN";
			cusCode2.OK_CodeType = OrgCusCode.ChinaCodeTypes.VAG;
			cusCode2.OK_CustomsRegNo = "43243244";

			var cusCode3 = org.CustomsCodes.AddNew();
			cusCode3.OK_RN_NKCodeCountry = "CN";
			cusCode3.OK_CodeType = OrgCusCode.ChinaCodeTypes.VAS;
			cusCode3.OK_CustomsRegNo = "45234543";
			AssertHasError(cusCode3.OK_CodeTypeInfo, "Either VAG or VAS code can be entered, but not both.");

			cusCode3.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			AssertNoError(cusCode3.OK_CodeTypeInfo, "Either VAG or VAS code can be entered, but not both.");
		}

		public void TestValidateVATAndVAGForCN()
		{
			cusCode.OK_RN_NKCodeCountry = "CN";
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.VAG;
			cusCode.OK_CustomsRegNo = "12312121";
			AssertHasError(cusCode.OK_CodeTypeInfo, "VAG should be used with VAT together.");

			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "CN";
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode2.OK_CustomsRegNo = "45234543";
			AssertNoError(cusCode2.OK_CodeTypeInfo, "VAG should be used with VAT together.");
		}

		public void TestValidateVATAndVASForCN()
		{
			cusCode.OK_RN_NKCodeCountry = "CN";
			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.VAS;
			cusCode.OK_CustomsRegNo = "12312121";
			AssertHasError(cusCode.OK_CodeTypeInfo, "VAS should be used with VAT together.");

			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "CN";
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode2.OK_CustomsRegNo = "45234543";
			AssertNoError(cusCode2.OK_CodeTypeInfo, "VAS should be used with VAT together.");
		}

		public void TestValidateNIFForAO()
		{
			cusCode.OK_CodeType = OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal;
			cusCode.OK_RN_NKCodeCountry = "AO";
			cusCode.OK_CustomsRegNo = "12345678901";
			AssertEquals("Angola's NIF must be 10 characters", true, cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertEquals("Angola's NIF must be 10 characters", false, cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "123456789A";
			AssertEquals("Angola's NIF format should be ##########", true, cusCode.OK_CustomsRegNoInfo.HasWarning("Format of NIF / Tax Identification Number (NIF) is not correct. It should contain 10 digits, e.g: '1234567890'."));
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertEquals("Angola's NIF format should be ##########", false, cusCode.OK_CustomsRegNoInfo.HasWarning("Format of NIF / Tax Identification Number (NIF) is not correct. It should contain 10 digits, e.g: '1234567890'."));
		}

		public void TestValidateCCNForNL()
		{
			cusCode.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber;
			cusCode.OK_RN_NKCodeCountry = "NL";
			cusCode.OK_CustomsRegNo = "123456789";
			AssertEquals("Netherland's CCN must be 8 characters", true, cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "12345678";
			AssertEquals("Netherland's CCN must be 8 characters", false, cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "1234567A";
			AssertEquals("Netherland's CCN format should be ########", true, cusCode.OK_CustomsRegNoInfo.HasWarning("Format of Chamber of Commerce (CCN) is not correct. It should contain 8 digits, e.g: '12345678'."));
			cusCode.OK_CustomsRegNo = "12345678";
			AssertEquals("Netherland's CCN format should be ########", false, cusCode.OK_CustomsRegNoInfo.HasWarning("Format of Chamber of Commerce (CCN) is not correct. It should contain 8 digits, e.g: '12345678'."));
		}

		public void TestValidateBTWForNL()
		{
			var message = "VAT Business Registration Number structures for Netherlands are either 'NLXXXXXXXXXXXX' or 'XXXXXXXXXXXX'.";
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
			cusCode.OK_RN_NKCodeCountry = "NL";

			cusCode.OK_CustomsRegNo = "1234567890ABC";
			AssertEquals(true, cusCode.OK_CustomsRegNoInfo.HasWarning(message));
			cusCode.OK_CustomsRegNo = "NL1234567890ABC";
			AssertEquals(true, cusCode.OK_CustomsRegNoInfo.HasWarning(message));

			cusCode.OK_CustomsRegNo = "123456789A*+";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarning(message));
			cusCode.OK_CustomsRegNo = "NL123456789A*+";
			AssertEquals(false, cusCode.OK_CustomsRegNoInfo.HasWarning(message));

			cusCode.OK_CustomsRegNo = "123456789-*+";
			AssertEquals(true, cusCode.OK_CustomsRegNoInfo.HasWarning(message));
		}

		public void TestValidateHRBForDE()
		{
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.Handelsregister;
			cusCode.OK_RN_NKCodeCountry = "DE";

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());

			cusCode.OK_CustomsRegNo = "XXX";
			AssertEquals("HRB length must be more than 3 characters.", true, cusCode.OK_CustomsRegNoInfo.HasWarnings());
			cusCode.OK_CustomsRegNo = "HRA 1234";
			AssertEquals("HRB length must be more than 3 characters.", false, cusCode.OK_CustomsRegNoInfo.HasWarnings());

			cusCode.OK_CustomsRegNo = "XXX 123456";
			AssertEquals("Germany's HRB format should be HRA ######## or HRB ########", true, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA 1234";
			AssertEquals("Germany's HRB format should be HRA ######## or HRB ########", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRB 1234";
			AssertEquals("Germany's HRB format should be HRA ######## or HRB ########", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());

			cusCode.OK_CustomsRegNo = "XXX";
			AssertEquals("HRB length must be more than 3 characters.", true, cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "HRA 1234";
			AssertEquals("HRB length must be more than 3 characters.", false, cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "XXX 123456";
			AssertEquals("Germany's HRB format should be HRA ######## or HRB ########", true, cusCode.OK_CustomsRegNoInfo.HasError(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA 1234";
			AssertEquals("Germany's HRB format should be HRA ######## or HRB ########", false, cusCode.OK_CustomsRegNoInfo.HasError(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRB 1234";
			AssertEquals("Germany's HRB format should be HRA ######## or HRB ########", false, cusCode.OK_CustomsRegNoInfo.HasError(HRBWarningMessage));
		}

		public void TestUpdatedHRBForDE()
		{
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.Handelsregister;
			cusCode.OK_RN_NKCodeCountry = "DE";

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());

			cusCode.OK_CustomsRegNo = "HRA 1234HRB 1234";
			AssertEquals("Appending two valid handelsregisters is not valid", true, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA1234";
			AssertEquals("Having no space after the initial 3 chars is invalid", true, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));

			cusCode.OK_CustomsRegNo = "HRA 1234HL";
			AssertEquals("HL at the end without a space is valid for HRA", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRB 1234HL";
			AssertEquals("HL at the end without a space is valid for HRB", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA 1234 HL";
			AssertEquals("HL at the end with a space is valid for HRA", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRB 1234 HL";
			AssertEquals("HL at the end with a space is valid for HRA", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));

			cusCode.OK_CustomsRegNo = "HRA 1234SOMERANDOMSTRING";
			AssertEquals("An abitrary string at the end without a space is valid for HRA", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRB 1234SOMERANDOMSTRING";
			AssertEquals("An abitrary string at the end without a space is valid for HRB", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA 1234 SOMERANDOMSTRING";
			AssertEquals("An abitrary string at the end with a space is valid for HRA", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRB 1234 SOMERANDOMSTRING";
			AssertEquals("An abitrary string at the end with a space is valid for HRA", false, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));

			cusCode.OK_CustomsRegNo = "HRA 1234\tHL";
			AssertEquals("Tab is not valid whitespace", true, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA 1234\nHL";
			AssertEquals("Newline is not valid whitespace", true, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA 1234\vHL";
			AssertEquals("Vertical tab is not valid whitespace", true, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA 1234\fHL";
			AssertEquals("Form feed is not valid whitespace", true, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
			cusCode.OK_CustomsRegNo = "HRA 1234\rHL";
			AssertEquals("Carriage return is not valid whitespace", true, cusCode.OK_CustomsRegNoInfo.HasWarning(HRBWarningMessage));
		}

		public void TestValidateEBSForDE()
		{
			var errMsg = "EORI Branch Suffix should be 4 digits and different to the EORI code, e.g. 0001";
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			cusCode.OK_RN_NKCodeCountry = "DE";

			cusCode.OK_CustomsRegNo = "123";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, errMsg);

			cusCode.OK_CustomsRegNo = "0AS1";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, errMsg);

			cusCode.OK_CustomsRegNo = "12345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, errMsg);

			cusCode.OK_CustomsRegNo = "1234";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, errMsg);
		}

		public void TestValidateEXDOCExporterNumberForAU()
		{
			cusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "1234";
			Assert("EXDOC Exporter number has length of 4", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "1234A";
			Assert("EXDOC Exporter number has length of 5", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "123456";
			Assert("EXDOC Exporter number has length of 6", cusCode.OK_CustomsRegNoInfo.HasErrors());
		}

		public void TestValidateEXDOCAMLCPerformanceExporterNumberForAU()
		{
			cusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCAMLCPerformanceExporterNumber;
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "1234";
			Assert("EXDOC AMLC performance exporter number has length of 4", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "1234A";
			Assert("EXDOC AMLC performance exporter number has length of 5", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "123456";
			Assert("EXDOC AMLC performance exporter number has length of 6", cusCode.OK_CustomsRegNoInfo.HasErrors());
		}

		public void TestValidateEXDOCEstablihsmentNumberForAU()
		{
			cusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "12345";
			Assert("EXDOC establishment number has length of 5", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "123456";
			Assert("EXDOC establishment number has length of 6", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "12345A";
			Assert("EXDOC establishment number has length of 6", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "1234567";
			Assert("EXDOC establishment number has length of 7", cusCode.OK_CustomsRegNoInfo.HasErrors());

			var address1 = org.Addresses.AddNew();
			cusCode.OK_OA_PremisesAddress = address1.PK;
			Assert(!cusCode.OK_OA_PremisesAddressInfo.HasErrors());

			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = "AU";
			cusCode2.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			cusCode2.OK_CustomsRegNo = "23456";
			Assert(!cusCode2.OK_CodeTypeInfo.HasErrors());
			AssertHasError(cusCode2.OK_OA_PremisesAddressInfo, "An address is required for code type 'ESN'.");

			var cusCode3 = org.CustomsCodes.AddNew();
			cusCode3.OK_RN_NKCodeCountry = "AU";
			cusCode3.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			cusCode3.OK_CustomsRegNo = "34567";
			AssertHasError(cusCode3.OK_CodeTypeInfo, "Each code of this type must have a different country/region and address combination.");

			cusCode3.OK_OA_PremisesAddress = address1.PK;
			AssertHasError(cusCode3.OK_CodeTypeInfo, "Each code of this type must have a different country/region and address combination.");

			var address2 = org.Addresses.AddNew();
			cusCode3.OK_OA_PremisesAddress = address2.PK;
			Assert(!cusCode3.OK_CodeTypeInfo.HasErrors());
		}

		public void TestValidateEXDOCEDIUserForAU()
		{
			cusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser;
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "123456";
			Assert("EXDOC establishment number has length of 6", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "1234567";
			Assert("EXDOC establishment number has length of 7", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "123456A";
			Assert("EXDOC establishment number has length of 7", !cusCode.OK_CustomsRegNoInfo.HasErrors());
			cusCode.OK_CustomsRegNo = "12345678";
			Assert("EXDOC establishment number has length of 8", cusCode.OK_CustomsRegNoInfo.HasErrors());
		}

		public void TestValidateSpainCusCodeRule()
		{
			var expectedErrorMessage = "The following (ES) Registration codes cannot coexist: NIF, DNI, IGC";

			org.CustomsCodes.RemoveAll();
			var spain = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, CountryCodes.Spain));
			var code1 = AddNewCusCode(spain, OrgCusCode.SpainCodeTypes.NIF);
			code1.Validation.ValidateOK_CodeType();
			AssertNoErrors(code1.OK_CodeTypeInfo);

			var code2 = AddNewCusCode(spain, OrgCusCode.SpainCodeTypes.DNI);
			code2.Validation.ValidateOK_CodeType();
			AssertHasError(code2.OK_CodeTypeInfo, expectedErrorMessage);

			code2.OK_CodeType = OrgCusCode.SpainCodeTypes.IGC;
			code2.Validation.ValidateOK_CodeType();
			AssertHasError(code2.OK_CodeTypeInfo, expectedErrorMessage);

			code1.OK_RN_NKCodeCountry = CountryCodes.Australia;
			code2.OK_CodeType = OrgCusCode.SpainCodeTypes.IGC;
			code2.Validation.ValidateOK_CodeType();
			AssertNoError(code2.OK_CodeTypeInfo, expectedErrorMessage);
		}

		public void TestValidateIndonesiaCusCodeRule()
		{
			var expectedErrorMessage = "The following (ID) Registration codes cannot coexist: PP2, PP3";

			org.CustomsCodes.RemoveAll();
			var indonesia = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, CountryCodes.Indonesia));
			var code1 = AddNewCusCode(indonesia, OrgCusCode.IndonesiaCodeTypes.PP2);

			var code2 = AddNewCusCode(indonesia, OrgCusCode.IndonesiaCodeTypes.PP3);
			code2.Validation.ValidateOK_CodeType();
			AssertHasError(code2.OK_CodeTypeInfo, expectedErrorMessage);

			code1.OK_RN_NKCodeCountry = CountryCodes.Colombia;
			code2.OK_CodeType = OrgCusCode.IndonesiaCodeTypes.PP3;
			code2.Validation.ValidateOK_CodeType();
			AssertNoError(code2.OK_CodeTypeInfo, expectedErrorMessage);
		}

		public void TestValidateCusCodeIsUnique()
		{
			org.CustomsCodes.RemoveAll();

			var country1 = Factory.New<RefCountry>();
			var country2 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, country1.Code));

			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();

			var code1 = AddNewCusCode(country1, OrgCusCode.CodeTypes.CustomsClientCode);
			code1.Validation.ValidateOK_CodeType();
			AssertNoErrors(code1.OK_CodeTypeInfo);

			var code2 = AddNewCusCode(country1, OrgCusCode.CodeTypes.CustomsClientCode);
			code2.Validation.ValidateOK_CodeType();
			AssertHasError(code2.OK_CodeTypeInfo, "There can only be one code of this type for each country/region.");

			code2.OK_CodeType = OrgCusCode.CodeTypes.ManifestProviderID;
			AssertNoErrors(code2.OK_CodeTypeInfo);

			var code3 = AddNewCusCode(country2, OrgCusCode.CodeTypes.CustomsClientCode);
			AssertNoErrors(code3.OK_CodeTypeInfo);

			code1.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			code2.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			AssertNoErrors(code1.OK_CodeTypeInfo);
			AssertHasError(code2.OK_CodeTypeInfo, "Each code of this type must have a different country/region and address combination.");

			code2.OK_OA_PremisesAddress = address1.PK;
			AssertNoErrors(code2.OK_CodeTypeInfo);

			code3.OK_RN_NKCodeCountry = country1.Code;
			code3.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			AssertHasError(code3.OK_CodeTypeInfo, "Each code of this type must have a different country/region and address combination.");

			code3.OK_OA_PremisesAddress = address2.PK;
			AssertNoErrors(code3.OK_CodeTypeInfo);

			const string imaMessageError = "Only one record allowed (Uniqueness: Country of Issue + Type 'IMA')";

			code1.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.IMA;
			code2.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.IMA;
			code3.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.IMA;
			AssertNoMessageErrorContaining(code1.OK_CodeTypeInfo, imaMessageError);
			AssertHasMessageError(code2.OK_CodeTypeInfo, imaMessageError);
			AssertHasMessageError(code3.OK_CodeTypeInfo, imaMessageError);
			AssertNoErrorContaining(code3.OK_CodeTypeInfo, "There can only be one code of this type for each country/region.");

			code1.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			code2.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			code3.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			AssertNoErrors(code1.OK_CodeTypeInfo);
			AssertHasError(code2.OK_CodeTypeInfo, "There can only be one code of this type for each country/region.");
			AssertHasError(code3.OK_CodeTypeInfo, "There can only be one code of this type for each country/region.");
			AssertNoMessageErrorContaining(code3.OK_CodeTypeInfo, imaMessageError);

			code1.OK_RN_NKCodeCountry = CountryCodes.China;
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_OA_PremisesAddress = address1.PK;
			code2.OK_RN_NKCodeCountry = CountryCodes.China;
			code2.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code2.OK_OA_PremisesAddress = ZGuid.NewZGuid();
			code3.OK_RN_NKCodeCountry = CountryCodes.China;
			code3.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code3.OK_OA_PremisesAddress = ZGuid.Empty;

			AssertEquals(code1.OK_CodeType, Country.GetConsumptionTaxRegistrationOrgCusCode(code1.OK_RN_NKCodeCountry));
			AssertEquals(code1.OK_CodeType, Country.GetConsumptionTaxRegistrationOrgCusCode(code1.OK_RN_NKCodeCountry));
			AssertEquals(code1.OK_CodeType, Country.GetConsumptionTaxRegistrationOrgCusCode(code1.OK_RN_NKCodeCountry));
			AssertNoErrors(code1.OK_CodeTypeInfo);
			AssertHasError(code2.OK_CodeTypeInfo, "There can only be one code of this type for each country/region.");
			AssertHasError(code3.OK_CodeTypeInfo, "There can only be one code of this type for each country/region.");
		}

		public void TestValidateCargoWiseOneCarrierCodeIsUniquePerOrganisation()
			=> AssertCodeIsUniquePerOrganisation(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode,
				"Only one CargoWise Carrier Code could be issued per organization.");

		public void TestValidateDomesticCarrierCodeIsUniquePerOrganisation()
			=> AssertCodeIsUniquePerOrganisation(OrgCusCode.CodeTypes.DomesticCarrierCode,
				"Only one Domestic Carrier Code could be issued per organization.");

		public void TestValidateCargoWiseRoadTransportProviderCodeIsUniquePerOrganisation()
			=> AssertCodeIsUniquePerOrganisation(OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode,
				"Only one CargoWise Road Transport Provider code could be issued per organization.");

		public void TestValidateJNPCodeIsUniquePerOrganisation()
			=> AssertCodeIsUniquePerOrganisation(OrgCusCode.CodeTypes.JNP,
				"Only one JNP code can be issued per organization.");

		void AssertCodeIsUniquePerOrganisation(string codeType, string errorMessage)
		{
			org.CustomsCodes.RemoveAll();

			var country1 = Factory.New<RefCountry>();
			var country2 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, country1.Code));

			org.Addresses.AddNew();
			org.Addresses.AddNew();

			var code1 = AddNewCusCode(country1, codeType);
			code1.Validation.ValidateOK_CodeType();
			AssertNoErrors(code1.OK_CodeTypeInfo);

			var code2 = AddNewCusCode(country1, codeType);
			code2.Validation.ValidateOK_CodeType();
			AssertHasError(code2.OK_CodeTypeInfo, errorMessage);

			code2.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			AssertNoErrors(code2.OK_CodeTypeInfo);

			code2.OK_CodeType = codeType.ToLower();
			AssertHasError(code2.OK_CodeTypeInfo, errorMessage);

			code2.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			AssertNoErrors(code2.OK_CodeTypeInfo);

			var code3 = AddNewCusCode(country2, codeType);
			AssertHasError(code3.OK_CodeTypeInfo, errorMessage);

			code3.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			AssertNoErrors(code3.OK_CodeTypeInfo);

			var code4 = org.CustomsCodes.AddNew();
			code4.OK_CodeType = codeType;
			code4.OK_RN_NKCodeCountry = string.Empty;
			AssertHasError(code4.OK_CodeTypeInfo, errorMessage);

			code4.OK_CodeType = OrgCusCode.CodeTypes.ManifestProviderID;
			AssertNoErrors(code4.OK_CodeTypeInfo);
		}

		public void TestValidateDUNCodeIsUniquePerAddress()
		{
			org.CustomsCodes.RemoveAll();

			var errorMessage = "Only one DUN code can be issued per address.";

			var country1 = Factory.New<RefCountry>();
			country1.RN_Code = CountryCodes.Australia;
			var country2 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, country1.Code));

			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();

			var code1 = AddNewCusCode(country1, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			code1.OK_OA_PremisesAddress = address1.PK;
			AssertNoErrors(code1.OK_CodeTypeInfo);

			var code2 = AddNewCusCode(country1, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			code2.OK_OA_PremisesAddress = address1.PK;
			AssertHasError("Same country, same address", code2.OK_CodeTypeInfo, errorMessage);

			code2.OK_OA_PremisesAddress = address2.PK;
			AssertNoErrors("Same country, different address", code2.OK_CodeTypeInfo);

			code2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrors("Same country, no address", code2.OK_CodeTypeInfo);

			code1.OK_OA_PremisesAddress = ZGuid.Empty;
			code2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError("Same country, both with no address", code2.OK_CodeTypeInfo, errorMessage);

			code1.OK_OA_PremisesAddress = address1.PK;
			code2.OK_RN_NKCodeCountry = country2.Code;
			code2.OK_OA_PremisesAddress = address1.PK;
			AssertHasErrors("Different country, same address", code2.OK_CodeTypeInfo);

			code2.OK_RN_NKCodeCountry = country2.Code;
			code2.OK_OA_PremisesAddress = address2.PK;
			AssertNoErrors("Different country, different address", code2.OK_CodeTypeInfo);

			code2.OK_RN_NKCodeCountry = country2.Code;
			code2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrors("Different country, no address", code2.OK_CodeTypeInfo);

			code2.OK_RN_NKCodeCountry = country2.Code;
			code1.OK_OA_PremisesAddress = ZGuid.Empty;
			code2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasErrors("Different country, both with no address", code2.OK_CodeTypeInfo);
		}

		public void TestValidateTRICodeValueIsUniquePerAddress()
		{
			org.CustomsCodes.RemoveAll();

			var errorMessage1 = "TRI Code TRI111 can only be registered once.";
			var errorMessage2 = "Only one code of this type can be registered per Address.";

			var country1 = Factory.New<RefCountry>();
			country1.RN_Code = CountryCodes.Australia;

			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();

			var code1 = AddNewCusCode(country1, OrgCusCode.CodeTypes.BoleroTitleRegisterID);
			code1.OK_CustomsRegNo = "TRI111";
			code1.OK_OA_PremisesAddress = address1.PK;
			AssertNoErrors("Only one TRI", code1.OK_CodeTypeInfo);

			var code2 = AddNewCusCode(country1, OrgCusCode.CodeTypes.BoleroTitleRegisterID);
			code2.OK_CustomsRegNo = "TRI111";
			code2.OK_OA_PremisesAddress = address2.PK;
			AssertHasError("Same OK_CustomsRegNo. Differnt OK_OA_PremisesAddress(Not Empty)", code2.OK_CodeTypeInfo, errorMessage1);

			code1.OK_OA_PremisesAddress = ZGuid.Empty;
			code2.OK_OA_PremisesAddress = address2.PK;
			AssertHasError("Same OK_CustomsRegNo. Differnt OK_OA_PremisesAddress(One is Empty).", code2.OK_CodeTypeInfo, errorMessage1);

			code1.OK_OA_PremisesAddress = address1.PK;
			code2.OK_OA_PremisesAddress = address1.PK;
			AssertHasError("Same OK_CustomsRegNo. Same OK_OA_PremisesAddress(Not Empty)", code2.OK_CodeTypeInfo, errorMessage2);

			code1.OK_OA_PremisesAddress = ZGuid.Empty;
			code2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError("Same OK_CustomsRegNo. Same OK_OA_PremisesAddress(Empty).", code2.OK_CodeTypeInfo, errorMessage2);

			code2.OK_CustomsRegNo = "TR2222";
			code1.OK_OA_PremisesAddress = address1.PK;
			code2.OK_OA_PremisesAddress = address2.PK;
			AssertNoErrors("Different OK_CustomsRegNo. Different OK_OA_PremisesAddress(No Empty).", code2.OK_CodeTypeInfo);

			code1.OK_OA_PremisesAddress = address1.PK;
			code2.OK_OA_PremisesAddress = address1.PK;
			AssertHasError("Different OK_CustomsRegNo. Same OK_OA_PremisesAddress(Not Empty).", code2.OK_CodeTypeInfo, errorMessage2);

			code2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrors("Different OK_CustomsRegNo. Differnt OK_OA_PremisesAddress(One is Empty).", code2.OK_CodeTypeInfo);

			code1.OK_OA_PremisesAddress = ZGuid.Empty;
			code2.OK_OA_PremisesAddress = address2.PK;
			AssertNoErrors("Different OK_CustomsRegNo. Differnt OK_OA_PremisesAddress(One is Empty).", code2.OK_CodeTypeInfo);

			code1.OK_OA_PremisesAddress = ZGuid.Empty;
			code2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError("Different OK_CustomsRegNo. Same OK_OA_PremisesAddress(Empty).", code2.OK_CodeTypeInfo, errorMessage2);

			code2.OK_CustomsRegNo = "TRI111";
			code1.OK_OA_PremisesAddress = address1.PK;
			code2.OK_CustomsRegNo = "tri111";
			code2.OK_OA_PremisesAddress = address2.PK;
			AssertHasError("Same OK_CustomsRegNo (Ignore Case). Differnt OK_OA_PremisesAddress(Not Empty)", code2.OK_CodeTypeInfo, errorMessage1);

			code1.OK_OA_PremisesAddress = ZGuid.Empty;
			code2.OK_OA_PremisesAddress = address2.PK;
			AssertHasError("Same OK_CustomsRegNo (Ignore Case). Differnt OK_OA_PremisesAddress(One is Empty).", code2.OK_CodeTypeInfo, errorMessage1);

			code1.OK_OA_PremisesAddress = address1.PK;
			code2.OK_OA_PremisesAddress = address1.PK;
			AssertHasError("Same OK_CustomsRegNo (Ignore Case). Same OK_OA_PremisesAddress(Not Empty)", code2.OK_CodeTypeInfo, errorMessage2);

			code1.OK_OA_PremisesAddress = ZGuid.Empty;
			code2.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError("Same OK_CustomsRegNo (Ignore Case). Same OK_OA_PremisesAddress(Empty).", code2.OK_CodeTypeInfo, errorMessage2);
		}

		public void TestValidateCountryDefaultIsUniqueForCountry()
		{
			org.CustomsCodes.RemoveAll();

			var country1 = Factory.New<RefCountry>();
			var country2 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, country1.Code));

			var code1 = AddNewCusCode(country1, OrgCusCode.CodeTypes.GSTCode);
			code1.OK_CustomsRegNo = "1234";
			code1.OK_CountryDefault = true;

			var code2 = AddNewCusCode(country1, OrgCusCode.CodeTypes.ManifestProviderID);
			code2.OK_CustomsRegNo = "123456";
			code2.OK_CountryDefault = false;

			AssertNoErrors(code1.OK_CountryDefaultInfo);
			AssertNoErrors(code2.OK_CountryDefaultInfo);

			code2.OK_CountryDefault = true;
			AssertHasError(code2.OK_CountryDefaultInfo, "There can only be one default selected for each country/region.");

			code2.OK_RN_NKCodeCountry = country2.Code;
			AssertNoErrors(code2.OK_CountryDefaultInfo);
		}

		public void TestValidateCustomsCodeForIS()
		{
			cusCode.OK_CodeType = OrgCusCode.IcelandCodeTypes.CustomsOfficeCode;
			cusCode.OK_RN_NKCodeCountry = "IS";
			cusCode.OK_CustomsRegNo = ZString.Empty;
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Please enter a " + cusCode.OK_CustomsRegNoInfo.Description + ".");
			cusCode.OK_CustomsRegNo = "123df";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Only alpha symbols are allowed.");
			cusCode.OK_CustomsRegNo = "abc";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateCustomsCodeForDE()
		{
			var message = "VAT Business Registration Number structures for Germany are either 'DE999999999' or '999999999'.";

			cusCode.OK_RN_NKCodeCountry = CountryCodes.Germany;
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.UST;
			cusCode.OK_CustomsRegNo = "AS123456734";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "AS1234567";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);
			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);
		}

		public void TestValidateCustomsCodeForDEMaxLength()
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Germany;
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode;
			cusCode.OK_CustomsRegNo = "ThisRegistrationIsTooLong";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The maximum length is 10 characters.");
		}

		public void TestValidateAEOForJP()
		{
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
			cusCode.OK_RN_NKCodeCountry = "JP";
			cusCode.OK_CustomsRegNo = "111";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "AEO Code should consist of 17 characters");
			cusCode.OK_CustomsRegNo = "ABCD12345678901234";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "AEO Code should consist of 17 characters");
			cusCode.OK_CustomsRegNo = "ABCD1234567890123";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "AEO Code should consist of 17 characters");
		}

		public void TestValidateAEOForEU()
		{
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
			cusCode.OK_RN_NKCodeCountry = "GB";
			cusCode.OK_CustomsRegNo = "111";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "too short");
			cusCode.OK_CustomsRegNo = "GBABCD12345678901234";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "not start with the country/region code");
			cusCode.OK_CustomsRegNo = "ABCD1234567890";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Certificate must start with 'AEO'");
			cusCode.OK_CustomsRegNo = "XXXX11111111";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "(character 4) not recognized");
			cusCode.OK_CustomsRegNo = "AEOF123456789123456";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "too short");
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "not start with the country/region code");
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Certificate must start with 'AEO'");
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "(character 4) not recognized");
		}

		public void TestValidateCodeCountry_WhenCodeTypeIsAEO_ForEU()
		{
			var errorMessage = "Only one AEO in EU countries/regions should be entered per Organization.";
			var orgHeader = Factory.New<OrgHeader>();

			var cnAeo = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO_CN", CountryCodes.China);
			AssertNoErrorContaining("The rule is not applied to non-EU countries.", cnAeo.OK_CodeTypeInfo, errorMessage);

			var jpAeo = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO_JP", CountryCodes.Japan);
			AssertNoErrorContaining("The rule is not applied to non-EU countries.", jpAeo.OK_CodeTypeInfo, errorMessage);

			var itAeo = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO_IT", CountryCodes.Italy);
			AssertNoErrorContaining("There is only one EU country who has the AEO code, which is legal.", itAeo.OK_CodeTypeInfo, errorMessage);

			var frAeo = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO_FR", CountryCodes.France);
			AssertHasErrorContaining("There are two EU couuntries who have the AEO code, which is illegal.", frAeo.OK_CodeTypeInfo, errorMessage);
		}

		public void TestValidateCodeType_WhenCodeTypeIsAEO_ForEU()
		{
			var errorMessage = "EORI and AEO issuing countries/regions usually are the same, please check.";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", CountryCodes.UnitedKingdom);
			var aeo = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "002", CountryCodes.Italy);
			AssertHasWarningContaining(aeo.OK_RN_NKCodeCountryInfo, errorMessage);
			aeo.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			AssertNoWarningContaining(aeo.OK_RN_NKCodeCountryInfo, errorMessage);
		}

		public void TestValidateTENForEU()
		{
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
			cusCode.OK_RN_NKCodeCountry = "GB";
			cusCode.OK_CustomsRegNo = "111";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Number must start with a 2 character Country/Region Code");
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Number must have 11 alphanumeric characters after the 2 character Country/Region Code");
			cusCode.OK_CustomsRegNo = "GBABCD12345678901234";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Number must start with a 2 character Country/Region Code");
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Number must have 11 alphanumeric characters after the 2 character Country/Region Code");
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Number is too long. Must have 13 Characters");
			cusCode.OK_CustomsRegNo = "GBCD123456789";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Number must start with a 2 character Country/Region Code");
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Number must have 11 alphanumeric characters after the 2 character Country/Region Code");
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "Number is too long. Must have 13 Characters");
		}

		public void TestTcuinEoriAeoForTrustedCountries()
		{
			cusCode.OK_CustomsRegNo = "111";
			var countries = new Dictionary<string, Tuple<bool, bool, bool>>();  // EORI, TCUIN, AEO
			countries.Add(CountryCodes.Australia, new Tuple<bool, bool, bool>(false, false, true));
			countries.Add(CountryCodes.UnitedStates, new Tuple<bool, bool, bool>(false, true, true));
			countries.Add(CountryCodes.UnitedKingdom, new Tuple<bool, bool, bool>(true, false, true));
			countries.Add(CountryCodes.France, new Tuple<bool, bool, bool>(true, false, true));
			countries.Add(CountryCodes.Japan, new Tuple<bool, bool, bool>(false, true, true));
			countries.Add(CountryCodes.China, new Tuple<bool, bool, bool>(false, true, true));
			countries.Add(CountryCodes.Canada, new Tuple<bool, bool, bool>(false, true, true));
			countries.Add(CountryCodes.Norway, new Tuple<bool, bool, bool>(true, true, true));
			countries.Add(CountryCodes.Switzerland, new Tuple<bool, bool, bool>(true, true, true));
			countries.Add(CountryCodes.SanMarino, new Tuple<bool, bool, bool>(true, true, true));
			countries.Add(CountryCodes.Andorra, new Tuple<bool, bool, bool>(true, true, true));
			countries.Add(CountryCodes.Liechtenstein, new Tuple<bool, bool, bool>(true, true, true));
			countries.Add(CountryCodes.Vatican, new Tuple<bool, bool, bool>(true, true, true));
			CombineAssertions(delegate
			{
				foreach (var pair in countries)
				{
					var countryOfIssue = pair.Key;
					RunEoriTcuinAeoTest(pair.Value, countryOfIssue);
				}
			}
			);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "15");
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "TCUIN");
			cusCode.OK_CustomsRegNo = "123456789012345";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "15");
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "TCUIN");
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.AccountsPayableSuppliersReference;  // anything
			cusCode.OK_CustomsRegNo = "x";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "TCUIN");
		}

		public void TestValidateActivityCodeForIT()
		{
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceAttive;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode.OK_CustomsRegNo = "111";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "The CAT registration code must be a six digits numeric code (NNNNNN).");
			cusCode.OK_CustomsRegNo = "1234567";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "The CAT registration code must be a six digits numeric code (NNNNNN).");
			cusCode.OK_CustomsRegNo = "12345b";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "The CAT registration code must be a six digits numeric code (NNNNNN).");
			cusCode.OK_CustomsRegNo = "654321";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, "The CAT registration code must be a six digits numeric code (NNNNNN).");

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			var expectedError = "The IVA registration code needs to be 13 or 11 in length.";
			cusCode.OK_CustomsRegNo = "IT008912301536";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			cusCode.OK_CustomsRegNo = "008912301536";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			expectedError = @"The IVA registration code pattern is invalid.
Valid patterns are:
	ITnnnnnnnnnnn
	nnnnnnnnnnn";
			cusCode.OK_CustomsRegNo = "IT0089123015A";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "0089123015A";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			expectedError = "The check digit in the IVA registration code is incorrect.";
			cusCode.OK_CustomsRegNo = "IT00891230154";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "00891230152";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "IT00891230153";
			AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "00891230153";
			AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			expectedError = "The COD registration code needs to be 16 or 11 in length.";
			cusCode.OK_CustomsRegNo = "008912301536";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "AAAAAA12A45A678AA";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			expectedError = @"The COD registration code pattern is invalid.
Valid patterns are:
	AAAAAAnnAnnAnnnA
	nnnnnnnnnnn";
			cusCode.OK_CustomsRegNo = "0089123015A";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "AAAAAA12A45A6787";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			expectedError = "The check digit in the COD registration code is incorrect.";
			cusCode.OK_CustomsRegNo = "00891230154";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "00891230153";
			AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "AAAAAA12A45A678A";
			AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateVGMForHK()
		{
			var error = "VGM Registration number for HK should start with \"GMV\" following by a set of numbers.";

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VGMRegistrationNumber;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.HongKong;
			cusCode.OK_CustomsRegNo = "111";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, error);
			cusCode.OK_CustomsRegNo = "GMV";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, error);
			cusCode.OK_CustomsRegNo = "GMV123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, error);
			cusCode.OK_CustomsRegNo = "GMV123456789";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, error);
		}

		// 4 alphanumeric
		public void TestValidateCACarrierCode()
		{
			org.OH_IsShippingProvider = true;

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Canada;
			cusCode.OK_CustomsRegNo = "061234";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.CarrierCodeRightFormat);

			cusCode.OK_CustomsRegNo = "BLAH";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.CarrierCodeRightFormat);
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.CarrierCodeRightFormat);

			cusCode.OK_CustomsRegNo = "BLA";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.CarrierCodeRightFormat);

			cusCode.OK_CustomsRegNo = "014-";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.CarrierCodeRightFormat);
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.CarrierCodeRightFormat);
		}

		//AANNNNN
		public void TestValidateCAAuthorizationID()
		{
			cusCode.OK_CodeType = OrgCusCode.CACodeTypes.AuthorizationID;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Canada;
			cusCode.OK_CustomsRegNo = "061234";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat);

			cusCode.OK_CustomsRegNo = "AS1234";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat);

			cusCode.OK_CustomsRegNo = "AS12A4";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.AuthorizationIDRightFormat);
		}

		//AAAAAA
		public void TestValidateCAExportLicenceNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.CACodeTypes.ExportLicenceNumber;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Canada;
			cusCode.OK_CustomsRegNo = "1234567";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.ExportLicenceNumberCodeRightFormat);

			cusCode.OK_CustomsRegNo = "12345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.ExportLicenceNumberCodeRightFormat);

			cusCode.OK_CustomsRegNo = "123456";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.ExportLicenceNumberCodeRightFormat);

			cusCode.OK_CustomsRegNo = "ABCDEF";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.ExportLicenceNumberCodeRightFormat);
		}

		//NNNNNNNNNRTNNNM
		public void TestValidateBusinessNumberForGoodsServicesHarmonizedSalesTax()
		{
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, "RT", "RC", CanadianCustomsCodeValidator.Constants.BusinessNumberForGoodsServicesHarmonizedSalesTaxFormat);
		}

		//NNNNNNNNNRCNNNM
		public void TestValidateBusinessNumberForCorporateIncomeTax()
		{
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax, "RC", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberForCorporateIncomeTaxFormat);
		}

		//NNNNNNNNNRMNNNM
		public void TestValidateBusinessNumberForImportExport()
		{
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat, true);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat, false);
		}

		//NNNNNNNNNRMNNNM
		public void TestValidateBusinessNumberCustomsBroker()
		{
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberCustomsBrokerFormat, true);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberCustomsBrokerFormat, false);
		}

		//NNNNNNNNNRMNNNM
		public void TestValidateBusinessNumberImporterNonCommercial()
		{
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberImporterNonCommercialFormat, true);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberImporterNonCommercialFormat, false);
		}

		//NNNNNNNNNRMNNNM
		public void TestValidateBusinessNumberForExport()
		{
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForExport, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat, true);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForExport, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberForExportFormat, false);
		}

		//NNNNNNNNNRMNNNM
		public void TestValidateBusinessNumberForLowValueShipments()
		{
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberForLowValueShipmentsFormat, true);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, "RM", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberForLowValueShipmentsFormat, false);
		}

		//NNNNNNNNNRPNNNM
		public void TestValidateBusinessNumberForPayrollDeductions()
		{
			AssertBusinessNumberForCA(OrgCusCode.CACodeTypes.BusinessNumberForPayrollDeductions, "RP", "RT", CanadianCustomsCodeValidator.Constants.BusinessNumberForPayrollDeductionsRightFormat);
		}

		// 9 alphanumeric
		public void TestValidateCAImporterNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Canada;
			cusCode.OK_CustomsRegNo = "123456";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);

			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);

			cusCode.OK_CustomsRegNo = "123x5678A";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);

			cusCode.OK_CustomsRegNo = "12345678A9";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);
		}

		public void TestValidateCAWorldManufacturerIdentifier()
		{
			cusCode.OK_CodeType = OrgCusCode.CACodeTypes.WorldManufacturerIdentifier;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Canada;

			cusCode.OK_CustomsRegNo = "A2R3456";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.WorldManufacturerIdentifierFormat);

			cusCode.OK_CustomsRegNo = "A2R3";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.WorldManufacturerIdentifierFormat);

			cusCode.OK_CustomsRegNo = "A2R";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, CanadianCustomsCodeValidator.Constants.WorldManufacturerIdentifierFormat);
		}

		// 3-4 alphanumeric
		public void TestValidateJPCarrierCode()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Japan;
			cusCode.OK_CustomsRegNo = "061234";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The Carrier Code should be 3 or 4 Alphanumeric Characters");

			cusCode.OK_CustomsRegNo = "12";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The Carrier Code should be 3 or 4 Alphanumeric Characters");

			cusCode.OK_CustomsRegNo = "A_12";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The Carrier Code should be 3 or 4 Alphanumeric Characters");

			cusCode.OK_CustomsRegNo = "A12";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "AB12";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);
		}

		// 5 alphanumeric
		public void TestValidateJPCustomsClientCode()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Japan;
			cusCode.OK_CustomsRegNo = "061234";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The Customs Client Code should be 5 Alphanumeric Characters");

			cusCode.OK_CustomsRegNo = "BLAH";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The Customs Client Code should be 5 Alphanumeric Characters");

			cusCode.OK_CustomsRegNo = "JJ_7G";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The Customs Client Code should be 5 Alphanumeric Characters");

			cusCode.OK_CustomsRegNo = "JJ07G";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestTirIdentificationNumberFormatValidation()
		{
			var tirInvalidFormatMessage = "Invalid TIR Carnet holder ID number format";

			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers;
			cusCode.OK_CustomsRegNo = "123456789000";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "123/123/123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "12/123/123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "ZZ1/123/123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "ZZZZ/123/123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "AUT/ZZZ/123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "AUT/ZZ1/123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "AUT/1234/123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "AUT/12/123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "AUT/123/123/456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "AUT/123123456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "AUT/123/Z23456";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
			cusCode.OK_CustomsRegNo = "AUT/123/123456";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, tirInvalidFormatMessage);
		}

		public void TestDODCodeType_IsValidForUSA()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "ABCDEF";

			Factory.Save();

			AssertNoErrors(cusCode.OK_CodeTypeInfo);
		}

		public void TestDODCodeType_IsInvalidForCountriesOtherThanUSA()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CustomsRegNo = "ABCDEF";

			Factory.Save();

			AssertHasError(cusCode.OK_CodeTypeInfo, "Enter a valid Type.");
		}

		public void TestDODCodeType_IsValidWithSixAlphaNumericChars()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "ABCDEF";

			Factory.Save();

			AssertNoErrors(cusCode.OK_CodeTypeInfo);
		}

		public void TestDODCodeType_IsInvalidWithMoreThanSixAlphaNumericChars()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "ABCDEFG";

			Factory.Save();

			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Invalid code entered. Department of Defense Activity Address Code must be 6 alpha-numeric characters.");
		}

		public void TestDODCodeType_IsInvalidWithLessThanSixAlphaNumericChars()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "ABCDE";

			Factory.Save();

			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Invalid code entered. Department of Defense Activity Address Code must be 6 alpha-numeric characters.");
		}

		public void TestDODCodeType_IsInvalidWithNonAlphaNumericChars()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "ABCDE!";

			Factory.Save();

			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Invalid code entered. Department of Defense Activity Address Code must be 6 alpha-numeric characters.");
		}

		public void TestDODCodeType_IsValidWithoutPremisesAddress()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();

			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "ABCDEF";

			Factory.Save();

			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestDODCodeType_IsValidWithPremisesAddress()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newAddress = newOrg.Addresses.AddNew();
			newAddress.OA_Address1 = "1 Test St";

			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode.OK_CustomsRegNo = "ABCDEF";
			cusCode.OK_OA_PremisesAddress = newAddress.PK;

			Factory.Save();

			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestDODCodeType_IsValidWithMultipleDODCodes()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();

			var newAddress1 = newOrg.Addresses.AddNew();
			newAddress1.OA_Address1 = "1 Test St";

			var newAddress2 = newOrg.Addresses.AddNew();
			newAddress2.OA_Address1 = "2 Test St";

			var cusCode1 = newOrg.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode1.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode1.OK_CustomsRegNo = "ABCDEF";
			cusCode1.OK_OA_PremisesAddress = newAddress1.PK;

			var cusCode2 = newOrg.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.USACodeTypes.DepartmentOfDefenseActivityAddressCode;
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			cusCode2.OK_CustomsRegNo = "GHIJLK";
			cusCode2.OK_OA_PremisesAddress = newAddress2.PK;

			Factory.Save();

			AssertNoErrors(cusCode1.OK_OA_PremisesAddressInfo);
			AssertNoErrors(cusCode2.OK_OA_PremisesAddressInfo);
		}

		public void TestPECCodeType_IsValidForItaly()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode.OK_CustomsRegNo = "a@b.com";

			Factory.Save();

			AssertNoErrors(cusCode.OK_CodeTypeInfo);
		}

		public void TestPECCodeType_IsInvalidForCountriesOtherThanItaly()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Australia;
			cusCode.OK_CustomsRegNo = "a@b.com";

			Factory.Save();

			AssertHasError(cusCode.OK_CodeTypeInfo, "Enter a valid Type.");
		}

		public void TestPECCodeType_IsInvalidWhenRegNoIsNotAValidEmail()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode.OK_CustomsRegNo = "invalid.email";

			Factory.Save();

			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Email Address is not valid .");
		}

		public void TestPECCodeType_IsValidWhenRegNoIsAValidEmail()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode.OK_CustomsRegNo = "a@b.com";

			Factory.Save();

			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestPECCodeType_IsValidWhenRegNoHasUnderscore()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode.OK_CustomsRegNo = "a@b_1.com";

			Factory.Save();

			AssertHasErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.PEC;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Turkey;
			cusCode.OK_CustomsRegNo = "a@b_1.com";

			Factory.Save();

			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestPECCodeType_IsValidWithoutPremisesAddress()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();

			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode.OK_CustomsRegNo = "a@b.com";

			Factory.Save();

			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestPECCodeType_IsValidWithPremisesAddress()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newAddress = newOrg.Addresses.AddNew();
			newAddress.OA_Address1 = "1 Test St";

			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode.OK_CustomsRegNo = "a@b.com";
			cusCode.OK_OA_PremisesAddress = newAddress.PK;

			Factory.Save();

			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestPECCodeType_IsValidForTurkey()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime()))
			{
				var cusCode = CreateAndGetOrgCusCodePEC(CountryCodes.Turkey, "a@b.com");

				AssertNoErrors(cusCode.OK_CodeTypeInfo);
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);

				cusCode.OK_CustomsRegNo = "invalid.email";
				Factory.Save();

				AssertHasError(cusCode.OK_CustomsRegNoInfo, "PEC Registration Number is not valid. It should be in a valid Email format.");
			}
		}

		public void TestPECCodeType_IsValidForTurkeyFromDifferentCountry()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime()))
			{
				var cusCode = CreateAndGetOrgCusCodePEC(CountryCodes.Turkey, "a@b.com");

				AssertNoErrors(cusCode.OK_CodeTypeInfo);
				AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
			}
		}

		public void TestPECCodeType_IsInvalidForCountriesOtherThanTurkey()
		{
			var cusCode = CreateAndGetOrgCusCodePEC(CountryCodes.Australia, "a@b.com");

			AssertHasError(cusCode.OK_CodeTypeInfo, "Enter a valid Type.");
		}

		public void TestPECCodeType_IsValidWithPremisesAddressForTurkey()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newAddress = newOrg.Addresses.AddNew();
			newAddress.OA_Address1 = "1 Test St";

			var cusCode = CreateAndGetOrgCusCodePEC(CountryCodes.Turkey, "a@b.com");
			cusCode.OK_OA_PremisesAddress = newAddress.PK;

			Factory.Save();

			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestValidateCarrierShippingLine()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			orgCusCode.OK_CustomsRegNo = "aaa";
			AssertNoWarnings(orgCusCode.OK_CustomsRegNoInfo);
			AssertNoErrors(orgCusCode.OK_CustomsRegNoInfo);

			orgCusCode.OK_RN_NKCodeCountry = "US";
			orgCusCode.OK_CustomsRegNo = "aaa";
			AssertHasWarning(orgCusCode.OK_CustomsRegNoInfo, "This is not a valid SCAC. Refer to Shipping Lines under Reference Files for a list of valid codes.");

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "bbb";
			shippingLine.RSL_CargoWiseOneCode = "c1ab";
			shippingLine.RSL_CarrierName = "TEST";

			orgCusCode.OK_CustomsRegNo = "bbb";

			AssertNoWarnings(orgCusCode.OK_CustomsRegNoInfo);

			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_RN_NKCodeCountry = "US";
			orgCusCode.OK_CustomsRegNo = "aaa";
			AssertHasError(orgCusCode.OK_CustomsRegNoInfo, "SCAC must be 4 character length.");

			orgCusCode.OK_CustomsRegNo = "ABCD";
			AssertNoErrors(orgCusCode.OK_CustomsRegNoInfo);

			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
			orgCusCode.OK_CustomsRegNo = "aaa";
			AssertNoErrors(orgCusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateCargoWiseOneShippingLine()
		{
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			orgCusCode.OK_CustomsRegNo = "c1^#";

			AssertHasWarning(orgCusCode.OK_CustomsRegNoInfo, "This is not a valid SCAC. Refer to Shipping Lines under Reference Files for a list of valid codes.");

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = ZString.Empty;
			shippingLine.RSL_CargoWiseOneCode = "c1ab";
			shippingLine.RSL_CarrierName = "TEST";

			orgCusCode.OK_CustomsRegNo = "c1ab";

			AssertHasWarning(orgCusCode.OK_CustomsRegNoInfo, "This SCAC does not match the chosen C1C code. Please delete or correct this entry.");

			shippingLine.RSL_StandardCarrierAlphaCode = "1234";

			orgCusCode.OK_CustomsRegNo = "c1ab";

			AssertNoWarnings(orgCusCode.OK_CustomsRegNoInfo);
		}

		public void TestCAGCodeType_WhenNKCodeCountryNotIssuingCAG_CodeTypeShouldShowInvalidError()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var nonCAGIssuingCountry = CountryCodes.AlandIslands;
			var cusCode = AddNewCusCode(codeType: codeTypeCAG, countryCode: nonCAGIssuingCountry);
			AssertHasError("OK_CodeTypeInfo should show error for Non-CAG issuing country", cusCode.OK_CodeTypeInfo, "Enter a valid Type.");
		}

		public void TestCAGCodeType_WhenNKCodeCountryIsIssuingCAG_CodeTypeShouldNotShowErrors()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cagIssuingCountry = CountryCodes.Australia;
			var cusCode = AddNewCusCode(codeType: codeTypeCAG, countryCode: cagIssuingCountry);
			AssertNoErrors(cusCode.OK_CodeTypeInfo);
		}

		public void TestCAGCodeType_WhenNKCodeCountryNotExist_NKCodeCountryShouldShowInvalidError()
		{
			var cusCode = AddNewCusCode(countryCode: "XX", codeType: OrgCusCode.CodeTypes.CommercialAndGovernmentEntity);
			AssertHasError("OK_RN_NKCodeCountryInfo should show error for non existing country", cusCode.OK_RN_NKCodeCountryInfo, "Enter a valid Country/Region Of Issue.");
		}

		public void TestCAGCodeType_WhenCAGCusCodeNoIsInvalid_RegCodeInfoShouldShowInvalidError()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var invalidCAGCusCodes = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "ZABCD#", countryCode: CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "ZAB#", countryCode: CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "XABC#", countryCode: CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "ZABCX", countryCode: CountryCodes.UnitedStates)
			};
			CombineAssertions(() =>
			{
				foreach (var cusCode in invalidCAGCusCodes)
				{
					AssertHasError("When CAG Cus Code is invalid RegCodeInfo should show error", cusCode.OK_CustomsRegNoInfo, "Commercial And Government Entity Code is not valid.");
				}
			});
		}

		public void TestCAGCodeType_WhenCAGCusCodeNoIsValid_RegCodeShouldNotShowErrors()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCode = AddNewCusCode(codeType: codeTypeCAG, code: "ZABC#", countryCode: CountryCodes.Australia);
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCAGCodeIssuer_AssertCAGCodeIsNotValid_WhenCusCodesCAGWithIncorrectFirstChar()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithIncorrectFirstChar = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCQ", countryCode: CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCH", countryCode: CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCN", countryCode: CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCU", countryCode: CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCK", countryCode: CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCV", countryCode: CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCU", countryCode: CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "OABC#", countryCode: CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCA", countryCode: CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCZ", countryCode: CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCB", countryCode: CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCG", countryCode: CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD", countryCode: CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCJ", countryCode: CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCS", countryCode: CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCG", countryCode: CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCR", countryCode: CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCV", countryCode: CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCY", countryCode: CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCZ", countryCode: CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCA", countryCode: CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCX", countryCode: CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCF", countryCode: CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCK", countryCode: CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD", countryCode: CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCR", countryCode: CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCC", countryCode: CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCW", countryCode: CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCM", countryCode: CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCE", countryCode: CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCT", countryCode: CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCP", countryCode: CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCY", countryCode: CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCP", countryCode: CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCH", countryCode: CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCL", countryCode: CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCF", countryCode: CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCE", countryCode: CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCS", countryCode: CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCM", countryCode: CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCQ", countryCode: CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCB", countryCode: CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCN", countryCode: CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCC", countryCode: CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCT", countryCode: CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCJ", countryCode: CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCW", countryCode: CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: "")
			};

			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithIncorrectFirstChar)
				{
					AssertHasError($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be invalid when CAG has less than 5 chars", cusCode.OK_CustomsRegNoInfo, "Commercial And Government Entity Code is not valid.");
				}
			});
		}

		public void TestCAGCodeIssuer_AssertCAGCodeIsNotValid_WhenCusCodesCAGHasIncorrectLastChar()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithIncorrectLastChar = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "WABCI", countryCode: CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "ZABCI", countryCode: CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "BABCI", countryCode: CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "RABCI", countryCode: CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "FABCI", countryCode: CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "CABCI", countryCode: CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "GABCI", countryCode: CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "SABCI", countryCode: CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "JABCI", countryCode: CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "BABCI", countryCode: CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "YABCI", countryCode: CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "HABCI", countryCode: CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "EABCI", countryCode: CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "NABCI", countryCode: CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "PABCI", countryCode: CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "QABCI", countryCode: CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "VABCI", countryCode: CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "TABCI", countryCode: CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "UABCI", countryCode: CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: "")
			};
			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithIncorrectLastChar)
				{
					AssertHasError($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be invalid when CAG has less than 5 chars", cusCode.OK_CustomsRegNoInfo, "Commercial And Government Entity Code is not valid.");
				}
			});
		}

		public void TestCAGCodeType_AssertCAGCodeIsNotValid_WhenCustomsRegNoHasLessThan5Chars()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithLessThan5Chars = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "AABQ", countryCode: CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABH", countryCode: CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "WAB#", countryCode: CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "ZAB#", countryCode: CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABN", countryCode: CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "BAB#", countryCode: CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABU", countryCode: CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABK", countryCode: CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABV", countryCode: CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABU", countryCode: CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "#AB#", countryCode: CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABA", countryCode: CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABZ", countryCode: CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABB", countryCode: CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABG", countryCode: CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "RAB#", countryCode: CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABD", countryCode: CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABJ", countryCode: CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABS", countryCode: CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABG", countryCode: CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "FAB#", countryCode: CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABR", countryCode: CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "CAB#", countryCode: CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "GAB#", countryCode: CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABV", countryCode: CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "SAB#", countryCode: CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABY", countryCode: CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABZ", countryCode: CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABA", countryCode: CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "AAB#", countryCode: CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "JAB#", countryCode: CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABX", countryCode: CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABF", countryCode: CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABK", countryCode: CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABD", countryCode: CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABR", countryCode: CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "BAB#", countryCode: CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABC", countryCode: CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "YAB#", countryCode: CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABW", countryCode: CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABM", countryCode: CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "HAB#", countryCode: CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "EAB#", countryCode: CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "NAB#", countryCode: CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABE", countryCode: CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABT", countryCode: CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABP", countryCode: CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABY", countryCode: CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABP", countryCode: CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABH", countryCode: CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "PAB#", countryCode: CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABL", countryCode: CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABF", countryCode: CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABE", countryCode: CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABS", countryCode: CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "QAB#", countryCode: CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABM", countryCode: CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABQ", countryCode: CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "VAB#", countryCode: CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABB", countryCode: CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABN", countryCode: CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABC", countryCode: CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABT", countryCode: CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "TAB#", countryCode: CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABJ", countryCode: CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABW", countryCode: CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "UAB#", countryCode: CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "#AB#", countryCode: CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "IAB#", countryCode: ""),
			};
			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithLessThan5Chars)
				{
					AssertHasError($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be invalid when CAG has less than 5 chars", cusCode.OK_CustomsRegNoInfo, "Commercial And Government Entity Code is not valid.");
				}
			});
		}

		public void TestCAGCodeIssuer_AssertCAGCodeIsNotValid_WhenCustomsRegNoHasMoreThan5Chars()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithIncorrectFirstChar = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDQ",countryCode: CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDH",countryCode: CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDN",countryCode: CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDU",countryCode: CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDK",countryCode: CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDV",countryCode: CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDU",countryCode: CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDA",countryCode: CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDZ",countryCode: CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDB",countryCode: CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDG",countryCode: CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDD",countryCode: CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDJ",countryCode: CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDS",countryCode: CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDG",countryCode: CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDR",countryCode: CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDV",countryCode: CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDY",countryCode: CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDZ",countryCode: CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDA",countryCode: CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDX",countryCode: CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDF",countryCode: CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDK",countryCode: CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDD",countryCode: CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDR",countryCode: CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDC",countryCode: CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDW",countryCode: CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDM",countryCode: CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDE",countryCode: CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDT",countryCode: CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDP",countryCode: CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDY",countryCode: CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDP",countryCode: CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDH",countryCode: CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDL",countryCode: CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDF",countryCode: CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDE",countryCode: CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDS",countryCode: CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDM",countryCode: CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDQ",countryCode: CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDB",countryCode: CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDN",countryCode: CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDC",countryCode: CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDT",countryCode: CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDJ",countryCode: CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDW",countryCode: CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "IABCD#", countryCode: "")
			};
			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithIncorrectFirstChar)
				{
					AssertHasError($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be invalid when CAG has less than 5 chars", cusCode.OK_CustomsRegNoInfo, "Commercial And Government Entity Code is not valid.");
				}
			});
		}

		public void TestCAGCodeType_NoExceptionThrown_WhenCountryIsNotCAGCodeIssuer()
		{
			var cusCode = AddNewCusCode(string.Empty, "IABC#", OrgCusCode.CodeTypes.CommercialAndGovernmentEntity);
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			AssertNoExceptionThrown("No exception throw when country code not exist in dictionary CAGValidationRulesByCountry", () =>
			{
				cusCode.OK_RN_NKCodeCountry = CountryCodes.China;
			});
		}

		public void TestEoriMaximumForEU()
		{
			var eoriLengthMsg = "Maximum Length of the EORI code is 15, this excludes the country/region code prefix which is automatically added.";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "1234567890123456";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, eoriLengthMsg);
			cusCode.OK_CustomsRegNo = "Z12345678901234";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, eoriLengthMsg);
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Germany;
			cusCode.OK_CustomsRegNo = "1234567890123456";
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, eoriLengthMsg);
			cusCode.OK_CustomsRegNo = "Z12345678901234";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, eoriLengthMsg);
		}

		public void TestIVACodeTypeForPTCannotBeChangedOnceATranasctionIsPosted()
		{
			AssertCusCodePropertyValidationForPT(
				(cusCode) =>
				{
					cusCode.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
					AssertNoErrors(cusCode.OK_CodeTypeInfo);
				},
				(cusCode) =>
				{
					cusCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
					AssertNoErrors(cusCode.OK_CodeTypeInfo);
				},
				(cusCode) =>
				{
					cusCode.OK_CodeType = OrgCusCode.CodeTypes.DriverLicenceID;
					AssertNoErrors(cusCode.OK_CodeTypeInfo);
				},
				(cusCode) =>
				{
					//After Clearing Cache
					AssertHasError(cusCode.OK_CodeTypeInfo, "You cannot edit this code type.At least one transaction has been posted in a Portugal Login Company in this database using this Organization.");
				});
		}

		public void TestCusCountryForPTIVACannotBeChangedOnceATranasctionIsPosted()
		{
			AssertCusCodePropertyValidationForPT(
				(cusCode) =>
				{
					cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
					AssertNoErrors(cusCode.OK_RN_NKCodeCountryInfo);
				},
				(cusCode) =>
				{
					cusCode.OK_RN_NKCodeCountry = CountryCodes.Portugal;
					AssertNoErrors(cusCode.OK_RN_NKCodeCountryInfo);
				},
				(cusCode) =>
				{
					cusCode.OK_RN_NKCodeCountry = CountryCodes.Spain;
					AssertNoErrors(cusCode.OK_RN_NKCodeCountryInfo);
				},
				(cusCode) =>
				{
					//After Clearing Cache
					AssertHasError(cusCode.OK_RN_NKCodeCountryInfo, "You cannot edit this country/region.At least one transaction has been posted in a Portugal Login Company in this database using this Organization.");
				});
		}

		public void TestPTIVANoCannotBeChangedOnceATranasctionIsPosted()
		{
			AssertCusCodePropertyValidationForPT(
				(cusCode) =>
				{
					cusCode.OK_CustomsRegNo = "PT123456789";
					AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				},
				(cusCode) =>
				{
					cusCode.OK_CustomsRegNo = "012345679";
					AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				},
				(cusCode) =>
				{
					cusCode.OK_CustomsRegNo = "789012340";
					AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
				},
				(cusCode) =>
				{
					//After Clearing Cache
					AssertHasError(cusCode.OK_CustomsRegNoInfo, "You cannot edit this registration number. At least one transaction has been posted in a Portugal Login Company in this database using this Organization.");
				});
		}

		public void TestValidationREX()
		{
			var msg = "REX code is composed by Country/Region Code + REX + Identification number, max total length of 35 uppercase characters";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Ireland;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber;

			cusCode.OK_CustomsRegNo = "IEREX12345AB";
			AssertNoWarningContaining("Case 1: valid REX nummber", cusCode.OK_CustomsRegNoInfo, msg);

			cusCode.OK_CustomsRegNo = "IEREX12345ab";
			AssertHasWarningContaining("Case 2: invalid REX nummber - lowercase chars", cusCode.OK_CustomsRegNoInfo, msg);

			cusCode.OK_CustomsRegNo = "ITREX12345";
			AssertHasWarningContaining("Case 3: invalid REX nummber - wrong country code", cusCode.OK_CustomsRegNoInfo, msg);

			cusCode.OK_CustomsRegNo = "IEABCDJA";
			AssertHasWarningContaining("Case 4: invalid REX nummber - REX part missing ", cusCode.OK_CustomsRegNoInfo, msg);

			cusCode.OK_CustomsRegNo = "IEREX123456789012345678901234567890";
			AssertNoWarningContaining("Case 5: valid REX nummber - 35 chars length", cusCode.OK_CustomsRegNoInfo, msg);

			cusCode.OK_CustomsRegNo = "IEREX1234567890123456789012345678901";
			AssertHasWarningContaining("Case 6: valid REX nummber - 36 chars length", cusCode.OK_CustomsRegNoInfo, msg);
		}

		public void TestValidationMMRAndSMR()
		{
			var message = "The maximum length of Meat Or Seafood Manufacturer Registration Number is 18.";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.China;
			var targetInfo = cusCode.OK_CustomsRegNoInfo;

			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.MMR;
			cusCode.OK_CustomsRegNo = "MMR123456789012345";
			AssertNoErrorContaining("MMR, 18 chars, should NOT have error of overlength.", targetInfo, message);
			cusCode.OK_CustomsRegNo += "X";
			AssertHasError("MMR, 19 chars, should HAVE error of MMR overlength.", targetInfo, message);

			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.SMR;
			cusCode.OK_CustomsRegNo = "MMR123456789012345";
			AssertNoErrorContaining("SMR, 18 chars, should NOT have error of overlength.", targetInfo, message);
			cusCode.OK_CustomsRegNo += "X";
			AssertHasError("MSMRMR, 19 chars, should HAVE error of MMR overlength.", targetInfo, message);

			cusCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.AEO;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoErrorContaining("None MMR or SMR, 19 chars, should NOT have error of overlength.", targetInfo, message);
		}

		public void TestValidateEoriCode()
		{
			var eoriCodePrefixWarning = "The EORI code does not need to start with the country/region code, this prefix will be added automatically when needed for messaging.";
			var allCountries = new RefCountryCollection(Factory);
			var eoriCountries = allCountries.Where(country =>
			{
				cusCode.OK_RN_NKCodeCountry = country.Code;
				return cusCode.Lookups.OK_CodeType_List.ContainsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			});

			var minimumEoriCountryCodes = new List<string>()
			{
				CountryCodes.Austria,
				CountryCodes.Belgium,
				CountryCodes.Bulgaria,
				CountryCodes.CzechRepublic,
				CountryCodes.Estonia,
				CountryCodes.Greece,
				CountryCodes.Finland,
				CountryCodes.Croatia,
				CountryCodes.Ireland,
				CountryCodes.Lithuania,
				CountryCodes.Luxembourg,
				CountryCodes.Latvia,
				CountryCodes.Malta,
				CountryCodes.Romania,
				CountryCodes.Sweden,
				CountryCodes.Slovenia,
				CountryCodes.Slovakia
			};

			foreach (var countryCode in minimumEoriCountryCodes)
			{
				Assert($"Precondition: {countryCode} should support EORI", eoriCountries.Any(country => country.Code == countryCode));
			}

			CombineAssertions(() =>
			{
				foreach (var country in eoriCountries)
				{
					cusCode.OK_RN_NKCodeCountry = country.Code;
					cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
					cusCode.OK_CustomsRegNo = $"{country.Code}123456789000";
					AssertHasMessageErrorContaining($"{country.Code} - {country.Description}", cusCode.OK_CustomsRegNoInfo, eoriCodePrefixWarning);
				}
			});
		}

		OrgCusCode cusCode;
		OrgHeader org;

		protected override void SetUp()
		{
			base.SetUp();
			org = Factory.NewWithValidTestData<OrgHeader>();
			cusCode = org.CustomsCodes.AddNew();
		}

		void AssertBusinessNumberForCA(string codeType, string validType, string invalidType, string message)
		{
			AssertBusinessNumberForCA(codeType, validType, invalidType, message, false);
		}

		void AssertBusinessNumberForCA(string codeType, string validType, string invalidType, string message, bool strictEnforcement)
		{
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Canada;

			if (strictEnforcement)
			{
				cusCode.OK_CodeType = codeType;
				cusCode.OK_CustomsRegNo = "123121234" + invalidType + "0001";
				AssertHasError(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "123121234" + validType + "0001";
				AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);
				AssertNoError(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "12234";
				AssertHasError(cusCode.OK_CustomsRegNoInfo, message);
				AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "123121234" + validType + "00012";
				AssertHasError(cusCode.OK_CustomsRegNoInfo, message);
				AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "1A3121234" + validType + "0001";
				AssertHasError(cusCode.OK_CustomsRegNoInfo, message);
				AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "123121234" + validType + "0A01";
				AssertHasError(cusCode.OK_CustomsRegNoInfo, message);
				AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);
			}
			else
			{
				cusCode.OK_CodeType = codeType;
				cusCode.OK_CustomsRegNo = "123121234" + invalidType + "0001";
				AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "123121234" + validType + "0001";
				AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "12234";
				AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "123121234" + validType + "00012";
				AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "1A3121234" + validType + "0001";
				AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

				cusCode.OK_CustomsRegNo = "123121234" + validType + "0A01";
				AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);
			}
		}

		void CreateARInvoiceWithLine(OrgHeader org, AccChargeCode chargeCode)
		{
			var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice.AH_OH = org.PK;

			var arInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine.AL_AH = arInvoice.PK;
			arInvoiceLine.AL_AT = ZGuid.Empty;
			arInvoiceLine.AL_AC = chargeCode.PK;
			arInvoiceLine.AL_LineAmount = 44;

			Factory.Save();
		}

		AccChargeCode LoadChargeCode(ZString code, ZGuid? companyPK)
		{
			var ccQuery = new ZQuery(AccChargeCodeSchema.AC_Code, code);
			ccQuery.AddToFilter(AccChargeCodeSchema.AC_GC, companyPK);
			var cc = Factory.LoadTop1<AccChargeCode>(ccQuery);
			return cc;
		}

		void CreateChargeCode(ZGuid companyPK, params (ZString code, ZString desc)[] chargeCodesWithDescription)
		{
			foreach (var chargeCodeWithDescription in chargeCodesWithDescription)
			{
				var ccGbl = Factory.NewWithValidTestData<AccChargeCode>();
				ccGbl.AC_Code = chargeCodeWithDescription.code;
				ccGbl.AC_Desc = chargeCodeWithDescription.desc;
				ccGbl.AC_GC = companyPK;
				ccGbl.AC_ChargeType = "MRG";
			}
			Factory.Save();
		}

		GlbBranch CreateCompanyWithBranch(ZString companyCountryCode)
		{
			var ptCompany = Factory.NewWithValidTestData<GlbCompany>();
			ptCompany.GC_RN_NKCountryCode = companyCountryCode;
			ptCompany.GC_IsGSTRegistered = false;

			var ptBranch = Factory.NewWithValidTestData<GlbBranch>();
			ptBranch.GB_GC = ptCompany.PK;

			Factory.Save();

			return ptBranch;
		}

		OrgCusCode AddNewCusCode(RefCountry country, string codeType)
		{
			var result = org.CustomsCodes.AddNew();
			result.OK_CodeType = codeType;
			result.OK_RN_NKCodeCountry = country.Code;
			return result;
		}

		OrgCusCode AddNewCusCode(string countryCode, string codeType)
		{
			var newCusCode = org.CustomsCodes.AddNew();
			newCusCode.OK_CodeType = codeType.IsNullOrEmpty() ? string.Empty : codeType;
			newCusCode.OK_RN_NKCodeCountry = countryCode.IsNullOrEmpty() ? string.Empty : countryCode;
			return newCusCode;
		}

		OrgCusCode AddNewCusCode(string countryCode, string code, string codeType)
		{
			var newCAGCode = org.CustomsCodes.AddNew();
			newCAGCode.OK_CodeType = codeType;
			newCAGCode.OK_RN_NKCodeCountry = countryCode.IsNullOrEmpty() ? string.Empty : countryCode;
			newCAGCode.OK_CustomsRegNo = code;
			return newCAGCode;
		}

		void RunEoriTcuinAeoTest(Tuple<bool, bool, bool> tuple, string countryOfIssue)
		{
			cusCode.OK_RN_NKCodeCountry = countryOfIssue;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			var canIssue = tuple.Item1;
			var message = string.Format("Country {0} should {1} be able to issue {2} codes", countryOfIssue, (canIssue ? "" : "not"), cusCode.OK_CodeType);
			AssertEquals(message, !canIssue, cusCode.OK_CodeTypeInfo.HasError("Enter a valid Type."));

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			canIssue = tuple.Item2;
			message = string.Format("Country {0} should {1} be able to issue {2} codes", countryOfIssue, (canIssue ? "" : "not"), cusCode.OK_CodeType);
			AssertEquals(message, !canIssue, cusCode.OK_CodeTypeInfo.HasError("Enter a valid Type."));

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
			canIssue = tuple.Item3;
			message = string.Format("Country {0} should {1} be able to issue {2} codes", countryOfIssue, (canIssue ? "" : "not"), cusCode.OK_CodeType);
			AssertEquals(message, !canIssue, cusCode.OK_CodeTypeInfo.HasError("Enter a valid Type."));
		}

		OrgCusCode CreateAndGetOrgCusCodePEC(string countryCode, string regNoValue)
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = TurkeyOrgCusCodeInfo.OrgCusCodes.PEC;
			cusCode.OK_RN_NKCodeCountry = countryCode;
			cusCode.OK_CustomsRegNo = regNoValue;
			Factory.Save();

			return cusCode;
		}

		void AssertCusCodePropertyValidationForPT(Action<OrgCusCode> changePropertyValue, Action<OrgCusCode> revertPropertyValue, Action<OrgCusCode> changePropertyValueAfterTransactionCreationWithoutClearingCache, Action<OrgCusCode> changePropertyValueAfterTransactionCreationWithClearedCache)
		{
			var newOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode1 = newOrg1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			cusCode1.OK_RN_NKCodeCountry = CountryCodes.Portugal;
			cusCode1.OK_CustomsRegNo = "BJU39V";

			var newOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode2 = newOrg2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.Portugal;
			cusCode2.OK_CustomsRegNo = "BJU39Z";

			Factory.Save();

			changePropertyValue(cusCode1);
			changePropertyValue(cusCode2);

			revertPropertyValue(cusCode1);
			revertPropertyValue(cusCode2);

			var ptBranch = CreateCompanyWithBranch(CountryCodes.Portugal);
			CreateChargeCode(ptBranch.GB_GC, ("CC1", "CC1 Desc"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ptBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var cc1 = LoadChargeCode("CC1", ptBranch.GB_GC);
				CreateARInvoiceWithLine(newOrg1, cc1);
			}

			changePropertyValueAfterTransactionCreationWithoutClearingCache(cusCode1);
			changePropertyValueAfterTransactionCreationWithoutClearingCache(cusCode2);

			newOrg1.RunPreSaveValidation();
			newOrg2.RunPreSaveValidation();

			changePropertyValueAfterTransactionCreationWithClearedCache(cusCode1);
			changePropertyValueAfterTransactionCreationWithoutClearingCache(cusCode2);
		}

		const string HRBWarningMessage = "Format of Handelsregister / Business Registration Number (HRB) is not correct. Accepted formats are: HRB or HRA followed by a space and one or more digits, e.g. 'HRB  123' or 'HRA 12345' OR HRB or HRA followed by a space, then one or more digits, followed by an optional space then alpha characters, e.g. 'HRB 123 HL' or 'HRB 123HL'.";
	}
}
