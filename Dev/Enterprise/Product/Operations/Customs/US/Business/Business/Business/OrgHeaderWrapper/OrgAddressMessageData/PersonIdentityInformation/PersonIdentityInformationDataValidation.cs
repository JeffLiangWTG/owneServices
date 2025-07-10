using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PersonIdentityInformationDataValidation : AutoPersonIdentityInformationDataValidation
	{
		public PersonIdentityInformationDataValidation(AutoPersonIdentityInformationData parent)
			: base(parent)
		{
		}

		public new PersonIdentityInformationData Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PersonIdentityInformationData)base.Parent; }
		}

		protected override void CheckUS_OC_Contact()
		{
			base.CheckUS_OC_Contact();

			ValidateUS_Name();
			ValidateUS_Title();
			ValidateUS_PhoneNumber();
			ValidateUS_Email();
		}

		protected override void CheckUS_Name()
		{
			base.CheckUS_Name();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NameInfo);

			var errMsg = ContactNameHelper.GetWarningMessageIfInvalidFormat(Parent.US_Name);
			if (!errMsg.IsEmpty)
			{
				Parent.US_NameInfo.AddMessageError(errMsg);
			}
		}

		protected override void CheckUS_Title()
		{
			base.CheckUS_Title();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TitleInfo);
		}

		protected override void CheckUS_SSN()
		{
			base.CheckUS_SSN();

			if (Parent.US_SSN.IsEmpty)
			{
				if (Parent.messageData != null && Parent.messageData.US_HaveSSNIndicator)
				{
					Parent.US_SSNInfo.AddMessageError(SSNRequired);
				}
			}
			else if (!SocialSecurityNumberValidator.IsValidSSN(Parent.US_SSN))
			{
				Parent.US_SSNInfo.AddMessageError(SocialSecurityNumberValidator.SocialSecurityNumberRightFormat);
			}
		}
		internal const string SSNRequired = "You have indicated you have SSN#, but no SSN# is entered.";

		protected override void CheckUS_PassportNo()
		{
			base.CheckUS_PassportNo();

			ValidateUS_ExpirationDate();
			ValidateUS_CountryOfIssuance();
			ValidateUS_PassportType();
		}

		protected override void CheckUS_ExpirationDateIsValidZDateTimeRange()
		{
			if (!Parent.US_ExpirationDate.IsEmpty)
			{
				TypeValidation.CheckValidZDateTimeRange(Parent.US_ExpirationDateInfo, new TypeValidationLimits()
				{
					FutureYearsBeforeError = 10,
					FutureYearsBeforeWarning = 100,
					PastYearsBeforeError = 100,
					PastYearsBeforeWarning = 100
				});

				if (Parent.US_ExpirationDate < ZDateTime.Today)
				{
					Parent.US_ExpirationDateInfo.AddWarning(ExpriationDateEarlierThanToday);
				}
			}
		}
		internal const string ExpriationDateEarlierThanToday = "Expiration date entered is in the past";

		protected override void CheckUS_ExpirationDate()
		{
			base.CheckUS_ExpirationDate();

			if (!Parent.US_PassportNo.IsEmpty && Parent.US_ExpirationDate.IsEmpty)
			{
				Parent.US_ExpirationDateInfo.AddMessageError(ExpirationDateRequired);
			}
		}
		internal const string ExpirationDateRequired = "Expiration Date is required when passport number is entered.";

		protected override void CheckUS_CountryOfIssuance()
		{
			base.CheckUS_CountryOfIssuance();

			if (!Parent.US_PassportNo.IsEmpty && Parent.US_CountryOfIssuance.IsEmpty)
			{
				Parent.US_CountryOfIssuanceInfo.AddMessageError(CountryOfIssuanceRequired);
			}
		}
		internal const string CountryOfIssuanceRequired = "Country of Issuance is required when passport number is entered.";

		protected override void CheckUS_PassportType()
		{
			base.CheckUS_PassportType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PassportTypeInfo, Parent.Lookups.PassportTypeList);

			if (!Parent.US_PassportNo.IsEmpty && Parent.US_PassportType.IsEmpty)
			{
				Parent.US_PassportTypeInfo.AddMessageError(PassportTypeRequired);
			}
		}
		internal const string PassportTypeRequired = "Passport Type is required when passport number is entered.";

		protected override void CheckUS_PhoneNumber()
		{
			base.CheckUS_PhoneNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PhoneNumberInfo);

			if (!Parent.US_PhoneNumber.IsEmpty && !Parent.US_PhoneNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.US_PhoneNumberInfo.AddMessageError(InvalidPhoneNumberFormat);
			}
		}
		internal const string InvalidPhoneNumberFormat = "The phone number format is invalid, should be only numerics.";

		protected override void CheckUS_Email()
		{
			base.CheckUS_Email();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EmailInfo);

			if (!Parent.US_Email.IsEmpty && !EmailAddressValidation.IsEmailAddressValid(Parent.US_Email))
			{
				Parent.US_EmailInfo.AddWarning(ImporterEmailInvalidFormat);
			}
		}
		internal const string ImporterEmailInvalidFormat = "The email address format is invalid";
	}
}
