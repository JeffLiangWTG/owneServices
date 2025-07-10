using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer;

public class ContainerLoadListLineValidationToolSettings : ValidationToolSettings
{
	public ContainerLoadListLineValidationToolSettings(ContainerLoadListLineWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.CLI;
}
