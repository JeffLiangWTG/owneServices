using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Schema;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;
using WTG.Foundation.Cryptography.UserSecrets;
using static Enterprise.Core.Constants;
using ZDateTime = CargoWise.Types.ZDateTime;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.PER_FullName), DescriptionProperty(Schema.PER_FullName)]
	public class GlbPerson : AutoGlbPerson,
		ICertificatesProvider,
		ICodeDescription,
		ISupportWebAddressValidation,
		IDeduplicatable,
		IPatternCleanup,
		IDocManagerSupport,
		IDocumentSupportable,
		IWorkflowProvider,
		IPatternMatchingRegenerationEntities<GlbPerson>,
		IGlbPerson,
		IPasswordStored,
		IEmailAddressGetterForTrigger,
		IGlbPasswordHistoryParent
	{
		public GlbPerson(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			phoneNumberPropertyHelperThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers));
		}

		public new class Schema : AutoGlbPerson.Schema
		{
			public new const int PER_PassportPlaceOfIssueMaxLength = 2;
			public const string PER_FaxNum_Formatted = "PER_FaxNum_Formatted";
			public const string PER_FaxNum_FormattedLocalNumberIfLoggedInSameCountry = "PER_FaxNum_FormattedLocalNumberIfLoggedInSameCountry";
			public const string PER_FaxNum_IsManuallyVerified = "PER_FaxNum_IsManuallyVerified";
			public const string PER_HomePhone_Formatted = "PER_HomePhone_Formatted";
			public const string PER_HomePhone_FormattedLocalNumberIfLoggedInSameCountry = "PER_HomePhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string PER_HomePhone_IsManuallyVerified = "PER_HomePhone_IsManuallyVerified";
			public const string PER_MobilePhone_Formatted = "PER_MobilePhone_Formatted";
			public const string PER_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry = "PER_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string PER_MobilePhone_IsManuallyVerified = "PER_MobilePhone_IsManuallyVerified";
			public const string PER_PrimaryEmail = "PER_PrimaryEmail";
			public const string PER_PrimaryWorkplace = "PER_PrimaryWorkplace";
			public const string PER_WorkingLocation = "PER_WorkingLocation";
		}

		#region Recalculate PatternMatchingTables

		PatternMatchingRecalculator<GlbPerson> patternMatchingRecalculator;
		public PatternMatchingRecalculator<GlbPerson> PatternMatchingRecalculator
		{
			get
			{
				return patternMatchingRecalculator ?? (patternMatchingRecalculator = new PatternMatchingRecalculator<GlbPerson>(this));
			}
		}

		public async Task RegeneratePatternTables()
		{
			await PatternMatchingRecalculator.RegenerateAsync(ObjectFactory.Get<IMasterDataProvider>().GetDeduplicationGlbPerson(this));
		}

		#endregion

		#region Properties

		public override ZString PER_FullName
		{
			get { return base.PER_FullName; }
			set
			{
				base.PER_FullName = value;
				FindDuplicates();
			}
		}

		public BusinessObject DeduplicationChildBizO { get; set; }

		ZGuid InfoPropagationSourcePk { get; set; }

		public void FindDuplicates()
		{
			if (IsDeduplicationAllowed && shouldRunDeduplication && !(DeduplicationChildBizO ?? this).HasErrors)
			{
				FindDuplicatesCore(false);
			}
		}

		void IDeduplicatable.FindDuplicatesBypassErrorChecking(bool isForAdminPanel)
		{
			if (IsDeduplicationAllowed && shouldRunDeduplication)
			{
				FindDuplicatesCore(isForAdminPanel);
			}
		}

#if DEBUG
		public
