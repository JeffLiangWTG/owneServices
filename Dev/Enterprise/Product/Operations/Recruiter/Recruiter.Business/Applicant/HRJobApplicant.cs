using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty(Schema.HA_FullName), DescriptionProperty(HRJobApplicantSchema.Constants.HA_EmailAddress)]
	public class HRJobApplicant : AutoHRJobApplicant,
		IHRJobApplicant,
		IPatternCleanup,
		IDocManagerSupport,
		IEDocsSecurity,
		IExamUrlRecipient,
		ICertificatesProvider,
		ISupportWebAddressValidation,
		IHaveRequiredDocuments
	{
		public new class Schema : AutoHRJobApplicant.Schema
		{
			public const string HA_FullName = "HA_FullName";
			public const string HA_City = "HA_City";
			public const string HA_State = "HA_State";
			public const string HA_NameSuffix = "HA_NameSuffix";
			public const string HA_Title = "HA_Title";
			public const string HA_HomePhone = "HA_HomePhone";
			public const string HA_Passport = "HA_Passport";
			public const string HA_DriversLicenseNumber = "HA_DriversLicenseNumber";
			public const string HA_RN_NKNationalityCodeISO = "HA_RN_NKNationalityCodeISO";
			public const string HA_Gender = "HA_Gender";
			public const string HA_Birthdate = "HA_Birthdate";
			public const string HA_MobilePhone = "HA_MobilePhone";
			public const string HA_RN_NKCountry = "HA_RN_NKCountry";
			public const string HA_UserAddress1 = "HA_UserAddress1";
			public const string HA_UserAddress2 = "HA_UserAddress2";
			public const string HA_Postcode = "HA_Postcode";
			public const string HA_FaxNum = "HA_FaxNum";
			public const string HA_FaxNum_Formatted = "HA_FaxNum_Formatted";
			public const string HA_FaxNum_FormattedLocalNumberIfLoggedInSameCountry = "HA_FaxNum_FormattedLocalNumberIfLoggedInSameCountry";
			public const string HA_FaxNum_IsManuallyVerified = "HA_FaxNum_IsManuallyVerified";
			public const string HA_HomePhone_Formatted = "HA_HomePhone_Formatted";
			public const string HA_HomePhone_FormattedLocalNumberIfLoggedInSameCountry = "HA_HomePhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string HA_HomePhone_IsManuallyVerified = "HA_HomePhone_IsManuallyVerified";
			public const string HA_MobilePhone_Formatted = "HA_MobilePhone_Formatted";
			public const string HA_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry = "HA_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string HA_MobilePhone_IsManuallyVerified = "HA_MobilePhone_IsManuallyVerified";
			public const string HA_WorkPhone_Formatted = "HA_WorkPhone_Formatted";
			public const string HA_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry = "HA_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string HA_WorkPhone_IsManuallyVerified = "HA_WorkPhone_IsManuallyVerified";

			public const int HA_GenderMaxLength = 1;
		}

		#region HA_ValidationStatus

		public ZString HA_ValidationStatus
		{
			get => Person.PER_ValidationStatus;

			set
			{
				Person.PER_ValidationStatus = value;
			}
		}

		#endregion

		[DocumentFieldExcludeFromMap]
		[BusinessObjectTestExclude]
		public ZString UnrestrictedAdditionalAddressInformation { get; set; }

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => GetZPropertyInfo(nameof(UnrestrictedAdditionalAddressInformation));

		[BusinessObjectTestExclude]
		public CodeDescriptionPairList AdditionalAddressInfoList { get; }

		public ZString AddressCode { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is never seen by the user")]
		public const string DissolvedAlias = "+dissolved";

		public HRJobApplicant(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
			phoneNumberPropertyHelperThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers));
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new HRJobApplicantFetchStrategy(this);
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList objects = new ArrayList();

				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.AddRange(Applications);
				foreach (HRJobApplication application in Applications)
				{
					objects.AddRange(application.BusinessObjectsWithRelatedEvents);
				}
				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				ArrayList objects = new ArrayList();

				objects.AddRange(base.BusinessObjectsWithRelatedNotes);
				objects.AddRange(Applications);
				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		public override Notes Notes
		{
			get { return notes ??= new HRCandidateNotes(this); }
		}
		Notes notes;

		#endregion

		#region Save

		public override void OnSaving()
		{
			base.OnSaving();
			Person?.UpdateFromApplicant(this);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			Campaigns.DeleteAll();
			Applications.DeleteAll();
			AddOnRuleAcks.DeleteAll();
			DeleteAllPatterns();
			RequiredDocuments.DeleteAll();
			if (glbPerson != null && !glbPerson.IsInDatabase)
			{
				glbPerson.Delete();
			}

			base.Delete();
		}

		public void DeleteAllPatterns(bool deleteChildren = true)
		{
			PatternMatchingRemover.DeleteAll(this);
			if (deleteChildren)
			{
				foreach (var certificate in Certificates)
				{
					PatternMatchingRemover.DeleteAll(certificate);
				}
			}
		}

		#endregion

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("C2190FA5-49A2-43D0-8C15-9443DC429FFC", "Applicant");
				if (!IsDeleted && !HA_FullName.IsEmpty)
				{
					result += " - " + HA_FullName;
				}

				return result;
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				return HumanReadableNameCore;
			}
		}

		#endregion

		public bool NeedToCallSetPhoneNumber(ZString oldValue, ZString newValue)
		{
			var newNormalizedValue = PhoneNumberPropertyHelper.NormalizePhoneNumber(newValue);
			if (newNormalizedValue.IsEmpty)
			{
				return true;
			}

			var oldNormalizedValue = PhoneNumberPropertyHelper.NormalizePhoneNumber(oldValue);
			if (oldNormalizedValue != newNormalizedValue)
			{
				return true;
			}

			if (oldNormalizedValue.IndexOf('+') == 0 && oldValue.IndexOf('+') != 0)
			{
				return true;
			}

			return false;
		}

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (kind == TestBusinessObjectKind.PopulateDependentCollections && Applications.Count == 0)
			{
				Applications.AddNew();
			}

			if (Applications.Count != 0)
			{
				var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>(TestBusinessObjectKind.MinimumRequiredToSave);

				foreach (HRJobApplication application in Applications)
				{
					application.HP_HV = campaign.PK;
				}
			}

			if (HA_FullName.IsEmpty)
			{
				HA_FullName = ZGuid.NewZGuid().ToString();
			}

			if (Person != null)
			{
				Person.UpdateFromApplicant(this);
			}
		}

