using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.GUI
{
	public class HRGlbCompanyCampaignContactFilterBusinessObject : GlbCompanyCampaignContactFilterBusinessObject
	{
		public HRGlbCompanyCampaignContactFilterBusinessObject(HRGlbCompanyCampaign campaign) : base(campaign)
		{
		}

		/// <summary>
		/// Parameterless constructor for color scheme and filter rule support.
		/// </summary>
		public HRGlbCompanyCampaignContactFilterBusinessObject() : base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DripMarketingFilterRuleHR;
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var hrCampaign = (HRGlbCompanyCampaign)Campaign;
			ModuleFilterCollection filters = new HRGlbCompanyCampaignModuleFilterCollection(hrCampaign);
			GetDataFromTableDeciderFilters(filters);
			AddDateFilters(filters);
			AddTextFilters(filters);
			AddStaffFilters(filters);
			AddJobApplicantFilters(filters);
			AddSubscribedRecipientsQuery(filters);
			AddCampaignTrackerFilters(filters);

			return filters;
		}

		#region Always Applied Filters

		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;

				var hrCampaign = (HRGlbCompanyCampaign)Campaign;
				if (hrCampaign != null && hrCampaign.IsUsingStaffDataSource)
				{
					var systemAccountQuery = new ZQuery();
					var staffSubGroup = new GlbStaffSubGroup();

					systemAccountQuery.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, ZBool.False);
					query = new ZQuery(query, staffSubGroup.GetSubQuery(systemAccountQuery));
				}

				return query;
			}
		}

		#endregion

		protected override void ResetCampaignModuleFilters()
		{
			var filters = ModuleFilters as HRGlbCompanyCampaignModuleFilterCollection;
			if (filters != null)
			{
				filters.ResetCampaignFilterList();
			}
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var contactNameFilter = filters.AddTextFilter(HRFilterDescription.ContactName, ViewCampaignContactSchema.VCC_ContactName);
			contactNameFilter.MultilingualDescription = ResString.GetMultilingualString("ba46fcdd-b3ec-4ae2-962d-d50fbef28fb0", HRFilterDescription.ContactName);
			contactNameFilter.Category = CampaignContactFilterCategories.CommonTypes;

			var emailAddressFilter = filters.AddTextFilter(HRFilterDescription.EmailAddress, ViewCampaignContactSchema.VCC_Email);
			emailAddressFilter.MaxLength = ViewCampaignContactSchema.VCC_Email.MaxLength;
			emailAddressFilter.MultilingualDescription = ResString.GetMultilingualString("8b6a8cc9-bd4e-497e-bd13-9b49ea049f05", HRFilterDescription.EmailAddress);
			emailAddressFilter.Category = CampaignContactFilterCategories.CommonTypes;

			var shouldAddSensitiveFilter = true;
			if (Campaign is HRGlbCompanyCampaign hrCampaign)
			{
				if (hrCampaign.IsUsingStaffDataSource)
				{
					shouldAddSensitiveFilter = Env.Security.StaffViewOtherStaffDetails.IsAllowed;
				}
				else if (hrCampaign.IsUsingJobApplicantDataSource)
				{
					shouldAddSensitiveFilter = Env.Security.HRJobApplicantView.IsAllowed;
				}
			}

			if (shouldAddSensitiveFilter)
			{
				var cityFilter = filters.AddTextFilter(HRFilterDescription.City, ViewCampaignContactSchema.VCC_City);
				cityFilter.MaxLength = ViewCampaignContactSchema.VCC_City.MaxLength;
				cityFilter.MultilingualDescription = ResString.GetMultilingualString("6b1ae402-4a41-4711-868d-aad7aaad2784", HRFilterDescription.City);
				cityFilter.Category = CampaignContactFilterCategories.CommonTypes;

				var stateFilter = filters.AddTextFilter(HRFilterDescription.State, ViewCampaignContactSchema.VCC_State);
				stateFilter.MaxLength = ViewCampaignContactSchema.VCC_State.MaxLength;
				stateFilter.MultilingualDescription = ResString.GetMultilingualString("6de702a5-4b30-4d89-867d-e917868a30da", HRFilterDescription.State);
				stateFilter.Category = CampaignContactFilterCategories.CommonTypes;
			}

			var notReceiveCampaignFilter = filters.AddGuidFilter(HRFilterDescription.HasNotReceivedCampaign, ModuleIDs.HRGlbCompanyCampaign, GlbCompanyCampaignItemSchema.G8_G0, Campaigns);
			notReceiveCampaignFilter.MultilingualDescription = ResString.GetMultilingualString("234fa291-2f7c-4fc3-aa04-b187981c8dd5", HRFilterDescription.HasNotReceivedCampaign);
			notReceiveCampaignFilter.Category = CampaignContactFilterCategories.CommonTypes;
			notReceiveCampaignFilter.SubGroup = new CampaignsSubGroup(isExcludingSentCampaign: true);

			var hasReceivedCampaignFilter = filters.AddGuidFilter(HRFilterDescription.HasReceivedCampaign, ModuleIDs.HRGlbCompanyCampaign, GlbCompanyCampaignItemSchema.G8_G0, Campaigns);
			hasReceivedCampaignFilter.MultilingualDescription = ResString.GetMultilingualString("a7d62bcb-6302-4dfb-936b-9fbee8a04f09", HRFilterDescription.HasReceivedCampaign);
			hasReceivedCampaignFilter.Category = CampaignContactFilterCategories.CommonTypes;
			hasReceivedCampaignFilter.SubGroup = new CampaignsSubGroup(isExcludingSentCampaign: false);
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var notReceiveCampaignsByDateFilter = filters.AddDateFilter(HRFilterDescription.HasNotReceivedCampaignsByDate, GetNotReceiveCampaignsByDate);
			notReceiveCampaignsByDateFilter.MultilingualDescription = ResString.GetMultilingualString("344d65da-fb4b-40b7-9ab7-3e682d403a03", HRFilterDescription.HasNotReceivedCampaignsByDate);
			notReceiveCampaignsByDateFilter.Category = CampaignContactFilterCategories.CommonTypes;

			var hasReceivedCampaignsByDateFilter = filters.AddDateFilter(HRFilterDescription.HasReceivedCampaignsByDate, GetHasReceivedCampaignsByDate);
			hasReceivedCampaignsByDateFilter.MultilingualDescription = ResString.GetMultilingualString("9e6d71e3-ecf3-4f31-986c-74afce707cf3", HRFilterDescription.HasReceivedCampaignsByDate);
			hasReceivedCampaignsByDateFilter.Category = CampaignContactFilterCategories.CommonTypes;
		}

		protected override ZQuery GetCampaignsByDate(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate, bool isExcludingSentCampaign)
		{
			var query = new ZDBOnlyQuery(typeof(CampaignContact));

			var campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID, isExcludingSentCampaign);
			AddDateRange(campaignItemSubQuery, comparisonOperator, JoinCondition.And, GlbCompanyCampaignItemSchema.G8_SystemCreateTimeUtc, fromDate.Date, toDate.Date);

			query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Subscribed Filter Overrides

		protected override void AddSubscribedContactsAndOrganisations(ZDBOnlyQuery viewQuery, bool subscribedStatus, bool notInFlag, ZQuery unsubscribeTypesQuery)
		{
			AddSubscribedContacts(viewQuery, subscribedStatus, notInFlag, unsubscribeTypesQuery);
		}

		#endregion

		#region Staff Filters

		void AddStaffFilters(ModuleFilterCollection filters)
		{
			var staffSubGroup = new GlbStaffSubGroup();
			var certificateSubGroup = new GlbStaffFilterProvider.CertificateSubGroup(staffSubGroup);
			var capabilitiesSubGroup = new GlbStaffFilterProvider.CapabilitySubGroup(staffSubGroup);
			var groupMembershipSubGroup = new GlbStaffFilterProvider.GroupMembershipSubGroup(staffSubGroup);
			var personSubGroup = new GlbPersonStaffSubGroup(staffSubGroup);

			#region Status and Flags

			var activeStatusFilter = filters.AddTextFilter(HRFilterDescription.StaffActiveStatus, GetActiveStatusQuery, CancelledStatusList);
			activeStatusFilter.Property = FormattableString.Invariant($"{CancelledStatusList[StatusActive].Code}");
			activeStatusFilter.Category = HRCampaignContactFilterCategories.Staff;
			activeStatusFilter.DefaultProperty = FormattableString.Invariant($"{CancelledStatusList[StatusActive].Code}");
			activeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|StatusAndFlags|StaffActiveStatus", HRFilterDescription.StaffActiveStatus);
			activeStatusFilter.SubGroup = staffSubGroup;

			var salesRepFilter = filters.AddTextFilter(HRFilterDescription.StaffSalesRep, StaffFilterProvider.GetSalesRepFilter, StaffFilterProvider.SalesRepStatusList);
			salesRepFilter.Category = HRCampaignContactFilterCategories.Staff;
			salesRepFilter.DefaultProperty = OrgConstants.FilterControl.SalesRepStatus.Code.AllStaff;
			salesRepFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|StaffSalesRep", HRFilterDescription.StaffSalesRep);
			salesRepFilter.SubGroup = staffSubGroup;

			#endregion

			#region Dates

			var birthdayFilter = filters.AddDateFilter(HRFilterDescription.StaffBirthDate, GlbStaffSchema.GS_Birthdate);
			birthdayFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|StaffBirthDate", HRFilterDescription.StaffBirthDate);
			birthdayFilter.Category = HRCampaignContactFilterCategories.Staff;
			birthdayFilter.SubGroup = staffSubGroup;

			var certificateIssueDateFilter = filters.AddDateFilter(HRFilterDescription.StaffCertificateIssueDate, GenRegCertAccredMaintListSchema.XZ_IssueDate);
			certificateIssueDateFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|StaffCertificateIssueDate", HRFilterDescription.StaffCertificateIssueDate);
			certificateIssueDateFilter.Category = HRCampaignContactFilterCategories.Staff;
			certificateIssueDateFilter.SubGroup = certificateSubGroup;

			var certificateExpiryDateFilter = filters.AddDateFilter(HRFilterDescription.StaffCertificateExpiryDate, GenRegCertAccredMaintListSchema.XZ_ExpiryOrDueDate);
			certificateExpiryDateFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|StaffCertificateExpiryDate", HRFilterDescription.StaffCertificateExpiryDate);
			certificateExpiryDateFilter.Category = HRCampaignContactFilterCategories.Staff;
			certificateExpiryDateFilter.SubGroup = certificateSubGroup;

			var employmentDateFilter = filters.AddDateFilter(HRFilterDescription.StaffEmploymentDate, GlbStaffSchema.GS_EmploymentDate);
			employmentDateFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|StaffEmployedDate", HRFilterDescription.StaffEmploymentDate);
			employmentDateFilter.Category = HRCampaignContactFilterCategories.Staff;
			employmentDateFilter.SubGroup = staffSubGroup;

			var departureDateFilter = filters.AddDateFilter(HRFilterDescription.StaffDepartureDate, GlbStaffSchema.GS_DepartureDate);
			departureDateFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|DepartureDate", HRFilterDescription.StaffDepartureDate);
			departureDateFilter.Category = HRCampaignContactFilterCategories.Staff;
			departureDateFilter.SubGroup = staffSubGroup;

			var reviewDateFilter = filters.AddDateFilter(HRFilterDescription.StaffNextReviewDate, GlbStaffSchema.GS_NextReviewDate);
			reviewDateFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|NextReviewDate", HRFilterDescription.StaffNextReviewDate);
			reviewDateFilter.Category = HRCampaignContactFilterCategories.Staff;
			reviewDateFilter.SubGroup = staffSubGroup;

			var notInTheWorkplaceFilter = filters.AddDateFilter(HRFilterDescription.StaffNotInTheWorkplace, StaffFilterProvider.GetHolidayQuery);
			notInTheWorkplaceFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|NotInTheWorkplace", HRFilterDescription.StaffNotInTheWorkplace);
			notInTheWorkplaceFilter.Category = HRCampaignContactFilterCategories.Staff;
			notInTheWorkplaceFilter.SubGroup = staffSubGroup;

			var birthMonthFilter = filters.AddTextFilter(HRFilterDescription.StaffBirthdayIn, StaffFilterProvider.GetBirthdayInMonthQuery, StaffFilterProvider.BirthdayMonthList);
			birthMonthFilter.Category = HRCampaignContactFilterCategories.Staff;
			birthMonthFilter.DefaultProperty = GlbStaffFilterProvider.AnyMonth.GetUnresolvedString();
			birthMonthFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|BirthdayIn", HRFilterDescription.StaffBirthdayIn);
			birthMonthFilter.ErrorOnCodeNotPresent = true;
			birthMonthFilter.SubGroup = staffSubGroup;

			var securityModifiedFilter = filters.AddDateFilter(HRFilterDescription.StaffSecurityModified, StaffFilterProvider.GetSecurityModifiedFilter);
			securityModifiedFilter.HideFutureDates = true;
			securityModifiedFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|SecurityModified", HRFilterDescription.StaffSecurityModified);
			securityModifiedFilter.Category = HRCampaignContactFilterCategories.Staff;
			securityModifiedFilter.SubGroup = staffSubGroup;

			#endregion

			#region Text Search

			var codeFilter = filters.AddTextFilter(HRFilterDescription.StaffCode, GlbStaffSchema.GS_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|Code", HRFilterDescription.StaffCode);
			codeFilter.Category = HRCampaignContactFilterCategories.Staff;
			codeFilter.SubGroup = staffSubGroup;

			var codeorFullNameFilter = filters.AddTextFilterForMultipleColumns(HRFilterDescription.StaffCodeOrFullName, GlbStaffSchema.GS_Code, GlbStaffSchema.GS_FullName);
			codeorFullNameFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|StaffCodeOrFullName", HRFilterDescription.StaffCodeOrFullName);
			codeorFullNameFilter.MaxLength = GlbStaffSchema.GS_Code.MaxLength >= GlbStaffSchema.GS_FullName.MaxLength ? GlbStaffSchema.GS_Code.MaxLength : GlbStaffSchema.GS_FullName.MaxLength;
			codeorFullNameFilter.Category = HRCampaignContactFilterCategories.Staff;
			codeorFullNameFilter.SubGroup = staffSubGroup;

			var driverBranchFilter = filters.AddTextFilter(StaffDriverCollection.DriverBranch, GetDriverBranchQuery, () => new CodeDescriptionPairList(StaffFilterProvider.BranchesList));
			driverBranchFilter.Property = Env.CurrentBranch.Code;
			driverBranchFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|DriverBranch", StaffDriverCollection.DriverBranch);
			driverBranchFilter.Category = HRCampaignContactFilterCategories.Staff;

			var genderFilter = filters.AddTextFilter(HRFilterDescription.StaffGender, GlbStaffSchema.GS_Gender, () => new CodeDescriptionPairList(OLookUpEditType.Gender));
			genderFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|Gender", HRFilterDescription.StaffGender);
			genderFilter.Category = HRCampaignContactFilterCategories.Staff;
			genderFilter.SubGroup = staffSubGroup;

			var languageFilter = filters.AddTextFilter(HRFilterDescription.StaffLanguage, GlbStaffSchema.GS_WorkingLanguage, () => new CodeDescriptionPairList(OLookUpEditType.Language));
			languageFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|Language", HRFilterDescription.StaffLanguage);
			languageFilter.Category = HRCampaignContactFilterCategories.Staff;
			languageFilter.SubGroup = staffSubGroup;

			var loginNameFilter = filters.AddTextFilter(HRFilterDescription.StaffLoginName, GlbStaffSchema.GS_LoginName);
			loginNameFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|LoginName", HRFilterDescription.StaffLoginName);
			loginNameFilter.Category = HRCampaignContactFilterCategories.Staff;
			loginNameFilter.SubGroup = staffSubGroup;

			#endregion

			#region Other

			var capabilityFilter = filters.AddGuidFilter(HRFilterDescription.StaffCapability, ModuleIDs.GlbCapability, GlbCapabilitySchema.PK, StaffFilterProvider.Capabilities);
			capabilityFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|Capability", HRFilterDescription.StaffCapability);
			capabilityFilter.Category = HRCampaignContactFilterCategories.Staff;
			capabilityFilter.SubGroup = capabilitiesSubGroup;

			var employmentBasisFilter = filters.AddTextFilter(HRFilterDescription.StaffEmploymentBasis, GlbStaffSchema.GS_EmploymentBasis, SystemDataRegistry.Instance.StaffEmploymentTypes.Value);
			employmentBasisFilter.Category = HRCampaignContactFilterCategories.Staff;
			employmentBasisFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|EmploymentBasis", HRFilterDescription.StaffEmploymentBasis);
			employmentBasisFilter.SubGroup = staffSubGroup;

			var groupMembershipFilter = filters.AddGuidFilter(HRFilterDescription.StaffGroupMembership, ModuleIDs.GlbGroup, GlbGroupSchema.PK, StaffFilterProvider.Groups);
			groupMembershipFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|GroupMembership", HRFilterDescription.StaffGroupMembership);
			groupMembershipFilter.Category = HRCampaignContactFilterCategories.Staff;
			groupMembershipFilter.SubGroup = groupMembershipSubGroup;

			var homeBranchFilter = filters.AddGuidFilter(HRFilterDescription.StaffHomeBranch, ModuleIDs.GlbBranch, GlbStaffSchema.GS_GB_HomeBranch, StaffFilterProvider.Branches);
			homeBranchFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|HomeBranch", HRFilterDescription.StaffHomeBranch);
			homeBranchFilter.Category = HRCampaignContactFilterCategories.Staff;
			homeBranchFilter.SubGroup = staffSubGroup;

			var homeDepartmentFilter = filters.AddGuidFilter(HRFilterDescription.StaffHomeDepartment, ModuleIDs.GlbDepartment, GlbStaffSchema.GS_GE_HomeDepartment, StaffFilterProvider.Departments);
			homeDepartmentFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|HomeDepartment", HRFilterDescription.StaffHomeDepartment);
			homeDepartmentFilter.Category = HRCampaignContactFilterCategories.Staff;
			homeDepartmentFilter.SubGroup = staffSubGroup;

			var nationalityFilter = filters.AddNkFilter(HRFilterDescription.StaffNationality, GlbStaffSchema.GS_RN_NKNationalityCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			nationalityFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|Nationality", HRFilterDescription.StaffNationality);
			nationalityFilter.Category = HRCampaignContactFilterCategories.Staff;
			nationalityFilter.SubGroup = staffSubGroup;

			var securityModuleFilter = new StaffSecurityModuleFilter(HRFilterDescription.StaffSecurityRights,
				StaffFilterProvider.Branches, StaffFilterProvider.Departments)
			{
				MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|Staff|SecurityRights", HRFilterDescription.StaffSecurityRights),
				Category = HRCampaignContactFilterCategories.Staff,
				SubGroup = staffSubGroup
			};
			filters.AddCustomFilter(securityModuleFilter);

			var staffRelatedAccreditationAttemptsFilter = new PersonAccreditationAttemptsFilter(HRFilterDescription.AccreditationAttemptsStaff, GlbPersonSchema.PK, GlbAccreditationAttemptSchema.HAA_PER, new GlbAccreditationAttemptCollection(Factory), typeof(GlbPerson));
			staffRelatedAccreditationAttemptsFilter.MultilingualDescription = ResString.GetMultilingualString("f757bce0-9c8b-4665-9b8a-60949ad54cc0", HRFilterDescription.AccreditationAttemptsStaff);
			staffRelatedAccreditationAttemptsFilter.SubGroup = personSubGroup;
			staffRelatedAccreditationAttemptsFilter.Category = HRCampaignContactFilterCategories.Staff;
			filters.AddFilter(staffRelatedAccreditationAttemptsFilter);

			#endregion

		}

		#region Driver Branch Query

		ZQuery GetDriverBranchQuery(ZString branchCode)
		{
			var driverBranchQuery = StaffFilterProvider.GetDataBranchFilter(branchCode);
			if (!driverBranchQuery.IsNoResultQuery)
			{
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.PK);
				staffSubQuery.AddToFilter(driverBranchQuery);
				query.AddSubQuery(ViewCampaignContactSchema.PK, staffSubQuery, JoinCondition.And);
				return query;
			}

			return driverBranchQuery;
		}

		#endregion

		#region Active Status Query

		ZQuery GetActiveStatusQuery(ZString status)
		{
			var query = new ZQuery();

			status = status.Trim();
			if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(GlbStaffSchema.GS_IsActive, false);
				query.IgnoreActiveFilter = true;
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(GlbStaffSchema.GS_IsActive, true);
				query.IgnoreActiveFilter = false;
			}
			else if (StatusAll.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(GlbStaffSchema.GS_IsActive, true);
				query.AddToFilter(JoinCondition.Or, GlbStaffSchema.GS_IsActive, false);
				query.IgnoreActiveFilter = true;
			}

			return query;
		}

		#endregion

		class GlbStaffSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, GlbStaffSchema.Constants.Prefix);
				var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.PK);
				staffSubQuery.AddToFilter(filter);
				query.AddSubQuery(ViewCampaignContactSchema.PK, staffSubQuery, JoinCondition.And);
				return query;
			}
		}

		public class GlbPersonStaffSubGroup : ModuleFilterSubGroup
		{
			public GlbPersonStaffSubGroup()
			{ }

			public GlbPersonStaffSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbStaff));

				var subQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(GlbStaffSchema.GS_PER, subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Job Applicant Filters

		void AddJobApplicantFilters(ModuleFilterCollection filters)
		{
			#region Status and Flags

			var applicantSubGroup = new HRJobApplicantSubGroup();
			var certificateSubGroup = new HRJobApplicantFilterProvider.CertificateSubGroup(applicantSubGroup);
			var personSubGroup = new GlbPersonApplicantSubGroup(applicantSubGroup);
			var applicationSubGroup = new HRJobApplicantFilterProvider.ApplicationSubGroup(applicantSubGroup);
			var jobCampaignSubGroup = new HRJobApplicantFilterProvider.JobCampaignSubGroup(applicationSubGroup);
			var jobRoleSubGroup = new HRJobApplicantFilterProvider.JobRoleSubGroup(jobCampaignSubGroup);

			var learningCentreUserFilter = filters.AddFlagsFilter(HRFilterDescription.ApplicantType,
				new string[] { Res.GetString("ab13150f-0a78-4993-9124-cf3895fe69e0", "Learning Center User") },
				new GetFlagsQuery[] { JobApplicantFilterProvider.IncludeLearningCentreUserQuery });
			learningCentreUserFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantType", HRFilterDescription.ApplicantType);
			learningCentreUserFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			learningCentreUserFilter.SubGroup = applicantSubGroup;

			var availabilityFilter = filters.AddTextFilter(HRFilterDescription.ApplicantAvailability, HRJobApplicantSchema.HA_Availability, JobApplicantFilterProvider.Availabilities);
			availabilityFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantAvailability", HRFilterDescription.ApplicantAvailability);
			availabilityFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			availabilityFilter.SubGroup = applicantSubGroup;

			var hasAppliedFilter = filters.AddFlagsFilter(HRFilterDescription.ApplicantHasApplied, new string[] { Res.GetString("a7c54279-039f-4c16-b5d8-ec46cf4cc17c", "Has Applied") }, new GetFlagsQuery[] { JobApplicantFilterProvider.GetHasAppliedFlagQuery });
			hasAppliedFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantHasApplied", HRFilterDescription.ApplicantHasApplied);
			hasAppliedFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			hasAppliedFilter.SubGroup = applicantSubGroup;

			var registrationMethodFilter = filters.AddFlagsFilter(HRFilterDescription.ApplicantRegistrationMethod,
				new string[]
				{
					Res.GetString("cc216e07-cc24-421a-aad0-a6a3b93c5b97", "Registered via Web"),
					Res.GetString("7c4340c1-497c-4fe6-8df0-06f4125c9f40", "Registered manually")
				}, new GetFlagsQuery[] { JobApplicantFilterProvider.GetRegistrationMethodDelegate(true), JobApplicantFilterProvider.GetRegistrationMethodDelegate(false) },
				JoinCondition.Or);
			registrationMethodFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantRegistrationMethod", HRFilterDescription.ApplicantRegistrationMethod);
			registrationMethodFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			registrationMethodFilter.SubGroup = applicantSubGroup;

			var workPermitStatusFilter = filters.AddTextFilter(HRFilterDescription.ApplicantWorkPermitStatus, HRJobApplicantSchema.HA_WorkPermitStatus, JobApplicantFilterProvider.WorkPermitStatuses);
			workPermitStatusFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantWorkPermitStatus", HRFilterDescription.ApplicantWorkPermitStatus);
			workPermitStatusFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			workPermitStatusFilter.SubGroup = applicantSubGroup;

			#endregion

			#region Dates

			var submissionTimeFilter = filters.AddDateFilter(HRFilterDescription.ApplicantSubmissionTime, JobApplicantFilterProvider.SubmissionTimeFilterQuery, true, false);
			submissionTimeFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantSubmissionTime", HRFilterDescription.ApplicantSubmissionTime);
			submissionTimeFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			submissionTimeFilter.SubGroup = applicantSubGroup;

			var certificateIssueDateFilter = filters.AddDateFilter(HRFilterDescription.ApplicantCertificateIssueDate, GenRegCertAccredMaintListSchema.XZ_IssueDate);
			certificateIssueDateFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantCertificateIssueDate", HRFilterDescription.ApplicantCertificateIssueDate);
			certificateIssueDateFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			certificateIssueDateFilter.SubGroup = certificateSubGroup;

			var certificateExpiryDateFilter = filters.AddDateFilter(HRFilterDescription.ApplicantCertificateExpiryDate, GenRegCertAccredMaintListSchema.XZ_ExpiryOrDueDate);
			certificateExpiryDateFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantCertificateExpiryDate", HRFilterDescription.ApplicantCertificateExpiryDate);
			certificateExpiryDateFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			certificateExpiryDateFilter.SubGroup = certificateSubGroup;

			#endregion

			#region Text Search

			var applicationStatusFilter = filters.AddTextFilter(HRFilterDescription.ApplicantApplicationStatus, HRJobApplicationSchema.HP_CurrentStatus, JobApplicantFilterProvider.ApplicationStatuses);
			applicationStatusFilter.MaxLength = HRJobApplicationSchema.HP_CurrentStatus.MaxLength;
			applicationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantApplicationStatus", HRFilterDescription.ApplicantApplicationStatus);
			applicationStatusFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			applicationStatusFilter.SubGroup = applicationSubGroup;

			var certificateTypeFilter = filters.AddTextFilter(HRFilterDescription.ApplicantCertificateType, GenRegCertAccredMaintListSchema.XZ_Type, JobApplicantFilterProvider.Certificates);
			certificateTypeFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantCertificateType", HRFilterDescription.ApplicantCertificateType); // Filter name
			certificateTypeFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			certificateTypeFilter.SubGroup = certificateSubGroup;

			var certificateRefNumberFilter = filters.AddTextFilter(HRFilterDescription.ApplicantCertificateNumber, GenRegCertAccredMaintListSchema.XZ_RefNumber);
			certificateRefNumberFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantCertificateNumber", HRFilterDescription.ApplicantCertificateNumber); // Filter name
			certificateRefNumberFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			certificateRefNumberFilter.SubGroup = certificateSubGroup;

			#endregion

			#region Other

			var jobRoleFilter = filters.AddGuidFilter(HRFilterDescription.ApplicantJobRole, ModuleIDs.HRJobRole, HRJobRoleSchema.PK, JobApplicantFilterProvider.JobRoles);
			jobRoleFilter.IsPublishedOnWeb = false;
			jobRoleFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantJobRole", HRFilterDescription.ApplicantJobRole);
			jobRoleFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			jobRoleFilter.SubGroup = jobRoleSubGroup;

			var campaignDetailsFilter = filters.AddGuidFilter(HRFilterDescription.ApplicantJobOpenings, ModuleIDs.HRJobOpenings, HRRecruitmentJobCampaignSchema.PK, JobApplicantFilterProvider.Campaigns);
			campaignDetailsFilter.IsPublishedOnWeb = false;
			campaignDetailsFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantJobOpenings", HRFilterDescription.ApplicantJobOpenings);
			campaignDetailsFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			campaignDetailsFilter.SubGroup = jobCampaignSubGroup;

			var countryFilter = filters.AddTextFilter(HRFilterDescription.ApplicantCountry, GetCountryQuery, JobApplicantFilterProvider.Countries);
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("HRGlbCompanyCampaignContactFilterStrip|JobApplicant|ApplicantCountry", "Country/Region");
			countryFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			countryFilter.SubGroup = applicantSubGroup;

			var applicantRelatedAccreditationAttemptsFilter = new PersonAccreditationAttemptsFilter(HRFilterDescription.AccreditationAttemptsApplicant, GlbPersonSchema.PK, GlbAccreditationAttemptSchema.HAA_PER, new GlbAccreditationAttemptCollection(Factory), typeof(GlbPerson));
			applicantRelatedAccreditationAttemptsFilter.MultilingualDescription = ResString.GetMultilingualString("10d3c7c9-bfa6-46cc-a785-b9fa7d609c25", HRFilterDescription.AccreditationAttemptsApplicant);
			applicantRelatedAccreditationAttemptsFilter.SubGroup = personSubGroup;
			applicantRelatedAccreditationAttemptsFilter.Category = HRCampaignContactFilterCategories.JobApplicant;
			filters.AddFilter(applicantRelatedAccreditationAttemptsFilter);

			#endregion
		}

		ZQuery GetCountryQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(HRJobApplicant));
			var personQuery = new ZDBOnlySubQuery(typeof(GlbPerson), HRJobApplicantSchema.HA_PER);
			personQuery.AddToFilter(GlbPersonSchema.PER_RN_NKCountry, comparisonOperator, value);

			query.AddSubQuery(personQuery, JoinCondition.And);
			return query;
		}

		class HRJobApplicantSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, HRJobApplicantSchema.Constants.Prefix);
				var applicantSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicantSchema.PK);
				applicantSubQuery.AddToFilter(filter);
				query.AddSubQuery(ViewCampaignContactSchema.PK, applicantSubQuery, JoinCondition.And);
				return query;
			}
		}

		public class GlbPersonApplicantSubGroup : ModuleFilterSubGroup
		{
			public GlbPersonApplicantSubGroup()
			{ }

			public GlbPersonApplicantSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(HRJobApplicant));

				var subQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(HRJobApplicantSchema.HA_PER, subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Table Decider

		void GetDataFromTableDeciderFilters(ModuleFilterCollection filters)
		{
			if (Campaign != null)
			{
				var tableDeciderFilter = filters.AddTextFilter("TableDecider", GetDataFromTableDeciderQuery);
				tableDeciderFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}
		}

		ZQuery GetDataFromTableDeciderQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CampaignContact));
			var hrCampaign = (HRGlbCompanyCampaign)Campaign;

			if (Campaign != null && hrCampaign.IsUsingStaffDataSource)
			{
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, GlbStaffSchema.Constants.Prefix);
			}
			if (Campaign != null && hrCampaign.IsUsingJobApplicantDataSource)
			{
				query.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.VCC_TableCode, HRJobApplicantSchema.Constants.Prefix);
			}
			return query;
		}

		#endregion

		#region Filter Providers

		GlbStaffFilterProvider StaffFilterProvider
		{
			get { return staffFilterProvider ?? (staffFilterProvider = new GlbStaffFilterProvider()); }
		}
		GlbStaffFilterProvider staffFilterProvider;

		HRJobApplicantFilterProvider JobApplicantFilterProvider
		{
			get { return jobApplicantFilterProvider ?? (jobApplicantFilterProvider = new HRJobApplicantFilterProvider()); }
		}
		HRJobApplicantFilterProvider jobApplicantFilterProvider;

		#endregion

		#endregion

		#region FilterCategory

		public static class HRCampaignContactFilterCategories
		{
			public static FilterCategory Staff
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Staff", HRFilterDescription.Staff)); }
			}

			public static FilterCategory JobApplicant
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.JobApplicant", HRFilterDescription.JobApplicant)); }
			}
		}

		#endregion

		#region Filter Descriptions

		public static class HRFilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string JobApplicant = "Job Applicant";
			public const string Staff = "Staff";
			public const string City = "City";
			public const string State = "State";
			public const string ContactName = "Contact Name";
			public const string EmailAddress = "Email Address";
			public const string HasReceivedCampaign = "Has Received Campaign";
			public const string HasNotReceivedCampaign = "Has Not Received Campaign";
			public const string HasReceivedCampaignsByDate = "Has Received Campaigns By Date";
			public const string HasNotReceivedCampaignsByDate = "Has Not Received Campaigns By Date";
			public const string StaffActiveStatus = "Active Status";
			public const string StaffSalesRep = "Sales Rep";
			public const string StaffBirthDate = "Birth Date";
			public const string StaffBirthdayIn = "Birthday In";
			public const string StaffCertificateIssueDate = "Staff Certificate Issue Date";
			public const string StaffCertificateExpiryDate = "Staff Certificate Expiry Date";
			public const string StaffDepartureDate = "Departure Date";
			public const string StaffEmploymentDate = "Employment Date";
			public const string StaffNextReviewDate = "Next Review Date";
			public const string StaffNotInTheWorkplace = "Not In The Workplace";
			public const string StaffSecurityModified = "Security Modified";
			public const string StaffCode = "Code";
			public const string StaffCodeOrFullName = "Code or Full Name";
			public const string StaffGender = "Gender";
			public const string StaffLanguage = "Language";
			public const string StaffLoginName = "Login Name";
			public const string StaffCapability = "Capability";
			public const string StaffEmploymentBasis = "Employment Basis";
			public const string StaffGroupMembership = "Group Membership";
			public const string StaffHomeBranch = "Home Branch";
			public const string StaffHomeDepartment = "Home Department";
			public const string StaffNationality = "Nationality";
			public const string StaffSecurityRights = "Security Rights";
			public const string StaffIsNotSystemAccount = "Staff Is Not System Account";
			public const string ApplicantSubmissionTime = "Submission Time";
			public const string ApplicantType = "Applicant Type";
			public const string ApplicantRegistrationMethod = "Registration Method";
			public const string ApplicantHasApplied = "Has Applied";
			public const string ApplicantJobOpenings = "Job Openings";
			public const string ApplicantJobRole = "Job Role";
			public const string ApplicantJobSkill = "Job Skill";
			public const string ApplicantCountry = "Country";
			public const string ApplicantCertificateExpiryDate = "Applicant Certificate Expiry Date";
			public const string ApplicantCertificateIssueDate = "Applicant Certificate Issue Date";
			public const string ApplicantCertificateNumber = "Certificate Number";
			public const string ApplicantCertificateType = "Certificate Type";
			public const string ApplicantAvailability = "Availability";
			public const string ApplicantWorkPermitStatus = "Work Permit Status";
			public const string ApplicantApplicationStatus = "Application Status";
			public const string AccreditationAttemptsStaff = "Accreditation Attempts (by Staff)";
			public const string AccreditationAttemptsApplicant = "Accreditation Attempts (by Applicant)";

			#endregion
		}

		internal static string InternalSubscriptionFilterCode => SubscriptionFilterCode;

		#endregion
	}
}
