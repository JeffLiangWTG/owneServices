using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer;

public class ContainerLoadPlanValidationToolSettings : ValidationToolSettings
{
	public ContainerLoadPlanValidationToolSettings(ContainerLoadPlanWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.CLP;
}
