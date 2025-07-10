using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddADLinkedFilterIfRequired(filters);
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddRelatedItemFilters(filters);
			AddFlagsFilters(filters);
			AddCustomFilters(filters);
			AddMiscFilters(filters);
			AddDomainNameFilter(filters);

			return filters;
		}

		internal ModuleFilterCollection TestGetModuleFiltersCoreInternal()
		{
			return GetModuleFiltersCore();
		}

		#region System

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(new ZQuery(GlbStaffSchema.GS_IsSystemAccount, ZBool.False).AddToFilter(JoinCondition.Or, GlbStaffSchema.GS_Code, User.SupportUserCode));

				return query;
			}
		}

		const string SystemAccountFieldName = "ISSYSTEMACCOUNT";
		const string StaffCodeFieldName = "CODE";

		public override IGlowQuery AdditionalIndexSearchQuery => new BooleanQuery(BooleanOperator.Or, new EqualQuery(new Term(SystemAccountFieldName, $"false"), useQuotes: false), new EqualQuery(new Term(StaffCodeFieldName, User.SupportUserCode)));

		#endregion

		#region ADLinked

		void AddADLinkedFilterIfRequired(ModuleFilterCollection filters)
		{
			if (ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled)
			{
				filters.AddFlagsFilter("ADLinked", new[] { Res.GetString("c99bee1b-3d14-48ac-860f-af07b99345a7", "Linked") }, new GetFlagsQuery[] { GetADLinkedQuery }).MultilingualDescription = ResString.GetMultilingualString("4db168e1-0a09-4243-a4f1-4285ae0bd776", "Linked with Active Directory");
			}
		}

		ZQuery GetADLinkedQuery(ZBool value)
		{
			var query = new ZQuery();
			if (!value)
			{
				query.AddToFilter(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.Equal, null)
					.AddToFilter(JoinCondition.Or, GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.Equal, ZGuid.Invalid);
			}
			else
			{
				query.AddToFilter(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null)
					.AddToFilter(JoinCondition.And, GlbStaffSchema.GS_ActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, ZGuid.Invalid);
			}
			return query;
		}

		#endregion

		#region DomainName

		void AddDomainNameFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(GlbStaffSchema.Constants.GS_DomainName, GlbStaffSchema.GS_DomainName, ObjectFactory.Get<IADRegistry>().DomainCredentialsCollectionAsCodeDescriptionPairList);
			filter.MultilingualDescription = ResString.GetMultilingualString("88958706-4735-433E-988D-06023C853ACC", "Domain Name");
			filter.ShowDescription = false;
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var codeOrFullNameFilter = filters.AddTextFilterForMultipleColumns((NoResString)"Code or Full Name", GlbStaffSchema.GS_Code, GlbStaffSchema.GS_FullName);
			codeOrFullNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|CodeOrFullName", "Code or Full Name");
			codeOrFullNameFilter.MaxLength = GlbStaffSchema.GS_Code.MaxLength >= GlbStaffSchema.GS_FullName.MaxLength ? GlbStaffSchema.GS_Code.MaxLength : GlbStaffSchema.GS_FullName.MaxLength;
			filters.AddTextFilter("Login Name", GlbStaffSchema.GS_LoginName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|LoginName", "Login Name");
			filters.AddTextFilter("Full Name", GlbStaffSchema.GS_FullName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|FullName", "Full Name");

			var column = new SchemaStringColumn(GlbPersonSchema.Instance, "PER_FullNameAI", -1, SqlDbType.NVarChar, "", false, 256, false, false, "");
			var filter = filters.AddTextFilter("Full Name (Accents Excluded)", column);
			var subGroup = new GlbStaffFilterProvider.GlbPersonSubGroup();
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|FullNameAccentsExcluded", "Full Name (Accents Excluded)");
			filter.SubGroup = subGroup;

			filters.AddTextFilter("Code", GlbStaffSchema.GS_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|Code", "Code");
			if (Env.Security.StaffViewOtherMobilePhone.IsAllowed)
			{
				filters.AddTextFilter("Mobile Phone", GlbStaffSchema.GS_MobilePhone).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|MobilePhone", "Mobile Phone");
			}
			if (Env.Security.StaffViewOtherWorkExtension.IsAllowed)
			{
				filters.AddTextFilter("Work Extension", GlbStaffSchema.GS_WorkExtension).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|WorkExtension", "Work Extension");
			}
			filters.AddTextFilter("Fax Number", GlbStaffSchema.GS_FaxNum).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|FaxNumber", "Fax Number");
			filters.AddTextFilter("Email Address", GlbStaffSchema.GS_EmailAddress).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|EmailAddress", "Email Address");
			if (Env.Security.StaffViewHomeAddressDetails.IsAllowed)
			{
				filters.AddTextFilter("Home Phone Number", GlbStaffSchema.GS_HomePhone).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|HomePhoneNumber", "Home Phone Number");
			}
			filters.AddTextFilter("Language", GlbStaffSchema.GS_WorkingLanguage, () => new CodeDescriptionPairList(OLookUpEditType.Language)).MultilingualDescription = ResString.GetMultilingualString("54f12dd5-60d7-4874-8b9d-9eeba874db41", "Language");

			if (Env.Security.StaffViewOtherGender.IsAllowed)
			{
				filters.AddTextFilter("Gender", GlbStaffSchema.GS_Gender, () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.Genders.Woman, Core.Constants.GenderDescriptions.Woman);
					list.AddPair(Core.Constants.Genders.Woman, Core.Constants.GenderDescriptions.Man);
					list.AddPair(Core.Constants.Genders.Agender, Core.Constants.GenderDescriptions.Agender);
					list.AddPair(Core.Constants.Genders.NonBinary, Core.Constants.GenderDescriptions.NonBinary);
					list.AddPair(Core.Constants.Genders.NotSpecified, Core.Constants.GenderDescriptions.NotSpecified);
					list.AddPair(Core.Constants.Genders.Custom, Core.Constants.GenderDescriptions.Custom);
					return list;
				}).MultilingualDescription = ResString.GetMultilingualString("50803CA4-04E6-4862-940E-439B9ED1AFA2", "Gender");
			}
			filters.AddTextFilter("Title", GlbStaffSchema.GS_Title).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|JobTitle", "Title");

			if (Env.Security.StaffViewOtherReferences.IsAllowed)
			{
				filters.AddTextFilter("Other References", GlbStaffSchema.GS_Pager).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|OtherReferences", "Other References");
			}
			filter = filters.AddTextFilter(StaffDriverCollection.DriverBranch, StaffFilterProvider.GetDataBranchFilter, () => new CodeDescriptionPairList(StaffFilterProvider.BranchesList));
			filter.Property = Env.CurrentBranch.Code;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|DriverBranch", StaffDriverCollection.DriverBranch);

			var idpIdFilter = filters.AddTextFilter("Identity Provider Id", GetIdpIdQuery);
			idpIdFilter.MultilingualDescription = ResString.GetMultilingualString("C3C94B66-9232-43E0-83C5-AD5E5166FECB", "Identity Provider Id");
			idpIdFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			idpIdFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			idpIdFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			idpIdFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			filters.AddTextFilter("External Id", GlbStaffSchema.GS_ExternalId).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|ExternalId", "External Id");
		}

		ZQuery GetIdpIdQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				return GetIdpIdQueryCore(comparisonOperator, DBNull.Value);
			}
			else if (ZGuid.TryParse(value, out var guidValue))
			{
				return GetIdpIdQueryCore(comparisonOperator, guidValue);
			}

			return ZQuery.NoResultQuery;
		}

		static ZDBOnlyQuery GetIdpIdQueryCore(SQLComparisonOperator comparisonOperator, object filterValue)
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));
			var personQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbStaffSchema.GS_PER);
			personQuery.AddToFilter(GlbPersonSchema.PER_IDPUserId, comparisonOperator, filterValue);

			query.AddSubQuery(personQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			if (Env.Security.StaffViewBirthDate.IsAllowed)
			{
				var birthdayFilter = filters.AddDateFilter("Birth Date", GlbStaffSchema.GS_Birthdate);
				birthdayFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|BirthDate", "Birth Date");
			}

			if (Env.Security.StaffViewOtherEmploymentHistory.IsAllowed)
			{
				var employmentFilter = filters.AddDateFilter("Employment Date", GlbStaffSchema.GS_EmploymentDate);
				employmentFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GldStaffFilter|EmployedDate", "Employment Date");

				var departureFilter = filters.AddDateFilter("Departure Date", GlbStaffSchema.GS_DepartureDate);
				departureFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|DepartureDate", "Departure Date");
			}

			filters.AddDateFilter("Next Review Date", GlbStaffSchema.GS_NextReviewDate).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|NextReviewDate", "Next Review Date");
			filters.AddDateFilter("Not In The Workplace", StaffFilterProvider.GetHolidayQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|NotInTheWorkplace", "Not In The Workplace");

			if (Env.Security.StaffViewBirthDate.IsAllowed)
			{
				var birthMonthFilter = filters.AddTextFilter("Birthday In", StaffFilterProvider.GetBirthdayInMonthQuery, StaffFilterProvider.BirthdayMonthList);
				birthMonthFilter.Category = FilterCategories.Dates;
				birthMonthFilter.DefaultProperty = GlbStaffFilterProvider.AnyMonth.GetUnresolvedString();
				birthMonthFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|BirthdayIn", "Birthday In");
				birthMonthFilter.ErrorOnCodeNotPresent = true;
			}

			var dateFilter = filters.AddDateFilter("Security Modified", StaffFilterProvider.GetSecurityModifiedFilter);
			dateFilter.HideFutureDates = true;
			dateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|SecurityModified", "Security Modified");

			if (Env.Security.StaffViewOtherCertificates.IsAllowed)
			{
				var certificateSubGroup = new GlbStaffFilterProvider.CertificateSubGroup();

				var certificateIssueDateFilter = filters.AddDateFilter("Certificate Issue Date", GenRegCertAccredMaintListSchema.XZ_IssueDate);
				certificateIssueDateFilter.MultilingualDescription = ResString.GetMultilingualString("6959C74F-457D-4FC5-978E-3DED180B3829", "Certificate Issue Date");
				certificateIssueDateFilter.SubGroup = certificateSubGroup;

				var certificateExpiryDateFilter = filters.AddDateFilter("Certificate Expiry Date", GenRegCertAccredMaintListSchema.XZ_ExpiryOrDueDate);
				certificateExpiryDateFilter.MultilingualDescription = ResString.GetMultilingualString("05626473-BCAE-4E04-9B20-F5B56317B2C6", "Certificate Expiry Date");
				certificateExpiryDateFilter.SubGroup = certificateSubGroup;
			}

			var loginFilter = filters.AddDateFilter("Last Login Date", GlbStaffSchema.GS_LastActivityDate, true);
			loginFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|LastLoginDate", "Last Login Date");
			loginFilter.UserEntersUtcValue = false;

			var logOutEvenFilter = filters.AddDateFilter("Logout Time", GetLogOutCollectionDateQuery, convertFromLocalToUTC: true);
			logOutEvenFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|logOutEvenFilter", "Logout Time");
			logOutEvenFilter.UserEntersUtcValue = false;

			var logInEvenFilter = filters.AddDateFilter("Login Time", GetLogInCollectionDateQuery, convertFromLocalToUTC: true);
			logInEvenFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|logInEventFilter", "Login Time");
			logInEvenFilter.UserEntersUtcValue = false;

			if (Env.Security.StaffViewOtherLeave.IsAllowed)
			{
				filters.AddCustomFilter(new LeaveDateRangeWithTypeFilter("Leave Type and Date") { MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|LeaveInTheRange", "Leave Type and Date") });
			}
		}

		ZQuery GetLogOutCollectionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(GlbStaff));

			if (!date1.IsEmpty && !date2.IsEmpty)
			{
				var logsSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_GS_NKUser);
				logsSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "LGO");
				AddDateTimeRange(logsSubQuery, comparisonOperator, JoinCondition.And, StmALogSchema.SL_PostedTimeUtc, date1, date2);

				result.AddSubQuery(GlbStaffSchema.GS_Code, logsSubQuery, JoinCondition.And);
			}
			return result;
		}

		ZQuery GetLogInCollectionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(GlbStaff));

			if (!date1.IsEmpty && !date2.IsEmpty)
			{
				var logsSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_GS_NKUser);
				logsSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, "LGI");
				AddDateTimeRange(logsSubQuery, comparisonOperator, JoinCondition.And, StmALogSchema.SL_PostedTimeUtc, date1, date2);

				result.AddSubQuery(GlbStaffSchema.GS_Code, logsSubQuery, JoinCondition.And);
			}
			return result;
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Home Branch", ModuleIDs.GlbBranch, GlbStaffSchema.GS_GB_HomeBranch, StaffFilterProvider.Branches).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|HomeBranch", "Home Branch");
			filters.AddGuidFilter("Home Department", ModuleIDs.GlbDepartment, GlbStaffSchema.GS_GE_HomeDepartment, StaffFilterProvider.Departments).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|HomeDepartment", "Home Department");

			filters.AddGuidFilter("Group Membership", ModuleIDs.GlbGroup, GetGroupMembershipQuery, Groups).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|GroupMembership", "Group Membership");

			var capabilityFilter = filters.AddGuidFilter("Capability", ModuleIDs.GlbCapability, GlbCapabilitySchema.PK, StaffFilterProvider.Capabilities);
			capabilityFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|Capability", "Capability");
			capabilityFilter.SubGroup = new GlbStaffFilterProvider.CapabilitySubGroup();

			filters.AddNkFilter("Nationality", GlbStaffSchema.GS_RN_NKNationalityCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("45A29C15-9262-4346-9FD4-5D995F4644FD", "Nationality");

			var reportingManagerFilter = new StaffReportingManagerRoleModuleFilter("Reporting Manager");
			reportingManagerFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			reportingManagerFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|ReportingManager", "Reporting Manager");
			filters.AddCustomFilter(reportingManagerFilter);
		}

		ZQuery GetGroupMembershipQuery(ZGuid groupPK)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbStaff));
			if (!groupPK.IsEmpty)
			{
				ZDBOnlySubQuery groupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
				groupSubQuery.AddToFilter(GlbGroupSchema.PK, groupPK);

				ZDBOnlySubQuery groupLinkQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
				groupLinkQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, groupSubQuery, JoinCondition.And);

				query.AddSubQuery(groupLinkQuery, JoinCondition.And);
			}
			return query;
		}

		public GlbGroupCollection Groups
		{
			get
			{
				if (fGroups == null)
				{
					var lGroups = GetNewGroups();
					lGroups.Load();
					fGroups = lGroups;
				}

				return fGroups;
			}
		}

		protected virtual GlbGroupCollection GetNewGroups()
		{
			return new GlbGroupCollection(Factory);
		}

		GlbGroupCollection fGroups;

		#endregion

		#region Flags

		protected virtual void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var sysAdminFilter = filters.AddTextFilter("Sys Admin Status", GetSysAdminFilter, SysAdminStatusList);
			sysAdminFilter.Category = FilterCategories.StatusAndFlags;
			sysAdminFilter.DefaultProperty = OrgConstants.FilterControl.SysAdminStatus.Code.AllStaff;
			sysAdminFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|SysAdminStatus", "Sys Admin Status");

			var nonOperationalFilter = filters.AddTextFilter("Operational Status", GetNonOperationalStaffFilter, OperationalStatusList);
			nonOperationalFilter.Category = FilterCategories.StatusAndFlags;
			nonOperationalFilter.DefaultProperty = OrgConstants.FilterControl.OperationalStatus.Code.AllStaff;
			nonOperationalFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|OperationalStatus", "Operational Status");

			var salesRepFilter = filters.AddTextFilter("Sales Rep", StaffFilterProvider.GetSalesRepFilter, StaffFilterProvider.SalesRepStatusList);
			salesRepFilter.Category = FilterCategories.StatusAndFlags;
			salesRepFilter.DefaultProperty = OrgConstants.FilterControl.SalesRepStatus.Code.AllStaff;
			salesRepFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|SalesRep", "Sales Rep");

			var backupOperatorFilter = filters.AddTextFilter("Backup Operator", GetBackupOperatorFilter, BackupOperatorStatusList);
			backupOperatorFilter.Category = FilterCategories.StatusAndFlags;
			backupOperatorFilter.DefaultProperty = OrgConstants.FilterControl.BackupOperatorStatus.Code.AllStaff;
			backupOperatorFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|BackupOperator", "Backup Operator");

			var dataReaderFilter = filters.AddTextFilter("Database Reader", GetDataReaderFilter, DatabaseReaderStatusList);
			dataReaderFilter.Category = FilterCategories.StatusAndFlags;
			dataReaderFilter.DefaultProperty = OrgConstants.FilterControl.DatabaseReaderStatus.Code.AllStaff;
			dataReaderFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|DatabaseReader", "Database Reader");

			var dataDeveloperFilter = filters.AddTextFilter("Database Developer", GetDataDeveloperFilter, DatabaseDeveloperStatusList);
			dataDeveloperFilter.Category = FilterCategories.StatusAndFlags;
			dataDeveloperFilter.DefaultProperty = OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.AllStaff;
			dataDeveloperFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|DatabaseDeveloper", "Database Developer");

			var isDeviceOnlyFilter = filters.AddTextFilter("Is Device Only", GetIsDeviceOnlyFilter, IsDeviceOnlyStatusList);
			isDeviceOnlyFilter.Category = FilterCategories.StatusAndFlags;
			isDeviceOnlyFilter.DefaultProperty = OrgConstants.FilterControl.IsDeviceOnlyStatus.Code.AllStaff;
			isDeviceOnlyFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|IsDeviceOnly", "Is Device Only");

			var isDriverFilter = filters.AddTextFilter("Driver", StaffFilterProvider.GetDriverFilter, DriverStatusList);
			isDriverFilter.Category = FilterCategories.StatusAndFlags;
			isDriverFilter.DefaultProperty = OrgConstants.FilterControl.DriverStatus.Code.AllStaff;
			isDriverFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|Driver", "Driver");

			var resourceStaffFlagsFilter = filters.AddFlagsFilter("Resources / Staff", new string[] { Res.GetString("MasterFiles|GlbStaffFilter|IncludeResources", "Include Resources"), Res.GetString("MasterFiles|GlbStaffFilter|IncludeStaff", "Include Staff") }, new GetFlagsQuery[] { GetIncludeResourcesQuery, GetIncludeStaffQuery });
			resourceStaffFlagsFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|ResourcesStaff", "Resources / Staff");
			resourceStaffFlagsFilter.DefaultProperties[Res.GetString("MasterFiles|GlbStaffFilter|IncludeResources", "Include Resources")] = true;
			resourceStaffFlagsFilter.DefaultProperties[Res.GetString("MasterFiles|GlbStaffFilter|IncludeStaff", "Include Staff")] = true;

			var isCanLoginUserFlagFilter = filters.AddFlagsFilter("Can Login", new string[] { Res.GetString("MasterFiles|GlbStaffFilter|CanLogin", "Can Login") }, new GetFlagsQuery[] { GetIsCanLoginQuery, });
			isCanLoginUserFlagFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|CanLogin", "Can Login");
			isCanLoginUserFlagFilter.DefaultProperties[Res.GetString("MasterFiles|GlbStaffFilter|CanLogin", "Can Login")] = true;

			var isCurrentUserFilter = filters.AddTextFilter("Is Current User", GetIsCurrentUserQuery, IsCurrentUserOptionsList);
			isCurrentUserFilter.Category = FilterCategories.StatusAndFlags;
			isCurrentUserFilter.DefaultProperty = IsCurrentUserOptionsListCodes.CurrentUser;
			isCurrentUserFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|Is Current User", "Is Current User");
		}

		ZQuery GetIsCanLoginQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (!value)
			{
				query.AddToFilter(GlbStaffSchema.GS_CanLogin, ZBool.False);
			}
			else
			{
				query.AddToFilter(GlbStaffSchema.GS_CanLogin, ZBool.True);
			}
			return query;
		}

		ZQuery GetIncludeResourcesQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (!value)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsResource, ZBool.False);
			}
			return query;
		}

		ZQuery GetIncludeStaffQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (!value)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsResource, ZBool.True);
			}
			return query;
		}

		ZQuery GetSysAdminFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == OrgConstants.FilterControl.SysAdminStatus.Code.SysAdminStaff)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsController, ZBool.True);
			}
			else if (value == OrgConstants.FilterControl.SysAdminStatus.Code.NonSysAdminStaff)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsController, ZBool.False);
			}

			return query;
		}

		ZQuery GetNonOperationalStaffFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == OrgConstants.FilterControl.OperationalStatus.Code.NonOperationalStaff)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsOperational, ZBool.False);
			}
			else if (value == OrgConstants.FilterControl.OperationalStatus.Code.OperationalStaff)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsOperational, ZBool.True);
			}

			return query;
		}

		ZQuery GetBackupOperatorFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(IGlbStaff));

			if (value == OrgConstants.FilterControl.BackupOperatorStatus.Code.BackupOperatorStaff ||
				value == OrgConstants.FilterControl.BackupOperatorStatus.Code.NonBackupOperatorStaff)
			{
				var glbGroupRoleSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupRole), GlbGroupLinkSchema.GK_GG);
				glbGroupRoleSubQuery.AddToFilter(GlbGroupRoleSchema.GGR_RoleName, SQLComparisonOperator.Equal, DbRoleTypes.DbBackupOperatorRole);

				var glbGroupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS, value == OrgConstants.FilterControl.DatabaseReaderStatus.Code.NonDatabaseReaderStaff);
				glbGroupLinkSubQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, GlbGroupRoleSchema.GGR_GG_Group, glbGroupRoleSubQuery, JoinCondition.And);

				query.AddSubQuery(GlbStaffSchema.PK, GlbGroupLinkSchema.GK_GS, glbGroupLinkSubQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetDataReaderFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(IGlbStaff));

			if (value == OrgConstants.FilterControl.DatabaseReaderStatus.Code.DatabaseReaderStaff ||
				value == OrgConstants.FilterControl.DatabaseReaderStatus.Code.NonDatabaseReaderStaff)
			{
				var glbGroupRoleSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupRole), GlbGroupLinkSchema.GK_GG);
				glbGroupRoleSubQuery.AddToFilter(GlbGroupRoleSchema.GGR_RoleName, SQLComparisonOperator.Equal, DbRoleTypes.CwRestrictedReaderRole);

				var glbGroupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS, value == OrgConstants.FilterControl.DatabaseReaderStatus.Code.NonDatabaseReaderStaff);
				glbGroupLinkSubQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, GlbGroupRoleSchema.GGR_GG_Group, glbGroupRoleSubQuery, JoinCondition.And);

				query.AddSubQuery(GlbStaffSchema.PK, GlbGroupLinkSchema.GK_GS, glbGroupLinkSubQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetDataDeveloperFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(IGlbStaff));

			if (value == OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.DatabaseDeveloperStaff ||
				value == OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.NonDatabaseDeveloperStaff)
			{
				var glbGroupRoleSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupRole), GlbGroupLinkSchema.GK_GG);
				glbGroupRoleSubQuery.AddToFilter(GlbGroupRoleSchema.GGR_RoleName, SQLComparisonOperator.Equal, DbRoleTypes.DbDataWriterRole);

				var glbGroupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS, value == OrgConstants.FilterControl.DatabaseReaderStatus.Code.NonDatabaseReaderStaff);
				glbGroupLinkSubQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, GlbGroupRoleSchema.GGR_GG_Group, glbGroupRoleSubQuery, JoinCondition.And);

				query.AddSubQuery(GlbStaffSchema.PK, GlbGroupLinkSchema.GK_GS, glbGroupLinkSubQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetIsDeviceOnlyFilter(ZString value)
		{
			var query = new ZQuery();
			if (value == OrgConstants.FilterControl.IsDeviceOnlyStatus.Code.DeviceOnlyStaff)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsDevice, ZBool.True);
			}
			else if (value == OrgConstants.FilterControl.IsDeviceOnlyStatus.Code.NonDeviceOnlyStaff)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsDevice, ZBool.False);
			}

			return query;
		}

		static ZQuery GetIsCurrentUserQuery(ZString value)
		{
			var query = new ZQuery();

			if (value == IsCurrentUserOptionsListCodes.CurrentUser || value == IsCurrentUserOptionsListCodes.NotCurrentUser)
			{
				var currentUserCode = EnvProxy.Instance.CurrentUser.Initials;
				var comparisonOperator = value == IsCurrentUserOptionsListCodes.CurrentUser ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
				query.AddToFilter(GlbStaffSchema.GS_Code, comparisonOperator, currentUserCode);
			}

			return query;
		}

		#endregion

		#region Custom

		void AddCustomFilters(ModuleFilterCollection filters)
		{
			var securityModuleFilter = new StaffSecurityModuleFilter("Security Rights", StaffFilterProvider.Branches, StaffFilterProvider.Departments);
			securityModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|SecurityRights", "Security Rights");
			filters.AddCustomFilter(securityModuleFilter);
		}

		internal StaffSecurityModuleFilter SecurityRightsFilter
		{
			get { return (StaffSecurityModuleFilter)this["Security Rights"]; }
		}

		protected virtual bool ShouldAddWorkflowCustomFieldsFilters
		{
			get { return true; }
		}

		#endregion

		#region Misc Filters

		void AddMiscFilters(ModuleFilterCollection filters)
		{
			var employmentBasisFilter = filters.AddTextFilter("Employment Basis", GlbStaffSchema.GS_EmploymentBasis, SystemDataRegistry.Instance.StaffEmploymentTypes.Value);
			employmentBasisFilter.Category = FilterCategories.Other;
			employmentBasisFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbStaffFilter|EmploymentBasis", "Employment Basis");
		}

		#endregion

		#region Workflow Filters

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var workflowHelper = new WorkflowFilterStripsHelper(typeof(GlbStaff), WorkflowDescriptors.GlbStaffDescriptorCode, Factory);

			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(ShouldAddWorkflowCustomFieldsFilters);
			helpers.Add(workflowHelper);

			return helpers;
		}

		#endregion

		#endregion

		#region Lookups

		#region Backup Operator List

		public CodeDescriptionPairList BackupOperatorStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.BackupOperatorStatus.Code.AllStaff, Res.GetString("MasterFiles|GlbStaffFilter|BackupOperator|AllStaff", "All Backup Operator and Non Backup Operator"));
				list.AddPair(OrgConstants.FilterControl.BackupOperatorStatus.Code.BackupOperatorStaff, Res.GetString("MasterFiles|GlbStaffFilter|BackupOperator|BackupOperatorStaff", "Backup Operator Only"));
				list.AddPair(OrgConstants.FilterControl.BackupOperatorStatus.Code.NonBackupOperatorStaff, Res.GetString("MasterFiles|GlbStaffFilter|BackupOperator|NonBackupOperatorStaff", "Non-Backup Operator Only"));

				return list;
			}
		}

		#endregion

		#region DatabaseReaderStatus List

		public CodeDescriptionPairList DatabaseReaderStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.DatabaseReaderStatus.Code.AllStaff, Res.GetString("MasterFiles|GlbStaffFilter|DatabaseReader|AllStaff", "All Database Reader and Non Database Reader"));
				list.AddPair(OrgConstants.FilterControl.DatabaseReaderStatus.Code.DatabaseReaderStaff, Res.GetString("MasterFiles|GlbStaffFilter|DatabaseReader|DatabaseReaderStaff", "Database Reader Only"));
				list.AddPair(OrgConstants.FilterControl.DatabaseReaderStatus.Code.NonDatabaseReaderStaff, Res.GetString("MasterFiles|GlbStaffFilter|DatabaseReader|NonDatabaseReaderStaff", "Non-Database Reader Only"));

				return list;
			}
		}

		#endregion

		#region DatabaseDeveloperStatus List

		public CodeDescriptionPairList DatabaseDeveloperStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.AllStaff, Res.GetString("MasterFiles|GlbStaffFilter|DatabaseDeveloper|AllStaff", "All Database Developer and Non Database Developer"));
				list.AddPair(OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.DatabaseDeveloperStaff, Res.GetString("MasterFiles|GlbStaffFilter|DatabaseDeveloper|DatabaseReaderStaff", "Database Developer Only"));
				list.AddPair(OrgConstants.FilterControl.DatabaseDeveloperStatus.Code.NonDatabaseDeveloperStaff, Res.GetString("MasterFiles|GlbStaffFilter|DatabaseDeveloper|NonDatabaseReaderStaff", "Non-Database Developer Only"));

				return list;
			}
		}

		#endregion

		#region IsDeviceOnlyStatus List

		public CodeDescriptionPairList IsDeviceOnlyStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(OrgConstants.FilterControl.IsDeviceOnlyStatus.Code.AllStaff, Res.GetString("MasterFiles|GlbStaffFilter|IsDeviceOnly|AllStaff", "All staff"));
				list.AddPair(OrgConstants.FilterControl.IsDeviceOnlyStatus.Code.DeviceOnlyStaff, Res.GetString("MasterFiles|GlbStaffFilter|IsDeviceOnly|DeviceOnlyStaff", "Device Only staff"));
				list.AddPair(OrgConstants.FilterControl.IsDeviceOnlyStatus.Code.NonDeviceOnlyStaff, Res.GetString("MasterFiles|GlbStaffFilter|IsDeviceOnly|NonDeviceOnlyStaff", "Non-Device Only staff"));

				return list;
			}
		}

		#endregion

		#region DriverStatus List

		public CodeDescriptionPairList DriverStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(OrgConstants.FilterControl.DriverStatus.Code.AllStaff, Res.GetString("MasterFiles|GlbStaffFilter|Driver|AllStaff", "All staff"));
				list.AddPair(OrgConstants.FilterControl.DriverStatus.Code.Driver, Res.GetString("MasterFiles|GlbStaffFilter|Driver|Driver", "Driver"));
				list.AddPair(OrgConstants.FilterControl.DriverStatus.Code.NonDriver, Res.GetString("MasterFiles|GlbStaffFilter|Driver|NonDriver", "Non Driver"));

				return list;
			}
		}

		#endregion

		#region IsCurrentUserOptionsList

		public CodeDescriptionPairList IsCurrentUserOptionsList
		{
			get
			{
				return Factory.GetCachedValue(nameof(IsCurrentUserOptionsList), () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(IsCurrentUserOptionsListCodes.CurrentUser, Res.GetString("81b8f716-ff76-4b28-a9eb-f44fefab2a29", "Matches the currently logged-in user only."));
					list.AddPair(IsCurrentUserOptionsListCodes.NotCurrentUser, Res.GetString("1defe2a5-20b1-4da1-9ad6-9dd7b24f7676", "Matches all records that are not the currently logged-in user."));
					list.AddPair(IsCurrentUserOptionsListCodes.AllStaff, Res.GetString("d1f40442-44f1-4eae-801a-8ac75b7fa381", "Matches all records."));

					return list;
				});
			}
		}

		internal static class IsCurrentUserOptionsListCodes
		{
			public const string CurrentUser = "CUR";
			public const string NotCurrentUser = "NOT";
			public const string AllStaff = "ALL";
		}

		#endregion

		#region Sys Admin List

		public CodeDescriptionPairList SysAdminStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.SysAdminStatus.Code.AllStaff, Res.GetString("MasterFiles|GlbStaffFilter|SysAdminStatus|AllStaff", "All Admin and Non Admin"));
				list.AddPair(OrgConstants.FilterControl.SysAdminStatus.Code.SysAdminStaff, Res.GetString("MasterFiles|GlbStaffFilter|SysAdminStatus|SysAdminStaff", "Admin Only"));
				list.AddPair(OrgConstants.FilterControl.SysAdminStatus.Code.NonSysAdminStaff, Res.GetString("MasterFiles|GlbStaffFilter|SysAdminStatus|NonSysAdminStaff", "Non-Admin Only"));

				return list;
			}
		}

		#endregion

		#region Operational Status List

		public CodeDescriptionPairList OperationalStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.OperationalStatus.Code.AllStaff, Res.GetString("MasterFiles|GlbStaffFilter|OperationalStatus|AllStaff", "All Operational and Non-Operational"));
				list.AddPair(OrgConstants.FilterControl.OperationalStatus.Code.NonOperationalStaff, Res.GetString("MasterFiles|GlbStaffFilter|OperationalStatus|NonOperationalStaff", "Non-Operational Only"));
				list.AddPair(OrgConstants.FilterControl.OperationalStatus.Code.OperationalStaff, Res.GetString("MasterFiles|GlbStaffFilter|OperationalStatus|OperationalStaff", "Operational Only"));

				return list;
			}
		}

		#endregion

		#endregion

		#region Filter Providers

		internal GlbStaffFilterProvider StaffFilterProvider
		{
			get { return staffFilterProvider ?? (staffFilterProvider = new GlbStaffFilterProvider()); }
		}
		GlbStaffFilterProvider staffFilterProvider;

		#endregion
	}
}
