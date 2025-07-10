using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffFilterProvider : FilterStripBusinessObject
	{
		#region Filters

		public ZQuery GetSalesRepFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == OrgConstants.FilterControl.SalesRepStatus.Code.SalesRep)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsSalesRep, ZBool.True);
			}
			else if (value == OrgConstants.FilterControl.SalesRepStatus.Code.NonSalesRep)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsSalesRep, ZBool.False);
			}

			return query;
		}

		public ZQuery GetDriverFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == OrgConstants.FilterControl.DriverStatus.Code.Driver)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsDriver, ZBool.True);
			}
			else if (value == OrgConstants.FilterControl.DriverStatus.Code.NonDriver)
			{
				query.AddToFilter(GlbStaffSchema.GS_IsDriver, ZBool.False);
			}

			return query;
		}

		public ZQuery GetDataBranchFilter(ZString branchCode)
		{
			switch (branchCode)
			{
				case AllStaff:
					return new ZDBOnlyQuery(typeof(GlbStaff));
				case AllDrivers:
					return StaffDriverCollection.GetDriversQuery(Factory);
				default:
					return StaffDriverCollection.GetDriversQuery(Factory, branchCode);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Drop List Code.")]
		const string AllDrivers = "All Drivers";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Drop List Code.")]
		const string AllStaff = "All Staff";

		public ZQuery GetBirthdayInMonthQuery(ZString birthdayMonth)
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));
			query.DefaultJoinCondition = JoinCondition.And;
			var birthday = BirthdayMonthList.GetDescriptionFromCode(birthdayMonth);

			if (!birthdayMonth.IsEmpty && !birthdayMonth.Equals(BirthdayMonthList[0].Code) && birthday != null)
			{
				query = new ZDBOnlyQuery(typeof(GlbStaff));
				ZString filterString = "MONTH(" + GlbStaffSchema.GS_Birthdate.Name + ") = " + birthday;
				query.AddFilterAndZSQLParameterCollection(filterString, new ZSqlParameterCollection());
			}
			return query;
		}

		public ZQuery GetHolidayQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var staffQuery = new ZDBOnlyQuery(typeof(GlbStaff));
			var subQuery = new ZDBOnlySubQuery(typeof(GlbStaffHoliday), GlbStaffHolidaySchema.GA_GS);

			var overlapFilter = new ZQuery();

			if (!toDate.IsEmpty)
			{
				overlapFilter.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThan, toDate.Date.AddDays(1));
			}

			if (!fromDate.IsEmpty)
			{
				overlapFilter.AddToFilter(JoinCondition.And, GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, fromDate.Date);
			}

			subQuery.AddToFilter(overlapFilter, JoinCondition.Or);
			staffQuery.AddSubQuery(subQuery, JoinCondition.And);

			return staffQuery;
		}

		public ZQuery GetSecurityModifiedFilter(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));

			var stmAlogSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			stmAlogSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.SecurityModified.Code);

			var securityModifiedDateFilter = new ZQuery();

			AddDateRange(securityModifiedDateFilter, comparisonOperator, JoinCondition.And, StmALogSchema.SL_EventTime, fromDate.Date, toDate.Date);

			stmAlogSubQuery.AddToFilter(securityModifiedDateFilter);
			query.AddSubQuery(stmAlogSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Module Filter Sub Groups

		#region GlbPerson

		public class GlbPersonSubGroup : ModuleFilterSubGroup
		{
			public GlbPersonSubGroup()
			{ }

			public GlbPersonSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbStaff));

				var subQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK, GlbStaffSchema.GS_PER);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Certificates

		public class CertificateSubGroup : ModuleFilterSubGroup
		{
			public CertificateSubGroup()
			{ }

			public CertificateSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbStaff));

				var subQuery = new ZDBOnlySubQuery(typeof(GenRegCertAccredMaintList), GenRegCertAccredMaintListSchema.XZ_ParentID);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Capabilities

		public class CapabilitySubGroup : ModuleFilterSubGroup
		{
			public CapabilitySubGroup()
			{ }

			public CapabilitySubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbStaff));

				var subQuery = new ZDBOnlySubQuery(typeof(GlbCapability), GlbCapabilitySchema.PK);
				subQuery.AddToFilter(filter);

				var linkQuery = new ZDBOnlySubQuery(typeof(GlbResourceCapabilityPivot), GlbResourceCapabilityPivotSchema.G5_GS_Resource);
				linkQuery.AddSubQuery(GlbResourceCapabilityPivotSchema.G5_G4_Capability, subQuery, JoinCondition.And);

				query.AddSubQuery(linkQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Group Membership
		public class GroupMembershipSubGroup : ModuleFilterSubGroup
		{
			public GroupMembershipSubGroup()
			{ }

			public GroupMembershipSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbStaff));
				var groupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
				groupSubQuery.AddToFilter(filter);

				var groupLinkQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
				groupLinkQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, groupSubQuery, JoinCondition.And);

				query.AddSubQuery(groupLinkQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#endregion

		#region Lists

		#region Sales Rep List

		public CodeDescriptionPairList SalesRepStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				list.AddPair(OrgConstants.FilterControl.SalesRepStatus.Code.AllStaff, Res.GetString("MasterFiles|GlbStaffFilter|SalesRep|AllStaff", "All Staff"));
				list.AddPair(OrgConstants.FilterControl.SalesRepStatus.Code.SalesRep, Res.GetString("MasterFiles|GlbStaffFilter|SalesRep|SalesRep", "Sales Reps Only"));
				list.AddPair(OrgConstants.FilterControl.SalesRepStatus.Code.NonSalesRep, Res.GetString("MasterFiles|GlbStaffFilter|SalesRep|NonSalesRep", "Non-Sales Reps Only"));

				return list;
			}
		}

		#endregion

		#region Branches List

		public CodeDescriptionPairList BranchesList
		{
			get
			{
				return Factory.GetCachedValue("DriverBranchFilterBusinessObject.BranchesList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(AllDrivers, Res.GetString("MasterFiles|DriverBranch|DatabaseReader|Branches|AllDrivers", "Drivers in any Branch"));
					result.AddPair(AllStaff, Res.GetString("MasterFiles|DriverBranch|DatabaseReader|Branches|AllStaff", "Staff in any Branch (Including non-Drivers)"));

					var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
					var companyBranches = Factory.Load<GlbBranch>(branchQuery);
					Array.ForEach(companyBranches, b => result.AddPair(b.GB_Code, b.GB_BranchName));

					return result;
				});
			}
		}

		#endregion

		#region Birthday Month List

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "List used for SQL string")]
		public CodeDescriptionPairList BirthdayMonthList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(AnyMonth, ZString.Empty);

				foreach (CodeDescriptionPair monthPair in new CodeDescriptionPairList(OLookUpEditType.Months))
				{
					list.AddPair(monthPair.MultilingualDescription, list.Count.ToString(CultureInfo.InvariantCulture));
				}

				return list;
			}
		}

		#endregion

		public static MultilingualString AnyMonth
		{
			get { return ResString.GetMultilingualString("e68fd646-246b-4eeb-b30a-1cd8fa3bb1e9", "Any Month"); }
		}

		#endregion

		#region Lookups

		#region Branches

		public GlbBranchCollection Branches
		{
			get { return branches ?? (branches = new GlbBranchCollection(Factory)); }
		}

		GlbBranchCollection branches;

		#endregion

		#region Capabilities

		public GlbCapabilityCollection Capabilities
		{
			get { return capabilities ?? (capabilities = new GlbCapabilityCollection(Factory)); }
		}

		GlbCapabilityCollection capabilities;

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}

				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

		#endregion

		#region Groups

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

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}
