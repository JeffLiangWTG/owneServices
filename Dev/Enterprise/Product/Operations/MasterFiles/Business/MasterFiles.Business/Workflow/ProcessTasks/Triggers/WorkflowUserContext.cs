using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowUserContext
	{
		public static WorkflowUserContext NullContext => new WorkflowUserContext(null, ZString.Empty, ZString.Empty, ZString.Empty);

		public static WorkflowUserContext Create(IWorkflowTriggerSource log)
		{
			if (log is IQueuedLog queuedLog)
			{
				return new WorkflowUserContext(queuedLog.Factory, new WorkflowTriggerEventData(queuedLog));
			}
			else if (log is StmALog stmALog)
			{
				return new WorkflowUserContext(stmALog.Factory, new WorkflowTriggerEventData(stmALog));
			}
			return null;
		}

		public WorkflowUserContext(BusinessObjectFactory factory, GlbStaff staff, GlbBranch branch, GlbDepartment department)
			: this(factory, staff.GS_Code, branch.GB_Code, department.GE_Code)
		{
			this.staff = staff;
			this.branch = branch;
			this.department = department;
		}

		WorkflowUserContext(BusinessObjectFactory factory, WorkflowTriggerEventData wteData)
		{
			Factory = factory;
			StaffCode = wteData.ContextStaffCode;
			BranchCode = wteData.ContextBranchCode;
			DepartmentCode = wteData.ContextDepartmentCode;
		}

		WorkflowUserContext(BusinessObjectFactory factory, string staffCode, string branchCode, string departmentCode)
		{
			Factory = factory;
			StaffCode = staffCode;
			BranchCode = branchCode;
			DepartmentCode = departmentCode;
		}

		public GlbStaff Staff
		{
			get
			{
				if (staff == null && !StaffCode.IsEmpty)
				{
					staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, StaffCode);
				}
				return staff;
			}
		}
		GlbStaff staff;

		public GlbBranch Branch
		{
			get
			{
				if (branch == null && !BranchCode.IsEmpty)
				{
					branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, BranchCode);
				}
				return branch;
			}
		}
		GlbBranch branch;

		public GlbDepartment Department
		{
			get
			{
				if (department == null && !DepartmentCode.IsEmpty)
				{
					department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, DepartmentCode);
				}
				return department;
			}
		}
		GlbDepartment department;

		public ZString StaffCode { get; }
		public ZString BranchCode { get; }
		public ZString DepartmentCode { get; }

		readonly BusinessObjectFactory Factory;

		public bool IsNull()
		{
			return StaffCode.IsEmpty
				|| BranchCode.IsEmpty
				|| DepartmentCode.IsEmpty;
		}
	}
}