#endif
			ISupportDuplicationFinder CurrentDuplicationFinder
		{ get; set; }

		void FindDuplicatesCore(bool isForAdminPanel)
		{
			PropagateDeduplicationStarted();
			CurrentDuplicationFinder?.RequestToCancel();
			CurrentDuplicationFinder = ObjectFactory.Get<IMasterDataProvider>().CreatePersonDuplicationFinder(this, isForAdminPanel);
			CurrentDuplicationFinder.FindDuplicates();
		}

		public void FindDuplicates(BusinessObject bizO)
		{
			DeduplicationChildBizO = bizO;
			((IDeduplicatable)this).FindDuplicates();
		}

		public bool IsDissolving { get; set; }
		public bool IsDeduplicationAllowed => !ReadOnly && SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value; //TODO Security.PersonModify needed

		[List("Lookups.Genders")]
		public override ZString PER_Gender
		{
			get { return ViewEditRights.ViewGenderForPerson ? base.PER_Gender : ViewDeniedMessage; }
			set { base.PER_Gender = value; }
		}

		protected bool PER_Gender_ReadOnly
		{
			get { return !ViewEditRights.ViewGenderForPerson || !ViewEditRights.EditGenderForPerson; }
		}

		public bool IsGenderReadOnly
		{
			get { return PER_Gender_ReadOnly; }
		}

		[List("Lookups.Languages")]
		public override ZString PER_PreferredLanguage
		{
			get { return base.PER_PreferredLanguage; }
			set { base.PER_PreferredLanguage = value; }
		}

		[List("Lookups.Countries")]
		[MaxLength(Schema.PER_PassportPlaceOfIssueMaxLength)]
		public override ZString PER_PassportPlaceOfIssue
		{
			get => ViewEditRights.ViewPassportForPerson ? base.PER_PassportPlaceOfIssue : ViewDeniedMessage;
			set
			{
				CheckMaximumLength(PER_PassportPlaceOfIssueInfo, value);
				base.PER_PassportPlaceOfIssue = value;
			}
		}

		protected bool PER_PassportPlaceOfIssue_ReadOnly => PER_Passport_ReadOnly;

		public override ZString PER_HomeAddress1
		{
			get { return ViewEditRights.ViewHomeAddressForPerson ? base.PER_HomeAddress1 : ViewDeniedMessage; }
			set
			{
				if (base.PER_HomeAddress1 != value)
				{
					base.PER_HomeAddress1 = value;
					ResetValidationStatus(PER_HomeAddress1Info);
				}
			}
		}

		protected bool PER_HomeAddress1_ReadOnly
		{
			get { return !ViewEditRights.ViewHomeAddressForPerson || !ViewEditRights.EditHomeAddressForPerson; }
		}

		public override ZString PER_HomeAddress2
		{
			get { return ViewEditRights.ViewHomeAddressForPerson ? base.PER_HomeAddress2 : ViewDeniedMessage; }
			set
			{
				if (base.PER_HomeAddress2 != value)
				{
					base.PER_HomeAddress2 = value;
					ResetValidationStatus(PER_HomeAddress2Info);
				}
			}
		}

		protected bool PER_HomeAddress2_ReadOnly
		{
			get { return !ViewEditRights.ViewHomeAddressForPerson || !ViewEditRights.EditHomeAddressForPerson; }
		}

		public override ZString PER_City
		{
			get { return ViewEditRights.ViewHomeAddressForPerson ? base.PER_City : ViewDeniedMessage; }
			set
			{
				if (base.PER_City != value)
				{
					base.PER_City = value;
					ResetValidationStatus(PER_CityInfo);
				}
			}
		}

		protected bool PER_City_ReadOnly
		{
			get { return !ViewEditRights.ViewHomeAddressForPerson || !ViewEditRights.EditHomeAddressForPerson; }
		}

		[List("Lookups.StateList")]
		public override ZString PER_State
		{
			get { return ViewEditRights.ViewHomeAddressForPerson ? base.PER_State : ViewDeniedMessage; }
			set
			{
				if (base.PER_State != value)
				{
					base.PER_State = value;
					ResetValidationStatus(PER_StateInfo);
				}
			}
		}

		protected bool PER_State_ReadOnly
		{
			get { return !ViewEditRights.ViewHomeAddressForPerson || !ViewEditRights.EditHomeAddressForPerson; }
		}

		[List("Lookups.Countries")]
		public override ZString PER_RN_NKCountry
		{
			get { return ViewEditRights.ViewHomeAddressForPerson ? base.PER_RN_NKCountry : ViewDeniedMessage; }
			set
			{
				if (PER_RN_NKCountry != value)
				{
					base.PER_RN_NKCountry = value;
					ResetValidationStatus(PER_RN_NKCountryInfo);
				}
			}
		}

		protected bool PER_RN_NKCountry_ReadOnly
		{
			get { return !ViewEditRights.ViewHomeAddressForPerson || !ViewEditRights.EditHomeAddressForPerson; }
		}

		public override ZString PER_Postcode
		{
			get { return ViewEditRights.ViewHomeAddressForPerson ? base.PER_Postcode : ViewDeniedMessage; }
			set
			{
				if (base.PER_Postcode != value)
				{
					base.PER_Postcode = value;
					ResetValidationStatus(PER_PostcodeInfo);
				}
			}
		}

		protected bool PER_Postcode_ReadOnly
		{
			get { return !ViewEditRights.ViewHomeAddressForPerson || !ViewEditRights.EditHomeAddressForPerson; }
		}

		[List("Lookups.Titles")]
		public override ZString PER_NameTitle
		{
			get => base.PER_NameTitle;
			set
			{
				base.PER_NameTitle = value;
				ResetValidationStatus(PER_NameTitleInfo);

				if (PER_NameTitle == GlbPersonLookups.TitleNames.Mr.EnglishText)
				{
					PER_Gender = Constants.Genders.Man;
				}
				else if (PER_NameTitle == GlbPersonLookups.TitleNames.Ms.EnglishText ||
						PER_NameTitle == GlbPersonLookups.TitleNames.Mrs.EnglishText ||
						PER_NameTitle == GlbPersonLookups.TitleNames.Miss.EnglishText)
				{
					PER_Gender = Constants.Genders.Woman;
				}
			}
		}

		public Image PER_ProfilePicture
		{
			get => ViewEditRights.ViewImageForPerson ? GetImage(PER_Picture) : null;
			set => SetImage(value, GlbPersonSchema.PER_Picture);
		}

		public override ZBlob PER_Picture
		{
			get => ViewEditRights.ViewImageForPerson ? base.PER_Picture : null;
			set => base.PER_Picture = value;
		}

		protected bool PER_Picture_ReadOnly
		{
			get { return !ViewEditRights.ViewImageForPerson || !ViewEditRights.EditImageForPerson; }
		}

		public ZInt PER_Age
		{
			get
			{
				if (PER_BirthDate != ZDate.Empty)
				{
					var now = ZDateTime.Now;
					if (now.DayOfYear < PER_BirthDate.DayOfYear)
					{
						return now.Year - PER_BirthDate.Year - 1;
					}
					else
					{
						return now.Year - PER_BirthDate.Year;
					}
				}
				return -1;
			}
		}

		public override ZString PER_LegalName
		{
			get => ViewEditRights.ViewLegalNameForPerson ? base.PER_LegalName : ViewDeniedMessage;
			set => base.PER_LegalName = value;
		}

		protected bool PER_LegalName_ReadOnly
		{
			get { return !ViewEditRights.ViewLegalNameForPerson || !ViewEditRights.EditLegalNameForPerson; }
		}

		public override ZDate PER_BirthDate
		{
			get => ViewEditRights.ViewBirthDateForPerson ? base.PER_BirthDate : ZDate.Empty;
			set => base.PER_BirthDate = value;
		}

		public CargoWise.Glow.Model.Interfaces.IGlbPerson CreateIGlbPerson(bool isDummy = false)
		{
			var person = ObjectFactory.Get<IMasterDataProvider>().CreateIGlbPerson(this, isDummy);

			return person;
		}

		protected bool PER_BirthDate_ReadOnly
		{
			get { return !ViewEditRights.ViewBirthDateForPerson || !ViewEditRights.EditBirthDateForPerson; }
		}

		public override ZString PER_EmailAddress
		{
			get => ViewEditRights.ViewEmailForPerson ? base.PER_EmailAddress : ViewDeniedMessage;
			set => base.PER_EmailAddress = value;
		}

		protected bool PER_EmailAddress_ReadOnly
		{
			get { return !ViewEditRights.ViewEmailForPerson || !ViewEditRights.EditEmailForPerson; }
		}

		public override ZString PER_EmailAddress2
		{
			get => ViewEditRights.ViewEmailForPerson ? base.PER_EmailAddress2 : ViewDeniedMessage;
			set => base.PER_EmailAddress2 = value;
		}

		protected bool PER_EmailAddress2_ReadOnly
		{
			get { return !ViewEditRights.ViewEmailForPerson || !ViewEditRights.EditEmailForPerson; }
		}

		public override ZString PER_MobilePhone
		{
			get => ViewEditRights.ViewMobileForPerson ? base.PER_MobilePhone : ViewDeniedMessage;
			set => base.PER_MobilePhone = value;
		}

		protected bool PER_MobilePhone_ReadOnly
		{
			get { return !ViewEditRights.ViewMobileForPerson || !ViewEditRights.EditMobileForPerson; }
		}

		public override ZString PER_MobilePhone2
		{
			get => ViewEditRights.ViewMobileForPerson ? base.PER_MobilePhone2 : ViewDeniedMessage;
			set => base.PER_MobilePhone2 = value;
		}

		protected bool PER_MobilePhone2_ReadOnly
		{
			get { return !ViewEditRights.ViewMobileForPerson || !ViewEditRights.EditMobileForPerson; }
		}

		public override ZString PER_HomePhone
		{
			get => ViewEditRights.ViewHomePhoneForPerson ? base.PER_HomePhone : ViewDeniedMessage;
			set => base.PER_HomePhone = value;
		}

		protected bool PER_HomePhone_ReadOnly
		{
			get { return !ViewEditRights.ViewHomePhoneForPerson || !ViewEditRights.EditHomePhoneForPerson; }
		}

		public override ZString PER_RN_NKNationalityCodeISO
		{
			get => ViewEditRights.ViewNationalityForPerson ? base.PER_RN_NKNationalityCodeISO : ViewDeniedMessage;
			set => base.PER_RN_NKNationalityCodeISO = value;
		}

		protected bool PER_RN_NKNationalityCodeISO_ReadOnly
		{
			get { return !ViewEditRights.ViewNationalityForPerson || !ViewEditRights.EditNationalityForPerson; }
		}

		public override ZString PER_PersonalInfo
		{
			get => ViewEditRights.ViewPersonalInfoForPerson ? base.PER_PersonalInfo : ViewDeniedMessage;
			set => base.PER_PersonalInfo = value;
		}

		protected bool PER_PersonalInfo_ReadOnly
		{
			get { return !ViewEditRights.ViewPersonalInfoForPerson || !ViewEditRights.EditPersonalInfoForPerson; }
		}

		public override ZString PER_Passport
		{
			get => ViewEditRights.ViewPassportForPerson ? base.PER_Passport : ViewDeniedMessage;
			set => base.PER_Passport = value;
		}

		protected bool PER_Passport_ReadOnly
		{
			get { return !ViewEditRights.ViewPassportForPerson || !ViewEditRights.EditPassportForPerson; }
		}

		public override ZDate PER_PassportExpiryDate
		{
			get => ViewEditRights.ViewPassportForPerson ? base.PER_PassportExpiryDate : ZDate.Empty;
			set => base.PER_PassportExpiryDate = value;
		}

		protected bool PER_PassportExpiryDate_ReadOnly => PER_Passport_ReadOnly;

		public override ZString PER_DriversLicenseNumber
		{
			get => ViewEditRights.ViewDriversLicenseNumberForPerson ? base.PER_DriversLicenseNumber : ViewDeniedMessage;
			set => base.PER_DriversLicenseNumber = value;
		}

		protected bool PER_DriversLicenseNumber_ReadOnly
		{
			get { return !ViewEditRights.ViewDriversLicenseNumberForPerson || !ViewEditRights.EditDriversLicenseNumberForPerson; }
		}

		public override ZString PER_ValidationStatus
		{
			get => ViewEditRights.ViewHomeAddressForPerson ? base.PER_ValidationStatus : ViewDeniedMessage;

			set
			{
				base.PER_ValidationStatus = value;
				isManuallyVerifiedByUser = value == AddressValidationStatus.ManuallyVerified;
			}
		}

		public IEnumerable<ZGuid> PersonIDs
		{
			get
			{
				var query = new ZQuery(GlbMergedPersonSchema.GMP_PER_Person, PK);
				var mergedPersons = Factory.Load<GlbMergedPerson>(query).Select(m => m.GMP_MergedPerson).ToList();
				mergedPersons.Add(PK);
				return mergedPersons;
			}
		}

		#endregion

		Image GetImage(ZBlob blob)
		{
			Image result = null;

			if (PER_Picture == CachedPER_Picture && CachedProfilePictureImage != null && !CachedProfilePictureImage.IsDisposed())
			{
				return CachedProfilePictureImage;
			}

			if (!blob.IsEmpty)
			{
				result = Image.FromStream(new MemoryStream(blob));
			}
			CachedProfilePictureImage = result;
			CachedPER_Picture = PER_Picture;
			return result;
		}

		void SetImage(Image value, SchemaColumn blobColumn)
		{
			if (value != null)
			{
				using (var stream = new MemoryStream())
				{
					value.Save(stream, value.RawFormat.Equals(ImageFormat.MemoryBmp) ? ImageFormat.Bmp : value.RawFormat);
					this[blobColumn] = stream.ToArray();
				}
			}
			else
			{
				this[blobColumn] = ZBlob.Empty;
			}
			CachedProfilePictureImage = value;
			CachedPER_Picture = (ZBlob)this[blobColumn];
		}

		ZBlob CachedPER_Picture;
		Image CachedProfilePictureImage;

		#region Security

		internal ZString ViewDeniedMessage
		{
			get { return Res.GetString("528A0429-7E01-4EDC-8FCF-743F5AE40E37", "** View Denied **"); }
		}

		internal PersonViewEditRights ViewEditRights
		{
			get { return viewEditRights ?? (viewEditRights = new PersonViewEditRights(this)); }
		}
		PersonViewEditRights viewEditRights;

		#endregion

		#region Primary

		public ZString PrimaryWorkplace => ViewEditRights.ViewPrimaryWorkplace ? CompanyName : ViewDeniedMessage;

		public ZString PrimaryWorkplaceCode => ViewEditRights.ViewPrimaryWorkplace ? PrimaryRelationship?.Primary?.Code ?? ZString.Empty : ViewDeniedMessage;

		public ZString PrimaryUNLOCO => ViewEditRights.ViewPrimaryWorkplace ? PrimaryRelationship?.Primary?.UNLOCO ?? ZString.Empty : ViewDeniedMessage;

		public ZString PrimaryJobTitle => ViewEditRights.ViewPrimaryWorkplace ? PrimaryRelationship?.Primary?.JobTitle ?? ZString.Empty : ViewDeniedMessage;

		public ZString PrimaryEmail => ViewEditRights.ViewPrimaryWorkplace ? PrimaryRelationship?.Primary?.Email ?? ZString.Empty : ViewDeniedMessage;

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString PER_PrimaryEmail
		{
			get { return PrimaryEmail; }
			set { }
		}

		public ZPropertyInfo PER_PrimaryEmailInfo
		{
			get { return GetZPropertyInfo(Schema.PER_PrimaryEmail); }
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString PER_PrimaryWorkplace
		{
			get { return PrimaryWorkplace; }
			set { }
		}

		public ZPropertyInfo PER_PrimaryWorkplaceInfo
		{
			get { return GetZPropertyInfo(Schema.PER_PrimaryWorkplace); }
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString PER_WorkingLocation
		{
			get { return PrimaryUNLOCO; }
			set { }
		}

		public ZPropertyInfo PER_WorkingLocationInfo
		{
			get { return GetZPropertyInfo(Schema.PER_WorkingLocation); }
		}

		public IGlbPersonPrimarySource PrimarySource => PrimaryRelationship?.Primary;

		public GlbPersonPrimaryRelationship PrimaryRelationship
		{
			get
			{
				if (primaryRelationship == null)
				{
					var query = new ZQuery(GlbPersonPrimaryRelationshipSchema.PPR_PER, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
					primaryRelationship = Factory.LoadTop1<GlbPersonPrimaryRelationship>(query);
				}

				return primaryRelationship;
			}
		}

		GlbPersonPrimaryRelationship primaryRelationship;

		public void SetPrimaryRelationship(IGlbPersonPrimarySource primary)
		{
			if (PrimaryRelationship == null)
			{
				primaryRelationship = Factory.New<GlbPersonPrimaryRelationship>();
				primaryRelationship.PPR_PER = PK;
			}

			primaryRelationship.Primary = primary;
			HasChanges = true;
			MarkAsNeedingValidation();
		}

		public void RemovePrimaryRelationship()
		{
			if (primaryRelationship != null)
			{
				primaryRelationship.Delete();
				primaryRelationship = null;

				HasChanges = true;
				MarkAsNeedingValidation();
			}
		}

		#endregion

		[BusinessObjectTestExclude] // Company of a person cannot be changed
		public ZString CompanyName
		{
			get => PrimaryRelationship?.Primary?.CompanyName ?? ZString.Empty;
			set { }
		}

		public ZString PER_BirthDateAndAge_Formatted {
			get
			{
				if (!ViewEditRights.ViewBirthDateForPerson)
				{
					return ViewDeniedMessage;
				}

				return PER_Age != -1
				? string.Format(CultureInfo.InvariantCulture, "{0} ({1} {2})", PER_BirthDate.ToShortDateString(), PER_Age, Enterprise.MasterFiles.Business.Res.GetString("GlbPerson|years", "yrs"))
				: string.Empty;
			}
		}

		public ZBool IsMale
		{
			get => PER_Gender == Genders.Man;
			set
			{
				if (value)
				{
					PER_Gender = Genders.Man;
				}
			}
		}

		public ZBool IsFemale
		{
			get => PER_Gender == Genders.Woman;
			set
			{
				if (value)
				{
					PER_Gender = Genders.Woman;
				}
			}
		}

		PersonAssociationsTreeModel personAssociationsTreeModel;
		public PersonAssociationsTreeModel PersonAssociationsTreeModel
		{
			get
			{
				if (personAssociationsTreeModel == null)
				{
					personAssociationsTreeModel = new PersonAssociationsTreeModel(this);
					RegisterEditableChildObject(personAssociationsTreeModel);
				}
				return personAssociationsTreeModel;
			}
			set => personAssociationsTreeModel = value;
		}

		public ZBool IsCreatedFromStaffOrContactOrApplicant { get; set; }

		#region Phone Numbers

		#region Mobile Phone

		PhoneNumber mobilePhoneNumber;
		public PhoneNumber MobilePhoneNumber
		{
			get
			{
				if (mobilePhoneNumber == null)
				{
					mobilePhoneNumber = new PhoneNumber(PER_MobilePhone_FormattedInfo, null, PER_MobilePhone_FormattedLocalNumberIfLoggedInSameCountryInfo, PER_MobilePhone_IsManuallyVerifiedInfo);
				}
				return mobilePhoneNumber;
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString PER_MobilePhone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(PER_MobilePhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(PER_MobilePhoneInfo, PER_MobilePhone_FormattedInfo, value, Validation.ValidatePER_MobilePhone_Formatted, PER_MobilePhone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo PER_MobilePhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.PER_MobilePhone_Formatted); }
		}

		protected bool PER_MobilePhone_Formatted_ReadOnly
		{
			get { return !ViewEditRights.ViewMobileForPerson || !ViewEditRights.EditMobileForPerson; }
		}

		public ZString PER_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewEditRights.ViewMobileForPerson ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(PER_MobilePhoneInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo PER_MobilePhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.PER_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool PER_MobilePhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbPersonSchema.Constants.PER_MobilePhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(PER_MobilePhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbPersonSchema.Constants.PER_MobilePhone, Validation.ValidatePER_MobilePhone_Formatted, MobilePhoneNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo PER_MobilePhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.PER_MobilePhone_IsManuallyVerified); }
		}

		#endregion

		#region PER_HomePhone

		PhoneNumber homePhoneNumber;
		[DocumentMacroIgnore]
		public PhoneNumber HomePhoneNumber
		{
			get
			{
				if (homePhoneNumber == null)
				{
					homePhoneNumber = new PhoneNumber(PER_HomePhone_FormattedInfo, null, PER_HomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo, PER_HomePhone_IsManuallyVerifiedInfo);
				}
				return homePhoneNumber;
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString PER_HomePhone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(PER_HomePhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(PER_HomePhoneInfo, PER_HomePhone_FormattedInfo, value, Validation.ValidatePER_HomePhone_Formatted, PER_HomePhone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo PER_HomePhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.PER_HomePhone_Formatted); }
		}

		protected bool PER_HomePhone_Formatted_ReadOnly
		{
			get { return !ViewEditRights.ViewHomePhoneForPerson || !ViewEditRights.EditHomePhoneForPerson; }
		}

		public ZString PER_HomePhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewEditRights.ViewHomePhoneForPerson ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(PER_HomePhoneInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo PER_HomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.PER_HomePhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool PER_HomePhone_IsManuallyVerified
		{
			get { return ViewEditRights.ViewHomePhoneForPerson && PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbPersonSchema.Constants.PER_HomePhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(PER_HomePhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbPersonSchema.Constants.PER_HomePhone, Validation.ValidatePER_HomePhone_Formatted, HomePhoneNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo PER_HomePhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.PER_HomePhone_IsManuallyVerified); }
		}

		#endregion

		#endregion

		#region PER_FaxNum

		public override ZString PER_FaxNumber
		{
			get => ViewEditRights.ViewFaxNumberForPerson ? base.PER_FaxNumber : ViewDeniedMessage;
			set => base.PER_FaxNumber = value;
		}

		[DocumentMacroIgnore]
		public PhoneNumber FaxNumber
		{
			get
			{
				if (faxNumber == null)
				{
					faxNumber = new PhoneNumber(PER_FaxNum_FormattedInfo, null, PER_FaxNum_FormattedLocalNumberIfLoggedInSameCountryInfo, PER_FaxNum_IsManuallyVerifiedInfo);
				}
				return faxNumber;
			}
		}
		PhoneNumber faxNumber;

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString PER_FaxNum_Formatted
		{
			get { return ViewEditRights.ViewFaxNumberForPerson ? PhoneNumberPropertyHelper.GetPhoneNumber(PER_FaxNumberInfo) : ViewDeniedMessage; }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(PER_FaxNumberInfo, PER_FaxNum_FormattedInfo, value, Validation.ValidatePER_FaxNum_Formatted, PER_FaxNum_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo PER_FaxNum_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.PER_FaxNum_Formatted); }
		}

		public ZString PER_FaxNum_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewEditRights.ViewFaxNumberForPerson ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(PER_FaxNumberInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo PER_FaxNum_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.PER_FaxNum_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool PER_FaxNum_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbPersonSchema.Constants.PER_FaxNumber, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(PER_FaxNum_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbPersonSchema.Constants.PER_FaxNumber, Validation.ValidatePER_FaxNum_Formatted, FaxNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo PER_FaxNum_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.PER_FaxNum_IsManuallyVerified); }
		}

		#endregion

		#region PER_ChallengePhrase

		public override ZString PER_ChallengePhrase
		{
			get => ViewEditRights.ViewChallengePhraseForPerson ? base.PER_ChallengePhrase : ViewDeniedMessage;
			set => base.PER_ChallengePhrase = value;
		}

		public override ZString PER_ChallengePhraseType
		{
			get => ViewEditRights.ViewChallengePhraseForPerson ? base.PER_ChallengePhraseType : ViewDeniedMessage;
			set => base.PER_ChallengePhraseType = value;
		}

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

		#endregion

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelperThunk.Value; }
		}

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperThunk;

		internal ZString DefaultCountryCodeForPhoneNumbers
		{
			get { return PER_RN_NKCountry; }
		}

		#region Collections

		OrgContactCollection contactCollection;
		public OrgContactCollection ContactCollection
		{
			get
			{
				if (contactCollection == null)
				{
					var query = new ZQuery(OrgContactSchema.OC_PER, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
					var localContactCollection = new OrgContactCollection(Factory, query);
					localContactCollection.Load();
					contactCollection = localContactCollection;
				}

				return contactCollection;
			}
		}

		GlbStaffCollection staffCollection;
		public GlbStaffCollection StaffCollection
		{
			get
			{
				if (staffCollection == null)
				{
					staffCollection = GetGlbStaffCollection();
				}

				return staffCollection;
			}
		}

		GlbStaffCollection GetGlbStaffCollection()
		{
			var query = new ZQuery(GlbStaffSchema.GS_PER, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
			return new GlbStaffCollection(Factory, query);
		}

		public void ReloadGlbStaffCollectionFromDb()
		{
			staffCollection = GetGlbStaffCollection();
			staffCollection.RefreshFromDb();
		}

		BusinessObjectCollection applicantCollection;
		public BusinessObjectCollection ApplicantCollection
		{
			get
			{
				if (applicantCollection == null)
				{
					var query = new ZQuery(HRJobApplicantSchema.HA_PER, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
					applicantCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IHRJobApplicantCollection>(), this, query);
					applicantCollection.Load();
				}

				return applicantCollection;
			}
		}

		public ZBool HasAssociation => IsCreatedFromStaffOrContactOrApplicant ? IsCreatedFromStaffOrContactOrApplicant : (ZBool)(ContactCollection.Count + StaffCollection.Count + ApplicantCollection.Count != 0);

		IGlbAccreditationAttemptCollection accreditationAttemptCollection;

		[ChildEditable(true)]
		public IGlbAccreditationAttemptCollection AccreditationAttemptCollection
		{
			get
			{
				if (accreditationAttemptCollection == null)
				{
					accreditationAttemptCollection = (IGlbAccreditationAttemptCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbAccreditationAttemptCollection>(), this);
					(accreditationAttemptCollection as BusinessObjectCollection).Load();
					RegisterEditableChildObject(accreditationAttemptCollection);
				}

				return accreditationAttemptCollection;
			}
		}

		public IGlbAccreditationAttempt GetCurrentAttempt(ZGuid accreditationPk, ZDate currentDate)
		{
			foreach (IGlbAccreditationAttempt attempt in AccreditationAttemptCollection)
			{
				if (attempt.HAA_HAC == accreditationPk && !attempt.IsCompleted && !attempt.CheckIsExpired(currentDate))
				{
					return attempt;
				}
			}

			return null;
		}

		public IGlbAccreditationAttempt GetLastAttempt(ZGuid accreditationPk, bool completedOnly = false)
		{
			return GetLastAttempt(accreditationPk, ZGuid.Empty, completedOnly);
		}

		public IGlbAccreditationAttempt GetLastAttempt(ZGuid accreditationPk, ZGuid excludeAttempt, bool completedOnly = false)
		{
			IGlbAccreditationAttempt result = null;
			foreach (IGlbAccreditationAttempt attempt in AccreditationAttemptCollection)
			{
				if (attempt.PK != excludeAttempt && attempt.HAA_HAC == accreditationPk)
				{
					if (result == null || attempt.HAA_CommencementDate > result.HAA_CommencementDate && (!completedOnly || attempt.HAA_CompletionDate.IsValid))
					{
						result = attempt;
					}
				}
			}

			return result;
		}

		public void DeleteAttempts(BusinessObject[] accreditationAttempts)
		{
			foreach (var attempt in accreditationAttempts)
			{
				AccreditationAttemptCollection.Delete(attempt);
			}

			HasChanges = true;
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("EFE33161-AFB2-4AA8-ABBE-E7E4F377021A", "Person");
				if (!IsDeleted && !PER_FullName.IsEmpty)
				{
					result += " - " + PER_FullName;
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

		protected override GlbPersonLookups GetNewLookups()
		{
			return new GlbPersonLookups(this);
		}

		public new GlbPersonLookups Lookups
		{
			get { return base.Lookups; }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "logging information")]
		const string ChangedPropertyTemplate = "Changed {0}.";
		public override void OnSaving()
		{
			base.OnSaving();

			if (IsInDatabase)
			{
				AddLogIfChanged(PER_FullNameInfo, PER_FullNameInternal);
				AddLogIfChanged(PER_LegalNameInfo, PER_LegalNameInternal);
				AddLogIfChanged(PER_BirthDateInfo, PER_BirthDateInternal);
				AddLogIfChanged(PER_EmailAddressInfo, PER_EmailAddressInternal);
				AddLogIfChanged(PER_MobilePhoneInfo, PER_MobilePhoneInternal);
				AddLogIfChanged(PER_HomePhoneInfo, PER_HomePhoneInternal);
			}

			if (!string.IsNullOrEmpty(referenceForDDR))
			{
				var ddrLog = Logs.AddNew(new EventValue(Events.DuplicateDetectedForReview));
				using (((IUpdateFieldsLock)ddrLog).LockForUpdatingKeyFields())
				{
					ddrLog.SL_Reference = referenceForDDR;
					ddrLog.SL_EventTime = eventTimeForDDR;
				}

				referenceForDDR = null;
			}

			if (!IsDeleted)
			{
				StaffCollection.Where(staff => staff.PK != InfoPropagationSourcePk).ForEach(staff => staff.UpdateFromPerson(this));
				ContactCollection.Cast<OrgContact>().Where(contact => contact.PK != InfoPropagationSourcePk).ForEach(contact => contact.UpdateFromPerson(this));
				ApplicantCollection.Cast<IHRJobApplicant>().Where(applicant => applicant.PK != InfoPropagationSourcePk).ForEach(applicant => applicant.UpdateFromPerson(this));

				if (IsPasswordBeingCleared())
				{
					Logs.AddNew(AutoEvents.WebAccessPasswordChanged, $"Password was Reset, user = {Env.CurrentUser.PK}", ZDateTimeOffset.Now);
				}
			}
		}

		bool IsPasswordBeingCleared() => !PER_PasswordHashInfo.OriginalValue.IsEmpty && PER_PasswordHash.IsEmpty;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			ShouldUpdateMobileOnRelatedRecords = true;
			if (saveSucceeded)
			{
				InfoPropagationSourcePk = ZGuid.Empty;
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		void AddLogIfChanged(ZPropertyInfo info, IZType currentValue)
		{
			if (info.HasChanges && info.OriginalValue != info.Value && !info.Value.IsEmpty && info.Value.ToString() != ViewDeniedMessage)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord,
					string.Format(CultureInfo.InvariantCulture, ChangedPropertyTemplate, info.Name));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		#region Create and Update

		public static GlbPerson CreateFromStaff(BusinessObjectFactory factory, GlbStaff staff)
		{
			var person = factory.New<GlbPerson>();
			staff.GS_PER = person.PK;
			person.IsCreatedFromStaffOrContactOrApplicant = true;
			person.UpdateFromStaff(staff);
			CreateTasksAndMilestonesFromTemplateIfRequired(person);
			person.SetPrimaryRelationship(staff);

			return person;
		}

		public void UpdateFromStaff(GlbStaff s)
		{
			if (IsMovingFromAnotherPerson)
			{
				return;
			}

			InfoPropagationSourcePk = s.PK;

			PER_FullName = s.GS_FullNameInternal.Left(Schema.PER_FullNameMaxLength);
			if (PER_FullNameInternal.IsEmpty)
			{
				PER_FullName = EmptyFullName;
			}

			PER_BirthDate = s.GS_BirthdateInternal;
			var gender = s.GS_GenderInternal.Left(1);
			PER_Gender = Lookups.Genders.ContainsCode(gender) ? gender : (ZString)Genders.NotSpecified;
			PER_NameSuffix = s.GS_NameSuffix;
			PER_HomePhone = s.GS_HomePhoneInternal;
			PER_FaxNumber = s.GS_FaxNumInternal;
			PER_RN_NKNationalityCodeISO = s.GS_RN_NKNationalityCodeInternal.Length <= Schema.PER_RN_NKNationalityCodeISOMaxLength
				? s.GS_RN_NKNationalityCodeInternal
				: ZString.Empty;

			PER_NameTitle = s.GS_NameTitle;

			if (IsOriginalMobileSameAsPerson(s.GS_MobilePhoneInfo) || s.GS_MobilePhoneInternal.IsEmpty)
			{
				PER_MobilePhone = s.GS_MobilePhoneInternal;
			}

			PER_PreferredLanguage = s.GS_WorkingLanguage;
			PER_FriendlyName = s.GS_FriendlyName;
			PER_PreferredLanguage = s.GS_WorkingLanguage;
			PER_Picture = s.GS_ProfilePhotoInternal;

			PER_HomeAddress1 = s.GS_UserAddress1Internal.Left(Schema.PER_HomeAddress1MaxLength);
			PER_HomeAddress2 = s.GS_UserAddress2Internal;
			PER_City = s.GS_CityInternal.Left(Schema.PER_CityMaxLength);
			PER_Postcode = s.GS_PostcodeInternal.Left(Schema.PER_PostcodeMaxLength);
			PER_State = s.GS_StateInternal.Left(Schema.PER_StateMaxLength);
			PER_RN_NKCountry = s.GS_RN_NKCountryCodeInternal.Left(Schema.PER_RN_NKCountryMaxLength);

			//TODO: to be removed and transferred to GenCert
			var pp = s.Certificates.Find(c => c.XZ_Type == StaffCertificateType.PAS).FirstOrDefault();  // PAS=passport
			PER_Passport = pp?.XZ_RefNumber.Left(Schema.PER_PassportMaxLength) ?? s.GS_PassportInternal;
			PER_PassportPlaceOfIssue = pp?.XZ_RN_NKCountryOfIssuance.Left(Schema.PER_PassportPlaceOfIssueMaxLength) ?? ZString.Empty;
			PER_PassportExpiryDate = pp?.XZ_ExpiryOrDueDate.Date ?? ZDate.Empty;
			PER_DriversLicenseNumber = s.Certificates.Find(c => c.XZ_Type == StaffCertificateType.NID).FirstOrDefault()?.XZ_RefNumber.Left(Schema.PER_DriversLicenseNumberMaxLength) ?? ZString.Empty;  // CAR = driving licence
		}

		public static GlbPerson CreateFromContact(BusinessObjectFactory factory, OrgContact contact)
		{
			var person = factory.New<GlbPerson>();
			contact.OC_PER = person.PK;
			person.IsCreatedFromStaffOrContactOrApplicant = true;
			person.UpdateFromContact(contact);
			CreateTasksAndMilestonesFromTemplateIfRequired(person);
			person.SetPrimaryRelationship(contact);

			return person;
		}

		public void UpdateFromContact(OrgContact contact, bool forceUpdate = false)
		{
			if (IsMovingFromAnotherPerson)
			{
				return;
			}

			var isCreatingNew = !contact.IsInDatabase;
			var shouldSyncRegardlessOfHasChanges = forceUpdate || isCreatingNew;
			InfoPropagationSourcePk = contact.PK;

			if (shouldSyncRegardlessOfHasChanges || contact.OC_ContactNameInfo.HasChanges)
			{
				if (!PER_FullNameInternal.EqualsIgnoringCase(contact.ContactNameWithoutNumberSuffix))
				{
					PER_FullName = contact.ContactNameWithoutNumberSuffix.Left(Schema.PER_FullNameMaxLength);
				}
			}

			if (PER_FullNameInternal.IsEmpty)
			{
				PER_FullName = EmptyFullName;
			}

			if (shouldSyncRegardlessOfHasChanges || !StaffCollection.Any())
			{
				SetValue(contact.OC_BirthdayInternalHasChanges, !shouldSyncRegardlessOfHasChanges, () => PER_BirthDate = contact.OC_BirthdayInternal.Date);
				SetValue(contact.OC_GenderInternalHasChanges, !shouldSyncRegardlessOfHasChanges, () =>
				{
					var gender = contact.OC_GenderInternal.Left(1);
					PER_Gender = Lookups.Genders.ContainsCode(gender) ? gender : (ZString)Genders.NotSpecified;
				});

				SetValue(contact.OC_HomePhoneInfo, !shouldSyncRegardlessOfHasChanges, (x) => PER_HomePhone = ((ZString)x.Value).Left(Schema.PER_HomePhoneMaxLength));
				SetValue(contact.OC_RN_NKNationalityInternalHasChanges, !shouldSyncRegardlessOfHasChanges, () => PER_RN_NKNationalityCodeISO = contact.OC_RN_NKNationalityInternal.Length <= Schema.PER_RN_NKNationalityCodeISOMaxLength ? contact.OC_RN_NKNationalityInternal : ZString.Empty);
				SetValue(contact.OC_PersonalInfoInternalHasChanges, !shouldSyncRegardlessOfHasChanges, () => PER_PersonalInfo = contact.OC_PersonalInfoInternal.Left(Schema.PER_PersonalInfoMaxLength));
				SetValue(contact.OC_MobileInfo, !isCreatingNew, (x) =>
				{
					if (IsOriginalMobileSameAsPerson(x) || ((ZString)x.Value).IsEmpty)
					{
						PER_MobilePhone = ((ZString)x.Value).Left(Schema.PER_MobilePhoneMaxLength);
					}
				});
			}

			if (!ContactCollection.Contains(contact))
			{
				ContactCollection.Add(contact);
			}
		}

		static void SetValue(ZPropertyInfo source, bool changedOnly, Action<ZPropertyInfo> setter)
		{
			if (!changedOnly || source.HasChanges)
			{
				setter(source);
			}
		}

		static void SetValue(bool hasChanges, bool changedOnly, Action setter)
		{
			if (!changedOnly || hasChanges)
			{
				setter();
			}
		}

		public static GlbPerson DetachAndCreateFromApplicant(BusinessObjectFactory factory, IHRJobApplicant applicant)
		{
			ZGuid prevPerson = applicant.HA_PER;
			applicant.HA_PER = ZGuid.Empty;
			return CreateFromApplicant(factory, applicant, prevPerson);
		}

		public static GlbPerson CreateFromApplicant(BusinessObjectFactory factory, IHRJobApplicant applicant)
		{
			return CreateFromApplicant(factory, applicant, ZGuid.Empty);
		}

		static GlbPerson CreateFromApplicant(BusinessObjectFactory factory, IHRJobApplicant applicant, ZGuid detachedPerson)
		{
			if (!applicant.HA_PER.IsEmpty)
			{
				return applicant.Person as GlbPerson;
			}

			var person = GetPersonFromRelatedContactOrStaff(factory, applicant);
			if (person != null && applicant.HA_PER != person.PK && person.PK != detachedPerson)
			{
				applicant.HA_PER = person.PK;
			}
			else
			{
				person = factory.New<GlbPerson>();
				applicant.HA_PER = person.PK;
				person.IsCreatedFromStaffOrContactOrApplicant = true;
				person.UpdateFromApplicant(applicant);
				CreateTasksAndMilestonesFromTemplateIfRequired(person);
			}

			return person;
		}

		public void UpdateFromApplicant(IHRJobApplicant a)
		{
			if (IsMovingFromAnotherPerson)
			{
				return;
			}

			InfoPropagationSourcePk = a.PK;

			if (!a.IsLearningCenterUser)
			{
				if ((PER_EmailAddress.IsEmpty || ((ZString)a.HA_EmailAddressInfo.OriginalValue).EqualsIgnoringCase(PER_EmailAddressInternal)) && !PER_EmailAddressInternal.EqualsIgnoringCase(a.HA_EmailAddress))
				{
					PER_EmailAddress = a.HA_EmailAddress.Left(Schema.PER_EmailAddressMaxLength);
				}
			}
		}

		public bool IsOriginalMobileSameAsPerson(ZPropertyInfo mobileInfo)
		{
			return mobileInfo != null && mobileInfo.HasChanges && mobileInfo.OriginalValue.ToString() == PER_MobilePhoneInternal;
		}

		public bool MobileExistsOnMultipleRelatedRecords(string mobile1, string mobile2)
		{
			var existsCount = StaffCollection.Count(s => s.GS_MobilePhoneInternal == mobile1 || s.GS_MobilePhoneInternal == mobile2);
			if (existsCount > 1)
			{
				return true;
			}

			existsCount += ContactCollection.Cast<OrgContact>().Count(c => c.OC_Mobile == mobile1 || c.OC_Mobile == mobile2);
			if (existsCount > 1)
			{
				return true;
			}

			return false;
		}

		public bool ShouldUpdateMobileOnRelatedRecords { get; set; } = true;

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			StaffCollection.ForEach(x => ((IPatternCleanup)x).DeleteAllPatterns());
			ContactCollection.ForEach(x => ((IPatternCleanup)x).DeleteAllPatterns());
			ApplicantCollection.ForEach(x => ((IPatternCleanup)x).DeleteAllPatterns());
			DeleteAllPatterns();
			if (PrimaryRelationship != null)
			{
				RemovePrimaryRelationship();
			}
			base.Delete();
		}

		public void RemoveFromRecentItems()
		{
			var shortcut = Factory.LoadTop1<StmLink>(new ZQuery(StmLinkSchema.STL_ItemPK, PK));
			if (shortcut != null)
			{
				var wrapper = new LinkWrapper(shortcut);
				RecentItemManager.Instance.RemoveFromRecentItems(nameof(GlbPerson), wrapper);
				RecentItemManager.Instance.RemoveFromRecentItems(string.Empty, wrapper);
			}
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

		static void CreateTasksAndMilestonesFromTemplateIfRequired(GlbPerson person)
		{
			if (person.Factory.IsInTransaction)
			{
				new ProcessTask.Loader(person.Factory).CreateTasksAndMilestonesFromTemplateIfRequired(person, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}
		}

		#endregion

		#region Get From Related Contact

		static GlbPerson GetPersonFromRelatedContactOrStaff(BusinessObjectFactory factory, IHRJobApplicant applicant)
		{
			var relatedContactPk = GetRelatedContact(factory, applicant);

			if (!relatedContactPk.IsEmpty)
			{
				var contact = factory.Load<OrgContact>(relatedContactPk);
				if (contact != null)
				{
					return contact.Person;
				}
			}

			var relatedStaffPk = GetRelatedStaff(factory, applicant);

			if (!relatedStaffPk.IsEmpty)
			{
				var staff = factory.Load<GlbStaff>(relatedStaffPk);
				if (staff != null)
				{
					return staff.Person;
				}
			}

			return null;
		}

		static ZGuid GetRelatedContact(BusinessObjectFactory factory, IHRJobApplicant applicant)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, applicant.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.EditedARecord.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Related Contact");

			var logs = new StmALogCollection(factory, query);
			logs.Load();

			foreach (StmALog log in logs)
			{
				if (log.SL_Reference.Length >= GuidStringLength)
				{
					if (ZGuid.TryParse(log.SL_Reference.Substring(log.SL_Reference.Length - GuidStringLength, GuidStringLength), out var relatedContactPk))
					{
						return relatedContactPk;
					}
				}
			}

			return ZGuid.Empty;
		}

		static ZGuid GetRelatedStaff(BusinessObjectFactory factory, IHRJobApplicant applicant)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, applicant.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.EditedARecord.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Related Staff");

			var logs = new StmALogCollection(factory, query);
			logs.Load();

			foreach (var log in logs.Cast<StmALog>())
			{
				if (log.SL_Reference.Length >= GuidStringLength)
				{
					if (ZGuid.TryParse(log.SL_Reference.Substring(log.SL_Reference.Length - GuidStringLength, GuidStringLength), out var relatedStaffPk))
					{
						return relatedStaffPk;
					}
				}
			}

			return ZGuid.Empty;
		}

		static int GuidStringLength => 36;

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
		GenRegCertAccredMaintListCollection certificates;

		ICodeDescriptionPairList ICertificatesProvider.GetCertificateTypeList()
		{
			return CertificateTypePairList.GetCertificateTypesIncludingProvided();
		}

		public ICodeDescriptionPairList GetActiveCertificateTypeList()
		{
			//Not a registry defined list
			return ((ICertificatesProvider)this).GetCertificateTypeList();
		}

		ZString ICertificatesProvider.GetDefaultDescription(ZString code)
		{
			return CertificateTypePairList.GetCertificateTypesIncludingProvided().GetDescriptionFromCode(code);
		}

		#endregion

		#region ISupportWebAddressValidation

		public ZString ValidationStatus
		{
			get
			{
				return PER_ValidationStatus;
			}
			set
			{
				PER_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();
			}
		}

		public event EventHandler TriggerWebAddressValidation;
		public event EventHandler AddressValidationStatusChanged;
		public event EventHandler TriggerWebGetCityTown;

		[BusinessObjectTestExclude]
		public ZString Language
		{
			get { return Constants.Languages.English; }
			set { }
		}

		public ZPropertyInfo LanguageInfo { get => GetZPropertyInfo(nameof(Language)); }

		public int Language_MaxLength { get { return AutoGlbPerson.Schema.PER_PreferredLanguageMaxLength; } }

		public CodeDescriptionPairList LanguageList { get { return null; } }

		[DocumentFieldExcludeFromMap]
		[BusinessObjectTestExclude]
		public ZString UnrestrictedAdditionalAddressInformation { get; set; }

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => GetZPropertyInfo(nameof(UnrestrictedAdditionalAddressInformation));

		[BusinessObjectTestExclude]
		public CodeDescriptionPairList AdditionalAddressInfoList { get; }

		public ZString AddressCode { get; set; }

		public ZString Address1 { get => PER_HomeAddress1; set => PER_HomeAddress1 = value; }

		public ZPropertyInfo Address1Info => PER_HomeAddress1Info;

		public int Address1_MaxLength => PER_HomeAddress1Info.MaxLength;

		public ZString Address2 { get => PER_HomeAddress2; set => PER_HomeAddress2 = value; }

		public ZPropertyInfo Address2Info => PER_HomeAddress2Info;

		public int Address2_MaxLength => PER_HomeAddress2Info.MaxLength;

		public ZString City { get => PER_City; set => PER_City = value; }

		public ZPropertyInfo CityInfo => PER_CityInfo;

		public int City_MaxLength => PER_CityInfo.MaxLength;

		public ZString Postcode { get => PER_Postcode; set => PER_Postcode = value; }

		public ZPropertyInfo PostcodeInfo => PER_PostcodeInfo;

		public int Postcode_MaxLength => PER_PostcodeInfo.MaxLength;

		public ZPropertyInfo CompanyNameInfo => GetZPropertyInfo(nameof(CompanyName));

		public int CompanyName_MaxLength => 0;

		public ZString StateCode { get => PER_State; set => PER_State = value; }

		public ZPropertyInfo StateCodeInfo => PER_StateInfo;

		public int StateCode_MaxLength => PER_StateInfo.MaxLength;

		public CodeDescriptionPairList StateCodeList => Lookups.StateList;

		[BusinessObjectTestExclude] // State has to be valid
		public ZString State
		{
			get { return ViewEditRights.ViewHomeAddressForPerson ? StateCodeList.GetDescriptionFromCode(PER_State) : ViewDeniedMessage.ToString(); }
			set
			{
				var code = (ZString)StateCodeList.GetCodeFromDescription(value);
				PER_State = string.IsNullOrEmpty(code) ? value : code;
				StateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		public int State_MaxLength => PER_StateInfo.MaxLength;

		public ZString CountryCodeISO2 { get => PER_RN_NKCountry; set => PER_RN_NKCountry = value; }

		public int CountryCodeISO2_MaxLength => PER_RN_NKCountryInfo.MaxLength;

		[Obsolete("Please use Country property. This will be removed once references in client-defined macros (Documents, Workflows, Notes, etc) are transformed.")]
		[MacroIgnore]
		public RefCountry CountryCode => Country;

		public RefCountryCollection CountryCodeList => Lookups.Countries;

		public ZString DisplayText { get => PER_FullName; set => throw new NotImplementedException(); }

		public ZGuid EntityPK => PK;

		public ZString AddressRecordGUID => PK.ToString();

		public ZString AddressSourceTable => GlbPersonSchema.Constants.Prefix;

		public ZString AddressMap
		{
			get { return ViewEditRights.ViewHomeAddressForPerson ? addressMap : ViewDeniedMessage; }
			set { addressMap = value; }
		}
		ZString addressMap;

		public ZString Addressee => PER_FullName;

		public ZGeography GeoLocation { get; set; }

		public ZString ClosestPort { get; set; }

		public bool NeedValidation
		{
			get
			{
				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, PER_RN_NKCountry);
				if (country != null)
				{
					if (!PER_HomeAddress1.IsEmpty && !PER_Postcode.IsEmpty && !PER_City.IsEmpty && !PER_State.IsEmpty)
					{
						if (!IsInDatabase || (PER_HomeAddress1Info.HasChanges || PER_HomeAddress2Info.HasChanges || PER_PostcodeInfo.HasChanges ||
																	PER_CityInfo.HasChanges || PER_StateInfo.HasChanges || PER_RN_NKCountryInfo.HasChanges))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool IsUpdatingCityTown { get; set; }
		public bool IsValidatingAddress { get; set; }
		public bool IsExactPointFound { get; set; }
		public bool IsValidatedByBackgroundService { get; set; }

		public bool IsErrorSuppressed => false;
		public bool IsJobDocAddress => false;
		public bool IsTSAKnownAddress => false;
		public bool IsMIDAddress => false;

		void RaiseAddressValidationStatusChanged()
		{
			if (AddressValidationStatusChanged != null)
			{
				AddressValidationStatusChanged(this, EventArgs.Empty);
			}
		}

		public bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return false;
		}

		public async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			return await AddressValidationService.ValidateAddressAsync(this, cancellationToken, false, cleanseAction);
		}

		public async Task<CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken)
		{
			return await AddressValidationService.GetCityTownAsync(this, cancellationToken);
		}

		bool isManuallyVerifiedByUser;

		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			if (isManuallyVerifiedByUser)
			{
				return;
			}

			if (Env.CurrentCompany != null && Env.Registry.EnableAddressValidationWebService)
			{
				if (ValidationStatus != AddressValidationStatus.CountryNotAvailable && Country != null && OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Country.PK.ToGuid(), ValidationSection))
				{
					ValidationStatus = AddressValidationStatus.ToBeVerified;
					AddressMap = string.Empty;
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					if (!string.IsNullOrEmpty(registrationKey.SystemId))
					{
						RaiseWebServices(propertyInfo);
					}
				}
				else if (propertyInfo.Name == nameof(PER_RN_NKCountry))
				{
					ValidationStatus = AddressValidationStatus.ToBeVerified;
					AddressMap = string.Empty;
				}
			}
		}

		public AddressValidationSection ValidationSection { get; } = AddressValidationSection.Person;

		void RaiseWebServices(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(PER_RN_NKCountry))
			{
				if ((string.IsNullOrEmpty(PER_City) || string.IsNullOrEmpty(PER_State)
					|| string.IsNullOrEmpty(PER_Postcode)) && (propertyInfo == PER_CityInfo
					|| propertyInfo == PER_StateInfo || propertyInfo == PER_PostcodeInfo))
				{
					RaiseTriggerWebGetCityTown(propertyInfo);
				}
				else
				{
					RaiseTriggerWebAddressValidation(propertyInfo);
				}
			}
		}

		void RaiseTriggerWebAddressValidation(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebAddressValidation != null)
			{
				TriggerWebAddressValidation(this, new InfoEventArgs(propertyInfo));
			}
		}

		void RaiseTriggerWebGetCityTown(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebGetCityTown != null)
			{
				TriggerWebGetCityTown(this, new InfoEventArgs(propertyInfo));
			}
		}

		public void PreValidationForAddressValidationService()
		{
			Validation.ValidatePER_HomeAddress1();
			Validation.ValidatePER_City();
			Validation.ValidatePER_RN_NKCountry();
			ValidatePostcodeAndStateForAddress();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Validation.ValidatePER_Postcode();
			Validation.ValidatePER_State();
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

		#endregion

		#region IDeduplicationValidation Members

		public bool IsDuplicateFound { get; private set; }

		bool IDeduplicatable.IsDeduplicationStarted { get; set; }

		public event EventHandler<IDuplicationEventArgs> DuplicationDetected;

		public void PropagateDeduplication(IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModel, object targetList)
		{
			if (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value)
			{
				DuplicationDetected?.Invoke(this, ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", this, targetList, results, resultsModel));
			}
		}

		public void PropagateDeduplicationEnded<TBizo>(IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModel, object targetObjects, DuplicationStatus lastRunStatus, DeduplicationExclusionManager<TBizo> exclusionManager)
			where TBizo : BusinessObject, IDeduplicatable
		{
			var manager = new DeduplicationExclusionManager<GlbPerson>();

			exclusionManager.BuildDisplay();
			manager.ItemsCount = exclusionManager.ItemsCount;
			manager.DisplayInfo = exclusionManager.DisplayInfo;

			DeduplicationEnded?.Invoke(this, ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", this, targetObjects, results, resultsModel, lastRunStatus, manager));
		}

		public void PropagateDeduplicationActionOccurred(DeduplicationAction userAction, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels, object targetList, bool isExcludingInactiveFromResults = false, bool isExcludingOtherCountriesFromResults = false, bool isShowIgnoredFromResults = true)
		{
			if (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value)
			{
				var eventArgs = ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", this, targetList, results, resultsModels);
				eventArgs.InvokedAction = userAction;
				eventArgs.IsExcludingInactiveFromResults = isExcludingInactiveFromResults;
				eventArgs.IsExcludingOtherCountriesFromResults = isExcludingOtherCountriesFromResults;
				eventArgs.IsShowIgnoredFromResults = isShowIgnoredFromResults;

				DeduplicationActionOccurred?.Invoke(this, eventArgs);
			}
		}

		public void PropagateDeduplicationStarted()
		{
			DeduplicationStarted?.Invoke(this, EventArgs.Empty);
			((IDeduplicatable)this).IsDeduplicationStarted = true;
		}

		public event EventHandler DeduplicationStarted;
		public event EventHandler<IDuplicationEventArgs> DeduplicationEnded;
		public event EventHandler<IDuplicationEventArgs> DeduplicationActionOccurred;

		public void ValidateDuplicationResult(bool isDuplicatesFound)
		{
			IsDuplicateFound = isDuplicatesFound;

			if (!IsDeleted)
			{
				Validation.ValidatePER_FullName();
				DeduplicationChildBizO?.RunPreSaveValidation();
			}
		}

		public List<ISupportDuplicationFinder> GetSupportedDuplicationFinders(Type targetType, DeduplicationProxyConfig config)
		{
			return ObjectFactory.Get<IMasterDataProvider>().GetPersonSupportedDuplicationFinders(this, config);
		}

		bool MyTestProperty { get; set; }

		bool shouldRunDeduplication;
		bool IDeduplicatable.ShouldRunDeduplication
		{
			get
			{
				return shouldRunDeduplication;
			}
			set
			{
				shouldRunDeduplication = value;
			}
		}

		public ZString DeduplicationCountryCode => DefaultCountryCodeForPhoneNumbers;

		bool IDeduplicatable.IsExcludedFromDeduplication
		{
			get
			{
				var dedupPerson = Factory.Load<IDeduplicationPerson>(PK);
				(dedupPerson as BusinessObject).Reload();
				return dedupPerson.DPE_Status == DeduplicationHelper.StatusConstants.Excluded;
			}
			set
			{
				ObjectFactory.Get<IMasterDataProvider>().ComputeIsExcludedFromDeduplication(this, value);
			}
		}

		string IDeduplicatable.Info => GetTypeInfo();
		string IDeduplicatable.FullName => PER_FullName;
		bool IDeduplicatable.IsActive => PER_IsActive;
		Type IDeduplicatable.BizoType => typeof(GlbPerson);
		ZString IDeduplicatable.NaturalKey => PER_FullName;
		public ZString TypeInfo => GetTypeInfo();

		string GetTypeInfo()
		{
			var typesList = new List<string>();

			if (ContactCollection.Count > 0)
			{
				typesList.Add(Res.GetString("84242C2F-B768-47F7-90ED-67387EE89F92", "Contact ({0})", ContactCollection.Count.ToString(CultureInfo.InvariantCulture)));
			}

			if (StaffCollection.Count > 0)
			{
				typesList.Add(Res.GetString("D5AE75DF-F737-42F2-9851-7266B287967A", "Staff ({0})", StaffCollection.Count.ToString(CultureInfo.InvariantCulture)));
			}

			if (ApplicantCollection.Count > 0)
			{
				typesList.Add(Res.GetString("B6DF316B-7739-4051-89E2-824402D1C7B8", "Applicant ({0})", ApplicantCollection.Count.ToString(CultureInfo.InvariantCulture)));
			}

			return string.Join(", ", typesList);
		}

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		ProcessTaskCollection workflowItems;

		[ChildEditable]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GlbPersonProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new GlbPersonWorkflowDescriptor().Code; }
		}

		#endregion

		#region Doc Manager

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new GlbPersonDocManagerInfo(this, Core.Constants.DocManagerCodes.Person);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region Add Or Update DDR Log Info

		internal string referenceForDDR;

		ZDateTime eventTimeForDDR;

		public void AddOrUpdateDDRLogInfo(IDuplicationEventArgs e)
		{
			if (e != null && e.Results != null && e.TargetObjects != null && e.TargetObjects is IEnumerable<CargoWise.Glow.Model.Interfaces.IGlbPerson> targetGlows)
			{
				var results = e.Results.OrderByDescending(u => u.Score).Select(score => targetGlows.FirstOrDefault(target => target.PER_PK == score.TargetPK)?.PER_FullName + " (" + (score.Score * 100) + "%)");
				var referenceStr = Res.GetString("09e17827-7d46-4ee8-8916-4a76269c6a37", "Match: {0}", string.Join(", ", results));
				if (referenceStr.Length > StmALogSchema.SL_Reference.MaxLength)
				{
					referenceStr = referenceStr.Substring(0, StmALogSchema.SL_Reference.MaxLength);
				}

				referenceForDDR = referenceStr;
				eventTimeForDDR = ZDateTime.UtcNow;
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "because not all contacts are born equal")]
		public const string EmptyFullName = "#Imported as Empty";

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GlbPersonFetchStrategy(this);
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return new PersonDocumentSupporter(this); }
		}

		[DocumentMacroIgnore]
		public ZString PER_FullNameInternal => base.PER_FullName;
		[DocumentMacroIgnore]
		public ZString PER_GenderInternal => base.PER_Gender;
		[DocumentMacroIgnore]
		public ZString PER_HomeAddress1Internal => base.PER_HomeAddress1;
		[DocumentMacroIgnore]
		public ZString PER_HomeAddress2Internal => base.PER_HomeAddress2;
		[DocumentMacroIgnore]
		public ZString PER_CityInternal => base.PER_City;
		[DocumentMacroIgnore]
		public ZString PER_StateInternal => base.PER_State;
		[DocumentMacroIgnore]
		public ZString PER_PostcodeInternal => base.PER_Postcode;
		[DocumentMacroIgnore]
		public ZString PER_RN_NKCountryInternal => base.PER_RN_NKCountry;
		[DocumentMacroIgnore]
		public ZString PER_MobilePhoneInternal => base.PER_MobilePhone;
		[DocumentMacroIgnore]
		public ZString PER_MobilePhone2Internal => base.PER_MobilePhone2;
		[DocumentMacroIgnore]
		public ZString PER_HomePhoneInternal => base.PER_HomePhone;
		[DocumentMacroIgnore]
		public ZString PER_FaxNumberInternal => base.PER_FaxNumber;
		[DocumentMacroIgnore]
		public ZDate PER_BirthDateInternal => base.PER_BirthDate;
		[DocumentMacroIgnore]
		public ZString PER_RN_NKNationalityCodeISOInternal => base.PER_RN_NKNationalityCodeISO;
		[DocumentMacroIgnore]
		public ZBlob PER_PictureInternal => base.PER_Picture;
		[DocumentMacroIgnore]
		public ZString PER_EmailAddressInternal => base.PER_EmailAddress;
		[DocumentMacroIgnore]
		public ZString PER_EmailAddress2Internal => base.PER_EmailAddress2;
		[DocumentMacroIgnore]
		public ZString PER_PassportInternal => base.PER_Passport;
		[DocumentMacroIgnore]
		public ZString PER_LegalNameInternal => base.PER_LegalName;
		[DocumentMacroIgnore]
		public ZString PER_PersonalInfoInternal => base.PER_PersonalInfo;
		[DocumentMacroIgnore]
		public ZString PER_DriversLicenseNumberInternal => base.PER_DriversLicenseNumber;

		IPatternMatchingRegenerator<GlbPerson>[] IPatternMatchingRegenerationEntities<GlbPerson>.RegenerationEntities(PatternMatchingRecalculator<GlbPerson> recalculator)
		{
			return ObjectFactory.Get<IMasterDataProvider>().GetPersonPatternMatchingRegenerationEntities(recalculator);
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new GlbPersonUniqueIndexFailureHandler(); }
		}

		public bool IsMovingFromAnotherPerson { get; set; }

		class GlbPersonUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return GlbPersonSchema.Constants.Indexes.NR_UX__PER_EmailAddress; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(
					Res.GetString("b4469cfa-9481-41f4-b378-9a490205df92", "This email is already in use by another active Person with web access."),
					Res.GetString("f65c8889-b795-49b3-a426-9f675b2bd448", "Email is already in use."));
			}
		}

		#endregion

		#region Password

		#region IPasswordStored Member

		int IPasswordStored.PasswordHashIterations => PER_PasswordHashIterations;

		ZBlob IPasswordStored.PasswordSalt => PER_PasswordSalt;

		ZBlob IPasswordStored.PasswordHash => PER_PasswordHash;

		#endregion

		public bool VerifyPassword(IUserSecretsContext userSecretsContext, string password) => !PER_PasswordHash.IsEmpty && userSecretsContext.IsMatchingSecret(password, GetPasswordAdapter());

		public bool HasPassword => !PER_PasswordHash.IsEmpty;

		public void SetHashedPassword(string newPassword, string currentPassword = null)
		{
			using (GetValidationSuspender())
			{
				PasswordHistoryHelper.AddPasswordHistory(this, currentPassword);
				UserSecretsContext.DefaultContext.SaveSecret(
					newPassword,
					UserSecretHashAlgorithmExtensions.PreferredAlgorithm,
					DataRegistry.Instance.PasswordHashingIterationsCount,
					GetPasswordAdapter());
			}
		}

		public void RemovePasswordHash()
		{
			using (GetValidationSuspender())
			{
				PasswordHistoryHelper.AddPasswordHistory(this, null); //save current password to history
				PER_PasswordHash = ZBlob.Empty;
				PER_PasswordSalt = ZBlob.Empty;
				PER_PasswordHashIterations = ZInt.Zero;
			}
		}

		public GlbPersonAdapter GetPasswordAdapter()
			=> new GlbPersonAdapter(this);

		[DocumentMacroIgnore]
		public override ZBlob PER_PasswordHash { get => base.PER_PasswordHash; set => base.PER_PasswordHash = value; }

		[DocumentMacroIgnore]
		public override ZBlob PER_PasswordSalt { get => base.PER_PasswordSalt; set => base.PER_PasswordSalt = value; }

		[DocumentMacroIgnore]
		public override ZInt PER_PasswordHashIterations
		{
			get => base.PER_PasswordHashIterations;
			set => base.PER_PasswordHashIterations = value;
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			PER_EmailAddress = "";
			PER_WebAccessEnabled = false;
		}
