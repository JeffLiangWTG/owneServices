using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer;

sealed class JobSupplierBookingLineValidationToolSettings : ValidationToolSettings
{
	public JobSupplierBookingLineValidationToolSettings(JobSupplierBookingLineWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
	{
	}

	public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.SBL;
}
