using Enterprise.Customs.Business.MessagingProcess;

namespace Enterprise.Customs.GUI.MessagingProcess
{
	public interface ISupportPreviewDialog
	{
		IDialog GetPreviewDialog(ActionResult result);
		string PreviewDialogCancelledMessage { get; }
	}
}
