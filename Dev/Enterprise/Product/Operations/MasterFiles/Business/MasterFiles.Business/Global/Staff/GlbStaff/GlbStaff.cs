using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Environment;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Business.SpellCheck;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using WTG.Foundation.Cryptography.UserSecrets;
using static Enterprise.MasterFiles.Business.DbUserManager;

namespace Enterprise.MasterFiles.Business
{
	[SingleObjectAroundARow]
	[UserDefinedValues]
	[DescriptionProperty(GlbStaff.Schema.GS_FullName, CanBeReferencedBy = true)]
	[UniversalDataContext(DataContextType.Staff)]
	[DebuggerDisplay("{GetType().Name, nq}: {GS_Code, nq}")]
	public class GlbStaff : AutoGlbStaff,
		IGlbStaff,
		IDocManagerSupport,
		IEDocsSecurity,
		ISupportChangeOthersSecurity,
		IOrgsAndWarehousesAccessProvider,
		IContactable,
		IAddressBookRecipient,
		IDocumentSupportable,
		ICertificatesProvider,
		ICustomFieldProvider,
		IUser,
		IStaff,
		IADLinkedEntity,
		IConversationParticipant,
		ISupportWebAddressValidation,
		IAuditParent,
		IPatternCleanup,
		IWorkflowProvider,
		IStmALogOperationProvider,
		IExamUrlRecipient,
		IGlbPersonPrimarySource,
		IGlbPasswordHistoryParent,
		ICertificatesValidationProvider,
		IExcelPasswordRetrieverForDocumentDelivery,
		IViewStmNumsOwner
	{
		#region Schema

		public abstract new class Schema : AutoGlbStaff.Schema
		{
			public const string DateOfReview = "DateOfReview";
			public const string GS_LastLogin = "GS_LastLogin";
			public const string GS_LastLogout = "GS_LastLogout";
			public const string MembershipType = "MembershipType";
			public const string SecurityBranch = "SecurityBranch";
			public const string SecurityDepartment = "SecurityDepartment";
			public const string StaffPlainTextPassword = "StaffPlainTextPassword";
			public const string StaffConfirmPassword = "StaffConfirmPassword";
			public const string StaffPlainTextPasswordForImportOnly = "StaffPlainTextPasswordForImportOnly";
			public const string UseTransactionCompanyAsPreferredPayment = "UseTransactionCompanyAsPreferredPayment";
			public const string GS_HomePhone_Formatted = "GS_HomePhone_Formatted";
			public const string GS_HomePhone_FormattedLocalNumberIfLoggedInSameCountry = "GS_HomePhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_HomePhone_IsManuallyVerified = "GS_HomePhone_IsManuallyVerified";
			public const string GS_WorkPhone_Formatted = "GS_WorkPhone_Formatted";
			public const string GS_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry = "GS_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_WorkPhone_IsManuallyVerified = "GS_WorkPhone_IsManuallyVerified";
			public const string GS_MobilePhone_Formatted = "GS_MobilePhone_Formatted";
			public const string GS_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry = "GS_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_MobilePhone_IsManuallyVerified = "GS_MobilePhone_IsManuallyVerified";
			public const string GS_FaxNum_Formatted = "GS_FaxNum_Formatted";
			public const string GS_FaxNum_FormattedLocalNumberIfLoggedInSameCountry = "GS_FaxNum_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_FaxNum_IsManuallyVerified = "GS_FaxNum_IsManuallyVerified";
			public const string GS_Pager_Formatted = "GS_Pager_Formatted";
			public const string GS_Pager_FormattedLocalNumberIfLoggedInSameCountry = "GS_Pager_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_Pager_IsManuallyVerified = "GS_Pager_IsManuallyVerified";
			public const string GS_NextOfKinHomePhone_Formatted = "GS_NextOfKinHomePhone_Formatted";
			public const string GS_NextOfKinHomePhone_FormattedLocalNumberIfLoggedInSameCountry = "GS_NextOfKinHomePhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_NextOfKinHomePhone_IsManuallyVerified = "GS_NextOfKinHomePhone_IsManuallyVerified";
			public const string GS_NextOfKinWorkPhone_Formatted = "GS_NextOfKinWorkPhone_Formatted";
			public const string GS_NextOfKinWorkPhone_FormattedLocalNumberIfLoggedInSameCountry = "GS_NextOfKinWorkPhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_NextOfKinWorkPhone_IsManuallyVerified = "GS_NextOfKinWorkPhone_IsManuallyVerified";

			public const string GS_EmergencyHomePhone_Formatted = "GS_EmergencyHomePhone_Formatted";
			public const string GS_EmergencyHomePhone_FormattedLocalNumberIfLoggedInSameCountry = "GS_EmergencyHomePhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_EmergencyHomePhone_IsManuallyVerified = "GS_EmergencyHomePhone_IsManuallyVerified";
			public const string GS_EmergencyWorkPhone_Formatted = "GS_EmergencyWorkPhone_Formatted";
			public const string GS_EmergencyWorkPhone_FormattedLocalNumberIfLoggedInSameCountry = "GS_EmergencyWorkPhone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string GS_EmergencyWorkPhone_IsManuallyVerified = "GS_EmergencyWorkPhone_IsManuallyVerified";

			public const string CurrentDRMManager = "CurrentDRMManager";
			public const string IsADLinked = "IsADLinked";

			public const int UserPasswordPlainTextMaxLength = 35;
			public const int GS_SqlLoginPasswordHashMaxLength = 256;
		}

		#endregion

		#region LocalPasswordMustBeReset

		[ResourceStringData("GlbStaff.LocalPasswordMustBeReset", Caption = "Reset Local Password")]
		[ReadOnlyMember(nameof(IsADIntegrationEnabled))]
		public ZBool LocalPasswordMustBeReset
		{
			get => GS_PasswordHash.IsEmpty && GS_PasswordSalt.IsEmpty && GS_PasswordHashIterations == ZInt.Zero;

			set
			{
				AddPasswordToHistory(null); // Keep the current password to history before override
				if (value)
				{
					GS_PasswordHash = ZBlob.Empty;
					GS_PasswordHashIterations = ZInt.Zero;
					GS_PasswordSalt = ZBlob.Empty;
				}
				else
				{
					SetLocalPasswordHash(Guid.NewGuid().ToString());
				}
			}
		}

		public ZPropertyInfo LocalPasswordMustBeResetInfo => GetWrappedZPropertyInfo(nameof(LocalPasswordMustBeReset), x => GS_PasswordHashInfo);

		bool IUser.LocalPasswordMustBeReset => LocalPasswordMustBeReset;

		#endregion

		bool IEquatable<IUser>.Equals(IUser other)
		{
			return this.PK.Equals(other.PK);
		}

		public static GlbStaff New(BusinessObjectFactory factory)
		{
			return factory.New<GlbStaff>();
		}

		public GlbStaff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GS_LastActivityDate), ConcurrencyPolicy.Ignore);
		}

		#region Load From LoginName

		/// <summary>
		/// Load staff by login name. Login name can optionally contain domain when separated by @, eg. username@domain
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="loginName"></param>
		/// <returns>Staff that matches the login name</returns>
		/// <exception cref="UserExistsInMultipleDomainsException">
		/// If the loginName returns multiple staffs.
		/// </exceptions>
		public static GlbStaff LoadFromLoginName(BusinessObjectFactory factory, string loginName)
		{
			if (!string.IsNullOrEmpty(loginName))
			{
				var users = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, loginName) { ReLoadExistingRows = true });
				if (users.Length == 1)
				{
					return users[0];
				}
				else if (users.Length > 1)
				{
					// the loginName returns more than one Staff
					throw new UserExistsInMultipleDomainsException(GetUserExistsInMultipleDomains(loginName, users));
				}
				else
				{
					// the loginName returns no matching staff, so we try to break it to username and domain
					var domainName = GetDomainNameComponent(loginName);
					if (!domainName.IsNullOrEmpty())
					{
						users = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, GetUserNameComponent(loginName)).AddToFilter(GlbStaffSchema.GS_DomainName, domainName));
						if (users.Length == 1)
						{
							return users[0];
						}
					}
				}
			}

			return null;
		}

		static string GetUserExistsInMultipleDomains(string loginName, GlbStaff[] staffs)
		{
			return Res.GetString("baa48826-dfef-4f00-8200-960946c902d1", @"User '{0}' exists in multiple domains, please enter one of the following:
{1}", loginName, string.Join("," + System.Environment.NewLine, staffs.Select(s => s.GS_LoginName + '@' + s.DomainName)));
		}

		static string GetUserNameComponent(string loginNameWithDomain)
		{
			var index = loginNameWithDomain.LastIndexOf('@');
			if (index >= 0)
			{
				return loginNameWithDomain.Substring(0, index);
			}
			return loginNameWithDomain;
		}

		static string GetDomainNameComponent(string loginNameWithDomain)
		{
			var index = loginNameWithDomain.LastIndexOf('@');
			if (index >= 0)
			{
				return loginNameWithDomain.Substring(index + 1);
			}
			return string.Empty;
		}

		#endregion

		#region Deleting

		public override void Delete()
		{
			var downLevelLogonName = GetDownLevelLogonName();
			var dbSupportsWindowsAuthentication = new DbUserManager().DbSupportsWindowsAuthentication(GetDownLevelLogonName);

			DeleteAllGlbGroupLinks();

			for (int i = AllGroups.Count - 1; i >= 0; --i)
			{
				AllGroups.Remove(AllGroups[i], true);
			}

			NumberRangeMatchingDetails.DeleteAll();
			Fountains.DeleteAll();
			Campaigns.DeleteAll();
			ReleaseNotesRead.RemoveAndDeleteAll();
			LanguageSkills.DeleteAll();
			Holidays.RemoveAndDeleteAll();
			TimeAllocations.RemoveAndDeleteAll();
			Certificates.DeleteAll();
			DirectReports.Where(x => x.GSM_GS_Staff != this.PK).DeleteAll();
			Managers.DeleteAll();
			WorkTimes.DeleteAll();
			this.DeleteChildren<GlbExternalPassword>(GlbExternalPasswordSchema.GP_GS);
			DeleteUnusedStmData();
			DeleteUnusedStmDefaultPrinter();
			CapabilityPivots.DeleteAll();
			AddOnRuleAcks.DeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			PatternMatchingRemover.DeleteAll(this);
			DeleteDictionary();
			EmailAddresses.DeleteAll();

			bool isResource = GS_IsResource;
			DeleteAllPatterns();

			base.Delete();

			SetRequiresDSANudge();
			CreateDBStaffLoginInfoIfNeeded(isResource, () => downLevelLogonName);
			ApplyStaffChangesToDbLogins(dbSupportsWindowsAuthentication);
		}

		protected override void OnSavedForDeletedObject(bool saveSucceeded)
		{
			base.OnSavedForDeletedObject(saveSucceeded);
			if (saveSucceeded && requiresDSANudge)
			{
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("DSA");
			}
		}

		void DeleteDictionary()
			=> Factory.Load<WordDictionary>(new ZQuery(WordDictionarySchema.DIC_GS_NKStaff, GS_Code)).ForEach(b => b.Delete());

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

		void DeleteAllGlbGroupLinks()
		{
			var databaseAccessGlbGroupLinks = Factory.Load<GlbGroupLink>(GetGlbGroupLinkDatabaseAccessQuery());
			databaseAccessGroupPKsOriginalValues = databaseAccessGlbGroupLinks.Select(ggl => ggl.GK_GG.ToGuid()).ToArray();

			if (databaseAccessGlbGroupLinks != null && databaseAccessGlbGroupLinks.Length > 0)
			{
				databaseAccessGlbGroupLinks.ForEach(g =>
				{
					if (!g.IsDeleted)
					{
						g.Delete();
					}
				});
			}

			if (databaseAccessGroupPKs != null)
			{
				databaseAccessGroupPKs.Clear();
			}
		}

		#endregion

		#region Default Values + Loading

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SecurityBranch = GlbBranch.CurrentBranch == null ? ZGuid.Empty : GlbBranch.CurrentBranch.PK;
			SecurityDepartment = GlbDepartment.CurrentDepartment == null ? ZGuid.Empty : GlbDepartment.CurrentDepartment.PK;

			GS_LastPasswordChangeDate = ZDateTime.Today;
			GS_ChangePasswordAtNextLogin = true;
			GS_PublishEmailAddress = true;
			UseTransactionCompanyAsPreferredPayment = true;

			if (IsADIntegrationEnabled)
			{
				GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			}

			base.GS_RN_NKCountryCode = GlbBranch.CurrentBranch == null ? ZString.Empty : GlbBranch.CurrentBranch.GB_RN_NKCountryCode;

			//Make sure to also set 'empty', as the row might still be null (as we read null as empty)
			GS_GeoLocation = ZGeography.Empty;
		}

		public bool SaturdayIsWorkingDay
		{
			get
			{
				GlbCompany company = GlbCompany.CurrentCompany;
				string countryCode = company?.Country.Code;

				return countryCode == Core.Constants.CountryCodes.Singapore ||
					countryCode == Core.Constants.CountryCodes.Thailand ||
					countryCode == Core.Constants.CountryCodes.Indonesia ||
					countryCode == Core.Constants.CountryCodes.Malaysia ||
					countryCode == Core.Constants.CountryCodes.HongKong ||
					countryCode == Core.Constants.CountryCodes.Cameroon ||
					countryCode == Core.Constants.CountryCodes.Philippines ||
					countryCode == Core.Constants.CountryCodes.India ||
					countryCode == Core.Constants.CountryCodes.Fiji ||
					countryCode == Core.Constants.CountryCodes.Japan ||
					countryCode == Core.Constants.CountryCodes.Myanmar;
			}
		}

		bool IsEmergencyDetailsSameAsNextOfKin
		{
			get
			{
				return GS_EmergencyContactName.Length > 0 &&
					GS_EmergencyContactName == GS_NextOfKin &&
					GS_EmergencyHomePhone == GS_NextOfKinHomePhone &&
					GS_EmergencyWorkPhone == GS_NextOfKinWorkPhone &&
					GS_EmergencyContactRelationship == GS_NextOfKinRelationship;
			}
		}

		protected virtual void AddUserToAllUsersGroup(BusinessObjectCollection groups)
		{
			if (GS_IsActive)
			{
				GlbGroup allUsers = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
				if (allUsers != null && !groups.Contains(allUsers))
				{
					using (allUsers.SuspendSettingHasChanges())
					{
						if (!Env.IsLoggedIn)
						{
							using (groups.SuspendCountChanged(null))
							{
								groups.Add(allUsers);
							}
						}
						else
						{
							groups.Add(allUsers);
						}
					}
				}
			}
		}

		protected virtual void RemoveUserFromAllUsersGroup(GroupCollectionView groups)
		{
			if (!GS_IsActive)
			{
				GlbGroup allUsers = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
				if (allUsers != null && groups.Contains(allUsers))
				{
					using (allUsers.SuspendSettingHasChanges())
					{
						groups.Remove(allUsers);
					}
				}
			}
		}

		void RemoveUserFromDatabaseAccessGroups(GroupCollectionView groups)
		{
			if (!GS_IsActive)
			{
				IsDatabaseDeveloper = false;
				IsReadOnlyDBUser = false;
				IsBackupOperator = false;

				var databaseAccessGroups = Factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.PK, SQLComparisonOperator.Equal, new Guid[] { GlbGroup.DbDeveloperGroupPK, GlbGroup.DbReaderGroupPK, GlbGroup.BackupOperatorGroupPK }));
				foreach (var databaseAccessGroup in databaseAccessGroups)
				{
					using (databaseAccessGroup.SuspendSettingHasChanges())
					{
						groups.Remove(databaseAccessGroup);
					}
				}
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			useTransactionCompanyAsPreferredPayment = GS_GC_PreferredPaymentCompany.IsEmpty;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			GS_SAMAccountFullName = GS_SAMAccountFullName.SubstringSafe(PK.ToString().Length) + PK.ToString();
			GS_ExternalId = ZString.Empty;
			GS_GB_HomeBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "DEM").PK;
			GS_GE_HomeDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA").PK;

			var reportingRoles = SystemDataRegistry.Instance.StaffReportingRoles.Value.ToArray<StaffReportingRole>();
			foreach (var role in reportingRoles.Where(x => x.IsMandatory))
			{
				var manager = Factory.New<GlbStaffManager>();
				manager.GSM_GS_Staff = this.PK;
				manager.GSM_GS_Manager = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "E").PK;
				manager.GSM_EffectiveDate = ZDateTime.Today;
				manager.GSM_ManagerType = role.Code;
			}

			WorkTimes.AddValidTestData();
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			public MyBusinessObjectTestDataHelper()
			{
			}

			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if (collectionProperty.Name != nameof(GlbStaff.NumberRangeMatchingDetails))
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}
		}

		public void ResetBranchAndDepartment()
		{
			GS_GB_HomeBranch = ZGuid.Empty;
			GS_GE_HomeDepartment = ZGuid.Empty;
		}
