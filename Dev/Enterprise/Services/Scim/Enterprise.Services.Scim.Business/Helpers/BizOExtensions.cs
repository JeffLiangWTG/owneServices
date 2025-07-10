using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Exceptions;
using SimpleIdServer.Scim.Parser.Expressions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.Scim.Business
{
	public static class BizOExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Prefix string")]
		const string TelPrefix = "tel:";
		const string RefUsersPrefix = "../Users/";
		const string RefGroupsPrefix = "../Groups/";

		#region Users

		static void UpdateFromScimUser(this GlbStaff staff, ScimUser scimUser, FastStaffCodeCalculator codeCalculator)
		{
			if (staff.GS_IsSystemAccount)
			{
				throw new SCIMNoTargetException("Cannot update a system account");
			}

			SetStaffValue(staff, nameof(scimUser.Active), scimUser.Active);
			SetStaffValue(staff, nameof(scimUser.UserName), scimUser.UserName);
			SetStaffValue(staff, nameof(scimUser.GivenName), scimUser.GivenName);
			SetStaffValue(staff, nameof(scimUser.MiddleName), scimUser.MiddleName);
			SetStaffValue(staff, nameof(scimUser.FamilyName), scimUser.FamilyName);
			SetStaffValue(staff, nameof(scimUser.NameHonorificPrefix), scimUser.NameHonorificPrefix);
			SetStaffValue(staff, nameof(scimUser.NameHonorificSuffix), scimUser.NameHonorificSuffix);
			SetStaffValue(staff, nameof(scimUser.NameFormatted), GetFullName(scimUser.NameFormatted, scimUser.NameHonorificPrefix, scimUser.NameHonorificSuffix));
			SetStaffValue(staff, nameof(scimUser.PhoneNumbersMobile), scimUser.PhoneNumbersMobile, TelPrefix);
			SetStaffValue(staff, nameof(scimUser.PhoneNumbersHome), scimUser.PhoneNumbersHome, TelPrefix);
			SetStaffValue(staff, nameof(scimUser.PhoneNumbersWork), scimUser.PhoneNumbersWork, TelPrefix);
			SetStaffValue(staff, nameof(scimUser.PhoneNumbersFax), scimUser.PhoneNumbersFax);
			SetStaffValue(staff, nameof(scimUser.AddressesStreetAddress), scimUser.AddressesStreetAddress);
			SetStaffValue(staff, nameof(scimUser.AddressesLocality), scimUser.AddressesLocality);
			SetStaffValue(staff, nameof(scimUser.AddressesRegion), scimUser.AddressesRegion);
			SetStaffValue(staff, nameof(scimUser.AddressesPostalCode), scimUser.AddressesPostalCode);
			SetStaffValue(staff, nameof(scimUser.AddressesCountry), string.IsNullOrEmpty(scimUser.AddressesCountry)
				? string.Empty
				: scimUser.AddressesCountry.Length == GlbStaffSchema.GS_RN_NKCountryCode.MaxLength
					? scimUser.AddressesCountry
					: TryGetCountryByName(scimUser.AddressesCountry, staff.Factory));
			SetStaffValue(staff, nameof(scimUser.Title), scimUser.Title);
			SetStaffValue(staff, nameof(scimUser.Email), scimUser.Email);
			SetStaffValue(staff, nameof(scimUser.NickName), scimUser.NickName);
			SetStaffValue(staff, nameof(scimUser.PreferredLanguage), SharedConstants.Languages.English);
			SetStaffValue(staff, nameof(scimUser.OtherReferences), scimUser.OtherReferences);

			HandleExternalId(staff, scimUser);

			staff.GS_CanLogin = Env.Registry.ScimCanLogin;

			SetBranch(staff, scimUser.HomeBranch);
			SetDepartment(staff, scimUser.HomeDepartment);

			if (codeCalculator != null && staff.GS_Code.IsEmpty)
			{
				while (codeCalculator.AvailableStaffCodes.Count > 0)
				{
					var success = false;
					try
					{
						staff.GS_Code = codeCalculator.GetBestCode(staff);

						staff.AllowEmptyPasswordForNewRecord();

						staff.Factory.Save();
						success = true;
					}
					catch (ZSaveException ex)
					{
						if (!ex.IndexNameIfUniqueIndexViolation.IsEmpty)
						{
							if (ex.IndexNameIfUniqueIndexViolation.EqualsIgnoringCase(GlbStaffSchema.Constants.Indexes.NR_UC__GS_Code))
							{
								continue;
							}

							throw new SCIMUniquenessAttributeException($"The value is not unique: {ex.IndexNameIfUniqueIndexViolation}");
						}
						else
						{
							throw;
						}
					}

					if (success)
					{
						break;
					}
				}
			}
		}

		static void HandleExternalId(GlbStaff staff, ScimUser scimUser)
		{
			if (staff.IsInDatabase)
			{
				if (staff.GS_ExternalId.IsEmpty && !string.IsNullOrEmpty(scimUser.ExternalId))
				{
					if (SystemDataRegistry.Instance.ScimClearPrivilegeFlagsOnMatching.Value)
					{
						if (staff.GS_IsController)
						{
							var otherControllerQuery = new ZQuery(GlbStaffSchema.GS_IsController, true);
							otherControllerQuery.AddToFilter(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, staff.PK);

							if (staff.Factory.Exists(typeof(GlbStaff), otherControllerQuery))
							{
								staff.GS_IsController = false;
							}
						}
						staff.IsDatabaseDeveloper = false;
						staff.IsBackupOperator = false;
						staff.IsReadOnlyDBUser = false;
					}

					if (SystemDataRegistry.Instance.ScimClearRoleFlagsOnMatching.Value)
					{
						staff.GS_IsSalesRep = false;
						staff.GS_IsDriver = false;
					}
				}

				if (!staff.GS_ExternalId.EqualsIgnoringCase(scimUser.ExternalId))
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					staff.Logs.AddNew(Events.EditedARecord, $"Matched to external id [{scimUser.ExternalId}].");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}

			SetStaffValue(staff, nameof(scimUser.ExternalId), scimUser.ExternalId);
		}

		static void SetBranch(GlbStaff staff, ZString homeBranch)
		{
			var error = $"[{homeBranch}] is not a valid Branch Code.";

			if (homeBranch.IsEmpty)
			{
				staff.GS_GB_HomeBranch = ZGuid.Empty;
				return;
			}

			if (homeBranch.Length > 3)
			{
				throw new SCIMNoTargetException(error);
			}

			var branchPK = staff.Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, homeBranch))?.PK ?? ZGuid.Empty;

			if (branchPK.IsEmpty)
			{
				throw new SCIMNoTargetException(error);
			}

			staff.GS_GB_HomeBranch = branchPK;
		}

		static void SetDepartment(GlbStaff staff, ZString homeDepartment)
		{
			var error = $"[{homeDepartment}] is not a valid Department Code.";

			if (homeDepartment.IsEmpty)
			{
				staff.GS_GE_HomeDepartment = ZGuid.Empty;
				return;
			}

			if (homeDepartment.Length > 3)
			{
				throw new SCIMNoTargetException(error);
			}

			var departmentPK = staff.Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, homeDepartment))?.PK ?? ZGuid.Empty;

			if (departmentPK.IsEmpty)
			{
				throw new SCIMNoTargetException(error);
			}

			staff.GS_GE_HomeDepartment = departmentPK;
		}

		static string TryGetCountryByName(string addressesCountry, BusinessObjectFactory factory)
		{
			if (string.IsNullOrEmpty(addressesCountry) || addressesCountry.Length == 1)
			{
				return string.Empty;
			}

			var country = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Desc, addressesCountry));

			return country?.RN_Code ?? string.Empty;
		}

		static ScimUser ToScimUser(GlbStaff staff, IEnumerable<SCIMAttributeExpression> included = null, IEnumerable<SCIMAttributeExpression> excluded = null)
		{
			if (staff == null)
			{
				return null;
			}

			var scimUser = new ScimUser();
			scimUser.Id = staff.PK.ToGuid();

			if (ShouldIncludeSimple(AttributeNames.Active, included, excluded))
			{
				scimUser.Active = staff.GS_IsActive;
			}

			scimUser.UserName = staff.GS_LoginName;

			if (ShouldIncludeSimple(AttributeNames.NameFormatted, included, excluded))
			{
				if (!staff.GS_FullName.IsEmpty)
				{
					var sb = new ZStringBuilder();
					if (!staff.GS_NameTitle.IsEmpty)
					{
						sb.Append(staff.GS_NameTitle);
						sb.Append(" ");
					}
					sb.Append(staff.GS_FullName);

					if (!staff.GS_NameSuffix.IsEmpty)
					{
						sb.Append(", ");
						sb.Append(staff.GS_NameSuffix);
					}

					scimUser.NameFormatted = sb.ToString();
				}
			}

			if (ShouldIncludeSimple(AttributeNames.GivenName, included, excluded))
			{
				scimUser.GivenName = staff.GS_GivenName;
			}

			if (ShouldIncludeSimple(AttributeNames.MiddleName, included, excluded))
			{
				scimUser.MiddleName = staff.GS_MiddleName;
			}

			if (ShouldIncludeSimple(AttributeNames.FamilyName, included, excluded))
			{
				scimUser.FamilyName = staff.GS_Surname;
			}

			if (ShouldIncludeSimple(AttributeNames.ExternalId, included, excluded))
			{
				scimUser.ExternalId = staff.GS_ExternalId;
			}

			if (ShouldIncludeSimple(AttributeNames.NameHonorificPrefix, included, excluded))
			{
				scimUser.NameHonorificPrefix = staff.GS_NameTitle;
			}

			if (ShouldIncludeSimple(AttributeNames.NameHonorificSuffix, included, excluded))
			{
				scimUser.NameHonorificSuffix = staff.GS_NameSuffix;
			}

			if (ShouldIncludeSimple(AttributeNames.Title, included, excluded))
			{
				scimUser.Title = staff.GS_Title;
			}

			if (ShouldIncludeSimple(AttributeNames.NickName, included, excluded))
			{
				scimUser.NickName = staff.GS_FriendlyName;
			}

			if (ShouldIncludeSimple(AttributeNames.PreferredLanguage, included, excluded))
			{
				scimUser.PreferredLanguage = staff.GS_WorkingLanguage;
			}

			if (ShouldIncludeSimple(AttributeNames.Emails, included, excluded))
			{
				scimUser.Email = staff.GS_EmailAddress;
			}

			if (ShouldIncludeSimple(AttributeNames.OtherReferences, included, excluded))
			{
				scimUser.OtherReferences = staff.GS_Pager;
			}

			if (ShouldIncludeSimple(AttributeNames.HomeBranch, included, excluded))
			{
				if (!staff.GS_GB_HomeBranch.IsEmpty)
				{
					scimUser.HomeBranch = staff.HomeBranch?.GB_Code ?? ZString.Empty;
				}
			}

			if (ShouldIncludeSimple(AttributeNames.HomeDepartment, included, excluded))
			{
				if (!staff.GS_GE_HomeDepartment.IsEmpty)
				{
					scimUser.HomeDepartment = staff.HomeDepartment?.GE_Code ?? ZString.Empty;
				}
			}

			if (ShouldIncludeSimple(AttributeNames.Groups, included, excluded))
			{
				var groups = new List<GroupMemberWithDisplay>();
				var includeAll = SystemDataRegistry.Instance.ScimReturnAllGroupMembershipsForStaff.Value;
				foreach (GlbGroup group in staff.Groups.Where(g =>
					(includeAll || !g.GG_ExternalId.IsEmpty)
					&& g.GG_Type == GlbGroupTypeList.Codes.Staff
					&& g.PK != GlbGroup.DbReaderGroupPK
					&& g.PK != GlbGroup.DbDeveloperGroupPK
					&& g.PK != GlbGroup.BackupOperatorGroupPK))
				{
					AddUserGroups(groups, group);
					scimUser.Groups = groups;
				}
			}

			if (included == null || !included.Any())
			{
				scimUser.AddressesStreetAddress = staff.GS_UserAddress1;
				scimUser.AddressesLocality = staff.GS_City;
				scimUser.AddressesRegion = staff.GS_State;
				scimUser.AddressesPostalCode = staff.GS_Postcode;
				scimUser.AddressesCountry = staff.GS_RN_NKCountryCode;
				scimUser.PhoneNumbersMobile = staff.GS_MobilePhone;
				scimUser.PhoneNumbersHome = staff.GS_HomePhone;
				scimUser.PhoneNumbersWork = staff.GS_WorkPhone;
				scimUser.PhoneNumbersFax = staff.GS_FaxNum;
			}

			return scimUser;
		}

		static void AddUserGroups(List<GroupMemberWithDisplay> groups, GlbGroup group)
		{
			if (group != null && !groups.Any(g => g.Value == group.PK.ToString()))
			{
				var member = new GroupMemberWithDisplay()
				{
					Display = group.GG_Desc,
					Type = SCIMResourceTypes.Group,
					Ref = RefGroupsPrefix + group.PK,
					Value = group.PK.ToString()
				};

				groups.Add(member);
				AddUserGroups(groups, group.ParentGroup);
			}
		}

		static void SetStaffValue(GlbStaff staff, string scimColumn, object value, string trimStart = "")
		{
			var column = StaffColumnHelper.GetStaffColumn(scimColumn);
			SetColumn(staff, scimColumn, value, column, trimStart);
		}

		static ZString GetFullName(string fullName, string prefix, string suffix)
		{
			if (string.IsNullOrEmpty(fullName))
			{
				return string.Empty;
			}

			if (!string.IsNullOrEmpty(prefix) && fullName.StartsWith(prefix))
			{
				fullName = fullName.Substring(prefix.Length + 1);
			}

			if (!string.IsNullOrEmpty(suffix) && fullName.EndsWith(suffix))
			{
				fullName = fullName.Substring(0, fullName.Length - suffix.Length - 2);
			}

			return fullName;
		}

		#endregion

		#region Groups

		static void UpdateFromScimGroup(this GlbGroup group, ScimGroup scimGroup)
		{
			if (group.GG_IsSystemDefined)
			{
				throw new SCIMNoTargetException("Cannot update a system group");
			}

			var duplicateQuery = new ZQuery(GlbGroupSchema.GG_Desc, scimGroup.DisplayName);
			duplicateQuery.AddToFilter(GlbGroupSchema.PK, SQLComparisonOperator.NotEqual, group.PK);

			if (group.Factory.Exists(typeof(GlbGroup), duplicateQuery))
			{
				throw new SCIMUniquenessAttributeException($"Group with displayName {scimGroup.DisplayName} already exists");
			}

			SetGroupValue(group, nameof(scimGroup.DisplayName), scimGroup.DisplayName);
			SetGroupValue(group, nameof(scimGroup.ExternalId), scimGroup.ExternalId);
			SetMembers(group, scimGroup);
			SetGroupCode(group, scimGroup);
			SetCategory(group, nameof(scimGroup.Category), scimGroup.Category);

			group.GG_IsSecurityEnabled = group.GG_GG_ParentGroup.IsEmpty;
			group.GG_IsActive = true;

			group.TrySave();
		}

		static void SetCategory(GlbGroup group, string scimColumn, string category)
		{
			if (!string.IsNullOrEmpty(category))
			{
				var categories = SystemDataRegistry.Instance.GroupCategoryList.Value;
				if (!categories.ContainsCode(category))
				{
					throw new SCIMNoTargetException($"Category [{category}] does not exist");
				}
			}

			SetGroupValue(group, scimColumn, category);
		}

		static void SetGroupCode(GlbGroup group, ScimGroup scimGroup)
		{
			if (!group.GG_Code.IsEmpty)
			{
				return;
			}

			var initialCode = !string.IsNullOrEmpty(scimGroup.DisplayName) ? scimGroup.DisplayName.Replace(" ", "").ToUpper() : string.Empty;
			if (initialCode.Length > GlbGroupSchema.GG_Code.MaxLength)
			{
				initialCode = initialCode.Substring(0, GlbGroupSchema.GG_Code.MaxLength);
			}

			if (GroupCodeExists(group.Factory, initialCode))
			{
				if (initialCode.Length == GlbGroupSchema.GG_Code.MaxLength)
				{
					initialCode = initialCode.Substring(0, initialCode.Length - 1);
				}

				for (int i = 1; i < int.MaxValue; i++)
				{
					var suffix = i.ToString();
					if (initialCode.Length + suffix.Length > GlbGroupSchema.GG_Code.MaxLength)
					{
						i = 1;
						initialCode = initialCode.Substring(0, initialCode.Length - 1);
						suffix = i.ToString();
					}

					var suggestedCode = initialCode + suffix;
					if (!GroupCodeExists(group.Factory, suggestedCode))
					{
						group.GG_Code = suggestedCode;
						break;
					}
				}
			}
			else
			{
				group.GG_Code = initialCode;
			}
		}

		static bool GroupCodeExists(BusinessObjectFactory factory, string code)
		{
			return factory.ExistsInDatabase(GlbGroupSchema.Constants.TableName, new ZQuery(GlbGroupSchema.GG_Code, code));
		}

		[ThreadSafe]
		static readonly ConcurrentDictionary<ZGuid, SemaphoreSlim> groupLocks = new ConcurrentDictionary<ZGuid, SemaphoreSlim>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		static void SetMembers(GlbGroup group, ScimGroup scimGroup)
		{
			var groupLock = groupLocks.GetOrAdd(group.PK, _ => new SemaphoreSlim(1, 1));
			var canProceed = false;

			try
			{
				canProceed = groupLock.WaitAsync(5000).GetAwaiter().GetResult();
			}
			catch (ObjectDisposedException) 
			{
				throw new SCIMNotFoundException("Semaphore has been disposed. Please try again later.");
			}

			if (!canProceed)
			{
				throw new SCIMNotFoundException("Could not obtain lock. Please try again later.");
			}

			try
			{
				RemoveMissingStaffFromGroup(scimGroup, group);

				if (scimGroup.Members == null || !scimGroup.Members.Any())
				{
					return;
				}
				var staffMembers = scimGroup.Members.Where(m => m.Type == SCIMResourceTypes.User);

				var notFoundError = "Group member not found: {0}";

				int staffCount = 0;

				if (staffMembers.Any())
				{
					var existingStaffPKs = group.Staff.Select(s => s.PK).ToArray();
					var newPKs = staffMembers.Select(m => new ZGuid(m.Value)).Where(pk => !existingStaffPKs.Contains(pk));

					staffCount = existingStaffPKs.Length;
					if (newPKs.Any())
					{
						var newStaffs = group.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, newPKs)).ToArray();

						staffCount += newStaffs.Length;
						if (staffCount < staffMembers.Count())
						{
							throw new SCIMNoTargetException(string.Format(notFoundError, SCIMResourceTypes.User));
						}

						AddStaffs(group, newStaffs);
					}
				}

				var groupMembers = scimGroup.Members.Where(m => m.Type == SCIMResourceTypes.Group);
				int groupCount = 0;
				if (groupMembers.Any())
				{
					var groups = group.Factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.PK, groupMembers.Select(m => new ZGuid(m.Value)).ToArray()));

					groupCount = groups.Length;
					if (groups.Length < groupMembers.Count())
					{
						throw new SCIMNoTargetException(string.Format(notFoundError, nameof(SCIMResourceTypes.Group)));
					}

					AddSubGroups(group, groups);
				}

				var unknownMembers = scimGroup.Members.Where(m => m.Type != SCIMResourceTypes.User && m.Type != SCIMResourceTypes.Group);
				if (unknownMembers.Any())
				{
					var staffs = group.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, unknownMembers.Select(m => new ZGuid(m.Value)).ToArray()));
					var groups = group.Factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.PK, unknownMembers.Select(m => new ZGuid(m.Value)).ToArray()));

					if (staffCount + groupCount + staffs.Length + groups.Length < scimGroup.Members.Count())
					{
						throw new SCIMNoTargetException(string.Format(notFoundError, "Unknown"));
					}

					AddStaffs(group, staffs);
					AddSubGroups(group, groups);
				}
			}
			finally
			{
				groupLock.Release();
			}
		}

		static void RemoveMissingStaffFromGroup(ScimGroup scimGroup, GlbGroup group)
		{
			var staffMembers = scimGroup.Members.Where(m => m.Type != SCIMResourceTypes.Group);
			var removeList = new List<ZGuid>();
			foreach (GlbStaff staff in group.Staff)
			{
				if (!staffMembers.Any(u => new ZGuid(u.Value) == staff.PK))
				{
					RemoveStaffFlagIfRequired(group, staff);
					removeList.Add(staff.PK);
				}
			}

			foreach (var pk in removeList)
			{
				group.Staff.Remove(pk);
			}
		}

		public static void RemoveStaffFlagsIfRequired(this GlbGroup group)
		{
			foreach (GlbStaff staff in group.Staff)
			{
				RemoveStaffFlagIfRequired(group, staff);
			}
		}

		static void RemoveStaffFlagIfRequired(GlbGroup group, GlbStaff staff)
		{
			SetStaffColumnValue(group, staff, false);
		}

		static void AddSubGroups(GlbGroup group, GlbGroup[] groups)
		{
			foreach (var childGroup in groups)
			{
				if (childGroup.GG_GG_ParentGroup == group.PK)
				{
					continue;
				}

				if (childGroup.PK == group.PK)
				{
					throw new SCIMNoTargetException($"Group [{childGroup.PK}] cannot be its own parent group.");
				}

				if (!childGroup.GG_GG_ParentGroup.IsEmpty)
				{
					throw new SCIMNoTargetException($"Group [{childGroup.PK}] already has a parent group.");
				}

				childGroup.GG_GG_ParentGroup = group.PK;
				childGroup.GG_IsSecurityEnabled = false;
			}
		}

		static void AddStaffs(GlbGroup group, GlbStaff[] staffs)
		{
			foreach (var staff in staffs)
			{
				AddStaffFlagsIfRequired(group, staff);

				if (group.Staff.Contains(staff.PK))
				{
					continue;
				}

				group.Staff.Add(staff);
			}
		}

		static void AddStaffFlagsIfRequired(GlbGroup group, GlbStaff staff)
		{
			SetStaffColumnValue(group, staff, true);
		}

		internal static void SetStaffColumnValue(GlbGroup group, GlbStaff staff, bool value)
		{
			var staffColumnToGroupDescriptionScimMapping = StaffColumnToGroupDescriptionScimMappingCollection.Cast<StaffColumnToGroupDescriptionScimMapping>().FirstOrDefault(x => x.GroupDescriptionMapping.EqualsIgnoringCase(group.GG_Desc));

			if (StaffColumnToGroupDescriptionScimMappingCollection.Count > 0 && staffColumnToGroupDescriptionScimMapping != null)
			{
				staff[staffColumnToGroupDescriptionScimMapping.StaffColumnName] = value;
			}
		}

		static StaffColumnToGroupDescriptionScimMappingCollection StaffColumnToGroupDescriptionScimMappingCollection => SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.Value;

		static void SetGroupValue(GlbGroup group, string scimColumn, object value)
		{
			var column = GroupColumnHelper.GetGroupColumn(scimColumn);
			SetColumn(group, scimColumn, value, column);
		}

		static ScimGroup ToScimGroup(GlbGroup group, IEnumerable<SCIMAttributeExpression> included = null, IEnumerable<SCIMAttributeExpression> excluded = null)
		{
			if (group == null)
			{
				return null;
			}

			var scimGroup = new ScimGroup();
			scimGroup.Id = group.PK.ToGuid();

			scimGroup.DisplayName = group.GG_Desc;

			if (ShouldIncludeSimple(AttributeNames.ExternalId, included, excluded))
			{
				scimGroup.ExternalId = group.GG_ExternalId;
			}

			if (ShouldIncludeSimple(AttributeNames.Category, included, excluded))
			{
				scimGroup.Category = group.GG_Category;
			}

			if (ShouldIncludeSimple(AttributeNames.Members, included, excluded))
			{
				var childGroups = group.Factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.GG_GG_ParentGroup, group.PK));

				scimGroup.Members = group.Staff.Cast<GlbStaff>()
					.Select(s => new GroupMember()
					{
						Value = s.PK.ToString(),
						Type = SCIMResourceTypes.User,
						Ref = RefUsersPrefix + s.PK
					})
					.Union(childGroups.Select(g => new GroupMember()
					{
						Value = g.PK.ToString(),
						Type = SCIMResourceTypes.Group,
						Ref = RefGroupsPrefix + g.PK
					}));
			}

			return scimGroup;
		}

		#endregion

		#region Common

		public static void TrySave(this BusinessObject bizO)
		{
			try
			{
				bizO.Factory.Save();
			}
			catch (ZSaveException ex)
			{
				if (!ex.IndexNameIfUniqueIndexViolation.IsEmpty)
				{
					throw new SCIMUniquenessAttributeException($"The request violates unique index: {ex.IndexNameIfUniqueIndexViolation}");
				}

				ErrorReporter.ReportOnce("SCIM Error Saving Record", ex);

				throw new SCIMNoTargetException($"Unable to save an updated record. Error: {ex.Message}");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public static void UpdateFromScim(this BusinessObject bizO, ScimBase scim, FastStaffCodeCalculator codeCalculator = null)
		{
			string error = "Parameter type mismatch";

			var staff = bizO as GlbStaff;
			if (staff != null)
			{
				var scimUser = scim as ScimUser
					?? throw new ArgumentException(error, nameof(scim));

				UpdateFromScimUser(staff, scimUser, codeCalculator);
				return;
			}
			else
			{
				var group = bizO as GlbGroup
					?? throw new ArgumentException(error, nameof(bizO));

				var scimGroup = scim as ScimGroup
					?? throw new ArgumentException(error, nameof(scim));

				UpdateFromScimGroup(group, scimGroup);
				return;
			}
		}

		public static ScimBase ToScim(this BusinessObject source, IEnumerable<SCIMAttributeExpression> included = null, IEnumerable<SCIMAttributeExpression> excluded = null)
		{
			if (source == null)
			{
				return null;
			}

			var sourceType = source.GetType();
			if (typeof(GlbStaff).IsAssignableFrom(sourceType))
			{
				return ToScimUser(source as GlbStaff, included, excluded);
			}
			else if (typeof(GlbGroup).IsAssignableFrom(sourceType))
			{
				return ToScimGroup(source as GlbGroup, included, excluded);
			}
			else
			{
				throw new SCIMSchemaNotFoundException();
			}
		}

		static bool ShouldIncludeSimple(string name, IEnumerable<SCIMAttributeExpression> included, IEnumerable<SCIMAttributeExpression> excluded)
		{
			if (included == null || !included.Any())
			{
				if (excluded == null)
				{
					return true;
				}

				return !excluded.Any(i => i.Name.ToLower().Equals(name.ToLower()));
			}

			if (included.Any(i => i.Name.ToLower().Equals(name.ToLower())))
			{
				return true;
			}

			return false;
		}

		static void SetColumn(BusinessObject bizO, string scimColumn, object value, SchemaColumn column, string trimStart = "")
		{
			if (column != null)
			{
				if (value == null)
				{
					if (column is SchemaStringColumn)
					{
						bizO[column] = ZString.Empty;
					}
					else if (column is SchemaBoolColumn)
					{
						bizO[column] = ZBool.False;
					}
					else if (column is SchemaGuidColumn)
					{
						bizO[column] = ZGuid.Empty;
					}
					else if (column is SchemaIntColumn)
					{
						bizO[column] = 0;
					}
				}
				else
				{
					if (string.IsNullOrEmpty(trimStart))
					{
						bizO[column] = value;
					}
					else
					{
						var strValue = value as string;
						if (!string.IsNullOrEmpty(strValue) && strValue.StartsWith(trimStart))
						{
							strValue = strValue.Substring(trimStart.Length);
						}

						bizO[column] = strValue;
					}
				}
			}
			else
			{
				throw new SCIMSchemaViolatedException($"Unable to map column [{scimColumn}]");
			}
		}

		#endregion
	}
}
