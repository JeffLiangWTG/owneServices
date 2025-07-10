using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public partial class ProcessTask : ITriggerUserContextConditions
	{
		ZString ITriggerUserContextConditions.TriggerContextCode
		{
			get => P9_TriggerContext;
			set => P9_TriggerContext = value;
		}

		ZString ITriggerUserContextConditions.TriggerStaffCode
		{
			get => P9_GS_NKAssignedStaffMember;
			set => P9_GS_NKAssignedStaffMember = value;
		}

		ZGuid ITriggerUserContextConditions.TriggerBranch
		{
			get => P9_GB_TriggerBranch;
			set => P9_GB_TriggerBranch = value;
		}

		ZGuid ITriggerUserContextConditions.TriggerCompany
		{
			get => P9_GC;
			set => P9_GC = value;
		}

		ZGuid ITriggerUserContextConditions.TriggerDepartment
		{
			get => P9_GE_TriggerDepartment;
			set => P9_GE_TriggerDepartment = value;
		}
	}
}