#endif

		#endregion

		#region StmFeatures

		public bool CheckFeatureEnabledForUser(string featureCode)
		{
			if (!DataRegistry.Instance.FeatureTestModeEnabled)
			{
				return false;
			}

			var collection = new DynamicBusinessObjectCollection(Factory);
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@StmFeatureName", featureCode, StmFeatureTestSchema.SFT_FeatureName);
			sqlParams.Add("@UserId", PK, GlbStaffSchema.PK);
			var query = $@"SELECT TOP 1 SFT_FeatureName FROM {nameof(StmFeatureTest)} sft
						INNER JOIN {nameof(GlbGroup)} glb
						ON sft.SFT_GG_Group = glb.GG_PK
						INNER JOIN {nameof(GlbGroupLink)} lnk
						ON SFT.SFT_GG_Group = lnk.GK_GG
						WHERE SFT.SFT_FeatureName = @StmFeatureName
						AND SFT.SFT_IsActive = 1
						AND lnk.GK_GS = @UserId;";

			collection.Load(query, sqlParams);

			return collection.Count > 0;
		}

		public bool CheckWinzorEnabledForUser()
		{
			return CheckFeatureEnabledForUser(StmFeatureTest.WinzorFeatureCode);
		}

		public bool CheckWebVersionEnforcedForUser()
		{
			return CheckFeatureEnabledForUser(StmFeatureTest.WebVersion);
		}

		public bool CheckForceCW1HomeScreenEnabledForUser()
		{
			return CheckFeatureEnabledForUser(StmFeatureTest.ForceCW1HomeScreen);
		}

		public bool CheckNetCoreVersionEnforcedForUser()
		{
			return CheckFeatureEnabledForUser(StmFeatureTest.NetCoreVersion);
		}

		#endregion

		#region Properties

		#region Scim

		public bool IsControlledByScim
		{
			get
			{
				return !GS_ExternalId.IsEmpty && !SystemDataRegistry.Instance.ScimAllowLocalEditing.Value;
			}
		}

		bool IsPropertyMappedForScim(ZString columnName)
		{
			return StaffScimHelper.IsPropertyMappedForScim(columnName);
		}

		StaffScimHelper staffScimHelper;
		StaffScimHelper StaffScimHelper
		{
			get
			{
				if (staffScimHelper == null)
				{
					staffScimHelper = new StaffScimHelper(this);
				}

				return staffScimHelper;
			}
		}

		#endregion

		#region GS_ActiveDirectoryObjectGuid

		public override ZGuid GS_ActiveDirectoryObjectGuid
		{
			get { return base.GS_ActiveDirectoryObjectGuid; }
			set
			{
				base.GS_ActiveDirectoryObjectGuid = value;
				if (IsADIntegrationEnabled && value.IsValid)
				{
					//Reset local password only when it is linking to a valid AD object
					LocalPasswordMustBeReset = true;
				}
				IsADLinkedInfo.RefreshBinding();
			}
		}

		#endregion

		#region GS_Birthdate

		public override ZDate GS_Birthdate
		{
			get { return ViewingRights.ViewBirthDateAllowedForStaff ? base.GS_Birthdate : ZDate.Empty; }
			set
			{
				if (base.GS_Birthdate != value)
				{
					base.GS_Birthdate = value;
					Person?.FindDuplicates(this);
				}
			}
		}

		protected bool GS_Birthdate_ReadOnly
		{
			get { return !ViewingRights.ViewBirthDateAllowedForStaff; }
		}

		#endregion

		#region GS_BrokerID

		public override ZString GS_BrokerID
		{
			get { return ViewingRights.ViewBrokerInfoAllowedForStaff ? base.GS_BrokerID : ViewDeniedMessage; }
			set { base.GS_BrokerID = value; }
		}

		protected bool GS_BrokerID_ReadOnly
		{
			get { return !ViewingRights.ViewBrokerInfoAllowedForStaff; }
		}

		#endregion

		#region GS_BrokerPasswordStatus

		public override ZString GS_BrokerPasswordStatus
		{
			get { return ViewingRights.ViewBrokerInfoAllowedForStaff ? base.GS_BrokerPasswordStatus : ViewDeniedMessage; }
			set { base.GS_BrokerPasswordStatus = value; }
		}

		protected bool GS_BrokerPasswordStatus_ReadOnly
		{
			get { return !ViewingRights.ViewBrokerInfoAllowedForStaff; }
		}

		#endregion

		#region GS_BrokerPassword

		[DocumentMacroIgnore]
		public override ZString GS_BrokerPassword
		{
			get { return base.GS_BrokerPassword; }
			set { base.GS_BrokerPassword = value; }
		}

		#endregion

		#region GS_BrokerWorkingPassword

		[DocumentMacroIgnore]
		public override ZString GS_BrokerWorkingPassword
		{
			get { return base.GS_BrokerWorkingPassword; }
			set { base.GS_BrokerWorkingPassword = value; }
		}

		#endregion

		#region GS_PersonalEDIMailBox

		public override ZString GS_PersonalEDIMailBox
		{
			get { return ViewingRights.ViewPersonalEDIMailBoxAllowedForStaff ? base.GS_PersonalEDIMailBox : ViewDeniedMessage; }
			set { base.GS_PersonalEDIMailBox = value; }
		}

		protected bool GS_PersonalEDIMailBox_ReadOnly
		{
			get { return !ViewingRights.ViewPersonalEDIMailBoxAllowedForStaff; }
		}

		#endregion

		#region GS_ResidencyExpiry

		public override ZDateTime GS_ResidencyExpiry
		{
			get { return ViewingRights.ViewResidencyStatusAllowedForStaff ? base.GS_ResidencyExpiry : ZDateTime.Empty; }
			set { base.GS_ResidencyExpiry = value; }
		}

		protected bool GS_ResidencyExpiry_ReadOnly
		{
			get { return !ViewingRights.ViewResidencyStatusAllowedForStaff; }
		}

		#endregion

		#region GS_ResidencyStatus

		public override ZString GS_ResidencyStatus
		{
			get { return ViewingRights.ViewResidencyStatusAllowedForStaff ? base.GS_ResidencyStatus : ViewDeniedMessage; }
			set { base.GS_ResidencyStatus = value; }
		}

		protected bool GS_ResidencyStatus_ReadOnly
		{
			get { return !ViewingRights.ViewResidencyStatusAllowedForStaff; }
		}

		#endregion

		#region GS_SecurityCardNumber

		public override ZString GS_SecurityCardNumber
		{
			get { return ViewingRights.ViewSecurityCardNumberAllowedForStaff ? base.GS_SecurityCardNumber : ViewDeniedMessage; }
			set { base.GS_SecurityCardNumber = value; }
		}

		protected bool GS_SecurityCardNumber_ReadOnly
		{
			get { return !ViewingRights.ViewSecurityCardNumberAllowedForStaff; }
		}

		#endregion

		#region GS_Code

		protected bool GS_Code_ReadOnly
		{
			get
			{
				return IsInDatabase || NoRightsExist;
			}
		}

		#endregion

		#region GS_CommissionBasis
		[List("Lookups.CommissionBasisTypes")]
		public override ZString GS_CommissionBasis
		{
			get
			{
				return base.GS_CommissionBasis;
			}
			set
			{
				base.GS_CommissionBasis = value;
			}
		}
		#endregion

		#region GS_EmailAddress
		[EmailAddress]
		[ReadOnlyMember(nameof(IsControlledByScim))]
		public override ZString GS_EmailAddress
		{
			get
			{
				return base.GS_EmailAddress;
			}
			set
			{
				if (base.GS_EmailAddress != value)
				{
					base.GS_EmailAddress = value;
					Person?.FindDuplicates(this);
				}
			}
		}
		#endregion

		#region GS_EmploymentBasis

		[BusinessObjectTestExclude]
		[List("Lookups.StaffEmploymentTypes")]
		public override ZString GS_EmploymentBasis
		{
			get => base.GS_EmploymentBasis;
			set => base.GS_EmploymentBasis = value;
		}

		#endregion

		#region GS_Title

		public override ZString GS_Title
		{
			get => ViewingRights.ViewTitleAllowedForStaff ? base.GS_Title : ViewDeniedMessage;
			set
			{
				if (GS_Title == value)
				{
					return;
				}

				base.GS_Title = value;
			}
		}

		protected bool GS_Title_ReadOnly
		{
			get { return !ViewingRights.ViewTitleAllowedForStaff; }
		}

		#endregion

		#region GS_GB_HomeBranch

		[List("Lookups.Branches")]
		public override ZGuid GS_GB_HomeBranch
		{
			get { return base.GS_GB_HomeBranch; }
			set { base.GS_GB_HomeBranch = value; }
		}

		public ZString BranchDescription
		{
			get
			{
				var branch = HomeBranch;
				return branch != null ? branch.GB_BranchName : ZString.Empty;
			}
		}

		public ZString BranchCountry
		{
			get
			{
				var branch = HomeBranch;
				return branch != null ? branch.GB_RN_NKCountryCode : ZString.Empty;
			}
		}

		#endregion

		#region GS_GC_PreferredPaymentCompany

		protected bool GS_GC_PreferredPaymentCompany_ReadOnly
		{
			get { return UseTransactionCompanyAsPreferredPayment; }
		}

		#region UseTransactionCompanyAsPreferredPayment

		public ZBool UseTransactionCompanyAsPreferredPayment
		{
			get { return useTransactionCompanyAsPreferredPayment; }
			set
			{
				if (useTransactionCompanyAsPreferredPayment != value)
				{
					SetNonPersistentPropertyValue(UseTransactionCompanyAsPreferredPaymentInfo, ref useTransactionCompanyAsPreferredPayment, value);
					if (value)
					{
						GS_GC_PreferredPaymentCompany = ZGuid.Empty;
					}
					else if (!IsValidationSuspended)
					{
						Validation.ValidateGS_GC_PreferredPaymentCompany();
					}
				}
			}
		}
		ZBool useTransactionCompanyAsPreferredPayment;

		public ZPropertyInfo UseTransactionCompanyAsPreferredPaymentInfo
		{
			get { return GetZPropertyInfo(Schema.UseTransactionCompanyAsPreferredPayment); }
		}

		#endregion

		#region PreferredPaymentCompanyName

		public ZString PreferredPaymentCompanyName
		{
			get
			{
				var preferredPaymentCompany = PreferredPaymentCompany;
				return preferredPaymentCompany != null ? preferredPaymentCompany.GC_Name : ZString.Empty;
			}
		}

		public ZPropertyInfo PreferredPaymentCompanyNameInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PreferredPaymentCompanyName), x => GS_GC_PreferredPaymentCompanyInfo); }
		}

		#endregion

		#endregion

		#region GS_GE_HomeDepartment

		[List("Lookups.Departments")]
		public override ZGuid GS_GE_HomeDepartment
		{
			get { return base.GS_GE_HomeDepartment; }
			set { base.GS_GE_HomeDepartment = value; }
		}

		#endregion

		#region GS_Gender

		[List("Lookups.Gender")]
		public override ZString GS_Gender
		{
			get { return ViewingRights.ViewGenderDetailsAllowedForStaff ? base.GS_Gender : ViewDeniedMessage; }
			set
			{
				GS_GenderCustomTerm = string.Empty;
				GS_GenderCustomTermInfo.RefreshBinding();
				base.GS_Gender = value;
			}
		}

		protected bool GS_Gender_ReadOnly
		{
			get { return !ViewingRights.ViewGenderDetailsAllowedForStaff; }
		}

		#endregion

		#region GS_IsActive

		[ReadOnlyMember(nameof(GS_IsActiveReadOnly))]
		public override ZBool GS_IsActive
		{
			get { return base.GS_IsActive; }
			set
			{
				if (base.GS_IsActive != value)
				{
					base.GS_IsActive = value;

					if (value)
					{
						AddUserToAllUsersGroup(Groups);

						if (IsADIntegrationEnabled &&
								(GS_ActiveDirectoryObjectGuid == ZGuid.Empty ||
								(GS_ActiveDirectoryObjectGuid.IsValid && !CanAccessDirectoryEntry())
							))
						{
							GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
						}

						scheduledReportDescriptionAssignedToPrintUserCache = null;
					}
					else
					{
						RemoveCurrentManagers();
						RemoveUserFromAllUsersGroup(Groups);
						RemoveUserFromDatabaseAccessGroups(Groups);
						if (IsADIntegrationEnabled && (ObjectFactory.Get<IADRegistry>().ShouldUnlinkInactiveStaff || !GS_ActiveDirectoryObjectGuid.IsValid))
						{
							GS_ActiveDirectoryObjectGuid = Guid.Empty;
						}
					}
				}

				Validation.ValidateGS_GB_HomeBranch();
			}
		}

		public bool GS_IsActiveReadOnly => NoRightsExist || (GS_Code.EqualsIgnoringCase("SUP") && !User.SupportUserName.Equals(Env.CurrentUser.LoginName, StringComparison.OrdinalIgnoreCase)) || IsControlledByScim;

		#endregion

		#region GS_IsDeveloper

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZBool GS_IsDeveloper
		{
			get { return base.GS_IsDeveloper; }
			set { base.GS_IsDeveloper = value; }
		}

		#endregion

		#region GS_IsSystemAccount

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZBool GS_IsSystemAccount
		{
			get { return base.GS_IsSystemAccount; }
			set { base.GS_IsSystemAccount = value; }
		}

		#endregion

		#region GS_IsSalesRep

		public override ZBool GS_IsSalesRep
		{
			get { return base.GS_IsSalesRep; }
			set
			{
				base.GS_IsSalesRep = value;
				if (!GS_IsSalesRep)
				{
					GS_CommissionBasis = ZString.Empty;
				}
				if (SalesRepStatusChanged != null)
				{
					SalesRepStatusChanged();
				}

				StaffScimHelper.SetScimGroupIfRequired(value, GS_IsSalesRepInfo);
			}
		}

		protected bool GS_IsSalesRep_ReadOnly
		{
			get
			{
				return IsControlledByScim && IsPropertyMappedForScim(GlbStaffSchema.Constants.GS_IsSalesRep);
			}
		}

		#endregion

		#region GS_IsResource

		public override ZBool GS_IsResource
		{
			get { return base.GS_IsResource; }
			set
			{
				base.GS_IsResource = value;
				if (value)
				{
					GS_LoginName = new ZString(PK.ToString()).SubstringSafe(0, GS_LoginNameInfo.MaxLength);
				}
				else
				{
					GS_LoginName = ZString.Empty;
				}
			}
		}

		#endregion

		#region GS_CanLogin

		public override ZBool GS_CanLogin
		{
			get => base.GS_CanLogin;
			set
			{
				base.GS_CanLogin = value;
				StaffScimHelper.SetScimGroupIfRequired(value, GS_CanLoginInfo);
			}
		}

		protected bool GS_CanLogin_ReadOnly
		{
			get
			{
				return IsControlledByScim && IsPropertyMappedForScim(GlbStaffSchema.Constants.GS_CanLogin);
			}
		}

		#endregion

		#region GS_IsRobot

		public override ZBool GS_IsRobot
		{
			get => base.GS_IsRobot;
			set
			{
				base.GS_IsRobot = value;
				StaffScimHelper.SetScimGroupIfRequired(value, GS_IsRobotInfo);
			}
		}

		protected bool GS_IsRobot_ReadOnly
		{
			get
			{
				return IsControlledByScim && IsPropertyMappedForScim(GlbStaffSchema.Constants.GS_IsRobot);
			}
		}

		#endregion

		#region GS_IsDevice

		public override ZBool GS_IsDevice
		{
			get => base.GS_IsDevice;
			set
			{
				base.GS_IsDevice = value;
				StaffScimHelper.SetScimGroupIfRequired(value, GS_IsDeviceInfo);
			}
		}

		protected bool GS_IsDevice_ReadOnly
		{
			get
			{
				return IsControlledByScim && IsPropertyMappedForScim(GlbStaffSchema.Constants.GS_IsDevice);
			}
		}

		#endregion

		#region GS_IsDriver

		public override ZBool GS_IsDriver
		{
			get => base.GS_IsDriver;
			set
			{
				base.GS_IsDriver = value;
				StaffScimHelper.SetScimGroupIfRequired(value, GS_IsDriverInfo);
			}
		}

		protected bool GS_IsDriver_ReadOnly
		{
			get
			{
				return IsControlledByScim && IsPropertyMappedForScim(GlbStaffSchema.Constants.GS_IsDriver);
			}
		}

		#endregion

		#region IsController

		public override ZBool GS_IsController
		{
			get => base.GS_IsController;
			set
			{
				base.GS_IsController = value;
				StaffScimHelper.SetScimGroupIfRequired(value, GS_IsControllerInfo);
			}
		}

		#endregion

		#region GS_GenderCustomTerm

		public override ZString GS_GenderCustomTerm
		{
			get { return ViewingRights.ViewGenderDetailsAllowedForStaff ? base.GS_GenderCustomTerm : ViewDeniedMessage; }
			set => base.GS_GenderCustomTerm = value;
		}

		public bool GS_GenderCustomTerm_ReadOnly => GS_Gender != "C";

		#endregion

		#region GS_LoginName

		[ReadOnlyMember(nameof(GS_LoginName_ReadOnly))]
		public override ZString GS_LoginName
		{
			get { return GS_IsResource ? ZString.Empty : base.GS_LoginName; }
			set
			{
				if (!GS_LoginNameInfo.Value.Equals(value))
				{
					_ = DatabaseAccessGroupPKs;
				}

				base.GS_LoginName = value;
			}
		}

		public bool GS_LoginName_ReadOnly => IsControlledByScim || NoRightsExist;

		#endregion

		#region GS_RN_NKNationalityCode

		[List("Lookups.NationalityTypes")]
		[ReadOnlyMember(nameof(ViewNationalityNotAllowedForStaff))]
		public override ZString GS_RN_NKNationalityCode
		{
			get { return ViewingRights.ViewNationalityAllowedForStaff ? base.GS_RN_NKNationalityCode : ViewDeniedMessage; }
			set
			{
				if (base.GS_RN_NKNationalityCode != value)
				{
					base.GS_RN_NKNationalityCode = value;
					Person?.FindDuplicates(this);
				}
			}
		}

		protected bool ViewNationalityNotAllowedForStaff
		{
			get { return !ViewingRights.ViewNationalityAllowedForStaff; }
		}

		#endregion

		#region GS_ResourceType
		[List("Lookups.ResourceTypes")]
		public override ZString GS_ResourceType
		{
			get
			{
				return base.GS_ResourceType;
			}
			set
			{
				base.GS_ResourceType = value;
			}
		}
		#endregion

		#region GS_WorkingLanguage

		[List("Lookups.WorkingLanguages")]
		[ReadOnlyMember(nameof(IsControlledByScim))]
		public override ZString GS_WorkingLanguage
		{
			get { return base.GS_WorkingLanguage; }
			set
			{
				base.GS_WorkingLanguage = value;
				if (IsCurrentUser)
				{
					Res.NotifyChange();
				}
			}
		}

		#endregion

		#region GS_ValidationStatus

		public override ZString GS_ValidationStatus
		{
			get => base.GS_ValidationStatus;

			set
			{
				base.GS_ValidationStatus = value;
				isManuallyVerifiedByUser = value == AddressValidationStatus.ManuallyVerified;
			}
		}

		#endregion

		#region GS_FullName

		[ReadOnlyMember(nameof(IsControlledByScim))]
		public override ZString GS_FullName
		{
			get => base.GS_FullName;

			set
			{
				base.GS_FullName = value;
				Person?.FindDuplicates(this);
			}
		}

		#endregion

		#region GS_GivenName

		protected bool GS_GivenName_ReadOnly
		{
			get
			{
				return IsControlledByScim;
			}
		}

		#endregion

		#region GS_SurName

		protected bool GS_Surname_ReadOnly
		{
			get
			{
				return IsControlledByScim;
			}
		}

		#endregion

		#region GS_MiddleName

		protected bool GS_MiddleName_ReadOnly
		{
			get
			{
				return IsControlledByScim;
			}
		}

		#endregion

		#region GS_NameTitle

		protected bool GS_NameTitle_ReadOnly
		{
			get
			{
				return IsControlledByScim;
			}
		}

		#endregion

		#region GS_NameSuffix

		protected bool GS_NameSuffix_ReadOnly
		{
			get
			{
				return IsControlledByScim;
			}
		}

		#endregion

		#region GS_FriendlyName

		protected bool GS_FriendlyName_ReadOnly
		{
			get
			{
				return IsControlledByScim;
			}
		}

		#endregion

		#region IsADLinked

		[ResourceStringData("GlbStaff.IsADLinked", Caption = "Is AD Linked", FullDescription = "Indicates whether this staff is linked with an Active Directory user.")]
		public ZBool IsADLinked => GS_ActiveDirectoryObjectGuid.IsValid;

		public ZPropertyInfo IsADLinkedInfo => GetZPropertyInfo(Schema.IsADLinked);

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Leave Status

		public ZDateTime GetFirstAvailableDate(ZDateTime dateToCheck, IWorkTimeArithmetic workTimeArithmetic = null)
		{
			var workTimeArithmeticToUse = workTimeArithmetic ?? WorkTimeArithmetic;
			return workTimeArithmeticToUse.GetNextWorkingDateTime(dateToCheck.ToDateTime()) ?? ZDateTime.Empty;
		}

		public ZDateTime GetFirstDateTimeNotOnHoliday(ZDateTime dateToCheck)
		{
			if (!GS_IsActive)
			{
				return ZDateTime.Empty;
			}

			GlbStaffHoliday holiday = GetHolidayForDate(dateToCheck);
			if (holiday != null && holiday.GA_AvailabilityPercentage == 0)
			{
				return GetFirstDateTimeNotOnHoliday(holiday.GA_EndTime.AddSeconds(1));
			}

			return dateToCheck;
		}

		public ZDate NextWorkingDay(bool includeToday)
		{
			var dateTime = WorkTimeArithmetic.GetNextWorkingDay(ZDate.Today.ToDateTime(), includeToday);
			var resultDateTime = dateTime ?? ZDateTime.Empty;
			return resultDateTime.Date;
		}

		public ZBool IsWorkingToday
		{
			get { return IsWorking(ZDate.Today); }
		}

		public ZBool IsWorkingRightNow
		{
			get
			{
				var useDateTime = HomeBranch != null ? ZDateTime.UtcNow.ToLocationTime(HomeBranch.HomePort).ToZDateTime() : ZDateTime.Now;
				return IsWorking(useDateTime);
			}
		}

		public ZBool IsWorking(ZDate onThisDay)
		{
			return GS_IsActive && WorkTimeArithmetic.IsWorkDay(onThisDay.ToDateTime());
		}

		public ZBool IsWorking(ZDateTime onThisDayAndTime, IWorkTimeArithmetic workTimeArithmetic = null)
		{
			var workTimeArithmeticToUse = workTimeArithmetic ?? WorkTimeArithmetic;
			return
				GS_IsActive
				&& workTimeArithmeticToUse.IsWorkDay(onThisDayAndTime.ToDateTime(), checkStaffHolidays: false, checkBranchHolidays: false)
				&& !workTimeArithmeticToUse.IsDateTimeAHoliday(onThisDayAndTime.ToDateTime());
		}

		public ZBool IsOnLeave
		{
			get
			{
				var useDateTime = HomeBranch != null ? ZDateTime.UtcNow.ToLocationTime(HomeBranch.HomePort).ToZDateTime() : ZDateTime.Now;
				return IsOnLeaveOnDate(useDateTime);
			}
		}

		public ZBool IsOnLeaveOnDate(ZDateTime dateToCheck)
		{
			return CheckHoliday(GetHolidayForDate(dateToCheck));
		}

		public ZBool IsOnLeaveOnDate(ZDate dateToCheck)
		{
			return CheckHoliday(GetHolidayForDate(dateToCheck));
		}

		static ZBool CheckHoliday(GlbStaffHoliday holiday)
		{
			return holiday != null && !holiday.GA_IsWorkingAway && holiday.GA_AvailabilityPercentage == 0;
		}

		GlbStaffHoliday GetHolidayForDate(ZDateTime dateToCheck)
		{
			return HolidaysIncBMSLeave.Cast<GlbStaffHoliday>().FirstOrDefault(holiday =>
				dateToCheck >= holiday.GA_StartTime
				&& dateToCheck <= holiday.GA_EndTime
				&& holiday.IsApproved);
		}

		GlbStaffHoliday GetHolidayForDate(ZDate dateToCheck)
		{
			return HolidaysIncBMSLeave.Cast<GlbStaffHoliday>().FirstOrDefault(holiday =>
				dateToCheck >= holiday.GA_StartTime
				&& dateToCheck.AddDays(1) <= holiday.GA_EndTime
				&& holiday.IsApproved);
		}

		public ZString WorkStatus
		{
			get
			{
				ZString result = Res.GetString("33378fec-9fd2-42fc-9f2b-4575ad8f96d9", "Working");
				if (!GS_IsActive)
				{
					result = Res.GetString("4a6653f0-02a5-4d2a-9891-0086d2d545e2", "Ceased Employment");
				}
				else
				{
					GlbStaffHoliday currentHoliday = GetHolidayForDate(ZDateTime.Now);
					if (HomeBranch != null && HomeBranch.GlbHolidays.Contains(ZDate.Today))
					{
						result = Res.GetString("fbf677da-bd0d-4c67-bf98-f70688b471de", "Branch Holiday");
					}
					else if (currentHoliday != null && currentHoliday.GA_IsWorkingAway)
					{
						result = Res.GetString("897C258F-5162-430B-A954-F449AD26DF5C", "Working Away - {0}", currentHoliday.TypeDescription);
					}
					else if (currentHoliday != null && currentHoliday.GA_AvailabilityPercentage == 0)
					{
						result = Res.GetString("84be20c8-ff5d-4642-998b-b9b4ac8d7346", "On Leave - {0}", currentHoliday.TypeDescription);
					}
					else if (currentHoliday == null && !IsWorkingToday)
					{
						result = Res.GetString("43c97c36-5e53-4761-b847-79930790b955", "Non Work Day");
					}
				}

				return result;
			}
		}

		public ZPropertyInfo WorkStatusInfo
		{
			get { return GetZPropertyInfo(nameof(WorkStatus)); }
		}

		#endregion

		#region IsNotController

		public ZBool IsNotController
		{
			get { return !GS_IsController; }
		}

		#endregion

		#region Is EDI Support Login

		public ZBool IsSupportUser
		{
			get { return GS_LoginName.EqualsIgnoringCase(User.SupportUserName); }
		}

		#endregion

		#region Login Status

		public bool IsLoggedIn
		{
			get
			{
				bool result = false;

				ActiveUsersModuleBusinessObject activeUsers = new ActiveUsersModuleBusinessObject(Factory);
				foreach (ActiveUser user in activeUsers.ActiveUsers)
				{
					if (user.AU_Initials == GS_Code)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public bool HasEverLoggedIn
		{
			get { return new BusinessObjectFactory().LoadTop1<StmALog>(AnyLoginQuery) != null; }
		}

		ZDBOnlyQuery AnyLoginQuery
		{
			get
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(StmALog));
				result.AddToFilter(StmALogSchema.SL_GS_NKUser, GS_CodeInfo.OriginalValue);
				result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Login.Code);
				result.AddToFilter(StmALogSchema.SL_Table, GlbCompanySchema.Constants.TableName);

				ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), StmALogSchema.SL_Parent); // Join dbo.GlbCompany for benefit of index
				result.AddSubQuery(companyQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region CurrentUser

#if DEBUG
		[TestExcludeDetectStaticBusinessObjectsCollectionsAndFactories]
#endif
		public static GlbStaff CurrentUser
		{
			get { return (GlbStaff)Env.CurrentUser; }
		}

		public static GlbStaff GetCurrentUser(BusinessObjectFactory factory)
		{
			var staff = CurrentUser;
			return staff != null && factory != staff.Factory ? factory.Load<GlbStaff>(staff.PK) : staff;
		}

		#endregion

		#region IsCurrentUser

		public bool IsCurrentUser
		{
			get
			{
				var currentUser = Env.CurrentUser;
				return (currentUser != null) && currentUser.PK == PK.ToGuid();
			}
		}

		#endregion

		#region IsCurrentUserLocalAdminForThisStaff

		public bool IsCurrentUserLocalAdminForThisStaff
		{
			get
			{
				bool result = Env.Security.FindOrCreateChangeStaffSecurityCheckpoint(PK.ToGuid()).IsAllowed;
				if (!result)
				{
					foreach (GlbGroup group in Groups)
					{
						if (Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(group.PK.ToGuid()).IsAllowed)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region IsCurrentUserLocalAdminForAtLeastOneGroup

		public bool IsCurrentUserLocalAdminForAtLeastOneGroup
		{
			get
			{
				bool result = false;

				foreach (GlbGroup group in Factory.Load<GlbGroup>(new ZQuery()))
				{
					if (Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(group.PK.ToGuid()).IsAllowed)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		#endregion

		#region StaffMemberBelongsToGroupsLabelText

		public ZString StaffMemberBelongsToGroupsLabelText
		{
			get
			{
				return Res.GetString("GlbStaffForm|53732e40-2e1f-4507-8922-42adaf0499c2", "Staff member belongs to the groups below");
			}
		}

		public ZPropertyInfo StaffMemberBelongsToGroupsLabelTextInfo
		{
			get { return GetZPropertyInfo(nameof(StaffMemberBelongsToGroupsLabelText)); }
		}

		#endregion

		#region StaffMemberIsGroupOwnerLabelText

		public ZString StaffMemberIsGroupOwnerLabelText
		{
			get
			{
				return Res.GetString("GlbStaffForm|b4700fe8-af4d-4035-a2ff-349e72f5daa0", "Staff member is an owner for the groups below");
			}
		}

		#endregion

		#region NeedsLocalAdminGroupToBeAdded

		public bool NeedsLocalAdminGroupToBeAdded
		{
			get { return (!IsInDatabase && !Env.Security.StaffModifyAll.IsAllowed && !IsCurrentUserLocalAdminForThisStaff); }
		}

		#endregion

		#region Images

		public Image SignatureImage
		{
			get { return GetImage(GS_UserSignature); }
			set { SetImage(value, GlbStaffSchema.GS_UserSignature); }
		}

		public Image ProfileImage
		{
			get { return GetImage(GS_ProfilePhoto); }
			set { SetImage(value, GlbStaffSchema.GS_ProfilePhoto); }
		}

		Image GetImage(ZBlob blob)
		{
			Image result = null;
			if (!blob.IsEmpty)
			{
				result = Image.FromStream(new MemoryStream(blob));
			}

			return result;
		}

		void SetImage(Image value, SchemaBinaryColumn blobColumn)
		{
			if (value != null)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					value.Save(stream, value.RawFormat.Equals(ImageFormat.MemoryBmp) ? ImageFormat.Bmp : value.RawFormat);
					this[blobColumn] = stream.ToArray();
				}
			}
			else
			{
				this[blobColumn] = ZBlob.Empty;
			}
		}

		#endregion

		#region Password

		#region GS_PasswordHash

		[DocumentMacroIgnore]
		[ReadOnly(true)]
		public override ZBlob GS_PasswordHash
		{
			get => base.GS_PasswordHash;
			set => base.GS_PasswordHash = value;
		}

		#endregion

		#region GS_PasswordHashIterations
		[DocumentMacroIgnore]
		[ReadOnly(true)]
		public override ZInt GS_PasswordHashIterations
		{
			get => base.GS_PasswordHashIterations;
			set => base.GS_PasswordHashIterations = value;
		}

		#endregion

		#region GS_PasswordSalt

		[DocumentMacroIgnore]
		[ReadOnly(true)]
		public override ZBlob GS_PasswordSalt
		{
			get => base.GS_PasswordSalt;
			set => base.GS_PasswordSalt = value;
		}

		#endregion

		#region Staff Plain Text Password

		ZString fStaffPlainTextPassword;

		[ReadOnlyMember(nameof(IsADIntegrationEnabled))]
		[MaxLength(Schema.UserPasswordPlainTextMaxLength)]
		[Password]
		public ZString StaffPlainTextPassword
		{
			get { return fStaffPlainTextPassword; }
			set
			{
				CheckMaximumLength(StaffPlainTextPasswordInfo, value);
				fStaffPlainTextPassword = value;
				StaffPlainTextPasswordInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateStaffPlainTextPassword();
				}
			}
		}

		public ZPropertyInfo StaffPlainTextPasswordInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.StaffPlainTextPassword); }
		}

		#endregion

		#region StaffConfirmPassword

		ZString fStaffConfirmPassword;

		[ReadOnlyMember(nameof(IsADIntegrationEnabled))]
		[MaxLength(Schema.UserPasswordPlainTextMaxLength)]
		[Password]
		public ZString StaffConfirmPassword
		{
			get { return fStaffConfirmPassword; }
			set
			{
				CheckMaximumLength(StaffConfirmPasswordInfo, value);
				fStaffConfirmPassword = value;
				StaffConfirmPasswordInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateStaffConfirmPassword();
				}
			}
		}

		public ZPropertyInfo StaffConfirmPasswordInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.StaffConfirmPassword); }
		}

		#endregion

		#region StaffPlainTextPasswordForImportOnly

		[ResourceStringData("GlbStaff|StaffPlainTextPasswordForImportOnly", Caption = "Password")]
		[Password]
		public ZString StaffPlainTextPasswordForImportOnly
		{
			get { return StaffPlainTextPassword; }
			set
			{
				StaffPlainTextPassword = value;
				StaffConfirmPassword = value;
			}
		}

		public ZPropertyInfo StaffPlainTextPasswordForImportOnlyInfo
		{
			[DebuggerStepThrough()]
			get { return StaffConfirmPasswordInfo; }
		}

		#endregion

		#region ChangeLoginPassword

		internal void AddPasswordToHistory(string password) => PasswordHistoryHelper.AddPasswordHistory(this, password);

		public bool HasPasswordBeenUsed(string password) => PasswordHistoryHelper.HasPasswordBeenUsed(this, password);

		public void ResetPassword(string newPassword) => ChangeOrResetPassword(null, newPassword);

		public void ChangePassword(string oldPassword, string newPassword) => ChangeOrResetPassword(oldPassword, newPassword);

		internal void ChangeOrResetPassword(string oldPassword, string newPassword)
		{
			if (IsADIntegrationEnabled)
			{
				if (!IsADLinked)
				{
					throw new InvalidOperationException(Res.GetString("29180AB2-B314-486A-8627-415DB164F87B", "The staff has not been synchronized to Active Directory yet, please try again later."));
				}
				else if (!CanAccessDirectoryEntry())
				{
					throw new InvalidOperationException(Res.GetString("B87B17B0-B5B0-4441-BCAF-AEB6ADF58D2E", "Active Directory User '{0}' is missing, please contact your system administrator.", GS_LoginName));
				}
				else
				{
					if (oldPassword == null)
					{
						SetADPassword(newPassword);
					}
					else
					{
						ChangeADPassword(oldPassword, newPassword);
					}
				}
			}
			else
			{
				ChangeLocalPassword(oldPassword, newPassword);
				GS_LastPasswordChangeDate = ZDateTime.Now;
				GS_ChangePasswordAtNextLogin = oldPassword == null;
			}
		}

		public void ChangeLocalPassword(string oldPassword, string newPassword)
		{
			AddPasswordToHistory(oldPassword);
			SetLocalPasswordHash(newPassword);
		}

		void SetLocalPasswordHash(string password) =>
			UserSecretsContext.DefaultContext.SaveSecret(
				password,
				UserSecretHashAlgorithmExtensions.PreferredAlgorithm,
				DataRegistry.Instance.PasswordHashingIterationsCount,
				GetPasswordAdapter());

		public GlbStaffAdapter GetPasswordAdapter()
			=> new GlbStaffAdapter(this);

		void SetADPassword(string newPassword)
		{
			GetADUser().SetPassword(newPassword);
		}

		void ChangeADPassword(string oldPassword, string newPassword)
		{
			GetADUser().ChangePassword(oldPassword, newPassword);
		}

		#endregion

		#endregion Password

		#region SecurityBranch
		[List("Lookups.Branches")]
		public ZGuid SecurityBranch
		{
			get { return fSecurityBranch; }
			set
			{
				if (value != fSecurityBranch)
				{
					fSecurityBranch = value;
					SecurityBranchInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateSecurityBranch();
					}
					OnActualSecurityPermissionsChange();
				}
			}
		}

		ZGuid fSecurityBranch;

		public ZPropertyInfo SecurityBranchInfo
		{
			get { return GetZPropertyInfo(Schema.SecurityBranch); }
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

		#region SecurityDepartment
		[List("Lookups.Departments")]
		public ZGuid SecurityDepartment
		{
			get { return fSecurityDepartment; }
			set
			{
				if (value != fSecurityDepartment)
				{
					fSecurityDepartment = value;
					SecurityDepartmentInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateSecurityDepartment();
					}
					OnActualSecurityPermissionsChange();
				}
			}
		}

		ZGuid fSecurityDepartment;

		public ZPropertyInfo SecurityDepartmentInfo
		{
			get { return GetZPropertyInfo(Schema.SecurityDepartment); }
		}

		#endregion

		#region Certificate Properties

		public ZString DangerousGoodsCertificateNumber
		{
			get { return Certificates.GetFirstCertificateNumber(Core.Constants.StaffCertificateType.DG, ZDateTime.Today); }
		}

		public ZString NationalIdentityNumber
		{
			get { return ViewingRights.ViewCertificatesAllowedForStaff ? Certificates.GetFirstCertificateNumber(Core.Constants.StaffCertificateType.NID) : ViewDeniedMessage; }
		}

		#endregion

		#region View Denied Message

		public ZString ViewDeniedMessage
		{
			get { return Res.GetString("cd292400-8e29-45b1-9fd5-135a6f21ed82", "** View Denied due to Security Access **"); }
		}

		#endregion

		#region Banking Details

		protected bool ViewBankDetailsNotAllowedForStaff
		{
			get { return !ViewingRights.ViewBankDetailsAllowedForStaff; }
		}

		#region GS_WagesBankAccount

		[ReadOnlyMember(nameof(ViewBankDetailsNotAllowedForStaff))]
		public override ZString GS_WagesBankAccount
		{
			get { return ViewingRights.ViewBankDetailsAllowedForStaff ? base.GS_WagesBankAccount : ViewDeniedMessage; }
			set { base.GS_WagesBankAccount = value; }
		}

		#endregion

		#region GS_WagesBankBsb

		[ReadOnlyMember(nameof(ViewBankDetailsNotAllowedForStaff))]
		public override ZString GS_WagesBankBsb
		{
			get { return ViewingRights.ViewBankDetailsAllowedForStaff ? base.GS_WagesBankBsb : ViewDeniedMessage; }
			set { base.GS_WagesBankBsb = value; }
		}

		#endregion

		#region GS_WagesBankName

		[ReadOnlyMember(nameof(ViewBankDetailsNotAllowedForStaff))]
		public override ZString GS_WagesBankName
		{
			get { return ViewingRights.ViewBankDetailsAllowedForStaff ? base.GS_WagesBankName : ViewDeniedMessage; }
			set { base.GS_WagesBankName = value; }
		}

		#endregion

		#region GS_WagesBankSwift

		[ReadOnlyMember(nameof(ViewBankDetailsNotAllowedForStaff))]
		public override ZString GS_WagesBankSwift
		{
			get { return ViewingRights.ViewBankDetailsAllowedForStaff ? base.GS_WagesBankSwift : ViewDeniedMessage; }
			set { base.GS_WagesBankSwift = value; }
		}

		#endregion

		#region GS_EftWages

		[ReadOnlyMember(nameof(ViewBankDetailsNotAllowedForStaff))]
		public override ZBool GS_EftWages
		{
			get { return ViewingRights.ViewBankDetailsAllowedForStaff ? base.GS_EftWages : ZBool.False; }
			set { base.GS_EftWages = value; }
		}

		#endregion

		#endregion

		#region Emergency Contact Details / Next of Kin

		protected bool ViewEmergencyContactNotAllowedForStaff
		{
			get { return !ViewingRights.ViewEmergencyContactAllowedForStaff; }
		}

		protected bool ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin
		{
			get { return ViewEmergencyContactNotAllowedForStaff || EmergencySameAsNextOfKin; }
		}

		#region GS_EmergencyContactName

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		public override ZString GS_EmergencyContactName
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_EmergencyContactName : ViewDeniedMessage; }
			set { base.GS_EmergencyContactName = value; }
		}

		#endregion

		#region GS_EmergencyContactEmail

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		public override ZString GS_EmergencyContactEmail
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_EmergencyContactEmail : ViewDeniedMessage; }
			set { base.GS_EmergencyContactEmail = value; }
		}

		#endregion

		#region GS_NextOfKinEmail

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		public override ZString GS_NextOfKinEmail
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_NextOfKinEmail : ViewDeniedMessage; }
			set { base.GS_NextOfKinEmail = value; }
		}

		#endregion

		#region GS_EmergencyContactRelationship

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		[List("Lookups.Relationships")]
		public override ZString GS_EmergencyContactRelationship
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_EmergencyContactRelationship : ViewDeniedMessage; }
			set { base.GS_EmergencyContactRelationship = value; }
		}

		#endregion

		#region GS_NextOfKin

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		public override ZString GS_NextOfKin
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_NextOfKin : ViewDeniedMessage; }
			set
			{
				if (base.GS_NextOfKin != value)
				{
					base.GS_NextOfKin = value;
					if (EmergencySameAsNextOfKin)
					{
						GS_EmergencyContactName = value;
					}
				}
			}
		}

		#endregion

		#region GS_NextOfKinRelationship

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		[List("Lookups.Relationships")]
		public override ZString GS_NextOfKinRelationship
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_NextOfKinRelationship : ViewDeniedMessage; }
			set
			{
				if (base.GS_NextOfKinRelationship != value)
				{
					base.GS_NextOfKinRelationship = value;

					if (EmergencySameAsNextOfKin)
					{
						GS_EmergencyContactRelationship = value;
					}
				}
			}
		}

		#endregion

		#endregion

		#region Home Address Details

		protected bool ViewHomeAddressNotAllowedForStaff
		{
			get { return !ViewingRights.ViewHomeAddressAllowedForStaff || IsControlledByScim; }
		}

		internal ViewingRights ViewingRights
		{
			get { return viewingRights ?? (viewingRights = new ViewingRights(this)); }
		}
		ViewingRights viewingRights;

		#region GS_UserAddress1

		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		public override ZString GS_UserAddress1
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? base.GS_UserAddress1 : ViewDeniedMessage; }
			set
			{
				if (value != base.GS_UserAddress1)
				{
					base.GS_UserAddress1 = value;
					ResetValidationStatus(GS_UserAddress1Info);
					Person?.FindDuplicates(this);
					ReportIfSettingViewDenied(value);
				}
			}
		}

		#endregion

		#region GS_UserAddress2

		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		public override ZString GS_UserAddress2
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? base.GS_UserAddress2 : ViewDeniedMessage; }
			set
			{
				if (value != base.GS_UserAddress2)
				{
					base.GS_UserAddress2 = value;
					ResetValidationStatus(GS_UserAddress2Info);
					Person?.FindDuplicates(this);
					ReportIfSettingViewDenied(value);
				}
			}
		}

		#endregion

		#region GS_City

		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		public override ZString GS_City
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? base.GS_City : ViewDeniedMessage; }
			set
			{
				if (value != base.GS_City)
				{
					base.GS_City = value;
					ResetValidationStatus(GS_CityInfo);
					Person?.FindDuplicates(this);
					ReportIfSettingViewDenied(value);
				}
			}
		}

		#endregion

		#region GS_State

		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		[List("Lookups.StateList")]
		public override ZString GS_State
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? base.GS_State : ViewDeniedMessage; }
			set
			{
				if (value != base.GS_State)
				{
					base.GS_State = value;
					ResetValidationStatus(GS_StateInfo);
					Person?.FindDuplicates(this);
					ReportIfSettingViewDenied(value);
				}
			}
		}

		void ReportIfSettingViewDenied(ZString value)
		{
			if (value.Length >= 10 && ViewDeniedMessage.StartsWith(value, StringComparison.OrdinalIgnoreCase))
			{
				ErrorReporter.ReportOnce("ViewDeniedValueSet", "Trying to set ViewDenied message to the bizO property.", null);
			}
		}

		#endregion

		#region GS_Country

		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		[List("Lookups.Countries")]
		public override ZString GS_RN_NKCountryCode
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? base.GS_RN_NKCountryCode : ViewDeniedMessage; }
			set
			{
				if (GS_RN_NKCountryCode != value)
				{
					base.GS_RN_NKCountryCode = value;
					GS_HomePhone_Wrapper.RefreshFormat();
					GS_WorkPhone_Wrapper.RefreshFormat();
					GS_MobilePhone_Wrapper.RefreshFormat();
					GS_FaxNum_Wrapper.RefreshFormat();
					GS_EmergencyHomePhone_Wrapper.RefreshFormat();
					GS_EmergencyWorkPhone_Wrapper.RefreshFormat();
					GS_NextOfKinHomePhone_Wrapper.RefreshFormat();
					GS_NextOfKinWorkPhone_Wrapper.RefreshFormat();
					ResetValidationStatus(GS_RN_NKCountryCodeInfo);
					Person?.FindDuplicates(this);
				}
			}
		}

		[Obsolete("Please use Country property. This will be removed once references in client-defined macros (Documents, Workflows, Notes, etc) are transformed.")]
		[CargoWise.Macros.MacroIgnore]
		public RefCountry CountryCode => Country;

		#endregion

		#region GS_Postcode

		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		public override ZString GS_Postcode
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? base.GS_Postcode : ViewDeniedMessage; }
			set
			{
				if (value != base.GS_Postcode)
				{
					base.GS_Postcode = value;
					ResetValidationStatus(GS_PostcodeInfo);
					Person?.FindDuplicates(this);
					ReportIfSettingViewDenied(value);
				}
			}
		}

		#endregion

		#region GS_Pager

		protected bool ViewOtherReferencesNotAllowedForStaff
		{
			get { return !ViewingRights.ViewOtherReferencesAllowedForStaff; }
		}

		[ReadOnlyMember(nameof(ViewOtherReferencesNotAllowedForStaff))]
		public override ZString GS_Pager
		{
			get { return ViewingRights.ViewOtherReferencesAllowedForStaff ? base.GS_Pager : ViewDeniedMessage; }
			set
			{
				if (value != base.GS_Pager)
				{
					base.GS_Pager = value;
				}
			}
		}

		#endregion

		#endregion

		#region EmergencySameAsNextOfKin

		public ZBool EmergencySameAsNextOfKin
		{
			get
			{
				if (!emergencySameAsNextOfKinCache.HasValue)
				{
					emergencySameAsNextOfKinCache = ViewingRights.ViewEmergencyContactAllowedForStaff &&
																					IsEmergencyDetailsSameAsNextOfKin;
				}
				return emergencySameAsNextOfKinCache.Value;
			}
			set
			{
				ZBool fakey = EmergencySameAsNextOfKin;
				SetNonPersistentPropertyValue(EmergencySameAsNextOfKinInfo, ref fakey, value);
				emergencySameAsNextOfKinCache = value;

				if (value)
				{
					GS_EmergencyContactName = base.GS_NextOfKin;
					GS_EmergencyHomePhone_Formatted = GS_NextOfKinHomePhone_Formatted;
					GS_EmergencyWorkPhone_Formatted = GS_NextOfKinWorkPhone_Formatted;
					GS_EmergencyContactRelationship = base.GS_NextOfKinRelationship;
				}

				GS_EmergencyContactNameInfo.RefreshBinding();
				GS_EmergencyHomePhone_Wrapper.FormattedForBindingInfo.RefreshBinding();
				GS_EmergencyWorkPhone_Wrapper.FormattedForBindingInfo.RefreshBinding();
				GS_EmergencyContactRelationshipInfo.RefreshBinding();
			}
		}

		bool? emergencySameAsNextOfKinCache;

		public ZPropertyInfo EmergencySameAsNextOfKinInfo
		{
			get { return GetZPropertyInfo(nameof(EmergencySameAsNextOfKin)); }
		}

		#endregion

		#region ActivityTrackingStatus

		[ActionField(FieldType = ActionFieldType.Hidden)]
		[List("Lookups.ActivityTrackingStatusList")]
		public override ZString GS_ActivityTrackingStatus
		{
			get { return base.GS_ActivityTrackingStatus; }
			set { base.GS_ActivityTrackingStatus = value; }
		}

		public bool GS_ActivityTrackingStatus_ReadOnly => !IsCurrentUser;

		#endregion

		#region Remunerations

		public GlbStaffRemunerationCollection Remuneration => fRemunerations = fRemunerations ?? new GlbStaffRemunerationCollection(Factory, new ZQuery(GlbStaffRemunerationSchema.GSR_GS_Staff, PK));
		GlbStaffRemunerationCollection fRemunerations;

		#endregion

		#region Classifications

		public GlbStaffClassificationCollection Classifications => fClassifications = fClassifications ?? new GlbStaffClassificationCollection(Factory, new ZQuery(GlbStaffClassificationSchema.GSL_GS_Staff, PK));
		GlbStaffClassificationCollection fClassifications;

		#endregion

		#region Reviews

		public GlbStaffReviewCollection Reviews => fReviews = fReviews ?? new GlbStaffReviewCollection(Factory, new ZQuery(GlbStaffReviewSchema.GSV_GS_Staff, PK));
		GlbStaffReviewCollection fReviews;

		#endregion

		#region Working Bases

		public GlbStaffWorkingBasisCollection WorkingBases => fWorkingBases = fWorkingBases ?? new GlbStaffWorkingBasisCollection(Factory, new ZQuery(GlbStaffWorkingBasisSchema.GSW_GS_Staff, PK));
		GlbStaffWorkingBasisCollection fWorkingBases;

		#endregion

		#region One Off Entitlements

		public GlbStaffOneOffEntitlementCollection OneOffEntitlements => fOneOffEntitlements = fOneOffEntitlements ?? new GlbStaffOneOffEntitlementCollection(Factory, new ZQuery(GlbStaffOneOffEntitlementSchema.GSO_GS_Staff, PK));
		GlbStaffOneOffEntitlementCollection fOneOffEntitlements;

		#endregion

		public ZDateTime LastLoginDateLocal
		{
			get
			{
				return GS_LastActivityDate.IsValid ? GS_LastActivityDate.ToLocalBranchTime(Factory) : GS_LastActivityDate;
			}
		}

		public ZPropertyInfo LastLoginDateLocalInfo
		{
			get { return GetZPropertyInfo(nameof(LastLoginDateLocal)); }
		}

		protected bool GS_IsController_ReadOnly
		{
			get { return (CurrentUser == null) || (!CurrentUser.GS_IsController && CurrentUser.GS_IsOperational) || (IsControlledByScim && IsPropertyMappedForScim(GlbStaffSchema.Constants.GS_IsController)); }
		}

		protected bool NoRightsExist
		{
			get
			{
				return !GS_IsResource && (CurrentUser == null || (Env.Security != null && (!Env.Security.StaffModifyAll.IsAllowed && !(Env.Security.StaffModifyOwn.IsAllowed && IsCurrentUser)) &&
					(!IsCurrentUserLocalAdminForAtLeastOneGroup || IsInDatabase)));
			}
		}

		internal ZString GenerateCode()
		{
			ZString result = ZString.Empty;

			if (!GS_FriendlyName.IsEmpty)
			{
				result = new StaffCodeCalculator(Factory).GetMostAppropriateStaffInitial(GS_FriendlyName);
			}
			if (result.IsEmpty && !GS_FullName.IsEmpty)
			{
				result = new StaffCodeCalculator(Factory).GetMostAppropriateStaffInitial(GS_FullName);
			}
			if (result.IsEmpty
#if DEBUG
 && !DoNotGenerateCodeForEmptyName4Test
#endif
)
			{
				result = new StaffCodeCalculator(Factory).GetNextAvailableUniqueCode();
			}

			return result;
		}

