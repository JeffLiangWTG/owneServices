using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicantValidation : AutoHRJobApplicantValidation
	{
		public HRJobApplicantValidation(AutoHRJobApplicant parent) : base(parent)
		{
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
			ZValidationInternals = this;
		}

		#region Availability

		protected override void CheckHA_Availability()
		{
			base.CheckHA_Availability();
			ListValidation.ErrorIfInvalidCode(Parent.HA_AvailabilityInfo, Parent.Lookups.Availabilities);
		}

		#endregion

		#region Work Permit Status

		protected override void CheckHA_WorkPermitStatus()
		{
			base.CheckHA_WorkPermitStatus();
			ListValidation.ErrorIfInvalidCode(Parent.HA_WorkPermitStatusInfo, Parent.Lookups.WorkPermitStatuses);
		}

		#endregion

		#region Current Wage Currency

		protected override void CheckHA_RX_NKCurrentWageCurrency()
		{
			base.CheckHA_RX_NKCurrentWageCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.HA_RX_NKCurrentWageCurrencyInfo, Parent.Lookups.CurrentWageCurrencies);
		}

		#endregion

		#region Wage Expectation Currency

		protected override void CheckHA_RX_NKWageExpectationCurrency()
		{
			base.CheckHA_RX_NKWageExpectationCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.HA_RX_NKWageExpectationCurrencyInfo, Parent.Lookups.CurrentWageCurrencies);
		}

		#endregion

		#region Email Address

		protected override void CheckHA_EmailAddress()
		{
			base.CheckHA_EmailAddress();
			MandatoryValidation.CheckEntered(Parent.HA_EmailAddressInfo);
			if (!EmailAddressValidation.IsEmailAddressValid(Parent.HA_EmailAddress))
			{
				Parent.HA_EmailAddressInfo.AddError(Res.GetString("e57118ee-9490-47b5-acde-ab7fb80a9233", "{0} is not a valid Email Address.", Parent.HA_EmailAddress));
			}
			CheckUniqueEmailAddress();
		}

		void CheckUniqueEmailAddress()
		{
			if (!Parent.HA_EmailAddress.IsEmpty)
			{
				ZQuery query = new ZQuery(HRJobApplicantSchema.HA_EmailAddress, Parent.HA_EmailAddress);
				query.AddToFilter(HRJobApplicantSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				HRJobApplicant[] otherApplicants = Parent.Factory.Load<HRJobApplicant>(query);
				if (otherApplicants.Length > 0)
				{
					Parent.HA_EmailAddressInfo.AddError(Res.GetString("48c376fe-e9ef-4a0f-aa24-437ab9ba139b", "{0} is already associated with another registered Account.", Parent.HA_EmailAddress));
				}
			}
		}

		#endregion

		#region Phone Numbers

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, Parent.DefaultCountryCodeForPhoneNumbers);
		}

		public void ValidateHA_WorkPhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.HA_WorkPhone_FormattedInfo);
		}

		protected virtual void CheckHA_WorkPhone_Formatted()
		{
			ValidatePhoneNumber(Parent.HA_WorkPhone_FormattedInfo, Parent.HA_WorkPhoneInfo, Parent.HA_WorkPhone_IsManuallyVerifiedInfo);
		}

		public void ValidateHA_MobilePhone()
		{
			ValidateCalculatedProperty(Parent.HA_MobilePhoneInfo);
			ZValidationInternals.Validate(Parent.HA_MobilePhoneInfo, GetHA_MobilePhoneValidationInvoker());
		}

		RunValidationInvoker GetHA_MobilePhoneValidationInvoker()
		{
			return delegate
			{
				CheckHA_MobilePhoneIsWesternEuropean();
				CheckHA_MobilePhone();
			};
		}

		protected virtual void CheckHA_MobilePhoneIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.HA_MobilePhoneInfo);
		}

		protected virtual void CheckHA_MobilePhone()
		{
			if (Parent.ViewEditRights.ViewMobilePhoneForJobApplicant)
			{
				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.HA_MobilePhoneInfo.AddError(MinimumInformationString);
				}

				PhoneNumberFormatterAndValidator.Validate(Parent.HA_MobilePhoneInfo, null, null, Parent.HA_RN_NKCountry);
			}
		}

		public void ValidateHA_MobilePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.HA_MobilePhone_FormattedInfo);
		}

		protected virtual void CheckHA_MobilePhone_Formatted()
		{
			if (Parent.ViewEditRights.ViewMobilePhoneForJobApplicant)
			{
				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.HA_MobilePhone_FormattedInfo.AddError(MinimumInformationString);
				}

				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.HA_MobilePhone_FormattedInfo);
				ValidatePhoneNumber(Parent.HA_MobilePhone_FormattedInfo, Parent.HA_MobilePhoneInfo, Parent.HA_MobilePhone_IsManuallyVerifiedInfo);
			}
		}

		public void ValidateHA_HomePhone()
		{
			ValidateCalculatedProperty(Parent.HA_HomePhoneInfo);
			ZValidationInternals.Validate(Parent.HA_HomePhoneInfo, GetHA_HomePhoneValidationInvoker());
		}

		RunValidationInvoker GetHA_HomePhoneValidationInvoker()
		{
			return delegate
			{
				CheckHA_HomePhoneIsWesternEuropean();
				CheckHA_HomePhone();
			};
		}

		protected virtual void CheckHA_HomePhoneIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.HA_HomePhoneInfo);
		}

		protected virtual void CheckHA_HomePhone()
		{
			if (Parent.ViewEditRights.ViewHomePhoneForJobApplicant)
			{
				PhoneNumberFormatterAndValidator.Validate(Parent.HA_HomePhoneInfo, null, null, Parent.HA_RN_NKCountry);
			}
		}

		public void ValidateHA_HomePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.HA_HomePhone_FormattedInfo);
		}

		protected virtual void CheckHA_HomePhone_Formatted()
		{
			if (Parent.ViewEditRights.ViewHomePhoneForJobApplicant)
			{
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.HA_HomePhone_FormattedInfo);
				ValidatePhoneNumber(Parent.HA_HomePhone_FormattedInfo, Parent.HA_HomePhoneInfo, Parent.HA_HomePhone_IsManuallyVerifiedInfo);
			}
		}

		public void ValidateHA_FaxNum()
		{
			ValidateCalculatedProperty(Parent.HA_FaxNumInfo);
			ZValidationInternals.Validate(Parent.HA_MobilePhoneInfo, GetHA_MobilePhoneValidationInvoker());
		}

		protected virtual void CheckHA_FaxNum()
		{
			if (Parent.ViewEditRights.ViewFaxForJobApplicant)
			{
				PhoneNumberFormatterAndValidator.Validate(Parent.HA_FaxNumInfo, null, null, Parent.HA_RN_NKCountry);
			}
		}

		public void ValidateHA_FaxNum_Formatted()
		{
			ValidateCalculatedProperty(Parent.HA_FaxNum_FormattedInfo);
		}

		protected virtual void CheckHA_FaxNum_Formatted()
		{
			if (Parent.ViewEditRights.ViewFaxForJobApplicant)
			{
				ValidatePhoneNumber(Parent.HA_FaxNum_FormattedInfo, Parent.HA_FaxNumInfo, Parent.HA_FaxNum_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region HA_PER

		protected override void CheckHA_PERIsNotEmpty()
		{
			if (Parent.IsInDatabase)
			{
				base.CheckHA_PERIsNotEmpty();
			}
		}

		protected override void CheckHA_PERIsValidZGuid()
		{
			if (Parent.IsInDatabase)
			{
				base.CheckHA_PERIsValidZGuid();
			}
		}

		#endregion

		#region Person Validations

		public void ValidateHA_Birthdate()
		{
			ValidateCalculatedProperty(Parent.HA_BirthdateInfo);
		}

		protected void CheckHA_BirthDateIsValidZDateRange()
		{
			if (Parent.HA_Birthdate > ZDate.Today)
			{
				Parent.HA_BirthdateInfo.AddError(Res.GetString("426A6AC3-A6A5-489D-B4F2-2B272AC4E4C4", "Birthdate cannot be in the future."));
			}
		}

		public void ValidateHA_Gender()
		{
			ValidateCalculatedProperty(Parent.HA_GenderInfo);
		}
		protected virtual void CheckHA_Gender()
		{
			if (Parent.ViewEditRights.ViewGenderForJobApplicant)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HA_GenderInfo);
			}
		}

		public void ValidateHA_NameSuffix()
		{
			ValidateCalculatedProperty(Parent.HA_NameSuffixInfo);
		}

		public void ValidateHA_FullName()
		{
			ValidateCalculatedProperty(Parent.HA_FullNameInfo);
		}

		protected virtual void CheckHA_FullName()
		{
			MandatoryValidation.CheckEntered(Parent.HA_FullNameInfo);
		}

		public void ValidateHA_Title()
		{
			ValidateCalculatedProperty(Parent.HA_TitleInfo);
		}

		public void ValidateHA_Passport()
		{
			ValidateCalculatedProperty(Parent.HA_PassportInfo);
		}

		protected virtual void CheckHA_Passport()
		{
			if (Parent.ViewEditRights.ViewPassportForJobApplicant)
			{
				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.HA_PassportInfo.AddError(MinimumInformationString);
				}
			}
		}

		public void ValidateHA_DriversLicenseNumber()
		{
			ValidateCalculatedProperty(Parent.HA_DriversLicenseNumberInfo);
		}

		protected virtual void CheckHA_DriversLicenseNumber()
		{
			if (Parent.ViewEditRights.ViewDriversLicenseNumberForJobApplicant)
			{
				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.HA_DriversLicenseNumberInfo.AddError(MinimumInformationString);
				}

				if (Parent.HA_RN_NKNationalityCodeISO == Core.Constants.CountryCodes.SouthAfrica && !Parent.HA_DriversLicenseNumber.IsEmpty)
				{
					new SouthAfricanIDNumberValidation().Validate(Parent.HA_DriversLicenseNumberInfo);
				}
			}
		}

		public void ValidateHA_RN_NKNationalityCodeISO()
		{
			ValidateCalculatedProperty(Parent.HA_RN_NKNationalityCodeISOInfo);
		}

		protected virtual void CheckHA_RN_NKNationalityCodeISO()
		{
			if (Parent.ViewEditRights.ViewNationalityForJobApplicant)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HA_RN_NKNationalityCodeISOInfo);
			}
		}

		public void ValidateHA_UserAddress1()
		{
			ValidateCalculatedProperty(Parent.HA_UserAddress1Info);
		}

		public void ValidateHA_UserAddress2()
		{
			ValidateCalculatedProperty(Parent.HA_UserAddress2Info);
		}

		public void ValidateHA_City()
		{
			ValidateCalculatedProperty(Parent.HA_CityInfo);
		}

		public void ValidateHA_State()
		{
			ValidateCalculatedProperty(Parent.HA_StateInfo);
		}

		protected virtual void CheckHA_State()
		{
			if (Parent.ViewEditRights.ViewUserAddressForJobApplicant)
			{
				ListValidation.WarnIfInvalidCode(Parent.HA_StateInfo);
			}
		}

		public void ValidateHA_RN_NKCountry()
		{
			ValidateCalculatedProperty(Parent.HA_RN_NKCountryInfo);
		}

		protected virtual void CheckHA_RN_NKCountry()
		{
			if (Parent.ViewEditRights.ViewUserAddressForJobApplicant)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HA_RN_NKCountryInfo);
			}
		}

		public void ValidateHA_Postcode()
		{
			ValidateCalculatedProperty(Parent.HA_PostcodeInfo);
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateHA_WorkPhone_Formatted();
			ValidateHA_MobilePhone_Formatted();
			ValidateHA_HomePhone_Formatted();
			ValidateHA_FaxNum_Formatted();
		}

		#endregion

		protected new HRJobApplicant Parent
		{
			get { return (HRJobApplicant)base.Parent; }
		}

		readonly IValidationInternals ZValidationInternals;

		bool ShouldAddStandalonePersonMinimumInformationError => Parent.HA_EmailAddress.IsEmpty && Parent.HA_MobilePhone.IsEmpty && Parent.HA_DriversLicenseNumber.IsEmpty && Parent.HA_Passport.IsEmpty;

		protected string MinimumInformationString => Res.GetString("BDAC84FB-271E-4D4D-B437-25EFE7BE1E69",
			"Please enter at least a driver's license number, email address, mobile phone number or passport number.");
	}
}
