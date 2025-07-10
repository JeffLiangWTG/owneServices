using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccComplianceSequenceFilterBusinessObject : FilterStripBusinessObject
	{
		public AccComplianceSequenceFilterBusinessObject()
		{ }

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedItemFilters(filters);
			AddCompanyHiddenFilter(filters);
			AddDateFilters(filters);
			AddAuditInformationFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Sub type", AccComplianceSequenceSchema.XD_SequenceClass, AccComplianceSequenceLookups.SequenceClassListBaseOnCurrentCompanyCountryCode()).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccComplianceSequenceFilter|SubType", "Sub type");
			filters.AddTextFilter("Code", AccComplianceSequenceSchema.XD_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccComplianceSequenceFilter|Code", "Code");
			filters.AddTextFilter("Description", AccComplianceSequenceSchema.XD_Description).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccComplianceSequenceFilter|Description", "Description");
			filters.AddTextFilter("Allocation Level", AccComplianceSequenceSchema.XD_AllocationLevel, new CodeDescriptionPairList(OLookUpEditType.ComplianceBookAllocationLevel)).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccComplianceSequenceFilter|AllocationLevel", "Allocation Level");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccComplianceSequenceSchema.XD_GB_BranchOwner, Branches);
			branchFilter.Category = FilterCategories.Other;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|Branch", "Branch");

			var departmentFilter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccComplianceSequenceSchema.XD_GE_Department, Departments);
			departmentFilter.Category = FilterCategories.Other;
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccBankAccountFilter|Department", "Department");
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			ModuleDateFilter validFromFilter = filters.AddDateFilter("Valid From", AccComplianceSequenceSchema.XD_StartDate);
			validFromFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccComplianceSequenceFilter|ValidDate", "Valid From");

			ModuleDateFilter expireAtFilter = filters.AddDateFilter("Expiry Date", AccComplianceSequenceSchema.XD_ExpiryDate);
			expireAtFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccComplianceSequenceFilter|ExpiryDate", "Expiry Date");
		}

		#endregion

		#region Hidden

		void AddCompanyHiddenFilter(ModuleFilterCollection filters)
		{
			ModuleGuidFilter hiddenGuidFilter = filters.AddGuidFilter("Company", ModuleIDs.GlbCompany, AccComplianceSequenceSchema.XD_GC_Company, Companies);
			hiddenGuidFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			hiddenGuidFilter.Property = Env.CurrentCompany.PK;
			hiddenGuidFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|AccComplianceSequenceFilter|Company", "Company");
		}

		#endregion

		#region AuditInformation

		void AddAuditInformationFilters(ModuleFilterCollection filters)
		{
			var lockByFilter = filters.AddGuidFilter("Lock By", ModuleIDs.GlbStaff, GetLockQuery, StaffList);
			lockByFilter.Category = FilterCategories.AuditInformation;
			lockByFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccComplianceSequenceFilter|LockBy", "Lock By");
		}

		internal GenAddOnColumnQueryHelper GenAddOnColumnQueryHelper
		{
			get { return helper ?? (helper = new GenAddOnColumnQueryHelper(typeof(AccComplianceSequence))); }
		}
		GenAddOnColumnQueryHelper helper;

		ZQuery GetLockQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceSequence));

			var innerSubQuery = GenAddOnColumnQueryHelper.GetContainsValueForAnyQuery(false, AccComplianceSequence.Schema.XD_LockBy);

			innerSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, value.ToString());

			query.AddSubQuery(innerSubQuery, JoinCondition.And);

			return query;
		}
		#endregion

		#endregion

		#region Lookups

		public GlbCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					fCompanies = new GlbCompanyCollection(Factory);
				}
				return fCompanies;
			}
		}

		GlbCompanyCollection fCompanies;

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

		protected GlbStaffCollection StaffList
		{
			get
			{
				if (fStaffList == null)
				{
					fStaffList = new GlbStaffCollection(Factory);
				}

				return fStaffList;
			}
		}

		GlbStaffCollection fStaffList;

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
	}
}
