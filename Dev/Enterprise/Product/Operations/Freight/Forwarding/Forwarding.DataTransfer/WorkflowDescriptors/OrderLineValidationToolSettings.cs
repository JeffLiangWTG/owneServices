using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class OrderLineValidationToolSettings	: ValidationToolSettings
	{
		public OrderLineValidationToolSettings(OrderLineWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
		{
		}

		public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.ORL;
	}
}
