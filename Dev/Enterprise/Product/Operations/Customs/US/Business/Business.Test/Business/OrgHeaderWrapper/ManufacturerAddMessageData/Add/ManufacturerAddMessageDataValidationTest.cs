using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class ManufacturerAddMessageDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Country()
		{
			Organisation.MainAddress.OA_RL_NKRelatedPortCode = "CAVAN";
			Organisation.MainAddress.OA_State = "AlbertaX";//invalid. It should be a two-letter code or valid description

			ManufacturerAddMessageData messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(Organisation));
			AssertEquals("PreCondition:US_Country should have been defaulted as CA to give a meaningful message", "CA", messageData.US_Country);
			AssertHasMessageErrorContaining(messageData.US_CountryInfo, ManufacturerAddMessageDataValidation.InvalidCACountryCode);

			Organisation.MainAddress.OA_State = "AB";//valid.
			messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(Organisation));
			AssertEquals("Country is calculated correctly", "XA", messageData.US_Country);
			AssertNoMessageErrorContaining(messageData.US_CountryInfo, ManufacturerAddMessageDataValidation.InvalidCACountryCode);

			Organisation.MainAddress.OA_State = "Alberta";//valid.
			messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(Organisation));
			AssertEquals("Country is calculated correctly", "XA", messageData.US_Country);
			AssertNoMessageErrorContaining(messageData.US_CountryInfo, ManufacturerAddMessageDataValidation.InvalidCACountryCode);

			messageData.US_Country = "";
			AssertHasMessageErrorContaining(messageData.US_CountryInfo, ManufacturerAddMessageDataValidation.CountryCodeMandatory);

			messageData.US_Country = "A'";
			AssertHasMessageErrorContaining(messageData.US_CountryInfo, messageData.US_CountryInfo.HumanReadableName + ManufacturerAddMessageDataValidation.AlphanumericMessage);
		}

		public void TestCheckUS_FirmName()
		{
			MessageData.US_FirmName = "";
			AssertHasMessageErrorContaining(MessageData.US_FirmNameInfo, MandatoryValidation.YouHaveNotEntered);

			MessageData.US_FirmName = "790542907";
			AssertNoMessageErrorContaining(MessageData.US_FirmNameInfo, MandatoryValidation.YouHaveNotEntered);

			MessageData.US_FirmName = "ABC*";
			AssertHasMessageErrorContaining(MessageData.US_FirmNameInfo, MessageData.US_FirmNameInfo.HumanReadableName + ManufacturerAddMessageDataValidation.AlphanumericMessage);
		}

		public void TestCheckUS_City()
		{
			MessageData.US_City = "";
			AssertHasMessageErrorContaining(MessageData.US_CityInfo, MandatoryValidation.YouHaveNotEntered);

			MessageData.US_City = "790542907";
			AssertNoMessageErrorContaining(MessageData.US_CityInfo, MandatoryValidation.YouHaveNotEntered);

			MessageData.US_City = "ABC/";
			AssertHasMessageErrorContaining(MessageData.US_CityInfo, MessageData.US_CityInfo.HumanReadableName + ManufacturerAddMessageDataValidation.AlphanumericMessage);
		}

		public void TestCheckUS_MID()
		{
			Organisation.OH_Code = "ABCD12345";
			var address1 = Organisation.MainAddress;
			address1.OA_RL_NKRelatedPortCode = "USCHI";
			MessageData.US_OA_AddressDetails = address1.PK;

			MessageData.US_MID = "";
			AssertNoNotifications(MessageData.US_MIDInfo);

			var address2 = Organisation.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "US7804587");

			MessageData.US_MID = "US7804587";
			AssertNoNotifications(MessageData.US_MIDInfo);

			MessageData.US_MID = "7804";
			AssertHasError(MessageData.US_MIDInfo, ManufacturerIDValidator.Constants.Format);

			MessageData.US_MID = "7804GHYUJKI8945";
			AssertNoError(MessageData.US_MIDInfo, ManufacturerIDValidator.Constants.Format);

			MessageData.US_MID = "CV3456GTRFDESWD";
			AssertHasMessageError(MessageData.US_MIDInfo, ManufacturerIDValidator.Constants.Country);

			MessageData.US_MID = "US3456GTRFDESWD";
			AssertNoMessageError(MessageData.US_MIDInfo, ManufacturerIDValidator.Constants.Country);

			MessageData.US_OA_AddressDetails = ZGuid.Empty;
			address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Myanmar;
			address1.OA_RL_NKRelatedPortCode = "MMAKY";
			MessageData.US_OA_AddressDetails = address1.PK;
			AssertEquals(Core.Constants.CountryCodes.Myanmar, MessageData.US_Country);
			AssertNoMessageError(MessageData.US_MIDInfo, ManufacturerIDValidator.Constants.Country);

			MessageData.US_OA_AddressDetails = ZGuid.Empty;
			address1.OA_RN_NKCountryCode = USCCountry.Burma;
			address1.OA_RL_NKRelatedPortCode = "BUAKY";
			MessageData.US_OA_AddressDetails = address1.PK;
			AssertEquals(USCCountry.Burma, MessageData.US_Country);
			AssertNoMessageError(MessageData.US_MIDInfo, ManufacturerIDValidator.Constants.Country);
			
			MessageData.US_MID = "ABC!";
			AssertHasMessageErrorContaining(MessageData.US_MIDInfo, MessageData.US_MIDInfo.HumanReadableName + ManufacturerAddMessageDataValidation.AlphanumericMessage);
		}

		public void TestCheckUS_Zip()
		{
			MessageData.US_Country = "US";
			MessageData.US_Zip = "";
			AssertHasMessageError(MessageData.US_ZipInfo, ManufacturerAddMessageDataValidation.ZipIsMandatoryForUSManufacturer);

			MessageData.US_Country = Core.Constants.CountryCodes.PuertoRico;
			MessageData.Validation.ValidateUS_Zip();
			AssertHasMessageError(MessageData.US_ZipInfo, ManufacturerAddMessageDataValidation.ZipIsMandatoryForUSManufacturer);

			MessageData.US_Country = "KR";
			AssertNoMessageError(MessageData.US_ZipInfo, ManufacturerAddMessageDataValidation.ZipIsMandatoryForUSManufacturer);

			MessageData.US_Country = "XA";
			AssertEquals(true, MessageData.IsCA);
			MessageData.US_Zip = "A";
			AssertHasMessageError(MessageData.US_ZipInfo, ZipCodeValidation.InvalidPostCodeForCA);

			MessageData.US_Zip = "A1A2B2";//space should be inserted
			AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.InvalidPostCodeForCA);

			MessageData.US_Country = "MX";
			AssertEquals(true, MessageData.IsMX);

			MessageData.US_Zip = "A";
			AssertHasMessageError(MessageData.US_ZipInfo, ZipCodeValidation.InvalidPostCodeForMX);

			MessageData.US_Zip = "12345";
			AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.InvalidPostCodeForMX);

			MessageData.US_Zip = "";
			AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.InvalidPostCodeForMX);

			MessageData.US_Zip = "ABCβ";
			AssertHasMessageErrorContaining(MessageData.US_ZipInfo, MessageData.US_ZipInfo.HumanReadableName + ManufacturerAddMessageDataValidation.AlphanumericMessage);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PostalCodeIsRequiredForChinaMF, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				MessageData.US_Country = Core.Constants.CountryCodes.China;
				MessageData.US_Zip = ZString.Empty;
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				MessageData.US_Zip = "123456";
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				MessageData.US_Zip = "ABCDEF";
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				MessageData.US_Country = Core.Constants.CountryCodes.UnitedStates;
				MessageData.Validation.ValidateUS_Zip();
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PostalCodeIsRequiredForChinaMF, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				MessageData.US_Country = Core.Constants.CountryCodes.China;
				MessageData.US_Zip = ZString.Empty;
				AssertHasMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				MessageData.US_Zip = "123456";
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				MessageData.US_Zip = "ABCDEF";
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertHasMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
				MessageData.US_Country = Core.Constants.CountryCodes.UnitedStates;
				MessageData.Validation.ValidateUS_Zip();
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeRequiredForChinaManufacturer);
				AssertNoMessageError(MessageData.US_ZipInfo, ZipCodeValidation.PostalCodeIsInvalidForChinaManufacturer);
			}
		}

		public void TestCheckUS_Street()
		{
			MessageData.US_Street = "ABCα";
			AssertHasMessageErrorContaining(MessageData.US_StreetInfo, MessageData.US_StreetInfo.HumanReadableName + ManufacturerAddMessageDataValidation.AlphanumericMessage);
		}

		ManufacturerAddMessageData MessageData
		{
			get
			{
				if (messageData == null)
				{
					messageData = new ManufacturerAddMessageData(OrgHeaderWrapper.New(Organisation));
				}
				return messageData;
			}
		}
		ManufacturerAddMessageData messageData;

		OrgHeader Organisation
		{
			get
			{
				if (organisation == null)
				{
					organisation = Factory.New<OrgHeader>();
				}
				return organisation;
			}
		}
		OrgHeader organisation;
	}
}
