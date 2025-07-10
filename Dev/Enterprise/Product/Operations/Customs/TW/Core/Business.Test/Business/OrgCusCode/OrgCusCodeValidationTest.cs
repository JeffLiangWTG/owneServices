using System;
using System.Collections.ObjectModel;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business.Testing
{
	internal sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CustomsRegNo()
		{
			cusCode.OK_CustomsRegNo = "";
			Assert("OK_CustomsRegNo is mandatory", cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_CustomsRegNo = "a12dwer";
			Assert("No Error expected", !cusCode.OK_CustomsRegNoInfo.HasErrors());

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "1234567";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The Taiwan VAT number must be an 8-digit number.");
			cusCode.OK_CustomsRegNo = "123456ab";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The Taiwan VAT number must be an 8-digit number.");
			cusCode.OK_CustomsRegNo = "12345678";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "The Taiwan VAT number is invalid.");
			cusCode.OK_CustomsRegNo = "96944492";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "The Taiwan VAT number is invalid.");
			cusCode.OK_CustomsRegNo = "96944490";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.TPC;
			cusCode.OK_CustomsRegNo = "111111111";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The length of TPC shouldn't be more than 8.");
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of TPC shouldn't be more than 8.");
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;

			cusCode.OK_CustomsRegNo = "11111111";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of TPC shouldn't be more than 8.");

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PID;
			cusCode.OK_CustomsRegNo = "11111111111";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			cusCode.OK_CustomsRegNo = "1111111111";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PBR;
			cusCode.OK_CustomsRegNo = "1234567890123";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The length of PBR shouldn't be more than 12.");
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of PBR shouldn't be more than 12.");
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;

			cusCode.OK_CustomsRegNo = "123456789012";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of PBR shouldn't be more than 12.");

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusCode.OK_CustomsRegNo = "012345678901234";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The length of PAS (Passport Number) shouldn't be more than 14.");
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of PAS (Passport Number) shouldn't be more than 14.");

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CustomsRegNo = "01234567890123";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of PAS (Passport Number) shouldn't be more than 14.");
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "PAS number is formatted as NOxxxxxxxxx, but you only need to enter the suffix component of the number here.");

			cusCode.OK_CustomsRegNo = "NO1234";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "PAS number is formatted as NOxxxxxxxxx, but you only need to enter the suffix component of the number here.");

			var message = "Taiwan GTX (Government Tax File Code) should be 9 digits NNNNNNNNN.";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "12345";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "abc123456";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "abcdefghij";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "12345678,";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "987654321";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, message);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
			cusCode.OK_CustomsRegNo = "1234567890";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "12345";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "abc123456";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "abcdefghij";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "12345678,";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "987654321";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, message);

			var errorMessage = "Taiwan PIG (Public Interest Group) should be between 3 - 7 digits NNN, NNNN, NNNNN, NNNNNN, NNNNNNN.";

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PIG;
			cusCode.OK_CustomsRegNo = "12";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "12345678";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "1234567";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, errorMessage);

			var errorMessage1 = "Taiwan MCI (Mobile Carrier ID) should be 8 characters starting with '/' follow by 7 characters. The accepted characters are '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ+-. E.g. /J8U743A";

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.MCI;
			cusCode.OK_CustomsRegNo = "1234AZ+-";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, errorMessage1);

			cusCode.OK_CustomsRegNo = "/12345678";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, errorMessage1);

			cusCode.OK_CustomsRegNo = "/a123456";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, errorMessage1);

			cusCode.OK_CustomsRegNo = "/12AZ+-.";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, errorMessage1);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PIG;
			cusCode.OK_CustomsRegNo = "12";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "12345678";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, errorMessage);

			cusCode.OK_CustomsRegNo = "1234567";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.MCI;
			cusCode.OK_CustomsRegNo = "1234AZ+-";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, errorMessage1);

			cusCode.OK_CustomsRegNo = "/12345678";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, errorMessage1);

			cusCode.OK_CustomsRegNo = "/a123456";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, errorMessage1);

			cusCode.OK_CustomsRegNo = "/12AZ+-.";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			cusCode.OK_CustomsRegNo = "1111111";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The length of FRI must be 8.");

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			cusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of FRI must be 8.");

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CustomsRegNo = "11111111";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "The length of FRI must be 8.");

			cusCode.OK_CustomsRegNo = "111111111";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The length of FRI must be 8.");

			cusCode.OK_CustomsRegNo = ZString.Empty;
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "The length of FRI must be 8.");
		}

		public void TestCheckOK_CustomsRegNo_MaxLength()
		{
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;

			cusCode.OK_CustomsRegNo = ZString.Replicate('1', 15);
			AssertHasError(cusCode.OK_CustomsRegNoInfo, ValidationConstants.OrgCusCode.CCCMaximumAllowedLengthExceeded);

			cusCode.OK_CustomsRegNo = ZString.Replicate('1', 14);
			AssertNoError(cusCode.OK_CustomsRegNoInfo, ValidationConstants.OrgCusCode.CCCMaximumAllowedLengthExceeded);
		}

		public void TestCheckOK_OA_PremisesAddress()
		{
			string message = "Taiwan VAT (Government VAT Code) should have a Premises Address specified.";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, message);
			cusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, message);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasWarning(cusCode.OK_OA_PremisesAddressInfo, message);
			cusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertNoWarning(cusCode.OK_OA_PremisesAddressInfo, message);

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);

			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoWarning(cusCode.OK_OA_PremisesAddressInfo, message);

			string messageError = string.Concat("An address is required for code type '", OrgCusCode.CodeTypes.ControlledPremisesID, "'.");
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);
			cusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PID;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrors(cusCode.OK_OA_PremisesAddressInfo);

			messageError = string.Concat("An address is required for code type '", OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "'.");
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			messageError = string.Concat("An address is required for code type '", OrgCusCode.TaiwanCodeTypes.EPZ, "'.");
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			messageError = string.Concat("An address is required for code type '", OrgCusCode.TaiwanCodeTypes.FTZ, "'.");
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			messageError = string.Concat("An address is required for code type '", OrgCusCode.TaiwanCodeTypes.CBF, "'.");
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			messageError = string.Concat("An address is required for code type '", OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "'.");
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			cusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);

			messageError = string.Concat("An address is required for code type '", OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, "'.");
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);
		}

		public void TestCheckOK_OA_PremisesAddressForATP()
		{
			var messageError = "An address is required for code type 'ATP'.";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);
			cusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);
		}

		public void TestCheckOK_OA_PremisesAddressForSPK()
		{
			var messageError = "An address is required for code type 'SPK'.";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.SciencePark;
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasError(cusCode.OK_OA_PremisesAddressInfo, messageError);
			cusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			AssertNoError(cusCode.OK_OA_PremisesAddressInfo, messageError);
		}

		public void TestEPZ()
		{
			var code1 = AddNewCusCode(tWCountry, OrgCusCode.TaiwanCodeTypes.EPZ);
			code1.OK_CustomsRegNo = "AB12";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "An Export Processing Zone Customs Controlling Premises Code must be 5 characters long.");

			code1.OK_CustomsRegNo = "ABC123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "An Export Processing Zone Customs Controlling Premises Code must be 5 characters long.");

			code1.OK_CustomsRegNo = "AB123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(code1.OK_CustomsRegNoInfo, "An Export Processing Zone Customs Controlling Premises Code must be 5 characters long.");

			code1.OK_CustomsRegNo = "ABCDE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "The 4th and the 5th characters must be numbers.");

			code1.OK_CustomsRegNo = "ABC4E";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "The 4th and the 5th characters must be numbers.");

			code1.OK_CustomsRegNo = "ABCD5";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "The 4th and the 5th characters must be numbers.");

			code1.OK_CustomsRegNo = "ABC45";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(code1.OK_CustomsRegNoInfo, "The 4th and the 5th characters must be numbers.");

			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertHasError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'EPZ'.");

			code1.OK_OA_PremisesAddress = address.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertNoError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'EPZ'.");

			AssertCodeUnique(OrgCusCode.TaiwanCodeTypes.EPZ);
			foreach (var duplicatecode in noDuplicateAddressCodes)
			{
				AssertNoDuplicateAddress(OrgCusCode.TaiwanCodeTypes.EPZ, duplicatecode);
			}
			AssertAutoUpperCase(OrgCusCode.TaiwanCodeTypes.EPZ);
			AssertAlphaNumericOnly(OrgCusCode.TaiwanCodeTypes.EPZ, "ABC45", "AB-45", "An EPZ code can only have alphanumeric characters.");
		}

		public void TestCBF()
		{
			var code1 = AddNewCusCode(tWCountry, OrgCusCode.TaiwanCodeTypes.CBF);
			code1.OK_CustomsRegNo = "AB12";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Bonded Factory Customs Controlling Premises Code must be 5 characters long.");

			code1.OK_CustomsRegNo = "ABC123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Bonded Factory Customs Controlling Premises Code must be 5 characters long.");

			code1.OK_CustomsRegNo = "AB123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(code1.OK_CustomsRegNoInfo, "A Bonded Factory Customs Controlling Premises Code must be 5 characters long.");

			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertHasError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'CBF'.");

			code1.OK_OA_PremisesAddress = address.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertNoError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'CBF'.");

			AssertCodeUnique(OrgCusCode.TaiwanCodeTypes.CBF);
			foreach (var duplicatecode in noDuplicateAddressCodes)
			{
				AssertNoDuplicateAddress(OrgCusCode.TaiwanCodeTypes.CBF, duplicatecode);
			}
			AssertAutoUpperCase(OrgCusCode.TaiwanCodeTypes.CBF);
			AssertAlphaNumericOnly(OrgCusCode.TaiwanCodeTypes.CBF, "ABC45", "AB-45", "A CBF code can only have alphanumeric characters.");
		}

		public void TestFTZ()
		{
			var code1 = AddNewCusCode(tWCountry, OrgCusCode.TaiwanCodeTypes.FTZ);
			code1.OK_CustomsRegNo = "AB12";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Free Trade Zone Customs Controlling Premises Code must be 5 characters long.");

			code1.OK_CustomsRegNo = "ABC123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Free Trade Zone Customs Controlling Premises Code must be 5 characters long.");

			code1.OK_CustomsRegNo = "AB123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(code1.OK_CustomsRegNoInfo, "A Free Trade Zone Customs Controlling Premises Code must be 5 characters long.");

			var ftzWarning = "The first character of FTZ code should be one of \"W\", \"X\", \"Y\", \"Z\", \"P\", \"Q\", \"R\", \"S\".";
			code1.OK_CustomsRegNo = "ABCDE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.OK_CustomsRegNo = "WABCD";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.OK_CustomsRegNo = "XABCD";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.OK_CustomsRegNo = "YABCD";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.OK_CustomsRegNo = "ZABCD";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.OK_CustomsRegNo = "PABCD";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.OK_CustomsRegNo = "QABCD";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.OK_CustomsRegNo = "RABCD";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.OK_CustomsRegNo = "SABCD";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, ftzWarning);

			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertHasError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'FTZ'.");

			code1.OK_OA_PremisesAddress = address.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertNoError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'FTZ'.");

			AssertCodeUnique(OrgCusCode.TaiwanCodeTypes.FTZ);
			foreach (var duplicatecode in noDuplicateAddressCodes)
			{
				AssertNoDuplicateAddress(OrgCusCode.TaiwanCodeTypes.FTZ, duplicatecode);
			}
			AssertAutoUpperCase(OrgCusCode.TaiwanCodeTypes.FTZ);
			AssertAlphaNumericOnly(OrgCusCode.TaiwanCodeTypes.FTZ, "SABCD", "SA-CD", "A FTZ code can only have alphanumeric characters.");
		}

		public void TestCPW()
		{
			var code1 = AddNewCusCode(tWCountry, OrgCusCode.CodeTypes.WarehouseControlledPremisesID);
			code1.OK_CustomsRegNo = "AB12";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Customs Bonded Warehouse Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "ABC123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Customs Bonded Warehouse Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "ABC1234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Customs Bonded Warehouse Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "ABC123456";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Customs Bonded Warehouse Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "ABCD1234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(code1.OK_CustomsRegNoInfo, "A Customs Bonded Warehouse Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "AB123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, "The second character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses.");

			code1.OK_CustomsRegNo = "ABCD1234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, "The fifth character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses.");

			code1.OK_CustomsRegNo = "AD234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "The second character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses.");

			code1.OK_CustomsRegNo = "AG234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "The second character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses.");

			code1.OK_CustomsRegNo = "AABCD234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "The fifth character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses.");

			code1.OK_CustomsRegNo = "AABCG234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "The fifth character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses.");

			code1.OK_CustomsRegNo = "EDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEEEDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "ADEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "BDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "CDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "DDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEEADEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEEBDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEECDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEEDDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertHasError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'CPW'.");

			code1.OK_OA_PremisesAddress = address.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertNoError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'CPW'.");

			code1.OK_CustomsRegNo = "EEEDDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "The first three characters of a CPW code must be numeric digits.");

			code1.OK_CustomsRegNo = "123DDEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(code1.OK_CustomsRegNoInfo, "The first three characters of a CPW code must be numeric digits.");

			AssertCodeUnique(OrgCusCode.CodeTypes.WarehouseControlledPremisesID);
			foreach (var duplicatecode in noDuplicateAddressCodes)
			{
				AssertNoDuplicateAddress(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, duplicatecode);
			}
			AssertAutoUpperCase(OrgCusCode.CodeTypes.WarehouseControlledPremisesID);
			AssertAlphaNumericOnly(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "EEEDD123", "EEEDD-12", "A CPW code can only have alphanumeric characters.");
		}

		public void TestCCP()
		{
			var code1 = AddNewCusCode(tWCountry, OrgCusCode.CodeTypes.ControlledPremisesID);
			code1.OK_CustomsRegNo = "AB12";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Customs Logistic Center Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "ABC123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Customs Logistic Center Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "ABC1234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Customs Logistic Center Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "ABC123456";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(code1.OK_CustomsRegNoInfo, "A Customs Logistic Center Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "ABCD1234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(code1.OK_CustomsRegNoInfo, "A Customs Logistic Center Customs Controlling Premises Code must be 5 or 8 characters long.");

			code1.OK_CustomsRegNo = "AB123";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, "The second character a CCP code should be 'L'.");

			code1.OK_CustomsRegNo = "ABCD1234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, "The fifth character a CCP code should be 'L'.");

			code1.OK_CustomsRegNo = "AL234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "The second character a CCP code should be 'L'.");

			code1.OK_CustomsRegNo = "AABCL234";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "The fifth character a CCP code should be 'L'.");

			code1.OK_CustomsRegNo = "ELEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEEELEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "ALEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "BLEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "CLEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "DLEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEEALEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEEBLEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEECLEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.OK_CustomsRegNo = "EEEDLEEE";
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoWarning(code1.OK_CustomsRegNoInfo, "This Registration Number is invalid.");

			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertHasError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'CCP'.");

			code1.OK_OA_PremisesAddress = address.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertNoError(code1.OK_OA_PremisesAddressInfo, "An address is required for code type 'CCP'.");

			AssertCodeUnique(OrgCusCode.CodeTypes.ControlledPremisesID);
			foreach (var duplicatecode in noDuplicateAddressCodes)
			{
				AssertNoDuplicateAddress(OrgCusCode.CodeTypes.ControlledPremisesID, duplicatecode);
			}
			AssertAutoUpperCase(OrgCusCode.CodeTypes.ControlledPremisesID);
			AssertAlphaNumericOnly(OrgCusCode.CodeTypes.ControlledPremisesID, "EEEDL123", "EEEDL-12", "A CCP code can only have alphanumeric characters.");
		}

		public void TestFRINoDuplicateAddress()
		{
			org.CustomsCodes.RemoveAll();
			var address1 = org.Addresses.AddNew();

			var errorMessage = "One address can only have one factory premise code.";
			var code1 = AddNewCusCode(tWCountry, OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber);
			code1.OK_OA_PremisesAddress = address1.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertNoErrorContaining(code1.OK_OA_PremisesAddressInfo, errorMessage);

			var code2 = AddNewCusCode(tWCountry, OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber);
			code2.OK_OA_PremisesAddress = address1.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertHasError(code1.OK_OA_PremisesAddressInfo, errorMessage);
		}

		void AssertAlphaNumericOnly(string codeToTest, string regNoGood, string regNoBad, string expectedMsg)
		{
			org.CustomsCodes.RemoveAll();

			var code1 = AddNewCusCode(tWCountry, codeToTest);
			code1.OK_CustomsRegNo = regNoGood;
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(string.Format(CultureInfo.InvariantCulture, "When enter {1} to code {0}", codeToTest, regNoGood), code1.OK_CustomsRegNoInfo, expectedMsg);

			code1.OK_CustomsRegNo = regNoBad;
			code1.Validation.ValidateOK_CustomsRegNo();
			AssertHasError(string.Format(CultureInfo.InvariantCulture, "When enter {1} RegNo to code {0}", codeToTest, regNoBad), code1.OK_CustomsRegNoInfo, expectedMsg);
		}

		void AssertAutoUpperCase(string codeToTest)
		{
			org.CustomsCodes.RemoveAll();

			var code1 = AddNewCusCode(tWCountry, codeToTest);
			code1.OK_CustomsRegNo = "abcd1234";
			AssertEquals(string.Format(CultureInfo.InvariantCulture, "When enter RegNo to code {0}, RegNo should be Auto Capitalized.", codeToTest), "ABCD1234", code1.OK_CustomsRegNo);
		}

		void AssertCodeUnique(string codeToTest)
		{
			org.CustomsCodes.RemoveAll();

			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();

			var code1 = AddNewCusCode(tWCountry, codeToTest);
			code1.Validation.ValidateOK_CodeType();
			AssertNoErrors(code1.OK_CodeTypeInfo);

			var code2 = AddNewCusCode(tWCountry, codeToTest);
			code1.Validation.ValidateOK_CodeType();
			code2.Validation.ValidateOK_CodeType();
			AssertHasError(code1.OK_CodeTypeInfo, "Each code of this type must have a different country/region and address combination.");
			AssertHasError(code2.OK_CodeTypeInfo, "Each code of this type must have a different country/region and address combination.");

			code1.OK_OA_PremisesAddress = address1.PK;
			code1.Validation.ValidateOK_CodeType();
			AssertNoErrors(code1.OK_CodeTypeInfo);

			code2.OK_OA_PremisesAddress = address1.PK;
			code1.Validation.ValidateOK_CodeType();
			code2.Validation.ValidateOK_CodeType();
			AssertHasError(code1.OK_CodeTypeInfo, "Each code of this type must have a different country/region and address combination.");
			AssertHasError(code2.OK_CodeTypeInfo, "Each code of this type must have a different country/region and address combination.");

			code2.OK_OA_PremisesAddress = address2.PK;
			code1.Validation.ValidateOK_CodeType();
			code2.Validation.ValidateOK_CodeType();
			AssertNoErrors(code1.OK_CodeTypeInfo);
			AssertNoErrors(code2.OK_CodeTypeInfo);
		}

		void AssertNoDuplicateAddress(string basecode, string duplicateCode)
		{
			org.CustomsCodes.RemoveAll();

			var address1 = org.Addresses.AddNew();
			var code1 = AddNewCusCode(tWCountry, basecode);
			code1.OK_OA_PremisesAddress = address1.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			AssertNoErrors(code1.OK_OA_PremisesAddressInfo);

			var code2 = AddNewCusCode(tWCountry, duplicateCode);
			code2.OK_OA_PremisesAddress = address1.PK;
			code1.Validation.ValidateOK_OA_PremisesAddress();
			code2.Validation.ValidateOK_OA_PremisesAddress();
			AssertHasError(string.Format(CultureInfo.InvariantCulture, "When code {0} and code {1} have the same address: ", basecode, duplicateCode), code1.OK_OA_PremisesAddressInfo, "One address can only have one bonded premise code.");
			AssertHasError(string.Format(CultureInfo.InvariantCulture, "When code {0} and code {1} have the same address: ", basecode, duplicateCode), code2.OK_OA_PremisesAddressInfo, "One address can only have one bonded premise code.");
		}

		public void TestValidateCodeType()
		{
			var expectedErrorMessage = "The following (TW) Registration codes cannot coexist: MCI, PIG";

			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.MCI;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CustomsRegNo = "/1234567";
			AssertNoError(cusCode.OK_CodeTypeInfo, expectedErrorMessage);

			OrgCusCode cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode2.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PIG;
			cusCode2.OK_CustomsRegNo = "123";

			cusCode.Validation.ValidateOK_CodeType();
			AssertHasError(cusCode.OK_CodeTypeInfo, expectedErrorMessage);
			cusCode2.Validation.ValidateOK_CodeType();
			AssertHasError(cusCode2.OK_CodeTypeInfo, expectedErrorMessage);

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode2.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PIG;
			cusCode2.Validation.ValidateOK_CodeType();
			AssertNoError(cusCode2.OK_CodeTypeInfo, expectedErrorMessage);
		}

		public void TestCustomsCodeFEI()
		{
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			cusCode.OK_CustomsRegNo = "1111111111111111";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, ValidationConstants.OrgCusCode.FEIMaximumAllowedLengthExceeded);

			cusCode.OK_CustomsRegNo = "111111111111111";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, ValidationConstants.OrgCusCode.FEIMaximumAllowedLengthExceeded);
		}

		OrgCusCode cusCode;
		OrgHeader org;
		RefCountry tWCountry;
		OrgAddress address;
		Collection<ZString> noDuplicateAddressCodes;

		protected override void SetUp()
		{
			base.SetUp();
			org = Factory.NewWithValidTestData<OrgHeader>();
			cusCode = org.CustomsCodes.AddNew();
			tWCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.Taiwan));
			address = org.Addresses.AddNew();

			noDuplicateAddressCodes = new Collection<ZString>
			{
				OrgCusCode.TaiwanCodeTypes.EPZ,
				OrgCusCode.TaiwanCodeTypes.FTZ,
				OrgCusCode.TaiwanCodeTypes.CBF,
				OrgCusCode.CodeTypes.WarehouseControlledPremisesID,
				OrgCusCode.CodeTypes.ControlledPremisesID
			};
		}

		OrgCusCode AddNewCusCode(RefCountry country, string codeType)
		{
			OrgCusCode result = org.CustomsCodes.AddNew();
			result.OK_CodeType = codeType;
			result.OK_RN_NKCodeCountry = country.Code;
			return result;
		}
	}
}