#if DEBUG

		internal bool DoNotGenerateCodeForEmptyName4Test { get; set; }

#endif

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = GS_IsResource ? Res.GetString("80a61f21-f05f-4ba9-a823-6c30f519b631", "Resource") : Res.GetString("11f67e72-fa83-49f0-ba62-da2c698218f5", "Staff");
				if (!IsDeleted && !GS_Code.IsEmpty)
				{
					result += " (" + GS_Code + ")";
				}
				return result;
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public ZDateTime GS_LockoutDateTimeLocal
		{
			get
			{
				var personDisabledUntil = Person?.PER_LoginDisabledUntilUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
				return (personDisabledUntil == ZDateTime.Empty && IsInDatabase) ? LoginAttemptRecorder.LockoutDateTimeLocal(GS_LoginName) : personDisabledUntil;
			}
		}

		#endregion

		#region Events

		public delegate void SalesRepStatusChangeHandler();

		public event SalesRepStatusChangeHandler SalesRepStatusChanged;

		#region Save New Staff Member Event

		public event EventHandler SuccessfulSaveNewStaffMember;

		public void OnSuccessfulSaveNewStaffMember()
		{
			if (SuccessfulSaveNewStaffMember != null)
			{
				SuccessfulSaveNewStaffMember(this, EventArgs.Empty);
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

		#region Login Synchronise failed

		protected Exception CreateLoginSynchroniseException(Exception inner)
		{
			return new ZCannotSaveException("Failed to synchronize logins, please contact your System Administrator and report the following error:\r\n\r\n" + inner.ToString(),
				"Login Synchronization failed");
		}

		#endregion

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
				if (!IsCurrentUser)
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
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExcelPasswordForOpening)); }
		}

		protected internal bool ExcelPasswordForOpening_ReadOnly => !IsCurrentUser;

		[BusinessObjectTestExclude]
		[DocumentMacroIgnore]
		[MaxLength(32)]
		public ZString ExcelPasswordForModifying
		{
			get
			{
				if (!IsCurrentUser)
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
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ExcelPasswordForModifying)); }
		}

		protected internal bool ExcelPasswordForModifying_ReadOnly => !IsCurrentUser;

		#endregion

		#region Saving

		bool allowEmptyPasswordNewStaff;
		public void AllowEmptyPasswordForNewRecord()
		{
			allowEmptyPasswordNewStaff = true;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			SaveADPasswordPolicyIfRequired();
			ExcelPasswordSupporter.DeleteStmDataIfExcelPasswordIsEmpty();
			if (!GS_IsActive && GS_IsActiveInfo.HasChanges)
			{
				Groups.RemoveAll();
				DeleteUnusedStmData();
				DeleteGlbExternalPassword_NexdocsUserToken();
			}

			if (databaseAccessGroupPKsOriginalValues != null || databaseAccessGroupPKs != null)
			{
				if (databaseAccessGroupPKs == null || !databaseAccessGroupPKs.Any())
				{
					GS_SqlLoginPasswordHash = ZBlob.Empty;
				}
				else if (GS_SqlLoginPasswordHash.IsEmpty && !EnvProxy.Instance.Registry.UseModernSqlSecuritySystem)
				{
					GS_SqlLoginPasswordHash = DbUserManager.GetRandomPasswordHash();
				}

				SetRequiresDSANudge();
				CreateDBStaffLoginInfoIfNeeded(GS_IsResource);
				CreateDatabaseAccessPermissionFlagsChangeLog();
			}
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				if (!StaffPlainTextPassword.IsEmpty || !allowEmptyPasswordNewStaff)
				{
					ChangeLocalPassword(null, StaffPlainTextPassword);
				}

				isNewStaffMember = true;
				if (GS_Code.IsEmpty)
				{
					GS_Code = GenerateCode();
#if DEBUG
					AfterCodeGeneration();
#endif
					allocatingStaffInitialAutomatically = !GS_Code.IsEmpty;
					if (GS_Code.IsEmpty)
					{
						throw new ZCannotSaveException("Staff initials could not be calculated. Either insufficient information was entered in the full name or preferred name field, or all codes based on the staff name have been used.\nTo save, please enter initials for this staff.", "Cannot save");
					}
				}
				AddUserToAllUsersGroup(Groups);
			}
			else if (
				(!GS_IsActive || (GS_IsOperational && !GS_IsController))
				&& ((ZBool)GS_IsActiveInfo.OriginalValue)
				&& (!(ZBool)GS_IsOperationalInfo.OriginalValue || (ZBool)GS_IsControllerInfo.OriginalValue)
			)
			{
				var query = @"select count(*) from dbo.GlbStaff with (UPDLOCK) where GS_Code <> @Code and GS_IsSystemAccount = 0 and GS_IsActive = 1 and (GS_IsOperational = @IsOpr OR GS_IsController = @IsController)";
				using (var cmd = Db.Connection.Command(query))
				{
					cmd.AddParameterBasedOnDbColumn("@Code", this.GS_CodeInfo.OriginalValue.ToString(), GlbStaffSchema.GS_Code);
					cmd.AddParameterBasedOnDbColumn("@IsOpr", false, GlbStaffSchema.GS_IsOperational);
					cmd.AddParameterBasedOnDbColumn("@IsController", true, GlbStaffSchema.GS_IsController);

					var nonOpStaffCount = (int)cmd.ExecuteScalar();
					if (nonOpStaffCount == 0)
					{
						throw new ZCannotSaveException(Res.GetString("C6AA002D-5D39-4152-9955-16CC8147932D", "You are attempting to affect the last active non operational or controller staff member."), Res.GetString("D6AA002D-5D39-4152-9955-16CC8147932D", "Cannot save"));
					}
				}
			}

			AddApplicableStatusChangeLogs();

			base.OnSaving();
			DetachSalesTeamsIfNeeded();
			CreateEditLogsForProperties();
			CreatePermissionFlagsChangeLog();
			AddEmploymentCommencedLogIfRequired();
			AddCertificateLogIfRequired();

			if (GS_PER.IsEmpty)
			{
				GlbPerson.CreateFromStaff(Factory, this);
			}
			else
			{
				var person = Person;
				if (person != null)
				{
					if (person.IsInDatabase && !person.HasChanges)
					{
						person.Reload();
					}

					person.UpdateFromStaff(this);
				}
			}

			if (GS_EmailAddressInfo.HasChanges && !GS_EmailAddressInfo.OriginalValue.IsEmpty)
			{
				GlbStaffEmailAddress.UpdateScheduleTaskRecipientsIfUsedByEmailFromAddress(GS_EmailAddressInfo.OriginalValue, GS_EmailAddress, GS_Code, Factory);
			}

			if (GS_ActivityTrackingStatusInfo.HasChanges)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, $"Changed {GS_ActivityTrackingStatusInfo.HumanReadableName} from {GS_ActivityTrackingStatusInfo.OriginalValue} to {GS_ActivityTrackingStatus}."));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void AddCertificateLogIfRequired()
		{
			foreach (var certificate in Certificates.Where(c => c.XZ_TypeInfo.HasChanges || !c.IsInDatabase).ToArray())
			{
				var parameters = GetCTREventsParameters(certificate);
				Logs.AddNew(Events.CertificateReceived, parameters);
			}
		}

		KeyValuePair<string, string>[] GetCTREventsParameters(GenRegCertAccredMaintList certificate)
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, certificate.XZ_TypeDescription));

			if (certificate.IsUKStaffHandlingSecureCargoCertificate)
			{
				parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, (NoResString)"Security Training"));
			}

			return parameters.ToArray();
		}

