using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer;

public class CargoLoadPlanLineValidationToolSettings : ValidationToolSettings
{
	public CargoLoadPlanLineValidationToolSettings(CargoLoadPlanLineWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.CPL;
}
