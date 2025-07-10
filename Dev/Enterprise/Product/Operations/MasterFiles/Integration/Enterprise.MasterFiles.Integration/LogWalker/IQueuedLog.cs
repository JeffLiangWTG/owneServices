using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IQueuedLog : IWorkflowTriggerSource
	{
		BusinessObjectFactory Factory { get; }
		ZGuid PK { get; }
		ZGuid SJ_ParentID { get; }
		ZGuid SJ_ALogReference { get; }
		ZGuid SJ_TargetID { get; }
		ZString SJ_GB_NKBranch { get; }
		ZString SJ_GE_NKDepartment { get; }
		ZString SJ_GS_NKUser { get; }
		ZString SJ_Reference { get; }
		ZString SJ_SE_NKEvent { get; }
		ZDateTime SJ_EventTime { get; }
		ZDateTime SJ_EventTimeUtc { get; }
		ZString SJ_ParentTableCode { get; }
		ZBool SJ_IsEstimate { get; }
		bool IsRetry { get; }
		ZBool SJ_IsDelayFired { get; }
		ZDateTime SJ_PostedTimeUtc { get; }

		ZByte SJ_RetryCount { get; set; }
		ZString SJ_Status { get; set; }

		IEnumerable<IStmChangeLog> ChangeLogs { get; }
	}
}