#if DEBUG
		protected virtual void AfterCodeGeneration()
		{
		}
#endif

		public bool HasDatabaseAccess => Factory.Exists(typeof(GlbGroupLink), GetGlbGroupLinkDatabaseAccessQuery());

		void SetRequiresDSANudge()
		{
			requiresDSANudge = false;
			if (!EnvProxy.Instance.Registry.UseModernSqlSecuritySystem)
			{
				return;
			}

			var hasDatabaseAccess = DatabaseAccessGroupRoles != null && DatabaseAccessGroupRoles.Any();
			var hadDatabaseAccess = DatabaseAccessGroupRolesOriginalValues != null && DatabaseAccessGroupRolesOriginalValues.Any();

			var databaseAccessChanged =
				hasDatabaseAccess != hadDatabaseAccess
				|| (hasDatabaseAccess
					&& hadDatabaseAccess
					&& (DatabaseAccessGroupRolesOriginalValues.Except(DatabaseAccessGroupRoles).Any()
					|| DatabaseAccessGroupRoles.Except(DatabaseAccessGroupRolesOriginalValues).Any()));

			requiresDSANudge = GS_SqlLoginPasswordHashInfo.HasChanges
				|| databaseAccessChanged
				|| (hasDatabaseAccess
					&& (GS_LoginNameInfo.HasChanges
						|| GS_IsActiveInfo.HasChanges
						|| GS_ActiveDirectoryObjectGuidInfo.HasChanges
						|| GS_DomainNameInfo.HasChanges));
		}

		void CreateDBStaffLoginInfoIfNeeded(bool isResource, Func<string> getDownLevelLogonName = null)
		{
			if (EnvProxy.Instance.Registry.UseModernSqlSecuritySystem)
			{
				return;
			}

			var overrideGetDownLevelLogonName = getDownLevelLogonName ?? GetDownLevelLogonName;

			applyStaffChangesToDbLogins_SqlAuthentication = null;
			applyStaffChangesToDbLogins_WinAuthentication = null;

			if (!isResource)
			{
				var staffLoginAction = DbUserManager.StaffLoginInfo.StaffLoginActions.None;
				switch (((INeedRow)this).Row.RowState)
				{
					case DataRowState.Added:
						staffLoginAction = DbUserManager.StaffLoginInfo.StaffLoginActions.Create;
						break;
					case DataRowState.Modified:
						staffLoginAction = DbUserManager.StaffLoginInfo.StaffLoginActions.Alter;
						break;
					case DataRowState.Deleted:
						if (((INeedRow)this).Row.HasVersion(DataRowVersion.Original))
						{
							staffLoginAction = DbUserManager.StaffLoginInfo.StaffLoginActions.Drop;
						}
						break;
					default:
						break;
				}

				if (staffLoginAction == DbUserManager.StaffLoginInfo.StaffLoginActions.None)
				{
					if (!DatabaseAccessGroupPKsOriginalValues.Any() && DatabaseAccessGroupPKs.Count > 0)
					{
						staffLoginAction = DbUserManager.StaffLoginInfo.StaffLoginActions.Create;
					}
					else if (DatabaseAccessGroupPKsOriginalValues.Any() && DatabaseAccessGroupPKs.Count == 0)
					{
						staffLoginAction = DbUserManager.StaffLoginInfo.StaffLoginActions.Drop;
					}
					else if (GS_SqlLoginPasswordHashInfo.HasChanges
						|| DatabaseAccessGroupPKsOriginalValues.Count() != DatabaseAccessGroupPKs.Count
						|| !DatabaseAccessGroupPKs.EqualIgnoringOrder(DatabaseAccessGroupPKsOriginalValues))
					{
						staffLoginAction = DbUserManager.StaffLoginInfo.StaffLoginActions.Alter;
					}
				}

				if (staffLoginAction == DbUserManager.StaffLoginInfo.StaffLoginActions.Drop)
				{
					applyStaffChangesToDbLogins_SqlAuthentication = DbUserManager.StaffLoginInfo.New(
						staffLoginAction
						, old_LoginName: (ZString)GS_LoginNameInfo.OriginalValue, new_LoginName: null, getDownLevelLogonName: overrideGetDownLevelLogonName
						, staffPK: PK.ToGuid(), new_PasswordHash: null
						, old_IsActive: (ZBool)GS_IsActiveInfo.OriginalValue, new_IsActive: false
						, old_StaffDatabaseAccessGroupRoles: DatabaseAccessGroupRolesOriginalValues, new_StaffDatabaseAccessGroupRoles: new HashSet<string>()
						, dbAuthenticationMode: DatabaseAuthenticationMode.Sql
						);

					applyStaffChangesToDbLogins_WinAuthentication = DbUserManager.StaffLoginInfo.New(
						staffLoginAction
						, old_LoginName: (ZString)GS_LoginNameInfo.OriginalValue, new_LoginName: null, getDownLevelLogonName: overrideGetDownLevelLogonName
						, staffPK: PK.ToGuid(), new_PasswordHash: null
						, old_IsActive: (ZBool)GS_IsActiveInfo.OriginalValue, new_IsActive: false
						, old_StaffDatabaseAccessGroupRoles: DatabaseAccessGroupRolesOriginalValues, new_StaffDatabaseAccessGroupRoles: new HashSet<string>()
						, dbAuthenticationMode: DatabaseAuthenticationMode.Windows
						);
				}
				else if (staffLoginAction != DbUserManager.StaffLoginInfo.StaffLoginActions.None)
				{
					applyStaffChangesToDbLogins_SqlAuthentication = DbUserManager.StaffLoginInfo.New(
						staffLoginAction
						, old_LoginName: (ZString)GS_LoginNameInfo.OriginalValue, new_LoginName: GS_LoginName, getDownLevelLogonName: overrideGetDownLevelLogonName
						, staffPK: PK.ToGuid(), new_PasswordHash: DataUtils.BytesToHexString(GS_SqlLoginPasswordHash)
						, old_IsActive: (ZBool)GS_IsActiveInfo.OriginalValue, new_IsActive: GS_IsActive
						, old_StaffDatabaseAccessGroupRoles: DatabaseAccessGroupRolesOriginalValues, new_StaffDatabaseAccessGroupRoles: DatabaseAccessGroupRoles
						, dbAuthenticationMode: DatabaseAuthenticationMode.Sql
						);

					applyStaffChangesToDbLogins_WinAuthentication = DbUserManager.StaffLoginInfo.New(
						staffLoginAction
						, old_LoginName: (ZString)GS_LoginNameInfo.OriginalValue, new_LoginName: GS_LoginName, getDownLevelLogonName: overrideGetDownLevelLogonName
						, staffPK: PK.ToGuid(), new_PasswordHash: null
						, old_IsActive: (ZBool)GS_IsActiveInfo.OriginalValue, new_IsActive: GS_IsActive
						, old_StaffDatabaseAccessGroupRoles: DatabaseAccessGroupRolesOriginalValues, new_StaffDatabaseAccessGroupRoles: DatabaseAccessGroupRoles
						, dbAuthenticationMode: DatabaseAuthenticationMode.Windows
						);
				}
			}
		}

		void CreateEditLogsForProperties()
		{
			CreateEditLogForProperty(GS_TitleInfo, (NoResString)"Title");
			CreateEditLogForProperty(GS_GB_HomeBranchInfo, (NoResString)"Home Branch", typeof(GlbBranch));
			CreateEditLogForProperty(GS_GE_HomeDepartmentInfo, (NoResString)"Home Department", typeof(GlbDepartment));
			CreateEditLogForProperty(GS_LoginNameInfo, (NoResString)"Login");
			CreateEditLogForProperty(GS_DomainNameInfo, (NoResString)"Domain");
			CreateEditLogForProperty(GS_ActiveDirectoryObjectGuidInfo, "ADGuid");
			CreateEditLogForProperty(GS_EmailAddressInfo, (NoResString)"Email");
			CreateEditLogForSensitiveProperty(GS_UserAddress1Info, "Address1");
			CreateEditLogForSensitiveProperty(GS_UserAddress2Info, "Address2");
			CreateEditLogForSensitiveProperty(GS_CityInfo, (NoResString)"City");
			CreateEditLogForSensitiveProperty(GS_StateInfo, (NoResString)"State");
			CreateEditLogForSensitiveProperty(GS_PostcodeInfo, (NoResString)"Postcode");
			CreateEditLogForSensitiveProperty(GS_RN_NKCountryCodeInfo, "Country/Region");
		}

		void CreateEditLogForProperty(ZPropertyInfo propertyInfo, string description, Type type = null)
		{
			var oldValue = propertyInfo.OriginalValue;
			var newValue = propertyInfo.Value;
			if (propertyInfo.HasChanges && propertyInfo.OriginalValue != propertyInfo.Value && propertyInfo.Value.ToString() != ViewDeniedMessage)
			{
				if (type != null)
				{
					var oldBizo = Factory.Load(type, (ZGuid)oldValue);
					var newBizo = Factory.Load(type, (ZGuid)newValue);
					oldValue = oldBizo != null ? CodePropertyAttribute.CodeFromBusinessObject(oldBizo) : ZString.Empty;
					newValue = newBizo != null ? CodePropertyAttribute.CodeFromBusinessObject(newBizo) : ZString.Empty;
				}
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "{0}|OLD={1}|NEW={2}", description, oldValue, newValue));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void CreateEditLogForSensitiveProperty(ZPropertyInfo propertyInfo, string description)
		{
			if (propertyInfo.HasChanges && propertyInfo.OriginalValue != propertyInfo.Value && propertyInfo.Value.ToString() != ViewDeniedMessage)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, description);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void CreatePermissionFlagsChangeLog()
		{
			AddChangeLogForFlagProperty(GS_IsSalesRepInfo, (NoResString)"EDT - Is Sales Rep: {0}");
			AddChangeLogForFlagProperty(GS_IsControllerInfo, (NoResString)"EDT - Is Controller: {0}");
			AddChangeLogForFlagProperty(GS_IsOperationalInfo, (NoResString)"EDT - Is Operational: {0}");
		}

		void CreateDatabaseAccessPermissionFlagsChangeLog()
		{
			AddChangeLogForFlagProperty(IsDatabaseDeveloperInfo, IsDatabaseDeveloperChanged, (NoResString)"EDT - Is Database Developer: {0}");
			AddChangeLogForFlagProperty(IsReadOnlyDBUserInfo, IsReadOnlyDBUserChanged, (NoResString)"EDT - Is Database Reader: {0}");
			AddChangeLogForFlagProperty(IsBackupOperatorInfo, IsBackupOperatorChanged, (NoResString)"EDT - Is Backup Operator: {0}");
		}

		void AddChangeLogForFlagProperty(ZPropertyInfo propertyInfo, string referenceText) => AddChangeLogForFlagProperty(propertyInfo, propertyInfo.HasChanges, referenceText);

		void AddChangeLogForFlagProperty(ZPropertyInfo propertyInfo, ZBool propertyHasChanges, string referenceText)
		{
			if (propertyHasChanges)
			{
				Logs.AddNew(Events.SecurityModified, string.Format(CultureInfo.InvariantCulture, referenceText, (ZBool)propertyInfo.Value ? "Y" : "N"));
			}
		}

		void AddEmploymentCommencedLogIfRequired()
		{
			if (!GS_EmploymentDate.IsEmpty && (!IsInDatabase || GS_EmploymentDateInfo.HasChanges))
			{
				var existingEmploymentCommencedLogQuery = new ZQuery(StmALogSchema.SL_Parent, PK);
				existingEmploymentCommencedLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.EmploymentCommencedCode);
				var existingLog = Factory.LoadTop1<StmALog>(existingEmploymentCommencedLogQuery);

				if (existingLog != null)
				{
					existingLog.Cancel();
				}

				Logs.AddNew(new EventValue(AutoEvents.EmploymentCommenced, reference: FormattableString.Invariant($"{GS_Code} - Employment Commenced"), eventTime: GS_EmploymentDate.ToOffset(), deferFiringWorkflow: true));
			}
		}

		bool allocatingStaffInitialAutomatically;
		bool isNewStaffMember;
		Action applyStaffChangesToDbLogins_SqlAuthentication;
		Action applyStaffChangesToDbLogins_WinAuthentication;
		bool requiresDSANudge;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				GS_CodeInfo.RefreshBinding();
				if (isNewStaffMember)
				{
					OnSuccessfulSaveNewStaffMember();
					isNewStaffMember = false;
				}
			}
			else
			{
				DeleteUnsavedStatusChangeLogs();

				if (allocatingStaffInitialAutomatically)
				{
					GS_Code = string.Empty;
				}
			}

			allocatingStaffInitialAutomatically = false;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsInDatabase && WorkTimes.Count == 0)
			{
				WorkTimes.SetDefaultWorkingHours(GS_GB_HomeBranch, SaturdayIsWorkingDay);
			}

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (requiresDSANudge)
				{
					CreateUserRepositoryDatabaseIfMissing();
					ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("DSA");
				} else
				{
					ApplyStaffChangesToDbLogins();
				}
			}

			databaseAccessGroupPKsOriginalValues = databaseAccessGroupPKs == null ? null : databaseAccessGroupPKs.ToArray();
		}

		void ApplyStaffChangesToDbLogins(bool overrideDbSupportsWindowsAuthentication = false)
		{
			if (EnvProxy.Instance.Registry.UseModernSqlSecuritySystem)
			{
				return;
			}

			try
			{
				if (applyStaffChangesToDbLogins_SqlAuthentication != null && new DbUserManager().DbSupportsSqlAuthentication())
				{
					applyStaffChangesToDbLogins_SqlAuthentication.Invoke();
				}
				if (applyStaffChangesToDbLogins_WinAuthentication != null && (overrideDbSupportsWindowsAuthentication || new DbUserManager().DbSupportsWindowsAuthentication(GetDownLevelLogonName)))
				{
					applyStaffChangesToDbLogins_WinAuthentication.Invoke();
				}
			}
			catch (Exception e)
			{
				throw CreateLoginSynchroniseException(e);
			}
		}

		public bool ValidationSuspendedForStaff { get; set; }

		void DeleteUnusedStmData()
		{
			var datas = new StmDataCollection(Factory, new ZQuery(StmDataSchema.SD_Owner, this.PK));
			datas.DeleteAll();
		}

		void DeleteGlbExternalPassword_NexdocsUserToken()
		{
			this.LoadChildren<GlbExternalPassword>(GlbExternalPasswordSchema.GP_GS).Where(x => x.GP_PasswordType == PasswordTypesList.Codes.NUT).DeleteAll();
		}

		void DeleteUnusedStmDefaultPrinter()
		{
			var printers = Factory.Load(ObjectFactory.GetType<IStmDefaultPrinter>(), new ZQuery(StmDefaultPrinterSchema.SDP_SubjectID, this.PK));
			printers.DeleteAll();
		}

		void AddApplicableStatusChangeLogs()
		{
			// Audit changes to is device user flag
			if (GS_IsDeviceInfo.HasChanges)
			{
				if (GS_IsDevice)
				{
					this.Logs.AddNew(AutoEvents.StaffFlaggedAsDeviceOnly);
				}
				else
				{
					this.Logs.AddNew(AutoEvents.StaffUnFlaggedAsDeviceOnly);
				}
			}

			if (GS_CanLoginInfo.HasChanges)
			{
				if (!GS_CanLogin)
				{
					this.Logs.AddNew(AutoEvents.UserMarkedAsCannotLogin);
				}
				else
				{
					this.Logs.AddNew(AutoEvents.UserMarkedAsCanLogin);
				}
			}

			if (GS_IsRobotInfo.HasChanges)
			{
				if (!GS_IsRobot)
				{
					this.Logs.AddNew(AutoEvents.StaffUnFlaggedAsRobot);
				}
				else
				{
					this.Logs.AddNew(AutoEvents.StaffFlaggedAsRobot);
				}
			}

			// For STL purposes => log a user seat event if its licence user seat status changes
			if (!GS_IsResource && !GS_IsSystemAccount)
			{
				string currentUserSeatStatus = UserSeatReferenceStatus.GetUserSeatStatus(GS_IsActive, GS_IsDevice, GS_CanLogin, GS_IsRobot);
				string previousUserSeatStatus = (IsInDatabase)
					? UserSeatReferenceStatus.GetUserSeatStatus((ZBool)GS_IsActiveInfo.OriginalValue, (ZBool)GS_IsDeviceInfo.OriginalValue, (ZBool)GS_CanLoginInfo.OriginalValue, (ZBool)GS_IsRobotInfo.OriginalValue)
					: UserSeatReferenceStatus.NewUser;

				if (currentUserSeatStatus != previousUserSeatStatus)
				{
					this.Logs.AddNew(AutoEvents.UserSeat, reference: previousUserSeatStatus + ">>" + currentUserSeatStatus);
				}
			}
		}

		void DeleteUnsavedStatusChangeLogs()
		{
			this.Logs.LogsNotInDB
				.Where(log =>
					log.SL_SE_NKEvent == Events.UserSeatCode
					|| log.SL_SE_NKEvent == Events.StaffFlaggedAsDeviceOnlyCode
					|| log.SL_SE_NKEvent == Events.StaffUnFlaggedAsDeviceOnlyCode)
				.DeleteAll();
		}

		internal List<GlbGroupLink> GroupLinks = new List<GlbGroupLink>();

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new GlbStaffUniqueIndexFailureHandler(); }
		}

		class GlbStaffUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return GlbStaffSchema.Constants.Indexes.NR_UC__GS_Code; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(
					Res.GetString("56D21A0A-DBA0-43C5-BC5B-687FCBF60664", "A staff with this code already exists in database. If the code was auto-generated, please Save the form again."),
					Res.GetString("3FB1FA7A-D936-4020-962E-CD11BD96E356", "Staff code already exists."));
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			if (!ValidationSuspendedForStaff)
			{
				base.RunPreSaveValidationCore();
			}
		}

		protected override GlbStaffValidation GetNewValidation()
		{
			return GS_IsResource ? new GlbResourceValidation(this) : new GlbStaffValidationReal(this);
		}

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		internal List<string> GetScheduledReportDescriptionAssignedToPrintUserTop4()
		{
			return scheduledReportDescriptionAssignedToPrintUserCache ??
				(scheduledReportDescriptionAssignedToPrintUserCache = ObjectFactory.New<IScheduledReportHelper>().GetScheduleReportsDescriptionAssignedToUser(GS_Code, true, 4));
		}
		List<string> scheduledReportDescriptionAssignedToPrintUserCache;

		internal StmMenuItem[] GetUnpublishedCustomizedDocumentsAndReports()
		{
			return  unpublishedCustomizedDocumentsAndReports??= LoadUnpublishedCustomizedDocumentsAndReports();
		}

		StmMenuItem[] LoadUnpublishedCustomizedDocumentsAndReports()
		{
			var query = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, GS_Code);
			var unpublishedFilter = new ZQuery(StmMenuItemSchema.SU_IsPublished, false);

			query.AddToFilter(unpublishedFilter, JoinCondition.And);
			return Factory.Load<StmMenuItem>(query);
		}

		StmMenuItem[] unpublishedCustomizedDocumentsAndReports;

		public bool ShowBranchDepartmentAndPositionErrorsAsWarnings
		{
			get { return showBranchDepartmentAndPositionErrorsAsWarnings; }
			set { showBranchDepartmentAndPositionErrorsAsWarnings = value; }
		}

		bool showBranchDepartmentAndPositionErrorsAsWarnings;

		#endregion

		#region Related Business Objects

		#region Time Allocations

		public GlbTimeAllocationFilter TimeAllocationFilter
		{
			get
			{
				if (fTimeAllocationFilter == null)
				{
					fTimeAllocationFilter = new GlbTimeAllocationFilter(TimeAllocations);
					RegisterEditableChildObject(fTimeAllocations);
				}
				return fTimeAllocationFilter;
			}
		}

		GlbTimeAllocationFilter fTimeAllocationFilter;

		/// <summary>
		/// Record a time allocation against this resource or staff member.
		/// If a time allocation already exists for the specified source, then the time allocation is updated rather than created.
		/// </summary>
		/// <param name="source">The business object that is the source of this time allocation (Training Session, Job etc) - can be null</param>
		/// <param name="type">The type of the allocation - comes from GlbTimeAllocation.Lookups.Types</param>
		/// <param name="startTime">The time the allocation starts</param>
		/// <param name="endTime">The time the allocation ends</param>
		/// <param name="comment">A comment for the allocation</param>
		public void RecordOrUpdateTimeAllocation(BusinessObject source, ZString type, ZDateTime startTime, ZDateTime endTime, ZString comment)
		{
			GlbTimeAllocation time = null;
			if (source != null)
			{
				ZQuery query = new ZQuery(GlbStaffHolidaySchema.GA_ParentID, source.PK);
				query.AddToFilter(GlbStaffHolidaySchema.GA_ParentTableCode, ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(source.TableName));

				GlbTimeAllocation[] found = (GlbTimeAllocation[])TimeAllocations.Find(query);
				if (found.Length > 0)
				{
					time = found[0];
				}
			}

			if (time == null)
			{
				time = TimeAllocations.AddNew();
			}

			time.GA_WorkHolidayType = type;
			time.GA_StartTime = startTime;
			time.GA_EndTime = endTime;
			time.GA_LeaveComment = comment;
			if (source != null)
			{
				time.GA_ParentID = source.PK;
				time.GA_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(source.TableName);
			}
		}

		GlbTimeAllocationCollection TimeAllocations
		{
			get
			{
				if (fTimeAllocations == null)
				{
					fTimeAllocations = new GlbTimeAllocationCollection(this);
					fTimeAllocations.Load();
					fTimeAllocations.Sort(GlbStaffHolidaySchema.GA_StartTime.Name, ListSortDirection.Descending);
					RegisterEditableChildObject(fTimeAllocations);
				}

				return fTimeAllocations;
			}
		}

		GlbTimeAllocationCollection fTimeAllocations;

		#endregion

		#region WorkTimes

		[ChildEditable]
		public GlbWorkTimeCollection WorkTimes
		{
			get
			{
				if (fWorkTimes == null)
				{
					ZQuery query = new ZQuery(GlbWorkTimeSchema.GW_ParentTableCode, GlbStaffSchema.Constants.Prefix);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					query.AddToFilter(GlbWorkTimeSchema.GW_ParentID, PK);
					fWorkTimes = new GlbWorkTimeCollection(Factory, query);

					RegisterEditableChildObject(fWorkTimes);
				}

				return fWorkTimes;
			}
		}
		GlbWorkTimeCollection fWorkTimes;

		public GlbWorkTimeViewModel WorkTimeViewModel
		{
			get
			{
				if (workTimeViewModel == null)
				{
					var workPatternQuery = new ZQuery(GlbWorkPatternSchema.GWP_GS_Staff, PK);
					workPatternQuery.AddToFilter(GlbWorkPatternSchema.GWP_EffectiveDate, SQLComparisonOperator.LessThan, ZDateTimeOffset.UtcNow);
					workPatternQuery.OrderBy = GlbWorkPatternSchema.GWP_EffectiveDate.Name + " DESC";
					var currentWorkPattern = Factory.LoadTop1<GlbWorkPattern>(workPatternQuery);

					workTimeViewModel = new GlbWorkTimeViewModel(WorkTimes, currentWorkPattern != null);
				}
				return workTimeViewModel;
			}
		}
		GlbWorkTimeViewModel workTimeViewModel;

		#endregion

		#region WorkDays

		IWorkTimeArithmetic WorkTimeArithmetic => WorkingDays.GetInstance(Factory, GlbDepartment.CurrentDepartment.PK, GlbBranch.CurrentBranch.PK, PK);

		#endregion

		#region Staff Security Permissions

		[ChildEditable(true)]
		public GlbSecurityCollection StaffSecurityPermissionsCollection
		{
			get
			{
				if (fStaffSecurityPermissionsCollection == null)
				{
					fStaffSecurityPermissionsCollection = new GlbSecurityCollection(this, Factory);
					ReloadStaffSecurityPermissionsCollectionCore();
					RegisterEditableChildObject(fStaffSecurityPermissionsCollection);
				}

				return fStaffSecurityPermissionsCollection;
			}
		}
		GlbSecurityCollection fStaffSecurityPermissionsCollection;

		internal void ReloadStaffSecurityPermissionsCollection()
		{
			if (fStaffSecurityPermissionsCollection == null)
			{
				_ = StaffSecurityPermissionsCollection;
			}
			else
			{
				ReloadStaffSecurityPermissionsCollectionCore();
			}
		}

		void ReloadStaffSecurityPermissionsCollectionCore()
		{
			//var query = new ZQuery(GlbSecuritySchema.GU_GS, PK);
			//query.AddToFilter(new ZQuery(GlbSecuritySchema.GU_GG, Groups.Select(x => x.PK)), JoinCondition.Or);
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			fStaffSecurityPermissionsCollection.LoadWithMoreFiltering(query);
		}

		public GlbSecurityCollectionView StaffSecurityPermissionsView
		{
			get
			{
				if (fStaffSecurityPermissionsView == null)
				{
					fStaffSecurityPermissionsView = new GlbSecurityCollectionView(StaffSecurityPermissionsCollection);
				}

				return fStaffSecurityPermissionsView;
			}
		}

		GlbSecurityCollectionView fStaffSecurityPermissionsView;

		public GlbSecurityChangeOthersView SecurityChangeOthersView
		{
			get
			{
				if (fSecurityChangeOthersView == null)
				{
					fSecurityChangeOthersView = new GlbSecurityChangeOthersView(StaffSecurityPermissionsCollection, this, GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator);
				}

				return fSecurityChangeOthersView;
			}
		}

		public GlbSecurityChangeOthersView GroupOwners
		{
			get
			{
				if (fGroupOwners == null)
				{
					fGroupOwners = new GlbSecurityChangeOthersView(StaffSecurityPermissionsCollection, this, GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner);
				}
				return fGroupOwners;
			}
		}

		GlbSecurityChangeOthersView fSecurityChangeOthersView;
		GlbSecurityChangeOthersView fGroupOwners;

		[BusinessObjectTestExclude]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public GlbSecurityChangeOthersView GroupOwnersForGroupView
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Group Security Permissions

		public GlbSecurityCollection GroupSecurityPermissionsCollection
		{
			get
			{
				var localGroupSecurityPermissionsCollection = new GlbSecurityCollection(Factory);

				ZQuery groupFilter = new ZQuery(GlbSecuritySchema.GU_GG, Groups.GetPKs());
				localGroupSecurityPermissionsCollection.Load(groupFilter);
				fGroupSecurityPermissionsCollection = localGroupSecurityPermissionsCollection;
				return fGroupSecurityPermissionsCollection;
			}
		}

		GlbSecurityCollection fGroupSecurityPermissionsCollection;

		public GlbSecurityCollection GroupSecurityPermissionsCollectionForBinding
		{
			get
			{
				if (fGroupSecurityPermissionsCollectionForBinding == null)
				{
					fGroupSecurityPermissionsCollectionForBinding = new GlbSecurityCollection(Factory);
					fGroupSecurityPermissionsCollectionForBinding.SetReadOnlyIncludingChildren(true);
				}
				return fGroupSecurityPermissionsCollectionForBinding;
			}
		}

		GlbSecurityCollection fGroupSecurityPermissionsCollectionForBinding;

		#endregion

		#region Groups

		/// <summary>
		/// All groups includes security groups and sales teams.
		/// </summary>
