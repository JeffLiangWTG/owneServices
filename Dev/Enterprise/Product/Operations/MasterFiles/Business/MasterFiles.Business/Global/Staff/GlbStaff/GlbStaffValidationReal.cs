using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffValidationReal : GlbStaffValidation
	{
		public GlbStaffValidationReal(GlbStaff parent)
			: base(parent)
		{
		}

		public new GlbStaff Parent
		{
			get { return base.Parent; }
		}

		#region GS_Code

		protected override void CheckGS_Code()
		{
			base.CheckGS_Code();

			if (Parent.GS_CodeInfo.HasChanges && Parent.HasEverLoggedIn)
			{
				Parent.GS_CodeInfo.AddError(Res.GetString("c117bac5-59f8-4852-bd26-d229a32ed5d2", "Staff Member's Initials cannot be changed from {0} because these initials have already been used to log events on other records in the system.", Parent.GS_CodeInfo.OriginalValue));
			}
		}

		#endregion

		#region GS_EmploymentBasis

		protected override void CheckGS_EmploymentBasis()
		{
			base.CheckGS_EmploymentBasis();
			ListValidation.ErrorIfInvalidCode(Parent.GS_EmploymentBasisInfo);

			if (Parent.GS_EmploymentBasisInfo.HasErrors())
			{
				Parent.GS_EmploymentBasisInfo.AddError(Res.GetString("d6d5bf3e-7854-4162-96e3-154e23db2c7e", "Go to the Staff member's Job Title and add/change their Employment Type there."));
			}
		}

		#endregion

		#region Database

		void CheckADUserIfADIntegrationEnabled(ZPropertyInfo info, bool hasChanges)
		{
			if (Parent.IsADIntegrationEnabled && hasChanges && (ZBool)info.Value)
			{
				if (!Parent.IsADLinked || !Parent.CanAccessDirectoryEntry() || !AreCW1AndADLoginNamesTheSame())
				{
					info.AddError(Res.GetString("750AB978-2394-4FA5-9087-344F91473C6D", "Login Name '{0}' could not be found or is not accessible in Active Directory, please ensure the staff is synchronized to Active Directory and the Domain is accessible before enable this option. If the staff was created or renamed recently, please try again in a few minutes to allow it to be synchronized with Active Directory.", Parent.GS_LoginName));
				}
			}
		}

		bool AreCW1AndADLoginNamesTheSame()
		{
			// It is considered same if CW1 login name matches to either UPN or SAMAccountName
			return (Parent.GS_LoginName.EqualsIgnoringCase(Parent.ADLoginName) || Parent.GS_LoginName.EqualsIgnoringCase(Parent.ADLoginNamePreWin2K));
		}

		#region DatabaseAccessGroupFlag

		protected override void CheckIsDatabaseDeveloper()
		{
			base.CheckIsDatabaseDeveloper();
			CheckADUserIfADIntegrationEnabled(Parent.IsDatabaseDeveloperInfo, Parent.IsDatabaseDeveloperChanged);
			ValidateDatabaseAccessGroupFlag(Parent.IsReadOnlyDBUserInfo);
			if (Parent.IsDatabaseDeveloperChanged)
			{
				var useModernSqlSecuritySystem = EnvProxy.Instance.Registry.UseModernSqlSecuritySystem;
				if (!Parent.IsDatabaseDeveloper && !useModernSqlSecuritySystem && UserOwnsDatabaseObjects())
				{
					Parent.IsDatabaseDeveloperInfo.AddError(Res.GetString("GlbStaff_IsDatabaseDeveloper_FailedToDropUser", "Unable to drop user from Database role due to database objects existing in schemas owned by this user. Please address this first and then reload the form in order to remove the user from this role."));
				} else if (!Parent.IsDatabaseDeveloper && useModernSqlSecuritySystem && UserOwnsSchemaInUserRepository())
				{
					Parent.IsDatabaseDeveloperInfo.AddError(Res.GetString("GlbStaff_IsDatabaseDeveloper_FailedToDropUserWithSchema", "Unable to drop user from Database role due to some existing database schema(s) owned by this user. Please address this first and then reload the form in order to remove the user from this role."));
				}

				if (Parent.GS_IsDevice && Parent.IsDatabaseDeveloper)
				{
					Parent.IsDatabaseDeveloperInfo.AddError(Res.GetString("A8FAE289-0E43-4740-A591-C8F52B5A8414", "Is Database Developer cannot be enabled on an Is Device Only Staff record."));
				}

				CheckFlexibleDatabaseAccessRoleEnabled(Parent.IsDatabaseDeveloperInfo, DbRoleTypes.DbDataWriterRole, Res.GetString("385a0f05-75f4-4534-98ab-7f5f054cca3c", "Database Developer"));
			}
		}

		bool UserOwnsDatabaseObjects() => DbUserManager.StaffLoginOwnsSchemaWithCurrentObjects(Db.Connection, Parent.GS_LoginName, Parent.GetDownLevelLogonName);

		bool UserOwnsSchemaInUserRepository()
		{
			var userRepository = Db.Connection.GetDatabases(DatabaseType.UserRepository).FirstOrDefault();

			if (!string.IsNullOrWhiteSpace(userRepository))
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(userRepository))
				{
					return DbUserManager.StaffUserOwnsSchema(Db.Connection, Parent.GS_LoginName, Parent.GetDownLevelLogonName);
				}
			}

			return false;
		}

		protected override void CheckIsReadOnlyDBUser()
		{
			base.CheckIsReadOnlyDBUser();
			CheckADUserIfADIntegrationEnabled(Parent.IsReadOnlyDBUserInfo, Parent.IsReadOnlyDBUserChanged);
			if (Parent.IsReadOnlyDBUserChanged)
			{
				CheckFlexibleDatabaseAccessRoleEnabled(Parent.IsReadOnlyDBUserInfo, DbRoleTypes.CwRestrictedReaderRole, Res.GetString("44b66761-0d5e-4c5b-ad7f-ab953a95c652", "Database Reader"));
			}
			if (Parent.IsDatabaseDeveloper && !Parent.IsReadOnlyDBUser)
			{
				Parent.IsReadOnlyDBUserInfo.AddWarning(Res.GetString("9cfc0739-b7b5-452a-9236-55dbf86b9b08", "Staff should probably be granted read permission."));
			}

			if (Parent.GS_IsDevice && Parent.IsReadOnlyDBUser)
			{
				Parent.IsReadOnlyDBUserInfo.AddError(Res.GetString("8E8B3690-9032-47D6-8660-FB439FF296BC", "Is Database Reader cannot be enabled on an Is Device Only Staff record."));
			}
		}

		protected override void CheckIsBackupOperator()
		{
			base.CheckIsBackupOperator();
			CheckADUserIfADIntegrationEnabled(Parent.IsBackupOperatorInfo, Parent.IsBackupOperatorChanged);
			if (Parent.IsBackupOperatorChanged)
			{
				CheckFlexibleDatabaseAccessRoleEnabled(Parent.IsBackupOperatorInfo, DbRoleTypes.DbBackupOperatorRole, Res.GetString("cc4dbf18-f25d-4a32-b239-3854dd88c222", "Backup Operator"));
			}

			if (Parent.IsBackupOperator && Parent.GS_IsDevice)
			{
				Parent.IsBackupOperatorInfo.AddError(Res.GetString("411C2DD6-2264-4EDD-8AE9-4040105C9A77", "Is Backup Operator cannot be enabled on an Is Device Only Staff record."));
			}
		}

		void CheckFlexibleDatabaseAccessRoleEnabled(ZPropertyInfo info, string databaseAccessGroupRole, string databaseAccessGroupRoleNameText)
		{
			if (!(ZBool)info.Value)
			{
				var flexibleDatabaseAccessGroups = Parent.Groups.Cast<GlbGroup>().Where(g => g.Roles.Any(r => r.GGR_RoleName == databaseAccessGroupRole))
					.Where(g => g.PK != GlbGroup.DbDeveloperGroupPK && g.PK != GlbGroup.DbReaderGroupPK && g.PK != GlbGroup.BackupOperatorGroupPK)
					.OrderBy(g => g.GG_Code)
					.ToArray();

				if (flexibleDatabaseAccessGroups.Length > 0)
				{
					if (flexibleDatabaseAccessGroups.Length == 1)
					{
						var warningMsg = Res.GetString("7eaccb99-6850-459a-bd9b-96c69257a22c", "Group {0} with the {1} role is also associated with this staff.");

						info.AddWarning(string.Format(warningMsg, flexibleDatabaseAccessGroups[0].GG_Code, databaseAccessGroupRoleNameText));
					}
					else
					{
						var warningMsg = new StringBuilder();
						warningMsg.AppendLine("==============================================================");
						if (flexibleDatabaseAccessGroups.Length <= 10)
						{
							warningMsg.AppendLine(Res.GetString("13d17b26-249d-4cd3-bb9b-25482580de0b", "{0} groups with the {1} role are associated with this staff:"));
						}
						else
						{
							warningMsg.AppendLine(Res.GetString("d2623be6-53fe-4847-868d-1a5cc23b680e", "{0} groups with the {1} role are associated with this staff. The first 10 are shown:"));
						}
						warningMsg.AppendLine(string.Join(",\r\n", flexibleDatabaseAccessGroups.Take(10).Select(g => g.GG_Code)));
						warningMsg.AppendLine("==============================================================");

						info.AddWarning(string.Format(warningMsg.ToString(), flexibleDatabaseAccessGroups.Length, databaseAccessGroupRoleNameText));
					}
				}
			}
		}

		#endregion

		#endregion

		#region GS_UserSignatureIsValidZBlobSize

		protected override void CheckGS_UserSignatureIsValidZBlobSize()
		{
		}

		#endregion

		#region GS_RN_NKNationalityCode

		protected override void CheckGS_RN_NKNationalityCode()
		{
			if (Env.CurrentCompany?.Country?.Code == Core.Constants.CountryCodes.Singapore)
			{
				base.ValidateGS_RN_NKNationalityCode();
				ListValidation.ErrorIfInvalidCode(Parent.GS_RN_NKNationalityCodeInfo);
			}
		}

		#endregion

		#region GS_LoginName

		protected override void CheckGS_LoginName()
		{
			base.CheckGS_LoginName();
			MandatoryValidation.CheckEntered(Parent.GS_LoginNameInfo);
			if (((ZString)Parent.GS_LoginNameInfo.Value).Length < 2)
			{
				Parent.GS_LoginNameInfo.AddError(Res.GetString("647b0b86-a612-4dbe-939c-7dc191b683b5", "Login Name must be at least 2 characters"));
			}

			if (!IsUniqueOnStaff(GlbStaffSchema.GS_LoginName, Parent.GS_LoginNameInfo.Value))
			{
				Parent.GS_LoginNameInfo.AddError(Res.GetString("74543a96-e497-4ec5-a452-0cbca5c75427", "Staff Member's Login Name must be unique in the system"));
			}

			if (Parent.IsADIntegrationEnabled)
			{
				CheckForADLoginNameConflict();

				if ((Parent.GS_CanLogin || Parent.IsADLinked) && !Parent.GS_IsSystemAccount)
				{
					// TO-DO: this prefix check should be re-apply when CanLogin is set to true
					var expectedPrefix = ObjectFactory.Get<IADRegistry>().UserLoginPrefix;

					if (!string.IsNullOrEmpty(expectedPrefix) && !Parent.GS_LoginName.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
					{
						Parent.GS_LoginNameInfo.AddError(Res.GetString("90cdad6d-d309-4d79-8fe6-45cdf5a268e6", "Login Name must begin with '{0}'.", expectedPrefix));
					}
				}

				if (Parent.GS_LoginNameInfo.HasChanges && Parent.IsADLinked && Parent.DatabaseAccessGroupRoles.Count > 0 && !((ZString)Parent.GS_LoginNameInfo.Value).EqualsIgnoringCase((ZString)Parent.GS_LoginNameInfo.OriginalValue))
				{
					// Do not allow LoginName change if it has DB access and linked
					Parent.GS_LoginNameInfo.AddError(Res.GetString("A46AB195-FA49-4CD8-A96A-5279DA5E40A0", "Login Name cannot be changed while the staff has database access and Active Directory Integration is enabled. Please disable database access and try again."));
				}
			}
		}

		void CheckForADLoginNameConflict()
		{
			if (Parent.GS_CanLogin && !Parent.GS_IsSystemAccount)
			{
				var oldLoginName = Parent.GS_LoginNameInfo.OriginalValue.ToString();
				var newLoginName = Parent.GS_LoginNameInfo.Value.ToString();

				if (AreDifferentString(oldLoginName, newLoginName) || !Parent.IsInDatabase)
				{
					// We only care when the login name is literally different and don't care about case change, except it is a new staff
					try
					{
						var adUserCredentials = ObjectFactory.Get<IADEntityProvider>().GetADUser(Parent).DomainCredentials;
						var directorySearcher = ObjectFactory.Get<IDirectorySearcherProvider>().GetDirectorySearcher(adUserCredentials, false);

						var matchedAdUser = directorySearcher.FindUser(Parent.GS_LoginName);
						if (matchedAdUser != null)
						{
							if (matchedAdUser.Guid == Parent.GS_ActiveDirectoryObjectGuid)
							{
								var adLoginNameUPN = SearcherFilter.UserNameComponent(matchedAdUser.UserPrincipalName);

								if (AreDifferentAccent(newLoginName, adLoginNameUPN))
								{
									Parent.GS_LoginNameInfo.AddError(Res.GetString("6F9BCF80-9AC6-4AF7-9B32-380C03D68BA4",
										@"Please specify a different {0} as Active Directory will not accept this {0} due to the difference only in the diacritic marks (e.g. {1} to {2}) or ligature (e.g. {3} to {4} or {5} to {6}).
({7}: {8}, Active Directory: {9})",
										Parent.GS_LoginNameInfo.HumanReadableName,
										"à", "a",
										"Æ", "AE",
										"ß", "ss",
										BrandingFactory.Instance.ProductName,
										Parent.GS_LoginName,
										SearcherFilter.UserNameComponent(matchedAdUser.UserPrincipalName)));
								}
							}
							else
							{
								//returns a different AD user
								if (Parent.IsADLinked)
								{
									// If the staff is already linked, but login name returns a different AD user, it is a conflict where the login name has been used by another user in AD
									Parent.GS_LoginNameInfo.AddError(Res.GetString("3D2A380B-CC89-4E16-ADC7-5E047A537215", "Please specify a different {0} as it has been used by another user in Active Directory.", Parent.GS_LoginNameInfo.HumanReadableName));
								}
								else
								{
									// If the staff is not linked, ensure the returned AD user is not linked to another staff in CW1
									if (IsADUserLinkedToAnotherStaff(matchedAdUser))
									{
										Parent.GS_LoginNameInfo.AddError(Res.GetString("7104E8DB-299A-4C39-A932-F398ECA34CE1", "Please specify a different {0} as it has been used by an Active Directory user which has been linked to another staff member.", Parent.GS_LoginNameInfo.HumanReadableName));
									}
								}
							}
						}
						else if (directorySearcher.FindGroup(Parent.GS_LoginName) != null)
						{
							// Conflict with a group name
							Parent.GS_LoginNameInfo.AddError(Res.GetString("DCC0BD02-4826-40F8-9430-2B915B3192CC", "Please specify a different {0} as it has been used by a group in Active Directory.", Parent.GS_LoginNameInfo.HumanReadableName));
						}
					}
					catch (Exception ex) when (ex is DirectoryServicesException || ex is InvalidOUException || ex is COMException)
					{
						Parent.GS_LoginNameInfo.AddError(Res.GetString("34F97F12-743C-43A0-8CE6-5E38384F4848", @"Cannot connect to Active Directory: {0}.
Please contact your system administrator or try again later.", ex.Message));
					}
				}
			}
		}

		internal static bool AreSameWhenTransliterateToEnglish(string value1, string value2) => string.Equals(WesternLanguageTransliterationHelper.TransliterateToEnglish(value1), WesternLanguageTransliterationHelper.TransliterateToEnglish(value2), StringComparison.OrdinalIgnoreCase);

		static bool AreDifferentString(string value1, string value2) => !string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);

		// Accent differnt, eg. "à" vs "a" - they are different string but are same when transliterate to english
		public static bool AreDifferentAccent(string value1, string value2) => AreDifferentString(value1, value2) && AreSameWhenTransliterateToEnglish(value1, value2);

		internal bool IsADUserLinkedToAnotherStaff(IUserDirectoryEntry adUser)
		{
			var query = new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, adUser.Guid);
			query.AddToFilter(JoinCondition.And, GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return Parent.Factory.ExistsInDatabase(GlbStaffSchema.Constants.TableName, query);
		}

		#endregion

		#region GS_UserAddress1

		protected override void CheckGS_UserAddress1()
		{
			base.CheckGS_UserAddress1();
			MandatoryValidation.CheckEntered(Parent.GS_UserAddress1Info);
		}

		#endregion

		#region GS_UserAddress2

		protected override void CheckGS_UserAddress2()
		{
			base.CheckGS_UserAddress2();
			if (Parent.IsADIntegrationEnabled && !Parent.GS_UserAddress2.IsEmpty)
			{
				Parent.GS_UserAddress2Info.AddWarning(Res.GetString("910c7fde-06ea-4bad-8fc4-57f3921b9b4f", "Active Directory does not support multiple address values. This value will not be synced with Active Directory."));
			}
		}

		#endregion

		#region GS_Birthdate

		protected override void CheckGS_BirthdateIsValidZDateRange()
		{
			if (Parent.GS_Birthdate > ZDate.Today)
			{
				Parent.GS_BirthdateInfo.AddError(Res.GetString("f898cdd4-a33b-4d00-9d5b-525330517d61", "Birthdate cannot be in the future."));
			}
		}

		#endregion

		#region GS_EmploymentDate

		/// <summary>
		/// Employment date can be > 10 years so we don't want base validation
		/// </summary>
		protected override void CheckGS_EmploymentDateIsValidZDateTimeRange()
		{
		}

		#endregion

		#region GS_DepartureDate

		protected override void CheckGS_DepartureDate()
		{
			base.CheckGS_DepartureDate();
			if (Parent.GS_EmploymentDate.IsValid && Parent.GS_DepartureDate.IsValid)
			{
				if (Parent.GS_DepartureDate < Parent.GS_EmploymentDate)
				{
					Parent.GS_DepartureDateInfo.AddError(Res.GetString("7deac12a-8beb-4bc3-a79e-1536af5a8139", "Departure date should be after employment start date"));
				}
			}
		}

		#endregion

		#region GS_ProfilePhoto

		protected override void CheckGS_ProfilePhoto()
		{
			base.CheckGS_ProfilePhoto();
			if (Parent.IsADIntegrationEnabled)
			{
				if (Parent.GS_ProfilePhoto.Length > MaxADProfilePhotoBytes)
				{
					Parent.GS_ProfilePhotoInfo.AddError(Res.GetString("c2cd7113-2065-4cd7-9c38-7e1cbc44d3ce", "This image is too large to synchronize with Active Directory. It should be an image with a size no greater than 100KB."));
				}

				if (!Parent.GS_ProfilePhoto.IsEmpty && !IsValidImage(Parent.GS_ProfilePhoto))
				{
					Parent.GS_ProfilePhotoInfo.AddError(Res.GetString("2B033E0B-D0AC-4B87-852A-AEE1A2D5C26F", "The image supplied is invalid or corrupt. It cannot be synchronized with Active Directory."));
				}
			}
		}

		protected bool IsValidImage(byte[] data)
		{
			var result = true;
			try
			{
				using (var img = new Bitmap(new MemoryStream(data)))
				{ }
			}
			catch (ArgumentException)
			{
				result = false;
			}

			return result;
		}

		public const int MaxADProfilePhotoBytes = 102400; //100kb

		#endregion

		#region Staff Password

		PasswordControl fPasswordValidator;

		PasswordControl PasswordValidator
		{
			get
			{
				if (fPasswordValidator == null)
				{
					fPasswordValidator = new PasswordControl();
				}
				return fPasswordValidator;
			}
		}

		protected override void CheckStaffPlainTextPassword()
		{
			base.CheckStaffPlainTextPassword();

			if (!Parent.IsInDatabase && !Parent.IsADIntegrationEnabled)
			{
				MandatoryValidation.CheckEntered(Parent.StaffPlainTextPasswordInfo);
				if (Parent.StaffPlainTextPassword.Length != 0 && Parent.StaffConfirmPassword.Length != 0)
				{
					if (Parent.StaffPlainTextPassword != Parent.StaffConfirmPassword)
					{
						Parent.StaffPlainTextPasswordInfo.AddError(Res.GetString("8c227df5-f32f-4cbe-82a4-e3b29504f6d4", "Password does not match confirm password"));
					}
				}
				if (!PasswordValidator.IsValidPassword(Parent.StaffPlainTextPassword))
				{
					Parent.StaffPlainTextPasswordInfo.AddError(PasswordValidator.Errors[0]);
				}
			}
		}

		#endregion

		#region Staff Confirm Password

		protected override void CheckStaffConfirmPassword()
		{
			base.CheckStaffConfirmPassword();

			if (!Parent.IsInDatabase && !Parent.IsADIntegrationEnabled)
			{
				MandatoryValidation.CheckEntered(Parent.StaffConfirmPasswordInfo);
				if (Parent.StaffPlainTextPassword.Length != 0 && Parent.StaffConfirmPassword.Length != 0)
				{
					if (Parent.StaffPlainTextPassword != Parent.StaffConfirmPassword)
					{
						Parent.StaffConfirmPasswordInfo.AddError(Res.GetString("f43f1138-92f4-41c2-8d83-6a719f42344c", "Confirm password does not match password"));
					}
				}
			}
		}

		#endregion

		#region Password Policy

		protected override void CheckChangePasswordAtNextLogin()
		{
			CheckIsADLinkedForPasswordPolicy(Parent.ChangePasswordAtNextLoginInfo);
			CheckADObjectExists(Parent.ChangePasswordAtNextLoginInfo);
			CheckPasswordPolicy(Parent.ChangePasswordAtNextLoginInfo);
		}

		protected override void CheckPasswordNeverChanges()
		{
			CheckIsADLinkedForPasswordPolicy(Parent.PasswordNeverChangesInfo);
			CheckADObjectExists(Parent.PasswordNeverChangesInfo);
			CheckPasswordPolicy(Parent.PasswordNeverChangesInfo);
			AddWarningIfPasswordNeverExpiresCouldBeOverriddenByADPasswordPolicy();
		}

		string ADEntryMissingErrorMessage => Res.GetString("6B29A37C-1B36-4095-82FE-2764006FE537", "Could not retrieve the value of this field because the Active Directory User '{0}' is missing or is not accessible, please contact your system administrator.", Parent.GS_LoginName);

		string ADEntryNotYetCreatedErrorMessage => Res.GetString("3EA73FEA-714C-4EA5-8D00-8C4BDC7DE6C6", "The Active Director User '{0}' is pending for creation and it is not accessible at this time, please try again in 15 minutes.", Parent.GS_LoginName);

		void AddWarningIfPasswordNeverExpiresCouldBeOverriddenByADPasswordPolicy()
		{
			if (Parent.ShouldUseADPasswordPolicy && Parent.CanAccessDirectoryEntry() && !Parent.ShouldDisablePasswordSettings)
			{
				var adUser = ObjectFactory.Get<IADEntityProvider>().GetADUser(Parent);

				// Show a warning if the effective value of 'Password Never Expires' is true but the user attribute value is false or it is about to be set to false as it might be overwritten by the Fine-grained password policy value
				if (adUser.PasswordDoesntExpire && (!adUser.PasswordDoesntExpireUserAttribute || !Parent.PasswordNeverChanges))
				{
					Parent.PasswordNeverChangesInfo.AddWarning(Res.GetString("0B957D71-F774-41CE-A592-771EA51752EA", "When unticked, this setting could be overridden by Password Policy.", Parent.PasswordNeverChangesInfo.HumanReadableName));
				}
			}
		}

		void CheckADObjectExists(ZPropertyInfo info)
		{
			if (Parent.ShouldUseADPasswordPolicy && !Parent.CanAccessDirectoryEntry())
			{
				if (Parent.IsInDatabase && Parent.GS_SystemCreateTimeUtc.IsValid && Parent.GS_SystemCreateTimeUtc.AddHours(1) > ZDateTime.UtcNow)
				{
					info.AddWarning(ADEntryNotYetCreatedErrorMessage);
				}
				else
				{
					info.AddWarning(ADEntryMissingErrorMessage);
				}
			}
		}

		string ADIsNotLinkedMessage => Res.GetString("338CD807-7EA3-4B9B-95BB-DF6E3E9D0785", "This option is not synchronized as the staff has not been linked to Active Directory.");

		void CheckIsADLinkedForPasswordPolicy(ZPropertyInfo info)
		{
			if (Parent.IsADIntegrationEnabled && !Parent.IsADLinked)
			{
				info.AddWarning(ADIsNotLinkedMessage);
			}
		}

		string PasswordPolicyErrorMessage => Res.GetString("02D511AA-3F15-4582-91ED-39DA08F8EF88", "You cannot select both '{0}' and '{1}'.", Parent.ChangePasswordAtNextLoginInfo.HumanReadableName, Parent.PasswordNeverChangesInfo.HumanReadableName);

		string PasswordPolicyDisabledMessage => Res.GetString("9217D543-2CD3-49E8-BF18-954699BD6FD1", "This setting should be set in the domain.");

		void CheckPasswordPolicy(ZPropertyInfo info)
		{
			if (Parent.IsADPrimaryOneWaySync && Parent.ShouldDisablePasswordSettings)
			{
				info.AddWarning(PasswordPolicyDisabledMessage);
			}

			if (Parent.ChangePasswordAtNextLogin && Parent.PasswordNeverChanges)
			{
				info.AddError(PasswordPolicyErrorMessage);
			}
		}

		#endregion

		#region Security Branch

		protected override void CheckSecurityBranch()
		{
			base.CheckSecurityBranch();
			ListValidation.ErrorIfInvalidPK(Parent.SecurityBranchInfo);
		}

		#endregion

		#region Security Department

		protected override void CheckSecurityDepartment()
		{
			base.CheckSecurityDepartment();
			ListValidation.ErrorIfInvalidPK(Parent.SecurityDepartmentInfo);
		}

		#endregion

		#region GS_GE_HomeDepartment

		protected override void CheckGS_GE_HomeDepartment()
		{
			base.CheckGS_GE_HomeDepartment();

			if (!(Parent.GS_IsSystemAccount || Parent.GS_IsResource))
			{
				if (Parent.ShowBranchDepartmentAndPositionErrorsAsWarnings)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.GS_GE_HomeDepartmentInfo);
				}
				else
				{
					MandatoryValidation.CheckEntered(Parent.GS_GE_HomeDepartmentInfo);
				}
			}
		}

		#endregion

		#region GS_IsActive

		protected override void CheckGS_IsActive()
		{
			base.CheckGS_IsActive();
			if (!Parent.GS_IsActive && Parent.Groups.Count > 0)
			{
				Parent.GS_IsActiveInfo.AddError(Res.GetString("c620c8d4-8be8-48a9-9243-fe06174819af", "Please remove this staff member from all groups before marking this staff member as inactive."));
			}
			else if (Parent.GS_IsActive && !Parent.Groups.ContainsAnyCode(GlbGroup.AllStaffGroupCode))
			{
				Parent.GS_IsActiveInfo.AddError(Res.GetString("c710f124-1f98-45d8-81e8-012b07470d77", "All active staff must be part of the 'All Users' group. Please add this staff member to the 'All Users' group."));
			}

			if (!Parent.GS_IsActive && Parent.DirectReports.Any(x => x.GSM_EndDate == ZDateTime.Empty || x.GSM_EndDate > ZDateTime.Today))
			{
				Parent.GS_IsActiveInfo.AddWarning(Res.GetString("59833964-c5f5-4376-bd33-37c7f9f8fc16", "Please deactivate and/or transfer any current or future direct reports (This is automatically done on the Staff Form)."));
			}

			if (!Parent.GS_IsActive && Parent.Managers.Any(x => x.GSM_EndDate == ZDateTime.Empty || x.GSM_EndDate > ZDateTime.Today))
			{
				Parent.GS_IsActiveInfo.AddError(Res.GetString("522c21be-4dc3-4454-a177-d2f669e4a695", "Please deactivate any current or future managers."));
			}

			if (!Parent.GS_IsActive && Parent.GS_IsActiveInfo.HasChanges && Env.CurrentCompany != null)
			{
				var reportDescriptions = Parent.GetScheduledReportDescriptionAssignedToPrintUserTop4();
				if (reportDescriptions != null && reportDescriptions.Count > 0)
				{
					Parent.GS_IsActiveInfo.AddError(
						Res.GetString(
							"FC43B0A0-2A25-425A-A30E-C8A7157F6B66",
							"Staff cannot be deactivated because it has been set as a print user or recipient on at least one scheduled report:\r\n{0}",
							string.Join(", ", reportDescriptions.Take(3))
							) +
						(reportDescriptions.Count > 3 ? "..." : ".")
						);
				}
			}

			if (!Parent.GS_IsActive && Env.CurrentCompany?.Country?.Code == Core.Constants.CountryCodes.Singapore)
			{
				var wrapper = Parent.GetSGWrapper();
				var accessPassword = wrapper?.AccessPassword;
				if (accessPassword != null && (!accessPassword.GP_UserID.IsEmpty || !accessPassword.CurrentDecryptedPassword.IsEmpty || !accessPassword.NextDecryptedPassword.IsEmpty))
				{
					Parent.GS_IsActiveInfo.AddError(Res.GetString("DF3611CE-D020-4DBC-84F6-0224F6E5FD16", "An ACCESS user credential is detected for this staff member. Please remove the ACCESS credential before deactivating the User."));
				}
			}
			if (!Parent.GS_IsActive && Parent.HasActiveEDIClient)
			{
				Parent.GS_IsActiveInfo.AddError(Res.GetString("845231F2-DBB4-491F-9EB0-8CF22BD9661C", "Staff cannot be deactivated because it has an active EDI client profile linked to it."));
			}

			if(!Parent.GS_IsActive)
			{
				var documents = Parent.GetUnpublishedCustomizedDocumentsAndReports();
				if (documents.Length > 0)
				{
					var menuItemInfos = documents.Select(x => x.DocumentIdMultilingual).ToList();

					Parent.GS_IsActiveInfo.AddError(
						Res.GetString(
							"4c8c50be-5901-4786-a4b0-b1ea92ea15b4",
							"Staff cannot be deactivated because it has least one unpublished document or report linked to it:\r\n{0}",
							string.Join(", ", menuItemInfos.Take(3))
						) +
						(menuItemInfos.Count > 3 ? "..." : ".")
					);
				}
			}
		}

		#endregion

		#region ValidateGroupsForLocalAdmin

		public void ValidateGroupsForLocalAdmin()
		{
			ValidateCalculatedProperty(Parent.StaffMemberBelongsToGroupsLabelTextInfo);
		}

		protected void CheckStaffMemberBelongsToGroupsLabelText()
		{
			if (Parent.NeedsLocalAdminGroupToBeAdded)
			{
				Parent.StaffMemberBelongsToGroupsLabelTextInfo.AddError(Res.GetString("11F951ED-0A4E-47C4-BFC7-F5F79E2B78F6", "User needs to be added to Local Administrator's group before it can be saved."));
			}
		}

		#endregion

		#region GS_GB_HomeBranch

		protected override void CheckGS_GB_HomeBranch()
		{
			base.CheckGS_GB_HomeBranch();
			if (!(Parent.GS_IsSystemAccount || Parent.GS_IsResource))
			{
				if (Parent.ShowBranchDepartmentAndPositionErrorsAsWarnings)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.GS_GB_HomeBranchInfo);
				}
				else
				{
					MandatoryValidation.CheckEntered(Parent.GS_GB_HomeBranchInfo);
				}
			}
		}

		#endregion

		#region GS_DomainName

		protected override void CheckGS_DomainName()
		{
			base.CheckGS_DomainName();
			if (ObjectFactory.Get<IADRegistry>().DomainCredentialsCollection.Any())
			{
				ListValidation.ErrorIfInvalidCode(Parent.GS_DomainNameInfo);
			}
			if (Parent.GS_DomainName.IsEmpty && (Parent.IsADIntegrationEnabled || ObjectFactory.Get<IADRegistry>().HasMultipleDomains))
			{
				Parent.GS_DomainNameInfo.AddWarning(Res.GetString("92A296ED-D1C7-4A8A-89B1-AF5A02EF282B", "The default domain will be used during the next synchronization with Active Directory."));
			}
		}

		#endregion

		protected override void RunAdditionalValidationOnValidationObject(ZPropertyInfo propertyInfo)
		{
			base.RunAdditionalValidationOnValidationObject(propertyInfo);
			AddWarningIfSyncedOneWayFromAD(propertyInfo);
		}

		void AddWarningIfSyncedOneWayFromAD(ZPropertyInfo info)
		{
			if (Parent.GS_IsActive && !Parent.GS_IsSystemAccount && Parent.IsADIntegrationEnabled && info.Name != Parent.GS_IsActiveInfo.Name)
			{
				string infoName = string.Empty;
				if (info.IsPersistent && info.HasChanges)
				{
					infoName = info.Name;
				}
				else if (FormattedPhoneNumberProperties.TryGetValue(info.Name, out var persistentPropertyInfo) && persistentPropertyInfo.HasChanges)
				{
					infoName = persistentPropertyInfo.Name;
				}
				if (!string.IsNullOrEmpty(infoName) && ObjectFactory.Get<IADRegistry>().IsColumnSyncedOneWayFromAD(infoName))
				{
					info.AddWarning(Res.GetString("256d787d-d3b1-469c-ab12-02fc1e438996", "This field is synchronized from Active Directory. Its value will be overridden next time the synchronization occurs."));
				}
			}
		}

		Dictionary<string, ZPropertyInfo> FormattedPhoneNumberProperties
		{
			get
			{
				return formattedPhoneNumberProperties ?? (formattedPhoneNumberProperties = new Dictionary<string, ZPropertyInfo> {
						{ nameof(Parent.GS_WorkPhone_Formatted), Parent.GS_WorkPhoneInfo },
						{ nameof(Parent.GS_FaxNum_Formatted), Parent.GS_FaxNumInfo },
						{ nameof(Parent.GS_HomePhone_Formatted), Parent.GS_HomePhoneInfo },
						{ nameof(Parent.GS_MobilePhone_Formatted), Parent.GS_MobilePhoneInfo }
					});
			}
		}
		Dictionary<string, ZPropertyInfo> formattedPhoneNumberProperties;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGroupsForLocalAdmin();
			ValidateChangePasswordAtNextLogin();
			ValidatePasswordNeverChanges();
		}
	}
}
