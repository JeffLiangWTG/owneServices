using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class OrderValidationToolSettings : ValidationToolSettings
	{
		public OrderValidationToolSettings(OrderWorkflowDescriptor workflowDescriptor) : base(workflowDescriptor)
		{
		}

		public override string RequestTypeJobType => ExternalRequestTypeJobTypes.Codes.ORD;
	}
}
