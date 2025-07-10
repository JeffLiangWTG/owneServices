using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Core.Environment;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(OrgContact.Schema.OC_ContactName), DescriptionProperty(OrgContact.Schema.OC_Title)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgContact : AutoOrgContact
	, IPasswordEmailSource
	, IPasswordInstructionEmailSource
	, IExamUrlRecipient
	, IOrgContact
	, ICertificatesProvider
	, IAddressBookRecipient
	, IConversationParticipant
	, IPatternCleanup
	, IGlbPersonPrimarySource
	, IPasswordStored
	, ISupportDataImporting
	, IGlbPasswordHistoryParent
	, IExcelPasswordRetrieverForDocumentDelivery
	{
		public OrgContact(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			OC_OA_OrgAddress_ZAddress.DefaultAddressType = AddressType.NoDefault;
			phoneNumberPropertyHelperThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers));
		}

		#region Schema

		public new abstract class Schema : AutoOrgContact.Schema
		{
			public const string Location = "Location";
			public const string OC_Fax_Formatted = "OC_Fax_Formatted";
			public const string OC_Fax_IsManuallyVerified = "OC_Fax_IsManuallyVerified";
			public const string OC_HomePhone_Formatted = "OC_HomePhone_Formatted";
			public const string OC_HomePhone_IsManuallyVerified = "OC_HomePhone_IsManuallyVerified";
			public const string OC_Mobile_Formatted = "OC_Mobile_Formatted";
			public const string OC_Mobile_IsManuallyVerified = "OC_Mobile_IsManuallyVerified";
			public const string OC_OtherPhone_Formatted = "OC_OtherPhone_Formatted";
			public const string OC_OtherPhone_IsManuallyVerified = "OC_OtherPhone_IsManuallyVerified";
			public const string OC_Pager_Formatted = "OC_Pager_Formatted";
			public const string OC_Pager_IsManuallyVerified = "OC_Pager_IsManuallyVerified";
			public const string OC_Phone_Formatted = "OC_Phone_Formatted";
			public const string OC_Phone_IsManuallyVerified = "OC_Phone_IsManuallyVerified";
			public const string WorkingAddressPK = "WorkingAddressPK";
		}

		#endregion

		public ZString UniqueCode => ParentOrg != null ? ZString.Format("{0} ({1})", OC_ContactName, ParentOrg.OH_Code) : OC_ContactName;

		#region Logging

		protected override ZString CustomLogReferenceSuffix
		{
			get { return (NoResString)"Contact " + OC_ContactName; }
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList objects = new ArrayList();
				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.AddRange(Documents);
				objects.AddRange(Attributes);
				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgContactFetchStrategy(this);
		}

		internal void AddFetchHintsForBusinessObjectsWithRelatedEvents()
		{
			Factory.AddFetchHint(OrgDocumentSchema.OD_OC, PK);
			Factory.AddFetchHint(OrgContactAttributeSchema.PC_OC, PK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		public const string DetailsVerifiedLogReference = " details verified - ";

		public const int PasswordMaxLength = 1024;

		protected override void OnFactorySaving()
		{
			ExcelPasswordSupporter.DeleteStmDataIfExcelPasswordIsEmpty();
			switch (supersedeAction)
			{
				case SupersedeWebAccessAction.Supersede:
					Logs.AddNew(AutoEvents.WebAccessSuperseded, ZDateTimeOffset.Now);
					supersedeAction = SupersedeWebAccessAction.None;
					validSupersededLogs = null;
					break;
				case SupersedeWebAccessAction.Cancel:
					ValidSupersededLogs?.ForEach(x => x.Cancel());
					supersedeAction = SupersedeWebAccessAction.None;
					validSupersededLogs = null;
					break;
			}
		}

		public override void OnSaving()
		{
			//If you make any changes to this method, you may need to consider whether that needs to be called when changes are made in GLOW.
			//If so, equivalent calls would need to be made in ContactController.SyncPersons
			if ((!IsInDatabase || OC_DetailsVerifiedInfo.HasChanges) && !OC_DetailsVerified.IsEmpty && OC_DetailsVerified.IsValid)
			{
				Logs.AddNew(Events.RecordAudited, OC_ContactName + DetailsVerifiedLogReference + OC_DetailsVerified.ToShortDateString());
			}

			UpdatePerson();

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
			//^This logic is not supported in the GLOW OrgContact creation/editing

			if (IsPasswordBeingCleared())
			{
				Logs.AddNew(AutoEvents.WebAccessPasswordChanged, $"Password was Reset, user = {Env.CurrentUser.PK}", ZDateTimeOffset.Now);
			}

			base.OnSaving();
		}

		bool IsPasswordBeingCleared() => !OC_PasswordHashInfo.OriginalValue.IsEmpty && OC_PasswordHash.IsEmpty;

		public void UpdatePerson(bool forceUpdate = false)
		{
			var person = Person;
			if (person == null)
			{
				GlbPerson.CreateFromContact(Factory, this);
			}
			else
			{
				if (HasChanges || forceUpdate)
				{
					if (person.IsInDatabase && !person.HasChanges)
					{
						person.Reload();
					}

					person.UpdateFromContact(this, forceUpdate);
				}
			}
		}

		public void UpdateFromPerson(GlbPerson person)
		{
			var personNameWithoutNumberSuffix = OrgContactUniqueNameHelper.GetNameWithoutNumberSuffix(person.PER_FullNameInternal);
			if (!personNameWithoutNumberSuffix.Equals(GlbPerson.EmptyFullName) && !personNameWithoutNumberSuffix.EqualsIgnoringCase(ContactNameWithoutNumberSuffix))
			{
				OC_ContactName = OrgContactUniqueNameHelper.GenerateUniqueContactName(this, personNameWithoutNumberSuffix);
				IsContactNameChangedByGlbPersonSynchronisation = true;
			}

			var gendersToIgnore = new[] { Core.Constants.Genders.Custom, Core.Constants.Genders.Agender, Core.Constants.Genders.NonBinary };
			OC_Gender = gendersToIgnore.Any(s => s.Contains(person.PER_GenderInternal)) ? (ZString)Core.Constants.Genders.NotSpecified : person.PER_GenderInternal;
			OC_HomePhone = person.PER_HomePhoneInternal;
			OC_Birthday = person.PER_BirthDateInternal;
			OC_RN_NKNationality = person.PER_RN_NKNationalityCodeISOInternal;
			OC_PersonalInfo = person.PER_PersonalInfoInternal;

			if (person.ShouldUpdateMobileOnRelatedRecords && OC_Mobile == person.PER_MobilePhoneInfo.OriginalValue.ToString())
			{
				OC_Mobile = person.PER_MobilePhoneInternal;
			}
		}

		#endregion

		#region Loading / Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OC_NotifyMode = Constants.ContactNotifyModes.Email;
			OC_AttachmentType = OrganisationsDataRegistry.Instance.DefaultAttachmentType.Value;
			OC_Language = Constants.Languages.English;
			OC_JobCategory = OrgContactJobCategories.Codes.EMU;
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			OrgContact cloneContact = (OrgContact)base.CloneInternal(args);
			foreach (OrgDocument document in Documents)
			{
				cloneContact.Documents.Add(document.Clone());
			}
			return cloneContact;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted) // multiple objects around a row - Enterprise.MarketingManager.Business.CampaignOrgContact
			{
				Documents.RemoveAndDeleteAll();
				Campaigns.DeleteAll();
				Attributes.RemoveAndDeleteAll();
				Allocations.RemoveAndDeleteAll();
				EmailContactItems.RemoveAndDeleteAll();
				PhoneContactItems.RemoveAndDeleteAll();
				AddOnRuleAcks.DeleteAll();
				Subscriptions.RejectAllChanges();
				DeleteAllPatterns();
				DeletePrimaryRelationshipAndPersonIfRequiredAndCreateNewOne();
				DeleteAllGlbGroupOrgContactLinks();

				OrgSecurityContactsCollection securityRights = fSecurityRights;
				if (securityRights == null)
				{
					securityRights = new OrgSecurityContactsCollection(this);
					securityRights.Load(); // Done to avoid touching ReadOnly on the public collection
				}

				List<IDisposable> suspendedHasChanges = new List<IDisposable>();
				foreach (BusinessObject bizO in securityRights.ToArray())
				{
					suspendedHasChanges.Add(bizO.SuspendSettingHasChanges());
				}
				try
				{
					securityRights.RemoveAndDeleteAll();
					base.Delete();
				}
				finally
				{
					foreach (IDisposable hasChangesSuspender in suspendedHasChanges)
					{
						hasChangesSuspender.Dispose();
					}
				}
			}
			else
			{
				base.Delete();
			}
		}

		void DeletePrimaryRelationshipAndPersonIfRequiredAndCreateNewOne()
		{
			if (!IsPersonLoaded && OC_PER.IsValid)
			{
				ReloadPerson();
			}

			if (Person == null)
			{
				return;
			}

			if (Person.PrimarySource == this)
			{
				var staffQuery = new ZQuery(GlbStaffSchema.GS_PER, OC_PER);
				staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);

				var staff = Factory.LoadTop1<GlbStaff>(staffQuery);

				if (staff != null)
				{
					Person.SetPrimaryRelationship(staff);
					return;
				}

				var otherContactsQuery = new ZQuery(OrgContactSchema.OC_PER, OC_PER);
				otherContactsQuery.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, PK);
				otherContactsQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
				otherContactsQuery.OrderBy = OrgContactSchema.OC_SystemCreateTimeUtc.Name + OrderByClause.Descending;

				var otherContact = Factory.LoadTop1<OrgContact>(otherContactsQuery);

				if (otherContact != null)
				{
					Person.SetPrimaryRelationship(otherContact);
					return;
				}

				Person.PrimaryRelationship?.Delete();

				if (Factory.Load<IHRJobApplicant>(new ZQuery(HRJobApplicantSchema.HA_PER, OC_PER)).Length == 0)
				{
					Person.Delete();
				}
			}
		}

		void DeleteAllGlbGroupOrgContactLinks()
		{
			var orgContactLinkFilter = new ZQuery(GlbGroupOrgContactLinkSchema.GCK_OC_Contact, this.PK);
			var orgContactLinkList = Factory.Load<GlbGroupOrgContactLink>(orgContactLinkFilter);
			if (orgContactLinkList != null && orgContactLinkList.Length > 0)
			{
				foreach (var item in orgContactLinkList)
				{
					item.Delete();
				}
			}
		}

		public override bool CanDelete
		{
			get
			{
				return !IsInDatabase;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("703742aa-3c29-4006-9802-809652e9b796", "Existing contacts cannot be deleted. Mark them as inactive instead.");
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
				foreach (var contactItem in ContactItems)
				{
					PatternMatchingRemover.DeleteAll(contactItem);
				}
			}
		}

		#endregion

		#region Related Business Objects

		#region Attributes

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContactAttributeCollection Attributes
		{
			get
			{
				if (fAttributes == null)
				{
					fAttributes = new OrgContactAttributeCollection(this);
					fAttributes.Load();
					RegisterEditableChildObject(fAttributes);
					if (Header != null)
					{
						fAttributes.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyContactSecurity);
					}
				}
				return fAttributes;
			}
		}
		OrgContactAttributeCollection fAttributes;

		#endregion

		#region Allocations

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgContactAllocationCollection Allocations
		{
			get
			{
				if (fAllocations == null)
				{
					fAllocations = new OrgContactAllocationCollection(this);
					fAllocations.Load();
					RegisterEditableChildObject(fAllocations);
					if (Header != null)
					{
						fAllocations.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyContactSecurity);
					}
				}
				return fAllocations;
			}
		}
		OrgContactAllocationCollection fAllocations;

		#endregion

		#region Documents

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public virtual OrgDocumentCollection Documents
		{
			get
			{
				if (fDocuments == null)
				{
					fDocuments = new OrgDocumentCollection(this, Factory);
					fDocuments.Load();
					RegisterEditableChildObject(fDocuments);
					if (Header != null)
					{
						fDocuments.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyContactDocDeliveryDetailsSecurity);
					}
				}
				return fDocuments;
			}
		}
		OrgDocumentCollection fDocuments;

		public ZBool HasDocumentGroup(string documentGroupCode) => Documents.Find(new ZQuery(OrgDocumentSchema.OD_DocumentGroup, documentGroupCode)).Any();

		public void AddDocumentGroup(string documentGroupCode, bool value, ZPropertyInfo propertyInfo)
		{
			var documents = Documents.Find(new ZQuery(OrgDocumentSchema.OD_DocumentGroup, documentGroupCode));
			var shouldRefreshBinding = false;
			if (value)
			{
				if (!documents.Any())
				{
					var doc = Documents.AddNew();
					doc.OD_DocumentGroup = documentGroupCode;
					shouldRefreshBinding = true;
				}
			}
			else
			{
				foreach (var doc in documents)
				{
					Documents.RemoveAndDelete(doc);
					shouldRefreshBinding = true;
				}
			}

			if (shouldRefreshBinding)
			{
				propertyInfo?.RefreshBinding();
			}
		}

		#region IsCustomerServiceContact

		public ZBool IsCustomerServiceContact
		{
			get => HasDocumentGroup(ContactType.CustomerService.Code);
			set => AddDocumentGroup(ContactType.CustomerService.Code, value, IsCustomerServiceContactInfo);
		}

		public ZPropertyInfo IsCustomerServiceContactInfo
		{
			get { return GetZPropertyInfo(nameof(IsCustomerServiceContact)); }
		}

		#endregion

		#region IsCommissionAgreementRecipientContact

		public ZBool IsCommissionAgreementRecipientContact
		{
			get { return Documents.Find(new ZQuery(OrgDocumentSchema.OD_DocumentGroup, ContactType.CommissionAgreementRecipient.Code)).Length > 0; }
		}

		#endregion

		#endregion

		#region EffectiveContactAddress

		public OrgAddress EffectiveContactAddress
		{
			get
			{
				OrgAddress result = OrgAddress;
				if (result == null && AddressOverride != null)
				{
					result = AddressOverride.MainAddress;
				}
				if (result == null && ParentOrg != null)
				{
					result = ParentOrg.MainAddress;
				}
				return result;
			}
		}

		#endregion

		#region

		[List("Lookups.OrgAddresses")]
		public ZGuid WorkingAddressPK
		{
			get => AddressOverride == null ? WorkingAddress?.PK ?? ZGuid.Empty : OC_OA_OrgAddress;
			set => OC_OA_OrgAddress = value;
		}

		public ZWrappedPropertyInfo WorkingAddressPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.WorkingAddressPK, x => OC_OA_OrgAddressInfo); }
		}

		public ZAddress WorkingAddressPK_ZAddress
		{
			get
			{
				if (workingAddressPK_ZAddress == null)
				{
					workingAddressPK_ZAddress = new ZAddress(WorkingAddressPKInfo) { DefaultAddressType = AddressType.NoDefault };
				}

				return workingAddressPK_ZAddress;
			}
		}
		ZAddress workingAddressPK_ZAddress;

		#endregion

		#region SecurityRights

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgSecurityContactsCollection SecurityRightsForBindingOnly
		{
			get
			{
				if (fSecurityRights == null)
				{
					fSecurityRights = new OrgSecurityContactsCollection(this);
					fSecurityRights.Load();
					RegisterEditableChildObject(fSecurityRights);
					if (Header != null)
					{
						bool hasWebSecurity = Header.IsInDatabase ? Header.SecurityProvider.HasModifyDetailsWebSecurity : Header.SecurityProvider.HasNewDetailsWebSecurity;
						fSecurityRights.SetReadOnlyIncludingChildren(!hasWebSecurity);
					}
				}

				return fSecurityRights;
			}
		}
		OrgSecurityContactsCollection fSecurityRights;

		internal OrgSecurityContactsCollection SecurityRightsNoCreate
		{
			get { return fSecurityRights; }
		}

		OrgSecurityContactsCollection securityRightsNoDummies;
		public OrgSecurityContactsCollection SecurityRightsNoDummies
		{
			get
			{
				if (securityRightsNoDummies == null)
				{
					securityRightsNoDummies = new OrgSecurityContactsCollection(this);
					securityRightsNoDummies.DontLoadDummyContactRecords = true;
					securityRightsNoDummies.Load();
				}

				return securityRightsNoDummies;
			}
		}

		public OrgSecurityContactsCollectionView SecurityRightsView
		{
			get
			{
				if (securityRightsView == null)
				{
					securityRightsView = new OrgSecurityContactsCollectionView(SecurityRightsForBindingOnly);
				}
				return securityRightsView;
			}
		}
		OrgSecurityContactsCollectionView securityRightsView;

		public void RefreshSecurityRights()
		{
			if (fSecurityRights != null)
			{
				fSecurityRights.Refresh();
			}
		}

		#endregion

		#region WebWarehouseEligibility

		public OrgContactWebWarehouseEligibilityCollection WebWarehouseEligibility
		{
			get { return webWarehouseEligibility ?? (webWarehouseEligibility = new OrgContactWebWarehouseEligibilityCollection(Factory, this)); }
		}

		OrgContactWebWarehouseEligibilityCollection webWarehouseEligibility;

		#endregion

		#region Parent Organisation

		public OrgHeader OrganisationOrAddressOverride
		{
			get { return AddressOverride ?? ParentOrg; }
		}

		public OrgHeader ParentOrg
		{
			get { return Header; }
		}

		#endregion

		#region UNLOCO

		public RefUNLOCO OrgClosestPort
		{
			get
			{
				RefUNLOCO result = null;
				if (ParentOrg != null)
				{
					if (OC_OA_OrgAddress.IsEmpty)
					{
						result = ParentOrg.ClosestPort;
					}
					else if (BranchAddress != null)
					{
						if (!BranchAddress.OA_RL_NKRelatedPortCode.IsEmpty)
						{
							result = BranchAddress.RelatedPortCode;
						}
						else
						{
							result = BranchAddress.Header.ClosestPort;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region Branch Address

		public OrgAddress BranchAddress
		{
			get { return (OrgAddress)Factory.Load(typeof(OrgAddress), OC_OA_OrgAddress); }
		}

		#endregion

		#region Campaign Items

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

		#region Subscriptions

		public bool SubscriptionsLoadedAndNotEmpty => subscriptions?.Any() ?? false;

		[ChildEditable(true)]
		public IGlbCompanyCampaignSubscriptionForOrganisationContactCollection Subscriptions => subscriptions ?? (subscriptions = GetOrgSubscriptionCollection());

		IGlbCompanyCampaignSubscriptionForOrganisationContactCollection subscriptions;

		IGlbCompanyCampaignSubscriptionForOrganisationContactCollection GetOrgSubscriptionCollection()
		{
			var glbCompanyCampaignSubscriptionsForContact = ObjectFactory.Get<IGlbCompanyCampaignSubscriptionForOrganisationContactCollection>(nameof(IGlbCompanyCampaignSubscriptionForOrganisationContactCollection), this);
			RegisterEditableChildObject(glbCompanyCampaignSubscriptionsForContact);
			glbCompanyCampaignSubscriptionsForContact.SetCountedReadOnlyIncludingChildren(OC_Email.IsEmpty);
			glbCompanyCampaignSubscriptionsForContact.HasChangesChanged += SetNeedingValidationOnSubscriptionsChanged;
			return glbCompanyCampaignSubscriptionsForContact;
		}

		void SetNeedingValidationOnSubscriptionsChanged(object sender, HasChangesChangedEventArgs hasChangesChangedEventArgs)
		{
			if (!ShouldValidateOnSave)
			{
				return;
			}
			MarkAsNeedingValidation();
			ParentOrg?.MarkAsNeedingValidation();
		}

		#endregion

		#endregion

		#region Lookups

		#region OrganisationOverrides

		public OrgHeaderCollection OrganisationOverrides
		{
			get
			{
				if (fOrganisationOverrides == null)
				{
					fOrganisationOverrides = new OrgHeaderCollection(Factory);
				}
				return fOrganisationOverrides;
			}
		}

		OrgHeaderCollection fOrganisationOverrides;

		#endregion

		#region Notify Mode

		public CodeDescriptionPairList OC_NotifyMode_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.NotifyMode); }
		}

		public CodeDescriptionPairList NotifyModes
		{
			get
			{
				if (notifyModes == null)
				{
					notifyModes = new CodeDescriptionPairList();
					notifyModes.AddRange(OC_NotifyMode_List);
					notifyModes.AddPair(Constants.ContactNotifyModes.DoNotDeliver, Res.GetString("1c78b910-9e12-41f6-b30d-5e3da7d973cb", "Do Not Deliver"));
				}

				return notifyModes;
			}
		}

		CodeDescriptionPairList notifyModes;

		#endregion

		#region Attachment Types

		public CodeDescriptionPairList OC_AttachmentType_List
		{
			get { return OrgCodeLists.AttachmentType_List; }
		}

		#endregion

		#region Languages

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		#endregion

		#region Salutations

		public CodeDescriptionPairList Salutations
		{
			get
			{
				var list = SalutationHelper.GetSalutations(OC_Gender);
				CodeDescriptionPairList salutations = new CodeDescriptionPairList();
				foreach (var salutation in list)
				{
					var code = salutation.ToString(OC_Language);
					if (code.Contains(Core.Constants.SalutationMacros.Name) || code.Contains(Core.Constants.SalutationMacros.JobCategory))
					{
						code = code.Replace(Core.Constants.SalutationMacros.Name, OC_ContactName)
							.Replace(Core.Constants.SalutationMacros.JobCategory, OC_JobCategory)
							.Replace("  ", " ")
							.Replace(" ,", ",")
							.Trim();
					}
					var genderPrefix = OC_Gender == Constants.Genders.Man ? (NoResString)"[m] " : (OC_Gender == Constants.Genders.Woman ? (NoResString)"[f] " : "");
					if (!string.IsNullOrEmpty(genderPrefix))
					{
						code = code.Replace(genderPrefix, "");
					}

					if (!salutations.ContainsCode(code) && code.Length <= OC_SalutationInfo.MaxLength)
					{
						salutations.AddPair(code, salutation);
					}
				}
				return salutations;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region View Denied Message
		internal ZString ViewDeniedMessage => Res.GetString("cd292400-8e29-45b1-9fd5-135a6f21ed82", "** View Denied due to Security Access **");

		#endregion

		protected bool ModifyPersonalInformationAllowed => Header == null || !Header.IsInDatabase || Env.Security.OrgContactModifyPersonalInformation.IsAllowed;
		internal bool ViewPersonalInformationAllowed => Header == null || !Header.IsInDatabase || Env.Security.OrgContactViewPersonalInformation.IsAllowed;
		protected bool PersonalInformationIsReadOnly => !ViewPersonalInformationAllowed || !ModifyPersonalInformationAllowed;

		protected bool ModifyMobileAllowed => Header == null || !Header.IsInDatabase || Env.Security.OrgContactModifyMobileNumber.IsAllowed;
		protected bool ViewMobileAllowed => Header == null || !Header.IsInDatabase || Env.Security.OrgContactViewMobileNumber.IsAllowed;
		protected bool MobileIsReadOnly => !ViewMobileAllowed || !ModifyMobileAllowed;

		protected bool ModifyHomePhoneAllowed => Header == null || !Header.IsInDatabase || Env.Security.OrgContactModifyHomePhoneNumber.IsAllowed;
		protected bool ViewHomePhoneAllowed => Header == null || !Header.IsInDatabase || Env.Security.OrgContactViewHomePhoneNumber.IsAllowed;
		protected bool HomePhoneIsReadOnly => !ViewHomePhoneAllowed || !ModifyHomePhoneAllowed;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("2bef60c5-d34f-470b-b4f0-c6821d190ea2", "Contact");
				if (!IsDeleted && !OC_ContactName.IsEmpty)
				{
					result += " (" + OC_ContactName + ")";
				}

				return result;
			}
		}

		#region OC_AttachmentType

		[List("OC_AttachmentType_List")]
		public override ZString OC_AttachmentType
		{
			get { return base.OC_AttachmentType; }
			set { base.OC_AttachmentType = value; }
		}

		protected bool OC_AttachmentType_ReadOnly
		{
			get { return !DeliveryMethodHelper.IsEmailOrEPrint(OC_NotifyMode); }
		}

		#endregion

		#region OC_WebEnabled

		public override ZBool OC_WebAccessEnabled
		{
			get { return base.OC_WebAccessEnabled; }
			set
			{
				if (base.OC_WebAccessEnabled != value)
				{
					base.OC_WebAccessEnabled = value;

					if (!IsValidationSuspended)
					{
						EmailContactItems.OfType<EmailContactItem>().ForEach(e => e.Validation.ValidateOI_Address());
					}

					if (value)
					{
						if (fSecurityRights != null)
						{
							SecurityRightsForBindingOnly.InheritParentRights();
						}
					}
					else
					{
						SecurityRightsForBindingOnly.DenyAll();
					}
				}
			}
		}

		public bool OC_WebAccessEnabled_ReadOnly { get; set; }

		public bool IsValidForWebLogin => OC_WebAccessEnabled && OC_IsActive && HasPassword;

		#endregion

		#region OC_ContactName

		public override ZString OC_ContactName
		{
			get { return base.OC_ContactName; }
			set
			{
				if (base.OC_ContactName != value)
				{
					base.OC_ContactName = value;

					OrgContactNameChangeService.GetInstance(Factory).NotifyContactNameChanged(this);

					if (!IsValidationSuspended)
					{
						RunContactNameUniquenessValidationForOrg();
					}
					Header?.FindDuplicates();
				}
			}
		}

		public ZString ContactNameWithoutNumberSuffix => OrgContactUniqueNameHelper.GetNameWithoutNumberSuffix(OC_ContactName);

		#region Add Or Update DDR Log Info

		internal string referenceForDDR;

		ZDateTime eventTimeForDDR;

		public void AddOrUpdateDDRLogInfo(IDuplicationEventArgs e)
		{
			if (e != null && e.Results != null && e.TargetObjects != null && e.TargetObjects is IEnumerable<CargoWise.Glow.Model.Interfaces.IOrgContact> targetGlows)
			{
				var results = e.Results.OrderByDescending(u => u.Score).Select(score => targetGlows.FirstOrDefault(target => target.OC_PK == score.TargetPK)?.OC_ContactName + " (" + (score.Score * 100) + "%)");
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

		void RunContactNameUniquenessValidationForOrg()
		{
			if (Header != null)
			{
				foreach (OrgContact contact in Header.Contacts)
				{
					if (contact.PK != PK &&
						(
							contact.OC_ContactName.EqualsIgnoringCase(OC_ContactName)
							||
							contact.OC_ContactNameInfo.HasNotifications()
						))
					{
						contact.Validation.ValidateOC_ContactName();
					}
				}
			}
		}

		#endregion

		#region OC_ContactSource
		[List("Lookups.ContactSourceList")]
		public override ZString OC_ContactSource
		{
			get
			{
				return base.OC_ContactSource;
			}
			set
			{
				base.OC_ContactSource = value;
			}
		}

		#endregion

		#region OC_JobCategory

		class JobCategoryPairList : CodeDescriptionPairList
		{
			public JobCategoryPairList()
			{
				AddRange(OrgContactLookups.CreateJobCategoryList());
			}
		}

		public override ZString OC_JobCategory
		{
			get { return base.OC_JobCategory; }
			set
			{
				if (base.OC_JobCategory != value)
				{
					base.OC_JobCategory = value;
					triggerFindSuggestedJobCategories?.Invoke(this, new FindSuggestedJobCategoriesEventArgs() { ShouldOverwrite = false, UseCache = true });
				}
			}
		}

		[ActionField(CollectionType = typeof(JobCategoryPairList))]
		[ResourceStringData("OrgContact|JobCategory", Caption = "Job Category")]
		[List("Lookups.JobCategory_List")]
		public ZString JobCategoryDescription
		{
			get { return Lookups.JobCategory_List.GetDescriptionFromCode(OC_JobCategory); }
			set
			{
				OC_JobCategory = Lookups.JobCategory_List.GetCodeFromDescription(value) ?? value;
			}
		}

		public ZWrappedPropertyInfo JobCategoryDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OC_JobCategory, x => OC_JobCategoryInfo); }
		}

		#endregion

		#region OC_OH_AddressOverride
		[List("OrganisationOverrides")]
		public override ZGuid OC_OH_AddressOverride
		{
			get => base.OC_OH_AddressOverride;
			set
			{
				base.OC_OH_AddressOverride = value;
				SetOrganisationForAddressOverride();
			}
		}

		void SetOrganisationForAddressOverride()
		{
			using (GetValidationSuspender())
			{
				if (OC_OH_AddressOverride.IsValid)
				{
					WorkingAddressPK_ZAddress.OrgPK = OC_OH_AddressOverride;
				}
				else
				{
					WorkingAddressPK_ZAddress.OrgPK = OC_OH;
				}
			}
		}

		#endregion

		#region OC_Languages
		[List("Languages")]
		public override ZString OC_Language
		{
			get
			{
				return base.OC_Language;
			}
			set
			{
				if (Culture.LanguageCodeMapping.TryGetValue(value, out string newCode))
				{
					value = newCode;
				}
				base.OC_Language = value;
			}
		}
		#endregion

		#region OC_NotifyMode
		[List("NotifyModes")]
		public override ZString OC_NotifyMode
		{
			get { return base.OC_NotifyMode; }
			set
			{
				base.OC_NotifyMode = value;
				OC_AttachmentType = OC_AttachmentTypeInfo.ReadOnly ? "" : OrgConstants.AttachmentType.PDF;

				Validation.ValidateOC_Email();
				Validation.ValidateOC_Fax();
			}
		}

		#endregion

		#region OC_Email

		[EmailAddress]
		public override ZString OC_Email
		{
			get { return base.OC_Email; }
			set
			{
				if (base.OC_Email.EqualsIgnoringCase(value))
				{
					return;
				}

				base.OC_Email = value;
				if (OC_EmailInfo.HasErrors())
				{
					return;
				}

				if (subscriptions != null)
				{
					subscriptions.HasChangesChanged -= SetNeedingValidationOnSubscriptionsChanged;
				}
				subscriptions = null;
			}
		}

		#endregion

		#region OC_Birthday

		[ReadOnlyMember(nameof(PersonalInformationIsReadOnly))]
		public override ZDateTime OC_Birthday
		{
			get => ViewPersonalInformationAllowed ? base.OC_Birthday : ZDateTime.Empty;
			set
			{
				if (base.OC_Birthday != value)
				{
					base.OC_Birthday = value;
					OC_BirthdayInternalHasChanges = true;
				}
			}
		}

		public ZDateTime InsecureBirthday
		{
			get => base.OC_Birthday;
		}

		#endregion

		#region OC_Gender

		[List("Lookups.Gender")]
		[ReadOnlyMember(nameof(PersonalInformationIsReadOnly))]
		public override ZString OC_Gender
		{
			get => ViewPersonalInformationAllowed ? base.OC_Gender : ViewDeniedMessage;
			set
			{
				if (base.OC_Gender != value)
				{
					base.OC_Gender = value;
					OC_GenderInternalHasChanges = true;
				}
			}
		}

		#endregion

		#region OC_RN_NKNationality

		[List("Lookups.NationalityTypes")]
		[ReadOnlyMember(nameof(PersonalInformationIsReadOnly))]
		public override ZString OC_RN_NKNationality
		{
			get => ViewPersonalInformationAllowed ? base.OC_RN_NKNationality : ViewDeniedMessage;
			set
			{
				if (base.OC_RN_NKNationality != value)
				{
					base.OC_RN_NKNationality = value;
					OC_RN_NKNationalityInternalHasChanges = true;
				}
			}
		}

		#endregion

		#region OC_OH

		public override ZGuid OC_OH
		{
			get { return base.OC_OH; }
			set
			{
				base.OC_OH = value;
				Documents.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region OC_PersonalInfo

		[ReadOnlyMember(nameof(PersonalInformationIsReadOnly))]
		public override ZString OC_PersonalInfo
		{
			get => ViewPersonalInformationAllowed ? base.OC_PersonalInfo : ViewDeniedMessage;
			set
			{
				if (base.OC_PersonalInfo != value)
				{
					base.OC_PersonalInfo = value;
					OC_PersonalInfoInternalHasChanges = true;
				}
			}
		}

		#endregion

		#region OC_Salutation

		[List("Salutations")]
		[BusinessObjectTestExclude]
		public override ZString OC_Salutation
		{
			get
			{
				return base.OC_Salutation;
			}
			set
			{
				base.OC_Salutation = value.Left(OC_SalutationInfo.MaxLength);
			}
		}

		#endregion

		#region OC_Title
		public override ZString OC_Title
		{
			get => base.OC_Title;
			set
			{
				if (base.OC_Title != value)
				{
					base.OC_Title = value;
					triggerFindSuggestedJobCategories?.Invoke(this, new FindSuggestedJobCategoriesEventArgs() { ShouldOverwrite = !IsInDatabase || string.IsNullOrEmpty(OC_JobCategory), UseCache = false });
				}
			}
		}

		EventHandler triggerFindSuggestedJobCategories;

		public event EventHandler TriggerFindSuggestedJobCategories
		{
			add
			{
				if (triggerFindSuggestedJobCategories == null || triggerFindSuggestedJobCategories.GetInvocationList().Length == 0)
				{
					triggerFindSuggestedJobCategories += value;
				}
			}
			remove
			{
				triggerFindSuggestedJobCategories -= value;
			}
		}
		#endregion

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

		#region Phone numbers

		#region OC_Phone

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString OC_Phone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(OC_PhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(OC_PhoneInfo, OC_Phone_FormattedInfo, value, Validation.ValidateOC_Phone_Formatted, OC_Phone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo OC_Phone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_Phone_Formatted); }
		}

		public ZString PhoneFallbackToOrganisation
		{
			get { return OC_Phone.IsEmpty && ParentOrg != null ? ParentOrg.MainAddress.OA_Phone : OC_Phone; }
		}

		public ZPropertyInfo PhoneFallbackToOrganisationInfo
		{
			get { return GetZPropertyInfo(nameof(PhoneFallbackToOrganisation)); }
		}

		public ZBool OC_Phone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgContactSchema.Constants.OC_Phone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OC_Phone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgContactSchema.Constants.OC_Phone, Validation.ValidateOC_Phone_Formatted, null); }
		}

		public ZPropertyInfo OC_Phone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_Phone_IsManuallyVerified); }
		}

		#endregion

		#region OC_Fax

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString OC_Fax_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(OC_FaxInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(OC_FaxInfo, OC_Fax_FormattedInfo, value, Validation.ValidateOC_Fax_Formatted, OC_Fax_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo OC_Fax_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_Fax_Formatted); }
		}

		public ZString FaxFallbackToOrganisation
		{
			get { return OC_Fax.IsEmpty && ParentOrg != null ? ParentOrg.MainAddress.OA_Fax : OC_Fax; }
		}

		public ZPropertyInfo FaxFallbackToOrganisationInfo
		{
			get { return GetZPropertyInfo(nameof(FaxFallbackToOrganisation)); }
		}

		public ZBool OC_Fax_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgContactSchema.Constants.OC_Fax, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OC_Fax_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgContactSchema.Constants.OC_Fax, Validation.ValidateOC_Fax_Formatted, null); }
		}

		public ZPropertyInfo OC_Fax_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_Fax_IsManuallyVerified); }
		}

		#endregion

		#region OC_Mobile

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(MobileIsReadOnly))]
		public ZString OC_Mobile_Formatted
		{
			get => ViewMobileAllowed ? PhoneNumberPropertyHelper.GetPhoneNumber(OC_MobileInfo) : ViewDeniedMessage;
			set => PhoneNumberPropertyHelper.SetPhoneNumber(OC_MobileInfo, OC_Mobile_FormattedInfo, value, Validation.ValidateOC_Mobile_Formatted, OC_Mobile_IsManuallyVerifiedInfo);
		}

		public ZPropertyInfo OC_Mobile_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_Mobile_Formatted); }
		}

		public ZBool OC_Mobile_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgContactSchema.Constants.OC_Mobile, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OC_Mobile_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgContactSchema.Constants.OC_Mobile, Validation.ValidateOC_Mobile_Formatted, null); }
		}

		public ZPropertyInfo OC_Mobile_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_Mobile_IsManuallyVerified); }
		}

		#endregion

		#region OC_Pager

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString OC_Pager_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(OC_PagerInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(OC_PagerInfo, OC_Pager_FormattedInfo, value, Validation.ValidateOC_Pager_Formatted, OC_Pager_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo OC_Pager_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_Pager_Formatted); }
		}

		public ZBool OC_Pager_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgContactSchema.Constants.OC_Pager, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OC_Pager_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgContactSchema.Constants.OC_Pager, Validation.ValidateOC_Pager_Formatted, null); }
		}

		public ZPropertyInfo OC_Pager_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_Pager_IsManuallyVerified); }
		}

		#endregion

		#region OC_HomePhone

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(HomePhoneIsReadOnly))]
		public ZString OC_HomePhone_Formatted
		{
			get => ViewHomePhoneAllowed ? PhoneNumberPropertyHelper.GetPhoneNumber(OC_HomePhoneInfo) : ViewDeniedMessage;
			set => PhoneNumberPropertyHelper.SetPhoneNumber(OC_HomePhoneInfo, OC_HomePhone_FormattedInfo, value, Validation.ValidateOC_HomePhone_Formatted, OC_HomePhone_IsManuallyVerifiedInfo);
		}

		public ZPropertyInfo OC_HomePhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_HomePhone_Formatted); }
		}

		public ZBool OC_HomePhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgContactSchema.Constants.OC_HomePhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OC_HomePhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgContactSchema.Constants.OC_HomePhone, Validation.ValidateOC_HomePhone_Formatted, null); }
		}

		public ZPropertyInfo OC_HomePhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_HomePhone_IsManuallyVerified); }
		}

		#endregion

		#region OC_OtherPhone

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		public ZString OC_OtherPhone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(OC_OtherPhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(OC_OtherPhoneInfo, OC_OtherPhone_FormattedInfo, value, Validation.ValidateOC_OtherPhone_Formatted, OC_OtherPhone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo OC_OtherPhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_OtherPhone_Formatted); }
		}

		public ZBool OC_OtherPhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgContactSchema.Constants.OC_OtherPhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OC_OtherPhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgContactSchema.Constants.OC_OtherPhone, Validation.ValidateOC_OtherPhone_Formatted, null); }
		}

		public ZPropertyInfo OC_OtherPhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OC_OtherPhone_IsManuallyVerified); }
		}

		#endregion

		#region DefaultCountryCodeForPhoneNumbers

		internal ZString DefaultCountryCodeForPhoneNumbers
		{
			get
			{
				var result = ZString.Empty;
				if (BranchAddress != null)
				{
					result = BranchAddress.DefaultCountryCodeForPhoneNumbers;
				}
				if (result.IsEmpty && AddressOverride != null)
				{
					result = AddressOverride.DefaultCountryCodeForPhoneNumbers;
				}
				if (result.IsEmpty && Header != null)
				{
					result = Header.DefaultCountryCodeForPhoneNumbers;
				}
				return result;
			}
		}

		#endregion

		#region GetNumberForPhoneType

		public ZString GetNumberForPhoneType(string phoneType, bool formatted = false)
		{
			var result = ZString.Empty;
			switch (phoneType)
			{
				case PhoneTypeList.Codes.HOM:
					result = formatted ? OC_HomePhone_Formatted : OC_HomePhone;
					break;
				case PhoneTypeList.Codes.MOB:
					result = formatted ? OC_Mobile_Formatted : OC_Mobile;
					break;
				case PhoneTypeList.Codes.WRK:
					result = formatted ? OC_Phone_Formatted : OC_Phone;
					break;
				case PhoneTypeList.Codes.OTH:
					result = formatted ? OC_OtherPhone_Formatted : OC_OtherPhone;
					break;
				default:
					result = ZString.Empty;
					break;
			}

			return result;
		}

		#endregion

		#endregion

		#region OC_IsActive

		public override ZBool OC_IsActive
		{
			get { return base.OC_IsActive; }
			set
			{
				if (base.OC_IsActive != value)
				{
					base.OC_IsActive = value;

					if (!IsValidationSuspended)
					{
						Documents.OfType<OrgDocument>().ForEach(d => d.Validation.ValidateAll());
					}

					if (value)
					{
						CancelWebAccessSuperseded();
					}
				}
			}
		}

		#endregion

		#region WebAccessSuperseded

		public ZBool WebAccessSuperseded
		{
			get
			{
				switch (supersedeAction)
				{
					case SupersedeWebAccessAction.Supersede:
						return true;
					case SupersedeWebAccessAction.Cancel:
						return false;
					default:
						return ValidSupersededLogs.Any();
				}
			}
		}

		StmALog[] ValidSupersededLogs
		{
			get
			{
				if (!IsInDatabase)
				{
					validSupersededLogs = Array.Empty<StmALog>();
				}
				else if (validSupersededLogs == null || !validSupersededLogs.Any())
				{
					var supersededLogQuery = new ZQuery(StmALogSchema.SL_Parent, PK);
					AddSupersededLogFilter(supersededLogQuery);
					validSupersededLogs = Factory.Load<StmALog>(supersededLogQuery);
				}

				return validSupersededLogs;
			}
		}

		StmALog[] validSupersededLogs;

		public static ZDBOnlySubQuery GetSupersededLogSubQuery()
		{
			var supersededLogQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			AddSupersededLogFilter(supersededLogQuery);
			return supersededLogQuery;
		}

		static void AddSupersededLogFilter(ZQuery query)
		{
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.WebAccessSupersededCode);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddDays(-WebDataRegistry.Instance.ContactRedirectionExpiryDays.Value));
		}

		public bool SupersedeWebAccess()
		{
			if (CanSupersede)
			{
				supersedeAction = SupersedeWebAccessAction.Supersede;
				OC_IsActive = false;
				return true;
			}

			return false;
		}

		protected virtual bool CanSupersede => true;

		public virtual string CannotSupersedeReason => string.Empty;

		public void CancelWebAccessSuperseded()
		{
			supersedeAction = SupersedeWebAccessAction.Cancel;
		}

		enum SupersedeWebAccessAction
		{
			None,
			Supersede,
			Cancel
		}

		SupersedeWebAccessAction supersedeAction = SupersedeWebAccessAction.None;

		#endregion

		public ZString PasswordInstructionSentBy
			=> GetPasswordInstructionSentLog()?.User?.GS_FullName ?? ZString.Empty;

		public ZDateTime PasswordInstructionLastSentTime
		{
			get
			{
				var log = GetPasswordInstructionSentLog();
				return log != null ? log.SL_PostedTimeUtc.ToLocalBranchTime() : ZDateTime.Empty;
			}
		}

		public StmALog GetPasswordInstructionSentLog()
		{
			var query = GetLogQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.WebAccessPasswordEmailSentCode);
			return Factory.LoadTop1<StmALog>(query);
		}

		public ZBool OC_IsPrimaryContact
		{
			get
			{
				return (Factory.Exists(typeof(GlbPersonPrimaryRelationship), new ZQuery(GlbPersonPrimaryRelationshipSchema.PPR_PrimaryId, PK)));
			}
		}

		#region OC_PER

		[RelatedBusinessObject("Person")]
		[List("Lookups.Persons")]
		public override GlbPerson Person
		{
			get
			{
				var person = base.Person;
				if (person != null)
				{
					IsPersonLoaded = true;
					person.DeduplicationChildBizO = this;
				}
				return person;
			}
		}
		internal bool IsPersonLoaded { get; private set; }

		public override ZGuid OC_PER
		{
			get => base.OC_PER;
			set
			{
				if (base.OC_PER != value)
				{
					IsPersonLoaded = false;
					base.OC_PER = value;
				}
			}
		}

		public void ReloadPerson()
		{
			if (Person == null)
			{
				var factory = new BusinessObjectFactory();
				var contact = factory.Load<OrgContact>(PK);
				if (contact != null && !contact.OC_PER.IsEmpty)
				{
					OC_PER = contact.OC_PER;
				}
			}
		}

		#endregion

		#region Implementations

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelperThunk.Value; }
		}

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperThunk;

		#endregion

		#region Internal

		public ZDateTime OC_BirthdayInternal => base.OC_Birthday;

		public bool OC_BirthdayInternalHasChanges { get; private set; }

		public ZString OC_PersonalInfoInternal => base.OC_PersonalInfo;

		public bool OC_PersonalInfoInternalHasChanges { get; private set; }

		public ZString OC_GenderInternal => base.OC_Gender;

		public bool OC_GenderInternalHasChanges { get; private set; }

		public ZString OC_RN_NKNationalityInternal => base.OC_RN_NKNationality;

		public bool OC_RN_NKNationalityInternalHasChanges { get; private set; }

		#endregion

		#endregion

		#region Non-Delivery Report

		public LazyGlbEmailAddress EmailAddress
		{
			get { return emailAddress ?? (emailAddress = new LazyGlbEmailAddress(Factory, () => OC_Email)); }
		}
		LazyGlbEmailAddress emailAddress;

		public ZBool IsNDR
		{
			get => EmailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			set
			{
				if (IsNDR == value)
				{
					return;
				}

				EmailAddress.GI_DeliveryStatus = value ? (ZString)EmailDeliveryReportStatus.Codes.NonDeliveryReport : ZString.Empty;
				IsNDRInfo.RefreshBinding();
				HasChanges = true;
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
			get { return EmailAddress.GI_DeliveryReportTimeUtc; }
		}

		public ZString EmailFallbackToOrganisation
		{
			get { return OC_Email.IsEmpty && ParentOrg != null ? ParentOrg.MainAddress.OA_Email : OC_Email; }
		}

		public ZPropertyInfo EmailFallbackToOrganisationInfo
		{
			get { return GetZPropertyInfo(nameof(EmailFallbackToOrganisation)); }
		}

		#endregion

		#region ContactItems

		[ChildEditable]
		public EmailContactItemCollection EmailContactItems
		{
			get
			{
				if (emailContactItems == null)
				{
					emailContactItems = new EmailContactItemCollection(this);
					emailContactItems.Refresh();
					if (Header != null)
					{
						emailContactItems.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyContactContactDetailsSecurity);
					}

					RegisterEditableChildObject(emailContactItems);
				}

				return emailContactItems;
			}
		}
		EmailContactItemCollection emailContactItems;

		[ChildEditable]
		public PhoneContactItemCollection PhoneContactItems
		{
			get
			{
				if (phoneContactItems == null)
				{
					phoneContactItems = new PhoneContactItemCollection(this);
					phoneContactItems.Refresh();
					if (Header != null)
					{
						phoneContactItems.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyContactContactDetailsSecurity);
					}

					RegisterEditableChildObject(phoneContactItems);
				}

				return phoneContactItems;
			}
		}
		PhoneContactItemCollection phoneContactItems;

		#endregion

		#region Organisation code

		[MaxLength(12)]
		public ZString OrganisationCode
		{
			get { return ParentOrg != null ? ParentOrg.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo OrganisationCodeInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationCode)); }
		}

		#endregion

		#region Company Name

		public ZString WorkingAddressCompanyName => WorkingAddress?.SelectedTranslatedAddress?.CompanyName ?? ZString.Empty;

		#endregion

		#region Working Location

		public ZString WorkingLocation => WorkingAddress?.RelatedPortCodeWithFallback ?? ZString.Empty;

		#endregion

		#region Years In Company / Industry

		[MaxLength(13)]
		public ZString YearsInCompany
		{
			get { return CreateYearsDifferenceString(OC_YearJoinedCompany); }
		}

		public ZPropertyInfo YearsInCompanyInfo
		{
			get { return GetZPropertyInfo(nameof(YearsInCompany)); }
		}

		[MaxLength(13)]
		public ZString YearsInIndustry
		{
			get { return CreateYearsDifferenceString(OC_YearJoinedIndustry); }
		}

		public ZPropertyInfo YearsInIndustryInfo
		{
			get { return GetZPropertyInfo(nameof(YearsInIndustry)); }
		}

		ZString CreateYearsDifferenceString(ZDateTime yearJoinedDate)
		{
			ZString result;

			if (yearJoinedDate == ZDateTime.Invalid ||
				yearJoinedDate == ZDateTime.Empty ||
				yearJoinedDate.Year > ZDateTime.Today.Year ||
				yearJoinedDate.Year < ZDateTime.Today.AddYears(-50).Year)
			{
				result = "";
			}
			else if (yearJoinedDate.Year == ZDateTime.Today.AddYears(-1).Year)
			{
				result = (NoResString)"( 1 Year )";
			}
			else
			{
				result = ZString.Format((NoResString)"( {0} Years )", ZDateTime.Today.Year - yearJoinedDate.Year);
			}

			return result;
		}

		#endregion

		#region Language

		public bool IsEnglish
		{
			get { return OC_Language == Constants.Languages.English; }
		}

		#endregion

		#region Location

		public ZString Location
		{
			get
			{
				if (OrgClosestPort != null)
				{
					return OrgClosestPort.Code;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public ZPropertyInfo LocationInfo
		{
			get { return GetZPropertyInfo(Schema.Location); }
		}

		#endregion

		#region IsSystemGenerated

		public bool IsSystemGenerated
		{
			get { return OC_SystemCreateUser == User.ServiceUserCode; }
		}

		#endregion

		#region IsSystemDefaultContact

		public bool IsSystemDefaultContact { get; set; }
		public bool IsSystemDefaultContactForAutoDelivery { get; set; }
		public MultilingualString SystemDefaultContactName { get; set; }

		#endregion

		#region IsSavedByFactory

		public override bool IsSavedByFactory
		{
			get
			{
				return base.IsSavedByFactory && !IsSystemDefaultContact;
			}
		}

		#endregion

		#region Document Delivery Details

		public DocDeliveryContact DocDeliveryDetails()
		{
			return DocDeliveryDetails(null, null);
		}

		public DocDeliveryContact DocDeliveryDetails(OrgDocument deliveryDoc)
		{
			return DocDeliveryDetails(deliveryDoc, null);
		}

		public DocDeliveryContact DocDeliveryDetails(OrgDocument documentDelivery, IStmMenuItem menuItem)
		{
			DocDeliveryContact result = new DocDeliveryContact(Factory);
			result.Initialise(menuItem, documentDelivery);
			result.IsSystemDefaultContact = IsSystemDefaultContact;
			result.Name = OC_ContactName;
			result.OrgHeaderPK = OC_OH;
			if (menuItem != null && !menuItem.SU_DefaultAttachmentType.IsEmpty)
			{
				OC_AttachmentType = menuItem.SU_DefaultAttachmentType;
			}
			PopulateDeliveryDetails(result, documentDelivery);
			PopulateContactAddressDetails(result);
			if (documentDelivery != null)
			{
				result.ClearCcAndBccRecipients();
			}
			PopulateCcAndBccRecipients(result, documentDelivery);
			return result;
		}

		public ZBool SetDocGroupAsDefault(ZString docGroup)
		{
			return ParentOrg != null ? ParentOrg.Contacts.SetDocGroupAsDefault(docGroup) : ZBool.False;
		}

		void PopulateDeliveryDetails(DocDeliveryContact details, OrgDocument deliveryDoc)
		{
			details.DeliveryMethod = string.IsNullOrEmpty(deliveryDoc?.OD_DeliverBy) ? OC_NotifyMode : deliveryDoc.OD_DeliverBy;
			details.AttachmentType = deliveryDoc == null || string.IsNullOrEmpty(deliveryDoc.OD_AttachmentType) ? OC_AttachmentType : deliveryDoc.OD_AttachmentType;
			details.SendIndividually = deliveryDoc == null ? false : deliveryDoc.OD_SendIndividually;
			details.EmailSubjectMacro = deliveryDoc?.OD_EmailSubjectMacro ?? ZString.Empty;
		}

		void PopulateContactAddressDetails(DocDeliveryContact details)
		{
			var orgForDetails = AddressOverride != null && !IsGeneratingDocDeliveryRecipient ? AddressOverride : ParentOrg;
			var addressForDetails = OrgAddress;

			details.Salutation = OC_Salutation;
			details.SystemDefaultContactName = SystemDefaultContactName;
			if (orgForDetails != null)
			{
				details.OrgHeaderPK = orgForDetails.PK;
			}
			if (addressForDetails != null)
			{
				details.OrgAddressPK = addressForDetails.PK;
			}
		}

		internal void PopulateCcAndBccRecipients(DocDeliveryContact contact, OrgDocument document)
		{
			if (document != null)
			{
				foreach (var orgDocumentCarbonCopyRecipient in document.CarbonCopyRecipients)
				{
					var carbonCopyRecipient = new NonPersistentCopyRecipient(contact.OrgHeader, orgDocumentCarbonCopyRecipient.ODR_RecipientType) { EmailAddress = orgDocumentCarbonCopyRecipient.ODR_EmailAddress };
					contact.EmailCarbonCopyRecipients.Add(carbonCopyRecipient);
				}
				foreach (var orgDocumentBlindCarbonCopyRecipient in document.BlindCarbonCopyRecipients)
				{
					var blindCarbonCopyRecipient = new NonPersistentCopyRecipient(contact.OrgHeader, orgDocumentBlindCarbonCopyRecipient.ODR_RecipientType) { EmailAddress = orgDocumentBlindCarbonCopyRecipient.ODR_EmailAddress };
					contact.EmailBlindCarbonCopyRecipients.Add(blindCarbonCopyRecipient);
				}
			}
		}

		internal bool IsGeneratingDocDeliveryRecipient => isGeneratingDocDeliveryRecipientCount > 0;
		int isGeneratingDocDeliveryRecipientCount;
		public IDisposable GenerateDocDeliveryRecipient()
		{
			isGeneratingDocDeliveryRecipientCount++;
			return new DisposableAction(() => isGeneratingDocDeliveryRecipientCount--);
		}

		#endregion

		#region Web Password

		public GlbBranch BranchForLogin
		{
			get
			{
				var webBranchForLogin = ParentOrg.GetBranchForLogin();
				if (webBranchForLogin != null)
				{
					return webBranchForLogin;
				}
				else
				{
					var webBranch = Factory.Load<GlbBranch>(DataRegistry.Instance.WebBranch);
					return webBranch;
				}
			}
		}

		public Guid CompanyPKForLogin
		{
			get
			{
				return BranchForLogin?.Company.PK.ToGuid() ?? Guid.Empty;
			}
		}

		public ContactPasswordEmail GetWebPasswordEmail()
		{
			return new ContactPasswordEmail(this);
		}

		public Guid CompanyPKForEmailTemplate { get; set; }

		#endregion

		#region Hashed Password

#if DEBUG
		//used for tests to store plaintext password so it's easier to recall
		public ZString PasswordForTesting { get; private set; }
#endif

		public void SetHashedPassword(string newPassword, string currentPassword = null)
		{
#if DEBUG
			PasswordForTesting = newPassword;
#endif
			using (GetValidationSuspender())
			{
				PasswordHistoryHelper.AddPasswordHistory(this, currentPassword);
				var hashingIterationsCount = DataRegistry.Instance.PasswordHashingIterationsCount;
				UserSecretsContext.DefaultContext.SaveSecret(newPassword, UserSecretHashAlgorithmExtensions.PreferredAlgorithm, hashingIterationsCount, GetPasswordAdapter());
			}
		}

		public OrgContactAdapter GetPasswordAdapter()
			=> new OrgContactAdapter(this);

		public void RemovePasswordAndHash()
		{
			using (GetValidationSuspender())
			{
				if (Person == null)
				{
					PasswordHistoryHelper.AddPasswordHistory(this, null); //save current password to history
				}
				else
				{
					PasswordHistoryHelper.MovePasswordHistories(this, Person);
				}

				OC_PasswordHash = ZBlob.Empty;
				OC_PasswordSalt = ZBlob.Empty;
				OC_PasswordHashIterations = ZInt.Zero;
			}
		}

		public bool HasPassword => !(OC_PasswordHash.IsEmpty && (Person == null || !Person.HasPassword));

		[DocumentMacroIgnore]
		public override ZBlob OC_PasswordHash { get => base.OC_PasswordHash; set => base.OC_PasswordHash = value; }

		[DocumentMacroIgnore]
		public override ZBlob OC_PasswordSalt { get => base.OC_PasswordSalt; set => base.OC_PasswordSalt = value; }

		#endregion

		#region IExcelPasswordRetrieverForDocumentDelivery Members

		string IExcelPasswordRetrieverForDocumentDelivery.ExcelPasswordForOpening
		{
			get
			{
				return ExcelPasswordSupporter.ExcelPasswordForOpening;
			}
		}

		string IExcelPasswordRetrieverForDocumentDelivery.ExcelPasswordForModifying
		{
			get
			{
				return ExcelPasswordSupporter.ExcelPasswordForModifying;
			}
		}

		#endregion

		#region excel password

		ExcelPasswordSupporter ExcelPasswordSupporter
		{
			get { return excelPasswordSupporter ??= new ExcelPasswordSupporter(this); }
		}

		ExcelPasswordSupporter excelPasswordSupporter;

		[BusinessObjectTestExclude]
		[DocumentMacroIgnore]
		[MaxLength(32)]
		public ZString ExcelPasswordForOpening
		{
			get
			{
				if (!ViewPersonalInformationAllowed)
				{
					return ViewDeniedMessage;
				}

				return ExcelPasswordSupporter.ExcelPasswordForOpening;
			}
			set
			{
				if (ExcelPasswordSupporter.ExcelPasswordForOpening != value)
				{
					ExcelPasswordSupporter.ExcelPasswordForOpening = value;
					ExcelPasswordForOpeningInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo ExcelPasswordForOpeningInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExcelPasswordForOpening)); }
		}

		protected internal bool ExcelPasswordForOpening_ReadOnly => !ModifyPersonalInformationAllowed;

		[BusinessObjectTestExclude]
		[DocumentMacroIgnore]
		[MaxLength(32)]
		public ZString ExcelPasswordForModifying
		{
			get
			{
				if (!ViewPersonalInformationAllowed)
				{
					return ViewDeniedMessage;
				}

				return ExcelPasswordSupporter.ExcelPasswordForModifying;
			}
			set
			{
				if (ExcelPasswordSupporter.ExcelPasswordForModifying != value)
				{
					ExcelPasswordSupporter.ExcelPasswordForModifying = value;
					ExcelPasswordForModifyingInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo ExcelPasswordForModifyingInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExcelPasswordForModifying)); }
		}

		protected internal bool ExcelPasswordForModifying_ReadOnly => !ModifyPersonalInformationAllowed;

		#endregion

		#region IContactable Members

		string IContactBase.Name
		{
			get { return this.ContactNameWithoutNumberSuffix; }
		}

		ZString IGlbPersonPrimarySource.WorkNumber => OC_Phone;
		ZString IGlbPersonPrimarySource.WorkExtension => OC_PhoneExtension;
		ZString IGlbPersonPrimarySource.MobileNumber => OC_Mobile;
		ZString IGlbPersonPrimarySource.JobCategory => OC_JobCategory;

		string IContactBase.Email
		{
			get { return this.OC_Email; }
		}

		string IContactable.Mobile
		{
			get { return this.OC_Mobile; }
		}

		bool IContactable.IsActive
		{
			get { return this.OC_IsActive; }
		}

		IContactable[] IContactable.GetNestedContacts(string parentContactDescription)
		{
			return Array.Empty<IContactable>();
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			string propertyName = property.Name;

			if (Header != null)
			{
				if (propertyName == OrgContactSchema.OC_WebAccessEnabled.Name)
				{
					shouldBeReadOnly = !(Header.IsInDatabase ? Header.SecurityProvider.HasModifyDetailsWebSecurity : Header.SecurityProvider.HasNewDetailsWebSecurity);
				}
				else if (propertyName == OrgContactSchema.OC_Salutation.Name ||
							propertyName == OrgContactSchema.OC_NotifyMode.Name ||
							propertyName == OrgContactSchema.OC_AttachmentType.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyContactDocDeliveryDetailsSecurity;
				}
				else if (propertyName == OrgContactSchema.OC_Birthday.Name ||
					propertyName == OrgContactSchema.OC_PersonalInfo.Name)
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyContactPersonalInformationSecurity;
				}
				else
				{
					shouldBeReadOnly = !Header.SecurityProvider.HasModifyContactContactDetailsSecurity;
				}
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region IPasswordEmailSource Members

		ZString IPasswordEmailSource.Password => ZString.Empty;

		public ZString Url => WebDataRegistry.Instance.WebTrackerUrl.GetFallBackValueAtAllLevels(CompanyPKForLogin, Guid.Empty, Guid.Empty);

		public ZString OrgCode => ParentOrg != null ? ParentOrg.OH_Code : ZString.Empty;

		public ZString ExtraInstruction => Parser.Parse(this, WebDataRegistry.Instance.PasswordResetEmailTemplate.GetFallBackValueAtAllLevels(CompanyPKForLogin, Guid.Empty, Guid.Empty).EmailBody);

		public virtual ZString FromDisplayName => GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_FullName : ZString.Empty;

		public virtual ZString FromAddress => EnvProxy.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress;

		public ZString Salutation => !this.OC_Salutation.IsEmpty ? this.OC_Salutation : this.OC_ContactName;

		public ZString Language => OC_Language;

		DocumentParser Parser
		{
			get
			{
				if (parser == null)
				{
					parser = DocumentParser.New(ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(), Factory);
				}
				return parser;
			}
		}
		DocumentParser parser;

		#endregion

		#region IPasswordResetEmailSource Members

		public ContactPasswordInstructionEmail PasswordInstructionEmail
		{
			get => passwordInstructionEmail ?? (passwordInstructionEmail = new ContactPasswordInstructionEmail(this));
		}
		ContactPasswordInstructionEmail passwordInstructionEmail;

		public virtual ZString GeneratePasswordInstructionUrl(string token, PasswordInstructionType instructionType)
		{
			var companypkForLogin = CompanyPKForLogin;
			var webTrackerUrl = WebDataRegistry.Instance.WebTrackerUrl.GetFallBackValueAtAllLevels(companypkForLogin, Guid.Empty, Guid.Empty);

			if (string.IsNullOrEmpty(webTrackerUrl))
			{
				var branch = BranchForLogin;
				if (branch == null || !branch.GB_IsActive)
				{
					var branchName = branch?.GB_BranchName;
					throw new WebSiteUrlNotSetException(Res.GetString("6B4C0003-2083-44D3-9B62-2CEE096755A7", "The Organization lacks an active controlling branch. The inactive branch name is {0}", branchName));
				}

				var company = Factory.Load<GlbCompany>(companypkForLogin);
				var companyName = company?.CompanyName;
				if (company == null || !company.GC_IsActive)
				{
					var branchName = branch.GB_BranchName;
					throw new WebSiteUrlNotSetException(Res.GetString("33e32e0e-f198-4bdd-9498-7f302e9cf6bb", "The Organization lacks an active controlling branch. The inactive company name is {0}. The branch name is {1}.", companyName, branchName));
				}

				throw new WebSiteUrlNotSetException(Res.GetString("b47cd9a9-6f45-4ac6-bb4f-8fe8de614fd1", "You need to provide a Web Tracker URL for {0} in the System Registry under path Web > Web Component URLs > WebTracker URL", companyName));
			}

			var shouldSendToMasterPasswordPage = ((IPasswordInstructionEmailSource)this).ShouldSendMasterPassword;

			if (instructionType == PasswordInstructionType.Reset)
			{
				var resetPage = shouldSendToMasterPasswordPage ? TrackingConstants.RelativePath.ResetMasterPasswordPage : TrackingConstants.RelativePath.ResetPasswordPage;

				return FormattableString.Invariant($"{webTrackerUrl.TrimEnd('/')}/{resetPage}?{TrackingConstants.QueryStringKeys.ResetPasswordKey}={token}");
			}

			var setPage = shouldSendToMasterPasswordPage ? TrackingConstants.RelativePath.SetMasterPasswordPage : TrackingConstants.RelativePath.SetPasswordPage;
			return FormattableString.Invariant($"{webTrackerUrl.TrimEnd('/')}/{setPage}?{TrackingConstants.QueryStringKeys.SetPasswordKey}={token}");
		}

		#endregion

		#region Password Instruction Url

		StmALog GetPasswordPreviouslyChangedLog()
		{
			var query = GetLogQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.WebAccessPasswordChangedCode);

			return Factory.LoadTop1<StmALog>(query);
		}

		public StmALog GetPasswordChangedOrSentLog()
		{
			var query = GetLogQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.WebAccessPasswordEmailSentCode, AutoEvents.WebAccessPasswordChangedCode });

			return Factory.LoadTop1<StmALog>(query);
		}

		ZQuery GetLogQuery()
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, new[] { PK, OC_PER })
			{
				OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending
			};

			return query;
		}

		public PasswordInstructionType GetPasswordInstructionType()
		{
			if (IsPasswordBeingCleared())
			{
				return PasswordInstructionType.Set;
			}
			return GetPasswordPreviouslyChangedLog() != null ? PasswordInstructionType.Reset : PasswordInstructionType.Set;
		}

		public ZString PasswordInstructionMacroUrl => GeneratePasswordInstructionUrl(TokenMacro, GetPasswordInstructionType());

		public const string TokenMacro = "(*SetOrResetPasswordToken*)";

		BusinessObject IPasswordInstructionEmailSource.SenderForLogs => HasOrIsClearingPersonPassword ? Person : this;

		bool IPasswordInstructionEmailSource.ShouldSendMasterPassword => HasOrIsClearingPersonPassword || Person != null && Person.ContactCollection.Count == 1; //Should send master password if this is the only contact in the person relationship

		public ZString EmailOrgCodes
		{
			get
			{
				if (Person == null)
				{
					return ParentOrg.OH_Code;
				}

				return string.Join(", ", Person.ContactCollection.Cast<OrgContact>().Where(x => x.OC_IsActive && x.OC_WebAccessEnabled && OC_Email.EqualsIgnoringCase(x.OC_Email)).Select(x => x.ParentOrg.OH_Code).OrderBy(x => x));
			}
		}

		ZString IPasswordInstructionEmailSource.OrgCodes => EmailOrgCodes;

		bool HasOrIsClearingPersonPassword => IsClearingPersonPasswordOverride || Person != null && (Person.HasPassword || IsPersonPasswordBeingCleared);

		public bool IsClearingPersonPasswordOverride { get; set; }

		bool IsPersonPasswordBeingCleared => !Person.PER_PasswordHashInfo.OriginalValue.IsEmpty && Person.PER_PasswordHash.IsEmpty;

		#endregion

		#region IExamUrlRecipient Members

		string IExamUrlRecipient.Language
		{
			get { return OC_Language; }
		}

		bool IExamUrlRecipient.HasNonPermittedDuplicateEmail => false;

		#region IGlbCompanyCampaignItemRecipient Members

		string IGlbCompanyCampaignItemRecipient.Phone
		{
			get { return OC_Phone; }
		}

		OrgHeader IGlbCompanyCampaignItemRecipient.Organisation
		{
			get { return Header; }
		}

		string IGlbCompanyCampaignItemRecipient.Salutation
		{
			get { return OC_Salutation; }
		}

		string IGlbCompanyCampaignItemRecipient.Title
		{
			get { return OC_Title; }
		}

		string IGlbCompanyCampaignItemRecipient.Fax
		{
			get { return OC_Fax; }
		}

		string IGlbCompanyCampaignItemRecipient.RelatedDocName
		{
			get
			{
				var orgCode = Header == null ? string.Empty : $" ({Header.OH_Code})";
				return $"{ResString.GetMultilingualString("FB113FB4-A4C7-45E5-975D-DC25DC202A95", "Contact")} {OC_ContactName + orgCode}";
			}
		}

		#endregion

		#endregion

		#region IAddressBookRecipient Members

		OrgHeader IAddressBookRecipient.Organisation
		{
			get { return Header; }
		}

		ZString IAddressBookRecipient.Phone
		{
			get { return OC_Phone; }
		}

		ZString IAddressBookRecipient.Title
		{
			get { return OC_Title; }
		}

		ZString IAddressBookRecipient.Location
		{
			get { return BranchAddress != null ? BranchAddress.OA_Address1 : Header.MainAddress.OA_Address1; }
		}

		bool IAddressBookRecipient.IsActive
		{
			get { return OC_IsActive; }
		}

		ZString IAddressBookRecipient.Role
		{
			get { return string.Empty; }
		}

		public ZString Name
		{
			get { return ((IContactBase)this).Name; }
		}

		public ZString Email
		{
			get { return ((IContactBase)this).Email; }
		}

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

		#region IConversationParticipant

		ZString IConversationParticipant.Code => UniqueCode;
		ZString IConversationParticipant.Name => UniqueCode;
		ZString IConversationParticipant.JobTitle => OC_Title;
		ZString IConversationParticipant.OrganisationName => ParentOrg?.OH_FullName ?? string.Empty;
		ZString IConversationParticipant.Location => OrgAddress?.OA_RL_NKRelatedPortCode ?? string.Empty;
		ZBool IConversationParticipant.IsActive => OC_IsActive;
		ZBool IConversationParticipant.IsInternal => false;
		ZString IConversationParticipant.Email => OC_Email;
		ZString IConversationParticipant.Language => string.IsNullOrEmpty(OC_Language) ? Header.OH_Language : OC_Language;
		ZString IConversationParticipant.DisplayText => UniqueCode;

		void IConversationParticipant.CheckCanParticipate(INotifications notifications)
		{
			if (string.IsNullOrEmpty(OC_Email))
			{
				notifications.AddError(Res.GetString("67371e84-7edf-4302-9bfe-96ce64d3de32", "This Participant cannot be added as they do not have an associated email address"));
			}
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				var handlers = new List<IUniqueIndexFailureHandler>();

				if (IsContactNameChangedByGlbPersonSynchronisation)
				{
					handlers.Add(new OrgContactUniqueIndexFailureHandler(this));
				}
				else
				{
					handlers.AddRange(base.UniqueIndexFailureHandlers);
				}

				return handlers;
			}
		}

		public ActiveOrgContactItemCollection ContactItems
		{
			get
			{
				if (contactItems == null)
				{
					contactItems = new ActiveOrgContactItemCollection(this);
				}

				return contactItems;
			}
		}
		ActiveOrgContactItemCollection contactItems;

		class OrgContactUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public OrgContactUniqueIndexFailureHandler(OrgContact parent) => Parent = parent;

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return OrgContactSchema.Constants.Indexes.NR_UX__OC_ContactName_OC_OH;
				}
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(
					Res.GetString("a02684b0-d507-4c0f-b827-689085d0610e", @"You are trying to synchronize the Person's name to a Contact name ({0}) that already exists on Organization ({1}).
The contact name must be unique within each organization.
Please enter a different name or alter the existing contact first.", Parent.OC_ContactName, Parent.Header?.OH_Code),
					Res.GetString("9d9daccb-ae08-4c7a-ad1e-74c55dedc21b", "Duplicate"));
			}

			readonly OrgContact Parent;
		}

		#endregion

		bool ISupportDataImporting.IsImportingData { get; set; }

		#region IsContactNameChangedByGlbPersonSynchronisation

		bool IsContactNameChangedByGlbPersonSynchronisation { get; set; }

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				IsContactNameChangedByGlbPersonSynchronisation = false;
				OC_BirthdayInternalHasChanges = false;
				OC_PersonalInfoInternalHasChanges = false;
				OC_GenderInternalHasChanges = false;
				OC_RN_NKNationalityInternalHasChanges = false;
			}
		}

		#endregion

		#region Suggested Job Categories

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public List<(string, string)> SuggestedJobCategories { get; set; } = new List<(string, string)>();

		#endregion

		#region IGlbPersonPrimarySource Members

		ZGuid IGlbPersonPrimarySource.PersonPK => OC_PER;

		OrgAddress WorkingAddress
		{
			get
			{
				if (OrgAddress != null)
				{
					return OrgAddress;
				}

				var address = ParentOrg?.Addresses?.MainAddress;

				if (address != null &&
					IsGeneratingDocDeliveryRecipient &&
					IsSystemDefaultContact &&
					OC_NotifyMode == Constants.ContactNotifyModes.Email &&
					string.IsNullOrEmpty(address.OA_Email))
				{
					ParentOrg.Addresses.Reload(true);
					address = ParentOrg.Addresses.MainAddress;
				}

				return address;
			}
		}

		ZString IGlbPersonPrimarySource.CompanyName => WorkingAddressCompanyName;

		ZString IGlbPersonPrimarySource.Code => OrganisationCode;

		ZString IGlbPersonPrimarySource.City => WorkingAddress?.City ?? ZString.Empty;

		ZString IGlbPersonPrimarySource.State => WorkingAddress?.OA_State ?? ZString.Empty;

		ZString IGlbPersonPrimarySource.Country => WorkingAddress?.EffectiveRelatedPortCode?.Country?.RN_Desc ?? ZString.Empty;

		ZString IGlbPersonPrimarySource.UNLOCO => WorkingAddress?.OA_RL_NKRelatedPortCode ?? ZString.Empty;

		ZString IGlbPersonPrimarySource.JobTitle => OC_Title;

		string IGlbPersonPrimarySource.TableCode => OrgContactSchema.Constants.Prefix;

		#endregion

		#region IPasswordStored Member

		int IPasswordStored.PasswordHashIterations => OC_PasswordHashIterations;

		ZBlob IPasswordStored.PasswordSalt => OC_PasswordSalt;

		ZBlob IPasswordStored.PasswordHash => OC_PasswordHash;

		public bool VerifyPassword(IUserSecretsContext userSecretsContext, string password)
		{
			if (Person != null && Person.HasPassword)
			{
				return userSecretsContext.IsMatchingSecret(password, Person.GetPasswordAdapter());
			}

			return !OC_PasswordHash.IsEmpty && userSecretsContext.IsMatchingSecret(password, GetPasswordAdapter());
		}

		#endregion

		public bool CheckContactHasValidEmail()
		{
			return !(OC_Email.IsEmpty || OC_EmailInfo.HasErrors());
		}

		#region LockedOut

		IOrgContactLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IOrgContactLoginAttemptRecorder>());
		IOrgContactLoginAttemptRecorder loginAttemptRecorder;

		public ZDateTime LockoutDateTimeLocal => LoginAttemptRecorder.LockoutDateTimeLocal(OrgCode, OC_Email);

		#endregion

		#region IGlbPasswordHistoryParent Member

		bool IGlbPasswordHistoryParent.ShouldSavePasswordHistory => true;

		BusinessObject IGlbPasswordHistoryParent.BusinessEntity => this;

		int IGlbPasswordHistoryParent.PasswordHistoryCount => WebDataRegistry.Instance.WebPasswordHistoryCount.Value;

		public bool HasPasswordBeenUsed(string password) => PasswordHistoryHelper.HasPasswordBeenUsed(this, password);

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			Person?.UpdateFromContact(this);
		}
#endif
	}

	public class FindSuggestedJobCategoriesEventArgs : EventArgs
	{
		public bool ShouldOverwrite { get; set; }
		public bool UseCache { get; set; }
	}
}
