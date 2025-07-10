using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer;

public class JobConsolValidationToolSettings : ValidationToolSettings
{
	public JobConsolValidationToolSettings(JobConsolWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.CON;
}
