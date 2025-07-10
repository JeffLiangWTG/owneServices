using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Environment;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;
using WTG.RtfConverter;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(OrgColdCallRegisterSchema.Constants.O1_LeadUniqueReference), DescriptionProperty(OrgColdCallRegisterSchema.Constants.O1_LeadUniqueReference)]
	[UserDefinedValues]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class SalesEnquiry : OrgColdCallRegister,
		IDocManagerSupport,
		ISalesRelationActivity,
		IImportParentRelatedActivityInfoOnNew,
		IImportChildRelatedActivityInfoOnNewSaved,
		IImportChildRelatedActivityInfoOnAttach,
		IWorkflowProvider,
		ICustomFieldProvider,
		IGlbCompanyCampaignItemRecipient,
		ISupportWebAddressValidation
	{
		public SalesEnquiry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public abstract new class Schema : AutoOrgColdCallRegister.Schema
		{
			public const int LanguageMaxLength = 3;
		}

		#endregion

		public static class Codes
		{
			public const string SalesEnquiry = "INQ";
		}

		#region Default Values / Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
			O1_LeadCalledDate = ZDateTime.UtcNow;
			O1_PortOrCountry = GlbBranch.CurrentBranch?.GB_RL_NKHomePort ?? string.Empty;

			if (!O1_GeoLocation.IsValid)
			{
				O1_GeoLocation = ZGeography.Empty;
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (O1_OA_LinkedAddress.IsEmpty && Header != null && Header.MainAddress != null)
			{
				using (SuspendSettingHasChanges())
				{
					O1_OA_LinkedAddress = Header.MainAddress.PK;
				}
			}
		}

		#endregion

		#region Properties

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = ResString.GetMultilingualString("a3c64d63-999c-4a9c-ab02-2919861bd8c9", "Inquiry");
				if (!IsDeleted && !O1_LeadUniqueReference.IsEmpty)
				{
					result += " (" + O1_LeadUniqueReference + ")";
				}

				return result;
			}
		}

		#endregion

		#region Human Readable Shortcut Name

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = O1_LeadUniqueReference;

				if (!O1_EnquiryType.IsEmpty)
				{
					result += " - " + O1_EnquiryType;
				}

				if (!OrgCode.IsEmpty)
				{
					result += " - " + OrgCode;
				}
				else if (!O1_CompanyName.IsEmpty)
				{
					result += " - " + O1_CompanyNameTruncated;
				}

				return result;
			}
		}

		#endregion

		public override bool ReadOnly
		{
			get
			{
				if (!base.ReadOnly)
				{
					return IsNotOpen;
				}
				else
				{
					return true;
				}
			}
			set
			{
				base.ReadOnly = value;
			}
		}

		#region O1_FollowupDateLocal

		public ZDateTime O1_FollowupDateLocal
		{
			get
			{
				return O1_FollowupDate.IsValid ? Env.Time.GetLocalTimeFromUtc(O1_FollowupDate.ToDateTime()) : O1_FollowupDate;
			}
			set
			{
				base.O1_FollowupDate = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
			}
		}

		#endregion

		#region O1_LeadCalledDateLocal

		public ZDateTime O1_LeadCalledDateLocal
		{
			get
			{
				return O1_LeadCalledDate.IsValid ? Env.Time.GetLocalTimeFromUtc(O1_LeadCalledDate.ToDateTime()) : O1_LeadCalledDate;
			}
			set
			{
				base.O1_LeadCalledDate = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
			}
		}

		#endregion

		#region O1_CompanyNameTruncated
		/// <summary>
		/// Use this property to diplay first 50 characters for company name.
		/// In most cases the system is best suited to show 50 characters for company name.
		/// Use the property 'O1_CompanyName' if only we want to show the full company name.
		/// </summary>
		public ZString O1_CompanyNameTruncated
		{
			get
			{
				return O1_CompanyName.Substring(0, O1_CompanyNameTruncatedLength);
			}
		}

		const int O1_CompanyNameTruncatedLength = 50;

		#endregion

		#region O1_SystemCreateTimeLocal

		public ZDateTime O1_SystemCreateTimeLocal
		{
			get { return O1_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region O1_SystemLastEditTimeLocal

		public ZDateTime O1_SystemLastEditTimeLocal
		{
			get { return O1_SystemLastEditTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region O1_CloseReason

		[List("Lookups.CloseReason_ActiveList")]
		public override ZString O1_CloseReason
		{
			get { return base.O1_CloseReason; }
			set { base.O1_CloseReason = value; }
		}

		public ZString CloseReasonDescription
		{
			get { return Lookups.EnquiryCloseReasonList.GetDescriptionFromCode(O1_CloseReason); }
		}

		#endregion

		#region O1_EnquiryType

		internal class EnquiryTypePairList : CodeDescriptionPairList
		{
			public EnquiryTypePairList()
			{
				AddRange(SalesEnquiryLookups.GetAllEnquiryTypes());
			}
		}

		[ActionField(CollectionType = typeof(EnquiryTypePairList))]
		[List("Lookups.ActiveEnquiryTypes")]
		public override ZString O1_EnquiryType
		{
			get { return base.O1_EnquiryType; }
			set { base.O1_EnquiryType = value; }
		}

		#endregion

		#region O1_LeadStatus

		[List("Lookups.StatusList")]
		[ReadOnly(true)]
		public override ZString O1_LeadStatus
		{
			get { return base.O1_LeadStatus; }
			set { base.O1_LeadStatus = value; }
		}

		public ZString StatusDescription
		{
			get { return new SalesEnquiryStatusCodeList().GetDescriptionFromCode(O1_LeadStatus); }
		}

		#endregion

		#region O1_LeadSource

		[List("Lookups.Source_ActiveList")]
		public override ZString O1_LeadSource
		{
			get { return base.O1_LeadSource; }
			set { base.O1_LeadSource = value; }
		}

		public ZString SourceDescription
		{
			get { return Lookups.Source_List.GetDescriptionFromCode(O1_LeadSource); }
		}

		#endregion

		#region Referring Organisation and Contact

		[List("Lookups.OrganisationsList")]
		public override ZGuid O1_OH_SourceOfLead
		{
			get { return base.O1_OH_SourceOfLead; }
			set
			{
				if (base.O1_OH_SourceOfLead != value)
				{
					base.O1_OH_SourceOfLead = value;
					ReferringContactName = ZString.Empty;
				}
			}
		}

		public OrgHeader ReferringOrg
		{
			get { return base.SourceOfLead; }
		}

		[List("Lookups.ContactsOfReferringOrg")]
		[MaxLength(SalesEnquiry.Schema.O1_ContactNameMaxLength)]
		[ReadOnlyMember(nameof(ReferringContactName_ReadOnly))]
		public ZString ReferringContactName
		{
			get
			{
				var referringContact = ReferringContact;
				return referringContact != null ? referringContact.OC_ContactName : referringContactName;
			}
			set
			{
				CheckMaximumLength(ReferringContactNameInfo, value);
				SetNonPersistentPropertyValue(ReferringContactNameInfo, ref referringContactName, value);

				// Validation is run during the setting of the link
				var contactToLinkTo = O1_OH_SourceOfLead.IsValid ? FindMatchingContact(ReferringOrg, value) : null;
				O1_OC_ReferringContact = contactToLinkTo != null ? contactToLinkTo.PK : ZGuid.Empty;

				ReferringContactNameInfo.RefreshBinding();
			}
		}

		ZString referringContactName;

		protected ZBool ReferringContactName_ReadOnly
		{
			get { return !O1_OH_SourceOfLead.IsValid; }
		}

		public ZPropertyInfo ReferringContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(ReferringContactName)); }
		}

		[List("Lookups.ContactsOfReferringOrg")]
		public override ZGuid O1_OC_ReferringContact
		{
			get { return base.O1_OC_ReferringContact; }
			set
			{
				base.O1_OC_ReferringContact = value;

				// If O1_OC_ReferringContact is set due to an Operational Action
				// properly validate it.
				if (!IsValidationSuspended)
				{
					Validation.ValidateReferringContactName();
				}
			}
		}

		#endregion

		#region Refer To Organisation and Contact

		[List("Lookups.OrganisationsList")]
		public override ZGuid O1_OH_ReferTo
		{
			get { return base.O1_OH_ReferTo; }
			set
			{
				if (base.O1_OH_ReferTo != value)
				{
					base.O1_OH_ReferTo = value;
					ReferToContactName = ZString.Empty;
				}
			}
		}

		[List("Lookups.ContactsOfReferToOrg")]
		[MaxLength(SalesEnquiry.Schema.O1_ContactNameMaxLength)]
		[ReadOnlyMember(nameof(ReferToContactNameReadOnly))]
		public ZString ReferToContactName
		{
			get
			{
				var referToContact = ReferToContact;
				return referToContact != null ? referToContact.OC_ContactName : referToContactName;
			}
			set
			{
				CheckMaximumLength(ReferToContactNameInfo, value);
				SetNonPersistentPropertyValue(ReferToContactNameInfo, ref referToContactName, value);

				// Validation is run during the setting of the link
				var contactToLinkTo = O1_OH_ReferTo.IsValid ? FindMatchingContact(ReferTo, value) : null;
				O1_OC_ReferToContact = contactToLinkTo != null ? contactToLinkTo.PK : ZGuid.Empty;

				ReferToContactNameInfo.RefreshBinding();
			}
		}

		ZString referToContactName;

		protected ZBool ReferToContactNameReadOnly
		{
			get { return !O1_OH_ReferTo.IsValid; }
		}

		public ZPropertyInfo ReferToContactNameInfo
		{
			get { return GetZPropertyInfo(nameof(ReferToContactName)); }
		}

		[List("Lookups.ContactsOfReferToOrg")]
		public override ZGuid O1_OC_ReferToContact
		{
			get { return base.O1_OC_ReferToContact; }
			set
			{
				base.O1_OC_ReferToContact = value;

				// Properly validate O1_OC_ReferToContact if it is set due to an Operational Action.
				if (!IsValidationSuspended)
				{
					Validation.ValidateReferToContactName();
				}
			}
		}

		#endregion

		#region O1_LeadUniqueReference

		[ReadOnly(true)]
		public override ZString O1_LeadUniqueReference
		{
			get { return base.O1_LeadUniqueReference; }
			set { base.O1_LeadUniqueReference = value; }
		}

		#endregion

		#region OrgPk

		[List("Lookups.OrganisationsList")]
		public ZGuid OrgPk
		{
			get { return O1_OH_ConvertedToQualifiedLead; }
			set
			{
				if (value != O1_OH_ConvertedToQualifiedLead)
				{
					if (O1_OH_ConvertedToQualifiedLead.IsEmpty && !value.IsEmpty)
					{
						if (!O1_ContactName.IsEmpty)
						{
							var contact = CreateOrUpdateContactFromManualValues(value);
							O1_OC_LinkedContact = contact != null ? contact.PK : ZGuid.Empty;
						}
					}
					else if (!O1_OH_ConvertedToQualifiedLead.IsEmpty)
					{
						DeleteTempContacts(true);
						O1_OC_LinkedContact = ZGuid.Empty;
					}

					DeleteTempAddresses(true);

					O1_OH_ConvertedToQualifiedLead = value;
					O1_OA_LinkedAddress = (Header != null && Header.MainAddress != null) ? Header.MainAddress.PK : ZGuid.Empty;

					if (OrganisationsDataRegistry.Instance.SalesEnquiryDefaultAssignedSalesRep.Value
						&& Header?.StaffAssignments.OverallSalesRepStaff != null)
					{
						O1_GS_NKRepAssigned = Header.StaffAssignments.OverallSalesRepStaff.GS_Code;
					}
				}
			}
		}

		void DeleteTempAddresses(bool deleteAll)
		{
			if (Header != null)
			{
				var currentAddress = LinkedAddress;
				foreach (OrgAddress address in Header.Addresses.ToArray())
				{
					if (!address.IsInDatabase && !address.IsDeleted && !address.IsMainAddress
						&& (deleteAll || currentAddress == null || currentAddress.PK != address.PK))
					{
						address.Delete();
					}
				}
			}
		}

		void DeleteTempContacts(bool deleteAll)
		{
			if (Header != null)
			{
				var contactsToDelete = Header.Contacts
					.Where(contact => !contact.IsInDatabase && !contact.IsDeleted)
					.Where(contact => contact.PK != O1_OC_ReferToContact && contact.PK != O1_OC_ReferringContact)
					.Where(contact => deleteAll || contact.PK != O1_OC_LinkedContact);
				contactsToDelete.DeleteAll();
			}
		}

		public override ZGuid O1_OH_ConvertedToQualifiedLead
		{
			get { return base.O1_OH_ConvertedToQualifiedLead; }
			set
			{
				base.O1_OH_ConvertedToQualifiedLead = value;
				if (!value.IsEmpty)
				{
					ClearOrgManualValues();
				}

				OrgPkInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateO1_CompanyName();
					Validation.ValidateO1_ContactName();
				}
			}
		}

		void ClearOrgManualValues()
		{
			base.O1_CompanyName = ZString.Empty;
			base.O1_BusinessRegNo = ZString.Empty;
		}

		public ZPropertyInfo OrgPkInfo
		{
			get { return GetZPropertyInfo(nameof(OrgPk)); }
		}

		public OrgHeader Header
		{
			get { return Factory.Load<OrgHeader>(O1_OH_ConvertedToQualifiedLead); }
		}

		#endregion

		#region Org Company Name

		public override ZString O1_CompanyName
		{
			get
			{
				var org = Header;
				return org != null ? org.OH_FullNameTruncated : base.O1_CompanyName;
			}
			set
			{
				if (value != O1_CompanyName)
				{
					OrgPk = ZGuid.Empty;
					OrgCodeInfo.RefreshBinding();
					base.O1_CompanyName = value;
				}
			}
		}

		#endregion

		#region Org Code

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		public ZString OrgCode
		{
			get
			{
				var org = Header;
				return org != null ? org.OH_Code : ZString.Empty;
			}
		}

		public ZPropertyInfo OrgCodeInfo
		{
			get { return GetZPropertyInfo(nameof(OrgCode)); }
		}

		#endregion

		#region O1_OA_LinkedAddress

		[List("Lookups.AddressList")]
		public override ZGuid O1_OA_LinkedAddress
		{
			get { return base.O1_OA_LinkedAddress; }
			set
			{
				base.O1_OA_LinkedAddress = value;
				ClearAddressManualValues();
			}
		}

		void ClearAddressManualValues()
		{
			base.O1_Address1 = ZString.Empty;
			base.O1_Address2 = ZString.Empty;
			base.O1_City = ZString.Empty;
			base.O1_State = ZString.Empty;
			base.O1_PostCode = ZString.Empty;
			base.O1_PortOrCountry = ZString.Empty;
		}

		#endregion

		#region Org Address 1

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		public override ZString O1_Address1
		{
			get
			{
				var address = LinkedAddress;
				return address != null ? address.OA_Address1 : base.O1_Address1;
			}
			set
			{
				if (value != base.O1_Address1)
				{
					base.O1_Address1 = value;
					ResetValidationStatus(O1_Address1Info);
				}
			}
		}

		#endregion

		#region Org Address 2

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		public override ZString O1_Address2
		{
			get
			{
				var address = LinkedAddress;
				return address != null ? address.OA_Address2 : base.O1_Address2;
			}
			set
			{
				if (value != base.O1_Address2)
				{
					base.O1_Address2 = value;
					ResetValidationStatus(O1_Address2Info);
				}
			}
		}

		#endregion

		#region Org City

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		public override ZString O1_City
		{
			get
			{
				var address = LinkedAddress;
				return address != null ? address.OA_City : base.O1_City;
			}
			set
			{
				if (value != base.O1_City)
				{
					base.O1_City = value;
					ResetValidationStatus(O1_CityInfo);
				}
			}
		}

		#endregion

		#region Org State

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		[List("Lookups.StateList")]
		public override ZString O1_State
		{
			get
			{
				var address = LinkedAddress;
				return address != null ? address.OA_State : base.O1_State;
			}
			set
			{
				if (value != base.O1_State)
				{
					base.O1_State = value;
					ResetValidationStatus(O1_StateInfo);
				}
			}
		}

		#endregion

		#region Org Post Code

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		public override ZString O1_PostCode
		{
			get
			{
				var address = LinkedAddress;
				return address != null ? address.OA_PostCode : base.O1_PostCode;
			}
			set
			{
				if (value != base.O1_PostCode)
				{
					base.O1_PostCode = value;
					ResetValidationStatus(O1_PostCodeInfo);
				}
			}
		}

		#endregion

		#region Org Country

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		[List("Lookups.Locations")]
		public override ZString O1_PortOrCountry
		{
			get
			{
				var address = LinkedAddress;
				return address != null ? address.OA_RN_NKCountryCode : base.O1_PortOrCountry;
			}
			set
			{
				if (value != base.O1_PortOrCountry)
				{
					base.O1_PortOrCountry = value;
					ResetValidationStatus(O1_PortOrCountryInfo);
				}
			}
		}

		#endregion

		#region Validation Status

		public override ZString O1_ValidationStatus
		{
			get { return base.O1_ValidationStatus; }
			set
			{
				base.O1_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();
			}
		}

		#endregion

		#region Business Reg Number

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		public override ZString O1_BusinessRegNo
		{
			get
			{
				var org = Header;
				return org != null ? org.PrimaryRegistrationNumber.Number : base.O1_BusinessRegNo;
			}
			set
			{
				base.O1_BusinessRegNo = value;
			}
		}

		#endregion

		#region Website

		[ReadOnlyMember(nameof(OrgFieldsReadOnly))]
		public override ZString O1_WebAddress
		{
			get
			{
				var org = Header;
				return org != null ? org.MainWebURL.PU_URL : base.O1_WebAddress;
			}
			set
			{
				base.O1_WebAddress = value;
			}
		}

		#endregion

		#region O1_OC_LinkedContact

		[List("Lookups.Contacts")]
		public override ZGuid O1_OC_LinkedContact
		{
			get { return base.O1_OC_LinkedContact; }
			set
			{
				if (base.O1_OC_LinkedContact != value)
				{
					base.O1_OC_LinkedContact = value;
					ClearContactManualValues();
				}
			}
		}

		void ClearContactManualValues()
		{
			base.O1_ContactName = ZString.Empty;
			base.O1_Phone = ZString.Empty;
			base.O1_Fax = ZString.Empty;
			base.O1_Email = ZString.Empty;
			base.O1_Mobile = ZString.Empty;
			base.O1_JobCategory = ZString.Empty;

			// Refresh the bindings of the wrapped numbers so they are displayed correctly on the interface. Cannot test in this class.
			PhoneNumber.Refresh();
			MobilePhoneNumber.Refresh();
			FaxNumber.Refresh();
		}

		public OrgContact Contact
		{
			get { return Factory.Load<OrgContact>(O1_OC_LinkedContact); }
		}

		#endregion

		#region Contact Name

		[List("Lookups.Contacts")]
		public override ZString O1_ContactName
		{
			get
			{
				var contact = Contact;
				return contact != null ? contact.OC_ContactName : base.O1_ContactName;
			}
			set
			{
				var contactToLinkTo = OrgPk.IsValid ? FindMatchingContact(Header, value) : null;
				if (contactToLinkTo != null)
				{
					O1_OC_LinkedContact = contactToLinkTo.PK;
					base.O1_ContactName = ZString.Empty;
				}
				else
				{
					O1_OC_LinkedContact = ZGuid.Empty;
					base.O1_ContactName = value;
				}
			}
		}

		#endregion

		#region PhoneNumbers

		public PhoneNumber PhoneNumber
		{
			get
			{
				if (phoneNumber == null)
				{
					phoneNumber = new PhoneNumber(O1_PhoneInfo, null, null);
				}
				return phoneNumber;
			}
		}
		PhoneNumber phoneNumber;

		public PhoneNumber FaxNumber
		{
			get
			{
				if (faxNumber == null)
				{
					faxNumber = new PhoneNumber(O1_FaxInfo, null, null);
				}
				return faxNumber;
			}
		}
		PhoneNumber faxNumber;

		public PhoneNumber MobilePhoneNumber
		{
			get
			{
				if (mobilePhoneNumber == null)
				{
					mobilePhoneNumber = new PhoneNumber(O1_MobileInfo, null, null);
				}
				return mobilePhoneNumber;
			}
		}
		PhoneNumber mobilePhoneNumber;

		#endregion

		#region Contact Mobile

		[ReadOnlyMember(nameof(ContactFieldsReadOnly))]
		public override ZString O1_Mobile
		{
			get
			{
				var contact = Contact;
				return contact != null ? contact.OC_Mobile : base.O1_Mobile;
			}
			set
			{
				base.O1_Mobile = value;
			}
		}

		#endregion

		#region Contact Phone

		[ReadOnlyMember(nameof(ContactFieldsReadOnly))]
		public override ZString O1_Phone
		{
			get
			{
				var contact = Contact;
				return contact != null ? contact.OC_Phone : base.O1_Phone;
			}
			set
			{
				base.O1_Phone = value;
			}
		}

		#endregion

		#region Contact Fax

		[ReadOnlyMember(nameof(ContactFieldsReadOnly))]
		public override ZString O1_Fax
		{
			get
			{
				var contact = Contact;
				return contact != null ? contact.OC_Fax : base.O1_Fax;
			}
			set
			{
				base.O1_Fax = value;
			}
		}

		#endregion

		#region Contact Email

		[ReadOnlyMember(nameof(ContactFieldsReadOnly))]
		[EmailAddress]
		public override ZString O1_Email
		{
			get
			{
				var contact = Contact;
				return contact != null ? contact.OC_Email : base.O1_Email;
			}
			set
			{
				base.O1_Email = value;
			}
		}

		#endregion

		#region Contact Job Category

		[ReadOnlyMember(nameof(ContactFieldsReadOnly))]
		public override ZString O1_JobCategory
		{
			get
			{
				var contact = Contact;
				return contact != null ? contact.OC_JobCategory : base.O1_JobCategory;
			}
			set
			{
				base.O1_JobCategory = value;
			}
		}

		[MaxLength(AutoOrgColdCallRegister.Schema.O1_JobCategoryMaxLength)]
		[ReadOnlyMember(nameof(ContactFieldsReadOnly))]
		[List("Lookups.JobCategory_List")]
		public ZString JobCategoryDescription
		{
			get
			{
				var contact = Contact;
				return contact != null ? contact.JobCategoryDescription : (ZString)Lookups.JobCategory_List.GetDescriptionFromCode(base.O1_JobCategory);
			}
			set
			{
				base.O1_JobCategory = Lookups.JobCategory_List.GetCodeFromDescription(value) ?? value;
			}
		}

		internal ZPropertyInfo JobCategoryDescriptionInfo => GetZPropertyInfo(nameof(JobCategoryDescription));

		#endregion

		#region Has Contact Errors

		public bool HasContactErrors
		{
			get
			{
				bool result = false;
				if (!O1_ContactName.IsEmpty)
				{
					Validation.ValidateO1_Email();
					Validation.ValidateO1_Phone();
					Validation.ValidateO1_Mobile();
					Validation.ValidateO1_Fax();
					Validation.ValidateO1_JobCategory();

					result = O1_EmailInfo.HasErrors()
							|| O1_PhoneInfo.HasErrors()
							|| O1_MobileInfo.HasErrors()
							|| O1_FaxInfo.HasErrors()
							|| O1_JobCategoryInfo.HasErrors();
				}
				return result;
			}
		}

		#endregion

		#region Enquiry Notes

		public ZBlob EnquiryNotesContent
		{
			get
			{
				ZBlob result = EnquiryNotes.Rtf;
				if (!ORtfTextUtil.IsRtf(result))
				{
					result = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf(EnquiryNotes.Rtf.ToUTF8()));
				}
				return result;
			}
			set
			{
				value = ORtfTextUtil.IsRtf(value) ? value : ZBlob.FromUTF8(ORtfTextUtil.TextToRtf(value.ToUTF8()));
				if (EnquiryNotes.Rtf != value)
				{
					EnquiryNotes.Rtf = value;
					HasChanges = true;
					O1_SystemLastEditTimeUtc = ZDateTime.UtcNow;
					EnquiryNotesContentInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo EnquiryNotesContentInfo
		{
			get { return GetZPropertyInfo(nameof(EnquiryNotesContent)); }
		}

		HiddenRtfNote EnquiryNotes
		{
			get { return enquiryNotes ?? (enquiryNotes = new HiddenRtfNote(this)); }
		}
		HiddenRtfNote enquiryNotes;

		public ZBlob EnquiryNotesContent_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(EnquiryNotesContent);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				EnquiryNotesContent = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}
		#endregion

		#region Org & Contact Fields ReadOnly

		public bool OrgFieldsReadOnly
		{
			get { return !O1_OH_ConvertedToQualifiedLead.IsEmpty && Header != null; }
		}

		public bool ContactFieldsReadOnly
		{
			get { return O1_OC_LinkedContact.IsValid; }
		}

		#endregion

		#region Opportunity

		public OrgOpportunity Opportunity
		{
			get { return Factory.LoadTop1<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_O1_Enquiry, PK)); }
		}

		#endregion

		#region Overall Disposition

		public ZString OverallDispositionDescription
		{
			get
			{
				if (O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Open)
				{
					return SalesEnquiryStatusCodeList.Descriptions.Open;
				}
				else
				{
					return SalesEnquiryStatusCodeList.Descriptions.Closed;
				}
			}
		}

		public ZBool IsNotOpen
		{
			get { return O1_LeadStatus != SalesEnquiryStatusCodeList.Codes.Open; }
		}

		#endregion

		#region Lead Interest (Interest Level)

		[List("Lookups.LeadInterest_ActiveList")]
		public override ZString O1_InterestLevel
		{
			get { return base.O1_InterestLevel; }
			set { base.O1_InterestLevel = value; }
		}

		public ZString InterestLevelDescription
		{
			get { return Lookups.LeadInterest_List.GetDescriptionFromCode(O1_InterestLevel); }
		}

		#endregion

		#region RelatedCommunicationCollection

		public OrgSalesCallCollection RelatedCommunicationCollection
		{
			get
			{
				if (relatedCommunicationCollection == null)
				{
					relatedCommunicationCollection = new RelatedOrgSalesCallCollection(this);
				}

				return relatedCommunicationCollection;
			}
		}
		OrgSalesCallCollection relatedCommunicationCollection;

		#endregion

		#region Non-Delivery Report

		public GlbEmailAddress EmailAddress
		{
			get
			{
				return Factory.LoadTop1<GlbEmailAddress>(new ZQuery(GlbEmailAddressSchema.GI_EmailAddress, O1_Email));
			}
		}

		public ZBool IsNDR
		{
			get { return EmailAddress != null && EmailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport; }
			set
			{
				var emailAddress = EmailAddress;
				if (emailAddress == null)
				{
					emailAddress = Factory.New<GlbEmailAddress>();
					emailAddress.GI_EmailAddress = O1_Email;
				}

				emailAddress.GI_DeliveryStatus = value ? EmailDeliveryReportStatus.Codes.NonDeliveryReport : EmailDeliveryReportStatus.Codes.ValidReport;
				emailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;

				IsNDRInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsNDRInfo
		{
			get { return GetZPropertyInfo(nameof(IsNDR)); }
		}

		protected bool IsNDR_ReadOnly
		{
			get { return IsNDR == ZBool.False; }
		}

		public ZDateTime DeliveryReportTimeUtc
		{
			get { return EmailAddress != null ? EmailAddress.GI_DeliveryReportTimeUtc : ZDateTime.Empty; }
		}

		#endregion

		#region EnquiryTypeDescription

		public ZString EnquiryTypeDescription
		{
			get { return SalesEnquiryLookups.GetAllEnquiryTypes().GetDescriptionFromCode(O1_EnquiryType); }
		}

		public virtual ZPropertyInfo EnquiryTypeDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(EnquiryTypeDescription)); }
		}

		#endregion

		#region Assigned Sales Rep

		public ZString AssignedSalesRepBranchCode
		{
			get
			{
				var branch = RepAssigned?.HomeBranch;
				if (branch != null)
				{
					return $"[{branch.GB_Code}]";
				}
				return "";
			}
		}

		#endregion

		#endregion

		#region Modifiers

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			if (property.Name == AutoOrgColdCallRegister.Schema.O1_CloseReason)
			{
				return O1_LeadStatus != SalesEnquiryStatusCodeList.Codes.Closed;
			}
			else
			{
				return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			}
		}

		#endregion

		#region Saving

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void RunPreSaveValidationCore()
		{
			CreateContactFromManualValuesIfLinkedContactMissing();
			CreateReferralContactsIfLinkedContactMissing();
			base.RunPreSaveValidationCore();
		}

		void CreateContactFromManualValuesIfLinkedContactMissing()
		{
			if (!HasContactErrors && Contact == null && !O1_ContactName.IsEmpty)
			{
				var contact = CreateOrUpdateContactFromManualValues(OrgPk);
				O1_OC_LinkedContact = contact != null ? contact.PK : ZGuid.Empty;
			}
		}

		void CreateReferralContactsIfLinkedContactMissing()
		{
			if (!HasErrors)
			{
				var referringOrg = ReferringOrg;
				var referringContactName = ReferringContactName;
				if (referringOrg != null && ReferringContact == null && !referringContactName.IsEmpty && MissingContactNewSecurity(referringOrg) == null)
				{
					var referringContact = referringOrg.Contacts.AddNew();
					referringContact.OC_ContactName = referringContactName;
					O1_OC_ReferringContact = referringContact.PK;
				}

				var referToOrg = ReferTo;
				var referToContactName = ReferToContactName;
				if (referToOrg != null && ReferToContact == null && !referToContactName.IsEmpty && MissingContactNewSecurity(referToOrg) == null)
				{
					var referToContact = referToOrg.Contacts.AddNew();
					referToContact.OC_ContactName = referToContactName;
					O1_OC_ReferToContact = referToContact.PK;
				}
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			PopulateIdOnSaving();
			UpdateInquiryLinkedCommunications();

			DeleteTempAddresses(false);
			DeleteTempContacts(false);

			RaiseEvent();

			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				O1_LeadUniqueReference = ZString.Empty;
			}

			base.OnSaved(saveSucceeded);
		}

		internal void PopulateIdOnSaving()
		{
			if (!IsInDatabase && O1_LeadUniqueReference.IsEmpty)
			{
				O1_LeadUniqueReference = Env.NumberFountains.EnquiryID.GetNextFormatted(Factory);
			}
		}

		void RaiseEvent()
		{
			if (O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Closed)
			{
				Logs.AddNew(
					AutoEvents.StatusChange,
					string.Format(CultureInfo.InvariantCulture, "Close Reason: {0}", O1_CloseReason));
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			DeleteInquiryLinkedCommunications();

			WorkflowItems.RemoveAndDeleteAll();
			EnquiryNotes.Delete();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();

			var opp = Opportunity;
			if (opp != null && !opp.IsDeleted)
			{
				opp.P8_O1_Enquiry = ZGuid.Empty;
			}

			base.Delete();
		}

		#endregion

		#region Close

		public bool CanDoClose
		{
			get { return IsInDatabase && O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Open; }
		}

		public void DoClose()
		{
			O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			foreach (ProcessTask task in WorkflowItems)
			{
				if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled
					&& task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed
					&& !task.IsException)
				{
					if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned || task.P9_Status == ProcessTaskStatusCodeList.Codes.Open)
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
					}
					else
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}
			}
		}

		#endregion

		#region Create Org

		public virtual OrgHeader CreateOrg(BusinessObjectFactory factory)
		{
			var result = factory.New<OrgHeader>();
			result.OH_IsSalesLead = true;

			PopulateOrg(result);

			if (result.SecurityProvider.CanModifyDetailsStaffAssignment(StaffAssignmentRoles.Codes.SalesRep) && RepAssigned != null)
			{
				result.StaffAssignments.OverallSalesRep = RepAssigned.GS_Code;
			}

			var contact = result.Contacts.AddNew();
			contact.OC_WebAccessEnabled_ReadOnly = true;
			contact.OC_ContactName = O1_ContactName;
			contact.OC_Phone = O1_Phone;
			contact.OC_Mobile = O1_Mobile;
			contact.OC_Fax = O1_Fax;
			contact.OC_Email = O1_Email;
			contact.OC_JobCategory = O1_JobCategory;

			return result;
		}

		public void PopulateOrg(OrgHeader org)
		{
			org.OH_FullName = ParseOrgFieldBeforeCopy(O1_CompanyName);
			org.MainAddress.OA_Address1 = ParseOrgFieldBeforeCopy(O1_Address1);
			org.MainAddress.OA_Address2 = ParseOrgFieldBeforeCopy(O1_Address2);
			org.MainAddress.OA_City = ParseOrgFieldBeforeCopy(O1_City);
			org.MainAddress.OA_State = ParseOrgFieldBeforeCopy(O1_State);
			org.MainAddress.OA_PostCode = ParseOrgFieldBeforeCopy(O1_PostCode);
			org.OH_RL_NKClosestPort = ParseOrgFieldBeforeCopy(O1_PortOrCountry);
			org.PrimaryRegistrationNumber.Number = ParseOrgFieldBeforeCopy(O1_BusinessRegNo);
			org.MainWebURL.PU_URL = ParseOrgFieldBeforeCopy(O1_WebAddress);
		}

		protected ZString ParseOrgFieldBeforeCopy(ZString value)
		{
			return !Env.Registry.OrgAllowMixedCase ? value.ToUpper() : value;
		}

		// REVIEW
		public virtual EnquiryOrgFinder CreateEnquiryOrgFinder()
		{
			var factory = new BusinessObjectFactory();
			var orgForMatching = factory.New<OrgHeaderForEnquiryMatching>();
			PopulateOrg(orgForMatching);
			return new EnquiryOrgFinder(factory, orgForMatching, !O1_Email.IsEmpty, !O1_Address1.IsEmpty);
		}

		#endregion

		#region Convert To Opportunity

		public bool CanConvertToOpportunity
		{
			get { return O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Open; }
		}

		public bool IsConvertedToOpportunity
		{
			get { return O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Converted; }
		}

		public void OnConvertedToOpportunity(OrgOpportunity opp)
		{
			O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			Factory.Save();
			ReadOnly = true;
		}

		#region Link To Organization

		public void LinkToOrganizationDirectly(ZGuid orgPk, ZGuid addressPk, ZGuid contactPk)
		{
			DeleteTempAddresses(true);
			DeleteTempContacts(true);

			O1_OH_ConvertedToQualifiedLead = orgPk;
			O1_OA_LinkedAddress = addressPk;
			O1_OC_LinkedContact = contactPk;
			CopyNotesToOrganization(orgPk);
		}

		public virtual void LinkToOrganizationByLinkingToAddress(ZGuid addressPk)
		{
			CreateOriginalInquiryRegistrationDetailsNote();

			var address = Factory.Load<OrgAddress>(addressPk);
			var contact = CreateOrUpdateContactFromManualValues(address.OA_OH);
			var contactPk = contact != null ? contact.PK : ZGuid.Empty;
			LinkToOrganizationDirectly(address.OA_OH, address.PK, contactPk);
		}

		public void LinkToOrganizationByAddingInquiryAddress(ZGuid orgPk)
		{
			CreateOriginalInquiryRegistrationDetailsNote();

			var address = CreateAddressFromManualValues(orgPk);
			var contact = CreateOrUpdateContactFromManualValues(orgPk);
			var contactPk = contact != null ? contact.PK : ZGuid.Empty;
			LinkToOrganizationDirectly(orgPk, address.PK, contactPk);
		}

		void CopyNotesToOrganization(ZGuid orgPk)
		{
			var org = Factory.Load<OrgHeader>(orgPk);
			var notesOverrideArgs = new BusinessObjectCloneArgs(new string[] { StmNoteSchema.Constants.ST_ParentID, StmNoteSchema.Constants.ST_Table });

			foreach (StmNote item in Notes.GetAllNotes())
			{
				if (item.ST_IsCustomDescription)
				{
					var note = org.Notes.AddNew();
					note.CopyPersistentValuesFrom(item, notesOverrideArgs);
				}
			}
		}

		#endregion

		#region Inquiry Details Note

		IEnumerable<ZString> GetAllAddressValues()
		{
			yield return O1_Address1;
			yield return O1_Address2;
			yield return O1_City;
			yield return O1_State;
			yield return O1_PostCode;
			yield return O1_PortOrCountry;
		}

		protected void CreateOriginalInquiryRegistrationDetailsNote()
		{
			if (GetAllAddressValues().Any(value => !value.IsEmpty))
			{
				var note = Notes.AddNew();
				note.ST_IsCustomDescription = true;
				note.ST_Description = Res.GetString("58bd40b7-0f4c-42ef-97da-9feab4f71b7f", "Original Inquiry Registration Details");
				note.ST_NoteType = nameof(StmNoteVisibility.INT);
				note.ST_NoteDataAsText = BuildInquiryDetailsForNote();
			}
		}

		string BuildInquiryDetailsForNote()
		{
			var builder = new ZStringBuilder();
			builder.AppendLine(Res.GetString("a551962a-9e8f-4315-ab96-4f0307d680a4", "Company Name: {0}", O1_CompanyName));
			builder.AppendLine(Res.GetString("14807bb4-4cfb-4685-a194-4ea1efd2eb16", "Address 1: {0}", O1_Address1));
			builder.AppendLine(Res.GetString("1c811444-511b-44f3-a8c0-cabc732c9766", "Address 2: {0}", O1_Address2));
			builder.AppendLine(Res.GetString("ee64fd32-cfca-4c83-b1e3-6acff070885f", "City: {0}", O1_City));
			builder.AppendLine(Res.GetString("1edd9db5-d334-4e17-b109-9c525cdedbb9", "State: {0}", O1_State));
			builder.AppendLine(Res.GetString("c8aeafe2-d6e3-40ed-926c-25b7c1e4c780", "Post Code: {0}", O1_PostCode));
			builder.AppendLine(Res.GetString("5c1af246-aa50-4bed-86b0-05dbd90208d6", "Port/Country/Region: {0}", O1_PortOrCountry));

			return builder.ToString();
		}

		#endregion

		#region Create From Manual Values

		OrgAddress CreateAddressFromManualValues(ZGuid orgPk)
		{
			var org = Factory.Load<OrgHeader>(orgPk);
			var newAddress = org.Addresses.AddNew();
			newAddress.OA_Address1 = base.O1_Address1;
			newAddress.OA_Address2 = base.O1_Address2;
			newAddress.OA_City = base.O1_City;
			newAddress.OA_State = base.O1_State;
			newAddress.OA_PostCode = base.O1_PostCode;
			newAddress.OA_RL_NKRelatedPortCode = base.O1_PortOrCountry;
			newAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			return newAddress;
		}

		protected OrgContact CreateOrUpdateContactFromManualValues(ZGuid orgPk)
		{
			var org = Factory.Load<OrgHeader>(orgPk);
			if (org != null)
			{
				var contact = FindMatchingContact(org, O1_ContactName);
				if (contact == null)
				{
					if (MissingContactNewSecurity(org) == null)
					{
						contact = org.Contacts.AddNew();
						contact.OC_ContactName = base.O1_ContactName;
						UpdateContactFromManualValues(contact);
					}
				}
				else if (MissingContactModifySecurity(org) == null)
				{
					UpdateContactFromManualValues(contact);
				}

				return contact;
			}

			return null;
		}

		void UpdateContactFromManualValues(OrgContact contact)
		{
			if (contact.OC_Phone.IsEmpty)
			{ contact.OC_Phone = base.O1_Phone; }
			if (contact.OC_Email.IsEmpty)
			{ contact.OC_Email = base.O1_Email; }
			if (contact.OC_Mobile.IsEmpty)
			{ contact.OC_Mobile = base.O1_Mobile; }
			if (contact.OC_Fax.IsEmpty)
			{ contact.OC_Fax = base.O1_Fax; }

			if (contact.OC_JobCategory.IsEmpty || contact.OC_JobCategory == OrgContactJobCategories.Codes.EMU)
			{
				contact.OC_JobCategory = base.O1_JobCategory;
			}
		}

		OrgContact FindMatchingContact(OrgHeader org, string contactName)
		{
			OrgContact result = null;

			if (org != null)
			{
				var query = new ZQuery(OrgContactSchema.OC_ContactName, contactName);
				var matchingActiveContacts = org.ContactsActive.Find(query);
				if (matchingActiveContacts.Length > 0)
				{
					result = (OrgContact)matchingActiveContacts[0];
				}
				else
				{
					var matchingInactiveContacts = org.Contacts.Find(query);
					if (matchingInactiveContacts.Length > 0)
					{
						result = (OrgContact)matchingInactiveContacts[0];
					}
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Reopen

		public void Reopen()
		{
			O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
		}

		/// <summary>
		/// Reopens
		/// </summary>
		/// <param name="pks">Array of primary keys to process</param>
		/// <returns>Error message if any.</returns>
		public static INotification Reopen(BusinessObjectFactory factory, ZGuid[] pks)
		{
			var type = CargoWise.EntityFramework.NotificationType.Information;
			var message = ZString.Empty;
			var security = Env.Security.InquiryManagerEdit;
			if (security.IsAllowed)
			{
				SalesEnquiry[] enquiries;

				if (pks.Length == 0 || (enquiries = factory.Load<SalesEnquiry>(new ZQuery(OrgColdCallRegisterSchema.PK, pks))).Length == 0)
				{
					type = CargoWise.EntityFramework.NotificationType.Error;
					message = Res.GetString("7F4BC036-C353-438A-B176-060B9B5D4A64", "You have not selected any records.");
				}
				else
				{
					foreach (SalesEnquiry enquiry in enquiries)
					{
						if (enquiry.O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Closed)
						{
							enquiry.Reopen();
						}
					}

					try
					{
						factory.Save();
						message = Res.GetString("FFE09F7B-4003-49F0-A548-694E499A63DE", "Selected closed Inquiries are reopened.");
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
			else
			{
				type = CargoWise.EntityFramework.NotificationType.Error;
				message = NoEditSecurityMsg;
			}
			return new Notification(type, message);
		}

		internal static string NoEditSecurityMsg
		{
			get { return Res.GetString("C5E4D070-8EFF-4A75-AC76-4D6D8482D568", "You do not have rights to reopen Inquiries."); }
		}

		#endregion

		#region LeadStatus

		public ZString LeadStatus
		{
			get
			{
				return O1_LeadStatus;
			}
			set
			{
				if (CanDoClose && value == SalesEnquiryStatusCodeList.Codes.Closed)
				{
					DoClose();
				}
				else if (value == SalesEnquiryStatusCodeList.Codes.Open)
				{
					Reopen();
				}
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new SalesEnquiryFetchStrategy(this);
		}

		#endregion

		#region Validation

		public new SalesEnquiryValidation Validation
		{
			get { return (SalesEnquiryValidation)GetNewValidation(); }
		}

		protected override OrgColdCallRegisterValidation GetNewValidation()
		{
			return new SalesEnquiryValidation(this);
		}

		#endregion

		#region Lookups

		public new SalesEnquiryLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new SalesEnquiryLookups(this);
				}
				return lookups;
			}
		}

		SalesEnquiryLookups lookups;

		#endregion

		#region InquiryLinkedCommunications

		IEnumerable<OrgSalesCall> InquiryLinkedCommunications
		{
			get
			{
				return
					(Factory.Load<IGlbCompanyCampaignItem>(new ZQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, PK)).Cast<ISalesRelationActivity>().SelectMany(x => x.RelatedChildActivityPivotCollection.Activities.OfType<OrgSalesCall>())
					.Union(RelatedChildActivityPivotCollection.Activities.OfType<OrgSalesCall>()))
					.Where(x => x.LinkedInquiry != null && x.LinkedInquiry.PK == PK);
			}
		}

		void UpdateInquiryLinkedCommunications()
		{
			if (O1_OH_ConvertedToQualifiedLead.IsValid && O1_OH_ConvertedToQualifiedLeadInfo.HasChanges)
			{
				foreach (var linkedCommunication in InquiryLinkedCommunications.ToArray())
				{
					linkedCommunication.OQ_OH = O1_OH_ConvertedToQualifiedLead;
					linkedCommunication.OQ_OC = O1_OC_LinkedContact;
				}
			}
		}

		void DeleteInquiryLinkedCommunications()
		{
			foreach (var linkedCommunication in InquiryLinkedCommunications.ToArray())
			{
				linkedCommunication.Delete();
			}
		}

		#endregion

		#region IDocManagerSupport Members

		internal class SalesEnquiryDocManagerInfo : DocManagerInfo
		{
			public SalesEnquiryDocManagerInfo(BusinessObject parent)
				: base(parent, Core.Constants.DocManagerCodes.SalesEnquiry)
			{
			}

			public override bool ReadOnly
			{
				get { return false; }
			}
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new SalesEnquiryDocManagerInfo(this);
				}
				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
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

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (tasks == null)
				{
					tasks = this.GetOrCreateProcessTaskCollection(() => new SalesEnquiryProcessTaskCollection(this));
					RegisterEditableChildObject(tasks);
				}
				return tasks;
			}
		}
		ProcessTaskCollection tasks;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new SalesEnquiryWorkflowDescriptor().Code; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			// Must match SalesEnquiryWorkflowDescriptor.SubTypeInformation
			// and SalesEnquiryFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			ColumnValueRanker result = new ColumnValueRanker();

			// 1
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, O1_EnquiryType, ZString.Empty);

			// 2
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, O1_LeadSource, ZString.Empty);

			// 3
			ZString orgRelationship = O1_OH_ConvertedToQualifiedLead.IsEmpty ? SalesEnquiryOrgRelationshipCodeList.Codes.NewOrg : SalesEnquiryOrgRelationshipCodeList.Codes.ExistingOrg;
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, orgRelationship, ZString.Empty);

			return result;
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region Testing Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			O1_LeadUniqueReference = ZString.Empty;
			O1_EnquiryType = Codes.SalesEnquiry;
			O1_CompanyName = "CompanyName";
			O1_ContactName = "ContactName";
		}