#if DEBUG
		internal
#endif
		GlbGroupManyToManyCollection AllGroups
		{
			get
			{
				if (allGroups == null)
				{
					allGroups = new GlbGroupManyToManyCollection(this);
					allGroups.Load();
					allGroups.CountChanged += new CollectionCountChangedEventHandler(SecurityGroups_CountChanged);

					var factoryCopyService = Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>();
					if (factoryCopyService != null)
					{
						factoryCopyService.AddOnCopyFinishedAction(() => AddUserToAllUsersGroup(allGroups));
					}
					else
					{
						AddUserToAllUsersGroup(allGroups);
					}
				}

				return allGroups;
			}
		}

		IBusinessObjectCollection IGlbStaff.Groups
		{
			get { return Groups; }
		}

		GlbGroupManyToManyCollection allGroups;

		/// <summary>
		/// Security groups only. Does not include Sales Teams.
		/// </summary>
		public GroupCollectionView Groups
		{
			get
			{
				if (securityGroups == null)
				{
					securityGroups = new GroupCollectionView(AllGroups, false, false);
					securityGroups.CountChanged += new CollectionCountChangedEventHandler(SecurityGroups_CountChanged);
				}

				return securityGroups;
			}
		}
		GroupCollectionView securityGroups;

		void SecurityGroups_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!Factory.IsValidationSuspended)
			{
				Validation.ValidateAll();
			}
			ReloadStaffSecurityPermissionsCollection();
		}

		public GroupCollectionView ActiveGroups
		{
			get { return activeGroups ?? (activeGroups = new GroupCollectionView(AllGroups, false, true)); }
		}

		GroupCollectionView activeGroups;

		public bool IsMemberOf(GlbGroupCollection groups)
		{
			foreach (GlbGroup group in groups)
			{
				if (!Groups.Contains(group))
				{
					return false;
				}
			}

			return true;
		}

		public GlbGroup GetGroupThisUserLeads()
		{
			return GetGroupFromMembership(MembershipTypeList.Codes.TLD);
		}

		public GlbGroup GetGroupFromMembership(ZString membershipType)
		{
			ZQuery query = new ZQuery(GlbGroupLinkSchema.GK_GS, PK);

			foreach (GlbGroupLink link in Factory.Load<GlbGroupLink>(query))
			{
				if (link.GK_MembershipType == membershipType)
				{
					return link.Group;
				}
			}

			return null;
		}

		#endregion

		#region Capabilities

		public GlbResourceCapabilityPivot CapabilityPivot { get; set; }

		IBusinessObjectCollection IGlbStaff.Capabilities => Capabilities;

		[ChildEditable]
		public GlbCapabilityManyToManyCollection Capabilities
		{
			get
			{
				if (capabilities == null)
				{
					capabilities = new GlbCapabilityManyToManyCollection(this);
					capabilities.Load();
					RegisterEditableChildObject(capabilities);
				}

				return capabilities;
			}
		}

		GlbCapabilityManyToManyCollection capabilities;

		[ChildEditable]
		public GlbResourceCapabilityPivotCollection CapabilityPivots
		{
			get
			{
				if (capabilityPivots == null)
				{
					capabilityPivots = new GlbResourceCapabilityPivotCollection(this);
					RegisterEditableChildObject(capabilityPivots);
				}

				return capabilityPivots;
			}
		}

		GlbResourceCapabilityPivotCollection capabilityPivots;

		#endregion

		#region Release Notes Read

		[ChildEditable(true)]
		public GlbReleaseNoteReadCollectionByStaff ReleaseNotesRead
		{
			get
			{
				if (fReleaseNotesRead == null)
				{
					fReleaseNotesRead = new GlbReleaseNoteReadCollectionByStaff(this);
					fReleaseNotesRead.Load();
					RegisterEditableChildObject(fReleaseNotesRead);
				}

				return fReleaseNotesRead;
			}
		}
		GlbReleaseNoteReadCollectionByStaff fReleaseNotesRead;

		#endregion

		#region Current Group Link

		public GlbGroupLink CurrentGroupLink
		{
			get { return fCurrentGroupLink; }
			set { fCurrentGroupLink = value; }
		}

		GlbGroupLink fCurrentGroupLink;

		#endregion

		#region Holidays

		[ChildEditable(true)]
		public GlbStaffHolidayCollection Holidays
		{
			get
			{
				if (fHolidays == null)
				{
					fHolidays = new GlbStaffHolidayCollection(this);
					fHolidays.Load();
					fHolidays.Sort(GlbStaffHolidaySchema.GA_StartTime.Name, ListSortDirection.Descending);
					RegisterEditableChildObject(fHolidays);
				}

				return fHolidays;
			}
		}
		GlbStaffHolidayCollection fHolidays;

		public GlbStaffHolidayIncBMSLeaveCollection HolidaysIncBMSLeave
		{
			get
			{
				if (holidaysIncBMSLeave == null)
				{
					holidaysIncBMSLeave = new GlbStaffHolidayIncBMSLeaveCollection(this);
					holidaysIncBMSLeave.Load();
					holidaysIncBMSLeave.Sort(GlbStaffHolidaySchema.GA_StartTime.Name, ListSortDirection.Descending);
				}

				return holidaysIncBMSLeave;
			}
		}
		GlbStaffHolidayIncBMSLeaveCollection holidaysIncBMSLeave;

		public bool IsLeaveEnabled
		{
			get
			{
				if (isLeaveEnabled != null)
				{
					return isLeaveEnabled.Value;
				}
				else
				{
					var query = new ZQuery(HrlStaffPolicySchema.LLS_GS_Staff, this.PK);
					isLeaveEnabled = Factory.Load<IHrlStaffPolicy>(query).Length > 0;
					return isLeaveEnabled.Value;
				}
			}
		}
		bool? isLeaveEnabled;

		#endregion

		#region Language Skills

		[ChildEditable(true)]
		public GlbPersonLanguageCollection LanguageSkills
		{
			get
			{
				if (Person == null)
				{
					var emptyCollection = new GlbPersonLanguageCollection(Factory);
					RegisterEditableChildObject(emptyCollection);

					return emptyCollection;
				}

				if (fLanguageSkills == null)
				{
					fLanguageSkills = new GlbPersonLanguageCollection(Person);
					RegisterEditableChildObject(fLanguageSkills);
				}

				return fLanguageSkills;
			}
		}
		GlbPersonLanguageCollection fLanguageSkills;

		#endregion

		#region Activity Logs

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return base.ShouldCreateAutoLogIfOnlyChildrenHaveChanges && !GlbSecurity.IsSecurityModificationOnly(this, StaffSecurityPermissionsCollection); }
		}

		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges
		{
			get { return base.ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges && !GlbSecurity.IsSecurityModificationOnly(this, StaffSecurityPermissionsCollection); }
		}

		public StmActivityLogCollection ActivityLogsForUser
		{
			get
			{
				if (activityLogsForUser == null)
				{
					var localActivityLogsForUser = new StmActivityLogCollectionByStaff(this);
					localActivityLogsForUser.Load();
					activityLogsForUser = localActivityLogsForUser;
				}
				return activityLogsForUser;
			}
		}

		StmActivityLogCollection activityLogsForUser;

		public StmActivityLogFilterProvider ActivityLogForUserFilterProvider
		{
			get
			{
				if (activityLogForUserFilterProvider == null)
				{
					activityLogForUserFilterProvider = new StmActivityLogFilterProvider(ActivityLogsForUser);
				}
				return activityLogForUserFilterProvider;
			}
		}

		StmActivityLogFilterProvider activityLogForUserFilterProvider;

		public ZDateTime LastActivityUtc(int maximumMinutesToSearchBack)
		{
			ZQuery query = new ZQuery(StmActivityLogSchema.S7_GS_NKUser, GS_Code);
			query.OrderBy = StmActivityLogSchema.Constants.S7_OpenDateTimeUtc + OrderByClause.Descending;
			query.AddToFilter(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddMinutes(-maximumMinutesToSearchBack));
			var result = Factory.LoadTop1<StmActivityLog>(query);
			return result != null ? result.S7_OpenDateTimeUtc : ZDateTime.Empty;
		}

		#endregion

		#region Identity Provider

		public ZGuid IdentityProviderUserId
		{
			get
			{
				if (GS_PER.IsEmpty)
				{
					return ZGuid.Empty;
				}
				else
				{
					var person = Person;
					if (person != null)
					{
						return person.PER_IDPUserId;
					}
				}
				return ZGuid.Empty;
			}
			set
			{
				var person = Person;
				if (person == null)
				{
					return;
				}

				person.PER_IDPUserId = value;
				person.HasChanges = true;
			}
		}

		#endregion

		#region Sales Team

		public SalesTeamCollection SalesTeams
		{
			get
			{
				if (salesTeams == null)
				{
					salesTeams = new SalesTeamCollection(this);
					// Not Registering as child editable as it would produce a cycle between GlbGroup::Staff <-> GlbStaff::SalesTeams
					salesTeams.CollectionCountChange += salesTeams_CollectionCountChange;
				}

				return salesTeams;
			}
		}
		SalesTeamCollection salesTeams;

		void salesTeams_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			HasChanges = true;
		}

		public SalesTeam GetSalesTeamResponsibleFor(RefUNLOCO unloco)
		{
			if (unloco == null)
			{
				return null;
			}

			foreach (var salesTeam in SalesTeams.Where(x => x.GG_GC == GlbCompany.CurrentCompany.PK))
			{
				if (salesTeam.Covers(unloco))
				{
					return salesTeam;
				}
			}

			foreach (var salesTeam in SalesTeams.Where(x => x.GG_GC.IsEmpty))
			{
				if (salesTeam.Covers(unloco))
				{
					return salesTeam;
				}
			}

			return null;
		}

		void DetachSalesTeamsIfNeeded()
		{
			if (ShouldDetachSalesTeams)
			{
				foreach (var salesTeam in SalesTeams.ToArray())
				{
					SalesTeams.RemoveFromRelationship(salesTeam);
				}
			}
		}

		public bool ShouldDetachSalesTeams
		{
			get { return !GS_IsSalesRep && GS_IsSalesRepInfo.HasChanges && SalesTeams.Count > 0; }
		}

		#endregion

		#region Commission Rules

		public AccStaffCommissionRuleCollection CommissionRules
		{
			get
			{
				return commissionRules ?? (commissionRules = new AccStaffCommissionRuleCollection(this));
			}
		}
		AccStaffCommissionRuleCollection commissionRules;

		#endregion

		#region Overall Commission Rules

		public OverallStaffCommissionRuleCollection OverallCommissionRules
		{
			get
			{
				if (overallCommissionRules == null)
				{
					overallCommissionRules = new OverallStaffCommissionRuleCollection(this);
					overallCommissionRules.LoadRules();
				}

				return overallCommissionRules;
			}
		}
		OverallStaffCommissionRuleCollection overallCommissionRules;

		public OverallStaffCommissionRule GetOverallCommissionRuleResponsibleFor(RefUNLOCO unloco, RefUNLOCO fallbackUnloco, CommissionItemArgs itemArgs)
		{
			if (!GS_IsActive)
			{
				return null;
			}

			var unlocoMatchingPredicates = new OverallStaffCommissionRulePredicateQueue(GetUnlocoMatchingPredicates(unloco, fallbackUnloco).ToArray());

			var companyMatchingPredicates = GetCompanyMatchingPredicates();

			var productMatchingPredicates = GetProductMatchingPredicates(itemArgs);

			var serviceMatchingPredicates = GetServiceMatchingPredicates(itemArgs);

			var submodulePredicates = GetSubmodulePredicates(itemArgs);

			var modePredicate = GetModePredicate(itemArgs);

			var originPredicate = GetOriginPredicate(itemArgs);

			var destinationPredicate = GetDestinationPredicate(itemArgs);

			var activeRules = OverallCommissionRules.Cast<OverallStaffCommissionRule>().Where(x => x.IsCurrent(ZDate.Today) && x.RuleDisable == null).ToArray();
			var rules = new[]
			{
				unlocoMatchingPredicates, companyMatchingPredicates,
				productMatchingPredicates, serviceMatchingPredicates, submodulePredicates,
				modePredicate, originPredicate, destinationPredicate
			};

			return GetMatchingRules(activeRules, rules).FirstOrDefault();
		}

		static OverallStaffCommissionRulePredicateQueue GetProductMatchingPredicates(CommissionItemArgs itemArgs)
		{
			return new OverallStaffCommissionRulePredicateQueue(
				(x) => x.Product == itemArgs.Product,
				(x) => !itemArgs.Product.IsEmpty && x.Product == CommissionRuleLookups.AnyProductsCode);
		}

		static OverallStaffCommissionRulePredicateQueue GetServiceMatchingPredicates(CommissionItemArgs itemArgs)
		{
			return new OverallStaffCommissionRulePredicateQueue(
				(x) => x.Service == itemArgs.Service,
				(x) => !itemArgs.Service.IsEmpty && x.Service == CommissionRuleLookups.AnyServicesCode);
		}

		static OverallStaffCommissionRulePredicateQueue GetSubmodulePredicates(CommissionItemArgs itemArgs)
		{
			return new OverallStaffCommissionRulePredicateQueue(
				(x) => x.SubModule == itemArgs.SubModule,
				(x) => !itemArgs.SubModule.IsEmpty && x.SubModule == CommissionRuleLookups.AnySubModulesCode);
		}

		static OverallStaffCommissionRulePredicateQueue GetCompanyMatchingPredicates()
		{
			return new OverallStaffCommissionRulePredicateQueue(
				(x) => x.CompanyPk == GlbCompany.CurrentCompany.PK,
				(x) => x.CompanyPk.IsEmpty);
		}

		static OverallStaffCommissionRulePredicateQueue GetDestinationPredicate(CommissionItemArgs itemArgs)
		{
			return new OverallStaffCommissionRulePredicateQueue(
				(x) => itemArgs.Destination == x.Destination,
				(x) => !x.Destination.IsEmpty && itemArgs.Destination.StartsWith(x.Destination, StringComparison.OrdinalIgnoreCase),
				(x) => x.Destination.IsEmpty);
		}

		static OverallStaffCommissionRulePredicateQueue GetOriginPredicate(CommissionItemArgs itemArgs)
		{
			return new OverallStaffCommissionRulePredicateQueue(
				(x) => itemArgs.Origin == x.Origin,
				(x) => !x.Origin.IsEmpty && itemArgs.Origin.StartsWith(x.Origin, StringComparison.OrdinalIgnoreCase),
				(x) => x.Origin.IsEmpty);
		}

		static OverallStaffCommissionRulePredicateQueue GetModePredicate(CommissionItemArgs itemArgs)
		{
			return new OverallStaffCommissionRulePredicateQueue(
				(x) => x.Mode == itemArgs.Mode,
				(x) => itemArgs.Mode == OrgCommissionAgreementItemLookups.AllModesCode || (!itemArgs.Mode.IsEmpty && x.Mode == OrgCommissionAgreementItemLookups.AllModesCode));
		}

		static IEnumerable<Func<OverallStaffCommissionRule, bool>> GetUnlocoMatchingPredicates(RefUNLOCO unloco, RefUNLOCO fallbackUnloco)
		{
			yield return (x) => x.Group is SalesTeam && ((SalesTeam)x.Group).Covers(unloco);

			var fallbackUnlocoSameAsMainUnloco = (unloco == null && fallbackUnloco == null) || (unloco != null && fallbackUnloco != null && unloco.PK == fallbackUnloco.PK);
			if (!fallbackUnlocoSameAsMainUnloco)
			{
				yield return (x) => x.Group is SalesTeam && ((SalesTeam)x.Group).Covers(fallbackUnloco);
			}

			yield return (x) => x.GroupPk.IsEmpty;
		}

		static IEnumerable<OverallStaffCommissionRule> GetMatchingRules(IEnumerable<OverallStaffCommissionRule> ruleCollection, IEnumerable<OverallStaffCommissionRulePredicateQueue> predicateQueues)
		{
			var firstPredicateQueue = predicateQueues.FirstOrDefault();
			if (firstPredicateQueue == null)
			{
				return ruleCollection;
			}

			foreach (var predicate in firstPredicateQueue.PredicatesInOrderOfPriority)
			{
				var matchingRulesOfFirstPredicateQueue = ruleCollection.Where(x => predicate(x)).ToArray();
				if (matchingRulesOfFirstPredicateQueue.Any())
				{
					var matchinesRulesOfAllPredicateQueues = GetMatchingRules(matchingRulesOfFirstPredicateQueue, predicateQueues.Skip(1));
					if (matchinesRulesOfAllPredicateQueues.Any())
					{
						return matchinesRulesOfAllPredicateQueues;
					}
				}
			}

			return Enumerable.Empty<OverallStaffCommissionRule>();
		}

		class OverallStaffCommissionRulePredicateQueue
		{
			public OverallStaffCommissionRulePredicateQueue(params Func<OverallStaffCommissionRule, bool>[] predicatesInOrderOfPriority)
			{
				PredicatesInOrderOfPriority = predicatesInOrderOfPriority;
			}

			public readonly Func<OverallStaffCommissionRule, bool>[] PredicatesInOrderOfPriority;
		}

		#endregion

		#region Overall Commission Rules View

		[ChildEditable]
		public OverallStaffCommissionRuleCollectionView OverallCommissionRulesView
		{
			get
			{
				if (overallCommissionRulesView == null)
				{
					overallCommissionRulesView = new OverallStaffCommissionRuleCollectionView(this);
					overallCommissionRulesView.LoadRules();
					RegisterEditableChildObject(overallCommissionRulesView);
				}

				return overallCommissionRulesView;
			}
		}
		OverallStaffCommissionRuleCollectionView overallCommissionRulesView;

		#endregion

		#region Reporting Roles

		[ChildEditable(true)]
		public GlbStaffManagerCollection Managers
		{
			get
			{
				if (managers == null)
				{
					managers = new GlbStaffManagerCollection(this, new ZQuery());
					RegisterEditableChildObject(managers);
				}
				return managers;
			}
		}
		GlbStaffManagerCollection managers;

		public GlbStaffManager[] GetCurrentManagers()
		{
			return Managers.Where(x => x.IsCurrentManager).ToArray();
		}

		public GlbStaffManager[] GetFutureManagers()
		{
			return Managers.Where(x => x.IsFutureManager).ToArray();
		}

		public GlbStaff CurrentDRMManager
			=> Managers.FirstOrDefault(x => x.IsCurrentManager && x.GSM_ManagerType == DefaultStaffReportingRoles.Codes.DirectManager)?.Manager;

		GlbStaffManagementTreeModel managementTreeModel;
		public GlbStaffManagementTreeModel ManagementTreeModel
		{
			get
			{
				if (managementTreeModel == null)
				{
					managementTreeModel = new GlbStaffManagementTreeModel(this);
					RegisterEditableChildObject(managementTreeModel);
				}
				return managementTreeModel;
			}
			set => managementTreeModel = value;
		}

		[ChildEditable(true)]
		public GlbStaffManagerCollection DirectReports
		{
			get
			{
				if (directReports == null)
				{
					directReports = new GlbStaffManagerCollection(this, new ZQuery(), isManager: true);
					RegisterEditableChildObject(directReports);
				}
				return directReports;
			}
		}
		GlbStaffManagerCollection directReports;

		public GlbStaffManager[] GetCurrentDirectReports()
		{
			return DirectReports.Where(x => x.IsCurrentManager).ToArray();
		}

		public GlbStaffManager[] GetFutureDirectReports()
		{
			return DirectReports.Where(x => x.IsFutureManager).ToArray();
		}

		GlbStaffManagementTreeModel directReportsTreeModel;
		public GlbStaffManagementTreeModel DirectReportsTreeModel
		{
			get
			{
				if (directReportsTreeModel == null)
				{
					directReportsTreeModel = new GlbStaffManagementTreeModel(this, isManager: true);
					RegisterEditableChildObject(directReportsTreeModel);
				}
				return directReportsTreeModel;
			}
		}

		public StaffReportingRoleCollection GetMissingMandatoryReportingRoles()
		{
			var reportingRoles = SystemDataRegistry.Instance.StaffReportingRoles.Value.ToArray<StaffReportingRole>();
			var missingMandatoryRoles = new StaffReportingRoleCollection();

			foreach (var role in reportingRoles)
			{
				if (role.IsMandatory)
				{
					if (role.IsMandatory && !Managers.Any(x => x.GSM_ManagerType == role.Code && x.IsCurrentManager))
					{
						missingMandatoryRoles.Add(role);
					}
				}
			}

			return missingMandatoryRoles;
		}

		public bool IsCurrentlyManagedBy(GlbStaff manager, ZString role)
		{
			return Managers.Any(x =>
				x.GSM_ManagerType == role &&
				x.GSM_GS_Manager == manager.PK &&
				x.IsCurrentManager);
		}

		public void RemoveCurrentManagers()
		{
			foreach (var manager in GetCurrentManagers().Where(x => !x.IsFutureManager))
			{
				manager.GSM_EndDate = ZDateTime.Today;
			}

			foreach (var manager in GetFutureManagers())
			{
				manager.Delete();
			}
		}

		#endregion

		#endregion

		#region IContactable Members

		string IContactBase.Name
		{
			get { return this.GS_FullName; }
		}

		string IContactBase.Email
		{
			get { return this.GS_EmailAddress; }
		}

		string IContactable.Mobile
		{
			get { return this.GS_MobilePhone; }
		}

		bool IContactable.IsActive
		{
			get { return GS_IsActive; }
		}

		IContactable[] IContactable.GetNestedContacts(string parentContactDescription)
		{
			return Array.Empty<IContactable>();
		}

		#endregion

		#region IAddressBookRecipient Members

		OrgHeader IAddressBookRecipient.Organisation
		{
			get { return null; }
		}

		ZString IAddressBookRecipient.Role
		{
			get { return string.Empty; }
		}

		ZString IAddressBookRecipient.Phone
		{
			get { return GS_WorkPhone; }
		}

		ZString IAddressBookRecipient.Title
		{
			get { return GS_Title; }
		}

		ZString IAddressBookRecipient.Location
		{
			get { return HomeBranch != null ? HomeBranch.GB_Address1 : ZString.Empty; }
		}

		bool IAddressBookRecipient.IsActive
		{
			get { return GS_IsActive; }
		}

		ZString IAddressBookRecipient.Name
		{
			get { return ((IContactBase)this).Name; }
		}

		ZString IAddressBookRecipient.Email
		{
			get { return ((IContactBase)this).Email; }
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Staff);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsSecurity Members

		public SecurityCheckpoint EdocsSecurityCheckpoint => Env.Security.ViewStaffeDocs;

		#endregion

		#region Logging

		public void ReviewPerformed(ZDateTime reviewDate)
		{
			Logs.AddNew(Events.StaffPerformanceReview, string.Format("Reviewed on {0}", reviewDate.ToShortDateString()));
			GS_NextReviewDate = ZDateTime.Empty;
		}

		#endregion

		#region IAllowedOrgsAndWarehousesAccessProvider Members

		public GlbSecurityAllowedOrgsAndWarehousesView SecurityAllowedOrgsAndWarehousesView
		{
			get
			{
				if (securityAllowedOrgsAndWarehousesView == null)
				{
					securityAllowedOrgsAndWarehousesView = new GlbSecurityAllowedOrgsAndWarehousesView(StaffSecurityPermissionsCollection, this);
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
				Type whsWarehouseCollectionType = CargoWise.Application.ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouseCollection>();
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
					newSecurity.GU_GS = PK;
					newSecurity.GU_ItemGUID = orgOrWarehouse.PK;
					newSecurity.GU_SecurityRight = SecurityAllowedOrgsAndWarehousesView.SecurityRight;
				}
			}
		}

		#endregion

		#region ISupportChangeOthersSecurity Members

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
			get { return GlbSecuritySchema.GU_GS; }
		}

		#endregion

		#region IDocumentSupportable Members

		GlbStaffDocumentSupporter fDocumentSupporter;
		public DocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new GlbStaffDocumentSupporter(this);
				}
				return fDocumentSupporter;
			}
		}

		#endregion

		#region Staff Security

		public GlbStaffSecurityCollection StaffSecurity
		{
			get
			{
				if (staffSecurity == null)
				{
					staffSecurity = new GlbStaffSecurityCollection(Factory);
					FillStaffSecurityCollection();
				}
				return staffSecurity;
			}
		}
		GlbStaffSecurityCollection staffSecurity;

		public bool IsDetailsModifiable => Env.Security.StaffDetails.IsAllowed
			|| Env.Security.StaffOwnDetails.IsAllowed && IsCurrentUser
			|| IsCurrentUserLocalAdminForThisStaff
			|| IsCurrentUserLocalAdminForAtLeastOneGroup && !IsInDatabase;

		void FillStaffSecurityCollection()
		{
			ISecurityMap securityMap = ObjectFactory.Get<ISecurityMap>();
			foreach (var data in securityMap.GetStaffSecurity(this))
			{
				GlbStaffSecurity item = new GlbStaffSecurity();
				item.SecurityRight = data[0];
				item.Summary = data[1];
				item.Explicit = data[2];
				StaffSecurity.Add(item);
			}
		}

		#endregion

		#region Active Directory

		public bool ADPasswordExpired
		{
			get { return GetADUser().PasswordExpired; }
		}

		public bool CanAccessDirectoryEntry()
		{
			try
			{
				return GetADUser().HasExistingDirectoryEntry();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return false;
			}
		}

		public bool ADPasswordMustChangeAtNextLogon => CanAccessDirectoryEntry() && GetADUser().PasswordMustChangeAtNextLogon;

		public int GetADNumberOfDaysTillPasswordExpiry()
		{
			return GetADUser().GetNumberOfDaysTillPasswordExpiry();
		}

		bool ADPasswordDoesntExpireUserAttribute => CanAccessDirectoryEntry() && GetADUser().PasswordDoesntExpireUserAttribute;

		DateTime ADPasswordLastSet => CanAccessDirectoryEntry() ? GetADUser().PasswordLastSet : DateTime.MinValue;

		public DateTime ADPasswordExpirationDate => CanAccessDirectoryEntry() ? ((IUserDirectoryEntry)GetADUser().GetDirectoryEntry()).PasswordExpirationDate : DateTime.MinValue;
		public bool ShouldUseADPasswordPolicy => IsADIntegrationEnabled && IsADLinked;

		public bool IsADLockedOut => CanAccessDirectoryEntry() && GetADUser().LockedOut;

		public bool AllowPasswordChange => !IsADIntegrationEnabled || !ObjectFactory.Get<IADRegistry>().DisableADPasswordChange;

		[ResourceStringData("GlbStaff.PasswordNeverChanges", Caption = "Password Never Expires")]
		[ReadOnlyMember(nameof(PasswordNeverChangesReadOnly))]
		public ZBool PasswordNeverChanges
		{
			get
			{
				if (ShouldUseADPasswordPolicy)
				{
					if (!passwordNeverChanges.HasValue)
					{
						passwordNeverChanges = ADPasswordDoesntExpireUserAttribute;
					}
					return passwordNeverChanges.Value;
				}
				else
				{
					return GS_PasswordNeverChanges;
				}
			}
			set
			{
				if (ShouldUseADPasswordPolicy)
				{
					SetNonPersistentPropertyValue(PasswordNeverChangesInfo, ref passwordNeverChanges, value);
					passwordNeverChangesIsManuallySet = true;
				}
				else
				{
					GS_PasswordNeverChanges = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidatePasswordNeverChanges();
					Validation.ValidateChangePasswordAtNextLogin();
				}
				PasswordNeverChangesInfo.RefreshBinding();
			}
		}
		ZBool? passwordNeverChanges;
		bool passwordNeverChangesIsManuallySet;

		public ZPropertyInfo PasswordNeverChangesInfo => GetZPropertyInfo(nameof(PasswordNeverChanges));

		protected bool PasswordNeverChangesReadOnly => ShouldDisablePasswordSettings;

		bool CanEditStaffPasswordAndSignature => Env.Security.StaffPasswordAndSignature.IsAllowed || IsCurrentUserLocalAdminForThisStaff;
		bool CannotAccessADUser => IsADIntegrationEnabled && IsADLinked && !CanAccessDirectoryEntry();
		public bool IsADPrimaryOneWaySync => IsADIntegrationEnabled && ObjectFactory.Get<IADRegistry>().SyncMode == SyncMode.ADIsMaster && ObjectFactory.Get<IADRegistry>().SyncDirection == SyncDirection.OneWay;

		public bool ShouldDisablePasswordSettings => !CanEditStaffPasswordAndSignature || IsADPrimaryOneWaySync || CannotAccessADUser;

		/// <summary>
		/// DO NOT USE this property, use PasswordNeverChanges
		/// </summary>
		[ReadOnly(true)]
		public override ZBool GS_PasswordNeverChanges
		{
			get => base.GS_PasswordNeverChanges;
			set => base.GS_PasswordNeverChanges = value;
		}

		[ResourceStringData("GlbStaff.ChangePasswordAtNextLogin", Caption = "Change Password at Next Login")]
		[ReadOnlyMember(nameof(ChangePasswordAtNextLoginReadOnly))]
		public ZBool ChangePasswordAtNextLogin
		{
			get
			{
				if (ShouldUseADPasswordPolicy)
				{
					if (!changePasswordAtNextLogin.HasValue)
					{
						changePasswordAtNextLogin = ADPasswordMustChangeAtNextLogon;
					}
					return changePasswordAtNextLogin.Value;
				}
				else
				{
					return GS_ChangePasswordAtNextLogin;
				}
			}
			set
			{
				if (ShouldUseADPasswordPolicy)
				{
					SetNonPersistentPropertyValue(ChangePasswordAtNextLoginInfo, ref changePasswordAtNextLogin, value);
					changePasswordAtNextLoginIsManuallySet = true;
				}
				else
				{
					GS_ChangePasswordAtNextLogin = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateChangePasswordAtNextLogin();
					Validation.ValidatePasswordNeverChanges();
				}
				ChangePasswordAtNextLoginInfo.RefreshBinding();
			}
		}
		ZBool? changePasswordAtNextLogin;
		bool changePasswordAtNextLoginIsManuallySet;

		public ZPropertyInfo ChangePasswordAtNextLoginInfo => GetZPropertyInfo(nameof(ChangePasswordAtNextLogin));

		protected bool ChangePasswordAtNextLoginReadOnly => ShouldDisablePasswordSettings || EnforceChangePasswordAtNextLogin;

		bool EnforceChangePasswordAtNextLogin => Env.Registry.RawRegistry.EnforceChangePasswordAtNextLogin.Value
			&& ChangePasswordAtNextLogin && !GS_ChangePasswordAtNextLoginInfo.HasChanges && !changePasswordAtNextLoginIsManuallySet;

		public void ReloadPasswordSettingsFromAD()
		{
			changePasswordAtNextLogin = null;
			passwordNeverChanges = null;
		}

		/// <summary>
		/// DO NOT USE this property, use ChangePasswordAtNextLogin
		/// </summary>
		[ReadOnly(true)]
		public override ZBool GS_ChangePasswordAtNextLogin
		{
			get => base.GS_ChangePasswordAtNextLogin;
			set => base.GS_ChangePasswordAtNextLogin = value;
		}

		[ResourceStringData("GlbStaff.LastPasswordChangeDate", Caption = "Last Password Change")]
		public ZDateTime LastPasswordChangeDate
		{
			get
			{
				if (ShouldUseADPasswordPolicy)
				{
					return ADPasswordLastSet;
				}
				else
				{
					return GS_LastPasswordChangeDate;
				}
			}
		}

		void SaveADPasswordPolicyIfRequired()
		{
			try
			{
				if (passwordNeverChangesIsManuallySet || changePasswordAtNextLoginIsManuallySet)
				{
					if (ShouldUseADPasswordPolicy && CanAccessDirectoryEntry())
					{
						if (passwordNeverChangesIsManuallySet)
						{
							if (!PasswordNeverChangesReadOnly)
							{
								CommitADUserAction(u => u.PasswordDoesntExpireUserAttribute = PasswordNeverChanges, u => u.PasswordDoesntExpireUserAttribute != PasswordNeverChanges);
							}
							else
							{
								throw new SecurityAccessDeniedException(Env.Security.GetErrorMessageForNotAllowed(Env.Security.StaffModifyAll));
							}
						}

						if (changePasswordAtNextLoginIsManuallySet)
						{
							if (!ChangePasswordAtNextLoginReadOnly)
							{
								CommitADUserAction(u => u.PasswordMustChangeAtNextLogon = ChangePasswordAtNextLogin, u => u.PasswordMustChangeAtNextLogon != ChangePasswordAtNextLogin);
							}
							else
							{
								throw new SecurityAccessDeniedException(Env.Security.GetErrorMessageForNotAllowed(Env.Security.StaffModifyAll));
							}
						}
					}
				}
			}
			catch (Exception ex) when (ex is NoDomainPrivilegeException || ex is DirectoryServicesException)
			{
				throw new ZCannotSaveException(ex.Message, Res.GetString("D6AA002D-5D39-4152-9955-16CC8147932D", "Cannot save"));
			}
		}

		public void SynchroniseWithAD()
		{
			if (Env.Security.StaffViewHomeAddressDetails.IsAllowed || IsCurrentUserLocalAdminForThisStaff)
			{
				if (GetADUser().DomainCredentials != null)
				{
					GetADUser().DomainCredentials.DefaultPasswordFailsToMeetDomainPolicy = false;
				}
				CommitADUserAction(u =>
				{
					ReloadSafe(); // ensure syncing latest data from DB
					u.Synchronise();
					if (GetADUser().DomainCredentials?.DefaultPasswordFailsToMeetDomainPolicy ?? false)
					{
						throw new PasswordDoesNotMatchPolicyException(Res.GetString("864DC336-282A-4B96-9F2F-5C8CDE557A22",
							@"The Domain Credentials Collection registry is using a password that does not meet the current password policy requirements of domain: {0}. Please contact your system administrator to correct this registry setting.
Synchronization with Active Directory will not work properly until this is fixed.",
							GetADUser().DomainCredentials.DomainName
						));
					}
				});
			}
			else
			{
				throw new SecurityAccessDeniedException(Env.Security.GetErrorMessageForNotAllowed(Env.Security.StaffViewHomeAddressDetails));
			}
		}

		public void DisconnectFromAD()
		{
			if (Env.Security.StaffModifyAll.IsAllowed)
			{
				GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				GS_IsActive = false;
				DeleteAllGlbGroupLinks();
				Groups.RemoveAll();
			}
			else
			{
				throw new SecurityAccessDeniedException(Env.Security.GetErrorMessageForNotAllowed(Env.Security.StaffModifyAll));
			}
		}

		public void UnlockADAccount()
		{
			CommitADUserAction(u => u.UnlockAccount());
		}

		public void UnlockAccount()
		{
			if (ShouldUseADPasswordPolicy)
			{
				UnlockADAccount();
			}
			else
			{
				var person = Person;
				if (person != null)
				{
					person.PER_LoginDisabledUntilUtc = ZDateTime.Empty;
					HasChanges = true;
				}

				if (IsInDatabase)
				{
					LoginAttemptRecorder.Unlock(GS_LoginName);
				}
			}
		}

		void CommitADUserAction(Action<IADUser> userAction, Func<IADUser, bool> conditionFunction = null)
		{
			var adUser = GetADUser();
			if (conditionFunction == null || conditionFunction(adUser))
			{
				userAction(adUser);
				adUser.CommitChanges();
				SyncUsersToRoboticGroupIfRequired(adUser);
			}
		}

		void SyncUsersToRoboticGroupIfRequired(IADUser adUser)
		{
			var syncDirector = ObjectFactory.Get<ISynchronisationDirectorProvider>().GetSyncDirector(Factory, new[] { this });
			syncDirector.SyncUsersToRoboticGroupIfRequired(new[] { adUser });
		}

		public bool IsLoginNameInConflictWithExistingADAccount()
		{
			return GetADUser().IsIdentityInConflict();
		}

		IADUser GetADUser()
		{
			return ObjectFactory.Get<IADEntityProvider>().GetADUser(this);
		}

		public string ADLoginName => GetADUser().LoginName;

		public string ADLoginNamePreWin2K => GetADUser().SAMAccountName;

		public string GetDownLevelLogonName()
		{
			try
			{
				return !IsDeleted && IsADIntegrationEnabled && IsADLinked ? GetADUser().DomainNetBiosName + '\\' + ADLoginNamePreWin2K : null;
			}
			catch (RowNotInTableException)
			{
				return null;
			}
		}

		public static string GetEmailAddressFromUserCode(BusinessObjectFactory factory, ZString userCode)
		{
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, userCode);
			if (staff != null && !staff.GS_IsSystemAccount && staff.GS_EmailAddress.IsValid && staff.GS_IsActive)
			{
				return staff.GS_EmailAddress;
			}

			return string.Empty;
		}

		public bool IsADIntegrationEnabled
		{
			get { return ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled; }
		}

		bool IADLinkedEntity.IsActive
		{
			get { return GS_IsActive; }
		}

		ZBool IADLinkedEntity.IsADLinked
		{
			get { return IsADLinked; }
		}

		internal bool MissingDirectoryEntry => !IsDeleted && CannotAccessADUser;

		bool IADLinkedEntity.IsADLinkable => !GS_IsResource;

		ZDateTime IADLinkedEntity.SystemCreateTimeUtc => GS_SystemCreateTimeUtc;

		ZDateTime IADLinkedEntity.SystemLastEditTimeUtc => GS_SystemLastEditTimeUtc;

		#endregion

		#region WantsToSeeDocumentTemplateErrors - DEBUG Only

		public bool WantsToSeeDocumentTemplateErrors
		{
			get
			{
#if DEBUG
				return fWantsToSeeDocumentTemplateErrors;
#else
				return false;
#endif
			}
		}

