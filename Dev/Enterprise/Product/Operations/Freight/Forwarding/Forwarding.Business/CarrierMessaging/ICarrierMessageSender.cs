using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface ICarrierMessageSender
	{
		void Send(string caption, IWorkflowProvider workflowProvider, ICarrierMessagingValidation validation);
	}
}