#endif
		#endregion

		#region IRelatableActivity

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.InquiryManager; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return ConvertedToQualifiedLead; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return O1_OH_ConvertedToQualifiedLeadInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return Contact; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return O1_OC_LinkedContactInfo.HasChanges; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { EnquiryTypeDescription, O1_LeadSource, O1_OpportunitySourceDetails, StatusDescription }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region ISalesRelationActivity Members

		public SalesRelationModel SalesRelationModel
		{
			get
			{
				if (salesRelationModel == null)
				{
					salesRelationModel = new SalesRelationModel(this);
					RegisterEditableChildObject(salesRelationModel);
				}

				return salesRelationModel;
			}
		}
		SalesRelationModel salesRelationModel;

		ISalesRelationModel ISalesRelationActivity.SalesRelationModel
		{
			get { return SalesRelationModel; }
		}

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return nameof(EnquiryNotesContent); }
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (parentActivity.Client != null)
			{
				O1_OH_ConvertedToQualifiedLead = parentActivity.Client.PK;
				O1_OA_LinkedAddress = parentActivity.Client.MainAddress != null ? parentActivity.Client.MainAddress.PK : ZGuid.Empty;
			}

			if (parentActivity.Contact != null)
			{
				O1_OC_LinkedContact = parentActivity.Contact.PK;
			}

			var parentAsCallActivity = parentActivity as OrgSalesCall;
			if (parentAsCallActivity != null)
			{
				O1_GS_NKRepAssigned = parentAsCallActivity.OQ_GS_NKSalesRep;
				EnquiryNotesContent = parentAsCallActivity.OQ_SalesCallNotes;
			}

			var parentAsInquiryActivity = parentActivity as SalesEnquiry;
			if (parentAsInquiryActivity != null)
			{
				O1_EnquiryType = parentAsInquiryActivity.O1_EnquiryType;
				O1_GS_NKRepAssigned = parentAsInquiryActivity.O1_GS_NKRepAssigned;
				EnquiryNotesContent = parentAsInquiryActivity.EnquiryNotesContent;
			}

			return true;
		}

		#endregion

		#region IImportChildRelatedActivityInfoOnNewSaved

		bool IImportChildRelatedActivityInfoOnNewSaved.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			var childOpportunity = childActivity as OrgOpportunity;
			if (childOpportunity != null)
			{
				OnConvertedToOpportunity(childOpportunity);
			}

			return true;
		}

		#endregion

		#region IImportChildRelatedActivityInfoOnAttach

		bool IImportChildRelatedActivityInfoOnAttach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (O1_LeadStatus != SalesEnquiryStatusCodeList.Codes.Converted)
			{
				var childOpportunity = childActivity as OrgOpportunity;
				if (childOpportunity != null && childOpportunity.Enquiry == null)
				{
					var decider = deciderFactory.GetIfAvailable<IImportRelatedActivityYesNoDecider>();
					var shouldConvert = decider != null && decider.GetDecision(Res.GetString("651812c7-959f-48b8-9975-256943c72752", "Convert {0} to {1}?", HumanReadableName, childOpportunity.HumanReadableName));

					if (shouldConvert)
					{
						var childOpportunityInLocalFactory = Factory.Load<OrgOpportunity>(childOpportunity.PK);
						if (childOpportunityInLocalFactory != null)
						{
							O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
							childOpportunityInLocalFactory.P8_O1_Enquiry = PK;
							ReadOnly = true;
						}
					}
					else
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region IGlbCompanyCampaignItemRecipient Members

		string IGlbCompanyCampaignItemRecipient.Phone
		{
			get { return O1_Phone; }
		}

		OrgHeader IGlbCompanyCampaignItemRecipient.Organisation
		{
			get { return Factory.Load<OrgHeader>(O1_OH_ConvertedToQualifiedLead); }
		}

		string IGlbCompanyCampaignItemRecipient.Salutation
		{
			get { return string.Empty; }
		}

		string IGlbCompanyCampaignItemRecipient.Title
		{
			get { return string.Empty; }
		}

		string IGlbCompanyCampaignItemRecipient.Fax
		{
			get { return O1_Fax; }
		}

		string IGlbCompanyCampaignItemRecipient.RelatedDocName
		{
			get
			{
				var orgCode = string.IsNullOrWhiteSpace(OrgCode) ? string.Empty : $" ({OrgCode})";
				return $"{ResString.GetMultilingualString("BC840BA2-B7F6-4751-ADD2-05DD19A5CB9E", "Contact")} {O1_ContactName + orgCode}";
			}
		}

		#endregion

		#region IContactable Members

		string IContactBase.Name
		{
			get { return this.O1_ContactName; }
		}

		string IContactBase.Email
		{
			get { return this.O1_Email; }
		}

		string IContactable.Mobile
		{
			get { return this.O1_Mobile; }
		}

		IContactable[] IContactable.GetNestedContacts(string parentContactDescription)
		{
			return Array.Empty<IContactable>();
		}

		#endregion

		public bool IsActive
		{
			get { return true; }
		}

		#region ISupportWebAddressValidation

		public bool IsErrorSuppressed => false;
		public bool IsJobDocAddress => false;

		public event EventHandler TriggerWebAddressValidation;
		public event EventHandler AddressValidationStatusChanged;
		public event EventHandler TriggerWebGetCityTown;

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

		public AddressValidationSection ValidationSection { get; } = AddressValidationSection.SalesInquiry;

		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			if (Env.Registry.EnableAddressValidationWebService)
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
				else if (propertyInfo.Name == nameof(O1_PortOrCountry))
				{
					ValidationStatus = AddressValidationStatus.ToBeVerified;
					AddressMap = string.Empty;
				}
			}
		}

		void RaiseWebServices(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(O1_PortOrCountry))
			{
				if ((string.IsNullOrEmpty(O1_City) || string.IsNullOrEmpty(O1_State) || string.IsNullOrEmpty(O1_PostCode)) && (propertyInfo == O1_CityInfo || propertyInfo == O1_StateInfo || propertyInfo == O1_PostCodeInfo))
				{
					RaiseTriggerWebGetCityTown(propertyInfo);
				}
				else
				{
					RaiseTriggerWebAddressValidation(propertyInfo);
				}
			}
		}

		void RaiseTriggerWebGetCityTown(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebGetCityTown != null)
			{
				TriggerWebGetCityTown(this, new InfoEventArgs(propertyInfo));
			}
		}

		void RaiseTriggerWebAddressValidation(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebAddressValidation != null)
			{
				TriggerWebAddressValidation(this, new InfoEventArgs(propertyInfo));
			}
		}

		public void PreValidationForAddressValidationService()
		{
			Validation.ValidateO1_Address1();
			Validation.ValidateO1_City();
			Validation.ValidateO1_PortOrCountry();
			ValidatePostcodeAndStateForAddress();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Validation.ValidateO1_PostCode();
			Validation.ValidateO1_State();
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

		[BusinessObjectTestExclude] // Language is English only
		public ZString Language
		{
			get { return Enterprise.Core.Constants.Languages.English; }
			set { }
		}

		public ZPropertyInfo LanguageInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Language));
			}
		}

		public int Language_MaxLength
		{
			get { return Schema.LanguageMaxLength; }
		}

		public CodeDescriptionPairList LanguageList { get { return null; } }

		public ZString Address1
		{
			get { return O1_Address1; }
			set { O1_Address1 = value; }
		}

		public ZPropertyInfo Address1Info
		{
			get { return O1_Address1Info; }
		}

		public int Address1_MaxLength
		{
			get { return AutoOrgColdCallRegister.Schema.O1_Address1MaxLength; }
		}

		public ZString Address2
		{
			get { return O1_Address2; }
			set { O1_Address2 = value; }
		}

		public ZPropertyInfo Address2Info
		{
			get { return O1_Address2Info; }
		}

		public int Address2_MaxLength
		{
			get { return AutoOrgColdCallRegister.Schema.O1_Address2MaxLength; }
		}

		public ZString City
		{
			get { return O1_City; }
			set { O1_City = value; }
		}

		public ZPropertyInfo CityInfo
		{
			get { return O1_CityInfo; }
		}

		public int City_MaxLength
		{
			get { return AutoOrgColdCallRegister.Schema.O1_CityMaxLength; }
		}

		public ZString Postcode
		{
			get { return O1_PostCode; }
			set { O1_PostCode = value; }
		}

		public ZPropertyInfo PostcodeInfo
		{
			get { return O1_PostCodeInfo; }
		}

		public int Postcode_MaxLength
		{
			get { return AutoOrgColdCallRegister.Schema.O1_PostCodeMaxLength; }
		}

		public ZString CompanyName
		{
			get { return O1_CompanyName; }
			set { O1_CompanyName = value; }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return O1_CompanyNameInfo; }
		}

		public int CompanyName_MaxLength
		{
			get { return AutoOrgColdCallRegister.Schema.O1_CompanyNameMaxLength; }
		}

		public ZString StateCode
		{
			get { return O1_State; }
			set
			{
				O1_State = value;
				StateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateCodeInfo
		{
			get { return O1_StateInfo; }
		}

		public int StateCode_MaxLength
		{
			get { return AutoOrgColdCallRegister.Schema.O1_StateMaxLength; }
		}

		public CodeDescriptionPairList StateCodeList
		{
			get
			{
				return Lookups.StateList;
			}
		}

		[BusinessObjectTestExclude] // State has to be valid
		public ZString State
		{
			get { return StateCodeList.GetDescriptionFromCode(O1_State); }
			set
			{
				var code = (ZString)StateCodeList.GetCodeFromDescription(value);
				O1_State = string.IsNullOrEmpty(code) ? value : code;
				StateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		public int State_MaxLength
		{
			get { return AutoOrgColdCallRegister.Schema.O1_StateMaxLength; }
		}

		[List("CountryCodeList")]
		public ZString CountryCodeISO2
		{
			get { return O1_PortOrCountry.SubstringSafe(0, 2); }
			set { O1_PortOrCountry = value + O1_PortOrCountry.SubstringSafe(2); }
		}

		public int CountryCodeISO2_MaxLength => Schema.O1_PortOrCountryMaxLength;

		public RefCountry Country
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodeISO2); }
		}

		public RefCountryCollection CountryCodeList
		{
			get
			{
				return Lookups.OrgCountryList;
			}
		}

		public ZString DisplayText
		{
			get { return O1_BusinessRegNo; }
			set { O1_BusinessRegNo = value; }
		}

		public ZGuid EntityPK
		{
			get { return PK; }
		}

		public ZString AddressRecordGUID
		{
			get { return PK.ToString(); }
		}

		public ZString AddressSourceTable
		{
			get { return OrgColdCallRegisterSchema.Constants.Prefix; }
		}

		public ZString ValidationStatus
		{
			get
			{
				return O1_ValidationStatus;
			}
			set
			{
				O1_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();
			}
		}

		public ZString AddressMap
		{
			get { return O1_AddressMap; }
			set { O1_AddressMap = value; }
		}

		public ZString Addressee
		{
			get { return O1_ContactName; }
		}

		public ZGeography GeoLocation
		{
			get { return O1_GeoLocation; }
			set { O1_GeoLocation = value; }
		}

		public ZString ClosestPort { get; set; }

		public bool NeedValidation
		{
			get
			{
				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, O1_PortOrCountry);
				if (country != null)
				{
					if (!O1_Address1.IsEmpty && !O1_PostCode.IsEmpty && !O1_City.IsEmpty && !O1_State.IsEmpty)
					{
						if (!IsInDatabase || (O1_Address1Info.HasChanges || O1_Address2Info.HasChanges || O1_PostCodeInfo.HasChanges ||
												O1_CityInfo.HasChanges || O1_StateInfo.HasChanges || O1_PortOrCountryInfo.HasChanges))
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
		public bool IsTSAKnownAddress => false;
		public bool IsMIDAddress => false;

		#endregion

		protected bool O1_GS_NKRepAssigned_ReadOnly
		{
			get { return IsInDatabase && !Env.Security.InquiryManagerEditModifyStaffAssignment.IsAllowed; }
		}

		[DocumentFieldExcludeFromMap]
		[BusinessObjectTestExclude]
		public ZString UnrestrictedAdditionalAddressInformation
		{ get; set; }

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => GetZPropertyInfo(nameof(UnrestrictedAdditionalAddressInformation));

		[BusinessObjectTestExclude]
		public CodeDescriptionPairList AdditionalAddressInfoList { get; }

		public ZString AddressCode
		{ get; set; }

		#region Security

		SecurityCheckpoint[] ContactNewRelatedCheckpoints
		{
			get
			{
				if (contactNewRelatedCheckpoints == null)
				{
					contactNewRelatedCheckpoints = new SecurityCheckpoint[] { Env.Security.OrgContactModify, Env.Security.OrgContactModifyContactDetails, Env.Security.OrgContactNew };
				}
				return contactNewRelatedCheckpoints;
			}
		}

		SecurityCheckpoint[] contactNewRelatedCheckpoints;

		SecurityCheckpoint[] ContactModifyRelatedCheckpoints
		{
			get
			{
				if (contactModifyRelatedCheckpoints == null)
				{
					contactModifyRelatedCheckpoints = new SecurityCheckpoint[] { Env.Security.OrgContactModify, Env.Security.OrgContactModifyContactDetails };
				}
				return contactModifyRelatedCheckpoints;
			}
		}

		SecurityCheckpoint[] contactModifyRelatedCheckpoints;

		SecurityCheckpoint[] AddressModifyRelatedCheckpoints
		{
			get
			{
				if (addressModifyRelatedCheckpoints == null)
				{
					addressModifyRelatedCheckpoints = new SecurityCheckpoint[] { Env.Security.OrgAddressModify, Env.Security.OrgAddressListModify, Env.Security.OrgAddressCapabilitiesNonARAP, Env.Security.OrgAddressDetailsModify, Env.Security.OrgAddressDetailsNonARAP };
				}
				return addressModifyRelatedCheckpoints;
			}
		}

		SecurityCheckpoint[] addressModifyRelatedCheckpoints;

		public SecurityCheckpoint MissingContactNewSecurity(OrgHeader org)
		{
			return ContactNewRelatedCheckpoints.FirstOrDefault(x => !x.IsAllowed) ?? MissingStaffRoleModifySecurity(org);
		}

		public SecurityCheckpoint MissingContactModifySecurity(OrgHeader org)
		{
			return ContactModifyRelatedCheckpoints.FirstOrDefault(x => !x.IsAllowed) ?? MissingStaffRoleModifySecurity(org);
		}

		public SecurityCheckpoint MissingSecurityForLinkToOrganizationByLinkingToAddress(ZGuid orgPK, ZString contactName)
		{
			var org = Factory.Load<OrgHeader>(orgPK);
			return FindMatchingContact(org, contactName) == null ? MissingContactNewSecurity(org) : null;
		}

		public SecurityCheckpoint MissingSecurityForLinkToOrganizationByLinkingToAddress(ZGuid orgPK)
		{
			return MissingSecurityForLinkToOrganizationByLinkingToAddress(orgPK, O1_ContactName);
		}

		public SecurityCheckpoint MissingSecurityForLinkToOrganizationByAddingInquiryAddress(ZGuid orgPK)
		{
			var missingSecurity = AddressModifyRelatedCheckpoints.FirstOrDefault(x => !x.IsAllowed);
			return missingSecurity ?? MissingSecurityForLinkToOrganizationByLinkingToAddress(orgPK);
		}

		SecurityCheckpoint MissingStaffRoleModifySecurity(OrgHeader org)
		{
			var checkpoints = GetStaffRoleCheckpoints(org);
			if (checkpoints.Length > 0)
			{
				return checkpoints.FirstOrDefault(x => !x.IsAllowed);
			}
			else
			{
				return Env.Security.OrganisationCRMSecurity.EditByStaffNotAssigned.IsAllowed ? null : Env.Security.OrganisationCRMSecurity.EditByStaffNotAssigned;
			}
		}

		SecurityCheckpoint[] GetStaffRoleCheckpoints(OrgHeader org)
		{
			var staffAssignments = org.StaffAssignments.OfType<OrgStaffAssignments>().Where(x => x.O8_GS_NKPersonResponsible == GlbStaff.CurrentUser.GS_Code);
			var result = Env.Security.OrganisationCRMSecurity.EditByStaffRoleAssignedLookup.Where(x => staffAssignments.Any(a => a.O8_Role == x.Key)).Select(x => x.Value).ToArray();
			return result;
		}

		#endregion
	}
}
