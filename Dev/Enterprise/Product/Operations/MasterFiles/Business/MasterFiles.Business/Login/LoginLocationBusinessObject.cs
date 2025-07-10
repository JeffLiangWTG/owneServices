using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class LoginLocationBusinessObject : AutoLoginLocationBusinessObject
	{
		public LoginLocationBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{ }

		#region Company

		[List("Companies")]
		[ResourceStringData("6970f1d1-55b5-4672-bd5c-54b313866de7", Caption = "Company")]
		public override ZString CompanyCode
		{
			get => base.CompanyCode;
			set
			{
				if (base.CompanyCode != value)
				{
					base.CompanyCode = value;
					fBranches = null;
					fDepartments = null;
					BranchCode = ZString.Empty;
				}
			}
		}

		public GlbCompany Company
		{
			get
			{
				if (fCompany == null || fCompany.GC_Code != CompanyCode)
				{
					fCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, CompanyCode));
				}
				return fCompany;
			}
		}
		GlbCompany fCompany;

		public GlbCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					fCompanies = new GlbCompanyCollection(Factory, new ZQuery(GlbCompanySchema.GC_IsActive, true));

					if (ShouldApplyBranchSecurityFilter)
					{
						fCompanies.Factory.AddFetchHint(GlbBranchSchema.Instance, new ZQuery(GlbBranchSchema.GB_GC, fCompanies.Select(c => c.PK)));

						fCompanies.AdditionalFilter = new ZQuery(GlbCompanySchema.PK, fCompanies.Where(company => company.Branches.Any(branch => branch.GB_IsActive && IsAllowed(branch))).Select(company => company.PK).ToArray());
					}

					fCompanies.ApplySort(GlbCompany.Schema.GC_Name, ListSortDirection.Ascending);
				}

				return fCompanies;
			}
		}
		GlbCompanyCollection fCompanies;

		#endregion

		#region Branch

		[List("Branches")]
		[ResourceStringData("8c3466b9-fb98-41ef-a5f3-1a182f05bfd7", Caption = "Branch")]
		public override ZString BranchCode
		{
			get => base.BranchCode;
			set
			{
				if (base.BranchCode != value)
				{
					fDepartments = null;
					base.BranchCode = value;
				}
			}
		}

		public GlbBranchDependentCollection Branches
		{
			get
			{
				var currentlyShouldApplyBranchSecurityFilter = ShouldApplyBranchSecurityFilter;
				if (fShouldApplyBranchSecurityFilter != currentlyShouldApplyBranchSecurityFilter)
				{
					fBranches = null;
					fShouldApplyBranchSecurityFilter = currentlyShouldApplyBranchSecurityFilter;
				}

				if (fBranches == null)
				{
					fBranches = new GlbBranchDependentCollection(Company, Factory);
					fBranches.ApplySort(GlbBranch.Schema.GB_BranchName, ListSortDirection.Ascending);
					var filter = new ZQuery(GlbBranchSchema.GB_IsActive, true);

					if (currentlyShouldApplyBranchSecurityFilter)
					{
						filter.AddToFilter(new ZQuery(GlbBranchSchema.PK, fBranches.Where(IsAllowed).Select(branch => branch.PK).ToArray()));
					}

					fBranches.AdditionalFilter = filter;
				}
				return fBranches;
			}
		}
		GlbBranchDependentCollection fBranches;
		bool fShouldApplyBranchSecurityFilter;

		public GlbBranch Branch
		{
			get
			{
				if (fBranch == null || fBranch.GB_Code != BranchCode)
				{
					fBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, BranchCode));
				}
				return fBranch;
			}
		}
		GlbBranch fBranch;

		protected virtual bool ShouldApplyBranchSecurityFilter => LoginUser != null && !LoginUser.GS_IsController;

		bool IsAllowed(GlbBranch branch)
		{
			var staffAllowed = IsAllowedStaff(branch);

			if (staffAllowed == SecurityState.Granted)
			{
				return true;
			}

			if (staffAllowed == SecurityState.Denied)
			{
				return false;
			}

			var branchAllowed = IsAllowedGroup(branch);
			return (branchAllowed == SecurityState.Granted || branchAllowed == SecurityState.Implicit);
		}

		SecurityState IsAllowedStaff(GlbBranch branch)
		{
			var result = GetSecurityState(StaffLoginSecurity, LoginUser.PK, ZGuid.Empty, branch.PK); // first check on the branch - *, branch

			if (result == SecurityState.Implicit)
			{
				result = GetSecurityState(StaffLoginSecurity, LoginUser.PK, branch.GB_GC, ZGuid.Empty); // if not then check on the company - company, *
			}

			if (result == SecurityState.Implicit)
			{
				result = GetSecurityState(StaffLoginSecurity, LoginUser.PK, ZGuid.Empty, ZGuid.Empty); // otherwise check on global level - *, *
			}

			return result;
		}

		List<GlbGroup> Groups
		{
			get
			{
				if (groups == null)
				{
					groups = LoginUser.ActiveGroups.Cast<GlbGroup>().ToList();
				}
				return groups;
			}
		}
		List<GlbGroup> groups;

		SecurityState IsAllowedGroup(GlbBranch branch)
		{
			var result = SecurityState.Implicit;

			var count = Groups.Count;
			for (var i = 0; i < count; ++i)
			{
				var group = Groups[i];
				var resultGroup = IsAllowedGroup(group, branch);

				if (resultGroup != SecurityState.Denied)
				{
					if (i != 0)
					{
						//killer heuristic - this group is more likely to grant permissions in the future, so move it to the front
						Groups.RemoveAt(i);
						Groups.Insert(0, group);
					}
					return resultGroup;
				}

				result = resultGroup;
			}

			return result;
		}

		SecurityState IsAllowedGroup(GlbGroup group, GlbBranch branch)
		{
			var result = GetSecurityState(GroupLoginSecurity, group.PK, ZGuid.Empty, branch.PK); // first check on the branch - *, branch

			if (result == SecurityState.Implicit)
			{
				result = GetSecurityState(GroupLoginSecurity, group.PK, branch.GB_GC, ZGuid.Empty); // if not then check on the company - company, *
			}

			if (result == SecurityState.Implicit)
			{
				result = GetSecurityState(GroupLoginSecurity, group.PK, ZGuid.Empty, ZGuid.Empty); // otherwise check on global level - *, *
			}

			return result;
		}

		SecurityState GetSecurityState(Dictionary<(ZGuid, ZGuid, ZGuid), bool> securities, ZGuid identifier, ZGuid companyPk, ZGuid branchPk)
		{
			if (securities.TryGetValue((identifier, companyPk, branchPk), out bool granted))
			{
				return granted ? SecurityState.Granted : SecurityState.Denied;
			}

			return SecurityState.Implicit;
		}

		#endregion

		#region Department

		[List("Departments")]
		[ResourceStringData("aba84faf-3b88-4d95-937a-9aca351326b1", Caption = "Department")]
		public override ZString DepartmentCode
		{
			get => base.DepartmentCode;
			set
			{
				if (base.DepartmentCode != value)
				{
					base.DepartmentCode = value;
				}
			}
		}

		public virtual GlbDepartmentCollection Departments
		{
			get
			{
				var currentlyShouldApplyDepartmentSecurityFilter = ShouldApplyDepartmentSecurityFilter;
				if (fShouldApplyDepartmentSecurityFilter != currentlyShouldApplyDepartmentSecurityFilter)
				{
					fDepartments = null;
					fShouldApplyDepartmentSecurityFilter = currentlyShouldApplyDepartmentSecurityFilter;
				}

				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory, new ZQuery(GlbDepartmentSchema.GE_IsActive, true));
					fDepartments.ApplySort(GlbDepartment.Schema.GE_Desc, ListSortDirection.Ascending);

					if (currentlyShouldApplyDepartmentSecurityFilter)
					{
						fDepartments.AdditionalFilter = new ZQuery(GlbDepartmentSchema.PK, fDepartments.Where(IsAllowed).Select(department => department.PK).ToArray());
					}
				}

				return fDepartments;
			}
		}
		GlbDepartmentCollection fDepartments;

		public GlbDepartment Department
		{
			get
			{
				if (fDepartment == null || fDepartment.GE_Code != DepartmentCode)
				{
					fDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, DepartmentCode));
				}
				return fDepartment;
			}
		}
		GlbDepartment fDepartment;

		protected virtual bool ShouldApplyDepartmentSecurityFilter => LoginUser != null && !LoginUser.GS_IsController;
		bool fShouldApplyDepartmentSecurityFilter;

		bool IsAllowed(GlbDepartment department)
		{
			var result = IsAllowed(StaffLoginSecurityPlusDepartments, LoginUser.PK, department);

			//Check groups separately since one group might deny but another might allow.
			//Special case: ALL starts out implicit. Old security models which assumed the implicit in ALL didn't matter will be broken if it suddenly allows people to log into anything, so ignore Implicit from ALL.
			if (result == SecurityState.Implicit)
			{
				var count = Groups.Count;
				for (var i = 0; i < count; ++i)
				{
					var group = Groups[i];
					var newResult = IsAllowed(GroupLoginSecurityPlusDepartments, group.PK, department);
					if (group.GG_Code != "ALL" || newResult != SecurityState.Implicit)
					{
						result = newResult;
						if (result != SecurityState.Denied)
						{
							if (i != 0)
							{
								//killer heuristic - this group is more likely to grant permissions in the future, so move it to the front
								Groups.RemoveAt(i);
								Groups.Insert(0, group);
							}
							break;
						}
					}
				}
			}

			return result != SecurityState.Denied;
		}

		SecurityState IsAllowed(Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool> securities, ZGuid identifier, GlbDepartment department)
		{
			var result = SecurityState.Implicit;

			if (Branch != null)
			{
				result = GetSecurityState(securities, identifier, ZGuid.Empty, Branch.PK, department.PK);

				if (result == SecurityState.Implicit && Branch.Company != null)
				{
					result = GetSecurityState(securities, identifier, Branch.Company.PK, ZGuid.Empty, department.PK);
				}
			}

			if (result == SecurityState.Implicit && Company != null)
			{
				result = GetSecurityState(securities, identifier, Company.PK, ZGuid.Empty, department.PK);
			}

			if (result == SecurityState.Implicit && Branch != null)
			{
				result = GetSecurityState(securities, identifier, ZGuid.Empty, Branch.PK, ZGuid.Empty);

				if (result == SecurityState.Implicit && Branch.Company != null)
				{
					result = GetSecurityState(securities, identifier, Branch.Company.PK, ZGuid.Empty, ZGuid.Empty);
				}
			}

			if (result == SecurityState.Implicit && Company != null)
			{
				result = GetSecurityState(securities, identifier, Company.PK, ZGuid.Empty, ZGuid.Empty);
			}

			if (result == SecurityState.Implicit)
			{
				result = GetSecurityState(securities, identifier, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			}

			return result;
		}

		SecurityState GetSecurityState(Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool> securities, ZGuid identifier, ZGuid companyPk, ZGuid branchPk, ZGuid departmentPk)
		{
			if (securities.TryGetValue((identifier, companyPk, branchPk, departmentPk), out bool granted))
			{
				return granted ? SecurityState.Granted : SecurityState.Denied;
			}

			return SecurityState.Implicit;
		}

		#endregion

		protected virtual GlbStaff LoginUser
		{
			get
			{
#if DEBUG
				return CurrentUserForTesting ?? GlbStaff.CurrentUser;
#else
				return GlbStaff.CurrentUser;
#endif
			}
		}

#if DEBUG
		public GlbStaff CurrentUserForTesting { get; set; }

		internal void ClearSecurityCache()
		{
			groups = null;
			staffLoginSecurity = null;
			groupLoginSecurity = null;
			staffLoginSecurityPlusDepartments = null;
			groupLoginSecurityPlusDepartments = null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Security right code")]
#endif

		void InitializeStaffSecurityLookups()
		{
			staffLoginSecurity = new Dictionary<(ZGuid, ZGuid, ZGuid), bool>();
			staffLoginSecurityPlusDepartments = new Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool>();

			var query = new ZQuery(GlbSecuritySchema.GU_SecurityRight, "Login");
			query.AddToFilter(GlbSecuritySchema.GU_GS, LoginUser.PK);
			var securities = Factory.Load<GlbSecurity>(query);

			AddInitializeSecurity(securities, staffLoginSecurity, staffLoginSecurityPlusDepartments);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Security right code")]
		void InitializeGroupSecurityLookups()
		{
			groupLoginSecurity = new Dictionary<(ZGuid, ZGuid, ZGuid), bool>();
			groupLoginSecurityPlusDepartments = new Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool>();

			var query = new ZQuery(GlbSecuritySchema.GU_SecurityRight, "Login");
			query.AddToFilter(GlbSecuritySchema.GU_GG, Groups.Select(group => group.PK).ToArray());
			query.OrderBy = GlbSecuritySchema.GU_SecurityItemIsAllowed.Name + " DESC";
			var securities = Factory.Load<GlbSecurity>(query);

			AddInitializeSecurity(securities, groupLoginSecurity, groupLoginSecurityPlusDepartments, true);
		}

		void AddInitializeSecurity(GlbSecurity[] securities, Dictionary<(ZGuid, ZGuid, ZGuid), bool> loginSecurity, Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool> loginSecurityPlusDepartments, bool isGroupSecurity = false)
		{
			foreach (var security in securities)
			{
				var identifier = isGroupSecurity ? security.GU_GG : security.GU_GS;
				if (security.GU_GE.IsEmpty || security.GU_SecurityItemIsAllowed)
				{
					var key = (identifier, security.GU_GC, security.GU_GB);
					if (!(loginSecurity.ContainsKey(key) && loginSecurity[key])) //update the value only when existed value is false
					{
						loginSecurity[key] = security.GU_SecurityItemIsAllowed;
					}
				}
				loginSecurityPlusDepartments.Add((identifier, security.GU_GC, security.GU_GB, security.GU_GE), security.GU_SecurityItemIsAllowed);
			}
		}

		Dictionary<(ZGuid, ZGuid, ZGuid), bool> staffLoginSecurity;
		Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool> staffLoginSecurityPlusDepartments;
		Dictionary<(ZGuid, ZGuid, ZGuid), bool> groupLoginSecurity;
		Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool> groupLoginSecurityPlusDepartments;

		Dictionary<(ZGuid, ZGuid, ZGuid), bool> StaffLoginSecurity
		{
			get
			{
				if (staffLoginSecurity == null)
				{
					InitializeStaffSecurityLookups();
				}
				return staffLoginSecurity;
			}
		}

		Dictionary<(ZGuid, ZGuid, ZGuid), bool> GroupLoginSecurity
		{
			get
			{
				if (groupLoginSecurity == null)
				{
					InitializeGroupSecurityLookups();
				}
				return groupLoginSecurity;
			}
		}

		Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool> StaffLoginSecurityPlusDepartments
		{
			get
			{
				if (staffLoginSecurityPlusDepartments == null)
				{
					InitializeStaffSecurityLookups();
				}
				return staffLoginSecurityPlusDepartments;
			}
		}

		Dictionary<(ZGuid, ZGuid, ZGuid, ZGuid), bool> GroupLoginSecurityPlusDepartments
		{
			get
			{
				if (groupLoginSecurityPlusDepartments == null)
				{
					InitializeGroupSecurityLookups();
				}
				return groupLoginSecurityPlusDepartments;
			}
		}
	}
}
