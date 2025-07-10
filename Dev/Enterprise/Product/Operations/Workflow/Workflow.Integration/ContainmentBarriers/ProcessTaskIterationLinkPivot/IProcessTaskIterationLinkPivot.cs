using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessTaskIterationLinkPivot : IBusiness
	{
		ZGuid P9P_P9_Task { get; set; }
		ZGuid P9P_P9I_Iteration { get; set; }
		ZGuid P9P_ParentId { get; set; }
		ZString P9P_ParentTableCode { get; set; }

		ZDateTime P9P_SystemCreateTimeUtc { get; set; }
		ZString P9P_SystemCreateUser { get; set; }
		ZDateTime P9P_SystemLastEditTimeUtc { get; set; }
		ZString P9P_SystemLastEditUser { get; set; }

		IProcessTaskIterationLink Iteration { get; }
	}
}
