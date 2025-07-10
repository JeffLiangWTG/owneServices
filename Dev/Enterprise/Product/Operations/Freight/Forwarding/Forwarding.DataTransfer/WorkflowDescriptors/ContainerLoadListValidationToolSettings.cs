using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer;

public class ContainerLoadListValidationToolSettings : ValidationToolSettings
{
	public ContainerLoadListValidationToolSettings(ContainerLoadListWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.CLH;
}