#if DEBUG
		bool fWantsToSeeDocumentTemplateErrors;

		public void SetWantsToSeeDocumentTemplateErrorsForTesting(bool value)
		{
			fWantsToSeeDocumentTemplateErrors = value;
		}
#endif

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return Env.Security.StaffModifyAll.IsAllowed && Env.CurrentUser.PK != PK;
			}
		}

		#endregion

		#region ICancellable

		public override string CanCancel()
		{
			if (GS_IsActiveInfo.HasErrors())
			{
				return Res.GetString("379A3BCF-5965-4257-8788-9DABF840D853", "Please fix errors on {0} before deactivating.", HumanReadableName);
			}
			return string.Empty;
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
			return SystemDataRegistry.Instance.StaffCertificateTypes.Value.GetCodeDescriptionPairList();
		}

		ICodeDescriptionPairList ICertificatesProvider.GetActiveCertificateTypeList()
		{
			return SystemDataRegistry.Instance.StaffCertificateTypes.Value.GetActiveCodeDescriptionPairList();
		}

		ZString ICertificatesProvider.GetDefaultDescription(ZString code)
		{
			return ZString.Empty;
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		CustomBusinessObject CustomBusinessObject
		{
			get
			{
				var customBusinessObjectProcessTaskTemplateLoader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), Factory);
				var matches = customBusinessObjectProcessTaskTemplateLoader.FindMatches(this);

				var templateKeys = matches.Matches.Select(t => t.Identifier).ToArray();

				if (lastCustomBizoTemplates == null || !templateKeys.SequenceEqual(lastCustomBizoTemplates))
				{
					customBusinessObject = null;
					lastCustomBizoTemplates = templateKeys;
				}

				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this);

					properties.Add(matches);

					customBusinessObject = new CustomBusinessObject(Factory, this, properties);
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;
		ZGuid[] lastCustomBizoTemplates;

		#endregion

		#region IUser Members

		string IUser.EmailAddress
		{
			get { return GS_EmailAddress; }
		}

		string IUser.Fax
		{
			get { return GS_FaxNum; }
		}

		string IUser.FullName
		{
			get { return GS_FullName; }
		}

		string IUser.Initials
		{
			get { return GS_Code; }
		}

		bool IUser.IsActive
		{
			get { return GS_IsActive; }
		}

		bool IUser.IsBatchProcessor
		{
			get
			{
#if DEBUG
				if (((IUser)this).BatchProcessorOverride != null)
				{
					return true;
				}
#endif
				return IsBatchProcessor(GS_Code);
			}
		}

		public static bool IsBatchProcessor(ZString code)
		{
			return code == User.ServiceUserCode || code == User.InterchangeUserCode;
		}

		bool IUser.IsController
		{
			get { return GS_IsController; }
		}

		bool IUser.IsDeveloper
		{
			get { return GS_IsDeveloper; }
		}

		bool IUser.IsDeviceOnly
		{
			get { return GS_IsDevice; }
		}

		bool IUser.IsLockedOut => IsLockedOut;

		bool IUser.IsRobot => GS_IsRobot;

		IGlbStaffLoginAttemptRecorder loginAttemptRecorder;
		IGlbStaffLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IGlbStaffLoginAttemptRecorder>());

		public bool IsLockedOut
		{
			get
			{
				if (ShouldUseADPasswordPolicy)
				{
					return IsADLockedOut;
				}
				else
				{
					var lockedOut = Person?.PER_LoginDisabledUntilUtc > ZDateTime.UtcNow
						|| (IsInDatabase && LoginAttemptRecorder.IsAnonymousUserLockedOut(GS_LoginName));

					return lockedOut;
				}
			}
		}

		bool IUser.CanLogin
		{
			get { return GS_CanLogin; }
		}

		bool IUser.IsResource
		{
			get { return GS_IsResource; }
		}

		bool IUser.IsOperational
		{
			get { return GS_IsOperational; }
		}

		bool IUser.IsSystemAccount
		{
			get { return GS_IsSystemAccount; }
		}

		bool IUser.IsWebUser
		{
			get { return GS_IsSystemAccount && GS_LoginName.EqualsIgnoringCase(User.WebUserName); }
		}

		string IUser.Language
		{
			get { return GS_WorkingLanguage; }
		}

		string IUser.LoginName
		{
			get { return GS_LoginName; }
		}

		Guid IUser.PK
		{
			get { return PK.ToGuid(); }
		}
		string IUser.Title
		{
			get { return GS_Title; }
		}

		string IUser.WorkPhone
		{
			get { return GS_WorkPhone; }
		}

		bool IUser.IsDeveloperLogin
		{
			get { return ((IUser)this).LoginToken != null && ((IUser)this).LoginToken.IsDeveloper; }
		}

		string IUser.InitialsAndDateTime
		{
			get { return ((IUser)this).Initials.Trim() + " " + Env.Time.FormatDateTime(Env.Time.CurrentLocalDateTime) + ": "; }
		}

		string IUser.InitialsAndDateTimeGmt
		{
			get { return GS_Code + ' ' + Env.Time.CurrentLocalDateTimeIncludingGmt + ": "; }
		}

		bool IUser.LoggedInWithMasterPassword
		{
			get { return ((IUser)this).LoginToken != null && ((IUser)this).LoginToken.LoggedInWithSupportToken; }
		}

		string IUser.ActivityTrackingStatus => GS_ActivityTrackingStatus;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sysadmin is a constant for account name")]
		bool IUser.IsSysAdmin
		{
			get { return ((IUser)this).LoginName == "sysadmin"; }
		}

		public bool IsTwoFactorAuthenticationEnabled
		{
			set { GS_IsTwoFactorAuthenticationEnabled = value; }

			get
			{
				return GS_IsTwoFactorAuthenticationEnabled;
			}
		}

		ZDateTime IUser.LastActivityDateTimeUtc => GS_LastActivityDate;

#if DEBUG

		User.IsBatchProcessorOverride IUser.BatchProcessorOverride
		{
			get;
			set;
		}

		IDisposable IUser.SetIsControllerOverrideForTesting(bool value)
		{
			GS_IsController = value;
			return new DisposableAction(() => GS_IsController = (ZBool)GS_IsControllerInfo.OriginalValue);
		}

		IDisposable IUser.SetUserEmailAddressInTESTINGOnly(string email)
		{
			GS_EmailAddress = email;
			return new DisposableAction(() => GS_EmailAddress = (ZString)GS_EmailAddressInfo.OriginalValue);
		}