#endif
		#endregion

		#region Related Business Objects

		[ChildEditable(true)]
		public GenCustomAddOnRuleAckCollection AddOnRuleAcks
		{
			get
			{
				if (addOnRuleAcks == null)
				{
					addOnRuleAcks = new GenCustomAddOnRuleAckCollection(this);
					RegisterEditableChildObject(addOnRuleAcks);
				}

				return addOnRuleAcks;
			}
		}
		GenCustomAddOnRuleAckCollection addOnRuleAcks;

		#region PhoneNumbers

		internal ZString DefaultCountryCodeForPhoneNumbers
		{
			get { return HA_RN_NKCountry; }
		}

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelperThunk.Value; }
		}

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperThunk;

		#region HA_MobilePhone

		[MaxLength(AutoGlbPerson.Schema.PER_MobilePhoneMaxLength)]
		public ZString HA_MobilePhone
		{
			get => ViewEditRights.ViewMobilePhoneForJobApplicant ? Person?.PER_MobilePhoneInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_MobilePhone != value)
				{
					Person.PER_MobilePhone = value;
					if (Globals.IsWeb)
					{
						Validation.ValidateHA_MobilePhone();
					}

					HasChanges = true;
					HA_MobilePhoneInfo.RefreshBinding();
				}
			}
		}

		public bool HA_MobilePhone_ReadOnly => !ViewEditRights.ViewMobilePhoneForJobApplicant || !ViewEditRights.EditMobilePhoneForJobApplicant;

		public ZPropertyInfo HA_MobilePhoneInfo => GetZPropertyInfo(Schema.HA_MobilePhone);

		public PhoneNumber MobilePhoneNumber
		{
			get
			{
				if (mobilePhoneNumber == null)
				{
					mobilePhoneNumber = new PhoneNumber(HA_MobilePhone_FormattedInfo, null, HA_MobilePhone_FormattedLocalNumberIfLoggedInSameCountryInfo, HA_MobilePhone_IsManuallyVerifiedInfo);
				}
				return mobilePhoneNumber;
			}
		}
		PhoneNumber mobilePhoneNumber;

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString HA_MobilePhone_Formatted
		{
			get => PhoneNumberPropertyHelper.GetPhoneNumber(HA_MobilePhoneInfo);
			set
			{
				if (NeedToCallSetPhoneNumber(HA_MobilePhone, value))
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(HA_MobilePhoneInfo, HA_MobilePhone_FormattedInfo, value, Validation.ValidateHA_MobilePhone_Formatted, HA_MobilePhone_IsManuallyVerifiedInfo);
				}
			}
		}

		public bool HA_MobilePhone_Formatted_ReadOnly => !ViewEditRights.ViewMobilePhoneForJobApplicant || !ViewEditRights.EditMobilePhoneForJobApplicant;

		public ZPropertyInfo HA_MobilePhone_FormattedInfo => GetZPropertyInfo(Schema.HA_MobilePhone_Formatted);

		public ZString HA_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get => PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(HA_MobilePhoneInfo);
		}

		public ZPropertyInfo HA_MobilePhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get => GetZPropertyInfo(Schema.HA_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public ZBool HA_MobilePhone_IsManuallyVerified
		{
			get => Person.PER_MobilePhone_IsManuallyVerified;
			set
			{
				if (Person.PER_MobilePhone_IsManuallyVerified != value)
				{
					Person.PER_MobilePhone_IsManuallyVerified = value;
					Validation.ValidateHA_MobilePhone_Formatted();
					MobilePhoneNumber.FormattedForBindingInfo.RefreshBinding();
					HA_MobilePhone_IsManuallyVerifiedInfo.RefreshBinding();
				}
			}
		}
		public bool HA_MobilePhone_IsManuallyVerified_ReadOnly => !ViewEditRights.ViewMobilePhoneForJobApplicant || !ViewEditRights.EditMobilePhoneForJobApplicant;

		public ZPropertyInfo HA_MobilePhone_IsManuallyVerifiedInfo
		{
			get => GetZPropertyInfo(Schema.HA_MobilePhone_IsManuallyVerified);
		}

		#endregion

		#region HA_HomePhone

		public PhoneNumber HomePhoneNumber
		{
			get
			{
				if (homePhoneNumber == null)
				{
					homePhoneNumber = new PhoneNumber(HA_HomePhone_FormattedInfo, null, HA_HomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo, HA_HomePhone_IsManuallyVerifiedInfo);
				}
				return homePhoneNumber;
			}
		}
		PhoneNumber homePhoneNumber;

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString HA_HomePhone_Formatted
		{
			get => PhoneNumberPropertyHelper.GetPhoneNumber(HA_HomePhoneInfo);
			set
			{
				if (NeedToCallSetPhoneNumber(HA_HomePhone, value))
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(HA_HomePhoneInfo, HA_HomePhone_FormattedInfo, value, Validation.ValidateHA_HomePhone_Formatted, HA_HomePhone_IsManuallyVerifiedInfo);
				}
			}
		}
		public bool HA_HomePhone_Formatted_ReadOnly => !ViewEditRights.ViewHomePhoneForJobApplicant || !ViewEditRights.EditHomePhoneForJobApplicant;

		public ZPropertyInfo HA_HomePhone_FormattedInfo
		{
			get => GetZPropertyInfo(Schema.HA_HomePhone_Formatted);
		}

		public ZString HA_HomePhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get => PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(HA_HomePhoneInfo);
		}

		public ZPropertyInfo HA_HomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get => GetZPropertyInfo(Schema.HA_HomePhone_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public ZBool HA_HomePhone_IsManuallyVerified
		{
			get => Person.PER_HomePhone_IsManuallyVerified;
			set
			{
				if (Person.PER_HomePhone_IsManuallyVerified != value)
				{
					Person.PER_HomePhone_IsManuallyVerified = value;
					Validation.ValidateHA_HomePhone_Formatted();
					HomePhoneNumber.FormattedForBindingInfo.RefreshBinding();
					HA_HomePhone_IsManuallyVerifiedInfo.RefreshBinding();
				}
			}
		}
		public bool HA_HomePhone_IsManuallyVerified_ReadOnly => !ViewEditRights.ViewHomePhoneForJobApplicant || !ViewEditRights.EditHomePhoneForJobApplicant;

		public ZPropertyInfo HA_HomePhone_IsManuallyVerifiedInfo
		{
			get => GetZPropertyInfo(Schema.HA_HomePhone_IsManuallyVerified);
		}

		#endregion

		#region HA_FaxNum

		[MaxLength(AutoGlbPerson.Schema.PER_FaxNumberMaxLength)]
		public ZString HA_FaxNum
		{
			get => ViewEditRights.ViewFaxForJobApplicant ? Person?.PER_FaxNumberInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_FaxNumber != value)
				{
					Person.PER_FaxNumber = value;
					if (Globals.IsWeb)
					{
						Validation.ValidateHA_FaxNum();
					}

					HasChanges = true;
					HA_FaxNumInfo.RefreshBinding();
				}
			}
		}
		public bool HA_FaxNum_ReadOnly => !ViewEditRights.ViewFaxForJobApplicant || !ViewEditRights.EditFaxForJobApplicant;

		public ZPropertyInfo HA_FaxNumInfo
		{
			get { return GetZPropertyInfo(Schema.HA_FaxNum); }
		}

		public PhoneNumber FaxNumber
		{
			get
			{
				if (faxNumber == null)
				{
					faxNumber = new PhoneNumber(HA_FaxNum_FormattedInfo, null, HA_FaxNum_FormattedLocalNumberIfLoggedInSameCountryInfo, HA_FaxNum_IsManuallyVerifiedInfo);
				}
				return faxNumber;
			}
		}
		PhoneNumber faxNumber;

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString HA_FaxNum_Formatted
		{
			get => PhoneNumberPropertyHelper.GetPhoneNumber(HA_FaxNumInfo);
			set
			{
				if (NeedToCallSetPhoneNumber(HA_FaxNum, value))
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(HA_FaxNumInfo, HA_FaxNum_FormattedInfo, value, Validation.ValidateHA_FaxNum_Formatted, HA_FaxNum_IsManuallyVerifiedInfo);
				}
			}
		}

		public bool HA_FaxNum_Formatted_ReadOnly => !ViewEditRights.ViewFaxForJobApplicant || !ViewEditRights.EditFaxForJobApplicant;

		public ZPropertyInfo HA_FaxNum_FormattedInfo
		{
			get => GetZPropertyInfo(Schema.HA_FaxNum_Formatted);
		}

		public ZString HA_FaxNum_FormattedLocalNumberIfLoggedInSameCountry
		{
			get => PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(HA_FaxNumInfo);
		}

		public ZPropertyInfo HA_FaxNum_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get => GetZPropertyInfo(Schema.HA_FaxNum_FormattedLocalNumberIfLoggedInSameCountry);
		}

		public ZBool HA_FaxNum_IsManuallyVerified
		{
			get => Person.PER_FaxNum_IsManuallyVerified;
			set
			{
				if (Person.PER_FaxNum_IsManuallyVerified != value)
				{
					Person.PER_FaxNum_IsManuallyVerified = value;
					Validation.ValidateHA_FaxNum_Formatted();
					FaxNumber.FormattedForBindingInfo.RefreshBinding();
					HA_FaxNum_IsManuallyVerifiedInfo.RefreshBinding();
				}
			}
		}

		public bool HA_FaxNum_IsManuallyVerified_ReadOnly => !ViewEditRights.ViewFaxForJobApplicant || !ViewEditRights.EditFaxForJobApplicant;

		public ZPropertyInfo HA_FaxNum_IsManuallyVerifiedInfo
		{
			get => GetZPropertyInfo(Schema.HA_FaxNum_IsManuallyVerified);
		}

		#endregion

		#region WorkPhoneNumber

		public PhoneNumber WorkPhoneNumber
		{
			get
			{
				if (workNumber == null)
				{
					workNumber = new PhoneNumber(HA_WorkPhone_FormattedInfo, null, HA_WorkPhone_FormattedLocalNumberIfLoggedInSameCountryInfo, HA_WorkPhone_IsManuallyVerifiedInfo);
				}
				return workNumber;
			}
		}
		PhoneNumber workNumber;

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString HA_WorkPhone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(HA_WorkPhoneInfo); }
			set
			{
				if (NeedToCallSetPhoneNumber(HA_WorkPhone, value))
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(HA_WorkPhoneInfo, HA_WorkPhone_FormattedInfo, value, Validation.ValidateHA_WorkPhone_Formatted, HA_WorkPhone_IsManuallyVerifiedInfo);
				}
			}
		}

		public ZPropertyInfo HA_WorkPhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.HA_WorkPhone_Formatted); }
		}

		public ZString HA_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(HA_WorkPhoneInfo); }
		}

		public ZPropertyInfo HA_WorkPhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.HA_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool HA_WorkPhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, HRJobApplicantSchema.Constants.HA_WorkPhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(HA_WorkPhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, HRJobApplicantSchema.Constants.HA_WorkPhone, Validation.ValidateHA_WorkPhone_Formatted, WorkPhoneNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo HA_WorkPhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.HA_WorkPhone_IsManuallyVerified); }
		}

		#endregion

		public PhoneNumber WorkExtensionNumber
		{
			get
			{
				if (workExtensionNumber == null)
				{
					workExtensionNumber = new PhoneNumber(HA_WorkExtensionInfo, null, null);
				}
				return workExtensionNumber;
			}
		}
		PhoneNumber workExtensionNumber;

		#endregion

		#region Applications

		[ChildEditable(true)]
		public IJobApplicationDependantCollection Applications
		{
			get
			{
				if (fApplications == null)
				{
					fApplications = GetNewApplications();
					RegisterEditableChildObject(fApplications);
				}

				return fApplications;
			}
		}

		protected virtual IJobApplicationDependantCollection GetNewApplications()
			=> new HRJobApplicationDependentCollection(this);

		IJobApplicationDependantCollection fApplications;

		#endregion

		#region Skill Ratings
		protected virtual bool AllowCompulsorySkillRating => true;

		public bool HasCompletedJobSkill(string category, string skillCode)
		{
				return false;
		}

		#endregion

		public ZDateTime LastSubmittedJobApplicationSubmissionTime
		{
			get
			{
				var lastSubmissionTime = Applications.Cast<HRJobApplication>().OrderBy(a => a.HP_SubmissionTimeUtc).LastOrDefault();
				if (lastSubmissionTime != null)
				{
					return lastSubmissionTime.SubmissionTimeLocal;
				}
				return ZDateTime.Empty;
			}
		}

		#endregion

		#region Properties

		#region HA_PER & Person

		public override ZGuid HA_PER
		{
			get
			{
				return base.HA_PER;
			}
			set
			{
				if (HA_PER != value)
				{
					if (glbPerson != null && !glbPerson.IsInDatabase)
					{
						glbPerson.Delete();
					}

					base.HA_PER = value;
					glbPerson = null;
				}
			}
		}

		GlbPerson glbPerson;

		public override GlbPerson Person
		{
			get
			{
				if (glbPerson == null || glbPerson.PK != HA_PER)
				{
					if (HA_PER.IsEmpty)
					{
						glbPerson = GlbPerson.CreateFromApplicant(Factory, this);
					}
					else
					{
						glbPerson = base.Person;
					}
				}

				return glbPerson;
			}
		}

		IGlbPerson IHRJobApplicant.Person => Person;

		#endregion

		[EmailAddress]
		public override ZString HA_EmailAddress
		{
			get
			{
				return base.HA_EmailAddress;
			}
			set
			{
				base.HA_EmailAddress = value.Trim();
				if (
						!Person.PER_EmailAddress.EqualsIgnoringCase(value)
						&&
							(
								Person.PER_EmailAddressInternal.IsEmpty
								|| ((ZString)HA_EmailAddressInfo.OriginalValue).EqualsIgnoringCase(Person.PER_EmailAddressInternal)
							)
					)
				{
					Person.PER_EmailAddress = value.Trim();
				}
			}
		}

		#region Wrapped Properties

		#region HA_Gender

		[List("Lookups.Genders")]
		[MaxLength(Schema.HA_GenderMaxLength)]
		public ZString HA_Gender
		{
			get => ViewEditRights.ViewGenderForJobApplicant ? Person?.PER_GenderInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_Gender != value)
				{
					Person.PER_Gender = value;
					HasChanges = true;
					Validation.ValidateHA_Gender();
				}
				HA_GenderInfo.RefreshBinding();
			}
		}
		public bool HA_Gender_ReadOnly => !ViewEditRights.ViewGenderForJobApplicant || !ViewEditRights.EditGenderForJobApplicant;

		public ZPropertyInfo HA_GenderInfo
		{
			get => GetZPropertyInfo(Schema.HA_Gender);
		}

		#endregion

		[MaxLength(AutoGlbPerson.Schema.PER_NameSuffixMaxLength)]
		public ZString HA_NameSuffix
		{
			get => ViewEditRights.ViewNameForJobApplicant ? Person?.PER_NameSuffix ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_NameSuffix != value)
				{
					Person.PER_NameSuffix = value;
					HasChanges = true;
					Validation.ValidateHA_NameSuffix();
				}

				HA_NameSuffixInfo.RefreshBinding();
			}
		}
		public bool HA_NameSuffix_ReadOnly => !ViewEditRights.ViewNameForJobApplicant || !ViewEditRights.EditNameForJobApplicant;

		public ZPropertyInfo HA_NameSuffixInfo
		{
			get => GetZPropertyInfo(Schema.HA_NameSuffix);
		}

		[MaxLength(AutoGlbPerson.Schema.PER_HomePhoneMaxLength)]
		public ZString HA_HomePhone
		{
			get => ViewEditRights.ViewHomePhoneForJobApplicant ? Person?.PER_HomePhoneInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_HomePhone != value)
				{
					Person.PER_HomePhone = value;
					if (Globals.IsWeb)
					{
						Validation.ValidateHA_HomePhone();
					}

					HasChanges = true;
				}

				HA_HomePhoneInfo.RefreshBinding();
			}
		}
		public bool HA_HomePhone_ReadOnly => !ViewEditRights.ViewHomePhoneForJobApplicant || !ViewEditRights.EditHomePhoneForJobApplicant;

		public ZPropertyInfo HA_HomePhoneInfo
		{
			get => GetZPropertyInfo(Schema.HA_HomePhone);
		}

		public ZDate HA_Birthdate
		{
			get => ViewEditRights.ViewBirthdateForJobApplicant ? Person?.PER_BirthDateInternal ?? ZDate.Empty : ZDate.Empty;
			set
			{
				if (Person.PER_BirthDate != value)
				{
					Person.PER_BirthDate = value;
					HasChanges = true;
					Validation.ValidateHA_Birthdate();
				}

				HA_BirthdateInfo.RefreshBinding();
			}
		}
		public bool HA_Birthdate_ReadOnly => !ViewEditRights.ViewBirthdateForJobApplicant || !ViewEditRights.EditBirthdateForJobApplicant;

		public ZPropertyInfo HA_BirthdateInfo
		{
			get => GetZPropertyInfo(Schema.HA_Birthdate);
		}

		[MaxLength(AutoGlbPerson.Schema.PER_PassportMaxLength)]
		public ZString HA_Passport
		{
			get => ViewEditRights.ViewPassportForJobApplicant ? Person?.PER_PassportInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_Passport != value)
				{
					Person.PER_Passport = value;
					HasChanges = true;
					Validation.ValidateHA_Passport();
				}

				HA_PassportInfo.RefreshBinding();
			}
		}
		public bool HA_Passport_ReadOnly => !ViewEditRights.ViewPassportForJobApplicant || !ViewEditRights.EditPassportForJobApplicant;

		public ZPropertyInfo HA_PassportInfo
		{
			get => GetZPropertyInfo(Schema.HA_Passport);
		}

		[MaxLength(AutoGlbPerson.Schema.PER_DriversLicenseNumberMaxLength)]
		public ZString HA_DriversLicenseNumber
		{
			get => ViewEditRights.ViewDriversLicenseNumberForJobApplicant ? Person?.PER_DriversLicenseNumberInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_DriversLicenseNumber != value)
				{
					Person.PER_DriversLicenseNumber = value;
					HasChanges = true;
					Validation.ValidateHA_DriversLicenseNumber();
				}

				HA_DriversLicenseNumberInfo.RefreshBinding();
			}
		}
		public bool HA_DriversLicenseNumber_ReadOnly => !ViewEditRights.ViewDriversLicenseNumberForJobApplicant || !ViewEditRights.EditDriversLicenseNumberForJobApplicant;

		public ZPropertyInfo HA_DriversLicenseNumberInfo
		{
			get => GetZPropertyInfo(Schema.HA_DriversLicenseNumber);
		}

		[MaxLength(AutoGlbPerson.Schema.PER_FullNameMaxLength)]
		public ZString HA_FullName
		{
			get => ViewEditRights.ViewNameForJobApplicant ? Person?.PER_FullNameInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_FullName != value)
				{
					Person.PER_FullName = value;
					HasChanges = true;
					Validation.ValidateHA_FullName();
				}

				HA_FullNameInfo.RefreshBinding();
			}
		}
		public bool HA_FullName_ReadOnly => !ViewEditRights.ViewNameForJobApplicant || !ViewEditRights.EditNameForJobApplicant;

		public ZPropertyInfo HA_FullNameInfo
		{
			get => GetZPropertyInfo(Schema.HA_FullName);
		}

		[List("Lookups.Countries")]
		[MaxLength(AutoGlbPerson.Schema.PER_RN_NKNationalityCodeISOMaxLength)]
		public ZString HA_RN_NKNationalityCodeISO
		{
			get => ViewEditRights.ViewNationalityForJobApplicant ? Person?.PER_RN_NKNationalityCodeISOInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_RN_NKNationalityCodeISO != value)
				{
					Person.PER_RN_NKNationalityCodeISO = value;
					HasChanges = true;
					Validation.ValidateHA_RN_NKNationalityCodeISO();
				}

				if (value == GlbBranch.CurrentBranch.Country.RN_Code)
				{
					HA_WorkPermitStatus = Core.Constants.WorkPermitStatuses.Residence;
				}
				HA_RN_NKNationalityCodeISOInfo.RefreshBinding();
			}
		}
		public bool HA_RN_NKNationalityCodeISO_ReadOnly => !ViewEditRights.ViewNationalityForJobApplicant || !ViewEditRights.EditNationalityForJobApplicant;

		public ZPropertyInfo HA_RN_NKNationalityCodeISOInfo
		{
			get => GetZPropertyInfo(Schema.HA_RN_NKNationalityCodeISO);
		}

		#region HA_Title

		[MaxLength(AutoGlbPerson.Schema.PER_NameTitleMaxLength)]
		public ZString HA_Title
		{
			get => ViewEditRights.ViewNameForJobApplicant ? Person?.PER_NameTitle ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_NameTitle != value)
				{
					Person.PER_NameTitle = value;
					HasChanges = true;
					Validation.ValidateHA_Title();
				}

				HA_TitleInfo.RefreshBinding();
			}
		}
		public bool HA_Title_ReadOnly => !ViewEditRights.ViewNameForJobApplicant || !ViewEditRights.EditNameForJobApplicant;

		public ZPropertyInfo HA_TitleInfo
		{
			get => GetZPropertyInfo(Schema.HA_Title);
		}

		public static ZQuery GetLearningCenterUserLogQuery()
		{
			var logQuery = new ZQuery();
			logQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Related Contact");
			logQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Related Staff");
			logQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Created from Contact");
			logQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Approved by");
			return logQuery;
		}

		#endregion

		#region HA_UserAddress1

		[MaxLength(AutoGlbPerson.Schema.PER_HomeAddress1MaxLength)]
		public ZString HA_UserAddress1
		{
			get => ViewEditRights.ViewUserAddressForJobApplicant ? Person?.PER_HomeAddress1Internal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_HomeAddress1 != value)
				{
					Person.PER_HomeAddress1 = value;
					HasChanges = true;
					Validation.ValidateHA_UserAddress1();
				}

				HA_UserAddress1Info.RefreshBinding();
			}
		}
		public bool HA_UserAddress1_ReadOnly => !ViewEditRights.ViewUserAddressForJobApplicant || !ViewEditRights.EditUserAddressForJobApplicant;

		public ZPropertyInfo HA_UserAddress1Info
		{
			get => GetZPropertyInfo(Schema.HA_UserAddress1);
		}

		#endregion

		#region HA_UserAddress2

		[MaxLength(AutoGlbPerson.Schema.PER_HomeAddress2MaxLength)]
		public ZString HA_UserAddress2
		{
			get => ViewEditRights.ViewUserAddressForJobApplicant ? Person?.PER_HomeAddress2Internal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_HomeAddress2 != value)
				{
					Person.PER_HomeAddress2 = value;
					HasChanges = true;
					Validation.ValidateHA_UserAddress2();
				}

				HA_UserAddress2Info.RefreshBinding();
			}
		}
		public bool HA_UserAddress2_ReadOnly => !ViewEditRights.ViewUserAddressForJobApplicant || !ViewEditRights.EditUserAddressForJobApplicant;

		public ZPropertyInfo HA_UserAddress2Info
		{
			get => GetZPropertyInfo(Schema.HA_UserAddress2);
		}

		#endregion

		#region HA_City

		[MaxLength(AutoGlbPerson.Schema.PER_CityMaxLength)]
		public ZString HA_City
		{
			get => ViewEditRights.ViewUserAddressForJobApplicant ? Person?.PER_CityInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_City != value)
				{
					Person.PER_City = value;
					HasChanges = true;
					Validation.ValidateHA_City();
				}

				HA_CityInfo.RefreshBinding();
			}
		}
		public bool HA_City_ReadOnly => !ViewEditRights.ViewUserAddressForJobApplicant || !ViewEditRights.EditUserAddressForJobApplicant;

		public ZPropertyInfo HA_CityInfo
		{
			get => GetZPropertyInfo(Schema.HA_City);
		}

		#endregion

		#region HA_State

		[List("Lookups.StateList")]
		[MaxLength(AutoGlbPerson.Schema.PER_StateMaxLength)]
		public ZString HA_State
		{
			get => ViewEditRights.ViewUserAddressForJobApplicant ? Person?.PER_StateInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_State != value)
				{
					Person.PER_State = value;
					HasChanges = true;
					Validation.ValidateHA_State();
				}

				HA_StateInfo.RefreshBinding();
			}
		}
		public bool HA_State_ReadOnly => !ViewEditRights.ViewUserAddressForJobApplicant || !ViewEditRights.EditUserAddressForJobApplicant;

		public ZPropertyInfo HA_StateInfo
		{
			get => GetZPropertyInfo(Schema.HA_State);
		}

		#endregion

		#region HA_Country

		[List("Lookups.Countries")]
		[MaxLength(AutoGlbPerson.Schema.PER_RN_NKCountryMaxLength)]
		public ZString HA_RN_NKCountry
		{
			get => ViewEditRights.ViewUserAddressForJobApplicant ? Person?.PER_RN_NKCountryInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (HA_RN_NKCountry != value)
				{
					if (Person.PER_RN_NKCountry != value)
					{
						Person.PER_RN_NKCountry = value;
						HasChanges = true;
						Validation.ValidateHA_RN_NKCountry();
						HA_RN_NKCountryInfo.RefreshBinding();
					}
					HA_StateInfo.RefreshBinding();
				}
			}
		}
		public bool HA_RN_NKCountry_ReadOnly => !ViewEditRights.ViewUserAddressForJobApplicant || !ViewEditRights.EditUserAddressForJobApplicant;

		public ZPropertyInfo HA_RN_NKCountryInfo
		{
			get => GetZPropertyInfo(Schema.HA_RN_NKCountry);
		}

		#endregion

		#region HA_Postcode

		[MaxLength(AutoGlbPerson.Schema.PER_PostcodeMaxLength)]
		public ZString HA_Postcode
		{
			get => ViewEditRights.ViewUserAddressForJobApplicant ? Person?.PER_PostcodeInternal ?? ZString.Empty : ViewDeniedMessage;
			set
			{
				if (Person.PER_Postcode != value)
				{
					Person.PER_Postcode = value;
					HasChanges = true;
					Validation.ValidateHA_Postcode();
				}

				HA_PostcodeInfo.RefreshBinding();
			}
		}
		public bool HA_Postcode_ReadOnly => !ViewEditRights.ViewUserAddressForJobApplicant || !ViewEditRights.EditUserAddressForJobApplicant;

		public ZPropertyInfo HA_PostcodeInfo
		{
			get => GetZPropertyInfo(Schema.HA_Postcode);
		}

		#endregion

		#endregion

		public ZDecimal CompulsorySkillRatingTotal
		{
			get
			{
				return ZDecimal.Zero;
			}
		}

		public ZPropertyInfo CompulsarySkillRatingTotalInfo
		{
			get { return GetZPropertyInfo(nameof(CompulsorySkillRatingTotal)); }
		}
		protected virtual bool HA_WorkPermitStatus_ReadOnly
		{
			get { return HA_RN_NKNationalityCodeISO == GlbBranch.CurrentBranch.Country.RN_Code; }
		}

		public virtual ZBool IsLearningCenterUser
		{
			get
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, this.PK);
				query.AddToFilter(GetLearningCenterUserLogQuery());
				return Factory.Load<StmALog>(query).Length > 0;
			}
		}

		#endregion

		#region ISupportWebAddressValidation

		public ZString AddressRecordGUID
		{
			get { return PK.ToString(); }
		}

		public ZString AddressSourceTable
		{
			get { return GlbStaffSchema.Constants.Prefix; }
		}

		public ZString Address1
		{
			get { return HA_UserAddress1; }
			set { HA_UserAddress1 = value; }
		}

		public ZPropertyInfo Address1Info
		{
			get { return HA_UserAddress1Info; }
		}

		public int Address1_MaxLength
		{
			get { return AutoGlbPerson.Schema.PER_HomeAddress1MaxLength; }
		}

		public ZString Address2
		{
			get { return HA_UserAddress2; }
			set { HA_UserAddress2 = value; }
		}

		public ZPropertyInfo Address2Info
		{
			get { return HA_UserAddress2Info; }
		}

		public int Address2_MaxLength
		{
			get { return AutoGlbPerson.Schema.PER_HomeAddress2MaxLength; }
		}

		public ZString City
		{
			get { return HA_City; }
			set { HA_City = value; }
		}

		public ZPropertyInfo CityInfo
		{
			get { return HA_CityInfo; }
		}

		public int City_MaxLength
		{
			get { return AutoGlbPerson.Schema.PER_CityMaxLength; }
		}

		[BusinessObjectTestExclude] // State has to be valid
		public ZString State
		{
			get { return StateCodeList.GetDescriptionFromCode(HA_State); }
			set
			{
				var code = (ZString)StateCodeList.GetCodeFromDescription(value);
				HA_State = string.IsNullOrEmpty(code) ? value : code;
				StateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return HA_StateInfo; }
		}

		public int State_MaxLength
		{
			get { return AutoGlbPerson.Schema.PER_StateMaxLength; }
		}

		public ZString StateCode
		{
			get { return HA_State; }
			set
			{
				HA_State = value;
				StateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StateCode)); }
		}

		public int StateCode_MaxLength
		{
			get { return AutoGlbPerson.Schema.PER_StateMaxLength; }
		}

		public CodeDescriptionPairList StateCodeList
		{
			get
			{
				return Lookups.StateList;
			}
		}

		public ZString Postcode
		{
			get { return HA_Postcode; }
			set { HA_Postcode = value; }
		}

		public ZPropertyInfo PostcodeInfo
		{
			get { return HA_PostcodeInfo; }
		}

		public int Postcode_MaxLength
		{
			get { return AutoGlbPerson.Schema.PER_PostcodeMaxLength; }
		}

		public ZString AddressMap
		{
			get;
			set;
		}

		[List("CountryCodeList")]
		ZString ISupportWebAddressValidation.CountryCodeISO2
		{
			get { return this.HA_RN_NKCountry; }
			set { HA_RN_NKCountry = value; }
		}

		public int CountryCodeISO2_MaxLength => AutoGlbPerson.Schema.PER_RN_NKCountryMaxLength;

		public RefCountryCollection CountryCodeList
		{
			get { return Person.Lookups.Countries; }
		}

		[BusinessObjectTestExclude]
		public ZString Language
		{
			get { return Core.SharedConstants.Languages.English; }
			set { }
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(nameof(Language)); }
		}

		public int Language_MaxLength { get { return 0; } }

		public CodeDescriptionPairList LanguageList { get { return null; } }

		public ZString ValidationStatus
		{
			get { return HA_ValidationStatus; }
			set { HA_ValidationStatus = value; }
		}

		public ZString Addressee
		{
			get { return HA_FullName; }
		}

		public ZString ClosestPort { get; set; }

		public AddressValidationSection ValidationSection { get; } = AddressValidationSection.Applicant;

		public bool IsErrorSuppressed => false;
		public bool IsJobDocAddress => false;
		public bool IsValidatedByBackgroundService { get; set; }

		public event EventHandler AddressValidationStatusChanged;
		public event EventHandler TriggerWebAddressValidation;
		public event EventHandler TriggerWebGetCityTown;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "AddressValidationStatusChanged is required in implementing the ISupportWebAddressValidation interface")]
		void RaiseAddressValidationStatusChanged()
		{
			if (AddressValidationStatusChanged != null)
			{
				AddressValidationStatusChanged(this, EventArgs.Empty);
			}
		}

		public void ClearWebAddressValidationHandler()
		{
			if (TriggerWebAddressValidation != null)
			{
				foreach (EventHandler item in TriggerWebAddressValidation.GetInvocationList())
				{
					TriggerWebAddressValidation -= item;
				}
			}
		}

		public void ClearWebGetCityTownHandler()
		{
			if (TriggerWebGetCityTown != null)
			{
				foreach (EventHandler item in TriggerWebGetCityTown.GetInvocationList())
				{
					TriggerWebGetCityTown -= item;
				}
			}
		}

		public bool IsUpdatingCityTown { get; set; }
		public bool IsValidatingAddress { get; set; }
		public bool IsExactPointFound { get; set; }

		[BusinessObjectTestExclude] // Company of a staff cannot be changed
		public ZString CompanyName
		{
			get { return Env.CurrentCompany.Name; }
			set { }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyName)); }
		}

		public int CompanyName_MaxLength
		{
			get { return 0; }
		}

		public ZString DisplayText
		{
			get => HA_FullName;
			set => throw new NotImplementedException();
		}

		public ZGuid EntityPK
		{
			get { return PK; }
		}

		public bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return false;
		}

		public bool IsTSAKnownAddress => false;
		public bool IsMIDAddress => false;

		protected virtual bool ShouldTrackHasChanges { get { return shouldTrackHasChanges; } set { shouldTrackHasChanges = value; } }
		bool shouldTrackHasChanges = true;

		#endregion

		#region Email

		public virtual string FromDisplayName
		{
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		public ZString FromAddress
		{
			get { return Env.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress; }
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new HRJobApplicantDocManagerInfo(this, Core.Constants.DocManagerCodes.JobApplicant);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsSecurity Members

		public SecurityCheckpoint EdocsSecurityCheckpoint => Env.Security.HRJobApplicantView;

		#endregion

		#region IContactable Members

		public string Email
		{
			get { return HA_EmailAddress; }
		}

		public IContactable[] GetNestedContacts(string parentContactDescription)
		{
			return Array.Empty<IContactable>();
		}

		public string Mobile
		{
			get { return HA_MobilePhone; }
		}

		public string Name
		{
			get { return HA_FullName; }
		}

		public bool IsActive
		{
			get { return true; }
		}

		#endregion

		#region Campaigns

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public IGlbCompanyCampaignItemContactCollection Campaigns
		{
			get
			{
				if (campaigns == null)
				{
					campaigns = (IGlbCompanyCampaignItemContactCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbCompanyCampaignItemContactCollection>(), new object[] { Factory, this });
					RegisterEditableChildObject(campaigns);
					campaigns.SetCountedReadOnlyIncludingChildren(true);
				}

				return campaigns;
			}
		}

		IGlbCompanyCampaignItemContactCollection campaigns;

		#endregion

		#region IExamUrlRecipient Members

		string IExamUrlRecipient.Language => string.Empty;

		bool IExamUrlRecipient.HasNonPermittedDuplicateEmail => false;

		#region IGlbCompanyCampaignItemRecipient Members

		OrgHeader IGlbCompanyCampaignItemRecipient.Organisation
		{
			get { return null; }
		}

		string IGlbCompanyCampaignItemRecipient.Phone
		{
			get { return HA_MobilePhone; }
		}

		string IGlbCompanyCampaignItemRecipient.Fax
		{
			get { return HA_FaxNum; }
		}

		string IGlbCompanyCampaignItemRecipient.Salutation
		{
			get { return ""; }
		}

		string IGlbCompanyCampaignItemRecipient.Title
		{
			get { return HA_Title; }
		}

		string IGlbCompanyCampaignItemRecipient.RelatedDocName
		{
			get { return $"{ResString.GetMultilingualString("CC178F09-3F16-416E-B242-0397D7ECD9C9", "Job Applicant")} {HA_FullName}"; }
		}

		#endregion

		#endregion

		#region ICertificatesProvider Members

		[ChildEditable(true)]
		public GenRegCertAccredMaintListCollection Certificates
		{
			get
			{
				if (certificates == null)
				{
					certificates = new GenRegCertAccredMaintListCollection(this);
					RegisterEditableChildObject(certificates);
				}

				return certificates;
			}
		}

		[ActionFieldFollow(false)]
		[ChildEditable(true)]
		public IActiveBusinessObjectCollection CertificatesBizoCollection => Certificates;

		GenRegCertAccredMaintListCollection certificates;

		ICodeDescriptionPairList ICertificatesProvider.GetCertificateTypeList()
		{
			return RecruiterDataRegistry.Instance.CertificateTypesExtra.Value.GetCodeDescriptionPairList();
		}

		ICodeDescriptionPairList ICertificatesProvider.GetActiveCertificateTypeList()
		{
			return RecruiterDataRegistry.Instance.CertificateTypesExtra.Value.GetActiveCodeDescriptionPairList();
		}

		ZString ICertificatesProvider.GetDefaultDescription(ZString code)
		{
			return ZString.Empty;
		}

		#endregion

		#region Interface

		public void UpdateEmailAddress(ZString newEmailAddress)
		{
			HA_EmailAddress = newEmailAddress;
		}

		public void UpdateMobilePhone(ZString newMobilePhone)
		{
			HA_MobilePhone = newMobilePhone;
		}

		#endregion

		public void SetFromPerson(GlbPerson person)
		{
			HA_PER = person.PK;
			glbPerson = person;
			HA_EmailAddress = person.PER_EmailAddressInternal;
			UpdateFromPerson(person);
		}

		public void UpdateFromPerson(IGlbPerson person)
		{
			var realPerson = person as GlbPerson;
			HA_PER = realPerson.PK;

			if (!IsLearningCenterUser)
			{
				if (!realPerson.PER_EmailAddressInternal.IsEmpty &&
					HA_EmailAddress.EqualsIgnoringCase(realPerson.PER_EmailAddressInfo.OriginalValue.ToString()) &&
					!EmailContainsDissolvedAlias(realPerson.PER_EmailAddress))
				{
					HA_EmailAddress = realPerson.PER_EmailAddressInternal;
				}
			}
		}

		#region IHaveRequiredDocuments members

		public ZString UniqueConsignRef => null;

		public ZString HouseBill => null;

		public ZString MasterBill => null;

		public OrgHeader ExportBroker => null;

		public ZString TableCode => HRJobApplicantSchema.Constants.Prefix;

		[ChildEditable]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					requiredDocuments.Load();
					RegisterEditableChildObject(requiredDocuments);
				}
				return requiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection requiredDocuments;

		public BusinessObject UltimateDocumentParent => this;

		public IReadOnlyList<ZString> AdditionalRefTypes => Array.Empty<ZString>();

		[Obsolete("Please use Country property. This will be removed once references in client-defined macros (Documents, Workflows, Notes, etc) are transformed.")]
		[MacroIgnore]
		public RefCountry CountryCode => Country;

		public RefCountry Country => Person.Country;

		public ZGeography GeoLocation { get => Person.GeoLocation; set => Person.GeoLocation = value; }

		public bool NeedValidation => Person.NeedValidation;

		public void PreLogAllDocumentsReceivedEvents()
		{
		}

		#endregion

		#region Helpers

		public static bool EmailContainsDissolvedAlias(string email) => email.Contains("@") &&
				email.Substring(0, email.IndexOf("@", StringComparison.OrdinalIgnoreCase)).Length > DissolvedAlias.Length &&
				email.Substring(email.IndexOf("@", StringComparison.OrdinalIgnoreCase) - DissolvedAlias.Length, DissolvedAlias.Length) == DissolvedAlias;

		public Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			return Person.ValidateAddressAsync(cancellationToken, cleanseAction);
		}

		public Task<CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken)
		{
			return Person.GetCityTownAsync(cancellationToken);
		}

		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			Person.ResetValidationStatus(propertyInfo);
		}

		public void PreValidationForAddressValidationService()
		{
			Person.PreValidationForAddressValidationService();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Person.ValidatePostcodeAndStateForAddress();
		}

		#endregion

		internal HRJobApplicantViewEditRights ViewEditRights
		{
			get { return viewEditRights ?? (viewEditRights = new HRJobApplicantViewEditRights(this)); }
		}
		HRJobApplicantViewEditRights viewEditRights;

		internal ZString ViewDeniedMessage
		{
			get { return Res.GetString("037A39F4-7604-4B24-9402-9F4575CE03DA", "** View Denied **"); }
		}
	}
}
