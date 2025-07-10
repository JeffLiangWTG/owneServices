using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessTaskIterationLink : IBusiness
	{
		ZGuid PK { get; }
		ZGuid P9I_P9_ContainmentBarrierTask { get; set; }
		ZGuid P9I_P9_IterationTask { get; set; }
		ZGuid P9I_FH_IterationWorkflow { get; set; }
		ZString P9I_LinkType { get; set; }
		ZByte P9I_Sequence { get; set; }
		ZString P9I_IterationReason { get; set; }

		ZString P9I_Outcome { get; set; }
		ZString P9I_GS_NKResourceUnderReview { get; set; }
		ZDateTime P9I_SystemCreateTimeUtc { get; set; }
		ZString P9I_SystemCreateUser { get; set; }
		ZDateTime P9I_SystemLastEditTimeUtc { get; set; }
		ZString P9I_SystemLastEditUser { get; set; }

		IProcessTask ContainmentBarrierTask { get; }

		IProcessTaskIterationLinkPivotCollection TaskPivots { get; }
	}
}
