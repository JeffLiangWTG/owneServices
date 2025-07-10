//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbPersonValidation
//
//    This class should be used for overriding validation in AutoGlbPersonValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonValidation : AutoGlbPersonValidation
	{
		public GlbPersonValidation(AutoGlbPerson parent) : base(parent)
		{
		}

		protected override void CheckPER_BirthDateIsValidZDateRange()
		{
			if (Parent.PER_BirthDate > ZDate.Today)
			{
				Parent.PER_BirthDateInfo.AddError(Res.GetString("f898cdd4-a33b-4d00-9d5b-525330517d61", "Birthdate cannot be in the future."));
			}
		}

		protected override void CheckPER_DriversLicenseNumber()
		{
			if (Parent.ViewEditRights.ViewDriversLicenseNumberForPerson)
			{
				base.CheckPER_DriversLicenseNumber();

				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.PER_DriversLicenseNumberInfo.AddError(MinimumInformationString);
				}

				if (Parent.PER_RN_NKNationalityCodeISO == Core.Constants.CountryCodes.SouthAfrica && !Parent.PER_DriversLicenseNumber.IsEmpty)
				{
					new SouthAfricanIDNumberValidation().Validate(Parent.PER_DriversLicenseNumberInfo);
				}
			}
		}

		protected override void CheckPER_EmailAddress()
		{
			if (Parent.ViewEditRights.ViewEmailForPerson)
			{
				base.CheckPER_EmailAddress();

				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.PER_EmailAddressInfo.AddError(MinimumInformationString);
				}

				EmailAddressValidation.ValidateEmailAddress(Parent.PER_EmailAddressInfo);
				CheckDuplicateActiveWebUserEmails(Parent.PER_EmailAddressInfo);

				if (Parent.PER_WebAccessEnabled && string.IsNullOrWhiteSpace(Parent.PER_EmailAddressInternal))
				{
					Parent.PER_EmailAddressInfo.AddError(Res.GetString("eccb8f25-a33a-4f86-ae9d-bb8d97bbb7dc", "Email is required when web access is enabled."));
				}
			}
		}

		protected override void CheckPER_FaxNumber()
		{
			base.CheckPER_FaxNumber();
			PhoneNumberFormatterAndValidator.Validate(Parent.PER_FaxNumberInfo, null, null, Parent.PER_RN_NKCountry);
		}

		protected override void CheckPER_HomePhone()
		{
			if (Parent.ViewEditRights.ViewHomePhoneForPerson)
			{
				base.CheckPER_HomePhone();
				PhoneNumberFormatterAndValidator.Validate(Parent.PER_HomePhoneInfo, null, null, Parent.PER_RN_NKCountry);
			}
		}

		protected override void CheckPER_MobilePhone()
		{
			if (Parent.ViewEditRights.ViewMobileForPerson)
			{
				base.CheckPER_MobilePhone();

				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.PER_MobilePhoneInfo.AddError(MinimumInformationString);
				}

				PhoneNumberFormatterAndValidator.Validate(Parent.PER_MobilePhoneInfo, null, null, Parent.PER_RN_NKCountry);
			}
		}

		protected override void CheckPER_Passport()
		{
			if (Parent.ViewEditRights.ViewPassportForPerson)
			{
				base.CheckPER_Passport();

				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.PER_PassportInfo.AddError(MinimumInformationString);
				}
			}
		}

		protected override void CheckPER_PassportPlaceOfIssue()
		{
			if (Parent.ViewEditRights.ViewPassportForPerson)
			{
				base.CheckPER_PassportPlaceOfIssue();
				ListValidation.ErrorIfInvalidCode(Parent.PER_PassportPlaceOfIssueInfo);
			}
		}

		protected override void CheckPER_FullName()
		{
			base.CheckPER_FullName();
			MandatoryValidation.CheckEntered(Parent.PER_FullNameInfo);
		}

		protected override void CheckPER_Gender()
		{
			if (Parent.ViewEditRights.ViewGenderForPerson)
			{
				base.CheckPER_Gender();
				ListValidation.ErrorIfInvalidCode(Parent.PER_GenderInfo);
			}
		}

		protected override void CheckPER_RN_NKNationalityCodeISO()
		{
			if (Parent.ViewEditRights.ViewNationalityForPerson)
			{
				base.CheckPER_RN_NKNationalityCodeISO();
				ListValidation.ErrorIfInvalidCode(Parent.PER_RN_NKNationalityCodeISOInfo);
			}
		}

		protected override void CheckPER_RN_NKCountry()
		{
			if (Parent.ViewEditRights.ViewHomeAddressForPerson)
			{
				base.CheckPER_RN_NKCountry();
				ListValidation.ErrorIfInvalidCode(Parent.PER_RN_NKCountryInfo);
			}
		}

		protected override void CheckPER_State()
		{
			if (Parent.ViewEditRights.ViewHomeAddressForPerson)
			{
				base.CheckPER_State();
				ListValidation.ErrorIfInvalidCode(Parent.PER_StateInfo);
			}
		}

		protected override void CheckPER_PassportExpiryDateIsValidZDateRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.PER_PassportExpiryDateInfo, false);
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());

		public void ValidatePER_MobilePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.PER_MobilePhone_FormattedInfo);
		}

		protected virtual void CheckPER_MobilePhone_Formatted()
		{
			if (Parent.ViewEditRights.ViewMobileForPerson)
			{
				if (ShouldAddStandalonePersonMinimumInformationError)
				{
					Parent.PER_MobilePhone_FormattedInfo.AddError(MinimumInformationString);
				}

				ValidatePhoneNumber(Parent.PER_MobilePhone_FormattedInfo, Parent.PER_MobilePhoneInfo,
					Parent.PER_MobilePhone_IsManuallyVerifiedInfo);
			}
		}

		public void ValidatePER_HomePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.PER_HomePhone_FormattedInfo);
		}

		protected virtual void CheckPER_HomePhone_Formatted()
		{
			if (Parent.ViewEditRights.ViewHomePhoneForPerson)
			{
				ValidatePhoneNumber(Parent.PER_HomePhone_FormattedInfo, Parent.PER_HomePhoneInfo,
					Parent.PER_HomePhone_IsManuallyVerifiedInfo);
			}
		}

		public void ValidatePER_FaxNum_Formatted()
		{
			ValidateCalculatedProperty(Parent.PER_FaxNum_FormattedInfo);
		}

		protected virtual void CheckPER_FaxNum_Formatted()
		{
			ValidatePhoneNumber(Parent.PER_FaxNum_FormattedInfo, Parent.PER_FaxNumberInfo, Parent.PER_FaxNum_IsManuallyVerifiedInfo);
		}

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, Parent.DefaultCountryCodeForPhoneNumbers);
		}

		public void ValidatePrimaryRelationship()
		{
			Parent.RemoveRowError(EnterPrimaryWorkplaceErrorMessage);

			if ((Parent.ContactCollection.Count > 0 || Parent.StaffCollection.Count > 0) && Parent.PrimaryRelationship == null && !Parent.IsMovingFromAnotherPerson)
			{
				Parent.AddRowError(EnterPrimaryWorkplaceErrorMessage);
			}
		}

		static string EnterPrimaryWorkplaceErrorMessage => Res.GetString("28b86120-8689-4d78-9051-5a20dfd2df73", "Please set the person's primary workplace.");

		protected new GlbPerson Parent
		{
			get { return (GlbPerson)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePER_HomePhone_Formatted();
			ValidatePER_MobilePhone_Formatted();
			ValidatePER_FaxNum_Formatted();
			ValidatePrimaryRelationship();
		}

		#region Web Security

		protected override void CheckPER_WebAccessEnabled()
		{
			base.CheckPER_WebAccessEnabled();
			CheckDuplicateActiveWebUserEmails(Parent.PER_WebAccessEnabledInfo);
		}

		protected override void CheckPER_IsActive()
		{
			base.CheckPER_IsActive();
			CheckDuplicateActiveWebUserEmails(Parent.PER_IsActiveInfo);
		}

		void CheckDuplicateActiveWebUserEmails(ZPropertyInfo info)
		{
			if ((!Parent.IsInDatabase || info.HasChanges) && Parent.PER_IsActive && Parent.PER_WebAccessEnabled && !string.IsNullOrWhiteSpace(Parent.PER_EmailAddressInternal))
			{
				if (DuplicateEmailCheckCache.GetOrAdd(Parent.PER_EmailAddressInternal, CheckDuplicateEmail))
				{
					info.AddError(Res.GetString("b40c86a9-6bb7-4081-a7e8-c11e7eba3b6b", "This email is already in use by another active Person with web access."));
				}
			}
		}

		bool CheckDuplicateEmail()
		{
			var query = new ZQuery(GlbPersonSchema.PER_IsActive, true);
			query.AddToFilter(GlbPersonSchema.PER_WebAccessEnabled, true);
			query.AddToFilter(GlbPersonSchema.PER_EmailAddress, SQLComparisonOperator.Equal, Parent.PER_EmailAddressInternal);
			query.AddToFilter(GlbPersonSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return Parent.Factory.ExistsInDatabase(GlbPersonSchema.Constants.TableName, query);
		}

		readonly Dictionary<ZString, bool> DuplicateEmailCheckCache = new Dictionary<ZString, bool>();

		#endregion

		bool ShouldAddStandalonePersonMinimumInformationError => !Parent.HasAssociation && Parent.PER_EmailAddress.IsEmpty && Parent.PER_MobilePhone.IsEmpty && Parent.PER_DriversLicenseNumber.IsEmpty && Parent.PER_Passport.IsEmpty;

		protected string MinimumInformationString => Res.GetString("fce43a77-fe73-4f92-8830-d90fd2d487da",
			"Please enter at least a driver's license number, email address, mobile phone number or passport number.");
	}
}
