using Enterprise.Customs.Business.MessagingProcess;

namespace Enterprise.Customs.GUI.MessagingProcess
{
	public interface ISupportSendDialog
	{
		IDialog GetSendDialog(ActionResult result);
		string SendDialogCancelledMessage { get; }
	}
}
