using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CodeType()
		{
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			AssertNoError(cusCode.OK_CodeTypeInfo, OrgCusCodeValidation.OnlyOneNumberCanBeEntered);

			OrgCusCode cusCode2 = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567NN");
			AssertNoError(cusCode2.OK_CodeTypeInfo, OrgCusCodeValidation.OnlyOneNumberCanBeEntered);

			OrgCusCode cusCode3 = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "123-12-1234");
			AssertHasError(cusCode3.OK_CodeTypeInfo, OrgCusCodeValidation.OnlyOneNumberCanBeEntered);

			cusCode3.OK_CodeType = OrgCusCode.USACodeTypes.ForeignRegistrationNumber;
			AssertNoError(cusCode3.OK_CodeTypeInfo, OrgCusCodeValidation.OnlyOneNumberCanBeEntered);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode3.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertHasError(cusCode3.OK_CodeTypeInfo, OrgCusCodeValidation.OnlyOneNumberCanBeEntered);

			cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode3.OK_CodeType = OrgCusCode.USACodeTypes.DeprecatedSpecialAddressNotification;
			AssertHasError(cusCode3.OK_CodeTypeInfo, OrgCusCodeValidation.DeprecatedSpecialAddressNotificationMsg);
		}

		public void TestCheckOK_OA_PremisesAddress()
		{
			string messageError = "An address is required for code type '" + OrgCusCode.USACodeTypes.FIRMSCode + "'.";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);
			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			messageError = "An address is required for code type '" + OrgCusCode.USACodeTypes.ManufacturerID + "'.";
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);
			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);

			messageError = "An address is required for code type '" + OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber + "'.";
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);
			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			messageError = "An address is required for code type '" + OrgCusCode.USACodeTypes.FIRMSCode + "'.";
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CarrierPrefixCode;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestGS1CodeTypeIsValidated()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			cusCode.OK_CustomsRegNo = "#D@D";
			AssertHasErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestForeignProducerIdentifier()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignProducerIdentifier;
			var registrationIsRequiredMessage = "Please enter a Registration Number / Code.";
			cusCode.OK_CustomsRegNo = ZString.Empty;
			AssertHasError(cusCode.OK_CustomsRegNoInfo, registrationIsRequiredMessage);
			cusCode.OK_CustomsRegNo = "BACMEBR150520";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, registrationIsRequiredMessage);
			string messageError = "An address is required for code type '" + OrgCusCode.USACodeTypes.ForeignProducerIdentifier + "'.";
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignProducerIdentifier;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);
			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.USACodeTypes.ForeignProducerIdentifier;
			cusCode2.OK_CustomsRegNo = "BACMEBR150521";
			cusCode2.OK_OA_PremisesAddress = organisation.MainAddress.PK;
			var duplicateCombinationMessage = "Each code of this type must have a different country/region and address combination.";
			AssertHasError(cusCode2.OK_CodeTypeInfo, duplicateCombinationMessage);
			cusCode2.OK_OA_PremisesAddress = organisation.Addresses.AddNew().PK;
			AssertNoError(cusCode2.OK_CodeTypeInfo, duplicateCombinationMessage);
		}

		public void TestGS1AndRAIInvalidPremisesAddress()
		{
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.RegulatedAgentID;
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestCheckOK_OA_PremisesAddress_ShouldNotAddError_WhenOrgCusCodeIsDUN()
		{
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestValidateIFTPPermitNumber()
		{
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.IFTPPermitNumber;
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ValidateIFTPPermitNumberFormat);
			cusCode.OK_CustomsRegNo = "1234567890123456789012345678901234";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ValidateIFTPPermitNumberFormat);
		}

		public void TestNAICSPremisesAddressCheck()
		{
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem;
			cusCode.OK_CustomsRegNo = "123456";
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);
		}

		public void TestFIRMSCode()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode.OK_CustomsRegNo = "#D@D";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, FIRMSCodeValidator.FIRMSCodeRightFormat);

			cusCode.OK_CustomsRegNo = "A001";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, FIRMSCodeValidator.FIRMSCodeRightFormat);

			cusCode.OK_CustomsRegNo = "A0 01";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, FIRMSCodeValidator.FIRMSCodeRightFormat);

			cusCode.OK_CustomsRegNo = "A 01";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, FIRMSCodeValidator.FIRMSCodeRightFormat);
		}

		//-CCCCCCCCCCC
		public void TestEncryptedConsigneeNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EncryptedConsigneeNumber;
			cusCode.OK_CustomsRegNo = "#D@D";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat);

			cusCode.OK_CustomsRegNo = "-12Aa2CD34-1";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat);

			cusCode.OK_CustomsRegNo = "-12Aa2C 34-1";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat);

			cusCode.OK_CustomsRegNo = "-12Aa2CD34-11";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EncryptedConsigneeNumberValidator.EncryptedNumberRightFormat);
		}

		public void TestValidateAPHISAssignedNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.APHISAssignedNumber;
			cusCode.OK_CustomsRegNo = "#D@D";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.APHISAssignedNumberFormat);

			cusCode.OK_CustomsRegNo = "12345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.APHISAssignedNumberFormat);
		}

		//NNNNNNNNN
		public void TestDataUniversalNumberingSystem()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			cusCode.OK_CustomsRegNo = "#D@D";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);

			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);

			cusCode.OK_CustomsRegNo = "12345 789";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);

			cusCode.OK_CustomsRegNo = "1234A5789";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);

			cusCode.OK_CustomsRegNo = "1234567890";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemValidator.DUNSNumberFormat);
		}

		//NNNNNNNNNNNNN
		public void TestDataUniversalNumberingSystemPlus4()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4;
			cusCode.OK_CustomsRegNo = "#D@D";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat);

			cusCode.OK_CustomsRegNo = "1234567891234";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat);

			cusCode.OK_CustomsRegNo = "12345 7891234";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat);

			cusCode.OK_CustomsRegNo = "1234A57891234";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat);

			cusCode.OK_CustomsRegNo = "12345678901234";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, DataUniversalNumberingSystemPlus4Validator.DUNSPlust4RightFormat);
		}

		//YYDDPP-NNNNN
		public void TestValidateCBPAssignedNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			cusCode.OK_CustomsRegNo = "061234";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, CBPAssignedNumberValidator.CBPAssignedNumberRightFormat);

			cusCode.OK_CustomsRegNo = "061234-12345";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, CBPAssignedNumberValidator.CBPAssignedNumberRightFormat);
		}

		//NNN-NN-NNNN
		public void TestValidateSocialSecurityNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			cusCode.OK_CustomsRegNo = "123121234";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);

			cusCode.OK_CustomsRegNo = "123-12-1234";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);
		}

		public void TestCheckOK_CodeType_WhenCusCodeNotInDataBaseAndNoGrantedSSNSecurityRight()
		{
			var originalOrgDetailsViewPersonalInformation = Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;
			try
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;

				var ssnCusCode = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "111-11-1111", CountryCodes.UnitedStates);
				AssertEquals(true, ssnCusCode.OK_CodeTypeInfo.HasErrors());
				AssertHasError(ssnCusCode.OK_CodeTypeInfo, @"You do not have the appropriate security rights to select this Code.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: Maintain -> Master Data -> Organization -> View -> View Personal Information. ");
			}
			finally
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = originalOrgDetailsViewPersonalInformation;
			}
		}

		public void TestEditOK_CodeType_WhenCusCodeInDataBaseAndNoGrantedSSNSecurityRight()
		{
			var originalOrgDetailsViewPersonalInformation = Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;
			try
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
				var orgCusCode1 = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "111-11-1111", CountryCodes.UnitedStates);
				var orgCusCode2 = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ABIRoutingCode, "111-11-1111", CountryCodes.UnitedStates);
				Factory.Save();
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
				AssertEquals(false, orgCusCode1.OK_CodeTypeInfo.HasErrors());
				AssertEquals(false, orgCusCode2.OK_CodeTypeInfo.HasErrors());

				orgCusCode2.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
				AssertEquals(true, orgCusCode2.OK_CodeTypeInfo.HasErrors());
				AssertHasError(orgCusCode2.OK_CodeTypeInfo, @"You do not have the appropriate security rights to select this Code.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: Maintain -> Master Data -> Organization -> View -> View Personal Information. ");
			}
			finally
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = originalOrgDetailsViewPersonalInformation;
			}
		}

		//NN-NNNNNNNXX
		public void TestValidateEINNumber()
		{
			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode.OK_CustomsRegNo = "1234567NN";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "12-";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "-1234567NN";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "1-1234567NN";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "12-123456";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "12-123456D00";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "D2-123456700";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "12-12345670";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "12-12345670";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "123-1234567NN";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "12-1234567NN";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());

			cusCode.OK_CustomsRegNo = "1234567NN";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);

			cusCode.OK_CustomsRegNo = "12-1234567NN";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);
			AssertNoError(cusCode.OK_CustomsRegNoInfo, EmployerIdentificationNumberValidator.EINNumberRightFormat);
		}

		public void TestAMORegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.AirAMSOriginatorCode;
			cusCode.OK_CustomsRegNo = "~123456";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AirAMSOriginatorCodeFormat);

			cusCode.OK_CustomsRegNo = "1";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AirAMSOriginatorCodeFormat);

			cusCode.OK_CustomsRegNo = "ABCD123";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AirAMSOriginatorCodeFormat);

			cusCode.OK_CustomsRegNo = "";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");
		}

		public void TestDDTRegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.DDTCRegistrationNumber;
			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.DDTRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "12";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.DDTRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "G23456";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.DDTRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "M-3456";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.DDTRegistrationNumberFormatError);
		}

		public void TestACASValidation()
		{
			var errorMessage = OrgCusCodeValidation.ACARegistrationNumberFormatError;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ACASOriginatorCode;

			cusCode.OK_CustomsRegNo = "ABCDEFG";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGTWTG";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGTXX5";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGT!@#";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGT!@3";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);
			cusCode.OK_CustomsRegNo = "WTGTAB";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGT888";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGPAB";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGPABCD";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGP123";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGPABC";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTG1234";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "WTGPAB5";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, errorMessage);
		}

		public void TestAMSRegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.AMSRegistrationNumber;
			cusCode.OK_CustomsRegNo = "A1234";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AMSRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "A1";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AMSRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "A12";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AMSRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "A123";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AMSRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "123";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AMSRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "1234";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AMSRegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "1234567891";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.AMSRegistrationNumberFormatError);
		}

		public void TestValidateFDAEstablishmentIdentifier()
		{
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ASD3323ASD";
			var cusCode2 = org2.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "7804587598");

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;
			cusCode.OK_CustomsRegNo = "001234567890";
			string duplicateFDAEstablishmentIdentifier = string.Format(OrgCusCodeValidation.FDAEstablishmentIdentifiersAlreadyInUsed, cusCode2.CompanyCodeAndPremisesAddresses);
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat);
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, duplicateFDAEstablishmentIdentifier);

			cusCode.OK_CustomsRegNo = "7804587598";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat);
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, duplicateFDAEstablishmentIdentifier);

			cusCode.OK_CustomsRegNo = "7804587599";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, EstablishmentIdentifierValidator.EstablishmentIdentifierRightFormat);
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, duplicateFDAEstablishmentIdentifier);
		}

		public void TestValidateManufacturerID()
		{
			var country = Factory.New<USCCountry>();
			country.UC_Code = ISOCountryCodeForTest;

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			cusCode.OK_OA_PremisesAddress = organisation.MainAddress.PK;

			cusCode.OK_CustomsRegNo = MIDWithLowerCaseLettersForTest;
			AssertHasError(cusCode.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.UpperCase);

			Assert("Precondition: TooBigMIDForTest.length should be greather then 15", TooBigMIDForTest.Length > 15);
			cusCode.OK_CustomsRegNo = TooBigMIDForTest;
			AssertHasError(cusCode.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.Format);
			AssertNoError(cusCode.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.UpperCase);
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.ISOCode);

			cusCode.OK_CustomsRegNo = MIDWithAnotherISOForTest;
			AssertNoError(cusCode.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.Format);
			AssertNoError(cusCode.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.UpperCase);
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.ISOCode);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, CorrectMIDForTest);

			var duplicateManufacturerID = "This Manufacturer ID already exists on organization ORG1 ()";
			cusCode.OK_CustomsRegNo = CorrectMIDForTest;
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.ISOCode);
			AssertHasError(cusCode.OK_CustomsRegNoInfo, duplicateManufacturerID);

			cusCode.OK_CustomsRegNo = AnotherCorrectMIDForTest;
			AssertNoError(cusCode.OK_CustomsRegNoInfo, duplicateManufacturerID);

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ORG2";
			org3.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, CorrectMIDForTest);

			var duplicateManufacturerID_Multiple = @"This Manufacturer ID already exists on other organizations:
 - ORG1 ()
 - ORG2 ()";
			cusCode.OK_CustomsRegNo = CorrectMIDForTest;
			AssertHasError(cusCode.OK_CustomsRegNoInfo, duplicateManufacturerID_Multiple);
			cusCode.OK_CustomsRegNo = AnotherCorrectMIDForTest;
			AssertNoError(cusCode.OK_CustomsRegNoInfo, duplicateManufacturerID_Multiple);

			for (int i = 3; i <= 11; i++)
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "ORG" + i;
				org.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, CorrectMIDForTest);
			}

			var duplicateManufacturerID_MultipleMoreThan10 = @"This Manufacturer ID already exists on other organizations:
 - ORG1 ()
 - ORG10 ()
 - ORG11 ()
 - ORG2 ()
 - ORG3 ()
 - ORG4 ()
 - ORG5 ()
 - ORG6 ()
 - ORG7 ()
 - ORG8 ()
