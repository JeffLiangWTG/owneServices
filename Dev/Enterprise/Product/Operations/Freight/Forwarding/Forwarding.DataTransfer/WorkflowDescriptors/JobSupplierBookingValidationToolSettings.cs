using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer;

sealed class JobSupplierBookingValidationToolSettings : ValidationToolSettings
{
	public JobSupplierBookingValidationToolSettings(JobSupplierBookingWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.SBK;
}
