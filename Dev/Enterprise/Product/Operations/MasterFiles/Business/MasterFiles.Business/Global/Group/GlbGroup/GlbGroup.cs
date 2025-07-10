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
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(GlbGroup.Schema.GG_Desc)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[UserDefinedValues]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = "AfterUniversalCopy")]
	public class GlbGroup : AutoGlbGroup,
		IGlbGroup,
		IDocManagerSupport,
		IOrgsAndWarehousesAccessProvider,
		ISupportChangeOthersSecurity,
		IDocumentSupportable,
		IADLinkedEntity,
		IConversationParticipant,
		IWorkflowProvider,
		ICustomFieldProvider,
		IEmailAddressGetterForTrigger
	{
		#region Schema

		public abstract new class Schema : AutoGlbGroup.Schema
		{
			public const string IsGlobal = "IsGlobal";
			public const string IsADLinked = "IsADLinked";
		}

		#endregion

		public GlbGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new GlbGroupTypeDecider();

		#region SCIM

		public bool IsControlledByScim
		{
			get
			{
				return !GG_ExternalId.IsEmpty && !SystemDataRegistry.Instance.ScimAllowLocalEditing.Value;
			}
		}

		public ZBool IsScimMappedGroup
		{
			get
			{
				return SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.Value.HasGroup(this);
			}
		}

		#endregion

		#region GG_GC

		[List("Lookups.Companies")]
		public override ZGuid GG_GC
		{
			get { return base.GG_GC; }
			set
			{
				if (base.GG_GC != value)
				{
					base.GG_GC = value;
					if (!value.IsEmpty)
					{
						isGlobal = false;
					}
				}
			}
		}

		protected bool GG_GC_ReadOnly
		{
			get { return IsGlobal; }
		}

		#endregion

		#region GG_Type

		[List("Lookups.Types")]
		public override ZString GG_Type
		{
			get => base.GG_Type;
			set => base.GG_Type = value;
		}

		#endregion

		#region GG_Code

		[ReadOnlyMember(nameof(GG_CodeReadOnly))]
		public override ZString GG_Code
		{
			get { return base.GG_Code; }
			set { base.GG_Code = value; }
		}

		public bool GG_CodeReadOnly
		{
			get
			{
				return GG_IsSystemDefined;
			}
		}

		#endregion

		#region GG_Desc

		protected bool GG_Desc_ReadOnly
		{
			get
			{
				return IsControlledByScim;
			}
		}

		#endregion

		#region GG_GG_Parent

		protected bool GG_GG_ParentGroup_ReadOnly
		{
			get
			{
				return IsControlledByScim;
			}
		}

		#endregion

		#region System-defined Groups

		public ZBool IsAllStaffGroup
		{
			get { return GG_Code == AllStaffGroupCode; }
		}

		public const string AllStaffGroupCode = "ALL";
		public const string PostMastersGroupCode = "PMG";

		public readonly static Guid DbDeveloperGroupPK = new Guid("a99e7f0e-8379-4f50-8560-9b4bb804c0de");
		public readonly static Guid DbReaderGroupPK = new Guid("6f0eb310-fc5c-4696-9594-f8ce156542c6");
		public readonly static Guid BackupOperatorGroupPK = new Guid("208068b6-3383-44bf-8e0d-dbd827f9d675");

		public ZBool IsFixedDatabaseAccessGroup => PK.Equals(DbDeveloperGroupPK) || PK.Equals(DbReaderGroupPK) || PK.Equals(BackupOperatorGroupPK);

		#endregion

		#region GG_IsActive

		public override ZBool GG_IsActive
		{
			get { return base.GG_IsActive; }
			set
			{
				if (base.GG_IsActive != value)
				{
					base.GG_IsActive = value;

					if (value && IsADIntegrationEnabled &&
						(GG_ActiveDirectoryObjectGuid == ZGuid.Empty ||
						(GG_ActiveDirectoryObjectGuid.IsValid && !CanAccessDirectoryEntry())
					))
					{
						GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;
					}
				}
			}
		}

		#endregion

		#region IsADLinked

		[ResourceStringData("GlbGroup.IsADLinked", Caption = "Is AD Linked", FullDescription = "Indicates whether this group is linked with an Active Directory group.")]
		public ZBool IsADLinked => GG_ActiveDirectoryObjectGuid.IsValid;

		public ZPropertyInfo IsADLinkedInfo => GetZPropertyInfo(Schema.IsADLinked);

		#endregion

		#region GG_ActiveDirectoryObjectGuid

		public override ZGuid GG_ActiveDirectoryObjectGuid
		{
			get { return base.GG_ActiveDirectoryObjectGuid; }
			set
			{
				base.GG_ActiveDirectoryObjectGuid = value;
				IsADLinkedInfo.RefreshBinding();
			}
		}

		#endregion

		#region GG_IsSecurityEnabled

		[ReadOnlyMember(nameof(IsNonSecurityGroupReadOnly))]
		public override ZBool GG_IsSecurityEnabled
		{
			get { return base.GG_IsSecurityEnabled; }
			set
			{
				if (base.GG_IsSecurityEnabled != value)
				{
					base.GG_IsSecurityEnabled = value;

					if (!value)
					{
						ResetGroupPermissions();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateGG_GG_ParentGroup();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsNonSecurityGroupReadOnly))]
		public ZBool IsNonSecurityGroup
		{
			get
			{
				return !GG_IsSecurityEnabled;
			}
			set
			{
				GG_IsSecurityEnabled = !value;
			}
		}

		public ZPropertyInfo IsNonSecurityGroupInfo => GetWrappedZPropertyInfo(nameof(IsNonSecurityGroup), _ => GG_IsSecurityEnabledInfo);

		protected bool IsNonSecurityGroupReadOnly => !Env.Security.GroupsModifySecurityRights.IsAllowed && this.IsInDatabase;

		#endregion

		#region GG_Category

		[List("Lookups.CategoryList")]
		public override ZString GG_Category { get => base.GG_Category; set => base.GG_Category = value; }

		#endregion

		#region GG_GG_ParentGroup

		public override ZGuid GG_GG_ParentGroup
		{
			get => base.GG_GG_ParentGroup;
			set
			{
				base.GG_GG_ParentGroup = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateGG_IsSecurityEnabled();
				}
			}
		}

		#endregion

		#region Save

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted && GG_IsSystemDefined)
			{
				throw new CannotDeleteException($"The '{GG_Code}' group is a system-defined group and cannot be deleted");
			}

			Staff.RemoveAll();
			var customisedLayoutLinks = ObjectFactory.Get<IBMControlCustomisationLinkCollection>("IBMControlCustomisationLinkCollection", this);
			customisedLayoutLinks.DeleteAll();
			this.DeleteChildren<GlbExternalPassword>(GlbExternalPasswordSchema.GP_GS);
			WorkflowItems.RemoveAndDeleteAll();
			Roles.DeleteAll();

			base.Delete();
		}

		public override string CanCancel()
		{
			if (GG_IsActiveInfo.HasErrors())
			{
				return GG_IsActiveInfo.Notifications.GetFirstMessage();
			}

			if (GG_IsSystemDefined)
			{
				return Res.GetString("81AE9C40-8A49-4E7B-9E03-5DD08894970B", "{0} is a system defined group and cannot be deactivated.", HumanReadableName);
			}

			if (IsControlledByScim)
			{
				return Res.GetString("5D8E178F-76A4-4594-A894-360DFCE6FC9C", "{0} is controlled externally and cannot be deactivated.", HumanReadableName);
			}

			return base.CanCancel();
		}

		public override bool IsCancelled
		{
			get => base.IsCancelled;
			set
			{
				if (value && Staff.Count > 0 && !IsControlledByScim)
				{
					Staff.RemoveAll();
				}

				base.IsCancelled = value;
			}
		}

		#endregion

		#region Related Business Objects

		#region Staff

		[ChildEditable(true)]
		[ReadOnlyMember(nameof(IsControlledByScim))]
		public GlbStaffManyToManyCollection Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffManyToManyCollection(this);
					fStaff.Load();

					RegisterEditableChildObject(fStaff);
					if (IsControlledByScim)
					{
						fStaff.SetReadOnlyIncludingChildren(true);
					}
				}

				return fStaff;
			}
		}
		GlbStaffManyToManyCollection fStaff;

		#endregion

		#region Security (GlbSecurity)

		[ChildEditable(true)]
		public GlbSecurityCollection SecurityPermissions
		{
			get
			{
				if (fSecurityPermissions == null)
				{
					fSecurityPermissions = new GlbSecurityCollection(this, Factory);
					ZQuery query = new ZQuery();
					//ZQuery query = new ZQuery(GlbSecuritySchema.GU_GG, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					fSecurityPermissions.LoadWithMoreFiltering(query);
					RegisterEditableChildObject(fSecurityPermissions);
				}

				return fSecurityPermissions;
			}
		}
		GlbSecurityCollection fSecurityPermissions;

		public GlbSecurityCollectionView SecurityPermissionsView
		{
			get
			{
				if (fSecurityPermissionsView == null)
				{
					fSecurityPermissionsView = new GlbSecurityCollectionView(SecurityPermissions);
				}

				return fSecurityPermissionsView;
			}
		}

		GlbSecurityCollectionView fSecurityPermissionsView;

		#endregion

		[ChildEditable(true)]
		public GlbGroupRoleCollection Roles
		{
			get
			{
				if (fRoles == null)
				{
					fRoles = new GlbGroupRoleCollection(this);

					RegisterEditableChildObject(fRoles);
				}

				return fRoles;
			}
		}
		GlbGroupRoleCollection fRoles;

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SetDefaultValuesCore();
		}

		protected virtual void SetDefaultValuesCore()
		{
			SecurityBranch = GlbBranch.CurrentBranch != null ? GlbBranch.CurrentBranch.PK : ZGuid.Empty;
			SecurityDepartment = GlbDepartment.CurrentDepartment != null ? GlbDepartment.CurrentDepartment.PK : ZGuid.Empty;
			IsNonSecurityGroup = false;

			var factoryCopyService = Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>();
			if (factoryCopyService != null)
			{
				factoryCopyService.AddOnCopyFinishedAction(DefaultPermissionsForCopiedGroup);
			}
			else
			{
				DefaultPermissionsForNewGroup(true);
			}

			if (IsADIntegrationEnabled)
			{
				GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			}
		}

		#region Default Security Permissions for New Group

		void DefaultPermissionsForCopiedGroup()
		{
			DefaultPermissionsForNewGroup(false);
		}

		void DefaultPermissionsForNewGroup(bool overrideExisting)
		{
			if (Env.Security != null)
			{
				using (SuspendSettingHasChanges())
				{
					DenyGroupSecurity(Env.Security.Operations.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.Manage.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.Maintain.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.Login.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.SpecializedRights.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.UseSqlFilterStrip.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.PublishGlobalFilterLayouts.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.PublishGlobalGridColorSchemes.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.PublishGlobalNoteTemplates.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.AutoRefreshModuleGrids.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.Notes.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.DocumentsReports.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.PublishGlobalUniversalCopyTemplates.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.IgnoreMandatoryToRead.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.SaveDataImportWizardSettings.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.OrgDeniedPartyScreeningAllowJobLevelClear.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.EditAllGlobalColourSchemes.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.EditUserDefinedFilters.Code, overrideExisting);
					DenyGroupSecurity(Env.Security.AccessUnpublishedCustomizedDocumentsAndReports.Code, overrideExisting);
				}
			}
		}

		void DenyGroupSecurity(string securityRight, bool overrideExisting)
		{
			var security = SecurityPermissions.Find(s => s.GU_SecurityRight == securityRight).FirstOrDefault();
			if (security == null || overrideExisting)
			{
				if (security == null)
				{
					security = SecurityPermissions.AddNew();
				}

				using (security.SuspendSettingHasChanges())
				{
					security.GU_SecurityItemIsAllowed = false;
					security.GU_SecurityRight = securityRight;
					security.ChangeOthersRightsNeedsValidation = false;
					security.HasRightsToChangeOthers = true; //since, well, it's here by default, it shouldn't ever be denied
				}
			}
		}

		public void ResetGroupPermissions()
		{
			SecurityPermissions.RemoveAndDeleteAll();
			DefaultPermissionsForNewGroup(true);
		}

		#endregion

		#endregion

		#region OnLoaded

		public override void OnLoaded()
		{
			base.OnLoaded();
			isGlobal = GG_GC.IsEmpty;
		}

		#endregion

		#region Current Group Link

		public GlbGroupLink CurrentGroupLink
		{
			get { return fCurrentGroupLink; }
			set { fCurrentGroupLink = value; }
		}

		GlbGroupLink fCurrentGroupLink;

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return base.ShouldCreateAutoLogIfOnlyChildrenHaveChanges && !GlbSecurity.IsSecurityModificationOnly(this, SecurityPermissions); }
		}

		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges
		{
			get { return base.ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges && !GlbSecurity.IsSecurityModificationOnly(this, SecurityPermissions); }
		}

		#endregion

		#region Properties

		#region IsGlobal

		public ZBool IsGlobal
		{
			get { return isGlobal; }
			set
			{
				if (isGlobal != value)
				{
					SetNonPersistentPropertyValue(IsGlobalInfo, ref isGlobal, value);
					if (value)
					{
						GG_GC = ZGuid.Empty;
					}
					else if (!IsValidationSuspended)
					{
						Validation.ValidateGG_GC();
					}
				}
			}
		}
		ZBool isGlobal;

		public ZPropertyInfo IsGlobalInfo
		{
			get { return GetZPropertyInfo(Schema.IsGlobal); }
		}

		#endregion

		#region SecurityCompany

		public ZGuid SecurityCompany
		{
			get
			{
				GlbBranch branchForCompany = (GlbBranch)Factory.LoadTop1(typeof(GlbBranch), new ZQuery(GlbBranchSchema.PK, SecurityBranch));
				return branchForCompany != null ? branchForCompany.GB_GC : ZGuid.Empty;
			}
		}

		#endregion

		#region SecurityBranch
		[List("Lookups.Branches")]
		public ZGuid SecurityBranch
		{
			get { return fSecurityBranch; }
			set
			{
				fSecurityBranch = value;
				SecurityBranchInfo.RefreshBinding();
				OnActualSecurityPermissionsChange();
			}
		}

		ZGuid fSecurityBranch;

		public ZPropertyInfo SecurityBranchInfo
		{
			get { return GetZPropertyInfo(nameof(SecurityBranch)); }
		}

		#endregion

		#region SecurityDepartment
		[List("Lookups.Departments")]
		public ZGuid SecurityDepartment
		{
			get { return fSecurityDepartment; }
			set
			{
				fSecurityDepartment = value;
				SecurityDepartmentInfo.RefreshBinding();
				OnActualSecurityPermissionsChange();
			}
		}

		ZGuid fSecurityDepartment;

		public ZPropertyInfo SecurityDepartmentInfo
		{
			get { return GetZPropertyInfo(nameof(SecurityDepartment)); }
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("1c2f86f4-b757-4056-9cfe-627a5a307444", "Group");
				if (!IsDeleted && !GG_Code.IsEmpty)
				{
					result += " (" + GG_Code + ")";
				}
				return result;
			}
		}

		#endregion

		#region Filtering by Branch / Department Event

		public event EventHandler ChangeActualSecurityPermissions;

		public void OnActualSecurityPermissionsChange()
		{
			if (ChangeActualSecurityPermissions != null)
			{
				ChangeActualSecurityPermissions(this, EventArgs.Empty);
			}
		}

		#endregion

		#region IGlbGroup

		IList IGlbGroup.Staff
		{
			get { return Staff; }
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Group);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IReadOnlySecurity

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !GetSecurityCheckpointForReadOnlySecurity().IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		protected virtual SecurityCheckpoint GetSecurityCheckpointForReadOnlySecurity()
		{
			return Env.Security.GroupsModify;
		}

		#endregion

		#region IOrgsAndWarehousesAccessProvider Members

		public GlbSecurityAllowedOrgsAndWarehousesView SecurityAllowedOrgsAndWarehousesView
		{
			get
			{
				if (securityAllowedOrgsAndWarehousesView == null)
				{
					securityAllowedOrgsAndWarehousesView = new GlbSecurityAllowedOrgsAndWarehousesView(SecurityPermissions, this);
					securityAllowedOrgsAndWarehousesView.Rebuild();
				}
				return securityAllowedOrgsAndWarehousesView;
			}
		}

		GlbSecurityAllowedOrgsAndWarehousesView securityAllowedOrgsAndWarehousesView;

		void IOrgsAndWarehousesAccessProvider.AddSecurityToAccessOrgOrWarehouse(string code)
		{
			BusinessObject orgOrWarehouse;
			if (SecurityAllowedOrgsAndWarehousesView.SecurityRight == GlbSecurity.AllowedWarehousesSecurityRightName)
			{
				Type whsWarehouseCollectionType = ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouseCollection>();
				BusinessObjectCollection tempCollection = (BusinessObjectCollection)Activator.CreateInstance(whsWarehouseCollectionType, Factory);
				orgOrWarehouse = Factory.LoadTop1(tempCollection.TypeOfElements, new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, code));
			}
			else
			{
				orgOrWarehouse = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code);
			}

			if (orgOrWarehouse != null)
			{
				ZQuery existingSecurityQuery = new ZQuery(GlbSecuritySchema.GU_ItemGUID, orgOrWarehouse.PK);

				if (SecurityAllowedOrgsAndWarehousesView.Find(existingSecurityQuery).Length == 0)
				{
					GlbSecurity newSecurity = SecurityAllowedOrgsAndWarehousesView.AddNew();
					newSecurity.GU_SecurityItemIsAllowed = true;
					newSecurity.GU_GG = PK;
					newSecurity.GU_ItemGUID = orgOrWarehouse.PK;
					newSecurity.GU_SecurityRight = SecurityAllowedOrgsAndWarehousesView.SecurityRight;
				}
			}
		}

		#endregion

		#region ISupportChangeOthersSecurity Members

		GlbSecurityChangeOthersView securityChangeOthersView;

		IFindBoxListProvider ISupportChangeOthersSecurity.CompleteGroupList
		{
			get { return Lookups.CompleteGroupList; }
		}

		IFindBoxListProvider ISupportChangeOthersSecurity.CompleteStaffList
		{
			get { return Lookups.CompleteStaffList; }
		}

		SchemaGuidColumn ISupportChangeOthersSecurity.GlbSecurityRightHolderColumn
		{
			get { return GlbSecuritySchema.GU_GG; }
		}

		public GlbSecurityChangeOthersView SecurityChangeOthersView
		{
			get { return securityChangeOthersView ?? (securityChangeOthersView = new GlbSecurityChangeOthersView(SecurityPermissions, this, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator)); }
		}

		#endregion

		#region IDocumentSupportable Members

		GlbGroupDocumentSupporter fDocumentSupporter;
		public DocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new GlbGroupDocumentSupporter(this);
				}
				return fDocumentSupporter;
			}
		}

		#endregion

		#region Group security

		GlbGroupSecurityCollection groupSecurity;
		public GlbGroupSecurityCollection GroupSecurity
		{
			get
			{
				if (groupSecurity == null)
				{
					groupSecurity = new GlbGroupSecurityCollection(Factory);
					FillGroupSecurityCollection();
				}
				return groupSecurity;
			}
		}

		protected virtual void FillGroupSecurityCollection()
		{
			var fakeStaff = Factory.New<FakeStaffForGroupForm>();
			using (SuspendSettingHasChanges())
			{
				fakeStaff.Groups.Add(this);
			}

			ISecurityMap securityMap = ObjectFactory.Get<ISecurityMap>();
			foreach (var data in securityMap.GetStaffSecurity(fakeStaff))
			{
				GlbGroupSecurity item = new GlbGroupSecurity();
				item.SecurityRight = data[0];
				item.Summary = data[1];
				GroupSecurity.Add(item);
			}

			using (SuspendSettingHasChanges())
			{
				fakeStaff.Groups.Remove(this);
			}
		}

		public bool IsCurrentUserGroupOwnerForThisGroup
		{
			get
			{
				return Env.Security.FindOrCreateGroupOwnerSecurityCheckpoint(PK.ToGuid()).IsAllowed;
			}
		}

		public GlbSecurityChangeOthersView GroupOwnersForGroupView
		{
			get
			{
				if (fSecurityChangeOthersView == null)
				{
					fSecurityChangeOthersView = new GlbSecurityChangeOthersView(GroupOwnersForThisGroupSecurityPermissions, this, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup);
				}

				return fSecurityChangeOthersView;
			}
		}

		GlbSecurityChangeOthersView fSecurityChangeOthersView;

		[ChildEditable(true)]
		public GlbSecurityCollection GroupOwnersForThisGroupSecurityPermissions
		{
			get
			{
				if (fGroupOwnersForThisGroupSecurityPermissions == null)
				{
					var localGroupOwnersForThisGroupSecurityPermissions = new GlbSecurityCollection(this, Factory);
					ZQuery query = new ZQuery(GlbSecuritySchema.GU_ItemGUID, PK);
					query.AddToFilter(GlbSecuritySchema.GU_SecurityRight, GlbSecurity.GroupOwnerSecurityRightName);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					localGroupOwnersForThisGroupSecurityPermissions.Load(query);
					fGroupOwnersForThisGroupSecurityPermissions = localGroupOwnersForThisGroupSecurityPermissions;
					RegisterEditableChildObject(fGroupOwnersForThisGroupSecurityPermissions);
				}

				return fGroupOwnersForThisGroupSecurityPermissions;
			}
		}
		GlbSecurityCollection fGroupOwnersForThisGroupSecurityPermissions;

		#endregion

		#region Active Directory

		public void SynchroniseWithAD()
		{
			if (!GG_IsSystemDefined)
			{
				if (Env.Security.GroupsModify.IsAllowed)
				{
					CommitADGroupAction(u =>
					{
						ReloadSafe(); // ensure syncing latest data from DB
						u.Synchronise();
					});
				}
				else
				{
					throw new SecurityAccessDeniedException(Env.Security.GetErrorMessageForNotAllowed(Env.Security.GroupsModify));
				}
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("27021d35-f81e-4195-b4ba-77a8a72bc4c7", "Cannot synchronize system-defined group {0}", GG_Code));
			}
		}

		public void DisconnectFromAD()
		{
			if (Env.Security.GroupsModify.IsAllowed)
			{
				GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
				GG_IsActive = false;
			}
			else
			{
				throw new SecurityAccessDeniedException(Env.Security.GetErrorMessageForNotAllowed(Env.Security.GroupsModify));
			}
		}

		void CommitADGroupAction(Action<IADEntity> groupAction)
		{
			var adGroup = GetADGroup();
			groupAction(adGroup);
			adGroup.CommitChanges();
		}

		public bool IsADIntegrationEnabled => ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled;

		public bool IsGroupNameInConflictWithExistingADAccount() => GetADGroup().IsIdentityInConflict();

		bool IADLinkedEntity.IsActive => GG_IsActive;

		ZBool IADLinkedEntity.IsADLinked => IsADLinked;

		bool IADLinkedEntity.IsADLinkable => !GG_IsSystemDefined;

		IADEntity GetADGroup() => ObjectFactory.Get<IADEntityProvider>().GetADGroup(this);

		ZDateTime IADLinkedEntity.SystemCreateTimeUtc => GG_SystemCreateTimeUtc;

		ZDateTime IADLinkedEntity.SystemLastEditTimeUtc => GG_SystemLastEditTimeUtc;

		public bool CanAccessDirectoryEntry()
		{
			try
			{
				return GetADGroup().HasExistingDirectoryEntry();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return false;
			}
		}

		#endregion

		#region Validation

		protected override bool EnableLightValidationIfAvailable => false;

		#endregion

		#region IConversationParticipant

		ZString IConversationParticipant.Language => ZString.Empty;
		ZString IConversationParticipant.Email => ZString.Empty;
		ZString IConversationParticipant.Code => GG_Code;
		ZString IConversationParticipant.Name => GG_Desc;
		ZString IConversationParticipant.JobTitle => string.Empty;
		ZString IConversationParticipant.OrganisationName => string.Empty;
		ZString IConversationParticipant.Location => Company?.FirstActiveBranch?.GB_RL_NKHomePort ?? string.Empty;
		ZBool IConversationParticipant.IsActive => GG_IsActive;
		ZBool IConversationParticipant.IsInternal => true;
		ZString IConversationParticipant.DisplayText => GG_Desc + " (" + GG_Code + ")";
		void IConversationParticipant.CheckCanParticipate(INotifications notifications) { }

		#endregion

		#region Organisation

		[ChildEditable(true)]
		public OrgHeaderCollectionForGlbGroup Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					var localOrganisation = new OrgHeaderCollectionForGlbGroup(this);
					localOrganisation.Load();
					fOrganisation = localOrganisation;
					fOrganisation.SuspendValidation();
					RegisterEditableChildObject(fOrganisation);
				}

				return fOrganisation;
			}
		}
		OrgHeaderCollectionForGlbGroup fOrganisation;

		#endregion

		#region DomainName

		[List("Lookups.DomainNames")]
		[ReadOnlyMember(nameof(GG_DomainNameReadOnly))]
		public override ZString GG_DomainName
		{
			get => base.GG_DomainName;
			set => base.GG_DomainName = value;
		}

		protected bool GG_DomainNameReadOnly => GG_IsSystemDefined || !Env.Security.GroupsModify.IsAllowed;

		[List("Lookups.DomainNames")]
		[ReadOnlyMember(nameof(GG_DomainNameReadOnly))]
		public ZString DomainName
		{
			get => GG_DomainName;
			set => GG_DomainName = value;
		}

		public ZPropertyInfo DomainNameInfo => GG_DomainNameInfo;

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		ProcessTaskCollection workflowItems;

		[ChildEditable]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GlbGroupProcessTaskCollection(this));
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

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.GlbGroupWorkflowDescriptorCode;

		#endregion

		#region IEmailAddressGetterForTrigger

		IEnumerable<string> IEmailAddressGetterForTrigger.GetEmailAddressesFromTriggerParty(string triggerParty)
		{
			switch (triggerParty)
			{
				case MessageRecipientPartyTypeList.Codes.CurrentUser:
					var address = GlbStaff.GetCurrentUser(Factory)?.GS_EmailAddress;

					if (address.HasValue && !string.IsNullOrEmpty(address.Value))
					{
						return new[] { address.Value.ToString() };
					}
					break;

				case MessageRecipientPartyTypeList.Codes.GroupOwners:
					var owners = GroupOwnersForGroupView.Cast<GlbSecurity>().Select(s => s.GU_GS).ToArray();

					if (owners.Any())
					{
						var staff = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, owners));

						return staff.Select(s => s.GS_EmailAddress.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
					}
					break;
			}

			return Enumerable.Empty<string>();
		}

		#endregion

		#region UniversalCopyWithExtendedEntities

		protected void AfterUniversalCopy()
		{
			if (GG_Code == AllStaffGroupCode)
			{
				throw new UniversalCopyAbortException(Res.GetString("91DF397C-6A27-48E6-9B04-1AB98E1A3D69", "There should be only one Group with code ALL. Please check your Copy Template if either new record should be linked to existing Groups instead of copying them, or new group should not copy code."));
			}
		}

		#endregion

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			GG_ExternalId = ZString.Empty;
			GG_Desc = "Of all the groups, etc.";
		}

#endif
		#endregion
	}
}
