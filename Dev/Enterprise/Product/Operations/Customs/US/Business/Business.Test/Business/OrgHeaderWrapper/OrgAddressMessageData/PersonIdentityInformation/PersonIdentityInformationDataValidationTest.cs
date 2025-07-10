using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class PersonIdentityInformationDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Name()
		{
			var errorMsg = "Format of name should be Last Name(max 40 characters), First Name(max 40 characters), Middle Initial(optional, max 18 characters)";
			PersonIdentityData.US_Name = ZString.Empty;
			AssertHasMessageErrorContaining(PersonIdentityData.US_NameInfo, MandatoryValidation.YouHaveNotEntered);
			PersonIdentityData.US_Name = "Donald";
			AssertNoMessageErrorContaining(PersonIdentityData.US_NameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(PersonIdentityData.US_NameInfo, errorMsg);

			PersonIdentityData.US_Name = "Donald Trump";
			AssertHasMessageErrorContaining(PersonIdentityData.US_NameInfo, errorMsg);

			PersonIdentityData.US_Name = "Trump, Donald";
			AssertNoMessageErrorContaining(PersonIdentityData.US_NameInfo, errorMsg);

			PersonIdentityData.US_Name = "Trump, Donald, J";
			AssertNoMessageErrorContaining(PersonIdentityData.US_NameInfo, errorMsg);

			PersonIdentityData.US_Name = "Trump, Donald, J,";
			AssertHasMessageErrorContaining(PersonIdentityData.US_NameInfo, errorMsg);

			PersonIdentityData.US_Name = "Trump, Donald, Jaaaaaaaaabbbbbbbbbb";
			AssertHasMessageErrorContaining(PersonIdentityData.US_NameInfo, errorMsg);
		}

		public void TestCheckUS_Title()
		{
			PersonIdentityData.US_Title = ZString.Empty;
			AssertHasMessageErrorContaining(PersonIdentityData.US_TitleInfo, MandatoryValidation.YouHaveNotEntered);
			PersonIdentityData.US_Title = "AAA";
			AssertNoMessageErrorContaining(PersonIdentityData.US_TitleInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_SSN()
		{
			PersonIdentityData.US_SSN = ZString.Empty;
			AssertNoMessageError(PersonIdentityData.US_SSNInfo, PersonIdentityInformationDataValidation.SSNRequired);
			PersonIdentityData.messageData.US_HaveSSNIndicator = true;
			PersonIdentityData.Validation.ValidateUS_SSN();
			AssertHasMessageError(PersonIdentityData.US_SSNInfo, PersonIdentityInformationDataValidation.SSNRequired);
			PersonIdentityData.US_SSN = "123";
			AssertNoMessageError(PersonIdentityData.US_SSNInfo, PersonIdentityInformationDataValidation.SSNRequired);
			AssertHasMessageError(PersonIdentityData.US_SSNInfo, SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);
			PersonIdentityData.US_SSN = "123-45-6789";
			AssertNoMessageError(PersonIdentityData.US_SSNInfo, SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);
		}

		[TestDate(2019, 1, 2)]
		public void TestCheckUS_ExpirationDateIsValidZDateTimeRange()
		{
			PersonIdentityData.US_ExpirationDate = new ZDateTime(2019, 1, 1);
			AssertHasWarningContaining(PersonIdentityData.US_ExpirationDateInfo, "Expiration date entered is in the past");

			PersonIdentityData.US_ExpirationDate = new ZDateTime(2029, 1, 5);
			AssertNoWarningContaining(PersonIdentityData.US_ExpirationDateInfo, "Expiration date entered is in the past");
			AssertHasErrorContaining(PersonIdentityData.US_ExpirationDateInfo, "is more than 10 years from now and thus is not valid.");

			PersonIdentityData.US_ExpirationDate = new ZDateTime(2024, 1, 5);
			AssertNoErrorContaining(PersonIdentityData.US_ExpirationDateInfo, "is more than 10 years from now and thus is not valid.");
		}

		public void TestCheckUS_ExpirationDate()
		{
			PersonIdentityData.US_ExpirationDate = ZDateTime.Empty;
			AssertNoMessageError(PersonIdentityData.US_ExpirationDateInfo, PersonIdentityInformationDataValidation.ExpirationDateRequired);
			PersonIdentityData.US_PassportNo = "A";
			PersonIdentityData.Validation.ValidateUS_ExpirationDate();
			AssertHasMessageError(PersonIdentityData.US_ExpirationDateInfo, PersonIdentityInformationDataValidation.ExpirationDateRequired);
			PersonIdentityData.US_ExpirationDate = ZDateTime.BrettsBirthday;
			AssertNoMessageError(PersonIdentityData.US_ExpirationDateInfo, PersonIdentityInformationDataValidation.ExpirationDateRequired);
		}

		public void TestCheckUS_CountryOfIssuance()
		{
			PersonIdentityData.US_CountryOfIssuance = ZString.Empty;
			AssertNoMessageError(PersonIdentityData.US_CountryOfIssuanceInfo, PersonIdentityInformationDataValidation.CountryOfIssuanceRequired);
			PersonIdentityData.US_PassportNo = "A";
			PersonIdentityData.Validation.ValidateUS_CountryOfIssuance();
			AssertHasMessageError(PersonIdentityData.US_CountryOfIssuanceInfo, PersonIdentityInformationDataValidation.CountryOfIssuanceRequired);
			PersonIdentityData.US_CountryOfIssuance = "US";
			AssertNoMessageError(PersonIdentityData.US_CountryOfIssuanceInfo, PersonIdentityInformationDataValidation.CountryOfIssuanceRequired);
		}

		public void TestCheckUS_PassportType()
		{
			PersonIdentityData.US_PassportType = ZString.Empty;
			AssertNoMessageErrorContaining(PersonIdentityData.US_PassportTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(PersonIdentityData.US_PassportTypeInfo, PersonIdentityInformationDataValidation.PassportTypeRequired);
			PersonIdentityData.US_PassportType = "A";
			AssertHasMessageErrorContaining(PersonIdentityData.US_PassportTypeInfo, ListValidation.InvalidCodeMessageError);
			PersonIdentityData.US_PassportType = PassportTypesList.Codes._01;
			AssertNoMessageErrorContaining(PersonIdentityData.US_PassportTypeInfo, ListValidation.InvalidCodeMessageError);
			PersonIdentityData.US_PassportType = ZString.Empty;
			PersonIdentityData.US_PassportNo = "A";
			PersonIdentityData.Validation.ValidateUS_PassportType();
			AssertHasMessageErrorContaining(PersonIdentityData.US_PassportTypeInfo, PersonIdentityInformationDataValidation.PassportTypeRequired);
			PersonIdentityData.US_PassportType = PassportTypesList.Codes._02;
			AssertNoMessageErrorContaining(PersonIdentityData.US_PassportTypeInfo, PersonIdentityInformationDataValidation.PassportTypeRequired);
		}

		public void TestCheckUS_PhoneNumber()
		{
			PersonIdentityData.US_PhoneNumber = ZString.Empty;
			AssertHasMessageErrorContaining(PersonIdentityData.US_PhoneNumberInfo, MandatoryValidation.YouHaveNotEntered);
			PersonIdentityData.US_PhoneNumber = "+12345";
			AssertNoMessageErrorContaining(PersonIdentityData.US_PhoneNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(PersonIdentityData.US_PhoneNumberInfo, PersonIdentityInformationDataValidation.InvalidPhoneNumberFormat);
			PersonIdentityData.US_PhoneNumber = "A12345";
			AssertHasMessageErrorContaining(PersonIdentityData.US_PhoneNumberInfo, PersonIdentityInformationDataValidation.InvalidPhoneNumberFormat);
			PersonIdentityData.US_PhoneNumber = "12345";
			AssertNoMessageErrorContaining(PersonIdentityData.US_PhoneNumberInfo, PersonIdentityInformationDataValidation.InvalidPhoneNumberFormat);
		}

		public void TestCheckUS_Email()
		{
			PersonIdentityData.US_Email = ZString.Empty;
			AssertHasMessageErrorContaining(PersonIdentityData.US_EmailInfo, MandatoryValidation.YouHaveNotEntered);
			PersonIdentityData.US_Email = "12345";
			AssertNoMessageErrorContaining(PersonIdentityData.US_EmailInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(PersonIdentityData.US_EmailInfo, PersonIdentityInformationDataValidation.ImporterEmailInvalidFormat);
			PersonIdentityData.US_Email = "12345@ABC.com";
			AssertNoWarning(PersonIdentityData.US_EmailInfo, PersonIdentityInformationDataValidation.ImporterEmailInvalidFormat);
		}

		#region Implementation

		PersonIdentityInformationData PersonIdentityData
		{
			get
			{
				if (personIdentityData == null)
				{
					var organization = Factory.New<OrgHeader>();
					var wrapper = OrgHeaderWrapper.New(organization);
					var messageData = new OrgAddressMessageData(wrapper);
					personIdentityData = messageData.PIIs.AddNew();
				}

				return personIdentityData;
			}
		}
		PersonIdentityInformationData personIdentityData;

		#endregion
	}
}
