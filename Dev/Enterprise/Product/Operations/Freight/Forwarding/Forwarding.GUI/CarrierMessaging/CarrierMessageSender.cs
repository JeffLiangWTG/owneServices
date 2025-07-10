using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class CarrierMessageSender : ICarrierMessageSender
	{
		public void Send(string caption, IWorkflowProvider workflowProvider, ICarrierMessagingValidation validation)
		{
			if (workflowProvider != null && validation != null)
			{
				using (var progressForm = new ManualDataExportProgressForm())
				{
					var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(caption, workflowProvider, validation);
					behaviour.Apply(progressForm);

					ZFormModaliser.ShowDialogWithoutDispose(progressForm);
				}
			}
		}
	}
}
