using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public interface IAllocationLevelStrategy
	{
		bool IsBranchApplicable { get; }
		bool IsDepartmentApplicable { get; }
		void CreateMatchFilter(AccComplianceSequence parent, ZQuery query);
		bool AllowOverlappingBooks { get; }
	}

	internal static class AllocationLevelStrategyFactory
	{
		public static IAllocationLevelStrategy Create(string allocationLevel, ZGuid branchPK = default, ZGuid departmentPK = default)
		{
			IAllocationLevelStrategy strategy = null;

			switch (allocationLevel)
			{
				case ComplianceBookAllocationLevel.Company:
					strategy = new CompanyAllocationLevelStrategy();
					break;
				case ComplianceBookAllocationLevel.Branch:
					strategy = new BranchAllocationLevelStrategy(branchPK);
					break;
				case ComplianceBookAllocationLevel.BranchDepartment:
					strategy = new BranchAndDepartmentLevelStrategy(branchPK, departmentPK);
					break;
				case ComplianceBookAllocationLevel.Counter:
					strategy = new CounterAllocationLevelStrategy();
					break;
				default:
					strategy = new DefaultAllocationLevelStrategy();
					break;
			}

			return strategy;
		}
	}

	internal class CounterAllocationLevelStrategy : IAllocationLevelStrategy
	{
		public bool IsDepartmentApplicable => false;

		public bool IsBranchApplicable => false;

		public bool AllowOverlappingBooks => true;

		public void CreateMatchFilter(AccComplianceSequence parent, ZQuery query)
		{
			query.AddToFilter(AccComplianceSequenceSchema.XD_AllocationLevel, parent?.XD_AllocationLevel ?? ComplianceBookAllocationLevel.Counter);

			var dbQuery = query as ZDBOnlyQuery;
			if (dbQuery != null)
			{
				var helper = new GenAddOnColumnQueryHelper(typeof(AccComplianceSequence));
				var innerSubQuery = helper.GetContainsValueForAnyQuery(false, AccComplianceSequence.Schema.XD_LockBy);
				innerSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, GlbStaff.CurrentUser.PK.ToString());
				dbQuery.AddSubQuery(innerSubQuery, JoinCondition.And);
			}
		}
	}

	internal class CompanyAllocationLevelStrategy : IAllocationLevelStrategy
	{
		public bool IsDepartmentApplicable => false;

		public bool IsBranchApplicable => false;

		public bool AllowOverlappingBooks => false;

		public void CreateMatchFilter(AccComplianceSequence parent, ZQuery query)
		{
			query.AddToFilter(AccComplianceSequenceSchema.XD_AllocationLevel, parent?.XD_AllocationLevel ?? ComplianceBookAllocationLevel.Company);
		}
	}

	internal class BranchAllocationLevelStrategy : IAllocationLevelStrategy
	{
		internal BranchAllocationLevelStrategy(ZGuid branchPK)
		{
			BranchPK = branchPK;
		}

		readonly ZGuid BranchPK;

		public bool IsDepartmentApplicable => false;

		public bool IsBranchApplicable => true;

		public bool AllowOverlappingBooks => false;

		public void CreateMatchFilter(AccComplianceSequence parent, ZQuery query)
		{
			query.AddToFilter(AccComplianceSequenceSchema.XD_AllocationLevel, parent?.XD_AllocationLevel ?? ComplianceBookAllocationLevel.Branch);
			query.AddToFilter(AccComplianceSequenceSchema.XD_GB_BranchOwner, parent?.XD_GB_BranchOwner ?? (!BranchPK.IsDefault ? BranchPK : GlbBranch.CurrentBranch.PK));
		}
	}

	internal class BranchAndDepartmentLevelStrategy : IAllocationLevelStrategy
	{
		internal BranchAndDepartmentLevelStrategy(ZGuid branchPK, ZGuid departmentPK)
		{
			BranchPK = branchPK;
			DepartmentPK = departmentPK;
		}

		readonly ZGuid BranchPK;

		readonly ZGuid DepartmentPK;

		public bool IsDepartmentApplicable => true;

		public bool IsBranchApplicable => true;

		public bool AllowOverlappingBooks => false;

		public void CreateMatchFilter(AccComplianceSequence parent, ZQuery query)
		{
			query.AddToFilter(AccComplianceSequenceSchema.XD_AllocationLevel, parent?.XD_AllocationLevel ?? ComplianceBookAllocationLevel.BranchDepartment);
			query.AddToFilter(AccComplianceSequenceSchema.XD_GB_BranchOwner, parent?.XD_GB_BranchOwner ?? (!BranchPK.IsDefault ? BranchPK : GlbBranch.CurrentBranch.PK));
			query.AddToFilter(AccComplianceSequenceSchema.XD_GE_Department, parent?.XD_GE_Department ?? (!DepartmentPK.IsDefault ? DepartmentPK : GlbDepartment.CurrentDepartment.PK));
		}
	}

	internal class DefaultAllocationLevelStrategy : IAllocationLevelStrategy
	{
		public bool IsDepartmentApplicable => false;

		public bool IsBranchApplicable => false;

		public bool AllowOverlappingBooks => true;

		public void CreateMatchFilter(AccComplianceSequence parent, ZQuery query)
		{
		}
	}
}