...
Please go to Admin -> Organization module and find all organizations with this manufacturer ID using 'Registration Number' module filter.";
			cusCode.OK_CustomsRegNo = CorrectMIDForTest;
			AssertHasError(cusCode.OK_CustomsRegNoInfo, duplicateManufacturerID_MultipleMoreThan10);
			cusCode.OK_CustomsRegNo = AnotherCorrectMIDForTest;
			AssertNoError(cusCode.OK_CustomsRegNoInfo, duplicateManufacturerID_MultipleMoreThan10);
		}

		public void TestDuplicateMIDCheckIsCheckedAllTheTime()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			var cusCode1 = org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, TooBigMIDForTest);
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			var cusCode2 = org2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, TooBigMIDForTest);
			AssertHasErrorContaining(cusCode2.OK_CustomsRegNoInfo, "This Manufacturer ID already exists on ");
			AssertHasError(cusCode2.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.Format);
			cusCode2.OK_CustomsRegNo = CorrectMIDForTest;
			AssertNoErrorContaining(cusCode2.OK_CustomsRegNoInfo, "This Manufacturer ID already exists on ");
			AssertNoError(cusCode2.OK_CustomsRegNoInfo, ManufacturerIDValidator.Constants.Format);
		}

		public void TestDeactivatedMID()
		{
			var country = Factory.New<USCCountry>();
			country.UC_Code = ISOCountryCodeForTest;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			org1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, CorrectMIDForTest);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			org2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, CorrectMIDForTest);

			var midCusCode = org2.MainAddress.CustomsCodes.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.USACodeTypes.ManufacturerID);
			var duplicateManufacturerID = "This Manufacturer ID already exists";
			AssertNotNull(midCusCode);
			AssertHasErrorContaining(midCusCode.OK_CustomsRegNoInfo, duplicateManufacturerID);

			midCusCode.OK_CustomsRegNo = "123";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, duplicateManufacturerID);

			org1.OH_IsActive = false;
			midCusCode.OK_CustomsRegNo = CorrectMIDForTest;
			AssertNoErrorContaining(midCusCode.OK_CustomsRegNoInfo, duplicateManufacturerID);

			org1.OH_IsActive = true;
			midCusCode.OK_CustomsRegNo = CorrectMIDForTest;
			AssertHasErrorContaining(midCusCode.OK_CustomsRegNoInfo, duplicateManufacturerID);

			org2.OH_IsActive = false;
			var midCusCode2 = org1.MainAddress.CustomsCodes.FirstOrDefault(x => x.OK_CodeType == OrgCusCode.USACodeTypes.ManufacturerID);
			AssertNotNull(midCusCode2);
			AssertNoErrorContaining(midCusCode2.OK_CustomsRegNoInfo, duplicateManufacturerID);

			org2.OH_IsActive = true;
			midCusCode2.OK_CustomsRegNo = CorrectMIDForTest;
			AssertHasErrorContaining(midCusCode2.OK_CustomsRegNoInfo, duplicateManufacturerID);
		}

		const string ISOCountryCodeForTest = "G4";
		const string AnotherISOCountryCodeForTest = "G3";
		const string CorrectMIDForTest = ISOCountryCodeForTest + "ABC123ABC";
		const string AnotherCorrectMIDForTest = ISOCountryCodeForTest + "123ABC123";
		const string MIDWithLowerCaseLettersForTest = ISOCountryCodeForTest + "abcdef123";
		const string MIDWithAnotherISOForTest = AnotherISOCountryCodeForTest + "ABC123ABC";
		const string TooBigMIDForTest = "1234567890ABCDEF";

		public void TestValidateFoodFacilityRegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber;
			cusCode.OK_CustomsRegNo = "123456789456123";
			AssertHasErrors(ValidationConstants.PriorNotice.FoodFacilityRegistrationNumberFormat, cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "1234F678912";
			AssertHasErrors(ValidationConstants.PriorNotice.FoodFacilityRegistrationNumberFormat, cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "12345678912";
			AssertNoErrors(ValidationConstants.PriorNotice.FoodFacilityRegistrationNumberFormat, cusCode.OK_CustomsRegNoInfo);
		}

		public void TestCheckOK_CustomsRegNo()
		{
			cusCode.OK_CustomsRegNo = "";
			Assert("OK_CustomsRegNo is mandatory", cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "a12dwer";
			Assert("No Error expected", !cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			Assert("OK_CustomsRegNo.MaxLength should be 4 when Code Type is '" + OrgCusCode.CodeTypes.CarrierCode + "'", cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "abcd";
			Assert("No error expected", !cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CodeType = ZString.Empty;
			cusCode.OK_CustomsRegNo = "a12dwer";

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.TruckCarrierCode;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			Assert("OK_CustomsRegNo.MaxLength should be 4 when Code Type is '" + OrgCusCode.CodeTypes.TruckCarrierCode + "'", cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "abcd";
			Assert("No error expected", !cusCode.OK_CustomsRegNoInfo.HasWarnings());
		}

		public void TestValidateNMFCParticipant()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.NMFCParticipant;
			cusCode.OK_CustomsRegNo = ZString.Empty;
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Please enter a " + cusCode.OK_CustomsRegNoInfo.Description + ".");
			cusCode.OK_CustomsRegNo = "YEK";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Enter a valid " + cusCode.OK_CustomsRegNoInfo.Description + ".");
			cusCode.OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.Yes;
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
			cusCode.OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.No;
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		}

		public void TestValidateForeignRegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignRegistrationNumber;
			cusCode.OK_CustomsRegNo = "120";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ForeignRegistrationNumberRightFormat);

			cusCode.OK_CustomsRegNo = "123-ABS35-1D4";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.ForeignRegistrationNumberRightFormat);
		}

		// NNNNNNNNNNN
		public void TestValidateShipperRegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ShipperRegistrationNumber;
			cusCode.OK_CustomsRegNo = "120";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, ShipperRegistrationNumberValidator.SFRRightFormat);

			cusCode.OK_CustomsRegNo = "123A4563254";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, ShipperRegistrationNumberValidator.SFRRightFormat);

			cusCode.OK_CustomsRegNo = "123 4563254";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, ShipperRegistrationNumberValidator.SFRRightFormat);

			cusCode.OK_CustomsRegNo = "12344563254";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, ShipperRegistrationNumberValidator.SFRRightFormat);
		}

		//XXXXXXXXXX
		public void TestValidateACEId()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ACEAssignedNumber;
			cusCode.OK_CustomsRegNo = "061234";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, ACEIdentifierValidator.ACEIdRightFormat);

			cusCode.OK_CustomsRegNo = "123AAB1234";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, ACEIdentifierValidator.ACEIdRightFormat);
		}

		//NNNNXXXNN
		public void TestValidateABIRoutingCode()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ABIRoutingCode;
			cusCode.OK_CustomsRegNo = "061234";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, ABIFilerValidator.ABIFilerRightFormat);

			cusCode.OK_CustomsRegNo = "0612ZJ534";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, ABIFilerValidator.ABIFilerRightFormat);

			cusCode.OK_CustomsRegNo = "0612ZJ5";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, ABIFilerValidator.ABIFilerRightFormat);
		}

		public void TestValidateTTIRegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.TTIRegistrationNumber;
			cusCode.OK_CustomsRegNo = "AAA-BB";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat);

			cusCode.OK_CustomsRegNo = "34ERR";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat);

			cusCode.OK_CustomsRegNo = "BR-MA-1234567890";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, TTIRegistrationNumberValidator.TTIRegistrationNumberMaxLength);

			cusCode.OK_CustomsRegNo = "BR-MA-SAM-1";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat);
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, TTIRegistrationNumberValidator.TTIRegistrationNumberMaxLength);
		}

		//XXXXXXX
		public void TestValidateFASTId()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FreeAndSecureTradeCode;
			cusCode.OK_CustomsRegNo = "061234";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, FASTIdentifierValidator.FASTIdRightFormat);

			cusCode.OK_CustomsRegNo = "0612ZJ5";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, FASTIdentifierValidator.FASTIdRightFormat);
		}

		public void TestValidateTireManufacturerCode()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.TireManufacturerCode;
			cusCode.OK_CustomsRegNo = "X2X2";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TireManufacturerCodeRightFormat);

			cusCode.OK_CustomsRegNo = "XX2";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TireManufacturerCodeRightFormat);

			cusCode.OK_CustomsRegNo = "X2";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TireManufacturerCodeRightFormat);

			cusCode.OK_CustomsRegNo = "X2X";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TireManufacturerCodeRightFormat);
		}

		public void TestValidateGlazingManufacturerCode()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.GlazingManufacturerCode;
			cusCode.OK_CustomsRegNo = "X22";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlazingManufacturerCodeRightFormat);

			cusCode.OK_CustomsRegNo = "22";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlazingManufacturerCodeRightFormat);

			cusCode.OK_CustomsRegNo = "123";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlazingManufacturerCodeRightFormat);

			cusCode.OK_CustomsRegNo = "2233";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlazingManufacturerCodeRightFormat);

			cusCode.OK_CustomsRegNo = "2";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlazingManufacturerCodeRightFormat);
		}

		public void TestValidateDEARegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DEA;
			cusCode.OK_CustomsRegNo = "1234ABCD";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.DEARegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "1234ABCD-";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.DEARegistrationNumberFormatError);

			cusCode.OK_CustomsRegNo = "1234ABCDE";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.DEARegistrationNumberFormatError);
		}

		public void TestValidateTTENumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.TTEPermitNumber;

			cusCode.OK_CustomsRegNo = "123";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TTEPermitNumberFormatError);

			cusCode.OK_CustomsRegNo = "DH-388-I9-974AI";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TTEPermitNumberFormatError);

			cusCode.OK_CustomsRegNo = "DH-38-974AI";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TTEPermitNumberFormatError);

			cusCode.OK_CustomsRegNo = "DHA-38-974AI";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TTEPermitNumberFormatError);

			cusCode.OK_CustomsRegNo = "XXXXXXXXXXX";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TTEPermitNumberFormatError);

			cusCode.OK_CustomsRegNo = "XX-XXXXXXXXX";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TTEPermitNumberFormatError);

			cusCode.OK_CustomsRegNo = "XX-XX-XXX-X";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.TTEPermitNumberFormatError);
		}

		public void TestValidateFDAForeignSellerRegistrationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FDAForeignSellerRegistrationNumber;
			cusCode.OK_CustomsRegNo = "1234ABCD";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.FDAForeignSellerRegistrationNumberFormat);

			cusCode.OK_CustomsRegNo = "12345678";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.FDAForeignSellerRegistrationNumberFormat);

			cusCode.OK_CustomsRegNo = "123456789012";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.FDAForeignSellerRegistrationNumberFormat);

			cusCode.OK_CustomsRegNo = "123456798";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.FDAForeignSellerRegistrationNumberFormat);
		}

		public void TestValidateLegalEntityIdentifier()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.LegalEntityIdentifier;
			cusCode.OK_CustomsRegNo = "~";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.LegalEntityIdentifierFormat);

			cusCode.OK_CustomsRegNo = "1";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.LegalEntityIdentifierFormat);

			cusCode.OK_CustomsRegNo = "1234567890ABCDEFGHIJ";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.LegalEntityIdentifierFormat);
		}

		public void TestValidateGlobalLocationNumber()
		{
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.GlobalLocationNumber;
			cusCode.OK_CustomsRegNo = "~";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlobalLocationNumberFormat);

			cusCode.OK_CustomsRegNo = "1";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlobalLocationNumberFormat);

			cusCode.OK_CustomsRegNo = "1234567890ABC";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlobalLocationNumberFormat);

			cusCode.OK_CustomsRegNo = "1234567890123";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, OrgCusCodeValidation.GlobalLocationNumberFormat);
		}

		public void TestForeignProducerIdentifierForCBMA23()
		{
			var warningText = OrgCusCodeValidation.FPIRegistrationNumberFormatError;

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignProducerIdentifier;
			cusCode.OK_CustomsRegNo = "~";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "TTB";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "TTB-FP";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "TTB-FP-012345";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "TTB-FP-0123456";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "TTB-FP-ABCDEFG";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, warningText);
		}

		public void TestForeignProducerIdentifierForBeer()
		{
			var warningText = ZString.Format(OrgCusCodeValidation.FPB_FPS_FPW_ForeignProducerIdentifierFormat, "Beer", "B");

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer;
			cusCode.OK_CustomsRegNo = "~";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "A123420";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "B1234567890BC20";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "B1234567890B20";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "B1B20";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, warningText);
		}

		public void TestForeignProducerIdentifierForSpirits()
		{
			var warningText = ZString.Format(OrgCusCodeValidation.FPB_FPS_FPW_ForeignProducerIdentifierFormat, "Spirits", "S");

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits;
			cusCode.OK_CustomsRegNo = "~";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "A123420";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "S1234567890BC20";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "S1234567890B20";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "S1B20";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, warningText);
		}

		public void TestForeignProducerIdentifierForWine()
		{
			var warningText = ZString.Format(OrgCusCodeValidation.FPB_FPS_FPW_ForeignProducerIdentifierFormat, "Wine or Cider", "W");

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine;
			cusCode.OK_CustomsRegNo = "~";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "A123420";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "W1234567890BC20";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "W1234567890B20";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, warningText);

			cusCode.OK_CustomsRegNo = "W1B20";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, warningText);
		}

		#region Implementation

		OrgCusCode cusCode;
		OrgHeader organisation;

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			cusCode = organisation.CustomsCodes.AddNew();
		}

		#endregion
	}
}