#endif

		public IEnumerable<string> GetEmailAddressesFromTriggerParty(string triggerParty)
		{
			if (triggerParty == MessageRecipientPartyTypeList.Codes.PersonalEmail)
			{
				yield return PER_EmailAddressInternal;
			}

			var primarySource = PrimarySource;

			if (triggerParty == MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail && primarySource != null)
			{
				yield return primarySource.Email;
			}

			if (triggerParty == MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail)
			{
				yield return !string.IsNullOrEmpty(PER_EmailAddressInternal) ? PER_EmailAddressInternal : primarySource?.Email ?? string.Empty;
			}
		}

		public bool IsLockedOut => PER_LoginDisabledUntilUtc > ZDateTime.UtcNow;

		public void LockOutUntil(ZDateTime time) => PER_LoginDisabledUntilUtc = time;

		public void Unlock() => LockOutUntil(ZDateTime.Empty);

		public ZDateTime LockoutDateTimeLocal => PER_LoginDisabledUntilUtc.ToLocalBranchTime();

		#region IGlbPasswordHistoryParent Member

		bool IGlbPasswordHistoryParent.ShouldSavePasswordHistory => true;

		BusinessObject IGlbPasswordHistoryParent.BusinessEntity => this;

		int IGlbPasswordHistoryParent.PasswordHistoryCount => WebDataRegistry.Instance.WebPasswordHistoryCount.Value;

		public bool HasPasswordBeenUsed(string password) => PasswordHistoryHelper.HasPasswordBeenUsed(this, password);

		#endregion
	}
}