#endif

		ILoginToken IUser.LoginToken
		{
			get;
			set;
		}

		#endregion

		#region ComponentMembership

		[ChildEditable]
		public IBMComponentResourceLinkCollection ComponentMembership
		{
			get
			{
				if (componentMembership == null)
				{
					componentMembership = ObjectFactory.Get<IBMComponentResourceLinkCollection>("IBMComponentResourceLinkCollection", this);
					RegisterEditableChildObject(componentMembership);
				}

				return componentMembership;
			}
		}
		IBMComponentResourceLinkCollection componentMembership;

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
			get { return GS_UserAddress1; }
			set { GS_UserAddress1 = value; }
		}

		public ZPropertyInfo Address1Info
		{
			get { return GS_UserAddress1Info; }
		}

		public int Address1_MaxLength
		{
			get { return AutoGlbStaff.Schema.GS_UserAddress1MaxLength; }
		}

		public ZString Address2
		{
			get { return GS_UserAddress2; }
			set { GS_UserAddress2 = value; }
		}

		public ZPropertyInfo Address2Info
		{
			get { return GS_UserAddress2Info; }
		}

		public int Address2_MaxLength
		{
			get { return AutoGlbStaff.Schema.GS_UserAddress2MaxLength; }
		}

		public ZString City
		{
			get { return GS_City; }
			set { GS_City = value; }
		}

		public ZPropertyInfo CityInfo
		{
			get { return GS_CityInfo; }
		}

		public int City_MaxLength
		{
			get { return AutoGlbStaff.Schema.GS_CityMaxLength; }
		}

		[BusinessObjectTestExclude] // State has to be valid
		public ZString State
		{
			get { return StateCodeList.GetDescriptionFromCode(GS_State); }
			set
			{
				var code = (ZString)StateCodeList.GetCodeFromDescription(value);
				GS_State = string.IsNullOrEmpty(code) ? value : code;
				StateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateInfo
		{
			get { return GetZPropertyInfo(nameof(State)); }
		}

		public int State_MaxLength
		{
			get { return AutoGlbStaff.Schema.GS_StateMaxLength; }
		}

		public ZString StateCode
		{
			get { return GS_State; }
			set
			{
				GS_State = value;
				StateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateCodeInfo
		{
			get { return GetZPropertyInfo(nameof(StateCode)); }
		}

		public int StateCode_MaxLength
		{
			get { return AutoGlbStaff.Schema.GS_StateMaxLength; }
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
			get { return GS_Postcode; }
			set { GS_Postcode = value; }
		}

		public ZPropertyInfo PostcodeInfo
		{
			get { return GS_PostcodeInfo; }
		}

		public int Postcode_MaxLength
		{
			get { return AutoGlbStaff.Schema.GS_PostcodeMaxLength; }
		}

		public ZString AddressMap
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? addressMap : ViewDeniedMessage; }
			set { addressMap = value; }
		}

		ZString addressMap;

		[List("CountryCodeList")]
		ZString ISupportWebAddressValidation.CountryCodeISO2
		{
			get { return this.GS_RN_NKCountryCode; }
			set { GS_RN_NKCountryCode = value; }
		}

		public int CountryCodeISO2_MaxLength => Schema.GS_RN_NKCountryCodeMaxLength;

		public RefCountryCollection CountryCodeList
		{
			get { return Lookups.Countries; }
		}

		[BusinessObjectTestExclude]
		public ZString Language
		{
			get { return GS_WorkingLanguage; }
			set { }
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(nameof(Language)); }
		}

		public int Language_MaxLength { get { return AutoGlbStaff.Schema.GS_WorkingLanguageMaxLength; } }

		public CodeDescriptionPairList LanguageList { get { return null; } }

		public ZString ValidationStatus
		{
			get
			{
				return GS_ValidationStatus;
			}
			set
			{
				GS_ValidationStatus = value;
				RaiseAddressValidationStatusChanged();
			}
		}

		public ZString Addressee
		{
			get { return GS_FullName; }
		}

		public ZGeography GeoLocation
		{
			get { return GS_GeoLocation; }
			set { GS_GeoLocation = value; }
		}

		public ZString ClosestPort { get; set; }

		public AddressValidationSection ValidationSection { get; } = AddressValidationSection.Staff;

		bool isManuallyVerifiedByUser;

		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			if (isManuallyVerifiedByUser)
			{
				return;
			}

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
				else if (propertyInfo.Name == nameof(GS_RN_NKCountryCode))
				{
					ValidationStatus = AddressValidationStatus.ToBeVerified;
					AddressMap = string.Empty;
				}
			}
		}

		public async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, WTG.AddressCleansing.Common.CleanseAction cleanseAction = WTG.AddressCleansing.Common.CleanseAction.ValidateAndSuggest)
		{
			return await AddressValidationService.ValidateAddressAsync(this, cancellationToken, false, cleanseAction);
		}

		public async Task<WTG.AddressCleansing.Common.CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken)
		{
			return await AddressValidationService.GetCityTownAsync(this, cancellationToken);
		}

		public bool IsErrorSuppressed => false;
		public bool IsJobDocAddress => false;

		public event EventHandler AddressValidationStatusChanged;
		public event EventHandler TriggerWebAddressValidation;
		public event EventHandler TriggerWebGetCityTown;

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

		void RaiseAddressValidationStatusChanged()
		{
			if (AddressValidationStatusChanged != null)
			{
				AddressValidationStatusChanged(this, EventArgs.Empty);
			}
		}

		void RaiseWebServices(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(GS_RN_NKCountryCode))
			{
				if ((string.IsNullOrEmpty(GS_City) || string.IsNullOrEmpty(GS_State) || string.IsNullOrEmpty(GS_Postcode)) && (propertyInfo == GS_CityInfo || propertyInfo == GS_StateInfo || propertyInfo == GS_PostcodeInfo))
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

		public bool NeedValidation
		{
			get
			{
				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, GS_RN_NKCountryCode);
				if (country != null)
				{
					if (!GS_UserAddress1.IsEmpty && !GS_Postcode.IsEmpty && !GS_City.IsEmpty && !GS_State.IsEmpty)
					{
						if (!IsInDatabase || (GS_UserAddress1Info.HasChanges || GS_UserAddress2Info.HasChanges || GS_PostcodeInfo.HasChanges ||
										GS_CityInfo.HasChanges || GS_StateInfo.HasChanges || GS_RN_NKCountryCodeInfo.HasChanges))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public void PreValidationForAddressValidationService()
		{
			Validation.ValidateGS_UserAddress1();
			Validation.ValidateGS_City();
			Validation.ValidateGS_RN_NKCountryCode();
			ValidatePostcodeAndStateForAddress();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Validation.ValidateGS_Postcode();
			Validation.ValidateGS_State();
		}

		public bool IsUpdatingCityTown { get; set; }
		public bool IsValidatingAddress { get; set; }
		public bool IsExactPointFound { get; set; }
		public bool IsValidatedByBackgroundService { get; set; }

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
			get { return GS_Code; }
			set { GS_Code = value; }
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

		#endregion

		#region ICertificatesValidationProvider

		GenRegCertAccredMaintListValidation ICertificatesValidationProvider.GetValidation(GenRegCertAccredMaintList parent)
		{
			ICertificatesValidationProvider glbStaffCertificatesValidationProvider = null;
			string countryCode = parent.XZ_RN_NKCountryOfIssuance;
			var writers = ObjectFactory.Get<Hashtable>("GlbStaffCertificatesValidationProvider");
			var objectHandle = (ObjectHandle)writers[countryCode];
			if (objectHandle != null)
			{
				glbStaffCertificatesValidationProvider = (ICertificatesValidationProvider)objectHandle.GetObject();
			}

			return glbStaffCertificatesValidationProvider?.GetValidation(parent);
		}

		#endregion

		#region IConversationParticipant

		ZString IConversationParticipant.Language => GS_WorkingLanguage;
		ZString IConversationParticipant.Email => GS_EmailAddress;
		ZString IConversationParticipant.Code => GS_Code;
		ZString IConversationParticipant.Name => GS_FullName;
		ZString IConversationParticipant.JobTitle => GS_Title;
		ZString IConversationParticipant.OrganisationName => string.Empty;
		ZString IConversationParticipant.Location => HomeBranch?.GB_RL_NKHomePort ?? string.Empty;
		ZBool IConversationParticipant.IsActive => GS_IsActive;
		ZBool IConversationParticipant.IsInternal => true;
		ZString IConversationParticipant.DisplayText => GS_FullName + " (" + GS_Code + ")";
		void IConversationParticipant.CheckCanParticipate(INotifications notifications) { }

		#endregion

		#region IAuditParent Members

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(GlbStaffHolidaySchema.GA_GS, null);
				yield return new AuditChildInfo(GlbWorkTimeSchema.GW_ParentID, null);
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.GlbStaffFetchStrategy(this);
		}

		#endregion

		#region PhoneNumbers

		#region HomePhone

		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		public override ZString GS_HomePhone
		{
			get => ViewingRights.ViewHomeAddressAllowedForStaff ? base.GS_HomePhone : ViewDeniedMessage;
			set
			{
				if (base.GS_HomePhone != value)
				{
					base.GS_HomePhone = value;
					Person?.FindDuplicates(this);
				}
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		public ZString GS_HomePhone_Formatted
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumber(GS_HomePhoneInfo) : ViewDeniedMessage; }
			set
			{
				if (ViewingRights.ViewHomeAddressAllowedForStaff)
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(GS_HomePhoneInfo, GS_HomePhone_FormattedInfo, value, Validation.ValidateGS_HomePhone_Formatted, GS_HomePhone_IsManuallyVerifiedInfo);
				}
			}
		}

		public ZPropertyInfo GS_HomePhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_HomePhone_Formatted); }
		}

		[ReadOnlyMember(nameof(ViewHomeAddressNotAllowedForStaff))]
		public ZString GS_HomePhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GS_HomePhoneInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo GS_HomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GS_HomePhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GS_HomePhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbStaffSchema.Constants.GS_HomePhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GS_HomePhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbStaffSchema.Constants.GS_HomePhone, Validation.ValidateGS_HomePhone_Formatted, GS_HomePhone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GS_HomePhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_HomePhone_IsManuallyVerified); }
		}

		public PhoneNumber GS_HomePhone_Wrapper
		{
			get { return gsHomePhoneWrapper ?? (gsHomePhoneWrapper = new PhoneNumber(GS_HomePhone_FormattedInfo, GS_PublishHomePhoneInfo, GS_HomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo, GS_HomePhone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gsHomePhoneWrapper;

		#endregion

		#region WorkPhone

		[ReadOnlyMember(nameof(IsControlledByScim))]
		public override ZString GS_WorkPhone
		{
			get => base.GS_WorkPhone;
			set
			{
				if (base.GS_WorkPhone != value)
				{
					base.GS_WorkPhone = value;
					Person?.FindDuplicates(this);
				}
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(IsControlledByScim))]
		public ZString GS_WorkPhone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(GS_WorkPhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(GS_WorkPhoneInfo, GS_WorkPhone_FormattedInfo, value, Validation.ValidateGS_WorkPhone_Formatted, GS_WorkPhone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo GS_WorkPhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_WorkPhone_Formatted); }
		}

		public ZString GS_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GS_WorkPhoneInfo); }
		}

		public ZPropertyInfo GS_WorkPhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GS_WorkPhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GS_WorkPhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbStaffSchema.Constants.GS_WorkPhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GS_WorkPhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbStaffSchema.Constants.GS_WorkPhone, Validation.ValidateGS_WorkPhone_Formatted, GS_WorkPhone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GS_WorkPhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_WorkPhone_IsManuallyVerified); }
		}

		public PhoneNumber GS_WorkPhone_Wrapper
		{
			get { return gsWorkPhoneWrapper ?? (gsWorkPhoneWrapper = new PhoneNumber(GS_WorkPhone_FormattedInfo, GS_PublishWorkPhoneInfo, GS_WorkPhone_FormattedLocalNumberIfLoggedInSameCountryInfo, GS_WorkPhone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gsWorkPhoneWrapper;

		#endregion

		#region WorkExtension

		public PhoneNumber GS_WorkExtension_Wrapper
		{
			get { return gsWorkExtensionWrapper ?? (gsWorkExtensionWrapper = new PhoneNumber(GS_WorkExtensionInfo, GS_PublishWorkExtensionInfo, null)); }
		}

		PhoneNumber gsWorkExtensionWrapper;

		#endregion

		#region MobilePhone

		protected bool ViewMobilePhoneNotAllowedForStaff
		{
			get { return !ViewingRights.ViewMobilePhoneAllowedForStaff || IsControlledByScim; }
		}

		[ReadOnlyMember(nameof(ViewMobilePhoneNotAllowedForStaff))]
		public override ZString GS_MobilePhone
		{
			get => ViewingRights.ViewMobilePhoneAllowedForStaff ? base.GS_MobilePhone : ViewDeniedMessage;
			set
			{
				if (base.GS_MobilePhone != value)
				{
					base.GS_MobilePhone = value;
					Person?.FindDuplicates(this);
				}
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(ViewMobilePhoneNotAllowedForStaff))]
		public ZString GS_MobilePhone_Formatted
		{
			get { return ViewingRights.ViewMobilePhoneAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumber(GS_MobilePhoneInfo) : ViewDeniedMessage; }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(GS_MobilePhoneInfo, GS_MobilePhone_FormattedInfo, value, Validation.ValidateGS_MobilePhone_Formatted, GS_MobilePhone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo GS_MobilePhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_MobilePhone_Formatted); }
		}

		[ReadOnlyMember(nameof(ViewMobilePhoneNotAllowedForStaff))]
		public ZString GS_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewingRights.ViewMobilePhoneAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GS_MobilePhoneInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo GS_MobilePhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GS_MobilePhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GS_MobilePhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbStaffSchema.Constants.GS_MobilePhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GS_MobilePhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbStaffSchema.Constants.GS_MobilePhone, Validation.ValidateGS_MobilePhone_Formatted, GS_MobilePhone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GS_MobilePhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_MobilePhone_IsManuallyVerified); }
		}

		public PhoneNumber GS_MobilePhone_Wrapper
		{
			get { return gsMobilePhoneWrapper ?? (gsMobilePhoneWrapper = new PhoneNumber(GS_MobilePhone_FormattedInfo, GS_PublishMobilePhoneInfo, GS_MobilePhone_FormattedLocalNumberIfLoggedInSameCountryInfo, GS_MobilePhone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gsMobilePhoneWrapper;

		#endregion

		#region GS_WorkExtension

		public override ZString GS_WorkExtension
		{
			get => ViewingRights.ViewWorkExtensionAllowedForStaff ? base.GS_WorkExtension : ViewDeniedMessage;
			set => base.GS_WorkExtension = value;
		}

		protected bool GS_WorkExtension_ReadOnly
		{
			get { return !ViewingRights.ViewWorkExtensionAllowedForStaff; }
		}

		#endregion

		#region GS_Passport

		public override ZString GS_Passport
		{
			get => ViewingRights.ViewPassportAllowedForStaff ? base.GS_Passport : ViewDeniedMessage;
			set => base.GS_Passport = value;
		}

		protected bool GS_Passport_ReadOnly
		{
			get { return !ViewingRights.ViewPassportAllowedForStaff; }
		}

		#endregion

		#region FaxNum

		[ReadOnlyMember(nameof(IsControlledByScim))]
		public override ZString GS_FaxNum
		{
			get => base.GS_FaxNum;
			set
			{
				if (base.GS_FaxNum != value)
				{
					base.GS_FaxNum = value;
					Person?.FindDuplicates(this);
				}
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(IsControlledByScim))]
		public ZString GS_FaxNum_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(GS_FaxNumInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(GS_FaxNumInfo, GS_FaxNum_FormattedInfo, value, Validation.ValidateGS_FaxNum_Formatted, GS_FaxNum_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo GS_FaxNum_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_FaxNum_Formatted); }
		}

		public ZString GS_FaxNum_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GS_FaxNumInfo); }
		}

		public ZPropertyInfo GS_FaxNum_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GS_FaxNum_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GS_FaxNum_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbStaffSchema.Constants.GS_FaxNum, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GS_FaxNum_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbStaffSchema.Constants.GS_FaxNum, Validation.ValidateGS_FaxNum_Formatted, GS_FaxNum_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GS_FaxNum_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_FaxNum_IsManuallyVerified); }
		}

		public PhoneNumber GS_FaxNum_Wrapper
		{
			get { return gsFaxNumWrapper ?? (gsFaxNumWrapper = new PhoneNumber(GS_FaxNum_FormattedInfo, GS_PublishFaxNumInfo, GS_FaxNum_FormattedLocalNumberIfLoggedInSameCountryInfo, GS_FaxNum_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gsFaxNumWrapper;

		#endregion

		#region NextOfKinHomePhone

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		public override ZString GS_NextOfKinHomePhone
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_NextOfKinHomePhone : ViewDeniedMessage; }
			set { base.GS_NextOfKinHomePhone = value; }
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		public ZString GS_NextOfKinHomePhone_Formatted
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumber(GS_NextOfKinHomePhoneInfo) : ViewDeniedMessage; }
			set
			{
				if (ViewingRights.ViewEmergencyContactAllowedForStaff)
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(GS_NextOfKinHomePhoneInfo, GS_NextOfKinHomePhone_FormattedInfo, value, Validation.ValidateGS_NextOfKinHomePhone_Formatted, GS_NextOfKinHomePhone_IsManuallyVerifiedInfo);
					if (EmergencySameAsNextOfKin)
					{
						PhoneNumberPropertyHelper.SetPhoneNumber(GS_EmergencyHomePhoneInfo, GS_EmergencyHomePhone_FormattedInfo, value, Validation.ValidateGS_EmergencyHomePhone_Formatted, GS_EmergencyHomePhone_IsManuallyVerifiedInfo);
					}
				}
			}
		}

		public ZPropertyInfo GS_NextOfKinHomePhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_NextOfKinHomePhone_Formatted); }
		}

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		public ZString GS_NextOfKinHomePhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GS_NextOfKinHomePhoneInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo GS_NextOfKinHomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GS_NextOfKinHomePhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GS_NextOfKinHomePhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbStaffSchema.Constants.GS_NextOfKinHomePhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GS_NextOfKinHomePhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbStaffSchema.Constants.GS_NextOfKinHomePhone, Validation.ValidateGS_NextOfKinHomePhone_Formatted, GS_NextOfKinHomePhone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GS_NextOfKinHomePhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_NextOfKinHomePhone_IsManuallyVerified); }
		}

		public PhoneNumber GS_NextOfKinHomePhone_Wrapper
		{
			get { return gsNextOfKinHomePhoneWrapper ?? (gsNextOfKinHomePhoneWrapper = new PhoneNumber(GS_NextOfKinHomePhone_FormattedInfo, null, GS_NextOfKinHomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo, GS_NextOfKinHomePhone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gsNextOfKinHomePhoneWrapper;

		#endregion

		#region NextOfKinWorkPhone

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		public override ZString GS_NextOfKinWorkPhone
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_NextOfKinWorkPhone : ViewDeniedMessage; }
			set { base.GS_NextOfKinWorkPhone = value; }
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		public ZString GS_NextOfKinWorkPhone_Formatted
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumber(GS_NextOfKinWorkPhoneInfo) : ViewDeniedMessage; }
			set
			{
				if (ViewingRights.ViewEmergencyContactAllowedForStaff)
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(GS_NextOfKinWorkPhoneInfo, GS_NextOfKinWorkPhone_FormattedInfo, value, Validation.ValidateGS_NextOfKinWorkPhone_Formatted, GS_NextOfKinWorkPhone_IsManuallyVerifiedInfo);
					if (EmergencySameAsNextOfKin)
					{
						PhoneNumberPropertyHelper.SetPhoneNumber(GS_EmergencyWorkPhoneInfo, GS_EmergencyWorkPhone_FormattedInfo, value, Validation.ValidateGS_EmergencyWorkPhone_Formatted, GS_EmergencyWorkPhone_IsManuallyVerifiedInfo);
					}
				}
			}
		}

		public ZPropertyInfo GS_NextOfKinWorkPhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_NextOfKinWorkPhone_Formatted); }
		}

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaff))]
		public ZString GS_NextOfKinWorkPhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GS_NextOfKinWorkPhoneInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo GS_NextOfKinWorkPhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GS_NextOfKinWorkPhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GS_NextOfKinWorkPhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbStaffSchema.Constants.GS_NextOfKinWorkPhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GS_NextOfKinWorkPhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbStaffSchema.Constants.GS_NextOfKinWorkPhone, Validation.ValidateGS_NextOfKinWorkPhone_Formatted, GS_NextOfKinWorkPhone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GS_NextOfKinWorkPhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_NextOfKinWorkPhone_IsManuallyVerified); }
		}

		public PhoneNumber GS_NextOfKinWorkPhone_Wrapper
		{
			get { return gsNextOfKinWorkPhoneWrapper ?? (gsNextOfKinWorkPhoneWrapper = new PhoneNumber(GS_NextOfKinWorkPhone_FormattedInfo, null, GS_NextOfKinWorkPhone_FormattedLocalNumberIfLoggedInSameCountryInfo, GS_NextOfKinWorkPhone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gsNextOfKinWorkPhoneWrapper;

		#endregion

		#region EmergencyHomePhone

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		public override ZString GS_EmergencyHomePhone
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_EmergencyHomePhone : ViewDeniedMessage; }
			set { base.GS_EmergencyHomePhone = value; }
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		public ZString GS_EmergencyHomePhone_Formatted
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumber(GS_EmergencyHomePhoneInfo) : ViewDeniedMessage; }
			set
			{
				if (ViewingRights.ViewEmergencyContactAllowedForStaff)
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(GS_EmergencyHomePhoneInfo, GS_EmergencyHomePhone_FormattedInfo, value, Validation.ValidateGS_EmergencyHomePhone_Formatted, GS_EmergencyHomePhone_IsManuallyVerifiedInfo);
				}
			}
		}

		public ZPropertyInfo GS_EmergencyHomePhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_EmergencyHomePhone_Formatted); }
		}

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		public ZString GS_EmergencyHomePhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GS_EmergencyHomePhoneInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo GS_EmergencyHomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GS_EmergencyHomePhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GS_EmergencyHomePhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbStaffSchema.Constants.GS_EmergencyHomePhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GS_EmergencyHomePhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbStaffSchema.Constants.GS_EmergencyHomePhone, Validation.ValidateGS_EmergencyHomePhone_Formatted, GS_EmergencyHomePhone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GS_EmergencyHomePhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_EmergencyHomePhone_IsManuallyVerified); }
		}

		public PhoneNumber GS_EmergencyHomePhone_Wrapper
		{
			get { return gsEmergencyHomePhoneWrapper ?? (gsEmergencyHomePhoneWrapper = new PhoneNumber(GS_EmergencyHomePhone_FormattedInfo, null, GS_EmergencyHomePhone_FormattedLocalNumberIfLoggedInSameCountryInfo, GS_EmergencyHomePhone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gsEmergencyHomePhoneWrapper;

		#endregion

		#region EmergencyWorkPhone

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		public override ZString GS_EmergencyWorkPhone
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? base.GS_EmergencyWorkPhone : ViewDeniedMessage; }
			set { base.GS_EmergencyWorkPhone = value; }
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		public ZString GS_EmergencyWorkPhone_Formatted
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumber(GS_EmergencyWorkPhoneInfo) : ViewDeniedMessage; }
			set
			{
				if (ViewingRights.ViewEmergencyContactAllowedForStaff)
				{
					PhoneNumberPropertyHelper.SetPhoneNumber(GS_EmergencyWorkPhoneInfo, GS_EmergencyWorkPhone_FormattedInfo, value, Validation.ValidateGS_EmergencyWorkPhone_Formatted, GS_EmergencyWorkPhone_IsManuallyVerifiedInfo);
				}
			}
		}

		public ZPropertyInfo GS_EmergencyWorkPhone_FormattedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_EmergencyWorkPhone_Formatted); }
		}

		[ReadOnlyMember(nameof(ViewEmergencyContactNotAllowedForStaffOrEmergencySameAsNextOfKin))]
		public ZString GS_EmergencyWorkPhone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return ViewingRights.ViewEmergencyContactAllowedForStaff ? PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(GS_EmergencyWorkPhoneInfo) : ViewDeniedMessage; }
		}

		public ZPropertyInfo GS_EmergencyWorkPhone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get { return GetZPropertyInfo(Schema.GS_EmergencyWorkPhone_FormattedLocalNumberIfLoggedInSameCountry); }
		}

		public ZBool GS_EmergencyWorkPhone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, GlbStaffSchema.Constants.GS_EmergencyWorkPhone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(GS_EmergencyWorkPhone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, GlbStaffSchema.Constants.GS_EmergencyWorkPhone, Validation.ValidateGS_EmergencyWorkPhone_Formatted, GS_EmergencyWorkPhone_Wrapper.FormattedForBindingInfo); }
		}

		public ZPropertyInfo GS_EmergencyWorkPhone_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.GS_EmergencyWorkPhone_IsManuallyVerified); }
		}

		public PhoneNumber GS_EmergencyWorkPhone_Wrapper
		{
			get { return gsEmergencyWorkPhoneWrapper ?? (gsEmergencyWorkPhoneWrapper = new PhoneNumber(GS_EmergencyWorkPhone_FormattedInfo, null, GS_EmergencyWorkPhone_FormattedLocalNumberIfLoggedInSameCountryInfo, GS_EmergencyWorkPhone_IsManuallyVerifiedInfo)); }
		}

		PhoneNumber gsEmergencyWorkPhoneWrapper;

		#endregion

		#region AddOnRuleAcks

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

		#region DefaultCountryCodeForPhoneNumbers

		public ZString DefaultCountryCodeForPhoneNumbers
		{
			get { return !GS_RN_NKCountryCode.IsEmpty ? GS_RN_NKCountryCode : HomeBranch?.DefaultCountryCodeForPhoneNumbers ?? ZString.Empty; }
		}

		#endregion

		#region Implementations

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelper ?? (phoneNumberPropertyHelper = new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers)); }
		}

		[DocumentFieldExcludeFromMap]
		[BusinessObjectTestExclude]
		public ZString UnrestrictedAdditionalAddressInformation
		{ get; set; }

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => GetZPropertyInfo(nameof(UnrestrictedAdditionalAddressInformation));

		[BusinessObjectTestExclude]
		public CodeDescriptionPairList AdditionalAddressInfoList { get; }

		public ZString AddressCode
		{
			get { return ViewingRights.ViewHomeAddressAllowedForStaff ? addressCode : ViewDeniedMessage; }
			set { addressCode = value; }
		}

		ZString addressCode;

		PhoneNumberPropertyHelper phoneNumberPropertyHelper;

		#endregion

		#endregion

		#region Staff Email Address Collection

		[ChildEditable]
		public GlbStaffEmailAddressCollection EmailAddresses
		{
			get
			{
				if (emailAddresses == null)
				{
					var query = new ZQuery();

					if (!(IsCurrentUser || Env.CurrentUser.IsController || Env.Security.StaffViewOtherEmailAddresses.IsAllowed))
					{
						query.AddToFilter(GlbStaffEmailAddressSchema.GSE_IsVisible, 1);
					}

					emailAddresses = new GlbStaffEmailAddressCollection(this, query);

					var mainEmailAddress = Factory.New<GlbStaffEmailAddress>();

					using (mainEmailAddress.SuspendSettingHasChanges())
					{
						emailAddresses.Add(mainEmailAddress);

						mainEmailAddress.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
						mainEmailAddress.GSE_EmailAddress = GS_EmailAddress;
					}

					RegisterEditableChildObject(emailAddresses);

					return emailAddresses;
				}

				UpdateMainEmailAddress(); // Main Email Address is not saved by factory; therefore, it needs to be updated manually.

				return emailAddresses;
			}
		}

		GlbStaffEmailAddressCollection emailAddresses;

		void UpdateMainEmailAddress()
		{
			var mainEmailAddress = emailAddresses.FindByEmailAddressType(Core.Constants.EmailFromAddressTypes.Codes.Main);

			if (mainEmailAddress != null)
			{
				using (mainEmailAddress.SuspendSettingHasChanges())
				{
					mainEmailAddress.GSE_EmailAddress = GS_EmailAddress;
				}
			}
		}

		#endregion

		#region IStmALogOperationProvider Members

		[SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
		public ZString GetHandledStringProperty(ZString property)
		{
			if (property.IsEmpty)
			{
				return property;
			}

			if (!ViewingRights.ViewEmergencyContactAllowedForStaff && (property.StartsWith((NoResString)"NextOfKin Work Phone:") || property.StartsWith((NoResString)"NextOfKin Home Phone:") || property.StartsWith((NoResString)"Emergency Work Phone:") || property.StartsWith((NoResString)"Emergency Home Phone:"))
				|| (ViewOtherReferencesNotAllowedForStaff && property.StartsWith((NoResString)"Pager:")) || (ViewHomeAddressNotAllowedForStaff && property.StartsWith((NoResString)"Home Phone:")))
			{
				property = Regex.Replace(property, @"\d((\s|\-)?)\d{3} (?=\=\>)", m => "X" + m.Groups[1].Value + "XXX ");
				property = Regex.Replace(property, @"\d{4}(?=$)", "XXXX");
			}

			return property;
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

		string IExamUrlRecipient.Language
		{
			get { return GS_WorkingLanguage; }
		}

		bool IExamUrlRecipient.HasNonPermittedDuplicateEmail
		{
			get
			{
				var duplicateEmailQuery = new ZQuery(GlbStaffSchema.GS_EmailAddress, GS_EmailAddress);
				duplicateEmailQuery.AddToFilter(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, PK);

				if (Factory.ExistsInDatabase(GlbStaffSchema.Constants.TableName, duplicateEmailQuery))
				{
					return true;
				}

				var applicant = Factory.LoadFromUniqueKey<IHRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, GS_EmailAddress);
				return applicant != null && (applicant.HA_PER != GS_PER || !applicant.IsLearningCenterUser);
			}
		}

		#region IGlbCompanyCampaignItemRecipient Members

		OrgHeader IGlbCompanyCampaignItemRecipient.Organisation
		{
			get { return null; }
		}

		string IGlbCompanyCampaignItemRecipient.Phone
		{
			get { return GS_MobilePhone; }
		}

		string IGlbCompanyCampaignItemRecipient.Fax
		{
			get { return GS_FaxNum; }
		}

		string IGlbCompanyCampaignItemRecipient.Salutation
		{
			get { return string.Empty; }
		}

		string IGlbCompanyCampaignItemRecipient.Title
		{
			get { return GS_Title; }
		}

		string IGlbCompanyCampaignItemRecipient.RelatedDocName
		{
			get { return $"{ResString.GetMultilingualString("A2C0C143-F0BC-4E76-BFAF-8EE08DB5CC6A", "Staff")} {GS_FullName}"; }
		}

		#endregion

		#endregion

		#region IGlbPersonPrimarySource Members

		ZGuid IGlbPersonPrimarySource.PersonPK
		{
			get { return GS_PER; }
		}

		ZString IGlbPersonPrimarySource.CompanyName => WorkingBranch?.Company?.GC_Name ?? ZString.Empty;

		Guid IGlbPersonPrimarySource.CompanyPKForLogin => (WorkingBranch?.Company?.PK ?? ZGuid.Empty).ToGuid();

		ZString IGlbPersonPrimarySource.Code => GS_Code;

		ZString IGlbPersonPrimarySource.City => WorkingBranch?.City ?? ZString.Empty;

		ZString IGlbPersonPrimarySource.State => WorkingBranch?.GB_State ?? ZString.Empty;

		ZString IGlbPersonPrimarySource.Country => WorkingBranch?.Country?.RN_Desc ?? ZString.Empty;

		ZString IGlbPersonPrimarySource.UNLOCO => WorkingBranch?.GB_RL_NKHomePort ?? ZString.Empty;

		ZString IGlbPersonPrimarySource.JobTitle => GS_Title;

		string IGlbPersonPrimarySource.TableCode
		{
			get { return GlbStaffSchema.Constants.Prefix; }
		}

		ZString IGlbPersonPrimarySource.Email => GS_EmailAddress;

		ZString IGlbPersonPrimarySource.WorkNumber => GS_WorkPhone;

		ZString IGlbPersonPrimarySource.WorkExtension => GS_WorkExtension;

		ZString IGlbPersonPrimarySource.MobileNumber => GS_MobilePhone;

		ZString IGlbPersonPrimarySource.JobCategory => ZString.Empty;

		ZString IGlbPersonPrimarySource.Name => GS_FullName;

		GlbBranch WorkingBranch => HomeBranch ?? LastLogonBranch;

		#endregion

		#region IPasswordStored Member

		int IPasswordStored.PasswordHashIterations => GS_PasswordHashIterations;

		ZBlob IPasswordStored.PasswordSalt => GS_PasswordSalt;

		ZBlob IPasswordStored.PasswordHash => GS_PasswordHash;

		public bool VerifyPassword(IUserSecretsContext userSecretsContext, string password)
		{
			if (password != null && !LocalPasswordMustBeReset)
			{
				// To ensure verify against the latest password
				ReloadSafe();

				try
				{
					if (!GS_PasswordHash.IsEmpty)
					{
						return userSecretsContext.IsMatchingSecret(password, GetPasswordAdapter());
					}
				}
				catch (Exception ex) when (ex is ArgumentException)
				{
					// Report any corruption
					ErrorReporter.ReportOnce(FormattableString.Invariant($@"The staff password is corrupted, please investigate why this happen
Staff: {GS_LoginName}
GS_PK: {PK}
GS_PasswordHash: {Convert.ToBase64String(GS_PasswordHash)}
GS_PasswordSalt: {Convert.ToBase64String(GS_PasswordSalt)}
GS_PasswordHashIterations: {GS_PasswordHashIterations}
"), ex);
				}
				return false;
			}
			return false;
		}

		#endregion

		#region IGlbPasswordHistoryParent Member

		bool IGlbPasswordHistoryParent.ShouldSavePasswordHistory => !(GS_IsSystemAccount || IsADIntegrationEnabled);

		BusinessObject IGlbPasswordHistoryParent.BusinessEntity => this;

		int IGlbPasswordHistoryParent.PasswordHistoryCount => DataRegistry.Instance.PasswordHistoryCount;

		#endregion

		#region Number Fountains

		IViewStmNumsCollection<ViewStmNums> IViewStmNumsOwner.Fountains => Fountains;

		[ChildEditable()]
		public StaffViewStmNumsCollection Fountains
		{
			get
			{
				if (staffFountains == null)
				{
					staffFountains = new StaffViewStmNumsCollection(this);
					staffFountains.AdditionalFilter.AddToFilter(ViewStmNumsSchema.SN_ID, SQLComparisonOperator.NotEqual, 0);
					staffFountains.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(staffFountains);
				}

				return staffFountains;
			}
		}
		StaffViewStmNumsCollection staffFountains;

		#endregion

		#region Matching Number

		[ChildEditable()]
		public StmNumberRangeMatchingDetailsCollection NumberRangeMatchingDetails
		{
			get
			{
				if (stmNumberRangeMatchingDetailsCollection == null)
				{
					stmNumberRangeMatchingDetailsCollection = new StmNumberRangeMatchingDetailsCollection(this);
					stmNumberRangeMatchingDetailsCollection.SetReadOnlyIncludingChildren(!Env.Security.StaffModifyAll.IsAllowed);
					RegisterEditableChildObject(stmNumberRangeMatchingDetailsCollection);
				}

				return stmNumberRangeMatchingDetailsCollection;
			}
		}
		StmNumberRangeMatchingDetailsCollection stmNumberRangeMatchingDetailsCollection;

		#endregion

		#region EDI Clients
		public bool HasActiveEDIClient => Factory.LoadTop1<EDICommunicationParty>(new ZQuery(EDICommunicationPartySchema.ECP_GS_SecurityProxy, PK).AddToFilter(EDICommunicationPartySchema.ECP_IsActive, true)) != null;
		#endregion

		public void SetFromPerson(GlbPerson person)
		{
			GS_PER = person.PK;
			GS_MobilePhone = person.PER_MobilePhoneInternal;
			UpdateFromPerson(person);
		}

		public virtual void UpdateFromPerson(GlbPerson person)
		{
			if (!person.PER_FullNameInternal.Equals(GlbPerson.EmptyFullName))
			{
				GS_FullName = person.PER_FullNameInternal;
			}

			GS_NameSuffix = person.PER_NameSuffix;
			GS_NameTitle = person.PER_NameTitle.SubstringSafe(0, GlbStaffSchema.GS_NameTitle.MaxLength);
			GS_Gender = person.PER_GenderInternal;
			GS_UserAddress1 = person.PER_HomeAddress1Internal;
			GS_UserAddress2 = person.PER_HomeAddress2Internal;
			GS_City = person.PER_CityInternal;
			GS_Postcode = person.PER_PostcodeInternal;
			GS_State = person.PER_StateInternal;
			GS_RN_NKCountryCode = person.PER_RN_NKCountryInternal;
			GS_HomePhone = person.PER_HomePhoneInternal;
			GS_FaxNum = person.PER_FaxNumberInternal;
			GS_Birthdate = person.PER_BirthDateInternal;
			GS_FriendlyName = person.PER_FriendlyName;
			GS_RN_NKNationalityCode = person.PER_RN_NKNationalityCodeISOInternal;
			GS_ProfilePhoto = person.PER_PictureInternal;
			GS_Passport = person.PER_PassportInternal;

			if (person.ShouldUpdateMobileOnRelatedRecords && GS_MobilePhone == person.PER_MobilePhoneInfo.OriginalValue.ToString())
			{
				GS_MobilePhone = person.PER_MobilePhoneInternal;
			}
		}

		internal ZString GS_FullNameInternal => base.GS_FullName;
		internal ZString GS_GenderInternal => base.GS_Gender;
		internal ZString GS_UserAddress1Internal => base.GS_UserAddress1;
		internal ZString GS_UserAddress2Internal => base.GS_UserAddress2;
		internal ZString GS_CityInternal => base.GS_City;
		internal ZString GS_StateInternal => base.GS_State;
		internal ZString GS_PostcodeInternal => base.GS_Postcode;
		internal ZString GS_RN_NKCountryCodeInternal => base.GS_RN_NKCountryCode;
		internal ZString GS_MobilePhoneInternal => base.GS_MobilePhone;
		internal ZString GS_HomePhoneInternal => base.GS_HomePhone;
		internal ZString GS_FaxNumInternal => base.GS_FaxNum;
		internal ZDate GS_BirthdateInternal => base.GS_Birthdate;
		internal ZString GS_RN_NKNationalityCodeInternal => base.GS_RN_NKNationalityCode;
		internal ZBlob GS_ProfilePhotoInternal => base.GS_ProfilePhoto;
		internal ZString GS_EmailAddressInternal => base.GS_EmailAddress;
		internal ZString GS_PassportInternal => base.GS_Passport;

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GlbStaffProcessTasksCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZGuid IWorkflowProviderCore.PK
		{
			get { return PK; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new GlbStaffWorkflowDescriptor().Code; }
		}

		#endregion

		#region DomainName

		[List("Lookups.DomainNames")]
		[ReadOnlyMember(nameof(GS_DomainNameReadOnly))]
		public override ZString GS_DomainName
		{
			get { return base.GS_DomainName; }
			set { base.GS_DomainName = value; }
		}

		protected bool GS_DomainNameReadOnly => NoRightsExist;

		[List("Lookups.DomainNames")]
		[ReadOnlyMember(nameof(GS_DomainNameReadOnly))]
		public ZString DomainName
		{
			get { return GS_DomainName; }
			set { GS_DomainName = value; }
		}

		public ZPropertyInfo DomainNameInfo
		{
			get
			{
				return GS_DomainNameInfo;
			}
		}

		#endregion

		#region Enterprise Db Username

		public string SQLUserName
		{
			get
			{
				var dbUserManager = new DbUserManager();
				var enterpriseDbUsername = dbUserManager.GetFullUserLoginName(Db.DatabaseName, GS_LoginName, GetDownLevelLogonName,
																					dbUserManager.DbSupportsSqlAuthentication() ?
																						DatabaseAuthenticationMode.Sql :
																						DatabaseAuthenticationMode.Windows);

				var sql = $"SELECT count(name) FROM sys.database_principals WHERE name = @enterpriseDbUsername";

				var result = Db.Connection.ExecuteScalar(sql, cmd => cmd.AddParameter("@enterpriseDbUsername", SqlDbType.NVarChar, 128, enterpriseDbUsername));

				if (((int)result) == 1)
				{
					return enterpriseDbUsername;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(Factory.Load<GlbExternalPassword>(new ZQuery(GlbExternalPasswordSchema.GP_GS, PK)));
				return result.ToArray();
			}
		}

		#region Database Access Wrapper Properties

		#region IsDatabaseDeveloper

		public ZBool IsDatabaseDeveloperChanged => DatabaseAccessFlagChanged(GlbGroup.DbDeveloperGroupPK);

		public ZBool IsDatabaseDeveloper
		{
			get => HasDatabaseAccessFlag(GlbGroup.DbDeveloperGroupPK);
			set => SetDatabaseAccessFlag(value, GlbGroup.DbDeveloperGroupPK, IsDatabaseDeveloperInfo);
		}

		public ZPropertyInfo IsDatabaseDeveloperInfo => GetZPropertyInfo(nameof(IsDatabaseDeveloper));

		protected bool IsDatabaseDeveloper_ReadOnly
		{
			get
			{
				return IsControlledByScim && IsPropertyMappedForScim(nameof(IsDatabaseDeveloper));
			}
		}

		#endregion

		#region IsReadOnlyDBUser
		public ZBool IsReadOnlyDBUserChanged => DatabaseAccessFlagChanged(GlbGroup.DbReaderGroupPK);

		public ZBool IsReadOnlyDBUser
		{
			get => HasDatabaseAccessFlag(GlbGroup.DbReaderGroupPK);
			set => SetDatabaseAccessFlag(value, GlbGroup.DbReaderGroupPK, IsReadOnlyDBUserInfo);
		}

		public ZPropertyInfo IsReadOnlyDBUserInfo => GetZPropertyInfo(nameof(IsReadOnlyDBUser));

		protected bool IsReadOnlyDBUser_ReadOnly
		{
			get
			{
				return IsControlledByScim && IsPropertyMappedForScim(nameof(IsReadOnlyDBUser));
			}
		}

		#endregion

		#region IsBackupOperator

		public ZBool IsBackupOperatorChanged => DatabaseAccessFlagChanged(GlbGroup.BackupOperatorGroupPK);

		public ZBool IsBackupOperator
		{
			get => HasDatabaseAccessFlag(GlbGroup.BackupOperatorGroupPK);
			set => SetDatabaseAccessFlag(value, GlbGroup.BackupOperatorGroupPK, IsBackupOperatorInfo);
		}

		public ZPropertyInfo IsBackupOperatorInfo => GetZPropertyInfo(nameof(IsBackupOperator));

		protected bool IsBackupOperator_ReadOnly
		{
			get
			{
				return IsControlledByScim && IsPropertyMappedForScim(nameof(IsBackupOperator));
			}
		}

		#endregion

		ZBool HasDatabaseAccessFlag(Guid glbGroupLinkPK) => DatabaseAccessGroupPKs.Contains(glbGroupLinkPK);

		ZBool DatabaseAccessFlagChanged(Guid glbGroupLinkPK) => (databaseAccessGroupPKs != null && databaseAccessGroupPKsOriginalValues != null) && (DatabaseAccessGroupPKs.Contains(glbGroupLinkPK) != DatabaseAccessGroupPKsOriginalValues.Contains(glbGroupLinkPK));

		void SetDatabaseAccessFlag(ZBool value, Guid glbGroupPK, ZPropertyInfo propertyInfo)
		{
			if (value && !DatabaseAccessGroupPKs.Contains(glbGroupPK))
			{
				var glbGroupLink = Factory.New<GlbGroupLink>();
				glbGroupLink.GK_GG = glbGroupPK;
				glbGroupLink.GK_GS = PK;

				RegisterEditableChildObject(glbGroupLink);

				DatabaseAccessGroupPKs.Add(glbGroupPK);
			}
			else if (!value && DatabaseAccessGroupPKs.Contains(glbGroupPK))
			{
				var glbGroupLinkQuery = new ZQuery(GlbGroupLinkSchema.GK_GS, PK);
				glbGroupLinkQuery.AddToFilter(GlbGroupLinkSchema.GK_GG, SQLComparisonOperator.Equal, glbGroupPK);

				var glbGroupLink = Factory.LoadTop1<GlbGroupLink>(glbGroupLinkQuery);
				if (glbGroupLink != null)
				{
					RegisterEditableChildObject(glbGroupLink);
					glbGroupLink.Delete();
				}

				DatabaseAccessGroupPKs.Remove(glbGroupPK);
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateDatabaseAccessGroupFlag(propertyInfo);
			}

			propertyInfo.RefreshBinding();

			StaffScimHelper.SetScimGroupIfRequired(value, propertyInfo);
		}

		public ISet<string> DatabaseAccessGroupRoles => GetGlbGroupDatabaseAccessRoles(DatabaseAccessGroupPKs);

		public ISet<string> DatabaseAccessGroupRolesOriginalValues => GetGlbGroupDatabaseAccessRoles(DatabaseAccessGroupPKsOriginalValues);

		ISet<string> GetGlbGroupDatabaseAccessRoles(IEnumerable<Guid> groupPKs)
		{
			var databaseAccessRoles = DatabaseAccessGroupRoleTypes.AllDbRolesWithDescriptions.Keys.ToArray();
			var glbGroupRoles = Factory.Load<GlbGroupRole>(new ZQuery(GlbGroupRoleSchema.GGR_GG_Group, SQLComparisonOperator.Equal, groupPKs));

			return databaseAccessRoles.Intersect(glbGroupRoles.Select(r => r.GGR_RoleName.ToString()).Distinct(), StringComparer.OrdinalIgnoreCase).ToHashSet();
		}

		#region Database Access Group Pks

		public void InitDatabaseAccessGroupPks()
		{
			databaseAccessGroupPKsOriginalValues = Array.Empty<Guid>();
			databaseAccessGroupPKs = new List<Guid>();
		}

		List<Guid> DatabaseAccessGroupPKs
		{
			get
			{
				if (databaseAccessGroupPKs == null)
				{
					databaseAccessGroupPKsOriginalValues = LoadGlbGroupLinkPKs();
					databaseAccessGroupPKs = databaseAccessGroupPKsOriginalValues.ToList();
				}

				return databaseAccessGroupPKs;
			}
		}
		List<Guid> databaseAccessGroupPKs;

		IEnumerable<Guid> DatabaseAccessGroupPKsOriginalValues => databaseAccessGroupPKsOriginalValues ?? (databaseAccessGroupPKsOriginalValues = LoadGlbGroupLinkPKs());
		IEnumerable<Guid> databaseAccessGroupPKsOriginalValues;

		IEnumerable<Guid> LoadGlbGroupLinkPKs() => LoadGlbGroupLinks().Select(glbGroupLink => glbGroupLink.GK_GG.ToGuid()).ToArray();

		GlbGroupLink[] LoadGlbGroupLinks() => Factory.Load<GlbGroupLink>(GetGlbGroupLinkDatabaseAccessQuery());

		ZQuery GetGlbGroupLinkDatabaseAccessQuery()
		{
			var databaseAccessRoles = DatabaseAccessGroupRoleTypes.AllDbRolesWithDescriptions.Keys.ToArray();

			var glbGroupRoleSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupRole), GlbGroupLinkSchema.GK_GG);
			glbGroupRoleSubQuery.AddToFilter(GlbGroupRoleSchema.GGR_RoleName, SQLComparisonOperator.Equal, databaseAccessRoles);

			var query = new ZDBOnlyQuery(typeof(GlbGroupLink)) { ReLoadExistingRows = true };
			query.AddToFilter(GlbGroupLinkSchema.GK_GS, PK);
			query.AddSubQuery(GlbGroupLinkSchema.GK_GG, GlbGroupRoleSchema.GGR_GG_Group, glbGroupRoleSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#endregion

		[DocumentMacroIgnore]
		public override ZBlob GS_SqlLoginPasswordHash
		{
			get
			{
				return base.GS_SqlLoginPasswordHash;
			}
			set
			{
				var valueLength = value.Length;
				if (valueLength > Schema.GS_SqlLoginPasswordHashMaxLength)
				{
					throw new MaxLengthExceededException(
						Res.GetString(
							"B00ED476-EA07-4F12-BB07-B24B41A23ED9",
							"Max length of SQL login password hash is {0}, current is {1}.",
							Schema.GS_SqlLoginPasswordHashMaxLength,
							valueLength),
						Schema.GS_SqlLoginPasswordHashMaxLength);
				}

				_ = DatabaseAccessGroupPKs;

				base.GS_SqlLoginPasswordHash = value;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public virtual bool UserIsLoggedIn()
		{
			using (var cmd = Db.Connection.Command($"SELECT COUNT(*) FROM dbo.StmServiceHeartBeat WHERE SV_ParentTableCode = '{GlbStaffSchema.Constants.Prefix}' AND SV_ParentId = @StaffPK"))
			{
				cmd.AddParameter("@StaffPK", SqlDbType.UniqueIdentifier, PK.ToGuid());
				return (int)cmd.ExecuteScalar() > 0;
			}
		}
	}

	#region Fake Staff - Group Form

	[TestedAsNonPersistentBusinessObject]
	public class FakeStaffForGroupForm : GlbStaff
	{
		public FakeStaffForGroupForm(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void AddUserToAllUsersGroup(BusinessObjectCollection groups)
		{
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}
	}

	#endregion
}
