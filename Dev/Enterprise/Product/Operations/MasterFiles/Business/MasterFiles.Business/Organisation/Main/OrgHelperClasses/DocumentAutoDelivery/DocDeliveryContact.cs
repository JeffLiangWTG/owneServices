using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DocDeliveryContact : NonPersistentBusinessObject
	{
		public DocDeliveryContact(BusinessObjectFactory factory)
			: base(factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory), "BusinessObjectFactory factory");
			}
		}
		public DocDeliveryContact(BusinessObjectFactory factory, DocAutoDelivery docAutoDelivery)
			: base(factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory), "BusinessObjectFactory factory");
			}
			if (docAutoDelivery == null)
			{
				throw new ArgumentNullException(nameof(docAutoDelivery), "DocAutoDelivery docAutoDelivery");
			}
			DocAutoDelivery = docAutoDelivery;
		}

		internal IStmMenuItem MenuItem { get; private set; }
		public OrgDocument DocumentDeliveryType { get; private set; }
		internal IDocAddress OverridenAddress { get; private set; }
		public bool UpdateEmailAndFax { get; set; } = true;

		public DocAutoDelivery DocAutoDelivery { get; private set; }

		public void Initialise(IStmMenuItem menuItem, OrgDocument documentDeliveryType)
		{
			MenuItem = menuItem;
			DocumentDeliveryType = documentDeliveryType;
		}

		internal void Initialise(IStmMenuItem menuItem, IDocAddress overridenAddress, CodeDescriptionPairList overriddenAttachmentTypeList, CodeDescriptionPairList overridenNotifyModes, ZString deliveryLanguageInitialValue, bool attachmentTypeDisabledInitialValue, DocAutoDelivery docAutoDelivery, ZString defaultEmailFromAddress)
		{
			MenuItem = menuItem;
			OverridenAddress = overridenAddress;
			AttachmentTypes = overriddenAttachmentTypeList;
			NotifyModes = overridenNotifyModes;
			DeliveryLanguage = deliveryLanguageInitialValue;
			AttachmentTypeDisabled = attachmentTypeDisabledInitialValue;
			DocAutoDelivery = docAutoDelivery;
			DefaultEmailFromAddress = defaultEmailFromAddress;
		}

		#region Properties

		#region EmailSubject

		[MaxLength(AutoOrgDocument.Schema.OD_EmailSubjectMacroMaxLength)]
		[BusinessObjectTestExclude]
		public ZString EmailSubjectMacro
		{
			get => emailSubjectMacro;
			set => SetNonPersistentPropertyValue(EmailSubjectMacroInfo, ref emailSubjectMacro, value);
		}

		ZString emailSubjectMacro;

		ZString previousEmailSubjectMacro;

		public ZPropertyInfo EmailSubjectMacroInfo => GetZPropertyInfo(nameof(EmailSubjectMacro), "Email Subject");

		protected internal bool EmailSubjectMacro_ReadOnly => !DeliveryMethodHelper.IsEmail(DeliveryMethod);

		ZGuid deliveryGroupId;
		public ZGuid DeliveryGroupId
		{
			get => deliveryGroupId;
			set
			{
				if (EmailSubjectMacro.IsEmpty && !value.IsEmpty)
				{
					throw new InvalidOperationException("You shouldn't generate a delivery group id when EmailSubject macro is empty.");
				}

				deliveryGroupId = value;
			}
		}

		#endregion

		#region Contact

		[List("Contacts")]
		[MaxLength(256)]
		public ZString Name
		{
			get { return fName; }
			set
			{
				bool hasChanges = fName != value;
				CheckMaximumLength(NameInfo, value);
				SetNonPersistentPropertyValue(NameInfo, ref fName, value);
				if (hasChanges)
				{
					contact = null;

					if (Contact != null)
					{
						OrgHeaderPK = Contact.OC_OH;

						using (Contact.GenerateDocDeliveryRecipient())
						{
							if (DeliveryMethod.IsEmpty)
							{
								DeliveryMethod = Contact.OC_NotifyMode;
							}

							if (AttachmentType.IsEmpty)
							{
								AttachmentType = Contact.OC_AttachmentType;
							}

							Salutation = Contact.OC_Salutation;
							if (!Contact.IsSystemDefaultContactForAutoDelivery && Contact.WorkingAddressPK.IsValid)
							{
								OrgAddressPK = Contact.WorkingAddressPK;
							}

							this.UpdatePhoneDetails();
							this.UpdateFaxDetails();
							this.UpdateEmailDetails();
							this.UpdateCcAndBccEmailDetails();
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateDeliveryAddress();
					}

					if (Contact != null && Contact.IsSystemDefaultContact)
					{
						SystemDefaultContactName = Contact.SystemDefaultContactName;
						Salutation = Contact.OC_Salutation;
					}
					else if (OrgHeader == null && SystemDefaultContactName != null)
					{
						//In this case there will be only one system default recipient so just keep the contact name multilingual string
					}
					else
					{
						SystemDefaultContactName = null;
					}
				}
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		[List("Salutations")]
		[BusinessObjectTestExclude]
		[MaxLength(100)]
		public ZString Salutation
		{
			get { return salutation; }
			set
			{
				if (salutation != value)
				{
					if (value.Length > SalutationInfo.MaxLength)
					{
						ErrorReporter.ReportOnce("Salutation exceeds MaxLength", string.Format(CultureInfo.InvariantCulture, "Provided Salutation exceeds {0} characters. {1} characters were entered. Salutation: {2}", SalutationInfo.MaxLength, value.Length, value));
					}
					value = value.Left(SalutationInfo.MaxLength);
					SetNonPersistentPropertyValue(SalutationInfo, ref salutation, value);
				}
			}
		}

		public ZPropertyInfo SalutationInfo
		{
			get { return GetZPropertyInfo(nameof(Salutation)); }
		}

		public CodeDescriptionPairList Salutations
		{
			get
			{
				string language = DeliveryLanguage == ZString.Empty ? Constants.Languages.English : (string)DeliveryLanguage;
				var gender = Contact == null ? ZString.Empty : Contact.OC_Gender;
				var list = SalutationHelper.GetSalutations(gender);

				CodeDescriptionPairList salutations = new CodeDescriptionPairList();
				foreach (var salutation in list)
				{
					var code = salutation.ToString(language);
					if (code.Contains(Core.Constants.SalutationMacros.Name) || code.Contains(Core.Constants.SalutationMacros.JobCategory))
					{
						code = code.Replace(Core.Constants.SalutationMacros.Name, string.IsNullOrEmpty(Name) ? SystemDefaultContactName : Name);
						code = code.Replace(Core.Constants.SalutationMacros.JobCategory, Contact == null ? ZString.Empty : Contact.OC_JobCategory).Replace("  ", " ").Replace(" ,", ",");
					}
					var genderPrefix = gender == Constants.Genders.Man ? (NoResString)"[m] " : (gender == Constants.Genders.Woman ? (NoResString)"[f] " : "");
					if (!string.IsNullOrEmpty(genderPrefix))
					{
						code = code.Replace(genderPrefix, "");
					}

					if (!salutations.ContainsCode(salutation.GetUnresolvedString()))
					{
						salutations.AddPair(code, salutation);
					}
				}
				return salutations;
			}
		}

		public OrgContact Contact
		{
			get
			{
				if (contact == null && (IsSystemDefaultContact || !string.IsNullOrEmpty(Name)))
				{
					BusinessObject[] foundContacts = Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, Name));
					if (foundContacts.Length > 0)
					{
						contact = (OrgContact)foundContacts[0];
					}
				}
				return contact;
			}
		}
		OrgContact contact;

		public MultilingualString SystemDefaultContactName { get; set; }

		public bool IsSystemDefaultContact { get; set; }

		public ZString ContactDeliveryAddress
		{
			get
			{
				ZString result = ZString.Empty;
				OrgContact currentContact = Contact;
				if (currentContact != null)
				{
					if (DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
					{
						result = currentContact.OC_Email;
					}
					else if (DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
					{
						result = currentContact.OC_Fax;
					}
				}
				return result;
			}
		}

		ZString salutation;
		ZString fName;

		#endregion

		public ZGuid OrgAddressPK
		{
			get { return orgAddressPK; }
			set
			{
				if (orgAddressPK != value)
				{
					CompanyName = string.Empty;

					SetNonPersistentPropertyValue(OrgAddressPKInfo, ref orgAddressPK, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrgAddressPK();
					}

					using (var entryState = orgDefaultingEntryPoint.GetState())
					{
						if (entryState.IsAllowed && (Contact == null || !Contact.IsGeneratingDocDeliveryRecipient))
						{
							OrgHeaderPK = OrgAddress == null ? ZGuid.Empty : OrgAddress.OA_OH;
						}
					}

					this.PopulateAddressDetailsFromAddressOrContact();
				}
			}
		}
		ZGuid orgAddressPK;

		public ZPropertyInfo OrgAddressPKInfo
		{
			get { return GetZPropertyInfo(nameof(OrgAddressPK), "Address"); }
		}

		public OrgAddress OrgAddress
		{
			get { return Factory.Load<OrgAddress>(OrgAddressPK); }
		}

		readonly EntryPoint orgDefaultingEntryPoint = new EntryPoint();

		#region Organisation
		[List("Organisations")]
		public ZGuid OrgHeaderPK
		{
			get { return orgHeaderPK; }
			set
			{
				if (orgHeaderPK != value)
				{
					CompanyName = string.Empty;
					contact = null;

					SetNonPersistentPropertyValue(OrgHeaderPKInfo, ref orgHeaderPK, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrgHeaderPK();
					}
					EmailToRecipients.Organization = OrgHeader;
					EmailBlindCarbonCopyRecipients.Organization = OrgHeader;
					EmailCarbonCopyRecipients.Organization = OrgHeader;

					using (var entryState = orgDefaultingEntryPoint.GetState())
					{
						if (entryState.IsAllowed)
						{
							ResetAddressDetailsFromOrganisation();
						}
					}
				}
			}
		}

		public bool OrgHeaderPK_ReadOnly => !string.IsNullOrEmpty(DeliveryToTypeCode) &&  DeliveryToTypeCode != ScheduledReportDeliveryRecipientConstants.RecipientType.Contact || !StaffCode.IsEmpty;

		public ZPropertyInfo OrgHeaderPKInfo
		{
			get { return GetZPropertyInfo(nameof(OrgHeaderPK), "Organisation"); }
		}

		public OrgHeader OrgHeader
		{
			get { return (OrgHeader)Factory.Load(typeof(OrgHeader), OrgHeaderPK); }
		}

		[MaxLength(OrgAddress.Schema.OA_CompanyNameOverrideMaxLength)]
		public ZString CompanyName
		{
			get { return companyName; }
			set
			{
				if (companyName != value)
				{
					CheckMaximumLength(CompanyNameInfo, value);
					SetNonPersistentPropertyValue(CompanyNameInfo, ref companyName, value);
				}
			}
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyName)); }
		}

		ZString companyName;
		ZGuid orgHeaderPK;

		#endregion

		#region staff recipient

		public GlbStaff Staff { get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, StaffCode); } }

		[List("StaffRecipients")]
		public ZString StaffCode
		{
			get { return staffCode; }
			set
			{
				value = value.TrimEndSpaceTab();
				if (staffCode != value)
				{
					SetNonPersistentPropertyValue(StaffCodeInfo, ref staffCode, value);
					if (Staff != null)
					{
						Email = Staff.GS_EmailAddress;
						Name = Staff.GS_FullName;
					}
				}
			}
		}

		ZString staffCode;

		public bool StaffCode_ReadOnly => !string.IsNullOrEmpty(DeliveryToTypeCode) && DeliveryToTypeCode != ScheduledReportDeliveryRecipientConstants.RecipientType.Staff || !OrgHeaderPK.IsEmpty;

		public ZPropertyInfo StaffCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StaffCode), "Staff"); }
		}

		#endregion

		#region Delivery Recipient Type

		public ZString DeliveryToTypeCode
		{
			get
			{
				return deliveryToTypeCode;
			}
			set
			{
				deliveryToTypeCode = value;
				if (value != ScheduledReportDeliveryRecipientConstants.RecipientType.Contact)
				{
					OrgHeaderPK = ZGuid.Empty;
				}

				if (value != ScheduledReportDeliveryRecipientConstants.RecipientType.Staff)
				{
					StaffCode = ZString.Empty;
				}
			}
		}

		ZString deliveryToTypeCode;

		public ZString DeliveryRecipientType
		{
			get { return deliveryRecipientType ?? DeliveryRecipientTypes.GetDescriptionFromCode(DeliveryToTypeCode); }
			set
			{
				CheckMaximumLength(DeliveryRecipientTypeInfo, value);
				deliveryRecipientType = value;
				if (!DeliveryRecipientTypeInfo.HasErrors())
				{
					DeliveryToTypeCode = DeliveryRecipientTypes.GetCodeFromDescription(value);
				}
				DeliveryRecipientTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryRecipientTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryRecipientType)); }
		}

		ZString? deliveryRecipientType;

		#endregion

		#region DeliveryMethod

		[List("SystemNotifyModes")]
		[MaxLength(3)]
		public ZString DeliveryMethod
		{
			get { return deliveryMethod; }
			set
			{
				bool hasChanges = deliveryMethod != value;
				CheckMaximumLength(DeliveryMethodInfo, value);
				SetNonPersistentPropertyValue(DeliveryMethodInfo, ref deliveryMethod, value);
				if (hasChanges)
				{
					if (!DeliveryMethodHelper.IsEmail(deliveryMethod))
					{
						previousEmailSubjectMacro = EmailSubjectMacro;
						EmailSubjectMacro = string.Empty;
					}
					else if (EmailSubjectMacro.IsEmpty && !previousEmailSubjectMacro.IsEmpty)
					{
						EmailSubjectMacro = previousEmailSubjectMacro;
					}

					AttachmentType = GetDefaultAttachmentType();

					SendIndividually = false;

					OrgAddress overridenOrgAddress = OverridenAddress == null ? null : Factory.Load<OrgAddress>(OverridenAddress.E2_OA_Address);

					if (OverridenAddress == null || (overridenOrgAddress != null && overridenOrgAddress.OA_OH != OrgHeaderPK))
					{
						if (Contact == null && OrgHeader != null)
						{
							if (value == Core.Constants.ContactNotifyModes.Email)
							{
								Email = OrgHeader.MainAddress.OA_Email;
							}
							else if (value == Core.Constants.ContactNotifyModes.Fax)
							{
								Fax = OrgHeader.MainAddress.OA_Fax;
							}
						}

						ResetAddressDetailsFromOrganisation();
					}
					else if (overridenOrgAddress != null)
					{
						OrgAddressPK = overridenOrgAddress.PK;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateDeliveryMethod();
						Validation.ValidateDeliveryAddress();
					}

					this.UpdateCcAndBccEmailDetails();
				}
			}
		}

		public void ResetAddressDetailsFromOrganisation()
		{
			this.PopulateAddressDetailsFromOrganisation();
		}

		public static MultilingualString NoOrganizationDetailsFoundMessage
		{
			get { return ResString.GetMultilingualString("6aa64fab-f81f-4777-bc8d-bd700013d1db", "*** NO ORGANIZATION DETAILS FOUND ***"); }
		}

		public ZPropertyInfo DeliveryMethodInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryMethod)); }
		}

		bool fDeliveryMethod_ReadOnly;
		public bool DeliveryMethod_ReadOnly
		{
			get => fDeliveryMethod_ReadOnly;
			set
			{
				fDeliveryMethod_ReadOnly = value;
				DeliveryMethodInfo.RefreshBinding();
			}
		}

		ZString GetDefaultAttachmentType()
		{
			ZString result = "";

			if (DeliveryMethodHelper.IsEmailOrEPrint(DeliveryMethod))
			{
				if (Contact != null)
				{
					result = Contact.OC_AttachmentType;
				}

				if (result.IsEmpty && MenuItem != null && !MenuItem.SU_DefaultAttachmentType.IsEmpty)
				{
					result = MenuItem.SU_DefaultAttachmentType;
				}

				if (!AttachmentTypes.ContainsCode(result))
				{
					if (AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF))
					{
						result = OrgConstants.AttachmentType.PDF;
					}
					else if (AttachmentTypes.Count > 0)
					{
						var firstAttachmentType = AttachmentTypes[0] as ICodeDescription;
						if (firstAttachmentType != null)
						{
							result = firstAttachmentType.Code;
						}
					}
				}
			}
			else if (DeliveryMethodHelper.IsEDoc(DeliveryMethod))
			{
				result = SystemDataRegistry.Instance.EDocImportFileFormat.Value;
			}

			return result;
		}

		[List("NotifyModes"), BusinessObjectTestExclude]
		public ZString DeliveryMethodDescription
		{
			get { return SystemNotifyModes.GetDescriptionFromCode(DeliveryMethod); }
			set
			{
				object code = SystemNotifyModes.GetCodeFromDescription(value);
				DeliveryMethod = code != null ? (ZString)(code.ToString()) : ZString.Empty;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeliveryMethodDescription();
				}
			}
		}

		public ZPropertyInfo DeliveryMethodDescriptionInfo => GetZPropertyInfo(nameof(DeliveryMethodDescription));

		public bool DeliveryMethodDescription_ReadOnly
		{
			get => DeliveryMethod_ReadOnly;
			set => DeliveryMethod_ReadOnly = value;
		}

		ZString deliveryMethod;

		#endregion

		#region DeliveryLanguage

		[MaxLength(7)]
		public ZString DeliveryLanguage
		{
			get { return deliveryLanguage; }
			set
			{
				bool hasChanges = deliveryLanguage != value;
				SetNonPersistentPropertyValue(DeliveryLanguageInfo, ref deliveryLanguage, value);
				if (hasChanges)
				{
					ResetAddressDetailsFromOrganisation();
				}
			}
		}

		public ZPropertyInfo DeliveryLanguageInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryLanguage)); }
		}

		ZString deliveryLanguage;

		#endregion

		#region AttachmentType

		[BusinessObjectTestExclude]     // Does not return what it is set to - depends on Delivery Mode
		[List("AttachmentTypes")]
		[MaxLength(4)]
		public ZString AttachmentType
		{
			get { return DeliveryMethodHelper.IsEmailOrEPrint(DeliveryMethod) || DeliveryMethod == Core.Constants.ContactNotifyModes.Ftp || DeliveryMethod == Core.Constants.ContactNotifyModes.EDoc ? attachmentType : ZString.Empty; }
			set
			{
				if (attachmentType != value)
				{
					CheckMaximumLength(AttachmentTypeInfo, value);
					SetNonPersistentPropertyValue(AttachmentTypeInfo, ref attachmentType, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateAttachmentType();
					}
				}
			}
		}

		public ZPropertyInfo AttachmentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(AttachmentType)); }
		}

		internal bool AttachmentType_ReadOnly
		{
			get { return AttachmentTypeDisabled || !DeliveryMethodHelper.IsEmailOrEPrint(DeliveryMethod) && !DeliveryMethodHelper.IsEDoc(DeliveryMethod); }
		}

		public bool AttachmentTypeDisabled { get; set; }

		ZString attachmentType;

		public ZString AttachmentTypeWithFormatSwitching
		{
			get
			{
				return AttachmentType == AttachmentTypeList.Codes.Xls && IsFormatSwitchingRequired ? (ZString)AttachmentTypeList.Codes.Xlsx : AttachmentType;
			}
		}

		public ZBool IsFormatSwitchingRequired { get; set; }

		#endregion

		#region SendIndividually

		public ZBool SendIndividually
		{
			get { return DeliveryMethod == Constants.ContactNotifyModes.Print ? true : sendIndividually; }
			set
			{
				if (sendIndividually != value)
				{
					SetNonPersistentPropertyValue(SendIndividuallyInfo, ref sendIndividually, value);
				}
			}
		}

		public ZPropertyInfo SendIndividuallyInfo => GetZPropertyInfo(nameof(SendIndividually));

		internal bool SendIndividually_ReadOnly => DeliveryMethod == Constants.ContactNotifyModes.Print;

		ZBool sendIndividually;

		#endregion

		#region Address1

		[MaxLength(60)]
		public ZString Address1
		{
			get { return address1; }
			set
			{
				if (address1 != value)
				{
					CheckMaximumLength(Address1Info, value);
					SetNonPersistentPropertyValue(Address1Info, ref address1, value);
				}
			}
		}

		public ZPropertyInfo Address1Info
		{
			get { return GetZPropertyInfo(nameof(Address1)); }
		}

		ZString address1;

		#endregion

		#region AdditionalAddress

		[MaxLength(60)]
		public ZString AdditionalAddress
		{
			get { return additionalAddress; }
			set
			{
				if (additionalAddress != value)
				{
					CheckMaximumLength(AdditionalAddressInfo, value);
					SetNonPersistentPropertyValue(AdditionalAddressInfo, ref additionalAddress, value);
				}
			}
		}

		public ZPropertyInfo AdditionalAddressInfo
		{
			get { return GetZPropertyInfo(nameof(AdditionalAddress)); }
		}

		ZString additionalAddress;

		#endregion

		#region Address2

		[MaxLength(50)]
		public ZString Address2
		{
			get { return address2; }
			set
			{
				if (address2 != value)
				{
					CheckMaximumLength(Address2Info, value);
					SetNonPersistentPropertyValue(Address2Info, ref address2, value);
				}
			}
		}

		public ZPropertyInfo Address2Info
		{
			get { return GetZPropertyInfo(nameof(Address2)); }
		}

		ZString address2;

		#endregion

		#region City

		[MaxLength(50)]
		public ZString City
		{
			get { return city; }
			set
			{
				if (city != value)
				{
					CheckMaximumLength(CityInfo, value);
					SetNonPersistentPropertyValue(CityInfo, ref city, value);
				}
			}
		}

		public ZPropertyInfo CityInfo
		{
			get { return GetZPropertyInfo(nameof(City)); }
		}

		ZString city;

		#endregion

		#region State

		[MaxLength(25)]
		public ZString State
		{
			get { return state; }
			set
			{
				if (state != value)
				{
					CheckMaximumLength(StateInfo, value);
					SetNonPersistentPropertyValue(StateInfo, ref state, value);
				}
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		ZString state;

		#endregion

		#region PostCode

		[MaxLength(10)]
		public ZString PostCode
		{
			get { return postCode; }
			set
			{
				if (postCode != value)
				{
					CheckMaximumLength(PostCodeInfo, value);
					SetNonPersistentPropertyValue(PostCodeInfo, ref postCode, value);
				}
			}
		}

		public ZPropertyInfo PostCodeInfo
		{
			get { return GetZPropertyInfo(nameof(PostCode)); }
		}

		ZString postCode;

		#endregion

		#region Phone

		[MaxLength(20)]
		public ZString Phone
		{
			get { return phone; }
			set
			{
				if (phone != value)
				{
					CheckMaximumLength(PhoneInfo, value);
					SetNonPersistentPropertyValue(PhoneInfo, ref phone, value);
				}
			}
		}

		public ZPropertyInfo PhoneInfo
		{
			get { return GetZPropertyInfo(nameof(Phone)); }
		}

		ZString phone;

		#endregion

		#region DeliveryAddress

		[BusinessObjectTestExclude] // Does not necessarily return what is set
		[MaxLength("DeliveryAddress_MaxLength")]
		public ZString DeliveryAddress
		{
			get
			{
				ZString result = "";

				if (DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					result = Email;
				}
				else if (DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
				{
					result = Fax;
				}
				else if (DeliveryMethod == Core.Constants.ContactNotifyModes.Ftp)
				{
					result = FileLocation;
				}
				else if (DeliveryMethod == Core.Constants.ContactNotifyModes.EPrint)
				{
					result = EPrintEmail;
				}
				return result;
			}
			set
			{
				if (DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					Email = value;
				}
				else if (DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
				{
					Fax = value;
				}
				else if (DeliveryMethod == Core.Constants.ContactNotifyModes.Ftp)
				{
					FileLocation = value;
				}
				DeliveryAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryAddressInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryAddress), GetHumanReadableNameOfDeliveryAddress()); }
		}

		string GetHumanReadableNameOfDeliveryAddress()
		{
			if (DeliveryMethod == Constants.ContactNotifyModes.Email)
			{
				return (NoResString)"E-Mail Address";
			}

			if (DeliveryMethod == Constants.ContactNotifyModes.Fax)
			{
				return (NoResString)"Fax Address";
			}

			return (NoResString)"Delivery Address";
		}

		protected bool DeliveryAddress_ReadOnly
		{
			get { return DeliveryMethod != Core.Constants.ContactNotifyModes.Email && DeliveryMethod != Core.Constants.ContactNotifyModes.Fax; }
		}

		protected int DeliveryAddress_MaxLength
			=> DeliveryMethod == Core.Constants.ContactNotifyModes.Email ? EmailInfo.MaxLength : FaxInfo.MaxLength;

		public FieldType DeliveryMethodFieldType => DeliveryMethod == Core.Constants.ContactNotifyModes.Email ? FieldType.TextCodeFindBox : FieldType.Text;

		public bool IsDeliveryAddressDifferentFromContact
		{
			get
			{
				bool result = false;
				OrgContact currentContact = Contact;

				if (currentContact != null)
				{
					if (DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
					{
						result = !DeliveryAddress.EqualsIgnoringCase(currentContact.OC_Email);
					}
					else if (DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
					{
						result = FormatPhoneNumber(DeliveryAddress, true) != FormatPhoneNumber(currentContact.OC_Fax, true);
					}
				}

				return result;
			}
		}

		#endregion

		#region Language

		[MaxLength(60)]
		public ZString Language
		{
			get { return language; }
			set
			{
				if (language != value)
				{
					CheckMaximumLength(LanguageInfo, value);
					SetNonPersistentPropertyValue(LanguageInfo, ref language, value);
				}
			}
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(nameof(Language)); }
		}
		ZString language;

		#endregion

		#region Fax

		[MaxLength(254)]
		public ZString Fax
		{
			get { return fax; }
			set
			{
				if (fax != value)
				{
					CheckMaximumLength(FaxInfo, value);
					SetNonPersistentPropertyValue(FaxInfo, ref fax, FormatPhoneNumber(value));
					DeliveryAddressInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateDeliveryAddress();
					}
				}
			}
		}

		public ZPropertyInfo FaxInfo
		{
			get { return GetZPropertyInfo(nameof(Fax)); }
		}

		ZString fax;

		#endregion

		#region Phone Number Formatting

		ZString FormatPhoneNumber(ZString value)
		{
			return FormatPhoneNumber(value, Env.Registry.OrgUsePhoneNumberFormatting);
		}

		ZString FormatPhoneNumber(ZString value, bool shouldFormat)
		{
			var result = value;
			if (shouldFormat)
			{
				var formattedNumber = PhoneNumberFormatterAndValidator.FormatInternational(value, ClosestPort);
				if (!string.IsNullOrEmpty(formattedNumber))
				{
					result = formattedNumber;
				}
			}
			return result;
		}

		internal PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());

		#endregion

		#region Email Copy Recipient

		#region Email To Recipient

		[ChildEditable(true)]
		public NonPersistentCopyRecipientCollection EmailToRecipients
		{
			get
			{
				if (emailToRecipients == null)
				{
					emailToRecipients = new NonPersistentCopyRecipientCollection(OrgHeader, Constants.CopyRecipientType.EmailToRecipient);
					RegisterEditableChildObject(emailToRecipients);
					emailToRecipients.Updated += EmailToRecipients_OnUpdated;
				}
				return emailToRecipients;
			}
		}

		[BusinessObjectTestExclude]
		public ZString Email
		{
			get { return email ?? (email = EmailToRecipients.Value); }
			set
			{
				if (email != value)
				{
					isUpdatingEmail = true;
					try
					{
						CheckMaximumLength(EmailInfo, value);
						email = value;
						EmailToRecipients.Value = value;
						Validation.ValidateEmailCarbonCopyRecipientsAsString();
						Validation.ValidateDeliveryAddress();
						if (!EmailInfo.HasErrors())
						{
							email = emailToRecipients.Value;
						}
						EmailInfo.RefreshBinding();
						DeliveryAddressInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingEmail = false;
					}
				}
			}
		}

		public ZPropertyInfo EmailInfo
		{
			get { return GetZPropertyInfo(nameof(Email)); }
		}

		internal bool Email_ReadOnly
		{
			get { return DeliveryMethod != Constants.ContactNotifyModes.Email; }
		}

		void EmailToRecipients_OnUpdated(object sender, EventArgs eventArgs)
		{
			if (!isUpdatingEmail)
			{
				email = null;

				if (DeliveryAddressInfo.HasErrors() || Email.IsEmpty)
				{
					Validation.ValidateDeliveryAddress();
				}
			}
			EmailInfo.RefreshBinding();
		}

		NonPersistentCopyRecipientCollection emailToRecipients;
		string email;
		bool isUpdatingEmail;

		#endregion

		#region Email Sender

		public ZString EmailFromAddress
		{
			get => emailFromAddress.IsEmpty ? DefaultEmailFromAddress : emailFromAddress;

			set
			{
				if (value != emailFromAddress)
				{
					SetNonPersistentPropertyValue(EmailFromAddressInfo, ref emailFromAddress, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateEmailFromAddress();
					}
				}
			}
		}

		public ZPropertyInfo EmailFromAddressInfo => GetZPropertyInfo(nameof(EmailFromAddress));

		ZString emailFromAddress;

		[BusinessObjectTestExclude]
		[List("EmailFromAddressWithTypeList")]
		public ZString EmailFromAddressWithType
		{
			get
			{
				if (DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					if (!EmailFromAddress.IsEmpty)
					{
						var result = EmailFromAddressWithTypeList.GetCodeFromDescription(EmailFromAddress);

						return result ?? EmailFromAddress;
					}
				}

				return ZString.Empty;
			}

			set
			{
				EmailFromAddress = EmailFromAddressWithTypeList.GetDescriptionFromCode(value) ?? value;

				EmailFromAddressWithTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EmailFromAddressWithTypeInfo => GetWrappedZPropertyInfo(nameof(EmailFromAddressWithType), x => EmailFromAddressInfo);

		internal bool EmailFromAddressWithType_ReadOnly => DeliveryMethod != Constants.ContactNotifyModes.Email;

		public CodeDescriptionPairList EmailFromAddressWithTypeList
		{
			get
			{
				if (emailFromAddressList == null)
				{
					emailFromAddressList = new CodeDescriptionPairList();

					var mainEmailAddressString = ZString.Empty;

					foreach (var emailAddress in GlbStaff.CurrentUser.EmailAddresses.ToArray())
					{
						if (!emailAddress.GSE_EmailAddress.IsEmpty)
						{
							if (emailAddress.GSE_Type == Constants.EmailFromAddressTypes.Codes.Main)
							{
								mainEmailAddressString = emailAddress.GSE_EmailAddress;
							}
							else
							{
								emailFromAddressList.AddPair(JoinEmailTypeWithEmailAddress(emailAddress.EmailType, emailAddress.GSE_EmailAddress), emailAddress.GSE_EmailAddress);
							}
						}
					}

					emailFromAddressList.Sort();

					if (!DefaultEmailFromAddress.IsEmpty)
					{
						ZString defaultEmailType = default;

						if (DefaultEmailFromAddress == mainEmailAddressString)
						{
							defaultEmailType = Constants.EmailFromAddressTypes.Descriptions.Main;
						}
						else
						{
							var staffEmailAddress = GlbStaff.CurrentUser.EmailAddresses.FindByEmailAddressString(DefaultEmailFromAddress);

							if (staffEmailAddress != null)
							{
								defaultEmailType = staffEmailAddress.EmailType;
								emailFromAddressList.RemoveCode(JoinEmailTypeWithEmailAddress(staffEmailAddress.EmailType, DefaultEmailFromAddress));
							}

							if (!mainEmailAddressString.IsEmpty)
							{
								InsertEmailAddressAtBeginning(emailFromAddressList, Constants.EmailFromAddressTypes.Descriptions.Main, mainEmailAddressString);
							}
						}

						if (!defaultEmailType.IsEmpty)
						{
							defaultEmailType = " [" + defaultEmailType + "]";
						}
						InsertEmailAddressAtBeginning(emailFromAddressList, FormattableString.Invariant($"{Constants.EmailFromAddressTypes.Descriptions.Default}{defaultEmailType}"), DefaultEmailFromAddress);

						return emailFromAddressList;
					}

					if (!mainEmailAddressString.IsEmpty)
					{
						InsertEmailAddressAtBeginning(emailFromAddressList, Constants.EmailFromAddressTypes.Descriptions.Main, mainEmailAddressString);
					}
				}

				return emailFromAddressList;
			}
		}

		CodeDescriptionPairList emailFromAddressList;

		string JoinEmailTypeWithEmailAddress(string emailType, string emailAddress)
		{
			return FormattableString.Invariant($"{emailType} - {emailAddress}");
		}

		internal void ClearEmailFromAddressList()
		{
			emailFromAddressList = null;
		}

		void InsertEmailAddressAtBeginning(IList list, string emailType, string emailAddress)
		{
			list?.Insert(0, new CodeDescriptionPair(JoinEmailTypeWithEmailAddress(emailType, emailAddress), emailAddress));
		}

		public ZString DefaultEmailFromAddress { get; set; }

		#endregion

		#region Carbon Copy Recipient

		[ChildEditable(true)]
		public NonPersistentCopyRecipientCollection EmailCarbonCopyRecipients
		{
			get
			{
				if (emailCarbonCopyRecipients == null)
				{
					emailCarbonCopyRecipients = new NonPersistentCopyRecipientCollection(OrgHeader, Constants.CopyRecipientType.CarbonCopyRecipient);
					RegisterEditableChildObject(emailCarbonCopyRecipients);
					emailCarbonCopyRecipients.Updated += EmailCarbonCopyRecipients_OnUpdated;
				}
				return emailCarbonCopyRecipients;
			}
		}

		[BusinessObjectTestExclude]
		[List("CopyRecipientList")]
		public ZString EmailCarbonCopyRecipientsAsString
		{
			get { return emailCarbonCopyRecipientsAsString ?? (emailCarbonCopyRecipientsAsString = EmailCarbonCopyRecipients.Value); }
			set
			{
				if (emailCarbonCopyRecipientsAsString != value)
				{
					isUpdatingCarbonCopyRecipientsAsString = true;
					try
					{
						CheckMaximumLength(EmailCarbonCopyRecipientsAsStringInfo, value);
						emailCarbonCopyRecipientsAsString = value;
						EmailCarbonCopyRecipients.Value = value;
						Validation.ValidateEmailCarbonCopyRecipientsAsString();
						if (!EmailCarbonCopyRecipientsAsStringInfo.HasErrors())
						{
							emailCarbonCopyRecipientsAsString = emailCarbonCopyRecipients.Value;
						}
						EmailCarbonCopyRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingCarbonCopyRecipientsAsString = false;
					}
				}
			}
		}

		public ZPropertyInfo EmailCarbonCopyRecipientsAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(EmailCarbonCopyRecipientsAsString)); }
		}

		internal bool EmailCarbonCopyRecipientsAsString_ReadOnly
		{
			get { return DeliveryMethod != Constants.ContactNotifyModes.Email; }
		}

		void EmailCarbonCopyRecipients_OnUpdated(object sender, EventArgs eventArgs)
		{
			if (!isUpdatingCarbonCopyRecipientsAsString)
			{
				emailCarbonCopyRecipientsAsString = null;

				if (EmailCarbonCopyRecipientsAsStringInfo.HasErrors() || EmailCarbonCopyRecipientsAsString.IsEmpty)
				{
					Validation.ValidateEmailCarbonCopyRecipientsAsString();
				}
			}
			EmailCarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		NonPersistentCopyRecipientCollection emailCarbonCopyRecipients;
		string emailCarbonCopyRecipientsAsString;
		bool isUpdatingCarbonCopyRecipientsAsString;

		#endregion

		#region Blind Carbon Copy Recipient

		[ChildEditable(true)]
		public NonPersistentCopyRecipientCollection EmailBlindCarbonCopyRecipients
		{
			get
			{
				if (emailBlindCarbonCopyRecipients == null)
				{
					emailBlindCarbonCopyRecipients = new NonPersistentCopyRecipientCollection(OrgHeader, Constants.CopyRecipientType.BlindCarbonCopyRecipient);
					RegisterEditableChildObject(emailBlindCarbonCopyRecipients);
					emailBlindCarbonCopyRecipients.Updated += EmailBlindCarbonCopyRecipients_OnUpdated;
				}
				return emailBlindCarbonCopyRecipients;
			}
		}

		[BusinessObjectTestExclude]
		[List("CopyRecipientList")]
		public ZString EmailBlindCarbonCopyRecipientsAsString
		{
			get { return emailBlindCarbonCopyRecipientsAsString ?? (emailBlindCarbonCopyRecipientsAsString = EmailBlindCarbonCopyRecipients.Value); }
			set
			{
				if (emailBlindCarbonCopyRecipientsAsString != value)
				{
					isUpdatingBlindCarbonCopyRecipientsAsString = true;
					try
					{
						CheckMaximumLength(EmailBlindCarbonCopyRecipientsAsStringInfo, value);
						emailBlindCarbonCopyRecipientsAsString = value;
						EmailBlindCarbonCopyRecipients.Value = value;
						Validation.ValidateEmailBlindCarbonCopyRecipientsAsString();
						if (!EmailBlindCarbonCopyRecipientsAsStringInfo.HasErrors())
						{
							emailBlindCarbonCopyRecipientsAsString = emailBlindCarbonCopyRecipients.Value;
						}
						EmailBlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingBlindCarbonCopyRecipientsAsString = false;
					}
				}
			}
		}

		public ZPropertyInfo EmailBlindCarbonCopyRecipientsAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(EmailBlindCarbonCopyRecipientsAsString)); }
		}

		internal bool EmailBlindCarbonCopyRecipientsAsString_ReadOnly
		{
			get { return DeliveryMethod != Constants.ContactNotifyModes.Email; }
		}

		void EmailBlindCarbonCopyRecipients_OnUpdated(object sender, EventArgs eventArgs)
		{
			if (!isUpdatingBlindCarbonCopyRecipientsAsString)
			{
				emailBlindCarbonCopyRecipientsAsString = null;

				if (EmailBlindCarbonCopyRecipientsAsStringInfo.HasErrors() || EmailBlindCarbonCopyRecipientsAsString.IsEmpty)
				{
					Validation.ValidateEmailBlindCarbonCopyRecipientsAsString();
				}
			}
			EmailBlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		NonPersistentCopyRecipientCollection emailBlindCarbonCopyRecipients;
		string emailBlindCarbonCopyRecipientsAsString;
		bool isUpdatingBlindCarbonCopyRecipientsAsString;

		#endregion

		public List<string> CopyRecipientList
		{
			get { return new List<string>(); }
		}

		#endregion

		#region EPrintEmail

		[MaxLength(128)]
		public ZString EPrintEmail
		{
			get { return DocumentsDataRegistry.Instance.EPrintEmailAddress.Value; }
		}

		public ZPropertyInfo EPrintEmailInfo
		{
			get { return GetZPropertyInfo(nameof(EPrintEmail)); }
		}

		#endregion

		#region Ftp

		[MaxLength(256)]
		public ZString FileLocation
		{
			get { return fileLocation; }
			set
			{
				if (fileLocation != value)
				{
					CheckMaximumLength(FileLocationInfo, value);
					SetNonPersistentPropertyValue(FileLocationInfo, ref fileLocation, value);
					DeliveryAddressInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateDeliveryAddress();
					}
				}
			}
		}
		ZString fileLocation;

		public ZPropertyInfo FileLocationInfo
		{
			get { return GetZPropertyInfo(nameof(FileLocation)); }
		}

		[MaxLength(50)]
		public ZString UserName
		{
			get { return userName; }
			set
			{
				if (userName != value)
				{
					CheckMaximumLength(UserNameInfo, value);
					SetNonPersistentPropertyValue(UserNameInfo, ref userName, value);
				}
			}
		}
		ZString userName;

		public ZPropertyInfo UserNameInfo
		{
			get { return GetZPropertyInfo(nameof(UserName)); }
		}

		[MaxLength(50)]
		public ZString Password
		{
			get { return password; }
			set
			{
				if (password != value)
				{
					CheckMaximumLength(PasswordInfo, value);
					SetNonPersistentPropertyValue(PasswordInfo, ref password, value);
				}
			}
		}
		ZString password;

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(nameof(Password)); }
		}

		#endregion

		public EmptyReportContingency EmptyReportContingency;

		#region UNLOCO

		public RefUNLOCO UNLOCO
		{
			get { return fUNLOCO; }
			set { fUNLOCO = value; }
		}

		RefUNLOCO fUNLOCO;

		#endregion

		#region ClosestPort

		public RefUNLOCO ClosestPort
		{
			get { return OrgHeader != null ? OrgHeader.ClosestPort : null; }
		}

		#endregion

		public RefCountry MostRelevantPostalCountry => OrgAddress?.RelatedCountry ?? Contact?.OrganisationOrAddressOverride?.MainAddress.RelatedCountry;

		#endregion

		#region Validation

		protected virtual DocDeliveryContactValidation GetNewValidation()
		{
			return new DocDeliveryContactValidation(this);
		}

		public DocDeliveryContactValidation Validation
		{
			get { return GetNewValidation(); }
		}

		#endregion

		#region Lookups

		#region staff

		public GlbStaffCollection StaffRecipients
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#region Organisations

		public OrganisationsFindBoxCollection Organisations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region Contacts

		public OrgContactCollectionForDelivery Contacts
		{
			get
			{
				OrgContactDependentCollection contactCollection;
				if (OrgHeader == null)
				{
					contactCollection = new OrgContactDependentCollection(Factory);
				}
				else
				{
					var filter = new ZQuery(OrgContactSchema.OC_IsActive, true);
					contactCollection = new OrgContactDependentCollection(OrgHeader, new ZQuery(filter));
					contactCollection.Load();
				}
				return new OrgContactCollectionForDelivery(contactCollection);
			}
		}

		#endregion

		#region Notify Mode

		public CodeDescriptionPairList NotifyModes
		{
			get => overridenNotifyModes ?? DefaultNotifyModes;
			set => overridenNotifyModes = value;
		}

		CodeDescriptionPairList overridenNotifyModes;

		CodeDescriptionPairList DefaultNotifyModes
		{
			get
			{
				if (defaultNotifyModes == null)
				{
					defaultNotifyModes = new CodeDescriptionPairList();

					var deliveringDocument = MenuItem != null && !MenuItem.SU_BusinessContext.StartsWith(Constants.BusinessContextPrefixes.Reports, StringComparison.OrdinalIgnoreCase);

					foreach (CodeDescriptionPair pair in SystemNotifyModes)
					{
						if (!deliveringDocument && pair.Code == Constants.ContactNotifyModes.EDoc)
						{
							continue;
						}

						defaultNotifyModes.AddPair(pair.Description, pair.Description);
					}
				}

				return defaultNotifyModes;
			}
		}

		CodeDescriptionPairList defaultNotifyModes;

		public CodeDescriptionPairList SystemNotifyModes
		{
			get { return systemNotifyModes ?? (systemNotifyModes = GetNotifyModes()); }
		}

		CodeDescriptionPairList systemNotifyModes;

		protected virtual CodeDescriptionPairList GetNotifyModes()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.NotifyMode);
			result.AddPair(Constants.ContactNotifyModes.EDoc, ResString.GetMultilingualString("aef750a9-0067-4ead-a28b-f2f606efd199", "eDoc"));
			return result;
		}

		#endregion

		#region Delivery Recipient Types

		public CodeDescriptionPairList DeliveryRecipientTypes => deliveryRecipientTypes ?? (deliveryRecipientTypes = GetNewDeliveryRecipientTypesList());
		CodeDescriptionPairList deliveryRecipientTypes;

		protected virtual CodeDescriptionPairList GetNewDeliveryRecipientTypesList()
		{
			var deliveryRecipientTypes = new CodeDescriptionPairList();
			deliveryRecipientTypes.AddPair(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, ResString.GetMultilingualString("F7D36BA2-5416-461A-8E84-1132E328596A", "Contact"));
			deliveryRecipientTypes.AddPair(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, ResString.GetMultilingualString("37FB25DF-1167-497B-BF65-8F7A9C8CE714", "Staff"));
			return deliveryRecipientTypes;
		}

		#endregion

		#region Attachment Types

		public ICodeDescriptionPairList AttachmentTypes
		{
			get
			{
				if (overriddenAttachmentTypes != null)
				{
					return overriddenAttachmentTypes;
				}

				return MenuItem != null ? MenuItem.AttachmentTypes : GetDefaultAttachmentTypes();
			}
			set { overriddenAttachmentTypes = value; }
		}

		static CodeDescriptionPairList GetDefaultAttachmentTypes()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Descriptions.Xls);
			result.AddPair(AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Descriptions.Xlsx);
			result.AddPair(AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Descriptions.Pdf);
			result.AddPair(AttachmentTypeList.Codes.Pdfa, AttachmentTypeList.Descriptions.Pdfa);
			result.AddPair(AttachmentTypeList.Codes.Pdfc, AttachmentTypeList.Descriptions.Pdfc);
			result.AddPair(AttachmentTypeList.Codes.Tif, AttachmentTypeList.Descriptions.Tif);
			result.AddPair(AttachmentTypeList.Codes.Html, AttachmentTypeList.Descriptions.Html);
			result.AddPair(AttachmentTypeList.Codes.Htmf, AttachmentTypeList.Descriptions.Htmf);

			return result;
		}

		ICodeDescriptionPairList overriddenAttachmentTypes;

		#endregion

		#endregion
	}
}
