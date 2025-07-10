using CargoWise.Types;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessEstimateLog
	{
		ZString P9E_GS_NKUser { get; set; }
		ZBool P9E_HasWorkStarted { get; set; }
		ZDateTimeOffset P9E_LogDateTime { get; set; }
		ZInt P9E_NewHighEstimateMinutes { get; set; }
		ZInt P9E_NewLowEstimateMinutes { get; set; }
		ZGuid P9E_ParentId { get; set; }
		ZString P9E_ParentTableCode { get; set; }
		ZInt P9E_PreviousHighEstimateMinutes { get; set; }
		ZInt P9E_PreviousLowEstimateMinutes { get; set; }
		ZBool P9E_WasWorkPreviouslyStarted { get; set; }
	}
}
