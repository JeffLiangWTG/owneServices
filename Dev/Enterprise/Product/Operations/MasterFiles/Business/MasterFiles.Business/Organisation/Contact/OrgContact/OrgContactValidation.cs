using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngineCore;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactValidation : AutoOrgContactValidation
	{
		public OrgContactValidation(AutoOrgContact parent)
			: base(parent)
		{
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
		}

		#region Contact Name

		protected override void CheckOC_ContactName()
		{
			base.CheckOC_ContactName();
			var contact = Parent;
			var info = contact.OC_ContactNameInfo;
			MandatoryValidation.CheckEntered(info);

			if (contact.Header != null)
			{
				if (!contact.Header.HasUniqueContactName(contact))
				{
					info.AddError(Res.GetString("c68cd9f4-f017-468e-b7da-d017dad26db9", "The contact name must be unique within each organization."));
				}
			}

			CheckEnglishCharactersForEnglishContacts(info);

			if (contact.IsPersonLoaded && (contact.Person?.IsDuplicateFound ?? false))
			{
				info.AddWarningWithoutValidationCheck(Res.GetString("e0ae795e-2a96-4aec-a29c-1284ffc60b23", "The contact details you have entered resulted in potential duplicates. Please confirm that they are actual duplicates."));
			}
		}

		#endregion

		#region Notify Mode

		protected override void CheckOC_NotifyMode()
		{
			base.CheckOC_NotifyMode();
			MandatoryValidation.CheckEntered(Parent.OC_NotifyModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OC_NotifyModeInfo);
		}

		#endregion

		#region Attachment Type

		protected override void CheckOC_AttachmentType()
		{
			base.CheckOC_AttachmentType();
			if (DeliveryMethodHelper.IsEmailOrEPrint(Parent.OC_NotifyMode))
			{
				MandatoryValidation.CheckEntered(Parent.OC_AttachmentTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OC_AttachmentTypeInfo);
			}
		}

		#endregion

		#region Birthday

		protected override void CheckOC_Birthday()
		{
			if (!((Parent.OC_BirthdayInfo.IsNullable && Parent.OC_BirthdayInfo.Value.IsEmpty) || Parent.OC_BirthdayInfo.Value.IsValid))
			{
				Parent.OC_BirthdayInfo.AddError(Res.GetString("33eb9d25-b490-4ece-b6fb-fe20272d23e5", "Please enter a valid Birthday."));
			}
		}

		protected override void CheckOC_BirthdayIsValidZDateTimeRange()
		{
			if (Parent.OC_Birthday > ZDateTime.Now)
			{
				Parent.OC_BirthdayInfo.AddError(Res.GetString("f898cdd4-a33b-4d00-9d5b-525330517d61", "Birthdate cannot be in the future."));
			}
		}

		#endregion

		#region Phone Numbers

		#region Phone

		public void ValidateOC_Phone_Formatted()
		{
			ValidateCalculatedProperty(Parent.OC_Phone_FormattedInfo);
		}

		protected virtual void CheckOC_Phone_Formatted()
		{
			ValidatePhoneNumber(Parent.OC_Phone_FormattedInfo, Parent.OC_PhoneInfo, Parent.OC_Phone_IsManuallyVerifiedInfo);
		}

		#endregion

		#region Fax

		public void ValidateOC_Fax_Formatted()
		{
			ValidateCalculatedProperty(Parent.OC_Fax_FormattedInfo);
		}

		protected virtual void CheckOC_Fax_Formatted()
		{
			ValidatePhoneNumber(Parent.OC_Fax_FormattedInfo, Parent.OC_FaxInfo, Parent.OC_Fax_IsManuallyVerifiedInfo);
			// Document delivery check.
			Parent.RemoveRowError(EnterFaxForCorrespondenceErrorMessage);
			Parent.RemoveRowWarning(MainFaxForCorrespondenceWarningMessage);
			if (Parent.OC_Fax_Formatted.IsEmpty && Parent.ParentOrg != null && Parent.Documents.ContainsDocumentWithDeliveryMode(Constants.ContactNotifyModes.Fax))
			{
				if (Parent.ParentOrg.MainAddress.OA_Fax.IsEmpty)
				{
					Parent.AddRowError(EnterFaxForCorrespondenceErrorMessage);
				}
				else
				{
					Parent.AddRowWarning(MainFaxForCorrespondenceWarningMessage);
				}
			}
		}

		string EnterFaxForCorrespondenceErrorMessage
		{
			get { return Res.GetString("B6933A0F-412C-4e2e-9ED5-553BAD9B022F", "Please enter a fax number to be used for delivering documents to this Organization."); }
		}

		string MainFaxForCorrespondenceWarningMessage
		{
			get { return Res.GetString("5955F6DE-F290-499a-861F-223A5DB84946", "The fax number from the main details page of this organization will be used for delivering documents to this Organization."); }
		}

		#endregion

		#region Mobile

		public void ValidateOC_Mobile_Formatted()
		{
			ValidateCalculatedProperty(Parent.OC_Mobile_FormattedInfo);
		}

		protected virtual void CheckOC_Mobile_Formatted()
		{
			if (!Parent.OC_Mobile_Formatted.IsEmpty)
			{
				ValidatePhoneNumber(Parent.OC_Mobile_FormattedInfo, Parent.OC_MobileInfo, Parent.OC_Mobile_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region Pager

		public void ValidateOC_Pager_Formatted()
		{
			ValidateCalculatedProperty(Parent.OC_Pager_FormattedInfo);
		}

		protected virtual void CheckOC_Pager_Formatted()
		{
			ValidatePhoneNumber(Parent.OC_Pager_FormattedInfo, Parent.OC_PagerInfo, Parent.OC_Pager_IsManuallyVerifiedInfo);
		}

		#endregion

		#region HomePhone

		public void ValidateOC_HomePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.OC_HomePhone_FormattedInfo);
		}

		protected virtual void CheckOC_HomePhone_Formatted()
		{
			ValidatePhoneNumber(Parent.OC_HomePhone_FormattedInfo, Parent.OC_HomePhoneInfo, Parent.OC_HomePhone_IsManuallyVerifiedInfo);
		}

		#endregion

		#region OtherPhone

		public void ValidateOC_OtherPhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.OC_OtherPhone_FormattedInfo);
		}

		protected virtual void CheckOC_OtherPhone_Formatted()
		{
			ValidatePhoneNumber(Parent.OC_OtherPhone_FormattedInfo, Parent.OC_OtherPhoneInfo, Parent.OC_OtherPhone_IsManuallyVerifiedInfo);
		}

		#endregion

		#endregion

		#region Email

		protected override void CheckOC_Email()
		{
			base.CheckOC_Email();

			EmailContactItemValidationHelper.ValidateEmailAddress(Parent.OC_EmailInfo);

			Parent.RemoveRowError(EnterEmailAddressForCorrespondenceErrorMessage);
			Parent.RemoveRowWarning(MainEmailAddressForCorrespondenceWarningMessage);
			Parent.RemoveRowError(MainEmailAddressForCorrespondenceErrorInSubscriptions);

			if (
				Parent.OC_Email.IsEmpty &&
				Parent.ParentOrg != null &&
				Parent.Documents.ContainsDocumentWithDeliveryMode(Constants.ContactNotifyModes.Email) // check last since it is the most expensive
				)
			{
				if (Parent.ParentOrg.MainAddress.OA_Email.IsEmpty)
				{
					Parent.AddRowError(EnterEmailAddressForCorrespondenceErrorMessage);
				}
				else
				{
					Parent.AddRowWarning(MainEmailAddressForCorrespondenceWarningMessage);
				}
			}

			if (Parent.SubscriptionsLoadedAndNotEmpty)
			{
				Parent.Subscriptions.ValidateAllMembers();
				if (!Parent.OC_Email.IsEmpty && Parent.Subscriptions.HasErrors())
				{
					Parent.AddRowError(MainEmailAddressForCorrespondenceErrorInSubscriptions);
					Parent.OC_EmailInfo.AddError(MainEmailAddressForCorrespondenceErrorInSubscriptions);
				}
			}

			if (Parent.IsInDatabase && string.IsNullOrEmpty(Parent.OC_Email) && OrgContactIsTheScheduleTaskRecipient(Parent.PK))
			{
				Parent.OC_EmailInfo.AddError(CantSetEmailToEmptyErrorMessage);
			}

			if (Parent.OC_EmailInfo.HasChanges && Parent.OC_Email.IsEmpty)
			{
				var query = new ZQuery(WorkRequestSchema.WKR_OC_Client, Parent.PK);

				if (Parent.Factory.ExistsInDatabase(WorkRequestSchema.Constants.TableName, query))
				{
					Parent.OC_EmailInfo.AddError(CantSetEmailToEmptyForJobConversationParticipantErrorMessage);
				}
			}

			CheckDuplicateActiveWebUserEmails(Parent.OC_EmailInfo);
		}

		static string EnterEmailAddressForCorrespondenceErrorMessage => Res.GetString("c2cf2adc-da5e-4d64-ac21-a7974f0c490a", "Please enter an email address to be used for correspondence with this Organization.");
		static string MainEmailAddressForCorrespondenceWarningMessage => Res.GetString("ea11abb9-7bb3-44e9-80c2-93337b0e9f7d", "The main email address from the details page of this organization will be used for correspondence with this Organization.");
		static string MainEmailAddressForCorrespondenceErrorInSubscriptions => Res.GetString("cb5e5bd4-98a7-4fd5-b442-d9b077c11007", "You cannot change main e-mail address with errors in Subscriptions.");
		static string CantSetEmailToEmptyErrorMessage => Res.GetString("9FF5B59F-94FF-4A01-B74A-DF2AC5D0CFF1", "Email cannot set to empty because contact is assigned to recipient of scheduled reports.");
		static string CantSetEmailToEmptyForJobConversationParticipantErrorMessage => Res.GetString("E5616F11-CC14-4F37-91F1-3E0A063A93F2", "Setting an empty email on this client will prevent them from receiving eConversation notifications for Customer Service Tickets on which they are selected as the Client.");
		static string AttemptToUpdatePasswordInfoFieldWithoutUpdatingOtherHashInfoFieldsErrorMessage => Res.GetString("2F74F5EC-BD14-43FC-812E-D1996773A075", "Attempt to update a password info field without updating the hashing info fields");

		EmailContactItemValidationHelper EmailContactItemValidationHelper
		{
			get { return emailContactItemValidationHelper ?? (emailContactItemValidationHelper = new EmailContactItemValidationHelper(Parent)); }
		}
		EmailContactItemValidationHelper emailContactItemValidationHelper;

		bool OrgContactIsTheScheduleTaskRecipient(ZGuid pk)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryMethod, Constants.ContactNotifyModes.Email);
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryToType, ScheduledReportDeliveryRecipientConstants.RecipientType.Contact);
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_FaxOverride, "");
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_OC, pk);

			var taskRecipient = Parent.Factory.Load(ObjectFactory.GetType<IStmScheduleTaskRecipient>(), query);

			return taskRecipient.Any();
		}

		#endregion

		#region Language

		protected override void CheckOC_Language()
		{
			base.CheckOC_Language();
			MandatoryValidation.CheckEntered(Parent.OC_LanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OC_LanguageInfo);
		}

		#endregion

		#region Date Joined Company / Industry

		protected const int YearJoinedPastMaxYears = 50;
		protected const int YearJoinedFutureMaxMonths = 6;

		public static string CannotHaveYearJoined50YearsInPast
		{
			get { return Res.GetString("19816bc3-0ec2-46b1-a8d9-887b903194dc", "Year Joined Company can be at most 50 years prior to today's date."); }
		}

		public static string CannotHaveYearJoined6MonthsInFuture
		{
			get { return Res.GetString("a27c8663-5e55-489a-9f05-9732e7c3a5cf", "Year Joined Company cannot be set to more than 6 months from today's date."); }
		}

		#region Date Joined Company

		protected override void CheckOC_YearJoinedCompanyIsValidZDateTimeRange()
		{
			if (Parent.OC_YearJoinedCompany < ZDateTime.Today.AddYears(-YearJoinedPastMaxYears))
			{
				Parent.OC_YearJoinedCompanyInfo.AddError(CannotHaveYearJoined50YearsInPast);
			}
			else if (Parent.OC_YearJoinedCompany > ZDateTime.Today.AddMonths(YearJoinedFutureMaxMonths))
			{
				Parent.OC_YearJoinedCompanyInfo.AddError(CannotHaveYearJoined6MonthsInFuture);
			}
		}

		#endregion

		#region Date Joined Industry

		protected override void CheckOC_YearJoinedIndustryIsValidZDateTimeRange()
		{
			if (Parent.OC_YearJoinedIndustry < ZDateTime.Today.AddYears(-YearJoinedPastMaxYears))
			{
				Parent.OC_YearJoinedIndustryInfo.AddError(CannotHaveYearJoined50YearsInPast);
			}
			else if (Parent.OC_YearJoinedIndustry > ZDateTime.Today.AddMonths(YearJoinedFutureMaxMonths))
			{
				Parent.OC_YearJoinedIndustryInfo.AddError(CannotHaveYearJoined6MonthsInFuture);
			}
		}

		#endregion

		#endregion

		#region Job Category

		protected override void CheckOC_JobCategory()
		{
			base.CheckOC_JobCategory();

			if (Parent != null)
			{
				if (OrganisationsDataRegistry.Instance.ContactJobCategoryMandatory)
				{
					MandatoryValidation.CheckEntered(Parent.JobCategoryDescriptionInfo);
				}

				ListValidation.WarnIfInvalidCode(Parent.JobCategoryDescriptionInfo, Parent.Lookups.JobCategory_List, (IMultilingualString)ResString.GetMultilingualString("16e6fcc9-2c54-48b6-b86d-8b9413e2dcdd", "Chosen Job Category is not defined in the Registry. Please define it or use the an Operation Action to change all Organization Contacts with this category to valid one."));
			}
		}

		#endregion

		#region Contact Source

		protected override void CheckOC_ContactSource()
		{
			base.CheckOC_ContactSource();
			ListValidation.ErrorIfInvalidCode(Parent.OC_ContactSourceInfo);
		}

		#endregion

		#region Web Access

		protected override void CheckOC_WebAccessEnabled()
		{
			base.CheckOC_WebAccessEnabled();
			CheckDuplicateActiveWebUserEmails(Parent.OC_WebAccessEnabledInfo);
		}

		void CheckDuplicateActiveWebUserEmails(ZPropertyInfo info)
		{
			if ((!Parent.IsInDatabase || info.HasChanges) && Parent.OC_IsActive && Parent.OC_WebAccessEnabled && !string.IsNullOrWhiteSpace(Parent.OC_Email))
			{
				var contacts = Parent.ParentOrg.ContactsLoaded;

				if (contacts.Any(c => c.OC_Email == Parent.OC_Email && c.PK != Parent.PK && c.OC_IsActive && c.OC_WebAccessEnabled)
					|| (!contacts.Any() && DuplicateEmailCheckCache.GetOrAdd(Parent.OC_Email, CheckDuplicateEmail)))
				{
					info.AddError(Res.GetString("afc6b3de-3b22-4969-a80d-bcb68448f559", "This email is already in use by another active Contact with web access."));
				}
			}
		}

		bool CheckDuplicateEmail()
		{
			var query = new ZQuery(OrgContactSchema.OC_IsActive, true);
			query.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
			query.AddToFilter(OrgContactSchema.OC_OH, Parent.OC_OH);
			query.AddToFilter(OrgContactSchema.OC_Email, SQLComparisonOperator.Equal, Parent.OC_Email);
			query.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return Parent.Factory.ExistsInDatabase(OrgContactSchema.Constants.TableName, query);
		}

		readonly Dictionary<ZString, bool> DuplicateEmailCheckCache = new Dictionary<ZString, bool>();

		#endregion

		#region Validate index on Subscriptions table

		void ValidateSubscriptions()
		{
			Parent.Subscriptions.ValidateAllMembers();
		}

		#endregion

		#region IsActive

		protected override void CheckOC_IsActive()
		{
			base.CheckOC_IsActive();

			CheckInactiveAddressWhenContactIsActive(Parent.OC_IsActiveInfo);

			if (!Parent.OC_IsActive && Parent.Allocations.Cast<OrgContactAllocation>().Any(a => a.PC_Type == OrgConstants.ContactAllocationType.CAPGA || a.PC_Type == OrgConstants.ContactAllocationType.USPGA || a.PC_Type == OrgConstants.ContactAllocationType.USFSV))
			{
				Parent.OC_IsActiveInfo.AddError(Res.GetString("42c27ea6-e6c5-4c4e-b90f-8e42b3b2f805", "You cannot De-activate this contact because it is used in the allocated contacts as a PGA or USF Contact."));
			}

			CheckDuplicateActiveWebUserEmails(Parent.OC_IsActiveInfo);

			if (Parent.WebAccessSuperseded && Parent.OC_IsActive)
			{
				Parent.OC_IsActiveInfo.AddError(Res.GetString("c9e7a102-8bca-4fb9-a853-4b30ca02f71d", "Contacts with their Web Access Superseded cannot be Active."));
			}
		}

		protected virtual void CheckInactiveAddressWhenContactIsActive(ZPropertyInfo propertyInfo)
		{
			if (Parent.OrgAddress != null && !Parent.OrgAddress.OA_IsActive && Parent.OC_IsActive)
			{
				propertyInfo.AddError(Res.GetString("d3ee96ef-27a5-44c9-b8fd-9a4a72ba0f85", "Only Active address can be set as Contact's address"));
			}
		}

		#endregion

		#region Gender

		protected override void CheckOC_Gender()
		{
			if (Parent.ViewPersonalInformationAllowed)
			{
				base.CheckOC_Gender();
				ListValidation.ErrorIfInvalidCode(Parent.OC_GenderInfo);
			}
		}

		protected override void CheckOC_GenderIsWesternEuropean()
		{
			if (Parent.ViewPersonalInformationAllowed)
			{
				base.CheckOC_GenderIsWesternEuropean();
				ListValidation.ErrorIfInvalidCode(Parent.OC_GenderInfo);
			}
		}

		#endregion

		#region Nationality

		protected override void CheckOC_RN_NKNationality()
		{
			if (Parent.ViewPersonalInformationAllowed)
			{
				base.CheckOC_RN_NKNationality();
				ListValidation.ErrorIfInvalidCode(Parent.OC_RN_NKNationalityInfo);
			}
		}

		protected override void CheckOC_RN_NKNationalityIsWesternEuropean()
		{
			if (Parent.ViewPersonalInformationAllowed)
			{
				base.CheckOC_RN_NKNationalityIsWesternEuropean();
				ListValidation.ErrorIfInvalidCode(Parent.OC_RN_NKNationalityInfo);
			}
		}

		#endregion

		#region OC_PER

		protected override void CheckOC_PERIsNotEmpty()
		{
			if (Parent.IsInDatabase)
			{
				base.CheckOC_PERIsNotEmpty();
			}
		}

		protected override void CheckOC_PERIsValidZGuid()
		{
			if (Parent.IsInDatabase)
			{
				base.CheckOC_PERIsValidZGuid();
			}
		}

		#endregion

		#region Password

		protected override void CheckOC_PasswordHash()
		{
			base.CheckOC_PasswordHash();
			if (!IsPasswordFieldsChangeValid())
			{
				Parent.OC_PasswordHashInfo.AddError(Res.GetString("B88B0BE2-5187-496E-A1ED-73B1028D6939", "Attempt to update a password hash field without updating the hashing info fields"));
			}
		}

		protected override void CheckOC_PasswordSalt()
		{
			base.CheckOC_PasswordSalt();
			if (!IsPasswordFieldsChangeValid())
			{
				Parent.OC_PasswordSaltInfo.AddError(AttemptToUpdatePasswordInfoFieldWithoutUpdatingOtherHashInfoFieldsErrorMessage);
			}
		}

		protected override void CheckOC_PasswordHashIterations()
		{
			base.CheckOC_PasswordHashIterations();
			if (!IsPasswordFieldsChangeValid())
			{
				Parent.OC_PasswordHashIterationsInfo.AddError(AttemptToUpdatePasswordInfoFieldWithoutUpdatingOtherHashInfoFieldsErrorMessage);
			}
		}

		#endregion

		#region Implementation

		#region English Validation

		void CheckEnglishCharactersForEnglishContacts(ZPropertyInfo info)
		{
			if (Parent.IsEnglish)
			{
				if (info.HasChanges || !Parent.IsInDatabase)
				{
					EnglishCharactersValidation.ErrorIfNotWesternEuropean(info);
				}
				else
				{
					EnglishCharactersValidation.WarnIfNotWesternEuropean(info);
				}
			}
		}

		#endregion

		#region Phone Number Validation

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, Parent.DefaultCountryCodeForPhoneNumbers);
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		#endregion

		bool IsPasswordFieldsChangeValid() => !(
			(Parent.OC_PasswordHashInfo.HasChanges || Parent.OC_PasswordSaltInfo.HasChanges || Parent.OC_PasswordHashIterationsInfo.HasChanges) &&
			!(Parent.OC_PasswordHashInfo.HasChanges && Parent.OC_PasswordSaltInfo.HasChanges && Parent.OC_PasswordHashIterationsInfo.HasChanges)
		);

		new OrgContact Parent
		{
			get { return (OrgContact)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOC_Phone_Formatted();
			ValidateOC_Mobile_Formatted();
			ValidateOC_Fax_Formatted();
			ValidateOC_HomePhone_Formatted();
			ValidateOC_Pager_Formatted();
			ValidateOC_OtherPhone_Formatted();
			ValidateSubscriptions();
		}

		#endregion

		protected override void CheckOC_OA_OrgAddress()
		{
			base.CheckOC_OA_OrgAddress();

			CheckInactiveAddressWhenContactIsActive(Parent.OC_OA_OrgAddressInfo);
		}
	}
}
