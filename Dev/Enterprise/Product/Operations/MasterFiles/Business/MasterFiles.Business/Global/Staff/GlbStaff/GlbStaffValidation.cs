using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.ServiceManager;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffValidation : AutoGlbStaffValidation
	{
		public GlbStaffValidation(AutoGlbStaff parent)
			: base(parent)
		{
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
		}

		public new GlbStaff Parent
		{
			get { return (GlbStaff)base.Parent; }
		}

		#region GS_Code

		protected override void CheckGS_Code()
		{
			base.CheckGS_Code();
			if (Parent.IsInDatabase)
			{
				MandatoryValidation.CheckEntered(Parent.GS_CodeInfo);
			}
			else if (Parent.GS_Code.IsEmpty && Parent.GenerateCode().IsEmpty)
			{
				Parent.GS_CodeInfo.AddError(Res.GetString("2d80afd8-ce6a-4278-83c3-436e46b64e74", "Staff initials could not be calculated. Either insufficient information was entered in the full name or preferred name field, or all codes based on the staff name have been used. To save, please enter initials for this staff."));
			}

			if (!Parent.GS_Code.IsEmpty)
			{
				if (!IsUniqueOnStaff(GlbStaffSchema.GS_Code, Parent.GS_Code))
				{
					Parent.GS_CodeInfo.AddError(Res.GetString("8c50045e-d532-4870-bc9a-c202bccf246d", "{0} must be unique in the system", CodeDescription));
				}
			}

			if (Parent.GS_CodeInfo.HasChanges && Parent.PK == Env.CurrentUser.PK)
			{
				Parent.GS_CodeInfo.AddWarning(Res.GetString("903cbf18-05bf-4552-9a5e-e66410db663d", "By changing your own code, some operations may not function properly until you restart {0}.", BrandingFactory.Instance.ProductName));
			}
		}

		protected virtual string CodeDescription
		{
			get { return (NoResString)"Code"; }
		}

		protected bool IsUniqueOnStaff(SchemaColumn column, object fieldValue)
		{
			ZQuery query = new ZQuery(column, fieldValue);
			query.AddToFilter(JoinCondition.And, GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return !Parent.Factory.Exists(typeof(GlbStaff), query);
		}

		#endregion

		#region GS_LoginName

		protected override void CheckGS_LoginName()
		{
			base.CheckGS_LoginName();

			if (Parent.GS_LoginNameInfo.HasChanges && Parent.PK == Env.CurrentUser.PK)
			{
				Parent.GS_LoginNameInfo.AddWarning(Res.GetString("a9407a73-84b7-4b4f-86e8-287155f9d770", "By changing your own login name, some operations may not function properly until you restart {0}.", BrandingFactory.Instance.ProductName));
			}

			var message = ValidateLoginName(Parent.GS_LoginName);
			if (!string.IsNullOrEmpty(message))
			{
				Parent.GS_LoginNameInfo.AddError(message);
			}
		}

		public static string ValidateLoginName(string loginName)
		{
			if (loginName.Any(s => char.IsControl(s)))
			{
				return Res.GetString("529078FC-D93E-41D5-8F90-30A8EFF4F8AF", "Login Name can not contain control characters, such as line break or tab.");
			}

			if (((ZString)loginName).ContainsAnyChar(GlbStaffValidation.InvalidLoginNameCharacters))
			{
				return Res.GetString("9a7af48d-940c-4541-9362-4b9dae40c47c", "Login Name can not contain any of these symbols: {0}", InvalidLoginNameCharactersUserReadable());
			}

			return null;
		}

		#endregion

		#region GS_FullName

		protected override void CheckGS_FullName()
		{
			base.CheckGS_FullName();
			MandatoryValidation.CheckEntered(Parent.GS_FullNameInfo);

			if (IsStartsWithWhiteSpace(Parent.GS_FullName))
			{
				Parent.GS_FullNameInfo.AddError(Res.GetString("e704977c-570c-4ade-a4d0-f925bbb8ab81", "{0} starts with white space.", Parent.GS_FullNameInfo.HumanReadableName));
			}
		}

		#endregion

		#region GS_IsRobot

		protected override void CheckGS_IsRobot()
		{
			base.CheckGS_IsRobot();
			if (!(ZBool)Parent.GS_IsOperationalInfo.Value && (ZBool)Parent.GS_IsRobotInfo.Value)
			{
				Parent.GS_IsRobotInfo.AddError(Res.GetString("RobotCheckboxValidation|NotAvailableForNonOperationalUser", "A non operational user cannot be a robot."));
			}
			if (((ZBool)Parent.GS_IsRobotInfo.Value) && EnvProxy.IsHostedWithCargowise && !DataRegistry.Instance.EnableRPAOnWiseCloud)
			{
				Parent.GS_IsRobotInfo.AddError(Res.GetString("RobotCheckboxValidation|Re-EnableForHosted", "This checkbox is used to indicate that this user is used for Robotic Process Automation. To use this feature, RPA needs to be enabled for your environment."));
			}
		}

		#endregion

		#region Staff Password

		public void ValidateStaffPlainTextPassword()
		{
			ValidateCalculatedProperty(Parent.StaffPlainTextPasswordInfo);
		}

		protected virtual void CheckStaffPlainTextPassword()
		{
		}

		#endregion

		#region Staff Confirm Password

		public void ValidateStaffConfirmPassword()
		{
			ValidateCalculatedProperty(Parent.StaffConfirmPasswordInfo);
		}

		protected virtual void CheckStaffConfirmPassword()
		{
		}

		#endregion

		#region Password Policy

		public void ValidateChangePasswordAtNextLogin()
		{
			ValidateCalculatedProperty(Parent.ChangePasswordAtNextLoginInfo);
		}

		public void ValidatePasswordNeverChanges()
		{
			ValidateCalculatedProperty(Parent.PasswordNeverChangesInfo);
		}

		protected virtual void CheckChangePasswordAtNextLogin()
		{
		}

		protected virtual void CheckPasswordNeverChanges()
		{
		}

		#endregion

		#region Security Branch

		public void ValidateSecurityBranch()
		{
			ValidateCalculatedProperty(Parent.SecurityBranchInfo);
		}

		protected virtual void CheckSecurityBranch()
		{
		}

		#endregion

		#region Security Department

		public void ValidateSecurityDepartment()
		{
			ValidateCalculatedProperty(Parent.SecurityDepartmentInfo);
		}

		protected virtual void CheckSecurityDepartment()
		{
		}

		#endregion

		#region Sales Team

		public void ValidateSalesTeams()
		{
			if (Parent.IsTopLevel)
			{
				Parent.RemoveRowError(OverlappingTeamCoverageErrorMessage);
				foreach (var salesTeam in Parent.SalesTeams)
				{
					salesTeam.ClearRowNotifications();
				}

				if (Parent.SalesTeams.Count > 1)
				{
					var salesTeamsByCompanyAndCountry = new Dictionary<Tuple<ZGuid, ZString>, HashSet<SalesTeam>>();
					var salesTeamsByCompanyAndUnloco = new Dictionary<Tuple<ZGuid, ZString>, HashSet<SalesTeam>>();
					var salesTeamsByCompanyAndUnlocoCountry = new Dictionary<Tuple<ZGuid, ZString>, HashSet<SalesTeam>>();

					foreach (var salesTeam in Parent.SalesTeams)
					{
						foreach (var country in salesTeam.CoveredCountries)
						{
							AddToSetValueOfDictionary(salesTeamsByCompanyAndCountry, salesTeam.GG_GC, country.RN_Code, salesTeam);
						}

						foreach (var unloco in salesTeam.CoveredUnlocos)
						{
							AddToSetValueOfDictionary(salesTeamsByCompanyAndUnloco, salesTeam.GG_GC, unloco.RL_Code, salesTeam);
							AddToSetValueOfDictionary(salesTeamsByCompanyAndUnlocoCountry, salesTeam.GG_GC, unloco.RL_RN_NKCountryCode, salesTeam);
						}
					}

					foreach (var salesTeamsWithOverlappingCountryCoverage in salesTeamsByCompanyAndCountry.Values.Where(x => x.Count > 1))
					{
						AddOverlappingCoverageRowErrors(salesTeamsWithOverlappingCountryCoverage);
					}

					foreach (var salesTeamsWithOverlappingUnlocoCoverage in salesTeamsByCompanyAndUnloco.Values.Where(x => x.Count > 1))
					{
						AddOverlappingCoverageRowErrors(salesTeamsWithOverlappingUnlocoCoverage);
					}

					foreach (var unlocoCountryCodeAndSalesTeamsPair in salesTeamsByCompanyAndUnlocoCountry)
					{
						HashSet<SalesTeam> salesTeamsWithSameCoverageCountryCode;
						if (salesTeamsByCompanyAndCountry.TryGetValue(unlocoCountryCodeAndSalesTeamsPair.Key, out salesTeamsWithSameCoverageCountryCode))
						{
							AddOverlappingCoverageRowErrors(unlocoCountryCodeAndSalesTeamsPair.Value.Union(salesTeamsWithSameCoverageCountryCode));
						}
					}
				}
			}
		}

		static void AddToSetValueOfDictionary(Dictionary<Tuple<ZGuid, ZString>, HashSet<SalesTeam>> dictionary, ZGuid keyCompany, ZString keyCode, SalesTeam item)
		{
			HashSet<SalesTeam> set;
			Tuple<ZGuid, ZString> key = Tuple.Create(keyCompany, keyCode);
			if (!dictionary.TryGetValue(key, out set))
			{
				set = new HashSet<SalesTeam>();
				dictionary.Add(key, set);
			}
			set.Add(item);
		}

		void AddOverlappingCoverageRowErrors(IEnumerable<SalesTeam> overlappingTeams)
		{
			foreach (var team in overlappingTeams)
			{
				foreach (var otherTeam in overlappingTeams.Where(x => x != team))
				{
					team.AddRowError(Res.GetString("18fcd433-38be-422d-91cc-f10ed6a4a46a", "Has overlapping coverage area with {0}.", otherTeam.HumanReadableName));
					Parent.AddRowError(OverlappingTeamCoverageErrorMessage);
				}
			}
		}

		string OverlappingTeamCoverageErrorMessage
		{
			get { return Res.GetString("538c50ab-8e8c-49c7-983b-f233190be1e6", "Sales teams have overlapping coverage area."); }
		}

		#endregion

		#region Managers

		public void ValidateManagers()
		{
			if (Env.Security.StaffReportingManagerRolesAdd.IsAllowed && Parent.IsInDatabase && Parent.GS_IsActive)
			{
				var reportingRoles = SystemDataRegistry.Instance.StaffReportingRoles.Value.ToArray<StaffReportingRole>();
				var missingRoles = Parent.GetMissingMandatoryReportingRoles();
				foreach (var role in reportingRoles)
				{
					var missingRoleErrorString = MissingMandatoryManagementRoleMessage(role.Description);
					Parent.RemoveRowError(missingRoleErrorString);

					if (missingRoles.Contains(role))
					{
						Parent.AddRowError(missingRoleErrorString);
					}
				}
			}
		}

		string MissingMandatoryManagementRoleMessage(string roleDescription)
		{
			return Res.GetString("6564293e-5a21-4066-8faf-f99948de9ddf", "The {0} role is mandatory. It must be added for this staff member.", roleDescription);
		}

		#endregion

		#region GS_CommissionBasis

		protected override void CheckGS_CommissionBasis()
		{
			base.CheckGS_CommissionBasis();
			ListValidation.ErrorIfInvalidCode(Parent.GS_CommissionBasisInfo, new CommissionBasisType());
		}

		#endregion

		#region GS_EmailAddress

		protected override void CheckGS_EmailAddress()
		{
			base.CheckGS_EmailAddress();
			ValidateEmailAddress(Parent.GS_EmailAddressInfo, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry item key")]
		public void ValidateEmailAddress(ZPropertyInfo emailAddressPropertyInfo, bool isMainEmailAddress)
		{
			var emailAddressToValidate = (ZString)emailAddressPropertyInfo.Value;
			if (!EmailAddressValidation.IsEmailAddressValid(emailAddressToValidate) || (emailAddressToValidate.StartsWith("<", StringComparison.OrdinalIgnoreCase) && emailAddressToValidate.EndsWith(">", StringComparison.OrdinalIgnoreCase)))
			{
				emailAddressPropertyInfo.AddError(Res.GetString("f18fe013-558e-4f59-b4d6-2e0051bb9f2c", "{0} is not a valid email address.", emailAddressToValidate));
			}

			if (isMainEmailAddress)
			{
				// Only main email address (i.e. GS_EmailAddress) is used for 2FA and Schedule Task Recipient
				var twoFactorAuthenticationType = SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				if (twoFactorAuthenticationType == "Email" && Parent.GS_IsTwoFactorAuthenticationEnabled && string.IsNullOrEmpty(emailAddressToValidate))
				{
					emailAddressPropertyInfo.AddError(Res.GetString("83AFC8A8-5ECF-47F1-BFA2-ACAE5A329F7B", "An email address is required to enable two factor authentication."));
				}

				if (Parent.IsInDatabase && string.IsNullOrEmpty(emailAddressToValidate))
				{
					if (GlbStaffIsTheScheduleTaskRecipient(Parent.GS_Code))
					{
						emailAddressPropertyInfo.AddError(Res.GetString("7BDC69C2-2549-4F54-82E3-9CE189AE43D6", "Email cannot set to empty because staff is assigned to recipient of scheduled reports."));
					}
					if (GlbStaffIsTheOnlyOneGroupScheduleTaskRecipient(Parent.PK, Parent.Groups))
					{
						emailAddressPropertyInfo.AddError(Res.GetString("4DE634F9-D6FF-4F3D-A869-641708483CA5", "Email cannot set to empty because staff is assigned to group recipient of scheduled reports."));
					}
				}

				// In EDI, we need to stop changing main email address while staff has DB access and linked, as IS will need to update LoginName according to EmailAddress
				if (
						ClientHookLoader.Instance.Client == Clients.EDI &&
						Parent.IsADIntegrationEnabled &&
						Parent.GS_EmailAddressInfo.HasChanges &&
						Parent.IsADLinked &&
						Parent.DatabaseAccessGroupRoles.Count > 0
					)
				{
					var emailParts = ((ZString)Parent.GS_EmailAddressInfo.Value).Split("@");
					if (emailParts[0] != Parent.GS_LoginName)
					{
						emailAddressPropertyInfo.AddError(Res.GetString("9EC69B4E-C426-43B8-84DA-CDF240247979",
							@$"The {Parent.GS_EmailAddressInfo.HumanReadableName}'s local-part (username part) cannot be changed and it must match the {Parent.GS_LoginNameInfo.HumanReadableName} if the staff has database access.
If you need to update it, please disable database access and try again."));
					}
				}
			}

			bool GlbStaffIsTheScheduleTaskRecipient(string gs_Code)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryMethod, Constants.ContactNotifyModes.Email);
				query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryToType, ScheduledReportDeliveryRecipientConstants.RecipientType.Staff);
				query.AddToFilter(StmScheduleTaskRecipientSchema.S6_GS_NKRecipient, gs_Code);

				var taskRecipient = Parent.Factory.Load(ObjectFactory.GetType<IStmScheduleTaskRecipient>(), query);

				return taskRecipient.Any();
			}

			bool GlbStaffIsTheOnlyOneGroupScheduleTaskRecipient(ZGuid pk, GroupCollectionView groups)
			{
				foreach (GlbGroup group in groups)
				{
					var query = new ZQuery();
					query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryMethod, Constants.ContactNotifyModes.Email);
					query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryToType, ScheduledReportDeliveryRecipientConstants.RecipientType.Group);
					query.AddToFilter(StmScheduleTaskRecipientSchema.S6_GG, group.PK);
					if (Parent.Factory.Load(ObjectFactory.GetType<IStmScheduleTaskRecipient>(), query).Any())
					{
						if (!group.Staff.Cast<GlbStaff>().Any(s => !s.IsDeleted && !string.IsNullOrEmpty(s.GS_EmailAddress) && s.PK != pk))
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		#endregion

		#region Relationships

		protected override void CheckGS_EmergencyContactRelationship()
		{
			base.CheckGS_EmergencyContactRelationship();
			if (Parent.ViewingRights.ViewEmergencyContactAllowedForStaff)
			{
				ListValidation.ErrorIfInvalidCode(Parent.GS_EmergencyContactRelationshipInfo);
			}
		}

		protected override void CheckGS_NextOfKinRelationship()
		{
			base.CheckGS_NextOfKinRelationship();
			if (Parent.ViewingRights.ViewEmergencyContactAllowedForStaff)
			{
				ListValidation.ErrorIfInvalidCode(Parent.GS_NextOfKinRelationshipInfo);
			}
		}

		#endregion

		#region GS_WorkingLanguage

		protected override void CheckGS_WorkingLanguage()
		{
			MandatoryValidation.CheckEntered(Parent.GS_WorkingLanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GS_WorkingLanguageInfo);

			if (!Res.IsEnglish(Parent.GS_WorkingLanguage))
			{
				LanguageLicenceCheckpoint languageCheckpoint;
				Env.Licence.LanguagePackLookup.TryGetValue(Parent.GS_WorkingLanguage, out languageCheckpoint);
				var languageCodeDescription = Parent.Lookups.WorkingLanguages[Parent.GS_WorkingLanguage];
				if (languageCheckpoint != null)
				{
					Parent.GS_WorkingLanguageInfo.AddWarning(Res.GetString("60c14597-e350-45e5-9c92-4f3887db1a2e", "Selecting this language will mean the screens are shown in {0} for this user.", languageCodeDescription != null ? languageCodeDescription.Description : (NoResString)"[Unknown]"));
				}
			}
		}

		#endregion

		#region GS_ActiveDirectoryObjectGuidIsValidZGuid

		protected override void CheckGS_ActiveDirectoryObjectGuidIsValidZGuid()
		{
			// We use ZGuid.Invalid to indicate a new staff record needs to be created in AD
		}

		#endregion

		#region GS_GB_HomeBranch

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return info.Name != GlbStaffSchema.Constants.GS_GB_HomeBranch && base.ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region GS_GC_PreferredPaymentCompany

		protected override void CheckGS_GC_PreferredPaymentCompany()
		{
			base.CheckGS_GC_PreferredPaymentCompany();

			if (Parent.GS_IsSalesRep && !Parent.UseTransactionCompanyAsPreferredPayment)
			{
				MandatoryValidation.CheckEntered(Parent.GS_GC_PreferredPaymentCompanyInfo);
			}
		}

		#endregion

		#region Phone Numbers

		#region GS_HomePhone_Formatted

		public void ValidateGS_HomePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GS_HomePhone_FormattedInfo);
		}

		protected virtual void CheckGS_HomePhone_Formatted()
		{
			if (Parent.ViewingRights.ViewHomeAddressAllowedForStaff)
			{
				ValidatePhoneNumber(Parent.GS_HomePhone_FormattedInfo, Parent.GS_HomePhoneInfo, Parent.GS_HomePhone_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region GS_WorkPhone_Formatted

		public void ValidateGS_WorkPhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GS_WorkPhone_FormattedInfo);
		}

		protected virtual void CheckGS_WorkPhone_Formatted()
		{
			ValidatePhoneNumber(Parent.GS_WorkPhone_FormattedInfo, Parent.GS_WorkPhoneInfo, Parent.GS_WorkPhone_IsManuallyVerifiedInfo);
		}

		#endregion

		#region GS_MobilePhone_Formatted

		public void ValidateGS_MobilePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GS_MobilePhone_FormattedInfo);
		}

		protected virtual void CheckGS_MobilePhone_Formatted()
		{
			ValidatePhoneNumber(Parent.GS_MobilePhone_FormattedInfo, Parent.GS_MobilePhoneInfo, Parent.GS_MobilePhone_IsManuallyVerifiedInfo);
		}

		#endregion

		#region GS_FaxNum_Formatted

		public void ValidateGS_FaxNum_Formatted()
		{
			ValidateCalculatedProperty(Parent.GS_FaxNum_FormattedInfo);
		}

		protected virtual void CheckGS_FaxNum_Formatted()
		{
			ValidatePhoneNumber(Parent.GS_FaxNum_FormattedInfo, Parent.GS_FaxNumInfo, Parent.GS_FaxNum_IsManuallyVerifiedInfo);
		}

		#endregion

		#region GS_NextOfKinHomePhone_Formatted

		public void ValidateGS_NextOfKinHomePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GS_NextOfKinHomePhone_FormattedInfo);
		}

		protected virtual void CheckGS_NextOfKinHomePhone_Formatted()
		{
			if (Parent.ViewingRights.ViewEmergencyContactAllowedForStaff)
			{
				ValidatePhoneNumber(Parent.GS_NextOfKinHomePhone_FormattedInfo, Parent.GS_NextOfKinHomePhoneInfo, Parent.GS_NextOfKinHomePhone_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region GS_NextOfKinWorkPhone_Formatted

		public void ValidateGS_NextOfKinWorkPhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GS_NextOfKinWorkPhone_FormattedInfo);
		}

		protected virtual void CheckGS_NextOfKinWorkPhone_Formatted()
		{
			if (Parent.ViewingRights.ViewEmergencyContactAllowedForStaff)
			{
				ValidatePhoneNumber(Parent.GS_NextOfKinWorkPhone_FormattedInfo, Parent.GS_NextOfKinWorkPhoneInfo, Parent.GS_NextOfKinWorkPhone_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region GS_EmergencyHomePhone_Formatted

		public void ValidateGS_EmergencyHomePhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GS_EmergencyHomePhone_FormattedInfo);
		}

		protected virtual void CheckGS_EmergencyHomePhone_Formatted()
		{
			if (Parent.ViewingRights.ViewEmergencyContactAllowedForStaff)
			{
				ValidatePhoneNumber(Parent.GS_EmergencyHomePhone_FormattedInfo, Parent.GS_EmergencyHomePhoneInfo, Parent.GS_EmergencyHomePhone_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region GS_EmergencyWorkPhone_Formatted

		public void ValidateGS_EmergencyWorkPhone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GS_EmergencyWorkPhone_FormattedInfo);
		}

		protected virtual void CheckGS_EmergencyWorkPhone_Formatted()
		{
			if (Parent.ViewingRights.ViewEmergencyContactAllowedForStaff)
			{
				ValidatePhoneNumber(Parent.GS_EmergencyWorkPhone_FormattedInfo, Parent.GS_EmergencyWorkPhoneInfo, Parent.GS_EmergencyWorkPhone_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region Implementations

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			if (Parent.IsDetailsModifiable)
			{
				PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, Parent.DefaultCountryCodeForPhoneNumbers);
			}
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		#endregion

		#endregion

		#region GS_City

		protected override void CheckGS_City()
		{
			base.CheckGS_City();
			if (!Parent.GS_IsResource && !ShouldValidateAddress())
			{
				MandatoryValidation.CheckEntered(Parent.GS_CityInfo);
			}
		}

		bool ShouldValidateAddress()
		{
			return
				Parent.Country != null && EnvProxy.Instance.CurrentCompany != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Parent.Country.PK.ToGuid(), Parent.ValidationSection);
		}

		#endregion

		#region GS_RN_NKCountryCode
		protected override void CheckGS_RN_NKCountryCode()
		{
			if (!Parent.GS_IsResource && Parent.ViewingRights.ViewHomeAddressAllowedForStaff)
			{
				base.CheckGS_RN_NKCountryCode();
				var info = Parent.GS_RN_NKCountryCodeInfo;

				if (!info.HasErrors())
				{
					MandatoryValidation.CheckEntered(info);
					ListValidation.ErrorIfInvalidCode(info);
				}
			}
		}
		#endregion

		#region Validate_DatabaseAccessGroupFlag

		public virtual void ValidateDatabaseAccessGroupFlag(ZPropertyInfo propertyInfo)
		{
			ValidateCalculatedProperty(propertyInfo);
		}

		protected virtual void CheckIsDatabaseDeveloper()
		{
		}

		protected virtual void CheckIsReadOnlyDBUser()
		{
		}

		protected virtual void CheckIsBackupOperator()
		{
		}

		#endregion

		#region GS_IsController

		protected override void CheckGS_IsController()
		{
			base.CheckGS_IsController();
			if (Parent.GS_IsController && Parent.GS_IsDevice)
			{
				Parent.GS_IsControllerInfo.AddError(Res.GetString("44098899-83BE-4CC2-A6BC-46CC1274C015", "Is Controller cannot be enabled on an Is Device Only Staff record."));
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateStaffPlainTextPassword();
			ValidateStaffConfirmPassword();
			ValidateSecurityBranch();
			ValidateSecurityDepartment();
			ValidateSalesTeams();
			ValidateManagers();
			ValidateGS_HomePhone_Formatted();
			ValidateGS_WorkPhone_Formatted();
			ValidateGS_MobilePhone_Formatted();
			ValidateGS_FaxNum_Formatted();
			ValidateGS_EmergencyHomePhone_Formatted();
			ValidateGS_EmergencyWorkPhone_Formatted();
			ValidateGS_NextOfKinHomePhone_Formatted();
			ValidateGS_NextOfKinWorkPhone_Formatted();
			ValidateDatabaseAccessGroupFlag(Parent.IsDatabaseDeveloperInfo);
			ValidateDatabaseAccessGroupFlag(Parent.IsReadOnlyDBUserInfo);
			ValidateDatabaseAccessGroupFlag(Parent.IsBackupOperatorInfo);
		}

		public const string InvalidLoginNameCharacters = "\"/\\[]:;|=,+*?<>";

		public static string InvalidLoginNameCharactersUserReadable()
		{
			string result = string.Empty;
			foreach (char symbol in InvalidLoginNameCharacters)
			{
				result += symbol + " ";
			}
			return result.Trim();
		}

		public static bool IsStartsWithWhiteSpace(ZString text)
		{
			return text.Length > 0 && char.IsWhiteSpace(text[0]);
		}
	}
}
